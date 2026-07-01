using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.Client {
    /// <summary>
    /// Interaktionslogik für pagePlateData.xaml
    /// </summary>
    public partial class pagePlateData : Page, IDisposable {
        private LEVELEREntities entity;
        private CollectionViewSource _PDIControlViewSource;
        private CollectionViewSource _PDIViewSource;
        private PDI _PDI;
        private readonly LimitationData _Limitation;
        public enum AutomationMode {
            Disable = 0,
            Enable = 1
        }

        public pagePlateData(LimitationData limits) {
            this._Limitation = limits;
            this.InitializeComponent();
            this.InitEntity();
            this.SetPlateLocation();

            this._PDI = new PDI();
        }

        public void InitEntity() {
            this._PDIControlViewSource = (CollectionViewSource)this.FindResource("PDIControlDataViewSource");
            this._PDIViewSource = (CollectionViewSource)this.FindResource("PDIDataViewSource");
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            try {
                this._PDIViewSource.Source = null;
                var PDIList = this.entity.PDIs.Where(p => p.Location < (short)LocationDef.Out).ToList();

                this._PDIViewSource.Source = PDIList;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }

            var AuxVariable = this.entity.AuxVariables.FirstOrDefault();
            if (AuxVariable.VariableValue == "0") {
                this.btnEnableAutomation.Content = "Enable Leveler Set Points";
                this.btnEnableAutomation.Foreground = Brushes.Green;
            } else {
                this.btnEnableAutomation.Content = "Disable Leveler Set Points";
                this.btnEnableAutomation.Foreground = Brushes.Red;
            }
        }

        private void SetEnabled(bool bEnabled) {
            //txtGapCrossBow.IsEnabled = bEnabled;
            //txtGapLengthBow.IsEnabled = bEnabled;
            this.txtMaterialID.IsEnabled = bEnabled;
            this.txtPlateID.IsEnabled = bEnabled;
            this.txtPlateLength.IsEnabled = bEnabled;
            this.txtPlateThickness.IsEnabled = bEnabled;
            this.txtPlateWidth.IsEnabled = bEnabled;
            //txtRuler1MCross.IsEnabled = bEnabled;
            //txtRuler1MLength.IsEnabled = bEnabled;
            //txtRuler2MLength.IsEnabled = bEnabled;
            this.txtSteelGrade.IsEnabled = bEnabled;
            //txtTensileStrength.IsEnabled = bEnabled;
            this.txtYieldPoint.IsEnabled = bEnabled;
            this.chkLeveling.IsEnabled = bEnabled;
            this.btnCancelPDI.IsEnabled = bEnabled;
            this.btnCopy.IsEnabled = !bEnabled;
            this.btnCreatePDI.IsEnabled = !bEnabled;
            this.btnDeletePDI.IsEnabled = !bEnabled;
            this.btnEditPDI.IsEnabled = !bEnabled;
            this.btnSavePDI.IsEnabled = bEnabled;

            this.gridPDI.IsEnabled = !bEnabled;
        }

        private void btnSavePDI_Click(object sender, RoutedEventArgs e) {
            try {
                var result = MessageBox.Show("Do you want to save this item?", "PDI", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) {
                    var hadSelectedObject = false;
                    if (this.gridPDI.ActiveDataItem != null) {
                        this._PDI = (PDI)this.gridPDI.ActiveDataItem;
                        hadSelectedObject = true;
                    }

                    var steelGrade = this.txtSteelGrade.Text.ToString();
                    var setting = this.entity.PreSettings.FirstOrDefault(s => s.MaterialID == steelGrade);
                    if (setting == null) {
                        var pdi = this.entity.PDIs.Local[0];
                        var cassetteList = this.entity.Cassettes.ToList();
                        if (pdi.Thickness <= 16) {
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
                                    setting = new PreSetting {
                                        CassetteNo = cassette.CassetteNo,
                                        MaterialID = pdi.SteelGrade,
                                        CorrPlastification = 55,
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

                                    _ = this.entity.PreSettings.Add(setting);

                                    initv += increment;
                                }
                            }
                            Logging.SendMessage("New PreSettings added for SteelGrade: " + pdi.SteelGrade, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                        } else {
                            var stepsize = 0.5f;
                            var thickness = (float)(pdi.Thickness.Value - (pdi.Thickness.Value % stepsize));
                            foreach (var cas in cassetteList) {
                                var newSetting = new PreSetting {
                                    MaterialID = steelGrade,
                                    CassetteNo = cas.CassetteNo,
                                    StartRangeThickness = thickness,
                                    EndRangeThickness = thickness + stepsize,
                                    StartRangeWidth = 800,
                                    EndRangeWidth = 2000,
                                    CorrPlastification = 55,
                                    CenterHeightBending = 0,
                                    CurveProfile = 0,
                                    TiltingLeft = 2,
                                    TiltingRight = 2
                                };
                                _ = this.entity.PreSettings.Add(newSetting);
                            }
                            Logging.SendMessage("New PreSettings added for SteelGrade: " + pdi.SteelGrade + " and thickness: " + pdi.Thickness.Value, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, this.GetType());
                        }
                    }
                    _ = this.entity.SaveChanges();
                    this.SetEnabled(false);
                    this.InitEntity();
                    if (hadSelectedObject) {
                        this.gridPDI.ActiveDataItem = ((List<PDI>)this._PDIViewSource.Source).First(p => p.PKEY_PDI == this._PDI.PKEY_PDI);
                        this.gridPDI.SelectedDataItem = this.gridPDI.ActiveDataItem;
                    }
                }
            } catch (DbEntityValidationException dbex) {
                try {
                    var pdi = this.entity.PDIs.Local[0];
                    _ = this.entity.SavePDIData(pdi.PKEY_PDI, pdi.PlateID, pdi.MaterialID, pdi.SteelGrade, pdi.Length, pdi.Width, pdi.Thickness, pdi.YieldPoint, pdi.TensileStrength, pdi.EModule, pdi.MeasuringCode, pdi.Ruler1MCrossPDI,
                       pdi.Ruler1MLengthPDI, pdi.Ruler2MLengthPDI, pdi.Leveling, pdi.GapCrossBow, pdi.GapLengthBow);
                } catch {
                    Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, dbex, this.GetType());
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnEditPDI_Click(object sender, RoutedEventArgs e) => this.SetEnabled(true);

        private void btnDeletePDI_Click(object sender, RoutedEventArgs e) {
            this.SetEnabled(false);
            try {
                var cnt = this.gridPDI.SelectedItems.Records.Count;
                var message = "Do you want to delete ";
                message += cnt > 1 ? "these items?" : "this item?";
                var result = MessageBox.Show(message, "Plates", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) {
                    this._PDI = (PDI)this.gridPDI.ActiveDataItem;
                    if (this._PDI != null) {
                        var selectedPdi = this.entity.PDIs.FirstOrDefault(p => p.PKEY_PDI == this._PDI.PKEY_PDI);
                        _ = this.entity.PDIs.Remove(selectedPdi);
                        _ = this.entity.SaveChanges();
                    }

                    //if (entity != null)
                    //   entity.Dispose();
                    //entity = new LEVELEREntities();
                    ////if (cnt > 1)
                    ////{
                    //IEnumerable<PDI> list = (IEnumerable<PDI>)_PDIViewSource.Source;
                    //foreach (Infragistics.Windows.DataPresenter.Record rec in gridPDI.SelectedItems.Records)
                    //{
                    //   PDI pdi = list.ElementAt<PDI>(rec.Index);
                    //   _PDI = entity.PDIs.Single(p => p.PKEY_PDI == pdi.PKEY_PDI);
                    //   _PDI.State = 2;
                    //   entity.SaveChanges();

                    //}

                    //foreach (PDI item in list)
                    //   item.State = 2;
                    //}
                    //else
                    //{
                    //   _PDI = entity.PDIs.Single(p => p.PKEY_PDI == _PDI.PKEY_PDI);
                    //   _PDI.State = 2;
                    //   entity.SaveChanges();
                    //}

                    this.InitEntity();
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e) {
            if (this.entity.PDIs.Local.Any()) {
                var pdi = this.entity.PDIs.Local[0];
                pdi.PKEY_PDI = 0;
                pdi.PlateID = "";
                pdi.Location = 0;
                this.entity?.Dispose();

                this.entity = new LEVELEREntities();
                this.entity.PDIs.Local.Add(pdi);
                this._PDIControlViewSource.Source = this.entity.PDIs.Local;

                this.SetEnabled(true);
            }
        }

        private void btnEnableAutomation_Click(object sender, RoutedEventArgs e) {
            var newEntity = new LEVELEREntities();

            var automationChanged = newEntity.AuxVariables.FirstOrDefault();
            if (automationChanged.VariableValue == "0") {
                automationChanged.VariableValue = "1";
                this.btnEnableAutomation.Content = "Disable Leveler Set Points";
                this.btnEnableAutomation.Foreground = Brushes.Red;
            } else {
                automationChanged.VariableValue = "0";
                this.btnEnableAutomation.Content = "Enable Leveler Set Points";
                this.btnEnableAutomation.Foreground = Brushes.Green;
            }
            _ = newEntity.SaveChanges();
        }

        private void btnCreatePDI_Click(object sender, RoutedEventArgs e) {
            try {
                this._PDI = new PDI {
                    MeasuringCode = 1,
                    State = 0,
                    Manual = 1,

                    Location = 0,
                    EModule = this._Limitation.EModule
                };
                this.entity?.Dispose();

                this.entity = new LEVELEREntities();
                this.entity.PDIs.Local.Add(this._PDI);
                this._PDIControlViewSource.Source = this.entity.PDIs.Local;
                this.chkLeveling.IsChecked = true;
                this.SetEnabled(true);
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnCancelPDI_Click(object sender, RoutedEventArgs e) {
            this.SetEnabled(false);

            // Update the grid elements and revert the changes
            if (this.gridPDI.ActiveDataItem != null) {
                this._PDI = (PDI)this.gridPDI.ActiveDataItem;
                this.gridPDI.SelectedDataItem = null;
                this.gridPDI.SelectedDataItem = this._PDI;
            }
        }

        private void gridPDI_SelectedItemsChanged(object sender, Infragistics.Windows.DataPresenter.Events.SelectedItemsChangedEventArgs e) {
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            if (this.gridPDI.ActiveDataItem != null) {
                this._PDI = (PDI)this.gridPDI.ActiveDataItem;
                _ = this.entity.PDIs.Single(p => p.PKEY_PDI == this._PDI.PKEY_PDI);
            }
            this._PDIControlViewSource.Source = this.entity.PDIs.Local;
            this.btnCopy.IsEnabled = true;
            this.btnEditPDI.IsEnabled = true;
            this.btnDeletePDI.IsEnabled = true;
        }

        public void SetPlateLocation() {
            this.entity = new LEVELEREntities();

            var lastOut = new PDI();
            var pdi = this.entity.PDIs.Where(p => p.Location < (short)LocationDef.Out).ToList();//.FirstOrDefault(p => p.Location < 5);
            var listOfOuts = this.entity.PDIs.Where(lp => lp.Location == (short)LocationDef.Out).ToList();
            var count = listOfOuts.Count - 1;
            try {
                lastOut = listOfOuts[count];
            } catch (Exception) {
                lastOut = null;
            }

            this.InvokeText(this.lblCenteringTrack, "");
            this.InvokeText(this.lblLevelerTrack, "");
            this.InvokeText(this.lblOutTrack, "");
            this.InvokeText(this.lblNewTrack, "");

            var firstInCentering = pdi.FirstOrDefault(x => x.Location == (short)LocationDef.Centering_1);
            if (firstInCentering != null) {
                this.InvokeText(this.lblCenteringTrack, firstInCentering.PlateID);
            }

            var firstInLeveling = pdi.FirstOrDefault(x => x.Location == (short)LocationDef.Leveler);
            if (firstInLeveling != null) {
                this.InvokeText(this.lblLevelerTrack, firstInLeveling.PlateID);
            }

            var newPlate = pdi.FirstOrDefault(x => x.Location == (short)LocationDef.New_Plate);
            if (newPlate != null) {
                this.InvokeText(this.lblNewTrack, newPlate.PlateID);
            }

            if (lastOut != null) {
                this.InvokeText(this.lblOutTrack, lastOut.PlateID);
            }
        }

        private void InvokeText(Label control, string text) {
            this.Dispatcher.Invoke(new Action(delegate () {
                control.Content = text;
            }));
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
