namespace Level2.PlateLeveler.DataTypes {
    public class ProduktData {
        protected int _ID;
        public int ID {
            get => this._ID; set => this._ID = value;
        }

        protected string _ProduktName;
        public string ProduktName {
            get => this._ProduktName; set => this._ProduktName = value;
        }

        protected bool _Discontinued;
        public bool Discontinued {
            get => this._Discontinued; set => this._Discontinued = value;
        }
    }
}
