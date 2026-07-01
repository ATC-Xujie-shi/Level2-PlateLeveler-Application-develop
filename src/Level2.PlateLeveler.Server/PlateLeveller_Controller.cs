using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Level2.PlateLeveler.DataConverter;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataProvider;
using Level2.PlateLeveler.DataTypes;
using Level2.PlateLeveler.Model;

namespace Level2.PlateLeveler.Server {
    public class PlateLeveller_Controller : IPDICommunication, ICommunication, IL1WatchDog, IDisposable {
        #region Declarations
        private readonly ByteConverter _ByteConverter;
        //private DataAccessAdapter _DataAccessAdapter;
        private readonly InitData _Initialization;
        private readonly TelegramList _TelegramList;
        private readonly XMLConverter _XMLConverter;
        private readonly IPlateCalc _PlateCalculation;
        private readonly StartThreadCom threadCom;
        private LEVELEREntities entity;

        private readonly PreSettingsAdapter _AdapterSettings;
        //private ProductionReportAdapter _AdapterProductionReport;
        private readonly ValueConverter _ValueConverter;
        private LimitationData _Limitation;

        private ProductionReport _ProductionReport;
        private LevelerData _Levelerdata;
        private readonly LevelerData _StateData;
        private Flatness _FlatMeasureData;
        private ActualMeasureData _MeasureData;
        private readonly ActualMeasureList _Measures;
        private readonly StateData _State;
        private const string InitKey = "Init_Pfad";

        private readonly L3L2Interface _l3L2Interface;
        //private readonly XMLConverter _XMLConverter;
        private readonly StartThreadCom _ThreadCom;

        private readonly TrackingData _trackingData = new();
        private readonly L1WatchDog _Watchdog;

        private float PlastificationInputValue;

        private PDI lastGoodPDI;

        private string actualDummyPlate = "";
        private string oldDummyPlate = "";
        private string dummyMaterial = "";
        private int dummyCassette;

        private int _lasttelegramCounter = 1;
        private int _lastPLCtelegramCounter = 1;

        private enum PlateLocation {
            NewPlate = 0,
            Centering1 = 1,
            Centering2 = 2,
            Leveler = 3,
            NotUsed = 4,
            Out = 5
        }

        private const int PlastificationConst = 55;

        private enum CassetteState {
            NotActive = 0,
            Active = 1
        }

        public Cassette cassette { get; set; }
        #endregion Declarations

        #region Initialization
        public PlateLeveller_Controller() {
            this._ByteConverter = new ByteConverter();
            this._XMLConverter = new XMLConverter();
            this._PlateCalculation = new PlateCalcDummy();
            try {
                Logging.SendMessage("Starting leveler server application ...", MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());

                this._Initialization = (InitData)this._XMLConverter.ConvertFileToObject<InitData>(Functions.GetInitPath(InitKey));
                this._XMLConverter.Initialization = this._Initialization;

                //_DataAccessAdapter = new DataAccessAdapter(_Initialization.Connections[0].Namespace, _Initialization.Connections[0].ConnectionString);

                //_AdapterProductionReport = new ProductionReportAdapter(_Initialization.Connections[0]);
                this._AdapterSettings = new PreSettingsAdapter(this._Initialization);
                this._ValueConverter = new ValueConverter();
                this._Measures = [];
                this._StateData = new LevelerData();
                this._Levelerdata = new LevelerData();
                this._FlatMeasureData = new Flatness();
                this._MeasureData = new ActualMeasureData();

                this._State = new StateData();
                this.entity = new LEVELEREntities();
                this.SetLimitations();

                #region Network
                this._TelegramList = (TelegramList)this._XMLConverter.ConvertFileToObject<TelegramList>(this._Initialization.TelegramFile);
                this._TelegramList = this._XMLConverter.SetTelegramList(this._TelegramList);

                this.threadCom = new StartThreadCom(this._Initialization, this._TelegramList) {
                    Listener = this
                };
                this.threadCom.ComError += this.threadCom_ComError;
                this.threadCom.ComEstablished += this.threadCom_ComEstablished;
                this.threadCom.DataReceived += this.threadCom_DataReceived;
                this._Initialization.Communications = this.threadCom.StartThread();

                this._Watchdog = new L1WatchDog(this._Initialization) {
                    Listener = this
                };
                this._Watchdog.Start();

                this._ThreadCom = new StartThreadCom(this._Initialization, this._TelegramList);
                #endregion Network

                System.Threading.Thread.Sleep(2000);

                this._l3L2Interface = new L3L2Interface(this._Initialization) {
                    Listener = this
                };
                this._l3L2Interface.Start();

                this._trackingData.Location = this._l3L2Interface.LastTrackingLocation;

                //GetaDummyData();
                //GetDummyCassette();

                //var telegram = _TelegramList.FirstOrDefault(x => x.TelegramID == 31);
                //ManageTelegramActions(telegram);

                this._lasttelegramCounter = 1;
                this._lastPLCtelegramCounter = 1;

                Logging.SendMessage("Leveler server application is ready to go", MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }
        #endregion Initialization

        #region Telegram Handling
        private bool threadCom_DataReceived(byte[] Telegram, int index) {
            _ = this._Initialization.Communications[index];
            var telegram = this._ByteConverter.LoadTelegramFromByteArray(this._TelegramList, Telegram, this._Initialization.Communications[index]);

            if (telegram == null) {
                Logging.SendMessage("No telegram found for " + Telegram.Length + " bytes and communication index " + index + ". Will not execute telegram management function!", MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, this.GetType());
                return false;
            }

            this.ManageTelegramActions(telegram);
            return true;
        }

        public void ConnectionEstablished(bool isOn, int index = 0) {
            var dataCom = this._Initialization.Communications[index];

            var MsgText = DateTime.Now + " - Connection with: " + dataCom.Name + ": " + isOn.ToString();

            var val = isOn ? 1 : 0;

            switch (this._Initialization.Communications[index].Name) {
                case CommunicationDef.FM_L2:
                    this._State.FM = (short)val;
                    break;
                case CommunicationDef.L1_L2:
                    this._State.L1 = (short)val;
                    break;
                case CommunicationDef.L3_L2:
                    this._State.L3 = (short)val;
                    break;
                case CommunicationDef.HMI_L2:
                    //SendTelegram(95);
                    break;
            }

            Console.WriteLine(MsgText);

            Logging.SendMessage(MsgText, MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
        }

        public bool ReceiveData(byte[] arr, int index) {
            _ = this._Initialization.Communications[index];
            var telegram = this._ByteConverter.LoadTelegramFromByteArray(this._TelegramList, arr, this._Initialization.Communications[index]);

            if (telegram == null) {
                Logging.SendMessage("No telegram found for " + arr.Length + " bytes and communication index " + index + ". Will not execute telegram management function!", MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, this.GetType());
                return false;
            }

            try {
                this.LogRecievedTelegram(telegram);
                this.ManageTelegramActions(telegram);
                return true;
            } catch (Exception ex) {
                this.SuperLog(ex.Message);
                return false;
            }
        }

        private void threadCom_ComEstablished(int index) {
            switch (this._Initialization.Communications[index].Name) {
                case CommunicationDef.FM_L2:
                    this._State.FM = 1;
                    break;
                case CommunicationDef.L1_L2:
                    this._State.L1 = 1;
                    break;
                case CommunicationDef.L3_L2:
                    this._State.L3 = 1;
                    break;
                case CommunicationDef.HMI_L2:
                    this.SendTelegram(false, 95);
                    break;
            }
            this.SuperLog(index.ToString() + ": Communication '" + this._Initialization.Communications[index].Name + "' is established!");
            this.InvokeSignal(true, index);
        }

        private void threadCom_ComError(int index) {
            if (this._State != null) {
                switch (this._Initialization.Communications[index].Name) {
                    case CommunicationDef.FM_L2:
                        this._State.FM = 0;
                        break;
                    case CommunicationDef.L1_L2:
                        this._State.L1 = 0;
                        break;
                    case CommunicationDef.L3_L2:
                        this._State.L3 = 0;
                        break;
                }
                this.InvokeSignal(false, index);
            }
            this.SuperLog(index.ToString() + ": Communication '" + this._Initialization.Communications[index].Name + "' is lost!");
        }

        [STAThread]
        private void ManageTelegramActions(TelegramData telegram) {
            var dataPlate = new PlateData();
            var pdi = new PDI();
            var _SettingsData = new SettingsData();
            var signal = new SignalData();
            var cassette = new Cassette();
            var _ProductionReport = new ProductionReport();
            var track = new TrackingData();
            this.entity = new LEVELEREntities();
            var tel = new TelegramData();
            var _ProductionList = new List<ProductionReport>();

            try {
                switch (telegram.TelegramID) {
                    #region L3_L2_SignOfLife
                    case 1:
                        break;
                    #endregion
                    #region L3_L2_PDI
                    case 2:
                        break;
                    #endregion
                    #region L1_L2_SignOfLife
                    case 30:

                        _ = this._State?.L1 = (int)ConnectionStatus.OK;

                        //UpdateHMIWatchDog();

                        //if (_State != null)
                        //{
                        //   _State.L1 = (int)ConnectionStatus.NOT_OK;
                        //}
                        break;
                    #endregion
                    #region L1_L2_RPDI
                    case 31:
                        dataPlate = (PlateData)this._XMLConverter.ConvertTelegramToObject<PlateData>(telegram);
                        var forcedplate = false;

                        this.SuperLog("REQUEST PLATE DATA INFORMATION TELEGRAM: Plate ID from PLC is " + dataPlate.PlateID);

                        if (dataPlate.PlateID is "" or "0") {
                            this.GetDummyData();
                            dataPlate.PlateID = this.actualDummyPlate;
                            if (string.IsNullOrEmpty(this.actualDummyPlate)) {
                                this.SendTelegram(false, TelegramDef.L2_HMI_MissingPDI);
                                this.SuperLog("There is no available PDI!");

                                this.SendEmptyPlate();
                            } else {
                                this.SuperLog("PlateID from PLC was empty! New next found PlateID is: " + this.actualDummyPlate);
                            }
                        } else {
                            var firstPdi = this.entity.PDIs.FirstOrDefault(x => x.PlateID == dataPlate.PlateID);// && x.Location < (short)PlateLocation.Leveler);
                            if (firstPdi == null) {
                                this.SuperLog("PDI with received PlateID: " + dataPlate.PlateID + " from PLC not found!");
                                this.GetDummyData();
                                if (string.IsNullOrEmpty(this.actualDummyPlate)) {
                                    this.SuperLog("there is no available pdi in the database");
                                    this.SendTelegram(false, TelegramDef.L2_HMI_MissingPDI);
                                } else {
                                    this.SuperLog("found available pdi with ID: " + this.actualDummyPlate);
                                }
                            } else {
                                this.oldDummyPlate = this.actualDummyPlate;
                                this.actualDummyPlate = dataPlate.PlateID;
                                this.SuperLog("PDI with received PlateID: " + dataPlate.PlateID + " from PLC found!");
                                forcedplate = true;
                            }
                        }
                        if (string.IsNullOrEmpty(this.actualDummyPlate)) {
                            this.SuperLog(">>>>>>>>>ActualDummyPlate = null<<<<<<<<<<<<<");
                            this.UpdatePlateLocationsInDatabase(this.actualDummyPlate);
                            break;
                        }

                        this.SuperLog("Actual Dummy Plate: " + this.actualDummyPlate);
                        this.SuperLog("Old Dummy Plate: " + this.oldDummyPlate);

                        pdi = this.RequestPDI(dataPlate, forcedplate);
                        _ProductionList.Clear();
                        _ProductionList = [.. this.entity.ProductionReports.Where(p => p.PlateID == pdi.PlateID)];
                        if (_ProductionList.Count > 0) {
                            _ProductionReport = _ProductionList.Last();
                        } else { break; }

                        this.AdjustmentCalculation(this.actualDummyPlate, _ProductionReport);
                        //SendTelegram<PDI>(TelegramDef.L2_L3_Tracking, pdi, false);

                        var oldPlate = this.entity.PDIs.FirstOrDefault(x => x.PlateID == this.oldDummyPlate);
                        this.SendTelegram(true, TelegramDef.L2_HMI_Tracking, oldPlate, false);
                        //SendTelegram<PDI>(true, TelegramDef.L2_HMI_Tracking, pdi, false);

                        this.SuperLog("L1_L2_RPDI with PlateID:" + dataPlate.PlateID + " sent to PLC!");
                        this.UpdatePlateLocationsInDatabase(this.actualDummyPlate);

                        break;
                    #endregion
                    #region L1_L2_RADJ
                    case 32:
                        var adj = (AdjustmentData)this._XMLConverter.ConvertTelegramToObject<AdjustmentData>(telegram);

                        this.entity = new LEVELEREntities();

                        this.SuperLog("L1_L2_RADJ with PlateID:" + adj.PlateID);

                        _ProductionList.Clear();
                        _ProductionList = [.. this.entity.ProductionReports.Where(p => p.PlateID == adj.PlateID)];
                        if (_ProductionList.Count > 0) {
                            _ProductionReport = _ProductionList.Last();
                        } else {
                            this.SuperLog("Error: Received L1_L2_RADJ and could not find ProductionReport information for Plate ID: " + adj.PlateID);
                            break;
                        }

                        this.AdjustmentCalculation(adj.PlateID, _ProductionReport);
                        break;
                    #endregion
                    #region L1_L2_PlateState
                    case 33:
                        // Actual Leveler Data are saved into Production Report
                        this._Levelerdata = (LevelerData)this._XMLConverter.ConvertTelegramToObject<LevelerData>(telegram);

                        this.LogPlateStateData(this._Levelerdata);

                        this.entity = new LEVELEREntities();
                        pdi = this.entity.PDIs.FirstOrDefault(item => item.PlateID == this._Levelerdata.PlateID);
                        //pdi.Location = (int)PlateLocation.Out;

                        if (pdi == null) {
                            this.SuperLog("Error: Received L1_L2_PlateState and could not find PDI plate information for Plate ID: " + this._Levelerdata.PlateID);
                            break;
                        }

                        _ProductionList.Clear();
                        _ProductionList = [.. this.entity.ProductionReports.Where(p => p.PlateID == pdi.PlateID)];
                        if (_ProductionList.Count > 0) {
                            _ProductionReport = _ProductionList.Last();
                            if (!_ProductionReport.MaterialID.Equals(this._Levelerdata.MaterialID, StringComparison.Ordinal)) {
                                this.SuperLog("Warning: Received L1_L2_PlateState and found " + _ProductionList.Count + " existing ProductionReport(s) for plate ID '" + pdi.PlateID + "' but the material ID was different (existing ID: " + _ProductionReport.MaterialID + ", received ID: " + this._Levelerdata.MaterialID + ")");
                            }

                            if (_ProductionReport.ProdDate == null) {
                                _ProductionReport.ProdDate = DateTime.Now;
                                if (this._Levelerdata.Leveling == 1) {
                                    // When the Leveling values is set to 1, the plate starts leveling and the time-stamp can be used to save this point in time
                                    this.SuperLog("Info: Received L1_L2_PlateState and added missing production date to production report with Pkey:" + _ProductionReport.PKEY_PROD + " for plate ID " + pdi.PlateID + ". The added Time is the start time of the plate.");
                                } else {
                                    this.SuperLog("Info: Received L1_L2_PlateState and added missing production date to production report with Pkey:" + _ProductionReport.PKEY_PROD + " for plate ID " + pdi.PlateID);
                                }
                            }
                        } else {
                            this.SuperLog("Warning: Received L1_L2_PlateState and found no existing ProductionReport for plate ID '" + pdi.PlateID + "'. only actual values can be stored and send to L3. Set-Values are missing!");

                            _ProductionReport = new ProductionReport {
                                PlateID = this._Levelerdata.PlateID,
                                MaterialID = this._Levelerdata.MaterialID,
                                ProdDate = DateTime.Now
                            };
                            _ = this.entity.ProductionReports.Add(_ProductionReport);
                        }
                        _ProductionReport.SetLevelerData(this._Levelerdata);
                        _ = this.entity.SaveChanges();

                        //_ProductionReport.Temperature = 0; // This values seems not to be set anywhere else, so it will always be zero. L3 does not need this values, so it can be zero.

                        if (this._Levelerdata.Leveling == 0) {
                            // Create Plate Report and send it to L3. Only send the report if Leveling == 0 because I get the telegram twice!
                            this._l3L2Interface.SendPDOToL3(pdi, _ProductionReport);
                        }

                        if (this._Initialization.Interval.PredictModel > 0) {
                            this.CheckWithPredictionModel(this._Levelerdata.LevelerInlet, this._Levelerdata.LevelerOutlet, this._Levelerdata.PlateID);
                        }

                        break;
                    #endregion
                    #region L1_L2_Tracking
                    case 34:
                        track = (TrackingData)this._XMLConverter.ConvertTelegramToObject<TrackingData>(telegram);

                        if (track.Location == (int)PlateLocation.Centering2) {
                            break;
                        }

                        if (track.Location == 0) {
                            track.Location = (int)PlateLocation.Out;
                            if (!string.IsNullOrEmpty(track.PlateID)) {
                                pdi = this.entity.PDIs.FirstOrDefault(item => item.PlateID == track.PlateID && item.Location == (int)PlateLocation.Centering2);

                                if (pdi != null) {
                                    this.SuperLog("TrackingData.PlateID: " + track.PlateID);
                                    this.SuperLog("OUT: PDI with PlateID: " + pdi.PlateID + ", location: " + pdi.Location + " set to new location: " + track.Location);
                                    pdi.Location = track.Location;
                                } else {
                                    Logging.SendMessage("Warning: Trying to move plate '" + track.PlateID + "' to the out location but it was not found in the leveler location! Nothing was done.", MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                                }
                            }
                        } else if (track.Location == (int)PlateLocation.Leveler) {
                            if (!string.IsNullOrEmpty(track.PlateID)) {
                                pdi = this.entity.PDIs.FirstOrDefault(item => item.PlateID == track.PlateID && item.Location < (int)PlateLocation.Leveler);
                                this.SuperLog("LEVELER: PDI with PlateID: " + pdi.PlateID + ", location: " + pdi.Location + " set to new location: " + track.Location);
                                pdi.Location = track.Location;
                            }
                        } else if (track.Location == (int)PlateLocation.Centering1) {
                            pdi = this.entity.PDIs.FirstOrDefault(item => item.Location < (short)PlateLocation.Centering1);
                            if (pdi != null) {
                                pdi.Location = track.Location;
                                track.PlateID = pdi.PlateID;
                            }
                        }

                        if (track.PlateID is not null and not "") {
                            this._l3L2Interface.UpdateTracking(track);
                        }
                        this.entity.ChangeTracker.DetectChanges();
                        var changedObjectCount = this.entity.SaveChanges();
                        Logging.SendMessage("Info: Received L1_L2_Tracking and updated " + changedObjectCount + " objects in DB", MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                        //SendTelegram<TrackingData>(TelegramDef.L2_L3_Tracking, track, false);
                        oldPlate = this.entity.PDIs.FirstOrDefault(X => X.PlateID == this.oldDummyPlate);
                        this.SendTelegram(false, TelegramDef.L2_HMI_Tracking, oldPlate, false);
                        break;
                    #endregion
                    #region L1_L2_ActVal
                    case 35:
                        this._MeasureData = (ActualMeasureData)this._XMLConverter.ConvertTelegramToObject<ActualMeasureData>(telegram);
                        //_MeasureData = (ActualMeasureData)_XMLConverter.ConvertTelegramToObjectWithArrays<ActualMeasureData, float>(telegram, "a");

                        var listValue = new List<ActualValue>();
                        try {
                            pdi = this.entity.PDIs.FirstOrDefault(item => item.PlateID == this._MeasureData.PlateID);
                            if (pdi == null) {
                                this.SuperLog("Error: Received L1_L2_ActVal with PlateID:" + this._MeasureData.PlateID + " but could not find the plate in DB!");
                                break;
                            } else {
                                listValue = [.. this.entity.ActualValues.Where(av => av.PlateID == this._MeasureData.PlateID && av.MaterialID == pdi.MaterialID)];
                                if (this._Measures.Count < 20) {
                                    this._Measures.Add(this._MeasureData);
                                }

                                this._Measures.Plate = pdi;
                            }
                        } catch (Exception ex) {
                            Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                        }
                        var actVal = new ActualValue();
                        actVal.SetActualValue(this._MeasureData, listValue.Count + 1);
                        actVal.MaterialID = pdi.MaterialID;
                        _ = this.entity.ActualValues.Add(actVal);
                        _ = this.entity.SaveChanges();

                        this.SuperLog("L1_L2_ActVal with PlateID:" + this._MeasureData.PlateID + " handled");
                        break;
                    #endregion
                    #region L1_L2_Meso
                    case 36:
                        this.SuperLog(telegram.Name);
                        dataPlate = (PlateData)this._XMLConverter.ConvertTelegramToObject<PlateData>(telegram);

                        pdi = this.entity.PDIs.FirstOrDefault(item => item.PlateID == dataPlate.PlateID);
                        pdi = (PDI)this._ValueConverter.ChangeFloatToInt100(pdi);

                        //SendTelegram<PDI>(TelegramDef.L2_FM_Meso, pdi, false);
                        this.SuperLog("L1_L2_Meso with PlateID:" + dataPlate.PlateID + " handled");
                        break;
                    #endregion
                    #region L1_L2_ActiveCassette
                    case 37:
                        this.SuperLog(telegram.Name);
                        var dataCassette = (CassetteData)this._XMLConverter.ConvertTelegramToObject<CassetteData>(telegram);

                        var cas = this.entity.Cassettes.FirstOrDefault(c => c.CassetteNo == dataCassette.CassetteNo);

                        if (cas != null) {
                            foreach (var item in this.entity.Cassettes) {
                                item.Active = (short)CassetteState.NotActive;
                            }
                            cas.Active = (short)CassetteState.Active;
                            this.dummyCassette = cas.CassetteNo;
                            _ = this.entity.SaveChanges();
                            this.SendSignalToHMI(SignalDef.ActiveCassette, dataCassette.CassetteNo);
                            this.SuperLog("L1_L2_ActiveCassette with CassetteNo:" + cas.CassetteNo + " handled");
                        } else {
                            this.SuperLog("CassetteNo:" + dataCassette.CassetteNo + " not found!");
                        }

                        break;
                    #endregion
                    #region L1_L2_LineState
                    case 39:
                        this.SuperLog(telegram.Name);
                        var ls = (LineState)this._XMLConverter.ConvertTelegramToObject<LineState>(telegram);
                        ls.Date = DateTime.Now;
                        _ = this.entity.LineStates.Add(ls);
                        _ = this.entity.SaveChanges();

                        this.SuperLog("L1_L2_LineState handled");
                        break;
                    #endregion
                    #region FM_L2_SignOfLife
                    case 70:
                        //if (_Initialization.Communications[telegram.ComIndex].LogLiveTelegram)
                        //   Logging.SendMessage(telegram.Name + " was sent!",System.Reflection.MethodInfo.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                        //SendTelegram(TelegramDef.L2_FM_SignOfLife);
                        //SendSignalToHMI(SignalDef.FM, 1);
                        break;
                    #endregion
                    #region FM_L2_MES
                    case 71:
                        break;

                        //telegram.TelegramBlocks[telegram.TelegramBlocks.Count - 1].Value = "";
                        this.SuperLog(telegram.Name);
                        this._FlatMeasureData = (Flatness)this._XMLConverter.ConvertTelegramToObject<Flatness>(telegram);
                        this._FlatMeasureData.MaterialID = "";
                        this.SendTelegram(true, TelegramDef.L2_L1_MES, this._FlatMeasureData, false);
                        try {
                            pdi = this.entity.PDIs.Single(item => item.PlateID == this._FlatMeasureData.PlateID);
                            pdi.MeasuringCode = Convert.ToInt16(this._FlatMeasureData.MeasuringCode);
                            this._FlatMeasureData.MaterialID = pdi.MaterialID;
                        } catch { pdi = new PDI(); }

                        _ProductionReport = new ProductionReport();
                        // Save measure data to Production Report table
                        try {
                            _ProductionReport = this.entity.ProductionReports.Single(p => p.PlateID == this._FlatMeasureData.PlateID && p.MaterialID == pdi.MaterialID);
                            _ProductionReport = this.entity.ProductionReports.Local[0];
                            _ProductionReport.SetFlatnessData(this._FlatMeasureData);
                        } catch {
                        }

                        try {
                            var flatness = this.entity.Flatnesses.Single(f => f.PlateID == this._FlatMeasureData.PlateID && f.MaterialID == pdi.MaterialID);
                            this.entity = new LEVELEREntities();
                            _ = this.entity.Flatnesses.Attach(flatness);
                            _ = this.entity.Flatnesses.Remove(flatness);
                            _ = this.entity.SaveChanges();
                        } catch {
                        }

                        this.SendTelegram(false, TelegramDef.L2_L3_MES, this._FlatMeasureData, false);
                        _ = this.entity.Flatnesses.Add(this._FlatMeasureData);
                        this._FlatMeasureData = (Flatness)this._ValueConverter.ChangeIntToFloat100(this._FlatMeasureData);
                        _ = this.entity.SaveChanges();
                        // Check, if plate is in tolerance
                        var bTol = pdi.Ruler1MCrossPDI > _ProductionReport.Act1MRulerCross.Value && pdi.Ruler1MLengthPDI > _ProductionReport.Act1MRulerLength.Value && pdi.Ruler2MLengthPDI > _ProductionReport.Act2MRulerLength.Value;

                        // if plate is not in tolerance --> fault compensation
                        if (Convert.ToBoolean(this._Limitation.IsFaultCompensation)) {
                            if (!bTol) {
                                var faultComp = new FaultCompensation();
                                _SettingsData.CurveProfile = _ProductionReport.ActCurveProfile ?? 0;
                                _SettingsData.LevelerInlet = _ProductionReport.ActLevelerInlet ?? 0;
                                _SettingsData.LevelerOutlet = _ProductionReport.ActLevelerOutlet ?? 0;
                                _SettingsData.MiddleHeight = _ProductionReport.ActMiddleHeight ?? 0;
                                _SettingsData.TiltLeft = _ProductionReport.ActTiltLeft ?? 0;
                                _SettingsData.TiltRight = _ProductionReport.ActTiltRight ?? 0;

                                try {
                                    var activeCassette = this.entity.Cassettes.Single(c => c.Active == (short)CassetteState.Active);
                                    faultComp = this.entity.FaultCompensations.Single(fc => fc.CassetteNo == activeCassette.CassetteNo);
                                    faultComp.Priorities = this._AdapterSettings.LoadPriorities(faultComp);
                                    faultComp.Priorities.SortList();
                                    var bVariation = false;

                                    throw new NotImplementedException();
                                    // Calculation .dll required
                                    //_SettingsData = PlateCalc.CalcFaultCompansation(_SettingsData, faultComp, _ProductionReport, out bVariation);

                                    if (bVariation) {
                                        this.SendTelegram(true, TelegramDef.L2_L1_ADJ, _SettingsData, false);
                                        IEnumerable<ActualValue> avList = this.entity.ActualValues.Where(av => av.PlateID == this._FlatMeasureData.PlateID);
                                        foreach (var item in avList) {
                                            _ = this.entity.ActualValues.Remove(item);
                                        }

                                        _ProductionReport.NoOfLeveling = _ProductionReport.NoOfLeveling.HasValue ? _ProductionReport.NoOfLeveling.Value + 1 : 1;
                                        _ = this.entity.SaveChanges();
                                    }
                                } catch { }
                            }
                        }
                        // Save Production
                        break;
                    #endregion
                    #region HMI_L2_SignOfLife
                    case 90:
                        //if(_Initialization.Communications[telegram.ComIndex].LogLiveTelegram)
                        //   Logging.SendMessage(telegram.Name + " was sent!",System.Reflection.MethodInfo.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                        //SendTelegram(95);
                        //Process process = Process.GetCurrentProcess();
                        //_CPUCounter = new PerformanceCounter("Process", "% Processor Time", process.ProcessName);
                        //_CPUCounter.NextValue();
                        break;
                    #endregion
                    #region HMI_L2_Request
                    case 91:
                        this.SuperLog(telegram.Name);
                        dataPlate = (PlateData)this._XMLConverter.ConvertTelegramToObject<PlateData>(telegram);
                        _ = this.RequestPDI(dataPlate);
                        break;
                    #endregion
                    #region HMI_L2_Limitation
                    case 92:
                        this.SetLimitations();
                        break;
                        #endregion
                }
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }
        #endregion Telegram Handling

        #region Telegram Sending
        public void L2_L1_Watchdog() {
            if (this._State != null) {
                var telegram = this._TelegramList.FirstOrDefault(t => t.Name == TelegramDef.L2_L1_SignOfLife.ToString());

                this._State.DB = 1;
                this._State.FM = 1;

                telegram = this._XMLConverter.ConvertObjectToTelegram(this._State, telegram);
                this.SetTelegramCounter(true, ref telegram);
                this.SendTelegram(telegram);
            }
        }

        public void SendDatabaseUpdateToHMI() => this.SendTelegram(false, TelegramDef.L2_HMI_DatabaseUpdate);

        public void UpdateL3Watchdog() {
            if (this._State != null) {
                this._State.L3 = this._l3L2Interface.CheckL3Interface() ? (short)(int)ConnectionStatus.OK : (short)(int)ConnectionStatus.NOT_OK;

                var entity = new LEVELEREntities();
                this._State.DB = entity.Database.Exists() ? (short)(int)ConnectionStatus.OK : (short)(int)ConnectionStatus.NOT_OK;
            }

            this.UpdateHMIWatchDog();

            if (this._State != null) {
                this._State.L3 = (int)ConnectionStatus.NOT_OK;
                this._State.DB = (int)ConnectionStatus.NOT_OK;
            }
        }

        private void UpdateHMIWatchDog() {
            if (this._State != null) {
                this.SendSignalToHMI(SignalDef.L1, this._State.L1);
                this.SendSignalToHMI(SignalDef.L3, this._State.L3);
                this.SendSignalToHMI(SignalDef.DB, this._State.DB);
            } else {
                this.SendSignalToHMI(SignalDef.L1, (int)ConnectionStatus.NOT_OK);
                this.SendSignalToHMI(SignalDef.L3, (int)ConnectionStatus.NOT_OK);
                this.SendSignalToHMI(SignalDef.DB, (int)ConnectionStatus.NOT_OK);
            }
        }

        private void SendSignalToHMI(bool flag, int index) {
            var signal = new SignalData {
                Flag = Convert.ToInt32(flag),
                Signal = this._Initialization.Communications[index].Name switch {
                    CommunicationDef.L3_L2 => SignalDef.L3,
                    CommunicationDef.L2_L3 => SignalDef.L3,
                    CommunicationDef.L1_L2 => SignalDef.L1,
                    CommunicationDef.L2_L1 => SignalDef.L1,
                    CommunicationDef.FM_L2 => SignalDef.FM,
                    CommunicationDef.HMI_L2 => throw new NotImplementedException(),
                    CommunicationDef.S7_L2 => throw new NotImplementedException(),
                    CommunicationDef.Events_L2 => throw new NotImplementedException(),
                    _ => SignalDef.HMI,
                }
            };
            this.SendTelegram(false, TelegramDef.L2_HMI_Signal, signal, false);
        }

        private void InvokeSignal(bool flag, int index) {
            var del = new Action(delegate () {
                this.SendSignalToHMI(flag, index);
            });
            del.Invoke();
        }

        private void SendEmptyPlate() {
            if (this.lastGoodPDI != null) {
                this.lastGoodPDI.PlateID = "";
                this.SendTelegram(true, TelegramDef.L2_L1_PDI, this.lastGoodPDI, false);
            }
        }

        private void SendSignalToHMI(SignalDef signal, int flag) {
            var data = new SignalData {
                Signal = signal,
                Flag = flag
            };
            this.SendTelegram(false, TelegramDef.L2_HMI_Signal, data, false);
        }

        private void SendTelegram(bool isForPLC, TelegramDef telName) {
            var tel = this._TelegramList.GetItem(telName.ToString());
            this.SetTelegramCounter(isForPLC, ref tel);
            this.SetTypeIfExists(ref tel, tel.TelegramID);
            this.SendTelegram(tel);
        }

        private void SendTelegram<T>(bool isForPLC, TelegramDef telegramName, T obj, bool bWithArray) {
            var tel = this._TelegramList.GetItem(telegramName.ToString());
            tel = bWithArray
                ? this._XMLConverter.ConvertObjectToTelegramWithArray(obj, tel)
                : this._XMLConverter.ConvertObjectToTelegram(obj, tel);

            this.SetTelegramCounter(isForPLC, ref tel);
            if (tel.TelegramID is not 20 and not 30 and not 97) {
                this.SuperLog(tel.Name);
            }

            this.SetTypeIfExists(ref tel, tel.TelegramID);

            this.SendTelegram(tel);
        }

        private void SendTelegram(bool isForPLC, int telegramIndex) {
            var tel = this._TelegramList.GetItem(telegramIndex);
            this.SetTelegramCounter(isForPLC, ref tel);
            this.SendTelegram(tel);
        }

        private void SendTelegram(TelegramData telegram) {
            var btArr = this._ByteConverter.ConvertTelegramToByteArray(telegram, this._Initialization.Communications[telegram.ComIndex]);
            this.threadCom.SendByteArray(btArr, telegram.ComIndex);
        }

        private void SetTelegramCounter(bool isForPLC, ref TelegramData data) {
            _ = new Random();
            foreach (var item in data.TelegramValues) {
                if (item.Name.Equals("TelegramCounter", StringComparison.Ordinal)) {
                    if (isForPLC) {
                        this._lastPLCtelegramCounter++;

                        if (this._lastPLCtelegramCounter > 10000) {
                            this._lastPLCtelegramCounter = 1;
                        }
                        item.Value = this._lastPLCtelegramCounter;
                    } else {
                        this._lasttelegramCounter++;

                        if (this._lasttelegramCounter > 10000) {
                            this._lasttelegramCounter = 1;
                        }
                        item.Value = this._lasttelegramCounter;// (short)rnd.Next(10000);
                    }
                }
            }
        }

        private void SetTypeIfExists(ref TelegramData data, int TelegramID) {
            foreach (var item in data.TelegramValues) {
                if (item.Name.Equals("TelegramType", StringComparison.Ordinal)) {
                    item.Value = TelegramID;
                }
            }
        }
        #endregion Telegram Sending

        #region Data
        private void GetDummyCassette() {
            var firstCassette = this.entity.Cassettes.FirstOrDefault(cs => cs.Active == (short)CassetteState.Active);
            if (firstCassette != null) {
                this.dummyCassette = firstCassette.CassetteNo;
                this.SuperLog("Active cassette number: " + this.dummyCassette);
            } else {
                this.dummyCassette = 0;
                this.SuperLog("Active cassette not found!");
            }
        }

        private void GetDummyData() {
            this.entity = new LEVELEREntities();

            var firstPdi = this.entity.PDIs.FirstOrDefault(x => x.Location < (short)PlateLocation.Leveler);
            if (firstPdi != null) {
                this.oldDummyPlate = this.actualDummyPlate;
                this.actualDummyPlate = firstPdi.PlateID;
                this.dummyMaterial = firstPdi.MaterialID;
                this.SuperLog("Found new PDI: Old plate ID: " + this.oldDummyPlate + " New plate ID: " + this.actualDummyPlate);
            } else {
                this.oldDummyPlate = this.actualDummyPlate;
                this.actualDummyPlate = "";
            }

            //TelegramData firstTelegram = new TelegramData();
            //firstTelegram.TelegramID = 31;
            //firstTelegram.Name = "L1_L2_RPDI";

            //TelegramData secondTelegram = new TelegramData();
            //secondTelegram.TelegramID = 32;
            //secondTelegram.Name = "L1_L2_RADJ";

            //ManageTelegramActions(firstTelegram);
            //ManageTelegramActions(secondTelegram);

        }

        private void UpdatePlateLocationsInDatabase(string currentPlate) {
            try {
                var entity = new LEVELEREntities();

                var plateList = entity.PDIs.Where(item => item.Location == (int)PlateLocation.Leveler).ToList();
                if (plateList != null) {
                    foreach (var plate in plateList) {
                        if (plate.PlateID != currentPlate) {
                            plate.Location = (int)PlateLocation.Out;
                            this.SuperLog("Plate Location for plate " + plate.PlateID + "Updated!");
                        }
                    }
                }
                _ = entity.SaveChanges();
                this.SendDatabaseUpdateToHMI();
                this.SuperLog(">>>>>>Saved to database!<<<<<<<<");
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private SettingsData GetInitialSettingData(string PlateID) {
            var _itm = new SettingsData();

            var setting = new PreSetting();

            var pdi = this.entity.PDIs.FirstOrDefault(item => item.PlateID == PlateID);
            this.cassette ??= this.entity.Cassettes.FirstOrDefault(cs => cs.Active == (short)CassetteState.Active);

            setting = this.entity.PreSettings.FirstOrDefault(ps => ps.CassetteNo == this.cassette.CassetteNo && ps.StartRangeThickness <= pdi.Thickness && ps.EndRangeThickness > pdi.Thickness && ps.StartRangeWidth <= pdi.Width && ps.EndRangeWidth >= pdi.Width && ps.MaterialID == pdi.SteelGrade && ps.Enable == 1);

            if (setting == null) {
                setting = new PreSetting();
                var increment = 0.5F;
                var initv = pdi.Thickness.Value - (pdi.Thickness.Value % increment);
                setting.CassetteNo = this.cassette.CassetteNo;
                setting.MaterialID = pdi.SteelGrade;
                setting.CorrPlastification = PlastificationConst;
                setting.Enable = 1;
                setting.StartRangeThickness = initv;
                setting.EndRangeThickness = initv + increment;
                setting.StartRangeWidth = 800;
                setting.EndRangeWidth = 2000;
                setting.CenterHeightBending = 0;
                setting.CurveProfile = 0;
                setting.TiltingLeft = 2;
                setting.TiltingRight = 2;
                _ = this.entity.PreSettings.Add(setting);
                _ = this.entity.SaveChanges();
            } else {
                this.SuperLog("___________________SETTINGS PLASTIFICATION: " + setting.CorrPlastification + "_______________________");
            }

            _itm.SetSettingsData(setting, pdi);

            return _itm;
        }

        private Cassette GetInitialCasseteData() {
            var _cassette = this.entity.Cassettes.FirstOrDefault(item => item.Active == 1);

            return _cassette;
        }

        private PreSettingsOutlet GetInitialPreSettingsOutletData(string PlateID) {
            var outlet = new PreSettingsOutlet();
            var pdi = this.entity.PDIs.FirstOrDefault(item => item.PlateID == PlateID);
            outlet = this.entity.PreSettingsOutlets.FirstOrDefault(po => po.EndRangeActYieldPoint.Value > pdi.YieldPoint.Value && po.StartRangeActYieldPoint <= pdi.YieldPoint && po.StartRangeThickness <= pdi.Thickness && po.EndRangeThickness > pdi.Thickness);

            return outlet;
        }

        private void UpdatePresetOutData(float correctionFactor, string PlateID) {
            var outlet = new PreSettingsOutlet();
            var pdi = this.entity.PDIs.FirstOrDefault(item => item.PlateID == PlateID);
            outlet = this.entity.PreSettingsOutlets.FirstOrDefault(po => po.EndRangeActYieldPoint.Value > pdi.YieldPoint.Value && po.StartRangeActYieldPoint <= pdi.YieldPoint && po.StartRangeThickness <= pdi.Thickness && po.EndRangeThickness > pdi.Thickness);

            if (outlet == null) {
                outlet = new PreSettingsOutlet {
                    StartRangeThickness = pdi.Thickness - (pdi.Thickness % 0.5f)
                };
                outlet.EndRangeThickness = outlet.StartRangeThickness + 0.5f;
                outlet.StartRangeActYieldPoint = pdi.YieldPoint - (pdi.YieldPoint % 100);
                outlet.EndRangeActYieldPoint = outlet.StartRangeActYieldPoint + 100;
                outlet.Offset = 0;

                this.SuperLog("Warning: Could not find PreSettingsOutlet and created new one...");
                _ = this.entity.PreSettingsOutlets.Add(outlet);
            }

            outlet.Offset += correctionFactor;
            this.SuperLog("PreSettingsOutlet updated... PKEY <" + outlet.Pkey_SettingsOutlet + ">, Correction: <" + correctionFactor + ">, New Correction factor: <" + outlet.Offset + ">");

            _ = this.entity.SaveChanges();
        }

        private void UpdatePresetData(double plastificationResult, string PlateID) {
            try {
                var setting = new PreSetting();

                var pdi = this.entity.PDIs.FirstOrDefault(item => item.PlateID == PlateID);

                setting = this.entity.PreSettings.FirstOrDefault(ps => ps.CassetteNo == this.cassette.CassetteNo && ps.StartRangeThickness <= pdi.Thickness && ps.EndRangeThickness > pdi.Thickness && ps.StartRangeWidth <= pdi.Width && ps.EndRangeWidth >= pdi.Width && ps.MaterialID == pdi.SteelGrade && ps.Enable == 1);
                setting.CorrPlastification = (float)plastificationResult;

                this.SuperLog("PreSettings updated... PKEY <" + setting.PKEY_SETTINGS + ">");

                _ = this.entity.SaveChanges();

            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }

        private void CheckWithPredictionModel(float targetValue, float targetOutletValue, string PlateId) {
            try {
                if (!string.IsNullOrEmpty(PlateId)) {
                    this.SuperLog("Predicting for PlateID: " + PlateId + " with target: " + targetValue);

                    double reqPlast = 0;

                    var predModel = new PredictionModel(this._PlateCalculation, this._Initialization.Constants.StoodBolt) {
                        MaxAllowedError = this._Initialization.Interval.PredictionError,
                        TargetValue = targetValue,
                        InitialSettingsData = this.GetInitialSettingData(PlateId)
                    };

                    reqPlast = predModel.InitialSettingsData.Plastification;
                    predModel.InitialCassette = this.GetInitialCasseteData();
                    predModel.InitialPreSettingsOutlet = this.GetInitialPreSettingsOutletData(PlateId);
                    predModel.InitialLimitationData = this._Limitation;

                    var plastificationResult = predModel.GetPredictedPlastification(out var levelerINResult, out var levelerOutResult);

                    var updateResult = Math.Abs(reqPlast - plastificationResult);

                    this.SuperLog("Request Inlet: <" + targetValue + "> Error: <" + this._Initialization.Interval.PredictionError + "> Requested plastification: <" + reqPlast + "> Predicted plastification: <" + plastificationResult + ">");

                    if (updateResult > 0.5) {
                        this.UpdatePresetData(plastificationResult, PlateId);
                    } else {
                        this.SuperLog("Plastification update not required");
                    }

                    double updateOutletResult = Math.Abs(levelerOutResult - targetOutletValue);

                    if (updateOutletResult > 0) {
                        this.UpdatePresetOutData(targetOutletValue - levelerOutResult, PlateId);
                    } else {
                        this.SuperLog("Outlet correction not required");
                    }
                }
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }

        private void AdjustmentCalculation(string PlateID, ProductionReport _newProuctionReport) {
            var bSend = false;
            var _SettingsData = new SettingsData();
            this._ProductionReport = _newProuctionReport;
            this.GetDummyCassette();

            if (PlateID != null) {
                var pdi = this.entity.PDIs.FirstOrDefault(item => item.PlateID == PlateID);

                if (pdi != null) {
                    var adjustment = new AdjustmentData(pdi) {
                        CassetteNo = (short)this.dummyCassette,
                        EModule = this._Limitation.EModule
                    };
                    pdi.EModule = adjustment.EModule.Value;
                    pdi.YieldPoint = pdi.YieldPoint.Value;
                    adjustment.YieldPoint = pdi.YieldPoint;

                    if (pdi.YieldPoint > 0) {
                        adjustment.Plastification = this._Limitation.Plastification;

                        this.cassette = this.entity.Cassettes.FirstOrDefault(item => item.CassetteNo == adjustment.CassetteNo);
                        if (this.cassette != null) {
                            // in the first leveling
                            var setting = new PreSetting();

                            setting = this.entity.PreSettings.FirstOrDefault(ps => ps.CassetteNo == this.cassette.CassetteNo && ps.StartRangeThickness <= pdi.Thickness && ps.EndRangeThickness > pdi.Thickness && ps.StartRangeWidth <= pdi.Width && ps.EndRangeWidth >= pdi.Width && ps.MaterialID == pdi.SteelGrade && ps.Enable == 1);
                            if (setting != null) {
                                _SettingsData.SetSettingsData(setting, pdi);
                            } else {
                                setting = new PreSetting();
                                _SettingsData.SetSettingsData(setting, pdi);
                            }

                            var outlet = new PreSettingsOutlet();

                            outlet = this.entity.PreSettingsOutlets.FirstOrDefault(po => po.EndRangeActYieldPoint.Value > pdi.YieldPoint.Value && po.StartRangeActYieldPoint <= pdi.YieldPoint && po.StartRangeThickness <= pdi.Thickness && po.EndRangeThickness > pdi.Thickness);
                            outlet ??= new PreSettingsOutlet {
                                Offset = 0
                            };

                            this.SuperLog("PreSettingsOutlet Offset: " + outlet.Offset);

                            if (_SettingsData.Plastification == 0) {
                                _SettingsData.Plastification = this._Limitation.Plastification;
                            }
                            this.PlastificationInputValue = _SettingsData.Plastification;

                            bSend = pdi.Width >= this._Limitation.MinWidth && pdi.Width <= this._Limitation.MaxWidth;
                            bSend = bSend && pdi.YieldPoint <= this._Limitation.MaxYieldPoint;

                            if (_SettingsData != null) {
                                if (bSend) {
                                    var bLeveling = pdi.Thickness >= this.cassette.MinThicknessPlate && pdi.Thickness <= this.cassette.MaxThicknessPlate && pdi.Leveling.HasValue && pdi.Leveling.Value.Equals(1);
                                    if (bLeveling) {
                                        //Calculation .dll required
                                        _SettingsData = (SettingsData)this._PlateCalculation.CalcPlateComparison(_SettingsData, _SettingsData.Plastification);
                                        _SettingsData = (SettingsData)this._PlateCalculation.CalcLevelerData(_SettingsData, this.cassette, outlet, this._Limitation, this._Initialization.Constants.StoodBolt);
                                    } else {
                                        _SettingsData = new SettingsData {
                                            PlateID = pdi.PlateID,
                                            LevelerInlet = 35f,
                                            LevelerOutlet = 35f
                                        };
                                    }

                                    this._ProductionReport.SetPlateData(pdi);
                                    //_ProductionReport.NoOfLeveling = _ProductionReport.NoOfLeveling.HasValue ? _ProductionReport.NoOfLeveling.Value + 1 : 1;
                                    this._ProductionReport.CassetteNo = this.cassette.CassetteNo;
                                    this._ProductionReport.SetSettingsData(_SettingsData);

                                    _ = this.entity.SaveChanges();
                                } else {
                                    _SettingsData = new SettingsData {
                                        PlateID = pdi.PlateID
                                    };
                                }
                            } else {
                                _SettingsData = new SettingsData {
                                    PlateID = pdi.PlateID
                                };
                            }
                            this.LogSettingsData(_SettingsData, this.PlastificationInputValue);

                            if (this.CanSendToPLC()) {
                                this.SendTelegram(true, TelegramDef.L2_L1_ADJ, _SettingsData, false);
                                this.SuperLog("L2_L1_ADJ send to PLC!");
                            } else {
                                this.SuperLog("L2_L1_ADJ sending to PLC disabled!");
                            }
                        }
                    } else {
                        this.SuperLog("Yield was 0! Calculation canceled!");
                    }
                }
            }
        }

        private bool CanSendToPLC() {
            try {
                var entity = new LEVELEREntities();

                var aux = entity.AuxVariables.FirstOrDefault(item => item.Variable == "ENABLE_LVL_DATA");
                return aux != null && aux.VariableValue == "1";
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
                return false;
            }
        }

        private void SetLimitations() {
            try {
                var limitationList = this.entity.Limitations.ToList();

                Console.WriteLine("Connected to DB");
                this._Limitation = this._ByteConverter.GetLimitations(limitationList);
            } catch (Exception e) {
                Console.WriteLine("DB connection failed. Please check that the database is running and restart the application.");
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }

        private PDI RequestPDI(PlateData plate, bool manualRequest = false) {
            var entity = new LEVELEREntities();
            var track = new TrackingData();
            var pdi = new PDI();
            var bPlateID = plate.PlateID != null;
            bPlateID = bPlateID ? !string.IsNullOrEmpty(plate.PlateID) : bPlateID;
            this.SetPDIStatus();

            if (bPlateID) {
                pdi = manualRequest
                    ? entity.PDIs.FirstOrDefault(p => p.PlateID == plate.PlateID)
                    : entity.PDIs.FirstOrDefault(p => p.PlateID == plate.PlateID && p.Location == (int)PlateLocation.NewPlate);
            } else {
                entity.PDIs.Where(p => p.Location == 0).PrintToList();
                pdi = entity.PDIs.First();
            }

            pdi ??= new PDI();

            pdi.State = 0;
            pdi.Location = plate.Location ?? (short)PlateLocation.Centering1;
            track.PlateID = pdi.PlateID;
            track.Location = pdi.Location;
            this._l3L2Interface.UpdateTracking(track);
            bPlateID = pdi.PlateID != null;
            bPlateID = bPlateID ? !string.IsNullOrEmpty(pdi.PlateID) : bPlateID;

            this.SendTelegram(true, TelegramDef.L2_L1_PDI, pdi, false);

            this.lastGoodPDI = pdi;

            if (bPlateID) {
                var list = entity.ProductionReports.Where(x => x.PlateID == plate.PlateID).ToList();
                if (list.Count > 0) {
                    var lastNoOfLeveling = list.Last().NoOfLeveling;
                    this._ProductionReport = new ProductionReport {
                        NoOfLeveling = lastNoOfLeveling + 1,
                        ProdDate = DateTime.Now
                    };
                } else {
                    this._ProductionReport = new ProductionReport {
                        NoOfLeveling = 1,
                        ProdDate = DateTime.Now
                    };
                }

                _ = entity.ProductionReports.Add(this._ProductionReport);

                this._ProductionReport.SetPlateData(pdi);

                if (entity.PreSettings.FirstOrDefault(ps => ps.MaterialID == pdi.SteelGrade) == null) {

                    var cassetteList = entity.Cassettes.ToList();
                    foreach (var cassette in cassetteList) {
                        var increment = 0.5F;
                        float initv = 4;
                        if (cassette.MinThicknessPlate != null) {
                            initv = cassette.MinThicknessPlate.Value;
                        }

                        float maxThickness = 16;
                        if (cassette.MaxThicknessPlate != null) {
                            maxThickness = cassette.MaxThicknessPlate.Value;
                        }

                        // Create 0.5er steps between min and max thickness
                        var iterations = (int)((maxThickness - initv) / increment);
                        for (var i = 1; i <= iterations; i++) {
                            var setting = new PreSetting {
                                CassetteNo = cassette.CassetteNo,
                                MaterialID = pdi.SteelGrade,
                                CorrPlastification = PlastificationConst,
                                Enable = 1,
                                StartRangeThickness = initv,
                                EndRangeThickness = initv + increment,
                                StartRangeWidth = 800,
                                EndRangeWidth = 2000,
                                CenterHeightBending = 0,
                                CurveProfile = 0,
                                TiltingLeft = 2,
                                TiltingRight = 2
                            };

                            _ = entity.PreSettings.Add(setting);

                            initv += increment;
                        }
                    }
                    this.SuperLog("New PreSettings added for SteelGrade: " + pdi.SteelGrade);
                }
                _ = entity.SaveChanges();
            }
            return pdi;
        }

        private void SetPDIStatus() {
            try {
                this.entity = new LEVELEREntities();
                _ = this.entity.PDIs.Where(p => !p.State.HasValue | p.State == 0).ToList();
                foreach (var item in this.entity.PDIs) {
                    item.State = 1;
                }

                _ = this.entity.SaveChanges();
            } catch { }
        }
        #endregion Data

        #region Logging
        private void SuperLog(string Msg) {
            Console.WriteLine(DateTime.Now + " - " + Msg);
            Logging.SendMessage(DateTime.Now + " - " + Msg, MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
        }

        private void LogSettingsData(SettingsData settings, float plastificationInput) {
            this.SuperLog("SETTINGS DATA INPUT: ");

            this.SuperLog("Set Curve Profile: " + settings.CurveProfile);
            this.SuperLog("Set Tilt Left: " + settings.TiltLeft);
            this.SuperLog("Set Tilt Right: " + settings.TiltRight);
            this.SuperLog("Set Middle Height: " + settings.MiddleHeight);
            this.SuperLog("Thickness: " + settings.Thickness);
            this.SuperLog("Width: " + settings.Width);
            this.SuperLog("YieldPoint: " + settings.YieldPoint);
            this.SuperLog("Plastification: " + plastificationInput);
            this.SuperLog("..........      ............      .................");
            this.SuperLog("SETTINGS DATA OUTPUT: ");

            this.SuperLog("Leveler Inlet: " + settings.LevelerInlet);
            this.SuperLog("Leveler Outlet: " + settings.LevelerOutlet);
            this.SuperLog("Motor Power: " + settings.MotorPower);
            this.SuperLog("Motor Torque: " + settings.MotorTorque);
            this.SuperLog("Plastification Result: " + settings.Plastification);

            this.SuperLog("SETTINGS DATA STOP....");
        }

        private void LogPlateStateData(LevelerData plateStateData) {
            this.SuperLog("PLATE STATE DATA OUTPUT: ");

            this.SuperLog("Actual Tilt Left: " + plateStateData.TiltLeft);
            this.SuperLog("Actual Tilt Rigt: " + plateStateData.TiltRight);
            this.SuperLog("Actual Middle Height: " + plateStateData.MiddleHeight);
            this.SuperLog("Actual  Curve Profile: " + plateStateData.CurveProfile);

            this.SuperLog("SETTINGS DATA STOP....");
        }

        private void LogRecievedTelegram(TelegramData telegram) {
            this.SuperLog("...............................................");
            this.SuperLog(telegram.Name + " received");

            foreach (var itm in telegram.TelegramValues) {
                this.SuperLog(itm.Name + ": " + itm.Value);
            }

            this.SuperLog("...............................................");
        }
        #endregion Logging

        #region Testing
        public void TestL3PDOInterface_Send() {
            try {
                var pdi = new PDI();
                var productionReport = new ProductionReport();

                pdi.EModule = 210000;
                productionReport.CassetteNo = 1;
                pdi.PlateID = "GSM2T10055908";
                pdi.MaterialID = "GSM2T10063200";
                pdi.Leveling = 1;
                productionReport.NoOfLeveling = 1;
                productionReport.CassetteNo = 1;
                productionReport.SetLevelerInlet = 4.8f;
                productionReport.SetLevelerOutlet = -30.2f;
                productionReport.SetTiltLeft = 2;
                productionReport.SetTiltRight = 2;
                productionReport.SetMiddleHeight = 1;
                productionReport.SetCurveProfile = 1;
                productionReport.ActLevelerInlet = 6.8f;
                productionReport.ActLevelerOutlet = -25.2f;
                productionReport.ActTiltLeft = 2;
                productionReport.ActTiltRight = 2;
                productionReport.ActMiddleHeight = -1.2f;
                productionReport.ActCurveProfile = 2;
                pdi.TensileStrength = 250;
                productionReport.Temperature = 0;
                productionReport.ActTemperature = 30;

                this._l3L2Interface.SendPDOToL3(pdi, productionReport);
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        public void Test_L2_L1_PDI_Telegram() {
            try {
                var pdi = new PDI {
                    PlateID = "2780D",
                    MaterialID = "2780010070714",
                    SteelGrade = "SG112",
                    Length = 250.1f,
                    Width = 130.1f,
                    Thickness = 4.2f,
                    YieldPoint = 15.3f,
                    TensileStrength = 12.2f,
                    Ruler1MCrossPDI = 2,
                    Ruler1MLengthPDI = 12,
                    Ruler2MLengthPDI = 13
                };

                this.SendTelegram(true, TelegramDef.L2_L1_PDI, pdi, false);
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        public void Test_L2_L1_ADJ_Telegram() {
            try {
                var settingsData = new SettingsData {
                    PlateID = "2780D",
                    LevelerInlet = 13,
                    LevelerOutlet = 12,
                    TiltLeft = 2,
                    TiltRight = 2.1f,
                    MiddleHeight = 2.2f,
                    CurveProfile = 50,
                    Plastification = 3,
                    Elongation = 15
                };

                this.SendTelegram(true, TelegramDef.L2_L1_ADJ, settingsData, false);
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        public void FillDBValues() {
            try {
                this.entity = new LEVELEREntities();
                var preSettingsOutlets = this.entity.PreSettingsOutlets.ToList();

                if (preSettingsOutlets != null && preSettingsOutlets.Count > 0) {
                    Console.WriteLine("found " + preSettingsOutlets.Count + " items");
                    return;
                }
                uint counter = 0;

                float thicknessMin = 3;
                float thicknessMax = 16;
                float thicknessStep = 1;

                float yieldMin = 500;
                float yieldMax = 2100;
                float yieldStep = 100;

                float yieldLoop = 0;
                var thicknessLoop = thicknessMin;
                while (yieldLoop < yieldMax) {
                    thicknessLoop = thicknessMin;
                    while (thicknessLoop < thicknessMax) {
                        var preSettings = new PreSettingsOutlet {
                            StartRangeThickness = thicknessLoop,
                            EndRangeThickness = thicknessLoop + thicknessStep,
                            StartRangeActYieldPoint = yieldLoop,
                            EndRangeActYieldPoint = yieldLoop == 0 ? yieldMin : yieldLoop + yieldStep,

                            Offset = 0
                        };
                        _ = this.entity.PreSettingsOutlets.Add(preSettings);
                        counter++;

                        // Increase thickness
                        thicknessLoop += thicknessStep;
                    }

                    // Increase yield strength
                    if (yieldLoop == 0) {
                        yieldLoop = yieldMin;
                    } else {
                        yieldLoop += yieldStep;
                    }
                }

                _ = this.entity.SaveChanges();
                Console.WriteLine("Did write " + counter + " PreSettingsOutlet to DB");
            } catch (Exception ex) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        public void Dispose() => throw new NotImplementedException();
        #endregion Testing
    }
}
