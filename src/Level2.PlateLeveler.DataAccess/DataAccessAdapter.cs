using System;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.SqlClient;
using Level2.PlateLeveler.DataFunction;

namespace Level2.PlateLeveler.DataAccess            // used to write the table in the database through stored procedures
{
    public enum TableDef   //Preliminary
    {
        PDI, Temp, Steelgrades, Cassettes, PreSettingsOutlet, PreSettings, FaultCompensation, ProductionReport, PrimaryData
    }

    public enum PkeyDef    //ToDo!!
    {
        PKeyPDI, PKeyMEAS, PKeyGrindingPrograms, PKeyGrindOrder, MachineNo, PKeyRollResult, PKeyRollResultRoundness, PKeyRollResultShape, PKeyRolls, PKeyRollType, PkeyProfileType
    }

    public enum ProcedureDef    //ToDo!!
    {
        SavePDI, SaveMeasuring, SavePassMeasure, SaveCoefficient, SaveOutletSettings, SaveTemperature, SavePDIData, SaveLevelerData, SaveCassettes, SaveFaultCompensation, SaveProduction, SavePDIState, SavePrimaryData, SaveLineState, SaveActiveCassette
    }

    // Status des PDI Datensatzes
    public enum StateDef {
        ReceivedFromLevel3 = 0, Manual = 1, SendToPLC = 2, InProduction = 3, Finished = 4, CoilDividing = 5, Deleted = 6, All = 7
    }

    public class DataAccessAdapter : IDisposable {
        private readonly DataContext _Context;
        private readonly DbConnection cnn;
        private readonly DbProviderFactory provider;
        private readonly SqlConnection cnnSQL;
        public DataAccessAdapter(string client, string connectionString) {
            this.provider = DbProviderFactories.GetFactory(client);
            this.cnn = this.provider.CreateConnection();
            this.cnnSQL = new SqlConnection(connectionString);
            this.cnn.ConnectionString = connectionString;
            this._Context = new DataContext(connectionString);
        }

        public bool SaveDataToProcedure(ProcedureDef procedure, string[] fields, object[] values, out object returnValue) {
            var cmd = this.provider.CreateCommand();
            cmd.CommandText = procedure.ToString();
            cmd.Connection = this.cnn;
            cmd.CommandType = CommandType.StoredProcedure;

            _ = new object();

            _ = this.provider.CreateParameter();

            try {
                DbParameter param;
                for (var n = 0; n < values.Length; n++) {
                    //Conversion of the NULL-Values for the database. If there is an empty String the object is set to DBNull and if there is a null-Value, it is also transformed to DBNull
                    param = this.provider.CreateParameter();
                    param.ParameterName = fields[n];
                    if (values[n] == null) {
                        values[n] = DBNull.Value;
                    } else if (values[n].GetType().Equals(typeof(string))) {
                        if (string.IsNullOrEmpty(values[n].ToString())) {
                            values[n] = DBNull.Value;
                        }
                    }
                    if (values[n] != DBNull.Value) {
                        param.DbType = Functions.GetDbType(values[n]);
                    }

                    param.Value = values[n];
                    _ = cmd.Parameters.Add(param);
                }
                returnValue = new object();
                param = this.provider.CreateParameter();
                param.Direction = ParameterDirection.ReturnValue;
                _ = cmd.Parameters.Add(param);
                if (this.cnn.State == ConnectionState.Closed) {
                    this.cnn.Open();
                }

                var result = cmd.ExecuteNonQuery();
                returnValue = cmd.Parameters[cmd.Parameters.Count - 1].Value;
                return result != 0;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                returnValue = null;
                return false;
            }
        }

        public bool SaveDataToProcedure(ProcedureDef procedure, string[] fields, object[] values, int[] sizes, ref object returnValue) {
            var cmd = this.provider.CreateCommand();
            cmd.CommandText = procedure.ToString();
            cmd.Connection = this.cnn;
            cmd.CommandType = CommandType.StoredProcedure;

            _ = new object();

            _ = this.provider.CreateParameter();

            try {
                DbParameter param;
                for (var n = 0; n < values.Length; n++) {
                    //Conversion of the NULL-Values for the database. If there is an empty String the object is set to DBNull and if there is a null-Value, it is also transformed to DBNull
                    param = this.provider.CreateParameter();
                    param.ParameterName = fields[n];
                    if (values[n] == null) {
                        values[n] = DBNull.Value;
                    } else if (values[n].GetType().Equals(typeof(string))) {
                        if (string.IsNullOrEmpty(values[n].ToString())) {
                            values[n] = DBNull.Value;
                        } else {
                            param.Size = sizes[n];
                        }
                    }
                    if (values[n] != DBNull.Value) {
                        param.DbType = Functions.GetDbType(values[n]);
                    }

                    param.Value = values[n];
                    _ = cmd.Parameters.Add(param);
                }
                param = this.provider.CreateParameter();
                param.DbType = Functions.GetDbType(returnValue);
                param.Direction = ParameterDirection.ReturnValue;
                _ = cmd.Parameters.Add(param);
                if (this.cnn.State == ConnectionState.Closed) {
                    this.cnn.Open();
                }

                var result = cmd.ExecuteNonQuery();
                returnValue = cmd.Parameters[cmd.Parameters.Count - 1].Value;
                return result != 0;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                returnValue = null;
                return false;
            }
        }

        public bool SaveDataToProcedure(ProcedureDef procedure, string[] fields, object[] values) {
            var cmd = this.provider.CreateCommand();
            cmd.CommandText = procedure.ToString();
            cmd.Connection = this.cnn;
            cmd.CommandType = CommandType.StoredProcedure;

            try {
                for (var n = 0; n < values.Length; n++) {
                    //Conversion of the NULL-Values for the database. If there is an empty String the object is set to DBNull and if there is a null-Value, it is also transformed to DBNull
                    if (values[n] == null) {
                        values[n] = DBNull.Value;
                    } else if (values[n].GetType().Equals(typeof(string))) {
                        values[n] = values[n].ToString().Trim();
                    }

                    if (string.IsNullOrEmpty(values[n].ToString())) {
                        values[n] = DBNull.Value;
                    }

                    var param = this.provider.CreateParameter();
                    param.ParameterName = fields[n];
                    if (values[n] != DBNull.Value) {
                        param.DbType = Functions.GetDbType(values[n]);
                    }

                    param.Value = values[n];
                    _ = cmd.Parameters.Add(param);
                }

                if (this.cnn.State == ConnectionState.Closed) {
                    this.cnn.Open();
                }

                var result = cmd.ExecuteNonQuery();

                return result != 0;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return false;
            }
        }

        public bool SaveDataToProcedure(string procedure, string[] fields, object[] values) {
            var cmd = this.provider.CreateCommand();
            cmd.CommandText = procedure;
            cmd.Connection = this.cnn;
            cmd.CommandType = CommandType.StoredProcedure;

            _ = new object();

            try {
                for (var n = 0; n < values.Length; n++) {
                    //Conversion of the NULL-Values for the database. If there is an empty String the object is set to DBNull and if there is a null-Value, it is also transformed to DBNull
                    if (values[n] == null) {
                        values[n] = DBNull.Value;
                    } else if (values[n].GetType().Equals(typeof(string))) {
                        if (string.IsNullOrEmpty(values[n].ToString())) {
                            values[n] = DBNull.Value;
                        }
                    }

                    var param = this.provider.CreateParameter();
                    param.ParameterName = fields[n];
                    if (values[n] != DBNull.Value) {
                        param.DbType = Functions.GetDbType(values[n]);
                    }

                    param.Value = values[n];
                    _ = cmd.Parameters.Add(param);
                }
                if (this.cnn.State == ConnectionState.Closed) {
                    this.cnn.Open();
                }

                var result = cmd.ExecuteNonQuery();

                return result != 0;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return false;
            }
        }

        public bool UpdateTable(TableDef table, string[] fields, object[] values) {
            var sql = "UPDATE " + table.ToString() + " SET ";
            for (var n = 1; n < fields.Length; n++) {
                sql += fields[n] + " = ?, ";
            }

            sql = sql.Remove(sql.Length - 2);
            sql += " WHERE " + fields[0] + " = ?";
            try {
                var cmd = this.provider.CreateCommand();
                cmd.CommandText = sql;
                cmd.Connection = this.cnn;

                if (this.cnn.State == ConnectionState.Closed) {
                    this.cnn.Open();
                }

                for (var n = 1; n < values.Length; n++) {
                    _ = cmd.Parameters.Add(values[n]);
                }

                _ = cmd.Parameters.Add(values[0]);
                var result = cmd.ExecuteNonQuery();

                return result != 0;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return false;
            }
        }

        public bool UpdateState(int coilID, StateDef state) => this.UpdateData(TableDef.PDI, (int)state, "Status", coilID, "PKEY_PDI");

        public bool UpdateData(TableDef table, object val, string field, object pkey, string fieldPkey) {
            var str = val.ToString().Replace(",", ".");
            var sql = "UPDATE " + table.ToString() + " SET " + field + " = " + str + " WHERE " + fieldPkey + " = " + pkey.ToString();
            var cmd = this.provider.CreateCommand();
            cmd.CommandText = sql;
            cmd.Connection = this.cnn;
            if (this.cnn.State == ConnectionState.Closed) {
                this.cnn.Open();
            }

            var result = cmd.ExecuteNonQuery();

            return result != 0;
        }

        public int DeleteData(TableDef table, object val, string field) {
            var sql = "DELETE FROM " + table.ToString() + " WHERE " + field + " = ";

            sql += val is string ? "'" + val.ToString() + "'" : val.ToString();

            var cmd = this.provider.CreateCommand();
            cmd.CommandText = sql;
            cmd.Connection = this.cnn;
            if (this.cnn.State == ConnectionState.Closed) {
                this.cnn.Open();
            }

            var result = cmd.ExecuteNonQuery();

            return result;
        }

        public int DeleteData(TableDef table, object[] val, string[] field) {
            var sql = "DELETE FROM " + table.ToString() + " WHERE ";
            for (var n = 0; n < val.Length; n++) {
                if (n > 0) {
                    sql += " AND ";
                }

                sql += field[n] + " = ";

                sql += val[n] is string ? "'" + val[n].ToString() + "'" : val[n].ToString();
            }
            var cmd = this.provider.CreateCommand();
            cmd.CommandText = sql;
            cmd.Connection = this.cnn;
            if (this.cnn.State == ConnectionState.Closed) {
                this.cnn.Open();
            }

            var result = cmd.ExecuteNonQuery();

            return result;
        }

        public int DeleteData(TableDef table, object val, PkeyDef pkey) {
            var sql = "DELETE FROM " + table.ToString() + " WHERE " + pkey.ToString() + " = ";

            sql += val is string ? "'" + val.ToString() + "'" : val.ToString();

            var cmd = this.provider.CreateCommand();
            cmd.CommandText = sql;
            cmd.Connection = this.cnn;
            if (this.cnn.State == ConnectionState.Closed) {
                this.cnn.Open();
            }

            var result = cmd.ExecuteNonQuery();

            return result;
        }

        public int DeleteData(string sql) {
            var cmd = this.provider.CreateCommand();
            cmd.CommandText = sql;
            cmd.Connection = this.cnn;
            if (this.cnn.State == ConnectionState.Closed) {
                this.cnn.Open();
            }

            var result = cmd.ExecuteNonQuery();

            return result;
        }

        public void ClearAllPools() => SqlConnection.ClearAllPools();

        public bool CheckConnection(ref string message) {
            var sql = "SELECT GETDATE()";

            var cmd = this.provider.CreateCommand();
            cmd.CommandText = sql;
            cmd.Connection = this.cnn;
            try {
                if (this.cnn.State == ConnectionState.Open) {
                    this.cnn.Close();
                }

                this.cnn.Open();
                _ = cmd.ExecuteNonQuery();
                message = "";
                return true;
            } catch (Exception e) {
                message = e.Message;
                return false;
            }
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
