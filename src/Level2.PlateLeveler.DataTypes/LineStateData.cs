using System;
using System.Collections.Generic;

namespace Level2.PlateLeveler.DataTypes {
    public enum LineStateDef {
        Line_Off = 0,
        Manual_Mode = 1,
        Automatic_Mode = 2,
        In_Production = 3,
        Reversing = 4,
        Fault_1 = 30,
        Fault_2 = 31,
        Fault_3 = 32,
        Nothing = 100
    }

    public class LineStateData {
        public LineStateDef State { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }

        public TimeSpan Time => this.To - this.From;

        public double Hours { get; set; }
    }

    public class LineStateList : List<LineStateData> {
        public LineStateList()
           : base() {
        }
    }
}
