using System;

namespace Level2.PlateLeveler.DataTypes {
    public class RequestPDIEventArgs(object plateID, int location) : EventArgs {
        public object PlateID { get; set; } = plateID;
        public int Location { get; set; } = location;

    }
}