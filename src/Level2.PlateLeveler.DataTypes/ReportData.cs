using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Level2.PlateLeveler.DataTypes {
    public enum ExportType {
        xls, pdf, doc, txt, csv, jpg, rtf
    }
    public enum ProjectType {
        lst, crd, lbl
    }
    [Serializable]
    public class ReportData {
        private object _DataSource;
        public object DataSource {
            get => this._DataSource; set => this._DataSource = value;
        }

        private string _ExportPath;
        [XmlAttribute("ExportPath", DataType = "string")]
        public string ExportPath {
            get => this._ExportPath; set => this._ExportPath = value;
        }

        private string _Path;
        [XmlAttribute("Path", DataType = "string")]
        public string Path {
            get => this._Path; set => this._Path = value;
        }

        private string _File;
        [XmlAttribute("FileName", DataType = "string")]
        public string File {
            get => this._File; set => this._File = value;
        }

        private string _License;
        [XmlAttribute("License", DataType = "string")]
        public string License {
            get => this._License; set => this._License = value;
        }

        private string _ListType;
        [XmlAttribute("ListType", DataType = "string")]
        public string ListType {
            get => this._ListType; set => this._ListType = value;
        }

        private string _Mode;
        [XmlAttribute("Mode", DataType = "string")]
        public string Mode {
            get => this._Mode; set => this._Mode = value;
        }

        private string _PdfPath;
        [XmlAttribute("PdfPath", DataType = "string")]
        public string PdfPath {
            get => this._PdfPath; set => this._PdfPath = value;
        }

        public ProjectType Type => (ProjectType)Enum.Parse(typeof(ProjectType), this._ListType);

        [NonSerialized]
        private ExportData _Export;
        public ExportData Export {
            get => this._Export; set => this._Export = value;
        }
    }

    public class ReportList : List<ReportData> {
        public ReportList()
           : base() {
        }
    }

    public class ExportData {
        public string File { get; set; }
        public string Path { get; set; }
        public ExportType Type { get; set; }
    }

    public class VariableData {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string Status { get; set; }
        public string PlateID { get; set; }
        public string MaterialID { get; set; }
        public float? Width { get; set; }
        public float? YieldPoint { get; set; }
        public float? Thickness { get; set; }
        public float? RollDiametertRoll { get; set; }
        public string StartDate { get; set; }
        public string LastDate { get; set; }
    }
}
