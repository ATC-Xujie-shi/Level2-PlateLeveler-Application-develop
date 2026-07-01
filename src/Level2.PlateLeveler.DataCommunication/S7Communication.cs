using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.ErrorHandler;

namespace Level2.PlateLeveler.DataCommunication {
    public class S7Communication {
        private readonly S7Client _Client;
        private TcpListener _TCPListener;
        private S7Client.S7BlockInfo _Info;

        public TcpClient ClientTCP { get; }

        public NetworkStream ComStream { get; }

        public void DisconnectSocket() {
            this.Connected = false;
            this.ClientTCP?.Close();
        }

        [Obsolete]
        public bool ConnectToS7(IPAddress ipAddress, int Port) {
            _ = ipAddress.ToString();

            try {
                this._TCPListener ??= new TcpListener(3020);

                _ = this._TCPListener.AcceptTcpClient();

                //if (_ClientTCP == null)
                //   _ClientTCP = new TcpClient();
                //_ClientTCP.Connect(address, Port);
                //_ComStream = _ClientTCP.GetStream();
                //_ComStream.ReadTimeout = timeOutInterval;

                //_connected = _ClientTCP.Connected;

                return this.Connected;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                this.Connected = false;
                return this.Connected;
            }
        }

        public bool Connected { get; set; }

        public bool DisconnectS7() {
            try {
                var errCode = this._Client.Disconnect();
                if (errCode != 0) {
                    var message = this.GetErrorText("S7 Error", errCode);
                    throw new CustomErrorException(message, System.Reflection.MethodBase.GetCurrentMethod().Name);
                    //throw new Exception(message);
                }
                this.Connected = false;

                return true;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return false;
            }
        }

        public List<byte> GetCompleteByteArrayFromDB(int dbNo, int LengthToRead = 0) {
            try {
                var _DBNo = Convert.ToUInt16(dbNo);

                var DbLength = this._Info.MC7Size;

                if (LengthToRead > 0) {
                    DbLength = LengthToRead;
                }

                var rtnCode = this._Client.GetAgBlockInfo(S7Client.Block_DB, _DBNo, ref this._Info);
                var str = "";
                var data = new byte[DbLength];
                for (var n = 0; n < 3; n++) {
                    if ((rtnCode != 0) && (LengthToRead == 0)) {
                        _ = this._Client.Disconnect();
                        _ = this._Client.Connect();
                        rtnCode = this._Client.GetAgBlockInfo(S7Client.Block_DB, _DBNo, ref this._Info);
                        data = new byte[DbLength];
                        rtnCode = this._Client.DBGet(dbNo, data, ref DbLength);
                        if (rtnCode == 0) {
                            try {
                                str = this.GetErrorText("S7 Error " + rtnCode.ToString(), rtnCode);
                                Logging.SendMessage(str, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, this.GetType());
                            } catch { }
                            return [.. data];
                        }
                    }
                }
                rtnCode = this._Client.DBGet(dbNo, data, ref DbLength);
                if (rtnCode != 0) {
                    str = this.GetErrorText("S7 Error " + rtnCode.ToString(), rtnCode);
                    Logging.SendMessage(str, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, this.GetType());
                    throw new Exception(str);
                }
                return [.. data];
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                this.Connected = false;
                return null;
            }
        }

        public List<byte> GetSingleByteArrayFromDB(int dbNo, int bytePosition, int length) {
            var buffer = new byte[length];
            try {
                var rtnCode = 1;// _Client.ReadArea(S7Client.S7AreaDB, dbNo, bytePosition, length, S7Client.S7WLWord, buffer);
                for (var n = 0; n < 3; n++) {
                    if (rtnCode != 0) {
                        rtnCode = this._Client.ReadArea(S7Client.S7AreaDB, dbNo, bytePosition, length, S7Client.S7WLByte, buffer);
                        if (rtnCode == 0) {
                            return [.. buffer];
                        }

                        _ = this._Client.Disconnect();
                        _ = this._Client.Connect();
                    }
                }
                if (rtnCode != 0) {
                    //2016.03.21: Copy'n' Paste?!?
                    //string errorCode = "\n" + GetErrorText("DBWrite for DB" + dbNo.ToString(), rtnCode);
                    var errorCode = "\n" + this.GetErrorText("GetSingleByteArrayFromDB " + dbNo.ToString(), rtnCode);
                    Logging.SendMessage("ErrorCode: " + errorCode, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, this.GetType());
                    throw new Exception(errorCode);
                }
                return null;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return null;
            }
        }

        public string GetErrorText(string text, int errorCode) {
            try {
                return DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff") + ": " + text + ": " + this._Client.ErrorText(errorCode);
            } catch {
                return string.Empty;
            }
        }

        public bool SetSingleDBValue(byte[] btArr, int bytePos, int length, int dbNo, out string errorCode) {
            try {
                //string errorCode = string.Empty;
                var rtnCode = this._Client.DBWrite(dbNo, bytePos, length, btArr);
                if (rtnCode != 0) {
                    errorCode = "\n" + this.GetErrorText("DBWrite for DB" + dbNo.ToString(), rtnCode);
                    throw new Exception("DbNo: " + dbNo.ToString() + ", Pos: " + bytePos.ToString() + ", WriteLen: " + length.ToString() + " error:" + errorCode);
                }
                errorCode = "OK";
                return rtnCode == 0;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                errorCode = ex.Message;
                return false;
            }
        }

        private void currentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
            var ex = (Exception)e.ExceptionObject;
            Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            Logging.SendMessage("Runtime terminating: " + e.IsTerminating.ToString(), System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, this.GetType());
            if (ex.Source != null) {
                Logging.SendMessage("Source: " + ex.Source, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, this.GetType());
            }

            if (ex.InnerException != null) {
                Logging.SendErrorMessage("Inner Exception: " + System.Reflection.MethodBase.GetCurrentMethod().Name, ex.InnerException, this.GetType());
            }

            Console.WriteLine("MyHandler caught : " + ex.Message);
            Console.WriteLine("Runtime terminating: {0}", e.IsTerminating);
            _ = Console.ReadLine();
        }

        public int GetDBLengthSize(int dbNo) {
            try {

                return -1;
                //if (_Client.Connected())
                //{
                //   if (!_IsNewS7Type)
                //   {
                //      int rtnCode = _Client.GetAgBlockInfo(S7Client.Block_DB, dbNo, ref _Info);
                //      if (rtnCode != 0)
                //      {
                //         string str = GetErrorText("S7 Error " + rtnCode.ToString(), rtnCode);
                //         throw new Exception(str);
                //      }
                //      return _Info.MC7Size;
                //   }
                //   else
                //   {
                //      return 0;
                //   }
                //}
                //else
                //{
                //   return -1;
                //}
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return -1;
            }
        }

        public bool ReadMultiDBValues(ref List<byte[]> btListArr, List<int> dbList, int pos, int length, int offset = 0) {
            try {
                var currentDomain = AppDomain.CurrentDomain;
                currentDomain.UnhandledException += this.currentDomain_UnhandledException;

                if (this._Client == null) {
                    return false;
                }

                if (!this._Client.Connected()) {
                    return false;
                }

                var items = new S7Client.S7DataItem[btListArr.Count];
                for (var n = 0; n < btListArr.Count; n++) {
                    items[n] = new S7Client.S7DataItem();
                    var btArr = btListArr[n];
                    items[n].Set(S7Client.S7AreaDB, S7Client.S7WLByte, dbList[n], pos, length, ref btArr, offset);
                    btListArr[n] = btArr;
                }
                var errCode = string.Empty;
                var result = this._Client.ReadMultiVars(items, items.Length);
                if (result != 0) {
                    errCode = "\n" + this._Client.ErrorText(result);
                    throw new Exception(errCode);
                }

                return true;
            } catch (Exception) {
                return false;
            }
        }
    }
}
