using System;
using System.Data;
using System.Data.Common;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.DataProvider {
    public enum ProductionReportDef {
        ID, PRODDATE, PlateID, MaterialID, LevelingState, NoOfLeveling, CassetteNo, SetLevelerInlet, SetLevelerOutlet, SetTiltLeft, SetTiltRight, SetMiddleHeight, SetCurveProfile, ActLevelerInlet,
        ActLevelerOutlet, ActTiltLeft, ActTiltRight, ActMiddleHeight, ActCurveProfile, MeanAmpWaveLeft, MeasuringRange, MeanAmpWaveCenter, MeasuringCode, MeanAmpRight, HeightWaveLeft, SkiHead, SkiTail,
        YPosWaveLeft, LengthWaveLeft, NumberWaveLeft, Set1MRulerCross, Act1MRulerCross, Set1MRulerLength, Act1MRulerLength, Set2MRulerLength, Act2MRulerLength, GapLengthBow, MaxGapLengthBow, GapCrossBow,
        MaxGapCrossBow, Temperature, ActTemperature, HeightWaveQuarterLeft, YPosWaveQuarterLeft, LengthWaveCenter, NumberWaveCenter, HeightWaveQuarterRight, YPosWaveQuarterRight, LengthWaveQuarterRight,
        NumberWaveQuarterRight, HeightWaveRight, YPosWaveRight, NumberWaveRight, LengthWaveRight, XPosLengthBow, YPosLengthBow, XPosCrossBow, YPosCrossBow, XPosRuler1MCross, YPosRuler1MCross, XPosRuler1MLength,
        YPosRuler1MLength, XPosRuler2MLength, YPosRuler2MLength, MaxPlateWidth, MinPlateWidth, MeanPlateWidth, MaxDeviationOfCenterLine
    }

    public class ProductionReportAdapter {
        private readonly DbConnection cnn;
        private readonly DbProviderFactory provider;
        private readonly ConnectionData _Connection;

        public ProductionReportAdapter(ConnectionData con) {
            this.provider = DbProviderFactories.GetFactory(con.Namespace);
            this.cnn = this.provider.CreateConnection();
            this.cnn.ConnectionString = con.ConnectionString;
            this._Connection = con;
        }

        public ProductionReportData LoadProductionData(long id) {
            _ = new ProductionReportData();
            var sql = "SELECT PKEY_PROD,PlateID,PRODDATE,LEVELINGSTATE,NOOFLEVELING,CASSETTENO,SETLEVELERINLET,SETLEVELEROUTLET,SETTILTLEFT, SETTILTRIGHT,SETMIDDLEHEIGHT,SETCURVEPROFILE,ACTLEVELERINLET,ACTLEVELEROUTLET,ACTTILTLEFT,ACTTILTRIGHT, " +
                                 "ACTMIDDLEHEIGHT,ACTCURVEPROFILE,ACTEDGEWAVELEFT,ACTNOEDGEWAVELEFT,ACTEDGEWAVERIGHT,ACTNOEDGEWAVERIGHT, ACTCENTERWAVE,ACTNOCENTERWAVE,ACTSKIHEAD,ACTSKITAIL,SET1MRULER,ACT1MRULER,SET2MRULER,ACT2MRULER FROM PRODUCTIONREPORT " +
                                 "WHERE PKey_PDI = " + id.ToString();
            return this.LoadFullProductionData(sql);
        }

        public ProductionReportData LoadProductionData(string plateID) {
            _ = new ProductionReportData();
            var sql = "SELECT PKEY_PROD, ProdDate, PlateID, MaterialID, LevelingState,NoOfLeveling,CassetteNo,SetLevelerInlet, " +
                                      "SetLevelerOutlet,SetTiltLeft,SetTiltRight,SetMiddleHeight,SetCurveProfile,ActLevelerInlet,ActLevelerOutlet, " +
                                      "ActTiltLeft,ActTiltRight,ActMiddleHeight,ActCurveProfile,MeanAmpWaveLeft,MeasuringRange,MeanAmpWaveCenter, " +
                                      "MeasuringCode,MeanAmpRight,HeightWaveLeft,SkiHead,SkiTail,YPosWaveLeft,LengthWaveLeft,NumberWaveLeft, " +
                                      "Set1MRulerCross,Act1MRulerCross,Set1MRulerLength,Act1MRulerLength,Set2MRulerLength,Act2MRulerLength,GapLengthBow, " +
                                      "MaxGapLengthBow,GapCrossBow,MaxGapCrossBow,Temperature,ActTemperature,HeightWaveQuarterLeft,YPosWaveQuarterLeft, " +
                                      "LengthWaveCenter,NumberWaveCenter,HeightWaveQuarterRight,YPosWaveQuarterRight,LengthWaveQuarterRight,NumberWaveQuarterRight, " +
                                      "HeightWaveRight,YPosWaveRight,NumberWaveRight,LengthWaveRight,XPosLengthBow,YPosLengthBow,XPosCrossBow,YPosCrossBow, " +
                                      "XPosRuler1MCross,YPosRuler1MCross,XPosRuler1MLength,YPosRuler1MLength,XPosRuler2MLength,YPosRuler2MLength,MaxPlateWidth, " +
                                      "MinPlateWidth,MeanPlateWidth,MaxDeviationOfCenterLine FROM PRODUCTIONREPORT " +
                                 "WHERE PLATEID = '" + plateID + "'";
            return this.LoadFullProductionData(sql);
        }

        private ProductionReportData LoadFullProductionData(string sql) {
            var result = new ProductionReportData();
            var cmd = this.provider.CreateCommand();
            cmd.CommandText = sql;
            cmd.Connection = this.cnn;

            try {
                if (this.cnn.State == ConnectionState.Closed) {
                    this.cnn.Open();
                }

                var reader = cmd.ExecuteReader();

                while (reader.Read()) {
                    result = new ProductionReportData {
                        ActCurveProfile = reader.IsDBNull((int)ProductionReportDef.ActCurveProfile) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.ActCurveProfile]),
                        ActLevelerInlet = reader.IsDBNull((int)ProductionReportDef.ActLevelerInlet) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.ActLevelerInlet]),
                        ActLevelerOutlet = reader.IsDBNull((int)ProductionReportDef.ActLevelerOutlet) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.ActLevelerOutlet]),
                        ActMiddleHeight = reader.IsDBNull((int)ProductionReportDef.ActMiddleHeight) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.ActMiddleHeight]),
                        GapLengthBow = reader.IsDBNull((int)ProductionReportDef.GapLengthBow) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.GapLengthBow]),
                        ActRuler1MCross = reader.IsDBNull((int)ProductionReportDef.Act1MRulerCross) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.Act1MRulerCross]),
                        ActRuler1MLength = reader.IsDBNull((int)ProductionReportDef.Act1MRulerLength) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.Act1MRulerLength]),
                        ActRuler2MLength = reader.IsDBNull((int)ProductionReportDef.Act2MRulerLength) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.Act2MRulerLength]),
                        ActTemperature = reader.IsDBNull((int)ProductionReportDef.ActTemperature) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.ActTemperature]),
                        ActTiltLeft = reader.IsDBNull((int)ProductionReportDef.ActTiltLeft) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.ActTiltLeft]),
                        ActTiltRight = reader.IsDBNull((int)ProductionReportDef.ActTiltRight) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.ActTiltRight]),
                        CassetteNo = reader.IsDBNull((int)ProductionReportDef.CassetteNo) ? 0 : Convert.ToInt32(reader[(int)ProductionReportDef.CassetteNo]),
                        CurveProfile = reader.IsDBNull((int)ProductionReportDef.SetCurveProfile) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.SetCurveProfile]),
                        GapCrossBow = reader.IsDBNull((int)ProductionReportDef.GapCrossBow) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.GapCrossBow])
                    };
                    result.GapLengthBow = reader.IsDBNull((int)ProductionReportDef.GapLengthBow) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.GapLengthBow]);
                    result.HeightWaveLeft = reader.IsDBNull((int)ProductionReportDef.HeightWaveLeft) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.HeightWaveLeft]);
                    result.HeightWaveQuarterLeft = reader.IsDBNull((int)ProductionReportDef.HeightWaveQuarterLeft) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.HeightWaveQuarterLeft]);
                    result.HeightWaveQuarterRight = reader.IsDBNull((int)ProductionReportDef.HeightWaveQuarterRight) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.HeightWaveQuarterRight]);
                    result.HeightWaveRight = reader.IsDBNull((int)ProductionReportDef.HeightWaveRight) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.HeightWaveRight]);
                    result.LengthWaveCenter = reader.IsDBNull((int)ProductionReportDef.LengthWaveCenter) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.LengthWaveCenter]);
                    result.LengthWaveLeft = reader.IsDBNull((int)ProductionReportDef.LengthWaveLeft) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.LengthWaveLeft]);
                    result.LengthWaveQuarterRight = reader.IsDBNull((int)ProductionReportDef.LengthWaveQuarterRight) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.LengthWaveQuarterRight]);
                    result.LengthWaveRight = reader.IsDBNull((int)ProductionReportDef.LengthWaveRight) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.LengthWaveRight]);
                    result.LevelerInlet = reader.IsDBNull((int)ProductionReportDef.SetLevelerInlet) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.SetLevelerInlet]);
                    result.LevelerOutlet = reader.IsDBNull((int)ProductionReportDef.SetLevelerOutlet) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.SetLevelerOutlet]);
                    result.Leveling = reader.IsDBNull((int)ProductionReportDef.LevelingState) ? (short)0 : Convert.ToInt16(reader[(int)ProductionReportDef.LevelingState]);
                    result.MaterialID = reader[(int)ProductionReportDef.MaterialID].ToString();
                    result.MaxDeviationOfCenterLine = reader.IsDBNull((int)ProductionReportDef.MaxDeviationOfCenterLine) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.MaxDeviationOfCenterLine]);
                    result.MaxGapCrossBow = reader.IsDBNull((int)ProductionReportDef.MaxGapCrossBow) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.MaxGapCrossBow]);
                    result.MaxGapLengthBow = reader.IsDBNull((int)ProductionReportDef.MaxGapLengthBow) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.MaxGapLengthBow]);
                    result.MaxPlateWidth = reader.IsDBNull((int)ProductionReportDef.MaxPlateWidth) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.MaxPlateWidth]);
                    result.MeanAmpRight = reader.IsDBNull((int)ProductionReportDef.MeanAmpRight) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.MeanAmpRight]);
                    result.MeanAmpWaveCenter = reader.IsDBNull((int)ProductionReportDef.MeanAmpWaveCenter) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.MeanAmpWaveCenter]);
                    result.MeanAmpWaveLeft = reader.IsDBNull((int)ProductionReportDef.MeanAmpWaveLeft) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.MeanAmpWaveLeft]);
                    result.MeanPlateWidth = reader.IsDBNull((int)ProductionReportDef.MeanPlateWidth) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.MeanPlateWidth]);
                    result.MeasuringCode = reader.IsDBNull((int)ProductionReportDef.MeasuringCode) ? 0 : Convert.ToInt32(reader[(int)ProductionReportDef.MeasuringCode]);
                    result.MeasuringRange = reader.IsDBNull((int)ProductionReportDef.MeasuringRange) ? 0 : Convert.ToInt32(reader[(int)ProductionReportDef.MeasuringRange]);
                    result.MiddleHeight = reader.IsDBNull((int)ProductionReportDef.SetMiddleHeight) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.SetMiddleHeight]);
                    result.MinPlateWidth = reader.IsDBNull((int)ProductionReportDef.MinPlateWidth) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.MinPlateWidth]);
                    result.NoOfLeveling = reader.IsDBNull((int)ProductionReportDef.NoOfLeveling) ? (short)0 : Convert.ToInt16(reader[(int)ProductionReportDef.NoOfLeveling]);
                    result.NumberWaveCenter = reader.IsDBNull((int)ProductionReportDef.NumberWaveCenter) ? (short)0 : Convert.ToInt16(reader[(int)ProductionReportDef.NumberWaveCenter]);
                    result.NumberWaveLeft = reader.IsDBNull((int)ProductionReportDef.NumberWaveLeft) ? (short)0 : Convert.ToInt16(reader[(int)ProductionReportDef.NumberWaveLeft]);
                    result.NumberWaveQuarterRight = reader.IsDBNull((int)ProductionReportDef.NumberWaveQuarterRight) ? (short)0 : Convert.ToInt16(reader[(int)ProductionReportDef.NumberWaveQuarterRight]);
                    result.NumberWaveRight = reader.IsDBNull((int)ProductionReportDef.NumberWaveRight) ? (short)0 : Convert.ToInt16(reader[(int)ProductionReportDef.NumberWaveRight]);
                    result.Pkey_PDI = Convert.ToInt64(reader[(int)ProductionReportDef.ID]);
                    result.PlateID = reader[(int)ProductionReportDef.PlateID].ToString().Trim();
                    result.Ruler1MCrossPDI = reader.IsDBNull((int)ProductionReportDef.Set1MRulerCross) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.Set1MRulerCross]);
                    result.Ruler1MLengthPDI = reader.IsDBNull((int)ProductionReportDef.Set1MRulerLength) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.Set1MRulerLength]);
                    result.Ruler2MLengthPDI = reader.IsDBNull((int)ProductionReportDef.Set2MRulerLength) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.Set2MRulerLength]);
                    result.SkiHead = reader.IsDBNull((int)ProductionReportDef.SkiHead) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.SkiHead]);
                    result.SkiTail = reader.IsDBNull((int)ProductionReportDef.SkiTail) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.SkiTail]);
                    result.Temperature = reader.IsDBNull((int)ProductionReportDef.Temperature) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.Temperature]);
                    result.TiltLeft = reader.IsDBNull((int)ProductionReportDef.SetTiltLeft) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.SetTiltLeft]);
                    result.TiltRight = reader.IsDBNull((int)ProductionReportDef.SetTiltRight) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.SetTiltRight]);
                    result.XPosCrossBow = reader.IsDBNull((int)ProductionReportDef.XPosCrossBow) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.XPosCrossBow]);
                    result.XPosLengthBow = reader.IsDBNull((int)ProductionReportDef.XPosLengthBow) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.XPosLengthBow]);
                    result.XPosRuler1MCross = reader.IsDBNull((int)ProductionReportDef.XPosRuler1MCross) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.XPosRuler1MCross]);
                    result.XPosRuler1MLength = reader.IsDBNull((int)ProductionReportDef.XPosRuler1MLength) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.XPosRuler1MLength]);
                    result.XPosRuler2MLength = reader.IsDBNull((int)ProductionReportDef.XPosRuler2MLength) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.XPosRuler2MLength]);
                    result.YPosCrossBow = reader.IsDBNull((int)ProductionReportDef.YPosCrossBow) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.YPosCrossBow]);
                    result.YPosLengthBow = reader.IsDBNull((int)ProductionReportDef.YPosLengthBow) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.YPosLengthBow]);
                    result.YPosRuler1MCross = reader.IsDBNull((int)ProductionReportDef.YPosRuler1MCross) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.YPosRuler1MCross]);
                    result.YPosRuler1MLength = reader.IsDBNull((int)ProductionReportDef.YPosRuler1MLength) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.YPosRuler1MLength]);
                    result.YPosRuler2MLength = reader.IsDBNull((int)ProductionReportDef.YPosRuler2MLength) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.YPosRuler2MLength]);
                    result.YPosWaveLeft = reader.IsDBNull((int)ProductionReportDef.YPosWaveLeft) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.YPosWaveLeft]);
                    result.YPosWaveQuarterLeft = reader.IsDBNull((int)ProductionReportDef.YPosWaveQuarterLeft) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.YPosWaveQuarterLeft]);
                    result.YPosWaveQuarterRight = reader.IsDBNull((int)ProductionReportDef.YPosWaveQuarterRight) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.YPosWaveQuarterRight]);
                    result.YPosWaveRight = reader.IsDBNull((int)ProductionReportDef.YPosWaveRight) ? 0 : Convert.ToSingle(reader[(int)ProductionReportDef.YPosWaveRight]);
                }
                reader.Close();
                return result == null ? null : result.ID > 0 ? result : null;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return null;
            }
        }
    }
}
