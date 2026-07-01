using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace Level2.PlateLeveler.Client {
    public enum ReportNameDef {
        ProductionReport, LineState, PDI, Flatness, ActualValues, PlateReport
    }
    public class ReportController : IDisposable {
        private readonly InitData _Initialization;
        private Report _Report;

        public ReportController(InitData init) {
            this._Initialization = init;
            if (this._Initialization.Report.Path.Contains(".")) {
                this._Initialization.Report.Path = this._Initialization.Report.Path.Replace(".", Functions.App_Path());
            }

            Functions.CreatePath(this._Initialization.Report.Path);
            this._Report = new Report(init.Report, ProjectType.lst);
        }

        public void LoadReport<T>(string SelectedPlate, object data, T variables, ReportNameDef def, ProjectType PjType, PrintMode? mode = null, string path = null) {
            var dataReport = this._Initialization.Report;
            dataReport.File = def.ToString();
            var Path = path ?? this._Initialization.Report.PdfPath;
            try {
                dataReport.DataSource = data;

                this._Report = new Report(dataReport, PjType);
                this._Report.AddVariableFromObject(variables.GetType().Name, variables);

                if (SelectedPlate == null) {
                    if (mode.HasValue) {
                        _ = mode.Value == PrintMode.Export
                            ? this._Report.PrintPDF("" + def.ToString() + "_" + DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year + "__" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second, Path)
                            : this._Report.Print(mode.Value);
                    } else {
                        this._Report.Print("" + def.ToString() + "_" + DateTime.Now.Day + "_" + DateTime.Now.Month + "_" + DateTime.Now.Year + "__" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second);
                    }
                } else {
                    if (mode.HasValue) {
                        _ = mode.Value == PrintMode.Export
                            ? this._Report.PrintPDF("" + def.ToString() + "_" + SelectedPlate, Path)
                            : this._Report.Print(mode.Value);
                    } else {
                        this._Report.Print("" + def.ToString() + "_" + DateTime.Now.Day + "_" + SelectedPlate);
                    }
                }
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }

        public string CompactPdfFiles(List<string> pdfFiles, string path, PDI pdi) {
            var doc = new PdfDocument();
            try {
                doc.PageLayout = PdfPageLayout.TwoPageLeft;
                var cnt = 0;
                foreach (var file in pdfFiles) {
                    var inputDoc = PdfReader.Open(file + ".pdf", PdfDocumentOpenMode.Import);

                    if (cnt.Equals(0) && inputDoc.Pages.Count > 0) {
                        _ = doc.AddPage(inputDoc.Pages[0]);
                    } else {
                        foreach (var page in inputDoc.Pages) {
                            _ = doc.AddPage(page);
                        }
                    }

                    cnt++;
                }
                cnt = 0;
                var fileName = path + @"\PlateResult_" + pdi.PlateID + "_" + pdi.MaterialID + "_" + DateTime.Now.ToString("yyyyMMdd") + "_" + DateTime.Now.ToString("hhmmss") + ".pdf";
                doc.Save(fileName);
                var info = new FileInfo(fileName);
                path = fileName;
                return fileName;
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return null;
            }
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
