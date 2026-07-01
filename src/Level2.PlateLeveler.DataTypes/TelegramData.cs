using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Level2.PlateLeveler.DataTypes {
    public enum TelegramDef {
        HMI_L2_SignOfLife,
        HMI_L2_Request,
        HMI_L2_Limitation,
        L2_HMI_SignOfLife,
        L2_HMI_Tracking,
        L2_HMI_Signal,
        L3_L2_SignOfLife,
        L3_L2_PDI,
        L2_L3_SignOfLife,
        L2_L3_Tracking,
        L2_L3_PlateReport,
        L2_L3_ActiveCassette,
        L2_L3_MES,
        L1_L2_SignOfLife,
        L1_L2_RPDI,
        L1_L2_RADJ,
        L1_L2_PlateState,
        L1_L2_Tracking,
        L1_L2_ActVal,
        L1_L2_Meso,
        L1_L2_ActiveCassette,
        L1_L2_LineState,
        FM_L2_SignOfLife,
        FM_L2_MES,
        L2_FM_Meso,
        L2_L1_PDI,
        L2_L1_MES,
        L2_L1_ADJ,
        L2_L1_SignOfLife,
        L2_FM_SignOfLife,
        L2_HMI_MissingPDI,
        L2_HMI_DatabaseUpdate
    }

    public enum TelegramTypeDef {
        Acknowledge, LifeCheck, Data, S7Receive, S7Send
    }

    [Serializable]
    public class TelegramBlockData {
        private string _Name;
        [XmlAttribute]
        public string Name {
            get => this._Name; set => this._Name = value;
        }

        private string _Identity;
        [XmlAttribute]
        public string Identity {
            get => this._Identity; set => this._Identity = value;
        }

        private int _Length;
        [XmlAttribute]
        public int Length {
            get => this._Length; set => this._Length = value;
        }

        private int _Count;
        [XmlAttribute]
        public int Count {
            get => this._Count; set => this._Count = value;
        }

        private TelegramBlockList _TelegramBlocks;
        [XmlArray("TelegramList")]
        [XmlArrayItem("Telegramblock")]
        public TelegramBlockList TelegramBlocks {
            get => this._TelegramBlocks; set => this._TelegramBlocks = value;
        }

        //private TelegramBlockList[] _TelegramBlockArray;
        //public TelegramBlockList[] TelegramBlockArray
        //{
        //   get { return _TelegramBlockArray; }
        //   set { _TelegramBlockArray = value; }
        //}

        private TelegramBlockData[] _TelegramArray;
        [XmlArray("TelegramArray")]
        [XmlArrayItem("Telegramblock")]
        public TelegramBlockData[] TelegramArray {
            get => this._TelegramArray; set => this._TelegramArray = value;
        }

        private string _Format;
        [XmlAttribute]
        public string Format {
            get => this._Format; set => this._Format = value;
        }

        public TypeCode Typ => (TypeCode)Enum.Parse(typeof(TypeCode), this._Format);

        public string ShortTypeString {
            get {
                return this.Typ switch {
                    TypeCode.Boolean => "bool",
                    TypeCode.Char => "char",
                    TypeCode.DateTime => "DateTime",
                    TypeCode.Double => "double",
                    TypeCode.Int16 => "short",
                    TypeCode.Int32 => "int",
                    TypeCode.Int64 => "long",
                    TypeCode.Single => "float",
                    TypeCode.Empty => throw new NotImplementedException(),
                    TypeCode.Object => throw new NotImplementedException(),
                    TypeCode.DBNull => throw new NotImplementedException(),
                    TypeCode.SByte => throw new NotImplementedException(),
                    TypeCode.Byte => throw new NotImplementedException(),
                    TypeCode.UInt16 => throw new NotImplementedException(),
                    TypeCode.UInt32 => throw new NotImplementedException(),
                    TypeCode.UInt64 => throw new NotImplementedException(),
                    TypeCode.Decimal => throw new NotImplementedException(),
                    TypeCode.String => throw new NotImplementedException(),
                    _ => "string",
                };
            }
        }
        private bool _InDatabase;
        [XmlAttribute]
        public bool InDatabase {
            get => this._InDatabase; set => this._InDatabase = value;
        }

        private bool _InObject;
        [XmlAttribute]
        public bool InObject {
            get => this._InObject; set => this._InObject = value;
        }

        private object _Value;
        public object Value {
            get => this._Value; set => this._Value = value;
        }
    }

    [Serializable]
    public class TelegramBlockList : List<TelegramBlockData> {
        public TelegramBlockList()
           : base() {
        }

        private TelegramBlockList[] _TelegramBlockArray;
        public TelegramBlockList[] TelegramBlockArray {
            get => this._TelegramBlockArray; set => this._TelegramBlockArray = value;
        }
        private string _Name;
        public TelegramBlockData FindIndex(string name, out int idx) {
            this._Name = name;
            var n = this.FindIndex(this.IsData);
            idx = n;
            return this.Count > n ? this[n] : null;
        }

        public bool IsData(TelegramBlockData data) {
            if (this.Count > 1) {
                if (data.Name.Trim().Equals(this._Name, StringComparison.Ordinal)) {
                    return true;
                }
            }
            return false;
        }

        public TelegramBlockData GetItem(string block) {
            foreach (var item in this) {
                if (item.Name.Equals(block, StringComparison.Ordinal)) {
                    return item;
                }
            }

            return null;
        }

        //public TelegramBlockList[] GetTelegramListByLength(int length)
        //{
        //    TelegramBlockList result = new TelegramBlockList();
        //    for (int n = 0; n < length; n++)
        //        result.Add(this[n]);
        //    return result;
        //}
    }

    [Serializable]
    public class TelegramData {
        private string _Name;
        [XmlAttribute]
        public string Name {
            get => this._Name; set => this._Name = value;
        }

        private int _TelegramID;
        [XmlAttribute]
        public int TelegramID {
            get => this._TelegramID; set => this._TelegramID = value;
        }

        private string _MessageID;
        [XmlAttribute]
        public string MessageID {
            get => this._MessageID; set => this._MessageID = value;
        }

        private string _strLength;
        [XmlAttribute]
        public string StringLength {
            get => this._strLength; set => this._strLength = value;
        }

        private TelegramValueList _TelegramValues;
        [XmlArray("Telegram")]
        [XmlArrayItem("TelegramValue")]
        public TelegramValueList TelegramValues {
            get => this._TelegramValues; set => this._TelegramValues = value;
        }

        private int _Length;
        [XmlAttribute]
        public int Length {
            get => this._Length; set => this._Length = value;
        }

        private int _ComIndex;
        [XmlAttribute]
        public int ComIndex {
            get => this._ComIndex; set => this._ComIndex = value;
        }

        private TelegramTypeDef _telegramType;
        [XmlAttribute]
        public TelegramTypeDef TelegramType {
            get => this._telegramType; set => this._telegramType = value;
        }

        private TelegramBlockList _TelegramBlocks;
        [XmlArray("TelegramList")]
        [XmlArrayItem("Telegramblock")]
        public TelegramBlockList TelegramBlocks {
            get => this._TelegramBlocks; set => this._TelegramBlocks = value;
        }
    }

    [Serializable]
    [XmlRoot("TelegramList")]
    public class TelegramList : List<TelegramData> {
        public TelegramList()
           : base() {
        }
        private string _Name;
        public TelegramData FindIndex(string name) {
            this._Name = name;
            var n = this.FindIndex(this.IsData);
            return this[n];
        }

        private bool IsData(TelegramData data) {
            if (this.Count > 1) {
                if (data.Name.Trim().Equals(this._Name, StringComparison.Ordinal)) {
                    return true;
                }
            }
            return false;
        }

        public TelegramData GetItem(string name) {
            var result = new TelegramData();
            foreach (var item in this) {
                if (item.Name.Equals(name, StringComparison.Ordinal)) {
                    result = item;
                    break;
                }
            }

            return result;
        }

        public TelegramData GetItem<K>(K obj) {
            var result = new TelegramData();
            foreach (var item in this) {
                if (typeof(K).Equals(typeof(int)) && item.TelegramID.Equals(obj)) {
                    result = item;
                    break;
                }
            }

            return result;
        }
    }

    [Serializable]
    public class TelegramValueData {
        private string _Name;
        [XmlAttribute]
        public string Name {
            get => this._Name; set => this._Name = value;
        }

        private string _Identity = "";
        [XmlAttribute]
        public string Identity {
            get => this._Identity; set => this._Identity = value;
        }

        private int _Length;
        [XmlAttribute]
        public int Length {
            get => this._Length; set => this._Length = value;
        }

        private int _TelegramLength;
        [XmlAttribute]
        public int TelegramLength {
            get => this._TelegramLength; set => this._TelegramLength = value;
        }

        private int _TelegramType;
        [XmlAttribute]
        public int TelegramType {
            get => this._TelegramType; set => this._TelegramType = value;
        }

        private int _Count;
        [XmlAttribute]
        public int Count {
            get => this._Count; set => this._Count = value;
        }

        private bool _endOfHeader;
        [XmlAttribute]
        public bool EndOfHeader {
            get => this._endOfHeader; set => this._endOfHeader = value;
        }

        private TelegramValueList _TelegramValues;
        [XmlArray("TelegramList")]
        [XmlArrayItem("TelegramValue")]
        public TelegramValueList TelegramValues {
            get => this._TelegramValues; set => this._TelegramValues = value;
        }

        private TelegramValueData[] _TelegramArray;
        [XmlArray("TelegramArray")]
        [XmlArrayItem("TelegramValue")]
        public TelegramValueData[] TelegramArray {
            get => this._TelegramArray; set => this._TelegramArray = value;
        }

        private string _Format;
        [XmlAttribute]
        public string Format {
            get => this._Format; set => this._Format = value;
        }

        public TypeCode Typ => (TypeCode)Enum.Parse(typeof(TypeCode), this._Format);

        public string ShortTypeString {
            get {
                return this.Typ switch {
                    TypeCode.Boolean => "bool",
                    TypeCode.Char => "char",
                    TypeCode.DateTime => "DateTime",
                    TypeCode.Double => "double",
                    TypeCode.Int16 => "short",
                    TypeCode.Int32 => "int",
                    TypeCode.Int64 => "long",
                    TypeCode.Single => "float",
                    TypeCode.Byte => "byte",
                    TypeCode.Empty => throw new NotImplementedException(),
                    TypeCode.Object => throw new NotImplementedException(),
                    TypeCode.DBNull => throw new NotImplementedException(),
                    TypeCode.SByte => throw new NotImplementedException(),
                    TypeCode.UInt16 => throw new NotImplementedException(),
                    TypeCode.UInt32 => throw new NotImplementedException(),
                    TypeCode.UInt64 => throw new NotImplementedException(),
                    TypeCode.Decimal => throw new NotImplementedException(),
                    TypeCode.String => throw new NotImplementedException(),
                    _ => "string",
                };
            }
        }
        private bool _InDatabase;
        [XmlAttribute]
        public bool InDatabase {
            get => this._InDatabase; set => this._InDatabase = value;
        }

        private bool _InObject;
        [XmlAttribute]
        public bool InObject {
            get => this._InObject; set => this._InObject = value;
        }

        private object _Value;
        public object Value {
            get => this._Value; set => this._Value = value;
        }

        private int _BytePosition;
        public int BytePosition {
            get => this._BytePosition; set => this._BytePosition = value;
        }

        private int _AdressNo;
        [XmlAttribute]
        public int AdressNo {
            get => this._AdressNo; set => this._AdressNo = value;
        }

        private float _factor;
        [XmlAttribute]
        public float Factor {
            get => this._factor; set => this._factor = value;
        }
        private string _DefaultValue;
        [XmlAttribute]
        public string DefaultValue {
            get => this._DefaultValue; set => this._DefaultValue = value;
        }
    }

    [Serializable]
    public class TelegramValueList : List<TelegramValueData> {
        public TelegramValueList()
           : base() {
        }

        private TelegramValueList[] _TelegramValueArray;
        public TelegramValueList[] TelegramValueArray {
            get => this._TelegramValueArray; set => this._TelegramValueArray = value;
        }
        private string _Name;
        public TelegramValueData FindIndex(string name, out int idx) {
            this._Name = name;
            var n = this.FindIndex(this.IsData);
            idx = n;
            return this.Count > n ? this[n] : null;
        }

        public bool IsData(TelegramValueData data) {
            if (this.Count > 1) {
                if (data.Name.Trim().Equals(this._Name, StringComparison.Ordinal)) {
                    return true;
                }
            }
            return false;
        }
    }
}
