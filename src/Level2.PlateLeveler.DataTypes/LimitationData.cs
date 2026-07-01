using System.ComponentModel;

namespace Level2.PlateLeveler.DataTypes {
    public class LimitationData : INotifyPropertyChanged, ILimitationData {
        [Limitation("Plastification", 2)]
        public float Plastification {
            get; set => PropertyChanged.ChangeAndNotify(ref field, value, () => this.Plastification);
        }
        [Limitation("MinPlastification", 8)]
        public float MinPlastification {
            get; set => PropertyChanged.ChangeAndNotify(ref field, value, () => this.MinPlastification);
        }
        [Limitation("MinCoilLength", 1)]
        public float MinCoilLength {
            get; set => PropertyChanged.ChangeAndNotify(ref field, value, () => this.MinCoilLength);
        }
        [Limitation("EModule", 3)]
        public float EModule {
            get; set => PropertyChanged.ChangeAndNotify(ref field, value, () => this.EModule);
        }
        [Limitation("IsFaultCompensation", 4)]
        public bool IsFaultCompensation {
            get; set => PropertyChanged.ChangeAndNotify(ref field, value, () => this.IsFaultCompensation);
        }
        [Limitation("MaxYieldPoint", 5)]
        public float MaxYieldPoint {
            get; set => PropertyChanged.ChangeAndNotify(ref field, value, () => this.MaxYieldPoint);
        }
        [Limitation("MinWidth", 6)]
        public float MinWidth {
            get; set => PropertyChanged.ChangeAndNotify(ref field, value, () => this.MinWidth);
        }
        [Limitation("MaxWidth", 7)]
        public float MaxWidth {
            get; set => PropertyChanged.ChangeAndNotify(ref field, value, () => this.MaxWidth);
        }
        [Limitation("FrictionCoefficient", 8)]
        public float FrictionCoefficient { get; set; }
        [Limitation("RollFrictionCoefficient", 9)]
        public float RollFrictionCoefficient { get; set; }
        [Limitation("RollFrictionMoment", 10)]
        public float RollFrictionMoment { get; set; }
        [Limitation("ConveyorSpeed", 11)]
        public float ConveyorSpeed { get; set; }
        [Limitation("Drive", 12)]
        public float Drive { get; set; }
        [Limitation("GearEfficiencyFactor", 13)]
        public float GearEfficiencyFactor { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
