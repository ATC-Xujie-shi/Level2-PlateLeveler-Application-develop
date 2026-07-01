using Level2.PlateLeveler.DataTypes;
using Level2.PlateLeveler.Model;

namespace Level2.PlateLeveler.Server {
    public class PredictionModel(IPlateCalc plateCalculation, StoodBoltType selectedStoodBoltType) {
        private const int StartPlastification = 100;
        private readonly StoodBoltType _actualStoodBoltType = selectedStoodBoltType;
        private readonly IPlateCalc _plateCalculation = plateCalculation;

        public int MaxAllowedError { get; set; }
        public float TargetValue { get; set; }

        public SettingsData InitialSettingsData { get; set; }
        public Cassette InitialCassette { get; set; }
        public PreSettingsOutlet InitialPreSettingsOutlet { get; set; }
        public LimitationData InitialLimitationData { get; set; }

        public double GetPredictedPlastification(out float levelerIN, out float levelerOut) {
            var _maxError = 1 - (this.MaxAllowedError / 100.0);
            double _reqValue = this.TargetValue;

            var _startPlastification = StartPlastification;

            this.RequestPrediction(_startPlastification, out var levelerINResult, out var levelerOutResult);
            //double lastCalValue = 0;
            var lastErrorOffset = levelerINResult / _reqValue;
            if (_reqValue < 0) {
                while (lastErrorOffset > _maxError) {
                    _startPlastification--;
                    this.RequestPrediction(_startPlastification, out levelerINResult, out levelerOutResult);

                    lastErrorOffset = levelerINResult / _reqValue;

                    if (_startPlastification <= 0) {
                        break;
                    }
                }
            } else {
                while (lastErrorOffset < _maxError) {
                    _startPlastification--;
                    this.RequestPrediction(_startPlastification, out levelerINResult, out levelerOutResult);

                    lastErrorOffset = levelerINResult / _reqValue;

                    if (_startPlastification <= 0) {
                        break;
                    }
                }
            }

            levelerIN = levelerINResult;
            levelerOut = levelerOutResult;

            return _startPlastification;
        }

        private void RequestPrediction(int plastification, out float levelerIN, out float levelerOut) {
            var _SettingsData = this.GetSettingsData(plastification);
            var cassette = this.GetCasseteData(_SettingsData.CassetteNo);
            var outlet = this.GetOutLetData();

            var _Limitation = this.GetLimtationData();

            _SettingsData = (SettingsData)this._plateCalculation.CalcLevelerData(_SettingsData, cassette, outlet, _Limitation, this._actualStoodBoltType);

            levelerIN = _SettingsData.LevelerInlet;
            levelerOut = _SettingsData.LevelerOutlet;
        }

        private SettingsData GetSettingsData(float requestedPlastification) {
            var _SettingsData = this.InitialSettingsData;
            _SettingsData.Plastification = requestedPlastification;

            return _SettingsData;
        }

        private Cassette GetCasseteData(int casseteNo) {
            var cassette = this.InitialCassette;

            return cassette;
        }

        private PreSettingsOutlet GetOutLetData() {
            var outlet = this.InitialPreSettingsOutlet;

            return outlet;
        }

        private LimitationData GetLimtationData() {
            var _Limitation = this.InitialLimitationData;

            return _Limitation;
        }
    }
}
