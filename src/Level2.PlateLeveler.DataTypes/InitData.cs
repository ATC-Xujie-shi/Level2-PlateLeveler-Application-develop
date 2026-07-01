using System;
using System.Collections.Generic;
using System.Net;
using System.Xml.Serialization;

namespace Level2.PlateLeveler.DataTypes {
    public enum ProviderDef {
        SQLServer, Oracle, Firebird, MySQL, Access
    }

    /// <summary>
    /// Enum of available names for connections
    /// </summary>
    public enum ConnectionDef {
        /// <summary>
        /// Product Data Input
        /// </summary>
        PDI,
        /// <summary>
        /// Process Data
        /// </summary>
        PD,
        /// <summary>
        /// Product Data Output
        /// </summary>
        PDO
    }

    public enum CommunicationDef {
        L3_L2, L2_L3, L1_L2, L2_L1, FM_L2, HMI_L2, S7_L2, Events_L2
    }

    /// <summary>
    /// Enum of available order numbers
    /// </summary>
    public enum OrderNumberDef {
        C26841216
    }

    public class InitData {
        [XmlArray("DataBase")]
        [XmlArrayItem("Connection")]
        public ConnectionList Connections { get; set; }

        [XmlArray("TCPIP")]
        [XmlArrayItem("Communication")]
        public CommunicationList Communications { get; set; }

        [XmlArray("PLC")]
        [XmlArrayItem("S7")]
        public S7List S7 { get; set; }

        [XmlElement]
        public ConstantData Constants;

        [XmlElement]
        public ReportData Report;

        [XmlElement]
        public IntervalData Interval;

        public string TelegramFile => this.Constants.TelegramFile;
    }

    [Serializable]
    public class IntervalData {
        [XmlAttribute]
        public double S7 { get; set; }
        [XmlAttribute]
        public double Life { get; set; }

        [XmlAttribute]
        public double L3L2 { get; set; }

        [XmlAttribute]
        public double WatchDogRefresh { get; set; }

        [XmlAttribute]
        public double WatchDogErrorOffset { get; set; }

        [XmlAttribute]
        public int PredictionError { get; set; }
        [XmlAttribute]
        public int PredictModel { get; set; }
    }

    [Serializable]
    public class ConstantData {
        private string _SpcPath;
        [XmlAttribute]
        public string SpcPath {
            get => this._SpcPath; set => this._SpcPath = value;
        }

        private string _PsmPath;
        [XmlAttribute]
        public string PsmPath {
            get => this._PsmPath; set => this._PsmPath = value;
        }

        private string _TelegramFile;
        [XmlAttribute]
        public string TelegramFile {
            get => this._TelegramFile; set => this._TelegramFile = value;
        }

        private string _LoggerType;
        [XmlAttribute]
        public string LoggerType {
            get => this._LoggerType; set => this._LoggerType = value;
        }

        [XmlAttribute]
        public double Interval { get; set; }

        [XmlAttribute]
        public float MaxThicknessPlate { get; set; }

        private string _Order;
        /// <summary>
        /// Ordernumber of the customer, will activate customerspcific features
        /// </summary>
        [XmlAttribute]
        public string Order {
            get => this._Order; set => this._Order = value;
        }

        [XmlAttribute]
        public bool LogNetworkCommunication { get; set; }

        [XmlAttribute]
        public StoodBoltType StoodBolt { get; set; }
    }

    [Serializable]
    public class CommunicationData : BasicCommunication {
        [XmlAttribute("Timeout", DataType = "int")]
        public int Timeout { get; set; }
        [XmlAttribute("Start")]
        public bool Start { get; set; }

        [XmlAttribute("Active")]
        public bool Active { get; set; }

        [XmlAttribute("Endian", DataType = "int")]
        public int Endian { get; set; }

        [XmlAttribute("TelegramLengthSize", DataType = "int")]
        public int TelegramLengthSize { get; set; }

        [XmlAttribute("TelegramLengthOffset", DataType = "int")]
        public int TelegramLengthOffset { get; set; }
        [XmlAttribute("TypeLengthSize", DataType = "int")]
        public int TypeLengthSize { get; set; }

        [XmlAttribute("TypeLengthOffset", DataType = "int")]
        public int TypeLengthOffset { get; set; }

        [XmlAttribute("TelegramLengthAdjust", DataType = "int")]
        public int TelegramLengthAdjust { get; set; }

        [XmlAttribute("LogLiveTelegram")]
        public bool LogLiveTelegram { get; set; }

        public int Index { get; set; }
    }

    [Serializable]
    public class BasicCommunication {
        protected string _Address;
        [XmlAttribute("IPAddress", DataType = "string")]
        public string Address {
            get => this._Address; set => this._Address = value;
        }
        public IPAddress IPAddresse => IPAddress.Parse(this._Address);

        protected CommunicationDef _Name;
        [XmlAttribute("Name")]
        public CommunicationDef Name {
            get => this._Name; set => this._Name = value;
        }

        protected int _Port;
        [XmlAttribute("Port", DataType = "int")]
        public int Port {
            get => this._Port; set => this._Port = value;
        }

        protected int _IsNewS7Type;
        [XmlAttribute("IsNewS7Type", DataType = "int")]
        public int IsNewS7Type {
            get => this._IsNewS7Type; set => this._IsNewS7Type = value;
        }

        protected int _IsSocketCom;
        [XmlAttribute("IsSocketCom", DataType = "int")]
        public int IsSocketCom {
            get => this._IsSocketCom; set => this._IsSocketCom = value;
        }
    }

    [Serializable]
    public class CommunicationList : List<CommunicationData> {
        public CommunicationList()
           : base() {
        }
        public CommunicationData GetItem(int port) {
            CommunicationData data = new();
            foreach (var item in this) {
                if (item.Port.Equals(port)) {
                    return item;
                }
            }

            return data;
        }
    }
    [Serializable]
    public class ConnectionData {
        private string _Name;
        /// <summary>
        /// Name for this connection
        /// Possible values:
        /// <para/>PSM: Connection for PSM Server
        /// <para/>PD: Level 3 connection for PDI and PDO
        /// <para/>PDI: Level 3 connection for PDI
        /// <para/>PDO: Level 3 connection for PDI
        /// </summary>
        [XmlAttribute("Name", DataType = "string")]
        public string Name {
            get => this._Name; set => this._Name = value;
        }

        private string _Namespace;
        [XmlAttribute("Namespace", DataType = "string")]
        public string Namespace {
            get => this._Namespace; set => this._Namespace = value;
        }

        private string _ProviderName;
        [XmlAttribute("ProviderName", DataType = "string")]
        public string ProviderName {
            get => this._ProviderName; set => this._ProviderName = value;
        }

        private string _DataSource;
        [XmlAttribute("DataSource", DataType = "string")]
        public string DataSource {
            get => this._DataSource; set => this._DataSource = value;
        }

        [XmlAttribute]
        public bool PersistSecurityInfo { get; set; }

        private string _IntegratedSecurity;
        [XmlAttribute("IntegratedSecurity", DataType = "string")]
        public string IntegratedSecurity {
            get => this._IntegratedSecurity; set => this._IntegratedSecurity = value;
        }

        private string _Database;
        [XmlAttribute("Database", DataType = "string")]
        public string Database {
            get => this._Database; set => this._Database = value;
        }

        private string _UserId;
        [XmlAttribute("UserId", DataType = "string")]
        public string UserId {
            get => this._UserId; set => this._UserId = value;
        }

        [XmlAttribute("Port", DataType = "int")]
        public int Port { get; set; }

        private string _Password;
        [XmlAttribute("Password", DataType = "string")]
        public string Password {
            get => this._Password; set => this._Password = value;
        }

        private string _Unicode;
        [XmlAttribute("Unicode", DataType = "string")]
        public string Unicode {
            get => this._Unicode; set => this._Unicode = value;
        }

        public string ConnectionString {
            get {
                var result = this._ProviderName is not null and not "" ? "Provider=" + this._ProviderName + ";" : "";
                result += this._DataSource is not null and not "" ? "Data Source=" + this._DataSource : "";
                result += this.Port != 0 ? ", " + this.Port.ToString() : "";
                result += (this._DataSource != null && !string.IsNullOrEmpty(this._DataSource)) | this.Port != 0 ? "; " : "";
                result += this._Database is not null and not "" ? "Initial Catalog=" + this._Database + ";" : "";
                result += this._IntegratedSecurity is not null and not "" ? "Integrated Security=" + this._IntegratedSecurity + ";" : "";
                result += this.PersistSecurityInfo ? "Persist Security Info=" + this.PersistSecurityInfo.ToString().ToUpper(System.Globalization.CultureInfo.CurrentCulture) + ";" : "";
                result += this._UserId is not null and not "" ? "User Id=" + this._UserId + ";" : "";
                result += this._Password is not null and not "" ? "Password=" + this._Password + ";" : "";
                result += this._Unicode is not null and not "" ? "Unicode=" + this._Unicode + ";" : "";
                return result;
            }
        }

        public ProviderDef Provider {
            get {
                var strArr = this._Namespace.Split(['.']);
                return strArr[strArr.Length - 1] switch {
                    "SqlClient" => ProviderDef.SQLServer,
                    "OracleClient" => ProviderDef.Oracle,
                    "FirebirdClient" => ProviderDef.Firebird,
                    "MySqlClient" => ProviderDef.MySQL,
                    _ => ProviderDef.Access,
                };
            }
        }
    }
    [Serializable]
    public class ConnectionList : List<ConnectionData> {
        public ConnectionList()
           : base() {
        }

        /// <summary>
        /// Check is this as a connection with given name
        /// </summary>
        /// <param name="name">Name of the connection</param>
        /// <returns>True if match, else false</returns>
        public bool HasItem(string name) {
            foreach (var item in this) {
                if (item.Name.Equals(name, StringComparison.Ordinal)) {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Get ConnectionData for given name
        /// </summary>
        /// <param name="name">Name of the connection</param>
        /// <returns>ConnectionData or null if no match</returns>
        public ConnectionData GetItem(string name) {
            foreach (var item in this) {
                if (item.Name.Equals(name, StringComparison.Ordinal)) {
                    return item;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class HttpServerData {
        private string _Name;
        [XmlAttribute]
        public string Name {
            get => this._Name; set => this._Name = value;
        }

        [XmlAttribute]
        public int Port { get; set; }
        [XmlAttribute]
        public int HttpTypeLengthSize { get; set; }

        [XmlAttribute]
        public int HttpTypeLengthOffset { get; set; }
    }

    [Serializable]
    public class HttpServerList : List<HttpServerData> {
        public HttpServerList()
           : base() {
        }

        public HttpServerData GetItem(string name) {
            foreach (var item in this) {
                if (item.Name.Equals(name, StringComparison.Ordinal)) {
                    return item;
                }
            }
            return null;
        }

        public HttpServerData GetItem(int port) {
            foreach (var item in this) {
                if (item.Port.Equals(port)) {
                    return item;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class HttpClientData {
        private string _Name;
        [XmlAttribute]
        public string Name {
            get => this._Name; set => this._Name = value;
        }

        private string _Host;
        [XmlAttribute]
        public string Host {
            get => this._Host; set => this._Host = value;
        }

        [XmlAttribute]
        public int Port { get; set; }

        private string _Server;
        [XmlAttribute]
        public string Server {
            get => this._Server; set => this._Server = value;
        }
        public string Url => this.Port != 0 ? "http://" + this._Host + ":" + this.Port.ToString() + "/" + this._Server : "http://" + this._Host + "/" + this._Server;
    }

    [Serializable]
    public class HttpClientList : List<HttpClientData> {
        public HttpClientList()
           : base() {
        }

        public HttpClientData GetItem(string name) {
            foreach (var item in this) {
                if (item.Name.Equals(name, StringComparison.Ordinal)) {
                    return item;
                }
            }
            return null;
        }

        public HttpClientData GetItem(int port) {
            foreach (var item in this) {
                if (item.Port.Equals(port)) {
                    return item;
                }
            }
            return null;
        }
    }
    [Serializable]
    public class QDASConstantData {
        [XmlAttribute]
        public bool WithPassNo { get; set; }

        private string _Path;
        [XmlAttribute]
        public string Path {
            get => this._Path; set => this._Path = value;
        }

        [XmlAttribute]
        public int GenerationDuration { get; set; }

        private string _XMLFile;
        [XmlAttribute]
        public string XMLFile {
            get => this._XMLFile; set => this._XMLFile = value;
        }
    }

    [Serializable]
    public class S7ComData : BasicCommunication {
        [XmlAttribute()]
        public int RackNo { get; set; }
        [XmlAttribute()]
        public int SlotNo { get; set; }

        [XmlAttribute()]
        public int BlockNo { get; set; }

        [XmlAttribute()]
        public new int IsNewS7Type { get; set; }
    }

    [Serializable]
    public class S7List : List<S7ComData> {
        public S7List()
           : base() {
        }
    }
}
