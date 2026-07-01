using System.Windows;
using Level2.PlateLeveler.Client.Cultures;

namespace Level2.PlateLeveler.Client {
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public sealed partial class App : Application, System.IDisposable {
        private MainWindow window;
        private void Application_Startup(object sender, StartupEventArgs e) {
            this.window = new MainWindow();
            this.window.CultureChanged += this.window_CultureChanged;
            Current.MainWindow = this.window;
            Current.ShutdownMode = ShutdownMode.OnMainWindowClose;
            CultureResources.ChangeCulture(Client.Properties.Settings.Default.DefaultCulture);

            this.window.Show();
        }

        private void window_CultureChanged(object sender, DataTypes.CultureChangeEventArgs e) {
            if (Cultures.Resources.Culture != null && !Cultures.Resources.Culture.Equals(e.LangInfo)) {
                //change resources to new culture
                CultureResources.ChangeCulture(e.LangInfo);
            }
        }

        public void Dispose() => throw new System.NotImplementedException();
    }
}
