namespace Level2.PlateLeveler.DataTypes {
    public enum SignalDef {
        L1, L3, FM, HMI, DB, ActiveCassette, CPU
    }
    public class SignalData {
        public SignalDef Signal { get; set; }
        public int Flag { get; set; }
    }

    public class StateData {
        public short DB { get; set; }
        public short L1 { get; set; }
        public short L3 { get; set; }
        public short FM { get; set; }
    }
}
