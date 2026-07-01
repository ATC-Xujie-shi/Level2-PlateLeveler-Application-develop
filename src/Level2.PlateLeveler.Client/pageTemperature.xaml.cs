using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Level2.PlateLeveler.DataAccess;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.Client {
    /// <summary>
    /// Interaktionslogik für pageTemperature.xaml
    /// </summary>
    public partial class pageTemperature : Page, IDisposable {
        private LEVELEREntities entity;
        private readonly CollectionViewSource _TemperatureControlViewSource;
        private CollectionViewSource _TemperatureViewSource;
        private readonly CollectionViewSource _SteelgradeControlViewSource;
        private CollectionViewSource _SteelgradeViewSource;
        private readonly InitData _Initialization;

        private bool bTempEnabled;
        private Temp _Temperature;
        private Steelgrade _Steelgrade;
        public pageTemperature(InitData init) {
            this.InitializeComponent();
            this.entity = new LEVELEREntities();
            this._TemperatureControlViewSource = (CollectionViewSource)this.FindResource("TemperatureControlDataViewSource");
            this._TemperatureViewSource = (CollectionViewSource)this.FindResource("TemperatureDataViewSource");
            this._SteelgradeViewSource = (CollectionViewSource)this.FindResource("SteelgradeDataViewSource");
            this._SteelgradeControlViewSource = (CollectionViewSource)this.FindResource("SteelgradeControlDataViewSource");
            this.entity.Steelgrades.PrintToList();
            this.cboSteelgrade.ItemsSource = this.entity.Steelgrades.Local;
            this.cboSteelgrade.SelectedIndex = 0;
            this._Temperature = new Temp();
            this._Steelgrade = new Steelgrade();
            this.InitEntitySG();
            this._Initialization = init;
            this.SetEnabled(false);
        }

        private void InitEntity() {
            this._TemperatureViewSource = (CollectionViewSource)this.FindResource("TemperatureDataViewSource");
            this.entity = new LEVELEREntities();
            var sg = (Steelgrade)this.cboSteelgrade.SelectedItem;
            //entity.Temps.PrintToList();
            var list = new List<Temp>();
            if (sg != null) {
                list = [.. this.entity.Temps.Where(t => t.Steelgrade == sg.SteelGrade).OrderBy(temp => temp.Temperature)];
            }

            if (this.entity.Temps.Local.Count.Equals(0)) {
                this.btnEdit.IsEnabled = false;
            } else {
                this.gridTemperature.ActiveDataItem = this.entity.Temps.Local[0];
            }

            this._TemperatureViewSource.Source = this.entity.Temps.Local;
        }

        private void InitEntitySG() {
            this._SteelgradeViewSource = (CollectionViewSource)this.FindResource("SteelgradeDataViewSource");
            this.entity = new LEVELEREntities();
            this.entity.Steelgrades.PrintToList();
            this._SteelgradeViewSource.Source = this.entity.Steelgrades.Local;
        }
        private void SetEnabled(bool bEnabled) {
            this.txtEModule.IsEnabled = bEnabled;
            this.txtTemperature.IsEnabled = bEnabled;
            this.txtYieldPoint.IsEnabled = bEnabled;
            this.btnCreate.IsEnabled = !bEnabled;
            this.btnDelete.IsEnabled = bEnabled;
            this.btnEdit.IsEnabled = !bEnabled && this.entity.Temps.Local.Any();
            this.btnSave.IsEnabled = bEnabled;
            this.btnCancel.IsEnabled = bEnabled;
            this.bTempEnabled = bEnabled;
        }

        private void SetEnabledSG(bool bEnabled) {
            this.txtSteelgrade.IsEnabled = bEnabled;
            this.btnCreateSG.IsEnabled = !bEnabled;
            this.btnDeleteSG.IsEnabled = bEnabled;
            this.btnEditSG.IsEnabled = !bEnabled;
            this.btnSaveSG.IsEnabled = bEnabled;
            this.btnCancelSG.IsEnabled = bEnabled;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            if (this.entity.Temps.Local.Any()) {
                var sg = (Steelgrade)this.cboSteelgrade.SelectedItem;
                if (sg != null) {
                    this.entity.Temps.Local[0].Steelgrade = sg.SteelGrade;
                    _ = this.entity.SaveChanges();
                } else {
                    _ = MessageBox.Show("Please fill all values!", "Temperature");
                }
            }
            //cboSteelgrade.SelectedValue = entity.Temps[0].Steelgrade;
            this.SetEnabled(false);
            this.InitEntity();
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) => this.SetEnabled(true);

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            this.SetEnabled(false);
            try {
                var result = MessageBox.Show("Do you want to delete this item?", "Temperature", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) {
                    this.entity = new LEVELEREntities();
                    _ = this.entity.Temps.Attach(this._Temperature);
                    _ = this.entity.Temps.Remove(this._Temperature);
                    _ = this.entity.SaveChanges();
                    this.txtEModule.Text = "";
                    this.txtTemperature.Text = "";
                    this.txtYieldPoint.Text = "";

                    this.InitEntity();
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) => this.SetEnabled(false);

        private void gridTemperature_SelectedItemsChanged(object sender, Infragistics.Windows.DataPresenter.Events.SelectedItemsChangedEventArgs e) {
            try {
                if (this.gridTemperature.ActiveDataItem != null) {
                    this._Temperature = (Temp)this.gridTemperature.ActiveDataItem;
                    var list = (IEnumerable<Steelgrade>)this.cboSteelgrade.ItemsSource;
                    try {
                        var sg = list.Single(s => s.SteelGrade == this._Temperature.Steelgrade);
                        this.entity = new LEVELEREntities();
                        this._Temperature = this.entity.Temps.Single(temp => temp.PKEY_TEMP == this._Temperature.PKEY_TEMP);
                    } catch { }
                    this._TemperatureControlViewSource.Source = this.entity.Temps.Local;
                    this.SetEnabled(false);
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e) {
            try {
                this._Temperature = new Temp {
                    Steelgrade = this.cboSteelgrade.SelectedValue.ToString()
                };
                if (!this.entity.Temps.Local.Any()) {
                    _ = this.entity.Temps.Add(this._Temperature);
                }

                this._TemperatureControlViewSource.Source = this.entity.Temps.Local;
                this.SetEnabled(true);
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void cboSteelgrade_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            this.InitEntity();
            if (!this.bTempEnabled) {
                this.txtEModule.Text = "";
                this.txtTemperature.Text = "";
                this.txtYieldPoint.Text = "";
            }
        }

        private void btnCreateSG_Click(object sender, RoutedEventArgs e) {
            try {
                this.entity = new LEVELEREntities();
                this._Steelgrade = new Steelgrade();
                if (!this.entity.Steelgrades.Local.Any()) {
                    _ = this.entity.Steelgrades.Add(this._Steelgrade);
                }

                this._SteelgradeControlViewSource.Source = this.entity.Steelgrades.Local;
                this.SetEnabledSG(true);
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnSaveSG_Click(object sender, RoutedEventArgs e) {
            try {
                var result = MessageBox.Show("Do you want to save this Steelgrade?", "Steelgrade", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) {
                    IEnumerable<Temp> lst = this.entity.Temps.Where(t => t.Steelgrade == this._Steelgrade.SteelGrade);
                    foreach (var temp in lst) {
                        temp.Steelgrade = this.entity.Steelgrades.Local[0].SteelGrade;
                    }
                    _ = this.entity.SaveChanges();
                    this.entity.Steelgrades.PrintToList();

                    this.cboSteelgrade.ItemsSource = this.entity.Steelgrades;
                    this.cboSteelgrade.SelectedIndex = 0;
                    this.SetEnabledSG(false);
                    this.InitEntitySG();
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnEditSG_Click(object sender, RoutedEventArgs e) => this.SetEnabledSG(true);

        private void btnDeleteSG_Click(object sender, RoutedEventArgs e) {
            this.SetEnabled(false);
            try {
                var result = MessageBox.Show("Do you want to delete this item?", "Steelgrade", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) {
                    var adapter = new DataAccessAdapter(this._Initialization.Connections[0].Namespace, this._Initialization.Connections[0].ConnectionString);
                    var list = new List<Temp>();
                    try {
                        list = [.. this.entity.Temps.Where(t => t.Steelgrade == this._Steelgrade.SteelGrade)];
                    } catch {
                    }
                    if (list.Count > 0) {
                        result = MessageBox.Show("The item '" + this._Steelgrade.SteelGrade + "' is not deletable!\r\n The are connections to the Temperature table.", "Steelgrade");
                    } else {
                        try {
                            this.entity = new LEVELEREntities();
                            _ = this.entity.Steelgrades.Attach(this._Steelgrade);
                            _ = this.entity.Steelgrades.Remove(this._Steelgrade);
                            _ = this.entity.SaveChanges();
                        } catch {
                            _ = adapter.DeleteData(TableDef.Steelgrades, this._Steelgrade.Pkey_SG, "Pkey_SG");
                        }
                    }
                    this.entity = new LEVELEREntities();
                    this.entity.Steelgrades.PrintToList();

                    this.cboSteelgrade.ItemsSource = this.entity.Steelgrades;
                    if (this.cboSteelgrade.Items.Count > 0) {
                        this.cboSteelgrade.SelectedIndex = 0;
                    }

                    this.SetEnabledSG(false);
                    this.InitEntitySG();
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnCancelSG_Click(object sender, RoutedEventArgs e) => this.SetEnabledSG(false);

        private void gridSteelgrade_SelectedItemsChanged(object sender, Infragistics.Windows.DataPresenter.Events.SelectedItemsChangedEventArgs e) {
            this.entity = new LEVELEREntities();
            try {
                if (this.gridSteelgrade.ActiveDataItem != null) {
                    this._Steelgrade = (Steelgrade)this.gridSteelgrade.ActiveDataItem;
                    try {
                        _ = this.entity.Steelgrades.Single(po => po.Pkey_SG == this._Steelgrade.Pkey_SG);
                    } catch { }
                }
                this._SteelgradeControlViewSource.Source = this.entity.Steelgrades;
                this.btnEditSG.IsEnabled = true;
                this.btnDeleteSG.IsEnabled = true;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
