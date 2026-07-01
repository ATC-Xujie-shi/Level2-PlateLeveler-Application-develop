using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using Level2.PlateLeveler.DataAccess;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.Server {
    public enum Operations {
        Insert = 1, Update = 2, Delete = 3
    }

    public enum MsgStatus {
        ProcessedData = 2, New = 1, UpdatePDI = 3, Error = -1
    }

    public enum MsgLocation {
        NewPlate = 0, Centering1 = 1, Centering2 = 2, Leveler = 3, NotUsed = 4, Out = 5
    }

    public class L3L2Interface : IDisposable {
        private readonly Timer _Timer;
        private LEVELEREntities _Entity;
        private readonly InitData _initData;

        public short LastTrackingLocation { get; } = -1;

        private readonly PDIAdapter _PDIAdapter;
        private readonly PDOAdapter _PDOAdapter;

        public L3L2Interface(InitData init) {
            try {
                this._initData = init;

                var L3Connection = this._initData.Connections.GetItem(ConnectionDef.PDI.ToString());
                if (L3Connection == null) {
                    Logging.SendMessage("Error: Could not find L3 DB Connection for the PDI and PDO adapter!", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, this.GetType());
                }

                this._PDIAdapter = new PDIAdapter(L3Connection, null);
                this._PDOAdapter = new PDOAdapter(L3Connection, null);

                this.InitEntity();

                //_lastTrackingLocation = LastLocation();

                this._Timer = new Timer(this._initData.Interval.L3L2);
                this._Timer.Elapsed += this._TimerL3L2_Elapsed;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void _TimerL3L2_Elapsed(object sender, ElapsedEventArgs e) {
            try {
                this._Timer.Stop();
                if (this._initData.Constants.Order.Equals(nameof(OrderNumberDef.C26841216), StringComparison.Ordinal)) {
                    this.GetPDIData841216();
                } else {
                    this.SearchNewPDIMessage();
                }

                this.CheckL3Watchdog();

                this._Timer.Start();
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        /// <summary>
        /// Search for new PDI Data from L3 and add them to the internal DB.
        /// If the PlateID is already known in the internal DB, the plate will be updated with the new values.
        /// </summary>
        private void GetPDIData841216() {
            try {
                var PDIDataList = this._PDIAdapter.LoadPlateData841216();
                if (PDIDataList != null && PDIDataList.Count > 0) {
                    Logging.SendMessage("Info: Found new PDI data in L3 interface DB. List with " + PDIDataList.Count + " elements loaded.", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                    var PDIDataListSuccess = new List<PDI>();

                    foreach (var PDI in PDIDataList) {
                        if (PDI != null && !string.IsNullOrWhiteSpace(PDI.PlateID)) {
                            var PDIExisting = this._Entity.PDIs.FirstOrDefault(item => item.PlateID == PDI.PlateID);
                            if (PDIExisting != null && PDIExisting.PKEY_PDI > 0) {
                                PDIExisting.PlateID = PDI.PlateID;
                                PDIExisting.MaterialID = PDI.MaterialID;
                                PDIExisting.SteelGrade = PDI.SteelGrade;
                                PDIExisting.Length = PDI.Length;
                                PDIExisting.Width = PDI.Width;
                                PDIExisting.Thickness = PDI.Thickness;
                                PDIExisting.TensileStrength = PDI.TensileStrength;
                                PDIExisting.YieldPoint = PDI.YieldPoint;
                                PDIExisting.Ruler1MCrossPDI = PDI.Ruler1MCrossPDI;
                                PDIExisting.Ruler1MLengthPDI = PDI.Ruler1MLengthPDI;
                                PDIExisting.Ruler2MLengthPDI = PDI.Ruler2MLengthPDI;
                                PDIExisting.EModule = PDI.EModule;
                                PDIExisting.Leveling = PDI.Leveling;
                                PDIExisting.Location = PDI.Location;
                                PDIExisting.State = PDI.State;

                                Logging.SendMessage("Info: Update operation on PDI has been executed. (Plate ID: " + PDIExisting.PlateID + ", PKEY_PDI: " + PDIExisting.PKEY_PDI + ", PKey L3 InterfaceDB: " + PDI.PKEY_PDI + ")", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                            } else {
                                _ = this._Entity.PDIs.Add(PDI);
                                Logging.SendMessage("Info: Insert operation on PDI has been executed. (Plate ID: " + PDI.PlateID + ", PKey L3 InterfaceDB: " + PDI.PKEY_PDI + ")", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                            }

                            PDIDataListSuccess.Add(PDI);
                        }
                    }

                    if (!this._PDIAdapter.MarkPlateDataInL3AsProcessed841216(PDIDataListSuccess)) {
                        Logging.SendMessage("Error: Could not mark all received PDI Data as processed. The list contains " + PDIDataListSuccess.Count + " elements.", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, this.GetType());
                    }

                    _ = this._Entity.SaveChanges();
                    this.Listener.SendDatabaseUpdateToHMI();
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void SearchNewPDIMessage() {
            try {
                this.InitEntity();

                var listMessages = this._Entity.L3_L2_MODEL_PDI.Where(x => x.State == (int)MsgStatus.New || x.State == (int)MsgStatus.UpdatePDI).ToList();

                if (listMessages.Count != 0) {
                    foreach (var value in listMessages) {
                        if (value.State == (int)MsgStatus.New) {
                            this.InsertOperation(value);
                        }
                        if (value.State == (int)MsgStatus.UpdatePDI) {
                            this.UpdatePDIOperation(value);
                        }
                    }
                    _ = this._Entity.SaveChanges();
                    this.Listener.SendDatabaseUpdateToHMI();
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void UpdatePDIOperation(L3_L2_MODEL_PDI value) {
            //LEVELEREntities newEntity = new LEVELEREntities();
            var selPDI = this._Entity.PDIs.FirstOrDefault(x => x.PlateID == value.PlateID);
            //if (selPDI != null)
            //{
            try {
                selPDI.PlateID = value.PlateID;
                selPDI.MaterialID = value.MaterialID;
                selPDI.SteelGrade = value.SteelGrade;
                selPDI.Length = (float)value.PlateLength;
                selPDI.Width = (float)value.PlateWidth;
                selPDI.Thickness = (float)value.PlateThickness;
                selPDI.YieldPoint = (float)value.YieldPoint;
                selPDI.TensileStrength = (float)value.TensileStrength;
                selPDI.MeasuringCode = 0;
                selPDI.Ruler1MCrossPDI = (float)value.Ruler1MCross;
                selPDI.Ruler1MLengthPDI = (float)value.Ruler1MLength;
                selPDI.Ruler2MLengthPDI = (float)value.Ruler2MLength;
                selPDI.EModule = (float)value.EModule;
                selPDI.Leveling = value.Leveling;
                selPDI.GapLengthBow = 0;// value.GapLengthBow;
                selPDI.GapCrossBow = 0;// value.GapCrossBow;
                selPDI.Manual = 0;
                selPDI.Location = value.Location;
                selPDI.State = value.State;
                selPDI.Date = value.InsertDateTime;

                value.State = (int)MsgStatus.ProcessedData;
                value.UpdateDateTime = DateTime.Now;
                value.ErrorMessage = "No Error";

                Logging.SendMessage("Update operation on PDI has been executed. Plate ID: <" + value.PlateID + "> PKEY: <" + value.PKEY_PDI + ">", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
            } catch (Exception ex) {
                value.State = (int)MsgStatus.Error;
                value.UpdateDateTime = DateTime.Now;
                value.ErrorMessage = ex.Message;
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
            //}
            //else
            //{
            //   value.State = (int)MsgStatus.Error;
            //   value.UpdateDateTime = DateTime.Now;
            //   value.ErrorMessage = "PDI with selected PlateID <" + value.PlateID + "> does not exist. Insert requested before Update";
            //   Logging.SendMessage("ERROR - " + value.ErrorMessage, System.Reflection.MethodInfo.GetCurrentMethod().Name, LoggerLevel.Error, this.GetType());
            //}
            //newEntity.SaveChanges();
        }

        private void InsertOperation(L3_L2_MODEL_PDI value) {
            //LEVELEREntities newEntity = new LEVELEREntities();
            //PDI pdiExists = newEntity.PDIs.FirstOrDefault(x => x.PlateID == value.PlateID);

            //if (pdiExists == null)
            //{
            try {
                var pdi = new PDI {
                    PlateID = value.PlateID,
                    MaterialID = value.MaterialID,
                    SteelGrade = value.SteelGrade,
                    Length = (float)value.PlateLength,
                    Width = (float)value.PlateWidth,
                    Thickness = (float)value.PlateThickness,
                    YieldPoint = (float)value.YieldPoint,
                    TensileStrength = (float)value.TensileStrength,
                    MeasuringCode = 0,
                    Ruler1MCrossPDI = (float)value.Ruler1MCross,
                    Ruler1MLengthPDI = (float)value.Ruler1MLength,
                    Ruler2MLengthPDI = (float)value.Ruler2MLength,
                    EModule = (float)value.EModule,
                    Leveling = value.Leveling,
                    GapLengthBow = 0,// value.GapLengthBow;
                    GapCrossBow = 0,// value.GapCrossBow;
                    Manual = 0,
                    Location = value.Location,
                    State = value.State,
                    Date = value.InsertDateTime
                };

                _ = this._Entity.PDIs.Add(pdi);
                value.State = (int)MsgStatus.ProcessedData;
                value.UpdateDateTime = DateTime.Now;
                value.ErrorMessage = "No Error";

                Logging.SendMessage("Insert operation on PDI has been executed. Plate ID: <" + value.PlateID + "> PKEY: <" + value.PKEY_PDI + ">", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
            } catch (Exception ex) {
                value.State = (int)MsgStatus.Error;
                value.UpdateDateTime = DateTime.Now;
                value.ErrorMessage = ex.Message;
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
            //}
            //else
            //{
            //   value.State = (int)MsgStatus.Error;
            //   value.UpdateDateTime = DateTime.Now;
            //   value.ErrorMessage = "PDI with selected PlateID <" + value.PlateID + "> already exist.";
            //}
            //newEntity.SaveChanges();
        }

        public short LastLocation() {
            try {
                this.InitEntity();

                var ttL2_L3_MODEL_TRACKING = new L2_MODEL_L3_TRACKING();

                ttL2_L3_MODEL_TRACKING = this._Entity.L2_MODEL_L3_TRACKING.OrderByDescending(u => u.PKEY_PDI).FirstOrDefault();

                return (short)ttL2_L3_MODEL_TRACKING.Location;

            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                this._Timer.Start();

                return -1;
            }
        }

        public void UpdateTracking(TrackingData tracking) {
            if (tracking.PlateID is not "" or not null) {

                try {
                    this._Timer.Stop();

                    if (this._initData.Constants.Order.Equals(nameof(OrderNumberDef.C26841216), StringComparison.Ordinal)) {
                        _ = this._PDOAdapter.SaveTracking841216(tracking);
                    } else {
                        this.InitEntity();

                        var ttL2_L3_MODEL_TRACKING = new L2_MODEL_L3_TRACKING {
                            InsertDateTime = DateTime.Now,
                            UpdateDateTime = DateTime.Now,
                            PlateID = tracking.PlateID,
                            Location = tracking.Location
                        };

                        _ = this._Entity.L2_MODEL_L3_TRACKING.Add(ttL2_L3_MODEL_TRACKING);

                        _ = this._Entity.SaveChanges();

                        Logging.SendMessage("L2/L3 tracking added", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                    }
                    this._Timer.Start();
                } catch (Exception ex) {
                    Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                    this._Timer.Start();
                }
            }
        }

        public void SendPDOToL3(PDI pdi, ProductionReport productionReport) {
            try {
                _ = this._PDOAdapter.SavePlateReport841216(pdi, productionReport);
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                this._Timer.Start();
            }
        }

        public void SendPDOToL3(string PlateID) {
            try {
                this._Timer.Stop();

                this.InitEntity();

                var list = this._Entity.ProductionReports.Where(x => x.PlateID == PlateID).ToList();
                if (list.Count > 0) {
                    if (list.Count > 0) {
                        Logging.SendMessage("Warning: Found multiple reports for the PlateID: " + PlateID + ". Found " + list.Count + " elements.", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                    }

                    var Report = list.Last();

                    Logging.SendMessage("Plate report for PlateID: " + PlateID + " has PKEY_PROD: " + Report.PKEY_PROD, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                    Console.WriteLine("Plate report for PlateID: " + PlateID + " has PKEY_PROD: " + Report.PKEY_PROD);
                    if (this._initData.Constants.Order.Equals(nameof(OrderNumberDef.C26841216), StringComparison.Ordinal)) {
                        var PDI = this._Entity.PDIs.FirstOrDefault(item => item.PlateID == PlateID);
                        if (PDI != null && PDI.PKEY_PDI > 0) {
                            _ = this._PDOAdapter.SavePlateReport841216(PDI, Report);
                        } else {
                            Logging.SendMessage("Error: Could not find Plate for Plate report with PlateID: " + PlateID + " and ProductionReport.PKEY_PROD: " + Report.PKEY_PROD, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, this.GetType());
                        }
                    } else {
                        var ttL2L3Pdo = new L2_MODEL_L3_PLATE_REPORT {
                            ProdDateTime = DateTime.Now,
                            PlateID = Report.PlateID,
                            MaterialID = Report.MaterialID,
                            Leveling = Report.Leveling,
                            State = (int)MsgStatus.New,
                            NoOfLeveling = Report.NoOfLeveling,
                            CassetteNo = Report.CassetteNo,
                            SetLevelerInlet = (decimal?)Report.SetLevelerInlet,
                            SetLevelerOutlet = (decimal?)Report.SetLevelerOutlet,
                            SetTiltLeft = (decimal?)Report.SetTiltLeft,
                            SetTiltRight = (decimal?)Report.SetTiltRight,
                            SetMiddleHeight = (decimal?)Report.SetMiddleHeight,
                            SetCurveProfile = (decimal?)Report.SetCurveProfile,
                            ActLevelerInlet = (decimal?)Report.ActLevelerInlet,
                            ActLevelerOutlet = (decimal?)Report.ActLevelerOutlet,
                            ActTiltLeft = (decimal?)Report.ActTiltLeft,
                            ActTiltRight = (decimal?)Report.ActTiltRight,
                            ActMiddleHeight = (decimal?)Report.ActMiddleHeight,
                            ActCurveProfile = (decimal?)Report.ActCurveProfile,
                            TensileStrength = (decimal?)Report.TensileStrength,
                            Temperature = (decimal?)Report.Temperature,
                            ActTemperature = (decimal?)Report.ActTemperature
                        };

                        _ = this._Entity.L2_MODEL_L3_PLATE_REPORT.Add(ttL2L3Pdo);

                        _ = this._Entity.SaveChanges();

                        Logging.SendMessage("New PDO send to L3", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                    }
                } else {
                    Logging.SendMessage("Error: Found NO production report for the PlateID: " + PlateID, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                }

                this._Timer.Start();
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                this._Timer.Start();
            }
        }

        public void Start() => this._Timer.Start();
        public void Stop() => this._Timer.Stop();

        public IPDICommunication Listener { get; set; }

        private void InitEntity() {
            try {
                this._Entity?.Dispose();

                this._Entity = new LEVELEREntities();
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void CheckL3Watchdog() {
            try {
                this.Listener.UpdateL3Watchdog();
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        #region Interface
        /// <summary>
        /// Retruns true if the PDO and PDI adapter have a connection.
        /// </summary>
        /// <returns>True, if all L3 interfaces work. False otherwise.</returns>
        public bool CheckL3Interface() {
            try {
                var connected = false;
                if (this._PDOAdapter != null) {
                    connected = this._PDOAdapter.CheckDB();
                }

                if (this._PDIAdapter != null) {
                    connected &= this._PDIAdapter.CheckDB();
                }

                return connected;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return false;
            }
        }

        public void Dispose() => throw new NotImplementedException();
        #endregion Interface
    }
}
