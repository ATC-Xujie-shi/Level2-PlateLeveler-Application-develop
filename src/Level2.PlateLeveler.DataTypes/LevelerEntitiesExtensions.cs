using System.Data.Entity.Infrastructure;
using System.Data.Objects;

namespace Level2.PlateLeveler.DataTypes {
    public static class LevelerEntitiesExtensions {
        public static int SaveActiveCassette(this LEVELEREntities entities, int? cassetteNo) {
            var cassetteNoParameter = cassetteNo.HasValue ?
                new ObjectParameter("CassetteNo", cassetteNo) :
                new ObjectParameter("CassetteNo", typeof(int));

            return ((IObjectContextAdapter)entities).ObjectContext.ExecuteFunction("SaveActiveCassette", cassetteNoParameter);
        }

        public static int SaveCassettes(this LEVELEREntities entities, int? iD, int? noOfRolls, float? rollDiameter, float? bearingDiameter, float? pitchOfRolls, float? distanceA, float? distanceB, float? distanceC, float? maxMotorPower, float? maxMotorTorque, float? minThicknessPlate, float? maxThicknessPlate, float? maxAdjustment, float? maxCenterHeightBending, float? minCenterHeightBending, float? maxCrossTiltLeft, float? minCrossTiltLeft, float? maxCrossTiltRight, float? minCrossTiltRight, float? maxLevelingPressure) {
            var iDParameter = iD.HasValue ?
                new ObjectParameter("ID", iD) :
                new ObjectParameter("ID", typeof(int));

            var noOfRollsParameter = noOfRolls.HasValue ?
                new ObjectParameter("NoOfRolls", noOfRolls) :
                new ObjectParameter("NoOfRolls", typeof(int));

            var rollDiameterParameter = rollDiameter.HasValue ?
                new ObjectParameter("RollDiameter", rollDiameter) :
                new ObjectParameter("RollDiameter", typeof(float));

            var bearingDiameterParameter = bearingDiameter.HasValue ?
                new ObjectParameter("BearingDiameter", bearingDiameter) :
                new ObjectParameter("BearingDiameter", typeof(float));

            var pitchOfRollsParameter = pitchOfRolls.HasValue ?
                new ObjectParameter("PitchOfRolls", pitchOfRolls) :
                new ObjectParameter("PitchOfRolls", typeof(float));

            var distanceAParameter = distanceA.HasValue ?
                new ObjectParameter("DistanceA", distanceA) :
                new ObjectParameter("DistanceA", typeof(float));

            var distanceBParameter = distanceB.HasValue ?
                new ObjectParameter("DistanceB", distanceB) :
                new ObjectParameter("DistanceB", typeof(float));

            var distanceCParameter = distanceC.HasValue ?
                new ObjectParameter("DistanceC", distanceC) :
                new ObjectParameter("DistanceC", typeof(float));

            var maxMotorPowerParameter = maxMotorPower.HasValue ?
                new ObjectParameter("MaxMotorPower", maxMotorPower) :
                new ObjectParameter("MaxMotorPower", typeof(float));

            var maxMotorTorqueParameter = maxMotorTorque.HasValue ?
                new ObjectParameter("MaxMotorTorque", maxMotorTorque) :
                new ObjectParameter("MaxMotorTorque", typeof(float));

            var minThicknessPlateParameter = minThicknessPlate.HasValue ?
                new ObjectParameter("MinThicknessPlate", minThicknessPlate) :
                new ObjectParameter("MinThicknessPlate", typeof(float));

            var maxThicknessPlateParameter = maxThicknessPlate.HasValue ?
                new ObjectParameter("MaxThicknessPlate", maxThicknessPlate) :
                new ObjectParameter("MaxThicknessPlate", typeof(float));

            var maxAdjustmentParameter = maxAdjustment.HasValue ?
                new ObjectParameter("MaxAdjustment", maxAdjustment) :
                new ObjectParameter("MaxAdjustment", typeof(float));

            var maxCenterHeightBendingParameter = maxCenterHeightBending.HasValue ?
                new ObjectParameter("MaxCenterHeightBending", maxCenterHeightBending) :
                new ObjectParameter("MaxCenterHeightBending", typeof(float));

            var minCenterHeightBendingParameter = minCenterHeightBending.HasValue ?
                new ObjectParameter("MinCenterHeightBending", minCenterHeightBending) :
                new ObjectParameter("MinCenterHeightBending", typeof(float));

            var maxCrossTiltLeftParameter = maxCrossTiltLeft.HasValue ?
                new ObjectParameter("MaxCrossTiltLeft", maxCrossTiltLeft) :
                new ObjectParameter("MaxCrossTiltLeft", typeof(float));

            var minCrossTiltLeftParameter = minCrossTiltLeft.HasValue ?
                new ObjectParameter("MinCrossTiltLeft", minCrossTiltLeft) :
                new ObjectParameter("MinCrossTiltLeft", typeof(float));

            var maxCrossTiltRightParameter = maxCrossTiltRight.HasValue ?
                new ObjectParameter("MaxCrossTiltRight", maxCrossTiltRight) :
                new ObjectParameter("MaxCrossTiltRight", typeof(float));

            var minCrossTiltRightParameter = minCrossTiltRight.HasValue ?
                new ObjectParameter("MinCrossTiltRight", minCrossTiltRight) :
                new ObjectParameter("MinCrossTiltRight", typeof(float));

            var maxLevelingPressureParameter = maxLevelingPressure.HasValue ?
                new ObjectParameter("MaxLevelingPressure", maxLevelingPressure) :
                new ObjectParameter("MaxLevelingPressure", typeof(float));

            return ((IObjectContextAdapter)entities).ObjectContext.ExecuteFunction("SaveCassettes", iDParameter, noOfRollsParameter, rollDiameterParameter, bearingDiameterParameter, pitchOfRollsParameter, distanceAParameter, distanceBParameter, distanceCParameter, maxMotorPowerParameter, maxMotorTorqueParameter, minThicknessPlateParameter, maxThicknessPlateParameter, maxAdjustmentParameter, maxCenterHeightBendingParameter, minCenterHeightBendingParameter, maxCrossTiltLeftParameter, minCrossTiltLeftParameter, maxCrossTiltRightParameter, minCrossTiltRightParameter, maxLevelingPressureParameter);
        }

        public static int SavePDIData(this LEVELEREntities entities, long? iD, string plateID, string materialID, string steelGrade, float? length, float? width, float? thickness, float? yieldPoint, float? tensileStrength, float? eModule, short? measuringCode, float? ruler1MCrossPDI, float? ruler1MLengthPDI, float? ruler2MLengthPDI, short? leveling, float? gapCrossBow, float? gapLengthBow) {
            var iDParameter = iD.HasValue ?
                new ObjectParameter("ID", iD) :
                new ObjectParameter("ID", typeof(long));

            var plateIDParameter = plateID != null ?
                new ObjectParameter("PlateID", plateID) :
                new ObjectParameter("PlateID", typeof(string));

            var materialIDParameter = materialID != null ?
                new ObjectParameter("MaterialID", materialID) :
                new ObjectParameter("MaterialID", typeof(string));

            var steelGradeParameter = steelGrade != null ?
                new ObjectParameter("SteelGrade", steelGrade) :
                new ObjectParameter("SteelGrade", typeof(string));

            var lengthParameter = length.HasValue ?
                new ObjectParameter("Length", length) :
                new ObjectParameter("Length", typeof(float));

            var widthParameter = width.HasValue ?
                new ObjectParameter("Width", width) :
                new ObjectParameter("Width", typeof(float));

            var thicknessParameter = thickness.HasValue ?
                new ObjectParameter("Thickness", thickness) :
                new ObjectParameter("Thickness", typeof(float));

            var yieldPointParameter = yieldPoint.HasValue ?
                new ObjectParameter("YieldPoint", yieldPoint) :
                new ObjectParameter("YieldPoint", typeof(float));

            var tensileStrengthParameter = tensileStrength.HasValue ?
                new ObjectParameter("TensileStrength", tensileStrength) :
                new ObjectParameter("TensileStrength", typeof(float));

            var eModuleParameter = eModule.HasValue ?
                new ObjectParameter("EModule", eModule) :
                new ObjectParameter("EModule", typeof(float));

            var measuringCodeParameter = measuringCode.HasValue ?
                new ObjectParameter("MeasuringCode", measuringCode) :
                new ObjectParameter("MeasuringCode", typeof(short));

            var ruler1MCrossPDIParameter = ruler1MCrossPDI.HasValue ?
                new ObjectParameter("Ruler1MCrossPDI", ruler1MCrossPDI) :
                new ObjectParameter("Ruler1MCrossPDI", typeof(float));

            var ruler1MLengthPDIParameter = ruler1MLengthPDI.HasValue ?
                new ObjectParameter("Ruler1MLengthPDI", ruler1MLengthPDI) :
                new ObjectParameter("Ruler1MLengthPDI", typeof(float));

            var ruler2MLengthPDIParameter = ruler2MLengthPDI.HasValue ?
                new ObjectParameter("Ruler2MLengthPDI", ruler2MLengthPDI) :
                new ObjectParameter("Ruler2MLengthPDI", typeof(float));

            var levelingParameter = leveling.HasValue ?
                new ObjectParameter("Leveling", leveling) :
                new ObjectParameter("Leveling", typeof(short));

            var gapCrossBowParameter = gapCrossBow.HasValue ?
                new ObjectParameter("GapCrossBow", gapCrossBow) :
                new ObjectParameter("GapCrossBow", typeof(float));

            var gapLengthBowParameter = gapLengthBow.HasValue ?
                new ObjectParameter("GapLengthBow", gapLengthBow) :
                new ObjectParameter("GapLengthBow", typeof(float));

            return ((IObjectContextAdapter)entities).ObjectContext.ExecuteFunction("SavePDIData", iDParameter, plateIDParameter, materialIDParameter, steelGradeParameter, lengthParameter, widthParameter, thicknessParameter, yieldPointParameter, tensileStrengthParameter, eModuleParameter, measuringCodeParameter, ruler1MCrossPDIParameter, ruler1MLengthPDIParameter, ruler2MLengthPDIParameter, levelingParameter, gapCrossBowParameter, gapLengthBowParameter);
        }
    }
}
