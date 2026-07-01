using System;
using System.Threading;
using Infragistics.Controls.Charts;
using Level2.PlateLeveler.DataCommunication;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.Client {
    public delegate void ReceiveDataEventHandler(byte[] Telegram, int index);
    public delegate void EstablishedDataEventHandler(int index);
    public delegate void ErrorDataEventHandler(int index);
    public class StartThreadCom : ICommunication {
        private readonly NetworkCommunication[] _ComArray;
        private readonly CommunicationList _ListCom;

        public event ReceiveDataEventHandler DataReceived;
        public event EstablishedDataEventHandler ComEstablished;
        public event ErrorDataEventHandler ComError;
        private readonly InitData _Initialization;
        private readonly TelegramList _TelegramList;
        public StartThreadCom(CommunicationList list) {
            this._ListCom = list;
            this._ComArray = new NetworkCommunication[this._ListCom.Count];
            for (var n = 0; n < this._ComArray.Length; n++) {
                this._ComArray[n] = new NetworkCommunication(n);
                this._ComArray[n].DataReceive += new ReceiveComDataEventHandler(this.com_DataReceive);
                this._ComArray[n].ComError += new CommunicationLostEventHandler(this.com_ComError);
                this._ComArray[n].ComEstablished += new CommunicationEstablishedEventHandler(this.com_ComEstablished);
            }
        }
        public StartThreadCom(InitData init, TelegramList list) {

            try {
                this._TelegramList = list;
                this._Initialization = init;
                this._ComArray = new NetworkCommunication[init.Communications.Count];
                for (var n = 0; n < this._ComArray.Length; n++) {
                    this._ComArray[n] = new NetworkCommunication(n) {
                        Listener = this
                    };
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }
        public void ConnectionEstablished(bool isOn, int index = 0) => this.Listener?.ConnectionEstablished(isOn, index);
        public bool ReceiveData(byte[] arr, int index = 0) => this.Listener != null && this.Listener.ReceiveData(arr, index);
        private void com_ComEstablished(object sender) {
            var com = (NetworkCommunication)sender;
            for (var n = 0; n < this._ComArray.Length; n++) {
                if (this._ComArray[n].Equals(com)) {
                    ComEstablished(n);
                }
            }
        }

        private void com_ComError(object sender) {
            var com = (NetworkCommunication)sender;
            for (var n = 0; n < this._ComArray.Length; n++) {
                if (this._ComArray[n].Equals(com)) {
                    ComError(n);
                }
            }
        }

        private void com_DataReceive(byte[] Telegram, object sender) {
            var index = 0;
            var com = (NetworkCommunication)sender;
            for (var n = 0; n < this._ComArray.Length; n++) {
                if (this._ComArray[n].Equals(com)) {
                    index = n;
                }
            }

            DataReceived(Telegram, index);
        }

        public CommunicationList StartThread() {
            //var thread = new Thread(new ParameterizedThreadStart(this.StartThread));
            //for (var n = 0; n < this._ListCom.Count; n++) {
            //    if (this._ListCom[n].Start) {
            //        thread = new Thread(new ParameterizedThreadStart(this.StartThread));
            //        this._ListCom[n].Index = n;
            //        thread.Start(this._ListCom[n]);
            //    }
            //}
            //return this._ListCom;
            var thread = new Thread(new ParameterizedThreadStart(this.StartThread));
            for (var n = 0; n < this._Initialization.Communications.Count; n++) {
                if (this._Initialization.Communications[n].Start) {
                    thread = new Thread(new ParameterizedThreadStart(this.StartThread));
                    this._Initialization.Communications[n].Index = n;
                    thread.Start(this._Initialization.Communications[n]);
                }
            }
            return this._Initialization.Communications;
        }

        private void StartThread(object data) {
            var dataCom = (CommunicationData)data;
            this._ComArray[dataCom.Index].ReceiveDataFromTCPIPInterface(dataCom.Port, dataCom.IPAddresse, dataCom.Active, dataCom.TelegramLengthSize, dataCom.TelegramLengthOffset, dataCom.TelegramLengthAdjust, dataCom.Endian, dataCom.Timeout);
        }

        public void SendByteArray(byte[] btArr, int index) {
            if (this._ComArray[index].Connected) {
                //_ComArray[index].ReceiveDataFromTCPIPInterface(_ListCom[index].Port, _ListCom[index].IPAddresse, _ListCom[index].Active, _ListCom[index].TelegramLengthSize, _ListCom[index].TelegramLengthOffset, _ListCom[index].TelegramLengthAdjust, _ListCom[index].Endian, _ListCom[index].Timeout);

                this._ComArray[index].SendDataToTCPIPInterface(btArr);
            }
        }

        public ICommunication Listener { get; set; }
    }

    public class ComDataEventArgs(string subject, string message) : EventArgs {
        public string Subject { get; } = subject;

        public string Message { get; } = message;
    }
}
