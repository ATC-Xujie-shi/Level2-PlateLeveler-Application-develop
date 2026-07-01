using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.DataCommunication {
    public delegate void ReceiveComDataEventHandler(byte[] Telegram, object sender);
    public delegate void CommunicationLostEventHandler(object sender);
    public delegate void CommunicationSendEventHandler(byte[] Telegram);
    public delegate void CommunicationEstablishedEventHandler(object sender);

    public enum ByteDef {
        BigEndian, LittleEndian, Ascii
    }

    public class NetworkCommunication(int index) : IDisposable {
        public event ReceiveComDataEventHandler DataReceive;
        public event CommunicationLostEventHandler ComError;
        public event CommunicationSendEventHandler ComSendError;
        public event CommunicationEstablishedEventHandler ComEstablished;

        private int _TelegramLengthSize, _TelegramLengthOffset, _TelegramLengthAdjust;
        private bool IsActive, _IsVariableLength;
        private ByteDef _EndianForTelegramLength;
        private TcpClient ComClient;
        private NetworkStream ComStream;
        private TcpListener ComListener;
        private int glbPort;
        private Type type;
        private bool _IsLogging;
        private byte ETX;

        public bool Connected => this.ComClient != null && this.ComClient.Connected;

        public void ReceiveDataFromTCPIPInterface(int port, IPAddress address, bool bActive, int telegramLengthSize, int telegramLengthOffset, int telegramLengthAdjust, int endianForTelegramLength, int timeOutInterval, bool bLogging = false, byte etx = 0, bool bVariableLength = false) {
            this._TelegramLengthSize = telegramLengthSize;
            this._TelegramLengthOffset = telegramLengthOffset;
            this._TelegramLengthAdjust = telegramLengthAdjust;
            this._EndianForTelegramLength = (ByteDef)endianForTelegramLength;
            this.IsActive = bActive;
            this._IsVariableLength = bVariableLength;
            this.glbPort = port;
            this.type = typeof(NetworkCommunication);
            this._IsLogging = bLogging;
            this.ETX = etx;
            while (true) {
                //Check if the connection should be active or passive
                if (bActive) {
                    while (true) {
                        try {
                            // Try to connect to a server
                            this.ComClient ??= new TcpClient();

                            this.ComClient.Connect(address, port);
                            this.ComStream = this.ComClient.GetStream();
                            this.ComStream.ReadTimeout = timeOutInterval;
                            this.Listener?.ConnectionEstablished(true, this.Index);

                            break;
                        } catch {
                            // Connecting was not possible
                            this.ComClient = null;
                            Thread.Sleep(2000);
                            this.Listener?.ConnectionEstablished(false, this.Index);
                        }
                    }
                } else {
                    try {
                        this.ComListener ??= new TcpListener(IPAddress.Any, port);

                        this.ComListener.Start();

                        this.ComClient = new TcpClient();
                        this.ComClient = this.ComListener.AcceptTcpClient();
                        this.ComStream = this.ComClient.GetStream();
                        this.ComStream.ReadTimeout = timeOutInterval;
                        this.Listener?.ConnectionEstablished(true, this.Index);
                    } catch {
                        Thread.Sleep(2000);
                        this.Listener?.ConnectionEstablished(false, this.Index);
                    }
                }

                // When a connection is established
                var ReceivedData = new List<byte>();
                while (true) {
                    try {
                        // Receive Data - Wait for Data
                        this.ReceiveDataFromTCPStream(ref ReceivedData);
                        if (ReceivedData.Count == 0) {
                            // TCP disconnected
                            this.CloseCom();
                            break;
                        } else {
                            if (this.ETX != 0) {
                                this.HandleReceivedDataWithETX(ref ReceivedData);
                            } else {
                                this.HandleReceivedData(ref ReceivedData);
                            }
                        }
                    } catch {
                        // TCP Socket error
                        this.CloseCom();
                        break;
                    }
                }
            }
        }

        public void CloseCom() {
            this.ComClient?.Close();

            this.Listener?.ConnectionEstablished(false, this.Index);
        }

        private void ReceiveDataFromTCPStream(ref List<byte> ReceivedData) {
            int BytesRead;
            if (this.ComClient is not null) {
                var btarry = new byte[this.ComClient.ReceiveBufferSize];
                try {
                    if (this.ComStream != null) {
                        if (this.ComStream.CanRead) {
                            BytesRead = this.ComStream.Read(btarry, 0, btarry.Length);

                            //Console.WriteLine(DateTime.Now + " - BytesRead: " + BytesRead);
                            for (var n = 0; n < BytesRead; n++) {
                                ReceivedData.Add(btarry[n]);
                            }
                        }
                    }
                } catch (Exception e) {
                    if (this._IsLogging) {
                        Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                    }
                }
            }
        }

        private List<byte> result = [];
        private int LengthToCompleteTele;

        private void HandleReceivedData(ref List<byte> btDataReceived) {
            var bbufferHandeled = false;
            var btLength = new List<byte>();
            var receivedBytesCount = btDataReceived.Count;

            //DropArray(btDataReceived);

            if (this.result.Count == 0) {
                this.LengthToCompleteTele = this.GetTeleLength(btDataReceived);
            }

            if (this.LengthToCompleteTele != -1) {
                for (var n = 0; n < btDataReceived.Count; n++) {
                    this.result.Add(btDataReceived[n]);
                    this.LengthToCompleteTele--;
                    if (this.LengthToCompleteTele.Equals(0)) {
                        try {
                            if (this.Listener != null) {
                                if (!this.Listener.ReceiveData([.. this.result], this.Index) && this.result.Count != btDataReceived.Count) {
                                    Logging.SendMessage("Warning: could not handle telegram. Trying the last received (full) telegram with " + btDataReceived.Count + " bytes again!", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Warning, this.GetType());
                                    this.result = [];
                                    this.HandleReceivedData(ref btDataReceived);
                                }
                            }
                        } catch { }

                        this.result = [];
                        for (var i = n + 1; i < btDataReceived.Count; i++) {
                            btLength.Add(btDataReceived[i]);
                        }
                        btDataReceived = [];
                        bbufferHandeled = true;
                        if (btLength.Count > 0) {
                            for (var i = 0; i < btLength.Count; i++) {
                                btDataReceived.Add(btLength[i]);
                            }
                            this.HandleReceivedData(ref btDataReceived);
                        }
                        break;
                    }
                }
                if (!bbufferHandeled) {
                    btDataReceived = [];
                }

                if (this.LengthToCompleteTele > 0) {
                    Logging.SendMessage("Warning: Received " + receivedBytesCount + " bytes, but still waiting for " + this.LengthToCompleteTele + " bytes to complete the telegram.", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Warning, this.GetType());
                }
            } else {
                Logging.SendMessage("Warning: Failed to get and/or validate telegram length. Received " + btDataReceived.Count + " bytes.", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Warning, this.GetType());
            }
        }

        private int GetTeleLength(List<byte> DataReceived) {
            if (DataReceived.Count >= this._TelegramLengthOffset + this._TelegramLengthSize) {
                int TelegramLengthCompl;
                var tmpresult = new List<byte>();
                byte[] btArr;

                for (var n = 0; n < this._TelegramLengthOffset + this._TelegramLengthSize; n++) {
                    tmpresult.Add(DataReceived[n]);
                }

                btArr = [.. tmpresult.GetRange(this._TelegramLengthOffset, this._TelegramLengthSize)];

                if (this._EndianForTelegramLength == ByteDef.Ascii) {
                    TelegramLengthCompl = Convert.ToInt32(Encoding.ASCII.GetString(btArr));
                } else {
                    if (this._EndianForTelegramLength == ByteDef.BigEndian) {
                        Array.Reverse(btArr);
                    }

                    TelegramLengthCompl = BitConverter.ToInt16(btArr, 0);
                }

                return TelegramLengthCompl + this._TelegramLengthAdjust;
            } else {
                return -1;
            }
        }

        //List<byte> result = new List<byte>();
        //List<byte> buffer = new List<byte>();

        //private void HandleReceivedData(ref List<byte> btDataReceived)
        //{
        //   bool bbufferHandeled = false;
        //   List<byte> btLength = new List<byte>();
        //   int LengthToCompleteTele = 0;

        //   DropArray(btDataReceived);

        //   if (result.Count == 0)
        //      LengthToCompleteTele = GetTeleLength(btDataReceived);

        //   if (LengthToCompleteTele != -1)
        //   {
        //      for (int n = 0; n < btDataReceived.Count; n++)
        //      {
        //         result.Add(btDataReceived[n]);
        //         LengthToCompleteTele--;
        //         if (LengthToCompleteTele.Equals(0))
        //         {
        //            try
        //            {
        //               if (_listener != null)
        //                  _listener.ReceiveData(result.ToArray(), _index);
        //            }
        //            catch { }

        //            result = new List<byte>();
        //            for (int i = (n + 1); i < btDataReceived.Count; i++)
        //               btLength.Add(btDataReceived[i]);

        //            btDataReceived = new List<byte>();
        //            bbufferHandeled = true;
        //            if (btLength.Count > 0)
        //            {
        //               btDataReceived.AddRange(btLength);

        //               HandleReceivedData(ref btDataReceived);
        //            }
        //            break;
        //         }
        //      }
        //      if (!bbufferHandeled)
        //         btDataReceived = new List<byte>();
        //   }
        //}

        private void HandleReceivedDataWithETX(ref List<byte> btDataReceived) {
            _ = new ASCIIEncoding();
            var bEnd = false;
            foreach (var bt in btDataReceived) {
                if (this.ETX.Equals(bt)) {
                    bEnd = true;
                    try {
                        this.result.Add(bt);
                        _ = this.Listener?.ReceiveData([.. this.result], this.Index);

                        this.result = [];
                    } catch { }
                    break;
                } else {
                    this.result.Add(bt);
                }
            }
            btDataReceived = bEnd ? [] : this.result;
        }

        //private int GetTeleLength(List<byte> DataReceived)
        //{
        //   if (DataReceived.Count >= _TelegramLengthOffset + _TelegramLengthSize)
        //   {
        //      int TelegramLengthCompl;

        //      byte[] btArr = DataReceived.GetRange(_TelegramLengthOffset, _TelegramLengthSize).ToArray();

        //      //DropArray2(99, DataReceived);

        //      if (_EndianForTelegramLength == ByteDef.Ascii)
        //         TelegramLengthCompl = Convert.ToInt32(Encoding.GetEncoding(1251).GetString(btArr));
        //      else
        //      {
        //         if (_EndianForTelegramLength == ByteDef.BigEndian)
        //            Array.Reverse(btArr);

        //         //DropArray2(999, btArr);

        //         TelegramLengthCompl = (int)BitConverter.ToInt16(btArr, 0);
        //      }

        //      return TelegramLengthCompl + _TelegramLengthAdjust;
        //   }
        //   else
        //      return -1;
        //}

        public void SendDataToTCPIPInterface(byte[] buffer) {
            try {
                if (this.ComStream.CanWrite) {
                    this.ComStream.Write(buffer, 0, buffer.Length);
                } else {
                    this.Listener?.ConnectionEstablished(false, this.Index);
                }
            } catch {
                this.Listener?.ConnectionEstablished(false, this.Index);
            }
        }

        public int Index { get; set; } = index;
        public ICommunication Listener { get; set; }

        public void Dispose() => throw new NotImplementedException();
    }
}
