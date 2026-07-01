using System;
using System.Data;
using System.Data.Common;
using System.Reflection;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.DataProvider {
    public enum CassetteDef {
        CassetteNo, NoOfRolls, RollDiameter, BearingDiameter, PitchOfRolls, DistanceA, DistanceB, DistanceC, MaxMotorPower, MaxMotorTorque, MinThicknessPlate, MaxThicknessPlate,
        MaxAdjustment, MaxCenterHeightBending, MinCenterHeightBending, MaxCrossTiltLeft, MinCrossTiltLeft, MaxCrossTiltRight, MinCrossTiltRight, MaxLevellingPressure
    }

    public enum SettingsDef {
        ID, MaterialID, CassetteNo, Enable, StartRangeThickness, EndRangeThickness, StartRangeWidth, EndRangeWidth, Plastification, MiddleHeight, CurveProfile, TiltingLeft, TiltingRight
    }

    public enum OutletSettingsDef {
        ID, StartRangeThickness, EndRangeThickness, StartRangeYieldPoint, EndRangeYieldPoint, Offset
    }

    public enum FaultCompensationDef {
        ID, EdgeWave, CenterWave, Ski, Tilting, CassetteNo, MaxHeightEdgeWave, EvaluationEdgeWave, MaxTilting, EvaluationTilting, MaxHeightCenterWave, EvaluationCenterWave, MaxSki, EvaluationSki
    }

    public class PreSettingsAdapter {
        private readonly DbConnection cnn;
        private readonly DbProviderFactory provider;
        private readonly ConnectionData _Connection;
        private readonly InitData _Initialization;
        public PreSettingsAdapter(InitData init) {
            this.provider = DbProviderFactories.GetFactory(init.Connections[0].Namespace);
            this.cnn = this.provider.CreateConnection();
            this.cnn.ConnectionString = init.Connections[0].ConnectionString;
            this._Connection = init.Connections[0];
            this._Initialization = init;
        }

        public CassetteList LoadData() {
            var result = new CassetteList();
            _ = new CassetteData();
            var sql = "SELECT CASSETTENO, NoOfRolls, RollDiameter, BearingDiameter, PitchOfRolls, DistanceA, DistanceB, DistanceC, MaxMotorPower, MaxMotorTorque, MinThicknessPlate, MaxThicknessPlate, " +
                                 "MaxAdjustment, MaxCenterHeightBending, MinCenterHeightBending, MaxCrossTiltLeft, MinCrossTiltLeft, MaxCrossTiltRight, MinCrossTiltRight, MaxLevelingPressure FROM Cassettes";
            var cmd = this.provider.CreateCommand();
            cmd.CommandText = sql;
            cmd.Connection = this.cnn;

            try {
                if (this.cnn.State == ConnectionState.Closed) {
                    this.cnn.Open();
                }

                var reader = cmd.ExecuteReader();

                while (reader.Read()) {
                    var data = new CassetteData {
                        CassetteNo = Convert.ToInt32(reader[(int)CassetteDef.CassetteNo]),
                        NoOfRolls = reader.IsDBNull((int)CassetteDef.NoOfRolls) ? 0 : Convert.ToInt32(reader[(int)CassetteDef.NoOfRolls]),
                        PitchOfRolls = reader.IsDBNull((int)CassetteDef.PitchOfRolls) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.PitchOfRolls]),
                        BearingDiameter = reader.IsDBNull((int)CassetteDef.BearingDiameter) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.BearingDiameter]),
                        DistanceA = reader.IsDBNull((int)CassetteDef.DistanceA) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.DistanceA]),
                        DistanceB = reader.IsDBNull((int)CassetteDef.DistanceB) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.DistanceB])
                    };
                    data.DistanceB = reader.IsDBNull((int)CassetteDef.DistanceC) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.DistanceC]);
                    data.MaxAdjustment = reader.IsDBNull((int)CassetteDef.MaxAdjustment) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.MaxAdjustment]);
                    data.MaxCenterHeightBending = reader.IsDBNull((int)CassetteDef.MaxCenterHeightBending) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.MaxCenterHeightBending]);
                    data.MaxCrossTiltLeft = reader.IsDBNull((int)CassetteDef.MinCrossTiltLeft) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.MinCrossTiltLeft]);
                    data.MaxCrossTiltRight = reader.IsDBNull((int)CassetteDef.MinCrossTiltRight) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.MinCrossTiltRight]);
                    data.MaxLevelingPressure = reader.IsDBNull((int)CassetteDef.MaxLevellingPressure) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.MaxLevellingPressure]);
                    data.MaxMotorPower = reader.IsDBNull((int)CassetteDef.MaxMotorTorque) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.MaxMotorPower]);
                    data.MaxMotorTorque = reader.IsDBNull((int)CassetteDef.MaxMotorTorque) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.MaxMotorTorque]);
                    data.MaxThicknessPlate = reader.IsDBNull((int)CassetteDef.MaxThicknessPlate) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.MaxThicknessPlate]);
                    data.MinCenterHeightBending = reader.IsDBNull((int)CassetteDef.MinCenterHeightBending) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.MinCenterHeightBending]);
                    data.MinCrossTiltLeft = reader.IsDBNull((int)CassetteDef.MinCrossTiltLeft) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.MinCrossTiltLeft]);
                    data.MinCrossTiltRight = reader.IsDBNull((int)CassetteDef.MinCrossTiltRight) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.MinCrossTiltRight]);
                    data.MinThicknessPlate = reader.IsDBNull((int)CassetteDef.MinThicknessPlate) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.MinThicknessPlate]);
                    data.RollDiameter = reader.IsDBNull((int)CassetteDef.RollDiameter) ? 0 : Convert.ToSingle(reader[(int)CassetteDef.RollDiameter]);
                    result.Add(data);
                }
                reader.Close();
                return result;
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return null;
            }
        }

        public SettingsData LoadPreSettings(AdjustmentData adjustment) {
            try {
                var result = new SettingsData(adjustment);

                var sql = "SELECT * FROM PreSettings WHERE MaterialID = '" + adjustment.MaterialID + "' AND CassetteNo = " + adjustment.CassetteNo + " AND ENABLE = 1 " +
                               "AND STARTRANGETHICKNESS <= " + adjustment.Thickness.ToString() + " AND ENDRANGETHICKNESS >= " + adjustment.Thickness.ToString() +
                               " AND STARTRANGEWIDTH <= " + adjustment.Width.ToString() + " AND ENDRANGEWIDTH >= " + adjustment.Width.ToString();
                var cmd = this.provider.CreateCommand();
                cmd.CommandText = sql;
                cmd.Connection = this.cnn;
                if (this.cnn.State == ConnectionState.Closed) {
                    this.cnn.Open();
                }

                var reader = cmd.ExecuteReader();

                while (reader.Read()) {
                    result.Pkey_PDI = Convert.ToInt64(reader[(int)SettingsDef.ID]);
                    result.CassetteNo = Convert.ToInt32(reader[(int)SettingsDef.CassetteNo]);
                    result.MaterialID = reader[(int)SettingsDef.MaterialID].ToString().Trim();
                    result.CurveProfile = reader.IsDBNull((int)SettingsDef.CurveProfile) ? 0 : Convert.ToSingle(reader[(int)SettingsDef.CurveProfile]);
                    result.Enable = !reader.IsDBNull((int)SettingsDef.Enable) && Convert.ToBoolean(Convert.ToInt32(reader[(int)SettingsDef.Enable]));
                    result.EndRangeThickness = reader.IsDBNull((int)SettingsDef.EndRangeThickness) ? 0 : Convert.ToSingle(reader[(int)SettingsDef.EndRangeThickness]);
                    result.EndRangeWidth = reader.IsDBNull((int)SettingsDef.EndRangeWidth) ? 0 : Convert.ToSingle(reader[(int)SettingsDef.EndRangeWidth]);
                    result.MiddleHeight = reader.IsDBNull((int)SettingsDef.MiddleHeight) ? 0 : Convert.ToSingle(reader[(int)SettingsDef.MiddleHeight]);
                    result.Plastification = reader.IsDBNull((int)SettingsDef.Plastification) ? result.Plastification : Convert.ToSingle(reader[(int)SettingsDef.Plastification]);
                    result.StartRangeThickness = reader.IsDBNull((int)SettingsDef.StartRangeThickness) ? 0 : Convert.ToSingle(reader[(int)SettingsDef.StartRangeThickness]);
                    result.StartRangeWidth = reader.IsDBNull((int)SettingsDef.StartRangeWidth) ? 0 : Convert.ToSingle(reader[(int)SettingsDef.StartRangeWidth]);
                    result.TiltLeft = reader.IsDBNull((int)SettingsDef.TiltingLeft) ? 0 : Convert.ToSingle(reader[(int)SettingsDef.TiltingLeft]);
                    result.TiltRight = reader.IsDBNull((int)SettingsDef.TiltingRight) ? 0 : Convert.ToSingle(reader[(int)SettingsDef.TiltingRight]);
                }
                reader.Close();
                return result;
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return null;
            }
        }

        public OutletSettingsList LoadOutletData() {
            var result = new OutletSettingsList();
            _ = new OutletSettingsData();
            var sql = "SELECT * FROM PreSettingsOutlet";
            try {
                var cmd = this.provider.CreateCommand();
                cmd.CommandText = sql;
                cmd.Connection = this.cnn;
                if (this.cnn.State == ConnectionState.Closed) {
                    this.cnn.Open();
                }

                var reader = cmd.ExecuteReader();

                while (reader.Read()) {
                    var data = new OutletSettingsData {
                        ID = Convert.ToInt64(reader[(int)OutletSettingsDef.ID]),
                        EndRangeThickness = reader.IsDBNull((int)OutletSettingsDef.EndRangeThickness) ? 0 : Convert.ToSingle(reader[(int)OutletSettingsDef.EndRangeThickness]),
                        EndRangeYieldPoint = reader.IsDBNull((int)OutletSettingsDef.EndRangeYieldPoint) ? 0 : Convert.ToSingle(reader[(int)OutletSettingsDef.EndRangeYieldPoint]),
                        Offset = reader.IsDBNull((int)OutletSettingsDef.Offset) ? 0 : Convert.ToSingle(reader[(int)OutletSettingsDef.Offset]),
                        StartRangeThickness = reader.IsDBNull((int)OutletSettingsDef.StartRangeThickness) ? 0 : Convert.ToSingle(reader[(int)OutletSettingsDef.StartRangeThickness]),
                        StartRangeYieldPoint = reader.IsDBNull((int)OutletSettingsDef.StartRangeYieldPoint) ? 0 : Convert.ToSingle(reader[(int)OutletSettingsDef.StartRangeYieldPoint])
                    };
                    result.Add(data);
                }
                reader.Close();
                return result;
            } catch (Exception e) {
                Logging.SendErrorMessage(MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return result;
            }
        }

        public PriorityList LoadPriorities(FaultCompensation fc) {
            var result = new PriorityList();
            _ = new PrioritiesAttribute();
            var type = fc.GetType();
            var propertyInfos = type.GetProperties();
            foreach (var propertyInfo in propertyInfos) {
                var attr = (PrioritiesAttribute)Attribute.GetCustomAttribute(propertyInfo, typeof(PrioritiesAttribute));
                if (attr != null) {
                    var obj = propertyInfo.GetValue(fc, null);
                    if (obj != null) {
                        attr.PriorityID = int.Parse(obj.ToString());
                    }

                    result.Add(attr);
                }
            }
            return result;
        }

        public bool IsDBConnection() {
            var sql = "SELECT GETDATE()";
            try {
                var cmd = this.provider.CreateCommand();
                cmd.CommandText = sql;
                cmd.Connection = this.cnn;
                if (this.cnn.State != ConnectionState.Open) {
                    this.cnn.Open();
                }

                var n = cmd.ExecuteNonQuery();
                return n != 0;
            } catch (DbException) {
                return false;
            } catch {
                return true;
            }
        }
    }
}
