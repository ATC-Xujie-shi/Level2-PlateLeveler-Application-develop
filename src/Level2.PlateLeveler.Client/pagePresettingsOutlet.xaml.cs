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
    /// Interaktionslogik für pagePresettingsOutlet.xaml
    /// </summary>
    public partial class pagePresettingsOutlet : Page, IDisposable {
        private LEVELEREntities entity;
        private readonly CollectionViewSource _PresettingsOutletControlViewSource;
        private CollectionViewSource _PresettingsOutletViewSource;
        private PreSettingsOutlet _PresettingsOutlet;

        public pagePresettingsOutlet() {
            this.InitializeComponent();

            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            this._PresettingsOutletControlViewSource = (CollectionViewSource)this.FindResource("PresettingsOutletControlDataViewSource");
            this._PresettingsOutlet = new PreSettingsOutlet();
            this.InitEntity(true);
        }

        public void InitEntity(bool updateWhileInEdit = false) {
            if (updateWhileInEdit || this.btnRefresh.IsEnabled) // btnRefresh is used as indicator for edit mode
            {
                this._PresettingsOutletViewSource = (CollectionViewSource)this.FindResource("PresettingsOutletDataViewSource");
                this.entity?.Dispose();

                this.entity = new LEVELEREntities();

                this.entity.PreSettingsOutlets.PrintToList();
                if (!this.entity.PreSettingsOutlets.Local.Any()) {
                    this.entity.PreSettingsOutlets.Local.Add(new PreSettingsOutlet());
                }

                this._PresettingsOutletViewSource.Source = this.entity.PreSettingsOutlets.Local;
            }
        }

        private void SetEnabled(bool bEnabled) {
            this.txtEndRangeActYieldPoint.IsEnabled = bEnabled;
            this.txtEndRangeThickness.IsEnabled = bEnabled;
            this.txtOffset.IsEnabled = bEnabled;
            this.txtStartRangeActYieldPoint.IsEnabled = bEnabled;
            this.txtStartRangeThickness.IsEnabled = bEnabled;

            this.btnCreate.IsEnabled = !bEnabled;
            this.btnCopy.IsEnabled = !bEnabled;
            this.btnDelete.IsEnabled = !bEnabled;
            this.btnEdit.IsEnabled = !bEnabled;
            this.btnSave.IsEnabled = bEnabled;
            this.btnCancel.IsEnabled = bEnabled;
            this.btnRefresh.IsEnabled = !bEnabled;

            this.gridPresettingsOutlet.IsEnabled = !bEnabled;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e) {
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            this._PresettingsOutlet = new PreSettingsOutlet();
            if (!this.entity.PreSettingsOutlets.Local.Any()) {
                this.entity.PreSettingsOutlets.Local.Add(this._PresettingsOutlet);
            }

            this._PresettingsOutletControlViewSource.Source = this.entity.PreSettingsOutlets.Local;
            this.SetEnabled(true);
            //InitEntity(entity);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            try {
                var result = MessageBox.Show("Do you want to save this item?", "Presettings Outlet", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) {
                    // check for selection
                    var hadSelectedObject = false;
                    if (this.gridPresettingsOutlet.ActiveDataItem != null) {
                        this._PresettingsOutlet = (PreSettingsOutlet)this.gridPresettingsOutlet.ActiveDataItem;
                        hadSelectedObject = true;
                    }

                    _ = this.entity.SaveChanges();
                    this.SetEnabled(false);
                    this.InitEntity(true);

                    // Reselect the correct item
                    if (hadSelectedObject) {
                        this.gridPresettingsOutlet.ActiveDataItem = ((ObservableCollection<PreSettingsOutlet>)this._PresettingsOutletViewSource.Source).First(p => p.Pkey_SettingsOutlet == this._PresettingsOutlet.Pkey_SettingsOutlet);
                        this.gridPresettingsOutlet.SelectedDataItem = this.gridPresettingsOutlet.ActiveDataItem;
                    }
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) {
            if (this.gridPresettingsOutlet.ActiveDataItem is PreSettingsOutlet outlet && outlet.Pkey_SettingsOutlet > 0) {
                this.SetEnabled(true);
            } else {
                this.SetEnabled(false);
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e) {
            if (this.entity.PreSettingsOutlets.Local.Any()) {
                var ps = this.entity.PreSettingsOutlets.Local[0];
                ps.Pkey_SettingsOutlet = 0;
                ps.Offset = 0;
                this.entity?.Dispose();

                this.entity = new LEVELEREntities();
                this.entity.PreSettingsOutlets.Local.Add(ps);
                this._PresettingsOutletControlViewSource.Source = this.entity.PreSettingsOutlets.Local;
                this.SetEnabled(true);
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            this.SetEnabled(false);
            try {
                var result = MessageBox.Show("Do you want to delete this item?", "Presettings Outlet", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) {
                    this.entity?.Dispose();

                    this.entity = new LEVELEREntities();
                    _ = this.entity.PreSettingsOutlets.Attach(this._PresettingsOutlet);
                    _ = this.entity.PreSettingsOutlets.Remove(this._PresettingsOutlet);
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
            if (this.gridPresettingsOutlet.ActiveDataItem != null) {
                this._PresettingsOutlet = (PreSettingsOutlet)this.gridPresettingsOutlet.ActiveDataItem;
                this.gridPresettingsOutlet.SelectedDataItem = null;
                this.gridPresettingsOutlet.SelectedDataItem = this._PresettingsOutlet;
            }
        }

        private void btnRefresh_Click(object sender, RoutedEventArgs e) {
            try {
                this.InitEntity(true);
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void gridPresettingsOutlet_SelectedItemsChanged(object sender, Infragistics.Windows.DataPresenter.Events.SelectedItemsChangedEventArgs e) {
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            if (this.gridPresettingsOutlet.ActiveDataItem != null) {
                this._PresettingsOutlet = (PreSettingsOutlet)this.gridPresettingsOutlet.ActiveDataItem;
                try {
                    _ = this.entity.PreSettingsOutlets.Single(po => po.Pkey_SettingsOutlet == this._PresettingsOutlet.Pkey_SettingsOutlet);
                } catch { }
            }
            this._PresettingsOutletControlViewSource.Source = this.entity.PreSettingsOutlets.Local;
            this.btnCopy.IsEnabled = true;
            this.btnEdit.IsEnabled = true;
            this.btnDelete.IsEnabled = true;
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
