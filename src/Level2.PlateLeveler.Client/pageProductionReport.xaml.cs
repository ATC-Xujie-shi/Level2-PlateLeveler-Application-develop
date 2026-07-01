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
    /// Interaktionslogik für pageProductionReport.xaml
    /// </summary>
    public partial class pageProductionReport : Page, IDisposable {
        private LEVELEREntities entity;
        private readonly CollectionViewSource _ProductionReportControlViewSource;
        private CollectionViewSource _ProductionReportViewSource;
        private List<ProductionReport> _ProductionReportList;
        private readonly InitData _Initialization;
        private DateTime _From, _To;
        private readonly PDI _PDI;
        private ProductionReport _ProdReport;
        private readonly Flatness _Flatness;

        public pageProductionReport(InitData init) {
            this.InitializeComponent();
            this._Initialization = init;
            this._From = DateTime.Now;
            this._To = DateTime.Now;
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            this._ProductionReportControlViewSource = (CollectionViewSource)this.FindResource("ProductionReportControlDataViewSource");
            this._PDI = new PDI();
            this._ProdReport = new ProductionReport();
            this._Flatness = new Flatness();
        }

        private void dtFrom_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
            this._From = (DateTime)e.AddedItems[0];
            this.SetGridItems();
        }

        private void dtTo_SelectedDateChanged(object sender, SelectionChangedEventArgs e) {
            this._To = (DateTime)e.AddedItems[0];
            this.SetGridItems();
        }

        private void SetGridItems() {
            try {
                this.entity?.Dispose();

                this.entity = new LEVELEREntities();
                this._ProductionReportViewSource = (CollectionViewSource)this.FindResource("ProductionReportDataViewSource");
                var prodReport = this.entity.ProductionReports.Where(a => a.ProdDate >= this._From && a.ProdDate <= this._To).ToList();
                this._ProductionReportList = prodReport;
                this._ProductionReportViewSource.Source = this._ProductionReportList;
                this.btnReport.IsEnabled = this._ProductionReportList.Count > 0;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnReport_Click(object sender, RoutedEventArgs e) {
            try {
                var _ReportController = new ReportController(this._Initialization);
                var data = new VariableData {
                    From = this._From,
                    To = this._To
                };
                if (Keyboard.IsKeyUp(Key.LeftCtrl)) {
                    _ReportController.LoadReport(data.PlateID, this._ProductionReportViewSource.Source, data, ReportNameDef.ProductionReport, this._Initialization.Report.Type, this._PrintMode);
                } else {
                    _ReportController.LoadReport(data.PlateID, this._ProductionReportViewSource.Source, data, ReportNameDef.ProductionReport, this._Initialization.Report.Type);
                }

                if (this._PrintMode == PrintMode.Export) {
                    _ = MessageBox.Show("Report " + ReportNameDef.ProductionReport.ToString() + "_" + DateTime.Now.Day + "_"
                          + DateTime.Now.Month + "_" + DateTime.Now.Year + "__" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second + ".pdf is printed in path: " + this._Initialization.Report.PdfPath, "Report Print");
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private PrintMode _PrintMode;
        private void rbPreview_Checked(object sender, RoutedEventArgs e) => this._PrintMode = PrintMode.Preview;

        private void rbPrint_Checked(object sender, RoutedEventArgs e) => this._PrintMode = PrintMode.Normal;

        private void rbExport_Checked(object sender, RoutedEventArgs e) => this._PrintMode = PrintMode.Export;

        private void gridProductionReport_SelectedItemsChanged(object sender, Infragistics.Windows.DataPresenter.Events.SelectedItemsChangedEventArgs e) {
            try {
                this.btnPlateReport.IsEnabled = this.gridProductionReport.ActiveDataItem != null;

                if (this.gridProductionReport.ActiveDataItem != null) {
                    this._ProdReport = (ProductionReport)this.gridProductionReport.ActiveDataItem;
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnPlateReport_Click(object sender, RoutedEventArgs e) {
            try {
                var _ReportController = new ReportController(this._Initialization);
                var data = new VariableData {
                    From = this._From,
                    To = this._To
                };

                if (this.gridProductionReport.ActiveDataItem != null) {
                    this._ProdReport = (ProductionReport)this.gridProductionReport.ActiveDataItem;
                }

                if (this._ProdReport != null) {
                    var _PlateReportViewSource = new CollectionViewSource();
                    var prodReport = this.entity.ProductionReports.Where(a => a.PKEY_PROD == this._ProdReport.PKEY_PROD).ToList();
                    _PlateReportViewSource.Source = prodReport;

                    data.StartDate = prodReport.First().ProdDate + "";
                    data.PlateID = prodReport.First().PlateID;
                    data.MaterialID = prodReport.First().MaterialID;
                    if (prodReport.First().ProdDate != null) {
                        data.From = prodReport.First().ProdDate.Value;
                    }

                    if (Keyboard.IsKeyUp(Key.LeftCtrl)) {
                        _ReportController.LoadReport(data.PlateID, _PlateReportViewSource.Source, data, ReportNameDef.PlateReport, this._Initialization.Report.Type, this._PrintMode);
                    } else {
                        _ReportController.LoadReport(data.PlateID, _PlateReportViewSource.Source, data, ReportNameDef.PlateReport, this._Initialization.Report.Type);
                    }

                    if (this._PrintMode == PrintMode.Export) {
                        _ = MessageBox.Show("Report " + ReportNameDef.PlateReport.ToString() + "_" + data.PlateID + ".pdf is printed in path: "
                           + this._Initialization.Report.PdfPath, "Report Print");
                    }
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
            //ReportController _ReportController = new ReportController(_Initialization);
            //entity = new LVL5Entities();
            //try
            //{
            //   try
            //   {
            //      _PDI = entity.PDIs.Single(p => p.PlateID == _ProdReport.PlateID);
            //      _PDI.MaterialID = _ProdReport.MaterialID;
            //   }
            //   catch { }
            //   try
            //   {
            //      _Flatness = entity.Flatness.Single(f => f.PlateID == _ProdReport.PlateID && f.MaterialID == _ProdReport.MaterialID);
            //   }
            //   catch { }
            //   try
            //   {
            //      _ProdReport = entity.ProductionReports.Single(f => f.PlateID == _ProdReport.PlateID);
            //   }
            //   catch { }
            //   _ReportController.LoadReport<PDI>(entity.PDIs.Local, _PDI, ReportNameDef.PDI, ProjectType.crd, PrintMode.Export, _Initialization.Report.ExportPath);
            //   _ReportController.LoadReport<PDI>(entity.ProductionReports.Local, _PDI, ReportNameDef.ProductionReport, ProjectType.crd, PrintMode.Export, _Initialization.Report.ExportPath);
            //   _ReportController.LoadReport<PDI>(entity.Flatness.Local, _PDI, ReportNameDef.Flatness, ProjectType.crd, PrintMode.Export, _Initialization.Report.ExportPath);

            //   List<string> listFiles = new List<string>();
            //   listFiles.Add(_Initialization.Report.ExportPath + @"\" + ReportNameDef.PDIs.ToString());
            //   listFiles.Add(_Initialization.Report.ExportPath + @"\" + ReportNameDef.ProductionReports.ToString());
            //   listFiles.Add(_Initialization.Report.ExportPath + @"\" + ReportNameDef.Flatness.ToString());
            //   string report = _ReportController.CompactPdfFiles(listFiles, _Initialization.Report.PdfPath, _PDI);

            //   if (report != null)
            //      System.Windows.MessageBox.Show("Report " + report + " is printed in path: " + _Initialization.Report.PdfPath, "Report Print");

            //   SetGridItems();
            //}
            //catch (Exception ex)
            //{
            //   Logging.SendErrorMessage(System.Reflection.MethodInfo.GetCurrentMethod().Name, ex, this.GetType());
            //}
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
