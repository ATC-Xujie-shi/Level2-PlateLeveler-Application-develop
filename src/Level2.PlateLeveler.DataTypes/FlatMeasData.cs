using System;
using System.Collections.Generic;

namespace Level2.PlateLeveler.DataTypes {
    public class FlatMeasData {
        public string PlateID { get; set; }
        public string MaterialID { get; set; }

        protected int _MeanAmpWaveLeft;
        public int MeanAmpWaveLeft {
            get => this._MeanAmpWaveLeft; set => this._MeanAmpWaveLeft = value;
        }

        protected short _MeasuringRange;
        public short MeasuringRange {
            get => this._MeasuringRange; set => this._MeasuringRange = value;
        }

        protected int _MeanAmpWaveCenter;
        public int MeanAmpWaveCenter {
            get => this._MeanAmpWaveCenter; set => this._MeanAmpWaveCenter = value;
        }

        protected short _MeasuringCode;
        public short MeasuringCode {
            get => this._MeasuringCode; set => this._MeasuringCode = value;
        }

        protected int _MeanAmpRight;
        public int MeanAmpRight {
            get => this._MeanAmpRight; set => this._MeanAmpRight = value;
        }

        protected int _HeightWaveLeft;
        public int HeightWaveLeft {
            get => this._HeightWaveLeft; set => this._HeightWaveLeft = value;
        }

        protected int _HeightWaveCenter;
        public int HeightWaveCenter {
            get => this._HeightWaveCenter; set => this._HeightWaveCenter = value;
        }

        protected int _SkiHead;
        public int SkiHead {
            get => this._SkiHead; set => this._SkiHead = value;
        }

        protected int _SkiTail;
        public int SkiTail {
            get => this._SkiTail; set => this._SkiTail = value;
        }

        protected int _Ruler1MLength;
        public int Ruler1MLength {
            get => this._Ruler1MLength; set => this._Ruler1MLength = value;
        }

        protected int _Ruler1MCross;
        public int Ruler1MCross {
            get => this._Ruler1MCross; set => this._Ruler1MCross = value;
        }

        protected int _Ruler2MLength;
        public int Ruler2MLength {
            get => this._Ruler2MLength; set => this._Ruler2MLength = value;
        }

        protected short _PlateInTol;
        public short PlateInTol {
            get => this._PlateInTol; set => this._PlateInTol = value;
        }

        protected int _YPosWaveLeft;
        public int YPosWaveLeft {
            get => this._YPosWaveLeft; set => this._YPosWaveLeft = value;
        }

        protected short _NumberWaveLeft;
        public short NumberWaveLeft {
            get => this._NumberWaveLeft; set => this._NumberWaveLeft = value;
        }

        protected int _LengthWaveLeft;
        public int LengthWaveLeft {
            get => this._LengthWaveLeft; set => this._LengthWaveLeft = value;
        }

        protected int _LengthWaveQuarterLeft;
        public int LengthWaveQuarterLeft {
            get => this._LengthWaveQuarterLeft; set => this._LengthWaveQuarterLeft = value;
        }

        protected int _NumberWaveQuarterLeft;
        public int NumberWaveQuarterLeft {
            get => this._NumberWaveQuarterLeft; set => this._NumberWaveQuarterLeft = value;
        }

        protected int _YPosWaveCenter;
        public int YPosWaveCenter {
            get => this._YPosWaveCenter; set => this._YPosWaveCenter = value;
        }

        protected int _HeightWaveQuarterLeft;
        public int HeightWaveQuarterLeft {
            get => this._HeightWaveQuarterLeft; set => this._HeightWaveQuarterLeft = value;
        }

        protected int _YPosWaveQuarterLeft;
        public int YPosWaveQuarterLeft {
            get => this._YPosWaveQuarterLeft; set => this._YPosWaveQuarterLeft = value;
        }

        protected int _LengthWaveCenter;
        public int LengthWaveCenter {
            get => this._LengthWaveCenter; set => this._LengthWaveCenter = value;
        }

        protected short _NumberWaveCenter;
        public short NumberWaveCenter {
            get => this._NumberWaveCenter; set => this._NumberWaveCenter = value;
        }

        protected int _HeightWaveQuarterRight;
        public int HeightWaveQuarterRight {
            get => this._HeightWaveQuarterRight; set => this._HeightWaveQuarterRight = value;
        }

        protected int _YPosWaveQuarterRight;
        public int YPosWaveQuarterRight {
            get => this._YPosWaveQuarterRight; set => this._YPosWaveQuarterRight = value;
        }

        protected int _LengthWaveQuarterRight;
        public int LengthWaveQuarterRight {
            get => this._LengthWaveQuarterRight; set => this._LengthWaveQuarterRight = value;
        }

        protected short _NumberWaveQuarterRight;
        public short NumberWaveQuarterRight {
            get => this._NumberWaveQuarterRight; set => this._NumberWaveQuarterRight = value;
        }

        protected int _HeightWaveRight;
        public int HeightWaveRight {
            get => this._HeightWaveRight; set => this._HeightWaveRight = value;
        }

        protected int _YPosWaveRight;
        public int YPosWaveRight {
            get => this._YPosWaveRight; set => this._YPosWaveRight = value;
        }

        protected short _NumberWaveRight;
        public short NumberWaveRight {
            get => this._NumberWaveRight; set => this._NumberWaveRight = value;
        }

        protected int _LengthWaveRight;
        public int LengthWaveRight {
            get => this._LengthWaveRight; set => this._LengthWaveRight = value;
        }

        protected int _GapLengthBow;
        public int GapLengthBow {
            get => this._GapLengthBow; set => this._GapLengthBow = value;
        }

        protected int _MaxGapLengthBow;
        public int MaxGapLengthBow {
            get => this._MaxGapLengthBow; set => this._MaxGapLengthBow = value;
        }

        protected int _XPosLengthBow;
        public int XPosLengthBow {
            get => this._XPosLengthBow; set => this._XPosLengthBow = value;
        }

        protected int _YPosLengthBow;
        public int YPosLengthBow {
            get => this._YPosLengthBow; set => this._YPosLengthBow = value;
        }

        protected int _GapCrossBow;
        public int GapCrossBow {
            get => this._GapCrossBow; set => this._GapCrossBow = value;
        }

        protected int _MaxGapCrossBow;
        public int MaxGapCrossBow {
            get => this._MaxGapCrossBow; set => this._MaxGapCrossBow = value;
        }

        protected int _XPosCrossBow;
        public int XPosCrossBow {
            get => this._XPosCrossBow; set => this._XPosCrossBow = value;
        }

        protected int _YPosCrossBow;
        public int YPosCrossBow {
            get => this._YPosCrossBow; set => this._YPosCrossBow = value;
        }

        protected int _ActRuler1MCross;
        public int ActRuler1MCross {
            get => this._ActRuler1MCross; set => this._ActRuler1MCross = value;
        }

        protected int _XPosRuler1MCross;
        public int XPosRuler1MCross {
            get => this._XPosRuler1MCross; set => this._XPosRuler1MCross = value;
        }

        protected int _YPosRuler1MCross;
        public int YPosRuler1MCross {
            get => this._YPosRuler1MCross; set => this._YPosRuler1MCross = value;
        }

        protected int _ActRuler1MLength;
        public int ActRuler1MLength {
            get => this._ActRuler1MLength; set => this._ActRuler1MLength = value;
        }

        protected int _XPosRuler1MLength;
        public int XPosRuler1MLength {
            get => this._XPosRuler1MLength; set => this._XPosRuler1MLength = value;
        }

        protected int _YPosRuler1MLength;
        public int YPosRuler1MLength {
            get => this._YPosRuler1MLength; set => this._YPosRuler1MLength = value;
        }

        protected int _ActRuler2MLength;
        public int ActRuler2MLength {
            get => this._ActRuler2MLength; set => this._ActRuler2MLength = value;
        }

        protected int _XPosRuler2MLength;
        public int XPosRuler2MLength {
            get => this._XPosRuler2MLength; set => this._XPosRuler2MLength = value;
        }

        protected int _YPosRuler2MLength;
        public int YPosRuler2MLength {
            get => this._YPosRuler2MLength; set => this._YPosRuler2MLength = value;
        }

        protected int _MaxPlateWidth;
        public int MaxPlateWidth {
            get => this._MaxPlateWidth; set => this._MaxPlateWidth = value;
        }

        protected int _MinPlateWidth;
        public int MinPlateWidth {
            get => this._MinPlateWidth; set => this._MinPlateWidth = value;
        }

        protected int _MeanPlateWidth;
        public int MeanPlateWidth {
            get => this._MeanPlateWidth; set => this._MeanPlateWidth = value;
        }

        protected int _MaxDeviationOfCenterLine;
        public int MaxDeviationOfCenterLine {
            get => this._MaxDeviationOfCenterLine; set => this._MaxDeviationOfCenterLine = value;
        }
    }

    public class FlatMeasList : List<FlatMeasData> {
        public FlatMeasList()
           : base() {
        }

        public FlatMeasData GetItem(string plateID) {
            var data = new FlatMeasData();
            foreach (var item in this) {
                if (item.PlateID.Equals(plateID, StringComparison.Ordinal)) {
                    return item;
                }
            }

            return data;
        }
    }
}
