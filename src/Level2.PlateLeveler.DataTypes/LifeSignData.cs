namespace Level2.PlateLeveler.DataTypes {
    public enum ConnectionType {
        PLC, Flatness, Database
    }
    public class LifeSignData : HeaderData {
        public short StateFlatness { get; set; }
        public short StateDB { get; set; }
        public short StateS7 { get; set; }
    }
}
