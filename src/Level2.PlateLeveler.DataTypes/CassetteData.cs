using System.Collections.Generic;

namespace Level2.PlateLeveler.DataTypes {

    public class CassetteData : ICassetteData {
        public int CassetteNo { get; set; }
        public float RollDiameter { get; set; }
        public float BearingDiameter { get; set; }
        public int NoOfRolls { get; set; }
        public float DistanceA { get; set; }
        public float DistanceB { get; set; }
        public float DistanceC { get; set; }
        public float PitchOfRolls { get; set; }
        public float MaxMotorPower { get; set; }
        public float MaxMotorTorque { get; set; }
        public float MinThicknessPlate { get; set; }
        public float MaxThicknessPlate { get; set; }
        public float MaxAdjustment { get; set; }
        public float MaxCenterHeightBending { get; set; }
        public float MinCenterHeightBending { get; set; }
        public float MaxCrossTiltLeft { get; set; }
        public float MinCrossTiltLeft { get; set; }
        public float MaxCrossTiltRight { get; set; }
        public float MinCrossTiltRight { get; set; }
        public float MaxLevelingPressure { get; set; }

        public string[] Fields {
            get {
                return [ "pID", "pNoOfRolls", "pRollDiameter", "pBearingDiameter", "pPitchOfRolls", "pDistanceA", "pDistanceB", "pDistanceC", "pMaxMotorPower", "pMaxMotorTorque", "pMinThicknessPlate",
                              "pMaxThicknessPlate", "pMaxAdjustment", "pMaxCenterHeightBending", "pMinCenterHeightBending", "pMaxCrossTiltLeft", "pMinCrossTiltLeft", "pMaxCrossTiltRight",  "pMinCrossTiltRight",  "pMaxLevelingPressure" ];
            }
        }

        public object[] Values {
            get {
                return [ this.CassetteNo, this.NoOfRolls, this.RollDiameter, this.BearingDiameter, this.PitchOfRolls, this.DistanceA, this.DistanceB, this.DistanceC, this.MaxMotorPower, this.MaxMotorTorque, this.MinThicknessPlate, this.MaxThicknessPlate,
                                 this.MaxAdjustment, this.MaxCenterHeightBending, this.MinCenterHeightBending, this.MaxCrossTiltLeft, this.MinCrossTiltLeft, this.MaxCrossTiltRight, this.MinCrossTiltRight, this.MaxLevelingPressure ];
            }
        }

        public string[] FieldsActive => ["CassetteNo"];

        public object[] ValuesActive => [this.CassetteNo];
    }

    public sealed class CassetteList : List<ICassetteData> {
        public CassetteList()
           : base() {
        }

        public ICassetteData GetItem(int id) {
            var data = new CassetteData();
            foreach (var item in this) {
                if (item.CassetteNo.Equals(id)) {
                    return item;
                }
            }

            return data;
        }
    }
}
