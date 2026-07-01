using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.Client {
    /// <summary>
    /// Interaktionslogik für pagePresettings.xaml
    /// </summary>
    public partial class pagePresettings : Page, IDisposable {
        private LEVELEREntities entity;
        private readonly CollectionViewSource _PresettingsControlViewSource;
        private CollectionViewSource _PresettingsViewSource;
        private readonly CollectionViewSource _LimitViewSource;
        private PreSetting _Presettings;
        private readonly Limitation _Limit;

        public pagePresettings() {
            this.InitializeComponent();
            this.entity = new LEVELEREntities();
            this._PresettingsControlViewSource = (CollectionViewSource)this.FindResource("PreSettingsControlDataViewSource");
            this._LimitViewSource = (CollectionViewSource)this.FindResource("LimitViewSource");
            this._Limit = this.entity.Limitations.FirstOrDefault(l => l.Limit == "Plastification");

            if (this._Limit != null) {
                this._LimitViewSource.Source = this.entity.Limitations.Local;
                this._Presettings = new PreSetting();
                this.InitEntity(true);
                //SetEnabledPlast(false);
            }
        }

        public void InitEntity(bool updateWhileInEdit = false) {
            if (updateWhileInEdit || this.btnRefresh.IsEnabled) // btnRefresh is used as indicator for edit mode
            {
                this._PresettingsViewSource = (CollectionViewSource)this.FindResource("PreSettingsDataViewSource");
                this.entity = new LEVELEREntities();
                this.entity.PreSettings.PrintToList();
                if (!this.entity.PreSettings.Local.Any()) {
                    this.entity.PreSettings.Local.Add(new PreSetting());
                }

                this._PresettingsViewSource.Source = this.entity.PreSettings.Local;
            }
        }

        private void SetEnabled(bool bEnabled) {
            this.txtCassetteNo.IsEnabled = bEnabled;
            this.txtCenterHeightBending.IsEnabled = bEnabled;
            this.txtCorrPlastification.IsEnabled = bEnabled;
            this.txtCurveProfile.IsEnabled = bEnabled;
            this.txtEndRangeThickness.IsEnabled = bEnabled;
            this.txtEndRangeWidth.IsEnabled = bEnabled;
            this.txtMaterialID.IsEnabled = bEnabled;
            this.txtStartRangeThickness.IsEnabled = bEnabled;
            this.txtStartRangeWidth.IsEnabled = bEnabled;
            this.txtTiltLeft.IsEnabled = bEnabled;
            this.txtTiltRight.IsEnabled = bEnabled;
            this.chkEnable.IsEnabled = bEnabled;

            this.btnCopy.IsEnabled = !bEnabled;
            this.btnCreate.IsEnabled = !bEnabled;
            this.btnDelete.IsEnabled = !bEnabled;
            this.btnEdit.IsEnabled = !bEnabled;
            this.btnSave.IsEnabled = bEnabled;
            this.btnCancel.IsEnabled = bEnabled;
            this.btnRefresh.IsEnabled = !bEnabled;

            this.gridPresettings.IsEnabled = !bEnabled;
        }

        //private void SetEnabledPlast(bool bEnabled)
        //{
        //   txtPlastification.IsEnabled = bEnabled;
        //   btnCancelPlast.IsEnabled = bEnabled;
        //   btnEditPlast.IsEnabled = !bEnabled;
        //   btnSavePlast.IsEnabled = bEnabled;
        //}

        private void btnCreate_Click(object sender, RoutedEventArgs e) {
            this.entity = new LEVELEREntities();
            this._Presettings = new PreSetting();
            if (!this.entity.PreSettings.Local.Any()) {
                this.entity.PreSettings.Local.Add(this._Presettings);
            }

            this._PresettingsControlViewSource.Source = this.entity.PreSettings.Local;
            this.SetEnabled(true);
            this.chkEnable.IsChecked = true;
            //InitEntity(entity);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            try {
                var result = MessageBox.Show("Do you want to save this item?", "Presettings", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) {
                    // check for selection
                    var hadSelectedObject = false;
                    if (this.gridPresettings.ActiveDataItem != null) {
                        this._Presettings = (PreSetting)this.gridPresettings.ActiveDataItem;
                        hadSelectedObject = true;
                    }

                    _ = this.entity.SaveChanges();
                    this.SetEnabled(false);
                    this.InitEntity(true);

                    // Reselect the correct item
                    if (hadSelectedObject) {
                        this.gridPresettings.ActiveDataItem = ((ObservableCollection<PreSetting>)this._PresettingsViewSource.Source).First(p => p.PKEY_SETTINGS == this._Presettings.PKEY_SETTINGS);
                        this.gridPresettings.SelectedDataItem = this.gridPresettings.ActiveDataItem;
                    }
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e) {

            if (this.entity.PreSettings.Local.Any()) {
                var ps = this.entity.PreSettings.Local[0];
                ps.PKEY_SETTINGS = 0;

                this.entity = new LEVELEREntities();
                this.entity.PreSettings.Local.Add(ps);
                this._PresettingsControlViewSource.Source = this.entity.PreSettings.Local;
                this.SetEnabled(true);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) {
            if (this.gridPresettings.ActiveDataItem is PreSetting preSetting && preSetting.PKEY_SETTINGS > 0) {
                this.SetEnabled(true);
            } else {
                this.SetEnabled(false);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            this.SetEnabled(false);
            try {
                var result = MessageBox.Show("Do you want to delete this item?", "Presettings", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) {
                    this.entity = new LEVELEREntities();
                    _ = this.entity.PreSettings.Attach(this._Presettings);
                    _ = this.entity.PreSettings.Remove(this._Presettings);
                    _ = this.entity.SaveChanges();
                    this.InitEntity(true);
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.SetEnabled(false);

            // Update the grid elements and revert the changes
            if (this.gridPresettings.ActiveDataItem != null) {
                this._Presettings = (PreSetting)this.gridPresettings.ActiveDataItem;
                this.gridPresettings.SelectedDataItem = null;
                this.gridPresettings.SelectedDataItem = this._Presettings;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e) {
            try {
                this.InitEntity(true);
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void gridPresettings_SelectedItemsChanged(object sender, Infragistics.Windows.DataPresenter.Events.SelectedItemsChangedEventArgs e) {
            this.entity = new LEVELEREntities();
            if (this.gridPresettings.ActiveDataItem != null) {
                this._Presettings = (PreSetting)this.gridPresettings.ActiveDataItem;
                try {
                    _ = this.entity.PreSettings.Single(ps => ps.PKEY_SETTINGS == this._Presettings.PKEY_SETTINGS);
                } catch { }
            }
            this._PresettingsControlViewSource.Source = this.entity.PreSettings.Local;
            this.btnCopy.IsEnabled = true;
            this.btnEdit.IsEnabled = true;
            this.btnDelete.IsEnabled = true;
        }

        public void Dispose() => throw new NotImplementedException();

        //private void btnSavePlast_Click(object sender, RoutedEventArgs e)
        //{
        //   MessageBoxResult result = MessageBox.Show("Do you want to save this item?", "Limit", MessageBoxButton.YesNo);
        //   if (result == MessageBoxResult.Yes)
        //   {
        //      entity.SaveChanges();
        //      SetEnabledPlast(false);
        //   }
        //}

        //private void btnEditPlast_Click(object sender, RoutedEventArgs e)
        //{
        //   SetEnabledPlast(true);
        //   entity = new LVL6Entities();
        //   _Limit = entity.Limitations.Single(l => l.Limit == "Plastification");
        //   _LimitViewSource.Source = entity.Limitation;
        //}

        //private void btnCancelPlast_Click(object sender, RoutedEventArgs e)
        //{
        //   SetEnabledPlast(false);
        //}
    }
}
