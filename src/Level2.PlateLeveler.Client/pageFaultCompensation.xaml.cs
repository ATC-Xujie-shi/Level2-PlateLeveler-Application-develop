using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Level2.PlateLeveler.DataConverter;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.Client {
    /// <summary>
    /// Interaktionslogik für pageFaultCompensation.xaml
    /// </summary>
    public partial class pageFaultCompensation : Page, IDisposable {
        private LEVELEREntities entity;
        private readonly CollectionViewSource _FaultCompensationControlViewSource;
        private CollectionViewSource _FaultCompensationViewSource;
        private readonly CollectionViewSource _PriorityViewSource;
        private FaultCompensation _FaultCompensation;
        private readonly PriorityConverter _Converter;
        //List<FaultCompensation> FaultCompensationItems = new List<FaultCompensation>();

        public pageFaultCompensation() {
            this.InitializeComponent();
            this.entity = new LEVELEREntities();
            this._FaultCompensationControlViewSource = (CollectionViewSource)this.FindResource("FCControlDataViewSource");
            this._PriorityViewSource = (CollectionViewSource)this.FindResource("PriorityDataViewSource");
            this._FaultCompensation = new FaultCompensation();
            this._Converter = new PriorityConverter();
            this.InitEntity(this.entity);
        }

        private void InitEntity(LEVELEREntities ent) {
            this._FaultCompensationViewSource = (CollectionViewSource)this.FindResource("FCDataViewSource");
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            //foreach (FaultCompensation item in entity.FaultCompensations)
            //{
            //   FaultCompensationItems.Add(item);
            //}
            //dataGridFaultCompensation.ItemsSource = FaultCompensationItems;
            this.entity.FaultCompensations.PrintToList();
            if (this.entity.FaultCompensations.Local.Count.Equals(0)) {
                this.entity.FaultCompensations.Local.Add(new FaultCompensation());
            }

            this._FaultCompensationViewSource.Source = this.entity.FaultCompensations.Local;
        }

        private void SetEnabled(bool bEnabled) {
            this.txtCassetteNo.IsEnabled = bEnabled;
            this.txtEvaluationCenterWave.IsEnabled = bEnabled;
            this.txtEvaluationEdgeWave.IsEnabled = bEnabled;
            this.txtEvaluationSki.IsEnabled = bEnabled;
            this.txtEvaluationTilting.IsEnabled = bEnabled;
            this.txtMaxHeightCenterWave.IsEnabled = bEnabled;
            this.txtMaxHeightEdgeWave.IsEnabled = bEnabled;
            this.txtMaxSki.IsEnabled = bEnabled;
            this.txtMaxTilting.IsEnabled = bEnabled;
            this.gridPriority.FieldLayouts[0].Fields[1].Settings.AllowEdit = bEnabled;

            this.btnCreate.IsEnabled = !bEnabled;
            this.btnDelete.IsEnabled = bEnabled;
            this.btnEdit.IsEnabled = !bEnabled;
            this.btnSave.IsEnabled = bEnabled;
            this.btnCancel.IsEnabled = bEnabled;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e) {
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();

            this._FaultCompensation = new FaultCompensation();
            this.SetPriorityList();
            if (!this.entity.FaultCompensations.Local.Any()) {
                this.entity.FaultCompensations.Local.Add(this._FaultCompensation);
            }

            this._FaultCompensationControlViewSource.Source = this.entity.FaultCompensations.Local;
            this.SetEnabled(true);
            this.InitEntity(this.entity);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            try {
                var result = MessageBox.Show("Do you want to save this item?", "FaultCompensation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) {
                    var fc = this._Converter.LoadFaultCompensation((IEnumerable<PrioritiesAttribute>)this.gridPriority.DataSource);
                    this.entity.FaultCompensations.Local[0].CenterWave = fc.CenterWave;
                    this.entity.FaultCompensations.Local[0].EdgeWave = fc.EdgeWave;
                    this.entity.FaultCompensations.Local[0].Tilting = fc.Tilting;
                    this.entity.FaultCompensations.Local[0].Ski = fc.Ski;
                    _ = this.entity.SaveChanges();
                    this.SetEnabled(false);
                    this.InitEntity(this.entity);
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
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
                    _ = this.entity.FaultCompensations.Attach(this._FaultCompensation);
                    _ = this.entity.FaultCompensations.Remove(this._FaultCompensation);
                    _ = this.entity.SaveChanges();
                    this.InitEntity(this.entity);
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) => this.SetEnabled(false);

        private void btnCopy_Click(object sender, RoutedEventArgs e) {
            if (this.entity.FaultCompensations.Any()) {
                var fc = this.entity.FaultCompensations.Local[0];
                fc.PKEY_FC = 0;
                fc.CassetteNo = 0;
                this.entity?.Dispose();

                this.entity = new LEVELEREntities();
                this.entity.FaultCompensations.Local.Add(fc);
                this._FaultCompensationControlViewSource.Source = this.entity.FaultCompensations.Local;
                this.SetEnabled(true);
            }
        }

        private void gridFC_SelectedItemsChanged(object sender, Infragistics.Windows.DataPresenter.Events.SelectedItemsChangedEventArgs e) {
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            this._FaultCompensation = this.gridFC.ActiveDataItem == null ? new FaultCompensation() : (FaultCompensation)this.gridFC.ActiveDataItem;
            if (this._FaultCompensation != null) {
                if (this._FaultCompensation.PKEY_FC > 0) {
                    this.SetPriorityList();
                    try {
                        _ = this.entity.FaultCompensations.Single(fc => fc.PKEY_FC == this._FaultCompensation.PKEY_FC);
                    } catch { }
                    this._FaultCompensationControlViewSource.Source = this.entity.FaultCompensations.Local;
                } else {
                    this.gridPriority.DataSource = null;
                }
            } else {
                this.gridPriority.DataSource = null;
            }

            this.btnCopy.IsEnabled = true;
            this.btnEdit.IsEnabled = true;
            this.btnDelete.IsEnabled = true;
        }

        private void SetPriorityList() {
            var pl = this._Converter.LoadPriorities(this._FaultCompensation);
            var list = pl.OrderBy(p => p.PriorityID);
            this.gridPriority.DataSource = list;
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
