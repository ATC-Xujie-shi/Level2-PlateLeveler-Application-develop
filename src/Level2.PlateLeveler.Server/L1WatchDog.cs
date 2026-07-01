using System;
using System.Timers;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.Server {
    public class L1WatchDog : IDisposable {
        private readonly InitData _initData;
        private readonly Timer _Timer;

        public L1WatchDog(InitData init) {
            try {
                this._initData = init;

                this._Timer = new Timer(this._initData.Interval.WatchDogRefresh);
                this._Timer.Elapsed += this._Timer_Elapsed;
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        private void _Timer_Elapsed(object sender, ElapsedEventArgs e) {
            try {
                this._Timer.Stop();

                this.Listener.L2_L1_Watchdog();

                this._Timer.Start();
            } catch (Exception ex) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, ex, this.GetType());
            }
        }

        public void Start() => this._Timer.Start();

        public void Stop() => this._Timer.Stop();

        public IL1WatchDog Listener { get; set; }

        public void Dispose() => throw new NotImplementedException();
    }
}
