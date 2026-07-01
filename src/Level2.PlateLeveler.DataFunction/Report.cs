using System;
using combit.ListLabel18;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.DataFunction {
    public enum MasterMode {
        Fields, Variables
    }

    public enum PrintMode {
        Normal, Preview, Export
    }

    public class Report : IDisposable {
        public ListLabel LL;
        private ReportData _Report;
        public Report(ReportData data) {
            this.setReport(data, MasterMode.Fields, ProjectType.lst);
        }

        public Report(ReportData data, MasterMode masterMode) {
            this.setReport(data, masterMode, ProjectType.lst);
        }

        public Report(ReportData data, string mode) {
            var masterMode = (MasterMode)Enum.Parse(typeof(MasterMode), mode);
            this.setReport(data, masterMode, ProjectType.lst);
        }

        public Report(ReportData data, ProjectType project) {
            this.setReport(data, MasterMode.Fields, project);
        }

        public Report(ReportData data, MasterMode masterMode, ProjectType project) {
            this.setReport(data, masterMode, project);
        }

        [STAThread]
        private void setReport(ReportData data, MasterMode masterMode, ProjectType project) {
            this.LL = new ListLabel(LlLanguage.English, false) {
                LicensingInfo = data.License,
                AutoMasterMode = (LlAutoMasterMode)Enum.Parse(typeof(LlAutoMasterMode), "As" + masterMode.ToString()),
                DataSource = data.DataSource
            };
            this.LL.PrintJobInfo += new PrintJobInfoHandler(this.LL_PrintJobInfo);
            //combit.ListLabel18.LlPrintOption.Page
            try {
                this.Project = project switch {
                    ProjectType.lst => LlProject.List,
                    ProjectType.crd => LlProject.Card,
                    ProjectType.lbl => throw new NotImplementedException(),
                    _ => LlProject.Label,
                };
                this._Report = data;

                this.LL.AutoProjectFile = this._Report.Path + @"\" + this._Report.File + "." + project.ToString();
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }

        private void LL_PrintJobInfo(object sender, PrintJobInfoEventArgs e) {

        }

        public void AddVariable(string field, object val) => this.LL.Variables.Add(field, val);

        public void AddVariableFromObject(string field, object val) => this.LL.Variables.AddFromObject(field, val);

        public LlProject Project { get; set; }

        private void SetExportOptions(ExportData export) {
            if (this.LL.ExportOptions.Count > 0) {
                this.LL.ExportOptions.Clear();
            }

            this.LL.ExportOptions.Add(LlExportOption.ExportFile, export.File + "." + export.Type.ToString());
            this.LL.ExportOptions.Add(LlExportOption.ExportTarget, export.Type.ToString().ToUpper(System.Globalization.CultureInfo.CurrentCulture));
            this.LL.ExportOptions.Add(LlExportOption.ExportPath, export.Path);
            this.LL.ExportOptions.Add(LlExportOption.ExportAllInOneFile, "1");
            this.LL.ExportOptions.Add(LlExportOption.ExportShowResult, "0");
            this.LL.ExportOptions.Add(LlExportOption.ExportQuiet, "1");
            this.LL.ExportOptions.Add(LlExportOption.ExportShowResultAvailable, "0");
        }

        private void ClearExportOptions() {
            try {
                if (this.LL.ExportOptions.Count > 0) {
                    this.LL.ExportOptions.Clear();
                }
            } catch (Exception ex) {
                _ = ex.Message;
            }
        }

        public bool Export(ExportData export) {
            try {
                this.ClearExportOptions();
                this.SetExportOptions(export);
                var bPrint = this.Print(PrintMode.Export);
                // Juergens 12.01.2013
                //LL.Dispose();
                return bPrint;
            } catch (Exception e) {
                //fs.Close();
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return false;
            }
        }

        public bool PrintPDF(string file, string path) {
            var export = new ExportData {
                Path = path,
                Type = ExportType.pdf,
                File = file
            };
            this.SetExportOptions(export);
            return this.Print(PrintMode.Export);
        }

        public bool Print(PrintMode mode) {
            var printMode = mode switch {
                PrintMode.Export => LlPrintMode.Export,
                PrintMode.Normal => LlPrintMode.Normal,
                PrintMode.Preview => LlPrintMode.Preview,
                _ => LlPrintMode.MultipleJobs,
            };
            try {
                if (printMode == LlPrintMode.Normal) {
                    this.LL.Print(this.Project, this.LL.AutoProjectFile, false, printMode, LlBoxType.NormalMeter, mode.ToString(), true, @"C:\Windows\temp\");
                } else {
                    this.LL.Print(this.Project, this.LL.AutoProjectFile, false, printMode, LlBoxType.NormalMeter, mode.ToString(), false, @"C:\Windows\temp\");
                }

                return true;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return false;
            }
        }

        [STAThread]
        public void Print(string Title) {
            try {
                this.LL.Design(Title, this.Project, this.LL.AutoProjectFile, false);
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
