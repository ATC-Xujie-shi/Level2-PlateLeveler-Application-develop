using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.Client {
    /// <summary>
    /// Interaktionslogik für pageActualValues.xaml
    /// </summary>
    public partial class pageActualValues : Page, IDisposable {
        private LEVELEREntities entity;
        private CollectionViewSource _ActualValuesViewSource;
        private readonly CollectionViewSource _PDIViewSource;
        private readonly InitData _Initialization;
        private DateTime _From, _To;
        private ActualValue _ActualValue;
        private PrintMode _PrintMode;

        public pageActualValues(InitData init) {
            this.InitializeComponent();
            this._Initialization = init;
            this._From = DateTime.Now;
            this._To = DateTime.Now;
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            this._ActualValuesViewSource = (CollectionViewSource)this.FindResource("ActualValuesDataViewSource");
            this._PDIViewSource = (CollectionViewSource)this.FindResource("PDIDataViewSource");
        }

        private void dtFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
            this._From = (DateTime)e.AddedItems[0];
            this.SetGridItems();
        }

        private void dtTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
            this._To = (DateTime)e.AddedItems[0];
            this.SetGridItems();
        }

        private void gridActualValues_SelectedItemsChanged(object sender, Infragistics.Windows.DataPresenter.Events.SelectedItemsChangedEventArgs e) {
            this.btnReport.IsEnabled = this.gridActualValues.ActiveDataItem != null;

            if (this.gridActualValues.ActiveDataItem != null) {
                this._ActualValue = (ActualValue)this.gridActualValues.ActiveDataItem;
            }
        }

        private void btnReport_Click(object sender, RoutedEventArgs e) {
            try {
                var bKeyDown = Keyboard.IsKeyDown(Key.LeftCtrl) | Keyboard.IsKeyDown(Key.RightCtrl);
                var _ReportController = new ReportController(this._Initialization);
                var data = new VariableData {
                    From = this._From,
                    To = this._To,
                    PlateID = this.cboReport.SelectedValue.ToString()
                };
                var selectedPdi = this.entity.PDIs.FirstOrDefault(p => p.PlateID == data.PlateID);
                if (selectedPdi != null) {
                    data.Width = selectedPdi.Width;
                    data.YieldPoint = selectedPdi.YieldPoint;
                    data.Thickness = selectedPdi.Thickness;

                    var activeCassette = this.entity.Cassettes.FirstOrDefault(c => c.Active == 1);
                    var actValues = this.entity.ActualValues.Where(a => a.PlateID == this.cboReport.SelectedValue.ToString()).ToList();

                    data.StartDate = actValues.First().Date + "";
                    data.LastDate = actValues.Last().Date + "";

                    if (bKeyDown) {
                        _ReportController.LoadReport(data.PlateID, actValues, data, ReportNameDef.ActualValues, this._Initialization.Report.Type);
                    } else {
                        _ReportController.LoadReport(data.PlateID, actValues, data, ReportNameDef.ActualValues, this._Initialization.Report.Type, this._PrintMode);
                    }

                    if (this._PrintMode == PrintMode.Export) {
                        _ = MessageBox.Show("Report " + ReportNameDef.ActualValues.ToString() + "_" + data.PlateID + ".pdf is printed in path: " + this._Initialization.Report.PdfPath, "Report Print");
                    }
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void SetGridItems() {
            try {
                this.entity?.Dispose();

                this.entity = new LEVELEREntities();
                this._ActualValuesViewSource = (CollectionViewSource)this.FindResource("ActualValuesDataViewSource");
                var prodReport = this.entity.ActualValues.Where(a => a.Date >= this._From && a.Date <= this._To && a.PlateID != null && a.PlateID != "").ToList();

                var plates = prodReport.GroupBy(p => p.PlateID).Select(pl => pl.Key).ToList().Where(t => !string.IsNullOrEmpty(t)).ToList();
                plates.Sort();
                foreach (var item in plates) {
                    _ = this.cboReport.Items.Add(item);
                }
                //List<PDI> pdis =  entity.PDIs.ToList().FindAll(delegate(PDI p) { return plates.Contains(p.PlateID); });
                var val = new ActualValue();
                var n = 0;
                foreach (var item in this.entity.ActualValues.Local) {
                    if (item.PlateID != val.PlateID | item.MaterialID != val.MaterialID) {
                        n = 0;
                    }

                    n++;
                    item.Pos = n;
                    val = item;
                }
                _ = this.entity.SaveChanges();
                this._ActualValuesViewSource.Source = this.entity.ActualValues.Local;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void rbPreview_Checked(object sender, RoutedEventArgs e) => this._PrintMode = PrintMode.Preview;

        private void rbPrint_Checked(object sender, RoutedEventArgs e) => this._PrintMode = PrintMode.Normal;

        private void rbExport_Checked(object sender, RoutedEventArgs e) => this._PrintMode = PrintMode.Export;

        private void cboReport_SelectionChanged(object sender, SelectionChangedEventArgs e) => this.btnReport.IsEnabled = true;

        public void Dispose() => throw new NotImplementedException();
    }
}
