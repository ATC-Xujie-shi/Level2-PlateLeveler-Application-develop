using System;
using System.Collections.Generic;
using System.Linq;

namespace Level2.PlateLeveler.DataTypes {
    public class ProductionReportData : LevelerData {
        protected int _CassetteNo;
        public int CassetteNo {
            get => this._CassetteNo; set => this._CassetteNo = value;
        }

        protected DateTime _ProdDate;
        public DateTime ProdDate {
            get => this._ProdDate; set => this._ProdDate = value;
        }

        protected float _ActLevelerInlet;
        public float ActLevelerInlet {
            get => this._ActLevelerInlet; set => this._ActLevelerInlet = value;
        }

        protected float _ActLevelerOutlet;
        public float ActLevelerOutlet {
            get => this._ActLevelerOutlet; set => this._ActLevelerOutlet = value;
        }

        protected float _ActTiltLeft;
        public float ActTiltLeft {
            get => this._ActTiltLeft; set => this._ActTiltLeft = value;
        }

        protected float _ActTiltRight;
        public float ActTiltRight {
            get => this._ActTiltRight; set => this._ActTiltRight = value;
        }

        protected float _ActMiddleHeight;
        public float ActMiddleHeight {
            get => this._ActMiddleHeight; set => this._ActMiddleHeight = value;
        }

        protected float _ActCurveProfile;
        public float ActCurveProfile {
            get => this._ActCurveProfile; set => this._ActCurveProfile = value;
        }

        protected float _MeanAmpWaveLeft;
        public float MeanAmpWaveLeft {
            get => this._MeanAmpWaveLeft; set => this._MeanAmpWaveLeft = value;
        }

        protected int _MeasuringRange;
        public int MeasuringRange {
            get => this._MeasuringRange; set => this._MeasuringRange = value;
        }

        protected float _MeanAmpWaveCenter;
        public float MeanAmpWaveCenter {
            get => this._MeanAmpWaveCenter; set => this._MeanAmpWaveCenter = value;
        }

        protected new int _MeasuringCode;
        public new int MeasuringCode {
            get => this._MeasuringCode; set => this._MeasuringCode = value;
        }

        protected float _MeanAmpRight;
        public float MeanAmpRight {
            get => this._MeanAmpRight; set => this._MeanAmpRight = value;
        }

        protected float _HeightWaveLeft;
        public float HeightWaveLeft {
            get => this._HeightWaveLeft; set => this._HeightWaveLeft = value;
        }

        protected float _SkiHead;
        public float SkiHead {
            get => this._SkiHead; set => this._SkiHead = value;
        }

        protected float _SkiTail;
        public float SkiTail {
            get => this._SkiTail; set => this._SkiTail = value;
        }

        protected int _PlateInTol;
        public int PlateInTol {
            get => this._PlateInTol; set => this._PlateInTol = value;
        }

        protected float _YPosWaveLeft;
        public float YPosWaveLeft {
            get => this._YPosWaveLeft; set => this._YPosWaveLeft = value;
        }

        protected int _NumberWaveLeft;
        public int NumberWaveLeft {
            get => this._NumberWaveLeft; set => this._NumberWaveLeft = value;
        }

        protected float _LengthWaveLeft;
        public float LengthWaveLeft {
            get => this._LengthWaveLeft; set => this._LengthWaveLeft = value;
        }

        protected float _HeightWaveQuarterLeft;
        public float HeightWaveQuarterLeft {
            get => this._HeightWaveQuarterLeft; set => this._HeightWaveQuarterLeft = value;
        }

        protected float _YPosWaveQuarterLeft;
        public float YPosWaveQuarterLeft {
            get => this._YPosWaveQuarterLeft; set => this._YPosWaveQuarterLeft = value;
        }

        protected float _LengthWaveCenter;
        public float LengthWaveCenter {
            get => this._LengthWaveCenter; set => this._LengthWaveCenter = value;
        }

        protected int _NumberWaveCenter;
        public int NumberWaveCenter {
            get => this._NumberWaveCenter; set => this._NumberWaveCenter = value;
        }

        protected float _HeightWaveQuarterRight;
        public float HeightWaveQuarterRight {
            get => this._HeightWaveQuarterRight; set => this._HeightWaveQuarterRight = value;
        }

        protected float _YPosWaveQuarterRight;
        public float YPosWaveQuarterRight {
            get => this._YPosWaveQuarterRight; set => this._YPosWaveQuarterRight = value;
        }

        protected float _LengthWaveQuarterRight;
        public float LengthWaveQuarterRight {
            get => this._LengthWaveQuarterRight; set => this._LengthWaveQuarterRight = value;
        }

        protected int _NumberWaveQuarterRight;
        public int NumberWaveQuarterRight {
            get => this._NumberWaveQuarterRight; set => this._NumberWaveQuarterRight = value;
        }

        protected float _HeightWaveRight;
        public float HeightWaveRight {
            get => this._HeightWaveRight; set => this._HeightWaveRight = value;
        }

        protected float _YPosWaveRight;
        public float YPosWaveRight {
            get => this._YPosWaveRight; set => this._YPosWaveRight = value;
        }

        protected int _NumberWaveRight;
        public int NumberWaveRight {
            get => this._NumberWaveRight; set => this._NumberWaveRight = value;
        }

        protected float _LengthWaveRight;
        public float LengthWaveRight {
            get => this._LengthWaveRight; set => this._LengthWaveRight = value;
        }

        protected new float _GapLengthBow;
        public new float GapLengthBow {
            get => this._GapLengthBow; set => this._GapLengthBow = value;
        }

        protected float _MaxGapLengthBow;
        public float MaxGapLengthBow {
            get => this._MaxGapLengthBow; set => this._MaxGapLengthBow = value;
        }

        protected float _XPosLengthBow;
        public float XPosLengthBow {
            get => this._XPosLengthBow; set => this._XPosLengthBow = value;
        }

        protected float _YPosLengthBow;
        public float YPosLengthBow {
            get => this._YPosLengthBow; set => this._YPosLengthBow = value;
        }

        protected new float _GapCrossBow;
        public new float GapCrossBow {
            get => this._GapCrossBow; set => this._GapCrossBow = value;
        }

        protected float _MaxGapCrossBow;
        public float MaxGapCrossBow {
            get => this._MaxGapCrossBow; set => this._MaxGapCrossBow = value;
        }

        protected float _XPosCrossBow;
        public float XPosCrossBow {
            get => this._XPosCrossBow; set => this._XPosCrossBow = value;
        }

        protected float _YPosCrossBow;
        public float YPosCrossBow {
            get => this._YPosCrossBow; set => this._YPosCrossBow = value;
        }

        protected float _ActRuler1MCross;
        public float ActRuler1MCross {
            get => this._ActRuler1MCross; set => this._ActRuler1MCross = value;
        }

        protected float _XPosRuler1MCross;
        public float XPosRuler1MCross {
            get => this._XPosRuler1MCross; set => this._XPosRuler1MCross = value;
        }

        protected float _YPosRuler1MCross;
        public float YPosRuler1MCross {
            get => this._YPosRuler1MCross; set => this._YPosRuler1MCross = value;
        }

        protected float _ActRuler1MLength;
        public float ActRuler1MLength {
            get => this._ActRuler1MLength; set => this._ActRuler1MLength = value;
        }

        protected float _XPosRuler1MLength;
        public float XPosRuler1MLength {
            get => this._XPosRuler1MLength; set => this._XPosRuler1MLength = value;
        }

        protected float _YPosRuler1MLength;
        public float YPosRuler1MLength {
            get => this._YPosRuler1MLength; set => this._YPosRuler1MLength = value;
        }

        protected float _ActRuler2MLength;
        public float ActRuler2MLength {
            get => this._ActRuler2MLength; set => this._ActRuler2MLength = value;
        }

        protected float _ActTemperature;
        public float ActTemperature {
            get => this._ActTemperature; set => this._ActTemperature = value;
        }

        protected float _XPosRuler2MLength;
        public float XPosRuler2MLength {
            get => this._XPosRuler2MLength; set => this._XPosRuler2MLength = value;
        }

        protected float _YPosRuler2MLength;
        public float YPosRuler2MLength {
            get => this._YPosRuler2MLength; set => this._YPosRuler2MLength = value;
        }

        protected float _MaxPlateWidth;
        public float MaxPlateWidth {
            get => this._MaxPlateWidth; set => this._MaxPlateWidth = value;
        }

        protected float _MinPlateWidth;
        public float MinPlateWidth {
            get => this._MinPlateWidth; set => this._MinPlateWidth = value;
        }

        protected float _MeanPlateWidth;
        public float MeanPlateWidth {
            get => this._MeanPlateWidth; set => this._MeanPlateWidth = value;
        }

        protected float _MaxDeviationOfCenterLine;
        public float MaxDeviationOfCenterLine {
            get => this._MaxDeviationOfCenterLine; set => this._MaxDeviationOfCenterLine = value;
        }

        public void SetPlateData(PDI plate) {
            this._EModule = plate.EModule;
            this._GapCrossBow = plate.GapCrossBow ?? 0;
            this._GapLengthBow = plate.GapLengthBow ?? 0;
            this._Length = plate.Length;
            this._Leveling = plate.Leveling;
            this._Location = plate.Location;
            this._Manual = plate.Manual;
            this._MaterialID = plate.MaterialID;
            this._MeasuringCode = plate.MeasuringCode ?? 0;
            this._PlateID = plate.PlateID;
            this._Ruler1MCrossPDI = plate.Ruler1MCrossPDI;
            this._Ruler1MLengthPDI = plate.Ruler1MLengthPDI;
            this._Ruler2MLengthPDI = plate.Ruler2MLengthPDI;
            this._SteelGrade = plate.SteelGrade;
            this._TensileStrength = plate.TensileStrength;
            this._Thickness = plate.Thickness;
            this._Width = plate.Width;
            this._YieldPoint = plate.YieldPoint;
        }

        public void SetLevelerData(LevelerData data) {
            this._ActCurveProfile = data.CurveProfile;
            this._ActLevelerInlet = data.LevelerInlet;
            this._ActLevelerOutlet = data.LevelerOutlet;
            this._ActMiddleHeight = data.MiddleHeight;
            this._ActTiltLeft = data.TiltLeft;
            this._ActTiltRight = data.TiltRight;
            this._PlateID = data.PlateID;
            this._ActTemperature = data.Temperature;
            this._NoOfLeveling = data.NoOfLeveling;
            this._State = data.State;
            this._Leveling = data.Leveling;
        }

        public void SetFlatnessData(FlatMeasData flat) {
            this._ActRuler1MCross = flat.ActRuler1MCross / 100f;
            this._ActRuler1MLength = flat.ActRuler1MLength / 100f;
            this._ActRuler2MLength = flat.ActRuler2MLength / 100f;
            this._GapCrossBow = flat.GapCrossBow / 100f;
            this._GapLengthBow = flat.GapLengthBow / 100f;
            this._HeightWaveLeft = flat.HeightWaveLeft / 100f;
            this._HeightWaveQuarterLeft = flat.HeightWaveQuarterLeft / 100f;
            this._HeightWaveQuarterRight = flat.HeightWaveQuarterRight / 100f;
            this._HeightWaveRight = flat.HeightWaveRight / 100f;
            this._LengthWaveCenter = flat.LengthWaveCenter / 100f;
            this._LengthWaveLeft = flat.LengthWaveLeft / 100f;
            this._LengthWaveQuarterRight = flat.LengthWaveQuarterRight / 100f;
            this._LengthWaveRight = flat.LengthWaveRight / 100f;
            this._MaxDeviationOfCenterLine = flat.MaxDeviationOfCenterLine / 100f;
            this._MaxGapCrossBow = flat.MaxGapCrossBow / 100f;
            this._MaxGapLengthBow = flat.MaxGapLengthBow / 100f;
            this._MaxPlateWidth = flat.MaxPlateWidth / 100f;
            this._MeanAmpRight = flat.MeanAmpRight / 100f;
            this._MeanAmpWaveCenter = flat.MeanAmpWaveCenter / 100f;
            this._MeanAmpWaveLeft = flat.MeanAmpWaveLeft / 100f;
            this._MeanPlateWidth = flat.MeanPlateWidth / 100f;
            this._MeasuringCode = flat.MeasuringCode;
            this._MeasuringRange = flat.MeasuringRange;
            this._MinPlateWidth = flat.MinPlateWidth / 100f;
            this._NumberWaveCenter = flat.NumberWaveCenter;
            this._NumberWaveLeft = flat.NumberWaveLeft;
            this._NumberWaveQuarterRight = flat.NumberWaveQuarterRight;
            this._NumberWaveRight = flat.NumberWaveRight;
            this._Ruler1MCrossPDI = flat.Ruler1MCross / 100f;
            this._Ruler1MLengthPDI = flat.Ruler1MLength / 100f;
            this._Ruler2MLengthPDI = flat.Ruler2MLength / 100f;
            this._SkiHead = flat.SkiHead / 100f;
            this._SkiTail = flat.SkiTail / 100f;
            this._XPosCrossBow = flat.XPosCrossBow / 100f;
            this._XPosLengthBow = flat.XPosLengthBow / 100f;
            this._XPosRuler1MCross = flat.XPosRuler1MCross / 100f;
            this._XPosRuler1MLength = flat.XPosRuler1MLength / 100f;
            this._XPosRuler2MLength = flat.XPosRuler2MLength / 100f;
            this._YPosCrossBow = flat.YPosCrossBow / 100f;
            this._YPosLengthBow = flat.YPosLengthBow / 100f;
            this._YPosRuler1MCross = flat.YPosRuler1MCross / 100f;
            this._YPosRuler1MLength = flat.YPosRuler1MLength / 100f;
            this._YPosRuler2MLength = flat.YPosRuler2MLength / 100f;
            this._YPosWaveLeft = flat.YPosWaveLeft / 100f;
            this._YPosWaveQuarterLeft = flat.YPosWaveQuarterLeft / 100f;
            this._YPosWaveQuarterRight = flat.YPosWaveQuarterRight / 100f;
            this._YPosWaveRight = flat.YPosWaveRight / 100f;
        }
        public string[] Fields {
            get {
                return [ "ID", "ProdDate", "PlateID", "MaterialID", "LevelingState", "NoOfLeveling", "CassetteNo", "SetLevelerInlet",
                    "SetLevelerOutlet", "SetTiltLeft", "SetTiltRight", "SetMiddleHeight", "SetCurveProfile", "ActLevelerInlet", "ActLevelerOutlet",
                    "ActTiltLeft", "ActTiltRight", "ActMiddleHeight", "ActCurveProfile", "MeanAmpWaveLeft", "MeasuringRange", "MeanAmpWaveCenter",
                    "MeasuringCode", "MeanAmpRight", "HeightWaveLeft", "SkiHead", "SkiTail", "YPosWaveLeft", "LengthWaveLeft", "NumberWaveLeft",
                    "Set1MRulerCross", "Act1MRulerCross", "Set1MRulerLength", "Act1MRulerLength", "Set2MRulerLength", "Act2MRulerLength", "GapLengthBow",
                    "MaxGapLengthBow", "GapCrossBow", "MaxGapCrossBow", "Temperature", "ActTemperature", "HeightWaveQuarterLeft", "YPosWaveQuarterLeft",
                    "LengthWaveCenter", "NumberWaveCenter", "HeightWaveQuarterRight", "YPosWaveQuarterRight", "LengthWaveQuarterRight", "NumberWaveQuarterRight",
                    "HeightWaveRight", "YPosWaveRight", "NumberWaveRight", "LengthWaveRight", "XPosLengthBow", "YPosLengthBow", "XPosCrossBow", "YPosCrossBow",
                    "XPosRuler1MCross", "YPosRuler1MCross", "XPosRuler1MLength", "YPosRuler1MLength", "XPosRuler2MLength", "YPosRuler2MLength", "MaxPlateWidth",
                    "MinPlateWidth", "MeanPlateWidth", "MaxDeviationOfCenterLine" ];
            }
        }

        public object[] Values {
            get {
                return [this._ID,this._ProdDate,this._PlateID,this._MaterialID,this._Leveling,this._NoOfLeveling,this._CassetteNo,this._LevelerInlet,
                    this._LevelerOutlet,this._TiltLeft,this._TiltRight,this._MiddleHeight,this._CurveProfile,this._ActLevelerInlet,this._ActLevelerOutlet,
                    this._ActTiltLeft,this._ActTiltRight,this._ActMiddleHeight,this._ActCurveProfile,this._MeanAmpWaveLeft,this._MeasuringRange,this._MeanAmpWaveCenter,
                    this._MeasuringCode,this._MeanAmpRight,this._HeightWaveLeft,this._SkiHead,this._SkiTail,this._YPosWaveLeft,this._LengthWaveLeft,this._NumberWaveLeft,
                    this._Ruler1MCrossPDI,this._ActRuler1MCross,this._Ruler1MLengthPDI,this._ActRuler1MLength,this._Ruler2MLengthPDI,this._ActRuler2MLength,this._GapLengthBow,
                    this._MaxGapLengthBow,this._GapCrossBow,this._MaxGapCrossBow,this._Temperature,this._ActTemperature,this._HeightWaveQuarterLeft,this._YPosWaveQuarterLeft,
                    this._LengthWaveCenter,this._NumberWaveCenter,this._HeightWaveQuarterRight,this._YPosWaveQuarterRight,this._LengthWaveQuarterRight,this._NumberWaveQuarterRight,
                    this._HeightWaveRight,this._YPosWaveRight,this._NumberWaveRight,this._LengthWaveRight,this._XPosLengthBow,this._YPosLengthBow,this._XPosCrossBow,this._YPosCrossBow,
                    this._XPosRuler1MCross,this._YPosRuler1MCross,this._XPosRuler1MLength,this._YPosRuler1MLength,this._XPosRuler2MLength,this._YPosRuler2MLength,this._MaxPlateWidth,
                    this._MinPlateWidth,this._MeanPlateWidth,this._MaxDeviationOfCenterLine ];
            }
        }
    }

    public class PlateReport : PlateData {
        public PlateReport() {
        }

        public PlateReport(PDI plate) {
            this._Width = plate.Width;
            this._Thickness = plate.Thickness;
            this._Length = plate.Length;
            this._MaterialID = plate.MaterialID;
            this._YieldPoint = plate.YieldPoint;
            this._PlateID = plate.PlateID;
        }

        [TelegramDefinition("Telegramblock")]
        public int PlasticalRatio { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int CalcInlet { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int CalcOutlet { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int CalcTiltingLeft { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int CalcTiltingRight { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int CalcMiddleHeight { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int CalcCurveProfile { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int CalcPower { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int CalcTorque { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int CalcLevelerForce { get; set; }

        public ActualMeasureList ActualMeasures => this.ActualMeasureArray != null ? (ActualMeasureList)this.ActualMeasureArray.ToList() : [];

        [TelegramDefinition("TelegramList")]
        public ActualMeasureData[] ActualMeasureArray { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int ActTemperature { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int ActLengthWave { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int ActDistanceWave { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int ActPosLengthWave { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int ActCrossWave { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int ActPosCrossWave { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int ActWaveMax { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int ActPeakValue { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int ActPosXPeakValue { get; set; }
        [TelegramDefinition("Telegramblock")]
        public int ActPosYPeakValue { get; set; }
        [TelegramDefinition("Telegramblock")]
        public short MachineState { get; set; }
        [TelegramDefinition("TelegramArray", "FaultCode")]
        public short[] FaultCode { get; set; }
    }

    public class ActualMeasureData {
        public float SetTiltLeft { get; set; }

        //[TelegramDefinition("Telegramblock")]
        [TelegramDefinition("TelegramValue")]
        public float ActHeadPos { get; set; }
        [TelegramDefinition("TelegramValue")]
        public float ActLevelerInlet { get; set; }
        [TelegramDefinition("TelegramValue")]
        public float ActLevelerOutlet { get; set; }
        [TelegramDefinition("TelegramValue")]
        public string PlateID { get; set; }
        [TelegramDefinition("TelegramValue")]
        public string MaterialID { get; set; }
        [TelegramDefinition("TelegramValue")]
        public float ActTiltingLeft { get; set; }
        [TelegramDefinition("TelegramValue")]
        public float ActTiltingRight { get; set; }
        [TelegramDefinition("TelegramValue")]
        public float ActMiddleHeight { get; set; }
        [TelegramDefinition("TelegramValue")]
        public float ActCurveProfile1 { get; set; }
        [TelegramDefinition("TelegramValue")]
        public float ActCurveProfile { get; set; }
        [TelegramDefinition("TelegramValue")]
        public float ActSpeed { get; set; }
        public float ActPowerDrive1 { get; set; }
        public float ActPowerDrive2 { get; set; }
        public float ActPowerDrive3 { get; set; }
        public float ActPowerDrive4 { get; set; }
        public float ActPowerDrive5 { get; set; }
        public float ActPowerDrive6 { get; set; }
        public float ActPowerDrive7 { get; set; }
        public float ActPowerDrive8 { get; set; }
        public float ActPowerDrive9 { get; set; }
        public float ActPowerDrive10 { get; set; }
        public float ActPowerDrive11 { get; set; }
        public float ActPowerDrive12 { get; set; }
        public float ActPowerDrive13 { get; set; }
        public float ActTorqueDrive1 { get; set; }
        public float ActTorqueDrive2 { get; set; }
        public float ActTorqueDrive3 { get; set; }
        public float ActTorqueDrive4 { get; set; }
        public float ActTorqueDrive5 { get; set; }
        public float ActTorqueDrive6 { get; set; }
        public float ActTorqueDrive7 { get; set; }
        public float ActTorqueDrive8 { get; set; }
        public float ActTorqueDrive9 { get; set; }
        public float ActTorqueDrive10 { get; set; }
        public float ActTorqueDrive11 { get; set; }
        public float ActTorqueDrive12 { get; set; }
        public float ActTorqueDrive13 { get; set; }
        public float ActCurrentDrive1 { get; set; }
        public float ActCurrentDrive2 { get; set; }
        public float ActCurrentDrive3 { get; set; }
        public float ActCurrentDrive4 { get; set; }
        public float ActCurrentDrive5 { get; set; }
        public float ActCurrentDrive6 { get; set; }
        public float ActCurrentDrive7 { get; set; }
        public float ActCurrentDrive8 { get; set; }
        public float ActCurrentDrive9 { get; set; }
        public float ActCurrentDrive10 { get; set; }
        public float ActCurrentDrive11 { get; set; }
        public float ActCurrentDrive12 { get; set; }
        public float ActCurrentDrive13 { get; set; }
        [TelegramDefinition("TelegramArray", "ActPowerDrive")]
        public float[] ActPowerDrive { get; set; }
        [TelegramDefinition("TelegramArray", "ActTorqueDrive")]
        public float[] ActTorqueDrive { get; set; }
        [TelegramDefinition("TelegramArray", "ActCurrentDrive")]
        public float[] ActCurrentDrive { get; set; }
    }

    public class ActualMeasureList : List<ActualMeasureData> {
        public ActualMeasureList()
           : base() {
        }

        public PDI Plate { get; set; }

        public void FillActualMeasures(List<ActualValue> list) {
            _ = new ActualMeasureData();
            var idx = 0;
            foreach (var item in list) {
                idx++;
                if (idx > 20) {
                    break;
                }

                var data = new ActualMeasureData {
                    PlateID = item.PlateID,
                    ActSpeed = item.ActSpeed ?? 0f,
                    ActCurrentDrive = new float[4]
                };
                data.ActCurrentDrive[0] = item.ActCurrentDrive1 ?? 0f;
                data.ActCurrentDrive[1] = item.ActCurrentDrive2 ?? 0f;
                data.ActCurrentDrive[2] = item.ActCurrentDrive3 ?? 0f;
                data.ActCurrentDrive[3] = item.ActCurrentDrive4 ?? 0f;
                data.ActCurveProfile = item.ActCurveProfile ?? 0f;
                data.ActLevelerInlet = item.ActLevelerInlet ?? 0f;
                data.ActLevelerOutlet = item.ActLevelerOutlet ?? 0f;
                data.ActPowerDrive = new float[4];
                data.ActPowerDrive[0] = item.ActPowerDrive1 ?? 0f;
                data.ActPowerDrive[1] = item.ActPowerDrive2 ?? 0f;
                data.ActPowerDrive[2] = item.ActPowerDrive3 ?? 0f;
                data.ActPowerDrive[3] = item.ActPowerDrive4 ?? 0f;
                data.ActTiltingLeft = item.ActTiltLeft ?? 0f;
                data.ActTiltingRight = item.ActTiltRight ?? 0f;
                data.ActTorqueDrive = new float[4];
                data.ActTorqueDrive[0] = item.ActTorqueDrive1 ?? 0f;
                data.ActTorqueDrive[1] = item.ActTorqueDrive2 ?? 0f;
                data.ActTorqueDrive[2] = item.ActTorqueDrive3 ?? 0f;
                data.ActTorqueDrive[3] = item.ActTorqueDrive4 ?? 0f;
                this.Add(data);
            }
        }
    }
}
