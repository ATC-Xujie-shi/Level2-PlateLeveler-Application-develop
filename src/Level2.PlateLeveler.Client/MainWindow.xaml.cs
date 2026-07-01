using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;
using Level2.PlateLeveler.DataConverter;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.Client {
    public enum NavigationDef {
        PDI, Temperature, LineState, Presettings, PresettingsOutlet, FaultCompensation, Limitation, ProductionReport, Cassette
    }
    public delegate void CultureChangeEventHandler(object sender, CultureChangeEventArgs e);

    public partial class MainWindow : Window, IDisposable {
        private LEVELEREntities entity;
        private pagePlateData _PagePDI;
        //pageTemperature _PageTemp;
        private pageCassette _PageCassette;
        private pagePresettings _PagePresettings;
        //pageFaultCompensation _PageFaultCompensation;
        private pagePresettingsOutlet _PagePresettingsOutlet;
        private pageProductionReport _PageProductionReport;
        private pageActualValues _PageActualValues;
        private pageLineState _PageLineState;

        public event CultureChangeEventHandler CultureChanged;

        private readonly ByteConverter _ByteConverter;

        private readonly XMLConverter _XMLConverter;
        private readonly StartThreadCom _ThreadCom;
        private readonly InitData _Initialization;
        private Timer _Timer;
        private readonly System.Windows.Threading.DispatcherTimer _TimerSignals;
        private readonly List<SignalData> _Signals;

        private readonly TelegramList _TelegramList;

        private const string InitKey = "Init_Pfad";
        public MainWindow() {
            Logging.SendMessage("Starting leveler client!", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());

            this.InitializeComponent();

            this._XMLConverter = new XMLConverter();
            try {
                var limits = this.GetLimits();
                this._PagePDI = new pagePlateData(limits);
                _ = this.frame_INIUInterface.NavigationService.Navigate(this._PagePDI);
                this._Signals = [];

                this._ByteConverter = new ByteConverter();
                var signals = (SignalDef[])Enum.GetValues(typeof(SignalDef));
                var dataSignal = new SignalData();
                for (var n = 0; n < signals.Length; n++) {
                    dataSignal = new SignalData {
                        Signal = signals[n],
                        Flag = 0
                    };
                    this._Signals.Add(dataSignal);
                }
                this._Initialization = (InitData)this._XMLConverter.ConvertFileToObject<InitData>(Functions.GetInitPath(InitKey));
                this._XMLConverter.Initialization = this._Initialization;

                this._TelegramList = (TelegramList)this._XMLConverter.ConvertFileToObject<TelegramList>(this._Initialization.TelegramFile);
                this._TelegramList = this._XMLConverter.SetTelegramList(this._TelegramList);

                this._ThreadCom = new StartThreadCom(this._Initialization.Communications);
                this._ThreadCom.ComError += this._ThreadCom_ComError;
                this._ThreadCom.ComEstablished += this._ThreadCom_ComEstablished;
                this._ThreadCom.DataReceived += this._ThreadCom_DataReceived;
                _ = this._ThreadCom.StartThread();
                //Binding bin = (Binding)lblNavigation.Content;
                if (this.cbLanguages.Items.Count > 0) {
                    this.cbLanguages.SelectedIndex = 1;
                }

                this._TimerSignals = new System.Windows.Threading.DispatcherTimer {
                    Interval = new TimeSpan(0, 0, 4)
                };
                this._TimerSignals.Tick += this._TimerSignals_Tick;
                this._TimerSignals.Start();
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }

        private void _TimerSignals_Tick(object sender, EventArgs e) {
            this.ellipse_Connection_DB.Fill = Convert.ToBoolean(this._Signals.FirstOrDefault(s => s.Signal == SignalDef.DB).Flag) ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.Red);
            //ellipse_Connection_FM.Fill = Convert.ToBoolean(_Signals.Single(s => s.Signal == SignalDef.FM).Flag) ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.Red);
            this.ellipse_Connection_L1.Fill = Convert.ToBoolean(this._Signals.FirstOrDefault(s => s.Signal == SignalDef.L1).Flag) ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.Red);
            this.ellipse_Connection_L2.Fill = Convert.ToBoolean(this._Signals.FirstOrDefault(s => s.Signal == SignalDef.L3).Flag) ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.Red);

            this._Signals.FirstOrDefault(s => s.Signal == SignalDef.L1).Flag = 0;
            this._Signals.FirstOrDefault(s => s.Signal == SignalDef.L3).Flag = 0;
            this._Signals.FirstOrDefault(s => s.Signal == SignalDef.DB).Flag = 0;
        }

        private void MissingPDIDialog() {
            var popup = new PopupDialog();
            _ = popup.ShowDialog();
        }
        private void resultElapsed(object sender, ElapsedEventArgs e) {

        }
        private void SetBinding(Label label, NavigationDef nav) {
            BindingOperations.ClearBinding(label, ContentProperty);
            var binding = new Binding(nav.ToString()) {
                Mode = BindingMode.OneWay,
                Source = this.FindResource("Resources")
            };
            _ = BindingOperations.SetBinding(label, ContentProperty, binding);
        }

        private LimitationData GetLimits() {
            try {
                this.entity = new LEVELEREntities();
                var list = this.entity.Limitations.ToList();
                var _Converter = new PriorityConverter();
                var limits = _Converter.GetLimitations(list);
                return limits;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return null;
            }
        }
        private void _PageLimitation_SendLimit(object sender, EventArgs e) => this.SendTelegram(TelegramDef.HMI_L2_Limitation, new object());

        private void timer_Elapsed(object sender, ElapsedEventArgs e) {
            if (this._Initialization.Communications[0].LogLiveTelegram) {
                Logging.SendMessage("HMI_L2_SignOfLife is sent!", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
            }

            this.SendTelegram(TelegramDef.HMI_L2_SignOfLife, new object());
        }

        private void _PagePDI_RequestPDI(object sender, RequestPDIEventArgs e) {
            var pdi = new PDI();

            _ = this.entity.Limitations.ToList();
            pdi.PlateID = e.PlateID != null ? e.PlateID.ToString() : "";
            pdi.Location = (short)e.Location;
            this.SendTelegram(TelegramDef.HMI_L2_Request, pdi);
        }

        private void _ThreadCom_DataReceived(byte[] Telegram, int index = 0) {
            var telegram = this._ByteConverter.LoadTelegramFromByteArray(this._TelegramList, Telegram, this._Initialization.Communications[index]);
            this.ManageTelegramActions(telegram);
        }

        private void _ThreadCom_ComEstablished(int index) {
            this.InvokeEllipse(this.ellipse_Connection_Server, true);
            this._Timer = new Timer(this._Initialization.Constants.Interval);
            this._Timer.Elapsed += this.timer_Elapsed;
            this._Timer.Enabled = true;
            this._Timer.Start();
        }

        private void _ThreadCom_ComError(int index) {
            this.InvokeEllipse(this.ellipse_Connection_Server, false);
            if (this._Timer != null) {
                this._Timer.Stop();
                this._Timer.Enabled = false;
            }
        }

        private void ManageTelegramActions(TelegramData telegram) {
            if (telegram != null) {
                switch (telegram.TelegramID) {
                    #region L2_HMI_SignOfLife
                    case 95:
                        Logging.SendMessage(telegram.Name + " was sent!", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                        ;
                        break;
                    #endregion
                    #region L2_HMI_Tracking
                    case 96:
                        if (this._PagePDI != null) {
                            this._PagePDI.SetPlateLocation();
                            this.Dispatcher.Invoke(this._PagePDI.InitEntity);
                        }
                        break;
                    #endregion
                    #region L2_HMI_Signal
                    case 97:
                        var signal = (SignalData)this._XMLConverter.ConvertTelegramToObject<SignalData>(telegram);

                        if (signal.Signal == SignalDef.ActiveCassette) {
                            this._PageCassette?.SetActiveCassette(signal.Flag);
                        } else {
                            var flag = signal.Flag;
                            signal = this._Signals.Single(s => s.Signal == signal.Signal);
                            signal.Flag = flag;
                        }
                        break;
                    #endregion
                    #region L2_HMI_MissingPDI
                    case 98:
                        var thread = new System.Threading.Thread(new System.Threading.ThreadStart(this.MissingPDIDialog));
                        thread.Start();
                        break;
                    #endregion
                    #region L2_HMI_DatabaseUpdate
                    case 99:
                        if (this._PagePDI != null) {
                            Logging.SendMessage(telegram.Name + " recieved!...................", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                            ;
                            this.Dispatcher.Invoke(() => {
                                this._PagePDI.InitEntity();
                                this._PagePresettings.InitEntity(false);
                                this._PagePresettingsOutlet.InitEntity(false);
                            });
                        }
                        break;
                        #endregion
                }
            }
        }

        private void mnuPDI_Click(object sender, RoutedEventArgs e) {
            var limits = this.GetLimits();
            this._PagePDI ??= new pagePlateData(limits);

            _ = this.frame_INIUInterface.NavigationService.Navigate(this._PagePDI);
            this.SetBinding(this.lblNavigation, NavigationDef.PDI);
        }

        //private void mnuTemp_Click(object sender, RoutedEventArgs e)
        //{
        //   if (_PageTemp == null)
        //      _PageTemp = new pageTemperature(_Initialization);
        //   frame_INIUInterface.NavigationService.Navigate(_PageTemp);
        //   SetBinding(lblNavigation,  NavigationDef.Temperature);

        //}

        private void mnuCassette_Click(object sender, RoutedEventArgs e) {
            this._PageCassette ??= new pageCassette(this._Initialization);

            _ = this.frame_INIUInterface.NavigationService.Navigate(this._PageCassette);
            this.SetBinding(this.lblNavigation, NavigationDef.Cassette);
        }

        private void mnuPreSettings_Click(object sender, RoutedEventArgs e) {
            this._PagePresettings ??= new pagePresettings();

            _ = this.frame_INIUInterface.NavigationService.Navigate(this._PagePresettings);
            this.SetBinding(this.lblNavigation, NavigationDef.Presettings);
        }

        //private void mnuFaultCompensation_Click(object sender, RoutedEventArgs e)
        //{
        //   if (_PageFaultCompensation == null)
        //      _PageFaultCompensation = new pageFaultCompensation();
        //   frame_INIUInterface.NavigationService.Navigate(_PageFaultCompensation);
        //   SetBinding(lblNavigation, NavigationDef.FaultCompensations);
        //}

        private void mnuPreSettingsOutlet_Click(object sender, RoutedEventArgs e) {
            this._PagePresettingsOutlet ??= new pagePresettingsOutlet();

            _ = this.frame_INIUInterface.NavigationService.Navigate(this._PagePresettingsOutlet);
            this.SetBinding(this.lblNavigation, NavigationDef.PresettingsOutlet);
        }

        private void mnuProductionReport_Click(object sender, RoutedEventArgs e) {
            this._PageProductionReport = new pageProductionReport(this._Initialization);
            _ = this.frame_INIUInterface.NavigationService.Navigate(this._PageProductionReport);
            this.SetBinding(this.lblNavigation, NavigationDef.ProductionReport);
        }

        private void InvokeEllipse(Ellipse control, bool bConnected) {
            try {
                this.Dispatcher.Invoke(new Action(delegate () {
                    control.Fill = bConnected ? new SolidColorBrush(Colors.LightGreen) : new SolidColorBrush(Colors.Red);
                }));
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
            }
        }

        private void SendTelegram<T>(TelegramDef telegramName, T obj) {

            try {
                var tel = this._TelegramList.FirstOrDefault(t => t.Name.Equals(telegramName.ToString(), StringComparison.Ordinal));
                if (tel != null) {
                    tel = this._XMLConverter.ConvertObjectToTelegram(obj, tel);

                    var btArr = this._ByteConverter.ConvertTelegramToByteArray(tel, this._Initialization.Communications[tel.ComIndex]);
                    this._ThreadCom.SendByteArray(btArr, tel.ComIndex);
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        //private void SetTelegramCounter(ref TelegramData data)
        //{
        //   Random rnd = new Random();
        //   foreach (TelegramBlockData item in data.TelegramBlocks)
        //      if (item.Name.Equals("TelegramCounter"))
        //         item.Value = (short)rnd.Next(10000);
        //}

        private void Window_Closed(object sender, EventArgs e) {
            Logging.SendMessage("Stopping Leveler client!", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
            Environment.Exit(0);
        }

        private void mnuLineState_Click(object sender, RoutedEventArgs e) {
            this._PageLineState = new pageLineState(this._Initialization);
            _ = this.frame_INIUInterface.NavigationService.Navigate(this._PageLineState);
            this.lblNavigation.Content = "Line State";

        }

        private void cbLanguages_SelectionChanged(object sender, SelectionChangedEventArgs e) => CultureChanged?.Invoke(this, new CultureChangeEventArgs(new CultureInfo(this.cbLanguages.SelectedValue.ToString())));

        private void mnuActualValues_Click(object sender, RoutedEventArgs e) {
            this._PageActualValues = new pageActualValues(this._Initialization);
            _ = this.frame_INIUInterface.NavigationService.Navigate(this._PageActualValues);
            this.lblNavigation.Content = "Actual Values";
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
