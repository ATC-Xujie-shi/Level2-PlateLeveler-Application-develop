using System;
using System.Collections.Generic;

namespace Level2.PlateLeveler.DataTypes {
    [Serializable]
    public class LevelerData : PlateData, ILevelerData {
        protected float _LevelerInlet;
        public float LevelerInlet {
            get => this._LevelerInlet; set => this._LevelerInlet = value;
        }

        protected float _LevelerOutlet;
        public float LevelerOutlet {
            get => this._LevelerOutlet; set => this._LevelerOutlet = value;
        }

        protected float _TiltLeft;
        public float TiltLeft {
            get => this._TiltLeft; set => this._TiltLeft = value;
        }

        protected float _TiltRight;
        public float TiltRight {
            get => this._TiltRight; set => this._TiltRight = value;
        }

        protected float _MiddleHeight;
        public float MiddleHeight {
            get => this._MiddleHeight; set => this._MiddleHeight = value;
        }

        protected float _CurveProfile;
        public float CurveProfile {
            get => this._CurveProfile; set => this._CurveProfile = value;
        }

        protected float _Plastification;
        public float Plastification {
            get => this._Plastification; set => this._Plastification = value;
        }

        protected float _Elongation;
        public float Elongation {
            get => this._Elongation; set => this._Elongation = value;
        }

        protected float _Temperature;
        public float Temperature {
            get => this._Temperature; set => this._Temperature = value;
        }

        protected int _NoOfLeveling;
        public int NoOfLeveling {
            get => this._NoOfLeveling; set => this._NoOfLeveling = value;
        }

        public string[] FieldsLS => ["Date", "STATE"];

        public object[] ValuesLS => [DateTime.Now, this._State];
    }

    public class LevelerList : List<LevelerData> {
        public LevelerList()
           : base() {
        }

        public LevelerData GetItem(string plateID) {
            var data = new LevelerData();
            foreach (var item in this) {
                if (item.PlateID.Equals(plateID, StringComparison.Ordinal)) {
                    return item;
                }
            }

            return data;
        }
    }
}
