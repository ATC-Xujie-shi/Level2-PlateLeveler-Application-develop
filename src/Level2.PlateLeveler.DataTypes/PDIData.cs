using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Level2.PlateLeveler.DataTypes {
    [Serializable]
    public class PlateData : IPlateData {
        protected long _ID;
        public long Pkey_PDI {
            get => this._ID; set => this._ID = value;
        }

        protected string _PlateID;
        [TelegramDefinition("Telegramblock")]
        public string PlateID {
            get => this._PlateID; set => this._PlateID = value;
        }

        public long ID => this._ID;

        protected string _MaterialID;
        [TelegramDefinition("Telegramblock")]
        public string MaterialID {
            get => this._MaterialID; set => this._MaterialID = value;
        }

        protected string _SteelGrade;
        public string SteelGrade {
            get => this._SteelGrade; set => this._SteelGrade = value;
        }

        protected float? _Length;
        [TelegramDefinition("Telegramblock")]
        public float? Length {
            get => this._Length; set => this._Length = value;
        }

        protected float? _Width;
        [TelegramDefinition("Telegramblock")]
        public float? Width {
            get => this._Width; set => this._Width = value;
        }

        protected float? _Thickness;
        [TelegramDefinition("Telegramblock")]
        public float? Thickness {
            get => this._Thickness; set => this._Thickness = value;
        }

        protected float? _YieldPoint;
        [TelegramDefinition("Telegramblock")]
        public float? YieldPoint {
            get => this._YieldPoint; set => this._YieldPoint = value;
        }

        protected float? _TensileStrength;
        public float? TensileStrength {
            get => this._TensileStrength; set => this._TensileStrength = value;
        }

        protected float? _EModule;
        public float? EModule {
            get => this._EModule; set => this._EModule = value;
        }

        protected short? _MeasuringCode;
        public short? MeasuringCode {
            get => this._MeasuringCode; set => this._MeasuringCode = value;
        }

        protected float? _Ruler1MCrossPDI;
        public float? Ruler1MCrossPDI {
            get => this._Ruler1MCrossPDI; set => this._Ruler1MCrossPDI = value;
        }

        protected float? _Ruler1MLengthPDI;
        public float? Ruler1MLengthPDI {
            get => this._Ruler1MLengthPDI; set => this._Ruler1MLengthPDI = value;
        }

        protected float? _Ruler2MLengthPDI;
        public float? Ruler2MLengthPDI {
            get => this._Ruler2MLengthPDI; set => this._Ruler2MLengthPDI = value;
        }

        protected short? _Leveling;
        public short? Leveling {
            get => this._Leveling; set => this._Leveling = value;
        }

        protected float? _GapLengthBow;
        public float? GapLengthBow {
            get => this._GapLengthBow; set => this._GapLengthBow = value;
        }

        protected float? _GapCrossBow;
        public float? GapCrossBow {
            get => this._GapCrossBow; set => this._GapCrossBow = value;
        }

        protected short? _Manual;
        public short? Manual {
            get => this._Manual; set => this._Manual = value;
        }

        protected short? _Location;
        public short? Location {
            get => this._Location; set => this._Location = value;
        }

        protected short _State;
        [XmlAttribute("State")]
        public short State {
            get => this._State; set => this._State = value;
        }
    }

    public class PDIList : List<PlateData> {
        public PDIList()
           : base() {
        }

        public PlateData GetItem(string plateID) {
            foreach (var item in this) {
                if (item.PlateID.Equals(plateID, StringComparison.Ordinal)) {
                    return item;
                }
            }

            return null;
        }
    }

    public class TrackingData : PlateData {
        public string Date {
            get {
                var sb = new StringBuilder(DateTime.Today.Year.ToString());
                _ = sb.Append('.');
                _ = sb.Append(DateTime.Today.Month.ToString("00"));
                _ = sb.Append('.');
                _ = sb.Append(DateTime.Today.Day.ToString("00"));
                return sb.ToString();
            }
        }
        public string Time {
            get {
                var sb = new StringBuilder(DateTime.Now.Hour.ToString("00"));
                _ = sb.Append(':');
                _ = sb.Append(DateTime.Now.Minute.ToString("00"));
                _ = sb.Append(':');
                _ = sb.Append(DateTime.Now.Second.ToString("00"));
                return sb.ToString();
            }
        }
    }

    public class AdjustmentData : PlateData {
        public AdjustmentData() {
        }

        public AdjustmentData(PDI plate) {
            this._PlateID = plate.PlateID;
            this._SteelGrade = plate.SteelGrade;
            this._EModule = plate.EModule;
            this._YieldPoint = plate.YieldPoint;
            this._MaterialID = plate.MaterialID;
            this._Thickness = plate.Thickness;
            this._Width = plate.Width;
            this._Length = plate.Length;
        }

        public short CassetteNo { get; set; }
        public float Temperature { get; set; }
        public float Plastification { get; set; }
    }

    public class AdjustmentList : List<AdjustmentData> {
        public AdjustmentList()
           : base() {
        }

        public AdjustmentData GetAdjustmentFromList(AdjustmentData item) {
            var result = item;
            if (this.Count > 0) {
                if (item.Temperature.Equals(this[this.Count - 1].Temperature)) {
                    result.EModule = this[this.Count - 1].EModule;
                    result.YieldPoint = this[this.Count - 1].YieldPoint;
                } else {
                    for (var n = 0; n < this.Count - 1; n++) {
                        if (item.SteelGrade.Equals(this[n].SteelGrade, StringComparison.Ordinal)) {
                            if (item.Temperature.Equals(this[n].Temperature)) {
                                result.EModule = this[n].EModule;
                                result.YieldPoint = this[n].YieldPoint;
                            } else if (item.Temperature > this[n].Temperature && item.Temperature < this[n + 1].Temperature) {
                                if (item.Temperature - this[n].Temperature < this[n + 1].Temperature - item.Temperature) {
                                    result.EModule = this[n].EModule;
                                    result.YieldPoint = this[n].YieldPoint;
                                } else {
                                    result.EModule = this[n - 1].EModule;
                                    result.YieldPoint = this[n - 1].YieldPoint;
                                }
                            }
                            break;
                        }
                    }
                }
            }

            return result;
        }
    }
}
