using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.Client {
    /// <summary>
    /// Interaktionslogik für pageCassette.xaml
    /// </summary>
    public partial class pageCassette : Page, IDisposable {
        private LEVELEREntities entity;
        private readonly CollectionViewSource _CassetteControlViewSource;
        private CollectionViewSource _CassetteViewSource;
        private Cassette _Cassette;
        private readonly InitData _Initialization;

        public pageCassette(InitData init) {
            this.InitializeComponent();
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            this._CassetteControlViewSource = (CollectionViewSource)this.FindResource("CassetteControlDataViewSource");
            this._Cassette = new Cassette();
            this.InitEntity(this.entity);
            this._Initialization = init;
        }

        private void InitEntity(LEVELEREntities ent) {
            this._CassetteViewSource = (CollectionViewSource)this.FindResource("CassetteDataViewSource");
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            this.entity.Cassettes.PrintToList();
            if (this.entity.Cassettes.Local.Count.Equals(0)) {
                this.entity.Cassettes.Local.Add(new Cassette());
            }

            this._CassetteViewSource.Source = this.entity.Cassettes.Local;
        }

        private void SetEnabled(bool bEnabled) {
            this.txtBearingDiameter.IsEnabled = bEnabled;
            this.txtDistanceA.IsEnabled = bEnabled;
            this.txtDistanceB.IsEnabled = bEnabled;
            this.txtDistanceC.IsEnabled = bEnabled;
            this.txtMaxAdjustment.IsEnabled = bEnabled;
            this.txtMaxCenterHeightBending.IsEnabled = bEnabled;
            this.txtMaxCrossTiltLeft.IsEnabled = bEnabled;
            this.txtMaxCrossTiltRight.IsEnabled = bEnabled;
            this.txtMaxLevelingPressure.IsEnabled = bEnabled;
            this.txtMaxMotorPower.IsEnabled = bEnabled;
            this.txtMaxMotorTorque.IsEnabled = bEnabled;
            this.txtMaxThicknessPlate.IsEnabled = bEnabled;
            this.txtMinCenterHeightBending.IsEnabled = bEnabled;
            this.txtMinCrossTiltLeft.IsEnabled = bEnabled;
            this.txtMinCrossTiltRight.IsEnabled = bEnabled;
            this.txtMinThicknessPlate.IsEnabled = bEnabled;
            this.txtNoOfRolls.IsEnabled = bEnabled;
            this.txtPitchOfRolls.IsEnabled = bEnabled;
            this.txtRollDiameter.IsEnabled = bEnabled;
            this.chkActive.IsEnabled = bEnabled;

            this.btnCopy.IsEnabled = !bEnabled;
            this.btnCreate.IsEnabled = !bEnabled;
            this.btnDelete.IsEnabled = !bEnabled;
            this.btnEdit.IsEnabled = !bEnabled;
            this.btnSave.IsEnabled = bEnabled;
            this.btnCancel.IsEnabled = bEnabled;

            this.gridCassette.IsEnabled = !bEnabled;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e) {
            this.entity = new LEVELEREntities();
            this._Cassette = new Cassette();
            if (!this.entity.Cassettes.Local.Any()) {
                this.entity.Cassettes.Local.Add(this._Cassette);
            }

            this._CassetteControlViewSource.Source = this.entity.Cassettes.Local;
            this.SetEnabled(true);
            //InitEntity(entity);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            try {
                var result = MessageBox.Show("Do you want to save this item?", "Cassette", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) {
                    // check for selection
                    var hadSelectedObject = false;
                    if (this.gridCassette.ActiveDataItem != null) {
                        this._Cassette = (Cassette)this.gridCassette.ActiveDataItem;
                        hadSelectedObject = true;
                    }

                    _ = this.entity.SaveChanges();
                    this.SetEnabled(false);
                    this.InitEntity(this.entity);

                    // Reselect the correct item
                    if (hadSelectedObject) {
                        this.gridCassette.ActiveDataItem = ((ObservableCollection<Cassette>)this._CassetteViewSource.Source).First(p => p.CassetteNo == this._Cassette.CassetteNo);
                        this.gridCassette.SelectedDataItem = this.gridCassette.ActiveDataItem;
                    }
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e) {
            if (this.entity.Cassettes.Local.Any()) {
                var cassette = this.entity.Cassettes.Local[0];
                cassette.CassetteNo = 0;
                this.entity?.Dispose();

                this.entity = new LEVELEREntities();
                this.entity.Cassettes.Local.Add(cassette);
                this._CassetteControlViewSource.Source = this.entity.Cassettes.Local;
                this.SetEnabled(true);
            }
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) => this.SetEnabled(true);

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            this.SetEnabled(false);
            try {
                var result = MessageBox.Show("Do you want to delete this item?", "Cassette", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) {
                    this.entity?.Dispose();

                    this.entity = new LEVELEREntities();
                    _ = this.entity.Cassettes.Attach(this._Cassette);
                    _ = this.entity.Cassettes.Remove(this._Cassette);
                    _ = this.entity.SaveChanges();
                    this.InitEntity(this.entity);
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) {
            this.SetEnabled(false);

            // Update the grid elements and revert the changes
            if (this.gridCassette.ActiveDataItem != null) {
                this._Cassette = (Cassette)this.gridCassette.ActiveDataItem;
                this.gridCassette.SelectedDataItem = null;
                this.gridCassette.SelectedDataItem = this._Cassette;
            }
        }

        private void gridCassette_SelectedItemsChanged(object sender, Infragistics.Windows.DataPresenter.Events.SelectedItemsChangedEventArgs e) {
            this.entity = new LEVELEREntities();
            if (this.gridCassette.ActiveDataItem != null) {
                this._Cassette = this.gridCassette.ActiveDataItem == null ? new Cassette() : (Cassette)this.gridCassette.ActiveDataItem;
                try {
                    _ = this.entity.Cassettes.Single(c => c.CassetteNo == this._Cassette.CassetteNo);
                } catch { }
            }
            this._CassetteControlViewSource.Source = this.entity.Cassettes.Local;
            this.btnCopy.IsEnabled = true;
            this.btnEdit.IsEnabled = true;
            this.btnDelete.IsEnabled = true;
        }

        public void SetActiveCassette(int cassetteID) => this.InvokeText(this.txtActiveCassette, cassetteID.ToString());

        private void InvokeText(TextBlock control, string text) {
            this.Dispatcher.Invoke(new Action(delegate () {
                control.Text = text;
            }));
        }

        private void txtMaxThicknessPlate_TextChanged(object sender, TextChangedEventArgs e) {
            try {
                if (this._Cassette.MaxSelectThickness.HasValue) {
                    if (Convert.ToSingle(this.txtMaxThicknessPlate.Text) > this._Cassette.MaxSelectThickness.Value) {
                        this.txtMaxThicknessPlate.Text = this._Cassette.MaxSelectThickness.Value.ToString();
                    }
                }
            } catch { }
        }

        private void txtMaxThicknessPlate_KeyDown(object sender, KeyEventArgs e) {
            try {
                if (e.Key == Key.Return) {
                    if (e.KeyStates.HasFlag(KeyStates.Toggled)) {
                        //_Cassette = (Cassette)gridCassette.ActiveDataItem;
                        this._Cassette = this.entity.Cassettes.Local[0];
                        this._Cassette.MaxSelectThickness = Convert.ToSingle(this.txtMaxThicknessPlate.Text);
                        _ = this.entity.SaveChanges();
                    }
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
