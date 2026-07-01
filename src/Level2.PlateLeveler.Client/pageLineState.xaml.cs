using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.Client {
    /// <summary>
    /// Interaktionslogik für pageLineState.xaml
    /// </summary>
    public partial class pageLineState : Page, IDisposable {
        private LEVELEREntities entity;
        private readonly CollectionViewSource _LineStateViewSource;
        private readonly InitData _Initialization;
        private int _State;
        private DateTime _From, _To;

        public pageLineState(InitData init) {
            this.InitializeComponent();
            this._Initialization = init;
            this._From = DateTime.Now;
            this._To = DateTime.Now;
            this._State = 100;
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            this._LineStateViewSource = (CollectionViewSource)this.FindResource("LineStateDataViewSource");
            this.SetGridItems();
        }

        private void dtFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
            this._From = (DateTime)e.AddedItems[0];
            this.SetGridItems();
        }

        private void dtTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
            this._To = (DateTime)e.AddedItems[0];
            this.SetGridItems();
        }

        private void cboState_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = (ComboBoxItem)this.cboState.Items[this.cboState.SelectedIndex];
            this._State = Convert.ToInt32(item.Tag);
            this.SetGridItems();
        }

        private void SetGridItems() {
            try {
                this.entity?.Dispose();

                this.entity = new LEVELEREntities();
                var prodReport = new List<LineState>();
                prodReport = this._State < 100
                    ? [.. this.entity.LineStates.Where(p => p.State == this._State && p.Date >= this._From && p.Date <= this._To)]
                    : [.. this.entity.LineStates.Where(p => p.Date >= this._From && p.Date <= this._To)];

                this.btnLineState.IsEnabled = this.entity.LineStates.Local.Any();
                this._LineStateViewSource.Source = prodReport;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }

        private PrintMode _PrintMode;

        private void rbPreview_Checked(object sender, RoutedEventArgs e) => this._PrintMode = PrintMode.Preview;

        private void rbPrint_Checked(object sender, RoutedEventArgs e) => this._PrintMode = PrintMode.Normal;

        private void rbExport_Checked(object sender, RoutedEventArgs e) => this._PrintMode = PrintMode.Export;

        private void btnReport_Click(object sender, RoutedEventArgs e) {
            try {
                var _ReportController = new ReportController(this._Initialization);
                var data = new LineStateData {
                    From = this._From,
                    To = this._To
                };

                if (this.cboState.SelectedItem != null) {
                    var state = LineStateDef.Nothing;
                    if (this.cboState.SelectedItem == this.cbItem0) {
                        state = LineStateDef.Nothing;
                    } else if (this.cboState.SelectedItem == this.cbItem1) {
                        state = LineStateDef.Line_Off;
                    } else if (this.cboState.SelectedItem == this.cbItem2) {
                        state = LineStateDef.Manual_Mode;
                    } else if (this.cboState.SelectedItem == this.cbItem3) {
                        state = LineStateDef.Automatic_Mode;
                    } else if (this.cboState.SelectedItem == this.cbItem4) {
                        state = LineStateDef.In_Production;
                    } else if (this.cboState.SelectedItem == this.cbItem5) {
                        state = LineStateDef.Reversing;
                    } else if (this.cboState.SelectedItem == this.cbItem6) {
                        state = LineStateDef.Fault_1;
                    } else if (this.cboState.SelectedItem == this.cbItem7) {
                        state = LineStateDef.Fault_2;
                    } else if (this.cboState.SelectedItem == this.cbItem7) {
                        state = LineStateDef.Fault_3;
                    }

                    if (state != LineStateDef.Nothing) {
                        var SelectedLineState = this.entity.LineStates.Where(x => x.State == (int)state && x.Date >= this._From && x.Date <= this._To).ToList();
                        this._LineStateViewSource.Source = SelectedLineState;
                    }

                    //List<LineState_VW> list = (List<LineState_VW>)_LineStateViewSource.Source;
                    //List<LineStateData> lst = list.GroupBy(l => l.LineStates).Select(group => new LineStateData
                    //{
                    //   State = group.Key,
                    //   Hours = group.Sum(g => ((TimeSpan)(g.EndDate - g.StartDate)).TotalHours)
                    //}).ToList();
                    //LineStateData ls1 = lst.Single(l => l.State == LineStateDef.In_Production);
                    //LineStateData ls2 = lst.Single(l => l.State == LineStateDef.Automatic);
                    //LineStateData ls = new LineStateData();
                    //ls.State = LineStateDef.Nothing;
                    //ls.To = ls2.To;
                    //ls.From = ls2.From;
                    //ls.Hours = ls2.Hours;
                    //lst.Add(ls);
                    //ls2.To = ls.To + ls1.Time;
                    //ls2.Hours += ls1.Hours;

                    if (Keyboard.IsKeyDown(Key.LeftCtrl)) {
                        _ReportController.LoadReport("", this._LineStateViewSource.Source, data, ReportNameDef.LineState, this._Initialization.Report.Type);
                    } else {
                        _ReportController.LoadReport("", this._LineStateViewSource.Source, data, ReportNameDef.LineState, this._Initialization.Report.Type, this._PrintMode);
                    }
                } else {
                    data.State = LineStateDef.Nothing;
                    if (Keyboard.IsKeyDown(Key.LeftCtrl)) {
                        _ReportController.LoadReport("", this._LineStateViewSource.Source, data, ReportNameDef.LineState, this._Initialization.Report.Type);
                    } else {
                        _ReportController.LoadReport("", this._LineStateViewSource.Source, data, ReportNameDef.LineState, this._Initialization.Report.Type, this._PrintMode);
                    }

                    if (this._PrintMode == PrintMode.Export) {
                        _ = MessageBox.Show("Report " + ReportNameDef.LineState.ToString() + ".pdf is printed in path: " + this._Initialization.Report.PdfPath, "Report Print");
                    }
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e) => this.SetGridItems();

        public void Dispose() => throw new NotImplementedException();
    }
}
