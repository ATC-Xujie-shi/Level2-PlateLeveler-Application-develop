using System.Collections.Generic;

namespace Level2.PlateLeveler.DataTypes {
    public class SettingsData : LevelerData, ISettingsData {
        public SettingsData() {
        }
        public void SetSettingsData(IPreSetting ps, IPDI pdi) {
            this.CassetteNo = ps.CassetteNo ?? 0;
            this.EndRangeThickness = ps.EndRangeThickness ?? 0;
            this.EndRangeWidth = ps.EndRangeWidth ?? 0;
            this.StartRangeThickness = ps.StartRangeThickness ?? 0;
            this.StartRangeWidth = ps.StartRangeWidth ?? 0;
            this._TiltLeft = ps.TiltingLeft ?? 0;
            this._TiltRight = ps.TiltingRight ?? 0;
            this._MiddleHeight = ps.CenterHeightBending ?? 0;
            this._CurveProfile = ps.CurveProfile ?? 0;
            this._Plastification = ps.CorrPlastification ?? 0;
            this._MaterialID = ps.MaterialID;
            this._PlateID = pdi.PlateID;
            this._Thickness = pdi.Thickness.HasValue ? pdi.Thickness : 0;
            this._Width = pdi.Width.HasValue ? pdi.Width : 0;
            this._YieldPoint = pdi.YieldPoint.HasValue ? pdi.YieldPoint : 0;
            this._TensileStrength = pdi.TensileStrength.HasValue ? pdi.TensileStrength : 0;
            this._Ruler1MCrossPDI = pdi.Ruler1MCrossPDI.HasValue ? pdi.Ruler1MCrossPDI : 0;
            this._Ruler1MLengthPDI = pdi.Ruler1MLengthPDI.HasValue ? pdi.Ruler1MLengthPDI : 0;
            this._Ruler2MLengthPDI = pdi.Ruler2MLengthPDI.HasValue ? pdi.Ruler2MLengthPDI : 0;
            this._SteelGrade = pdi.SteelGrade;
            this._EModule = pdi.EModule.HasValue ? pdi.EModule : 0;
        }

        public SettingsData(AdjustmentData adjustment) {
            this._EModule = adjustment.EModule;
            this._Leveling = adjustment.Leveling;
            this._Location = adjustment.Location;
            this._MaterialID = adjustment.MaterialID;
            this._PlateID = adjustment.PlateID;
            this._Length = adjustment.Length;
            this._Plastification = adjustment.Plastification;
            this._Thickness = adjustment.Thickness;
            this._Width = adjustment.Width;
            this._YieldPoint = adjustment.YieldPoint;
        }

        public int CassetteNo { get; set; }
        public bool Enable { get; set; }
        public float StartRangeThickness { get; set; }
        public float EndRangeThickness { get; set; }
        public float StartRangeWidth { get; set; }
        public float EndRangeWidth { get; set; }
        public float LevelingPressure { get; set; }
        public float MotorPower { get; set; }
        public float MotorTorque { get; set; }
    }

    public class OutletSettingsData {
        public long ID { get; set; }
        public float StartRangeThickness { get; set; }
        public float EndRangeThickness { get; set; }
        public float StartRangeYieldPoint { get; set; }
        public float EndRangeYieldPoint { get; set; }
        public float Offset { get; set; }
    }

    public class OutletSettingsList : List<OutletSettingsData> {
        public OutletSettingsList()
           : base() {
        }

        public OutletSettingsData GetOutletByPlate(PDI plate) {
            if (plate != null) {
                foreach (var item in this) {
                    if (plate.Thickness >= item.StartRangeThickness && plate.Thickness <= item.EndRangeThickness &&
                       plate.YieldPoint >= item.StartRangeYieldPoint && plate.YieldPoint <= item.EndRangeYieldPoint) {
                        return item;
                    }
                }
            }
            return new OutletSettingsData();
        }
    }
}
