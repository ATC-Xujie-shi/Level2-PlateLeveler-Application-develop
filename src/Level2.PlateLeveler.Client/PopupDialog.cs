using System;
using System.Windows.Forms;

namespace Level2.PlateLeveler.Client {
    public partial class PopupDialog : Form {
        public PopupDialog() {
            this.InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e) => this.Close();

        private void popupTimer_Tick(object sender, EventArgs e) => this.Close();
    }
}
