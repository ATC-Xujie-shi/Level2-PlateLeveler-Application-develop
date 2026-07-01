using System;
using System.Globalization;

namespace Level2.PlateLeveler.DataTypes {
    public class CultureChangeEventArgs : EventArgs {
        public CultureChangeEventArgs()
           : base() {
        }
        public CultureChangeEventArgs(CultureInfo info) {
            this.LangInfo = info;
        }

        public CultureInfo LangInfo { get; set; }
    }
}
