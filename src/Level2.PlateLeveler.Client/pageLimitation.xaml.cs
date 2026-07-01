using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.Client {
    /// <summary>
    /// Interaktionslogik für pageLimitation.xaml
    /// </summary>
    public delegate void SendLimitEventHandler(object sender, EventArgs e);

    public partial class pageLimitation : Page, IDisposable {
        public event SendLimitEventHandler SendLimit;
        private LEVELEREntities entity;
        private readonly CollectionViewSource _LimitControlViewSource;
        private CollectionViewSource _LimitViewSource;
        private Limitation _Limitation;

        public pageLimitation() {
            this.InitializeComponent();
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            this._LimitControlViewSource = (CollectionViewSource)this.FindResource("LimitControlDataViewSource");
            this._LimitViewSource = (CollectionViewSource)this.FindResource("LimitDataViewSource");
            this.InitEntity(this.entity);
        }

        private void InitEntity(LEVELEREntities ent) {
            this._LimitViewSource = (CollectionViewSource)this.FindResource("LimitDataViewSource");
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            _ = this.entity.Limitations.ToList();
            this._LimitViewSource.Source = this.entity.Limitations.Local;
        }

        private void SetEnabled(bool bEnabled) {
            this.txtLimitation.IsEnabled = bEnabled;
            this.txtValue.IsEnabled = bEnabled;
            this.btnCreate.IsEnabled = !bEnabled;
            this.btnDelete.IsEnabled = !bEnabled;
            this.btnEdit.IsEnabled = !bEnabled;
            this.btnSave.IsEnabled = bEnabled;
            this.btnCancel.IsEnabled = bEnabled;
        }

        private void btnCreate_Click(object sender, RoutedEventArgs e) {
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            this._Limitation = new Limitation {
                Value = 0
            };

            if (!this.entity.Limitations.Local.Any()) {
                this.entity.Limitations.Local.Add(this._Limitation);
            }

            this._LimitControlViewSource.Source = this.entity.Limitations.Local;
            this.SetEnabled(true);
            //InitEntity(entity);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e) {
            _ = this.entity.SaveChanges();
            SendLimit(this, new EventArgs());
            this.SetEnabled(false);
            this.InitEntity(this.entity);
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e) => this.SetEnabled(true);

        private void btnDelete_Click(object sender, RoutedEventArgs e) {
            this.SetEnabled(false);
            try {
                var result = MessageBox.Show("Do you want to delete this item?", "Limitation", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes) {
                    this.entity?.Dispose();

                    this.entity = new LEVELEREntities();
                    _ = this.entity.Limitations.Attach(this._Limitation);
                    _ = this.entity.Limitations.Remove(this._Limitation);
                    _ = this.entity.SaveChanges();
                    this.InitEntity(this.entity);
                }
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e) => this.SetEnabled(false);

        private void gridLimit_SelectedItemsChanged(object sender, Infragistics.Windows.DataPresenter.Events.SelectedItemsChangedEventArgs e) {
            this.entity?.Dispose();

            this.entity = new LEVELEREntities();
            if (this.gridLimit.ActiveDataItem != null) {
                this._Limitation = (Limitation)this.gridLimit.ActiveDataItem;
                try {
                    _ = this.entity.Limitations.Single(l => l.Pkey_Limitation == this._Limitation.Pkey_Limitation);
                } catch { }
            }
            this._LimitControlViewSource.Source = this.entity.Limitations.Local;

            this.btnEdit.IsEnabled = true;
            this.btnDelete.IsEnabled = true;
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
