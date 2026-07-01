using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Level2.PlateLeveler.DataCommunication;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.Server {
    public class PLCCommunication : IDisposable {
        private readonly InitData _Initialization;
        private readonly TelegramList _TelegramList;
        private readonly TelegramData _TelEvent;
        private readonly TelegramValueData _LifeInt;
        private readonly S7Communication _S7, _Events;

        private bool bS7Started, bEventStarted;
        private readonly Timer _Timer, _TimerLife;

        [Obsolete]
        public PLCCommunication(InitData init, TelegramList telegrams) {
            this._Initialization = init;
            this._TelegramList = telegrams;
            this._S7 = new S7Communication();
            this._Events = new S7Communication();

            this._Timer = new Timer(this._Initialization.Interval.S7);
            this._Timer.Elapsed += this._Timer_Elapsed;

            this._TimerLife = new Timer(init.Interval.Life);
            this._TimerLife.Elapsed += this._TimerLife_Elapsed;

            //bPos = positionList.Select(p => p.Occupied).ToArray();
            //_occupiedList = positionList.Select(p => p.Occupied).ToList();
            //_DBList = positionList.Select(c => c.BlockNo).ToList();
            //_positionList = positionList;
        }

        public bool Connected => this._S7.Connected;

        private void DisconnectSocket() => this._S7?.DisconnectSocket();

        private short GetInt16FromByte(List<byte> ReceivedData, int offset) {
            var tmpBuff = new byte[2];

            tmpBuff[0] = ReceivedData[offset];
            tmpBuff[1] = ReceivedData[offset + 1];

            if (BitConverter.IsLittleEndian) {
                Array.Reverse(tmpBuff);
            }

            return BitConverter.ToInt16(tmpBuff, 0);
        }

        public void StartTCPChannel() {
            while (this._S7.Connected) {
                var ReceivedData = new List<byte>();

                try {
                    // Receive Data - Wait for Data
                    this.ReceiveDataFromTCPStream(ref ReceivedData);
                    if (ReceivedData.Count == 0) {
                        // TCP disconnected
                        this.DisconnectSocket();
                        break;
                    } else {
                        short TelegramLen = 0;
                        short TelegramCnt = 0;
                        short TelegramType = 0;

                        if (ReceivedData.Count >= 0) {
                            var tmpReceivedData = new List<byte> {
                                0,
                                150,
                                0,
                                1,
                                0,
                                32,
                                0,
                                0,
                                0,
                                0,
                                0,
                                0,
                                0,
                                0,
                                0,
                                0,
                                0,
                                0,
                                0,
                                0,
                                0,
                                0,
                                65,
                                66,
                                67,
                                68,
                                0,
                                1,
                                0,
                                0,
                                0,
                                0
                            };

                            for (var a = 0; a <= 117; a++) {
                                tmpReceivedData.Add(0);
                            }

                            //string number = "ABCD";
                            //byte[] bytes = Encoding.ASCII.GetBytes(number);
                            //if (BitConverter.IsLittleEndian)
                            //   Array.Reverse(bytes);

                            ReceivedData = tmpReceivedData;

                            Console.WriteLine("TCP buffer data received.... Length <" + (ReceivedData.Count - 1).ToString() + ">");

                            var ReceivedbyteData = new byte[ReceivedData.Count];
                            for (var i = 0; i <= ReceivedData.Count - 1; i++) {
                                ReceivedbyteData[i] = ReceivedData[i];
                            }

                            TelegramLen = this.GetInt16FromByte(ReceivedData, 0);
                            TelegramCnt = this.GetInt16FromByte(ReceivedData, 2);
                            TelegramType = this.GetInt16FromByte(ReceivedData, 4);

                            if (TelegramType != 0) {
                                this.ListenerCom.HandleReceivedData(ReceivedbyteData, TelegramLen, TelegramCnt, TelegramType);
                            }
                        } else {
                            Console.WriteLine("TCP buffer error.... Length <" + (ReceivedData.Count - 1).ToString() + ">");
                        }
                    }
                } catch {
                    // TCP Socket error
                    // DisconnectSocket();
                    Console.WriteLine("TCP buffer error.... Length <" + (ReceivedData.Count - 1).ToString() + ">");
                    break;
                }
            }
        }

        private void ReceiveDataFromTCPStream(ref List<byte> ReceivedData) {
            int BytesRead;
            var btarry = new byte[this._S7.ClientTCP.ReceiveBufferSize];
            try {
                if (this._S7.ClientTCP != null) {
                    if (this._S7.ComStream.CanRead) {
                        BytesRead = this._S7.ComStream.Read(btarry, 0, btarry.Length);
                        for (var n = 0; n < BytesRead; n++) {
                            ReceivedData.Add(btarry[n]);
                        }
                    }
                }
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }

        [Obsolete]
        private void StartComWithPLC() {
            try {
                //_TelEvent = _TelegramList.FirstOrDefault(t => t.Name == TelegramDef.L1_L2_Events.ToString());
                //_valEvents = _TelEvent.TelegramValues.FirstOrDefault(t => t.Name == EventDef.Events.ToString());
                //_LifeInt = _TelEvent.TelegramValues.FirstOrDefault(t => t.Name == "LifeInt");
                var s7 = this._Initialization.Communications.FirstOrDefault(s => s.Name == CommunicationDef.L1_L2);

                Console.WriteLine("Trying to start communication with S7 over socket. Address <" + s7.IPAddresse + ">, Port <" + s7.Port + ">");

                this.bS7Started = this._S7.ConnectToS7(s7.IPAddresse, s7.Port);
                //s7 = _Initialization.S7.FirstOrDefault(s => s.Name == CommunicationDef.Events_L2);
                this.bEventStarted = false;// _Events.ConnectToS7(s7.IPAddresse, s7.Port, s7.RackNo, s7.SlotNo, s7.IsNewS7Type, s7.IsSocketCom == 1);
                                           //if (bEventStarted)
                                           //   _TimerLife.Start();
                                           //btCoilList = new List<byte[]>();

                //foreach (int db in _DBList)
                //{
                //   int size = _Events.GetDBLengthSize(db);
                //   btCoilList.Add(new byte[size]);
                //}

                //_Events.ReadMultiDBValues(ref btCoilList, _DBList, 10, 16, 0);

                //bCoilIDSkid = new bool[3];

                if (this.bS7Started) {
                    this._Timer.Start();
                    var msg = "S7 Communication started!";
                    Console.WriteLine(msg);

                    this._Timer.Interval = this._Initialization.Interval.S7;

                    this.StartTCPChannel();

                    Logging.SendMessage(msg, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                } else {
                    this._Timer.Interval = 5000;
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        [Obsolete]
        public void Start() {

            try {
                if (!this.Connected) {
                    this.StartComWithPLC();

                    this._Timer.Start();
                } else {
                    var msg = "S7 Communication failed!";
                    Console.WriteLine(msg);
                    Logging.SendMessage(msg, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void _TimerLife_Elapsed(object sender, ElapsedEventArgs e) {
            try {
                var btList = this._Events.GetSingleByteArrayFromDB(this._TelEvent.TelegramID, this._LifeInt.BytePosition, this._LifeInt.Length);
                var bLife = (bool)Functions.ConvertByteArrayToObjectWithType([.. btList], TypeCode.Boolean, false);
                if (bLife) {
                    var btArr = new byte[] { 0, 0 };
                    var errCode = string.Empty;
                    bLife = this._Events.SetSingleDBValue(btArr, this._LifeInt.BytePosition, this._LifeInt.Length, this._TelEvent.TelegramID, out errCode);
                    if (bLife) {
                        throw new Exception(errCode);
                    }
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        public void StartS7Timer() => this._Timer.Start();

        public void StopS7Timer() => this._Timer.Stop();

        [Obsolete]
        private void _Timer_Elapsed(object sender, ElapsedEventArgs e) {
            //string coilID = string.Empty;
            //bool bOccupied = false;
            try {
                this._Timer.Stop();

                if (!this.Connected) {
                    this.StartComWithPLC();
                    this._Timer.Start();
                } else {
                    this.Listener.L1_L2_Tracking();
                    this.Listener.L1_L2_Watchdog();
                    this.Listener.L1_L2_RPDI();

                    //_listener.GetCyclicData();

                    this._Timer.Start();

                    //return;
                    //List<byte> buffer = _S7.GetCompleteByteArrayFromDB(_TelEvent.TelegramID);

                    //List<bool> bList = GetBoolArrayFromEvent(_valEvents, buffer);

                    //_Events.ReadMultiDBValues(ref btCoilList, _DBList, 10, 16, 0);
                    //List<string> strList = new List<string>();
                    //btCoilList.ForEach(delegate (byte[] item)
                    //{
                    //   coilID = Functions.GetObjectFromByteArray<string>(item.ToList().GetRange(0, 16).ToArray(), TypeCode.String, false);
                    //   strList.Add(coilID.Trim());
                    //});
                    //bool bSendPos = false;
                    //_occupiedList = bList.GetRange(8, 7);
                    //if (bPos == null)
                    //   bPos = _positionList.Select(p => p.Occupied).ToArray();

                    //for (int n = 0; n < _positionList.Count; n++)
                    //{
                    //   if (_occupiedList[n] && bPos[n])
                    //   {
                    //      if (_positionList[n].CoilID != strList[n])
                    //      {
                    //         PositionDef def = (PositionDef)Enum.Parse(typeof(PositionDef), _positionList[n].Name);
                    //         if (def == PositionDef.EntrySkid1 | def == PositionDef.EntrySkid2)
                    //            bSendPos = bSendPos | _positionList[n].CoilID != null ? _listener.SendIdentification(n, _positionList[n].CoilID, strList[n]) : true;
                    //         else
                    //            bSendPos = true;
                    //         _positionList[n].CoilID = strList[n];
                    //      }
                    //   }
                    //   else
                    //   {
                    //      if (_occupiedList[n] != bPos[n])
                    //      {
                    //         _positionList[n].CoilID = _occupiedList[n] ? strList[n] : null;
                    //         bSendPos = true;
                    //      }
                    //   }
                    //   _positionList[n].Occupied = _occupiedList[n];
                    //}

                    //if (bSendPos)
                    //   if (_listener != null)
                    //      _listener.SendPositions(_positionList);
                    //bPos = bList.GetRange(8, 7).ToArray();

                    //if (bEvents == null)
                    //   bEvents = new bool[8];

                    ////bList = GetBoolArrayFromEvent(_valEvents, buffer);
                    //ChangeEvent(bList.GetRange(0, 8).ToArray(), bEvents, EventDef.Events);
                    //bEvents = bList.GetRange(0, 8).ToArray();

                    ////if (_ListenerCom != null)
                    ////   _ListenerCom.ReceiveData(buffer.ToArray(), 1);
                    //_Timer.Start();
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        public byte[] GetS71500ByteArray(int DB, int StartPos, int DBLength, bool bReverse) {
            try {
                var btList = this._S7.GetSingleByteArrayFromDB(DB, StartPos, DBLength);
                var btArr = btList.ToArray();

                if (bReverse) {
                    Array.Reverse(btArr);
                }

                return btArr;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return null;
            }
        }

        public T GetObjectFromS7<T>(TelegramDef def, string field, bool bReverse = false, int blockNo = 0) {

            try {
                var telegram = this._TelegramList.FirstOrDefault(t => t.Name == def.ToString());
                if (blockNo == 0) {
                    blockNo = telegram.TelegramID;
                }

                var val = telegram.TelegramValues.FirstOrDefault(t => t.Name == field);

                var btList = this._S7.GetSingleByteArrayFromDB(blockNo, val.AdressNo, val.Length);
                var btArr = btList.ToArray();

                if (bReverse) {
                    Array.Reverse(btArr);
                }

                return Functions.GetObjectFromByteArray<T>(btArr, val.Typ);
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return default;
            }
        }

        public bool SendValuesToS7(byte[] btArr, int dbNo) {
            try {
                var btList = btArr.ToList();

                var res = this._S7.SetSingleDBValue(btArr, 0, btArr.Length, dbNo, out var errCode);
                return !res ? throw new Exception(errCode) : true;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return false;
            }
        }
        public bool SendValueToS7(byte[] btArr, int dbNo, int pos = 0) {
            try {
                var btList = btArr.ToList();

                var res = this._S7.SetSingleDBValue(btArr, pos, btArr.Length, dbNo, out var errCode);
                return !res ? throw new Exception(errCode) : true;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return false;
            }
        }
        public bool SendSingleValueToS7(TelegramDef def, string field, object obj, int dbNo = 0) {
            try {
                var telegram = this._TelegramList.FirstOrDefault(t => t.Name == def.ToString());
                var item = telegram.TelegramValues.FirstOrDefault(t => t.Name == field);
                item.Value = obj;
                if (dbNo == 0) {
                    dbNo = telegram.TelegramID;
                }

                var btArr = Functions.GetByteArrayFromType(item.Value, item.Typ, item.Length);
                if (item.Typ != TypeCode.String) {
                    Array.Reverse(btArr);
                }

                return this._S7.SetSingleDBValue(btArr, item.BytePosition, item.Length, dbNo, out var errCode);
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return false;
            }
        }

        public byte[] GetS7ByteArray(int blockNo, int LengthToRead = 0) {
            try {
                var btList = this._S7.GetCompleteByteArrayFromDB(blockNo, LengthToRead);
                return [.. btList];
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return null;
            }
        }

        public IEventData Listener { get; set; }
        public IPLCCom ListenerCom { get; set; }

        public void Dispose() => throw new NotImplementedException();
    }
}
