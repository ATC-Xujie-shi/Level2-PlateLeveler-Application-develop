using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.DataAccess {
    public class PDOAdapter {
        //DB Connection Properties
        private readonly bool _Available;
        private readonly DbConnection cnn;
        private readonly DbProviderFactory provider;
        private readonly string connectionString;

        private readonly InitData _Initialization;

        public PDOAdapter(ConnectionData con = null, InitData initialization = null) {
            this._Initialization = initialization;

            if (con == null && this._Initialization != null && this._Initialization.Connections.HasItem(ConnectionDef.PDO.ToString())) {
                con = this._Initialization.Connections.GetItem(ConnectionDef.PDO.ToString());
            }

            if (con != null && con.Namespace == "System.Data.SqlClient") {
                this.provider = DbProviderFactories.GetFactory(con.Namespace);
                this.cnn = this.provider.CreateConnection();
                //Hint: After cnn has been used to connect to DB, cnn.ConnectionString does not contain any security information any more because it gets set by what the sql server returns.
                //The .NET Framework Data Provider for SQL Server does not persist or return the password in a connection string unless you set Persist Security Info to true.
                this.cnn.ConnectionString = con.ConnectionString;

                this.connectionString = con.ConnectionString;
            }
        }

        /// <summary>
        /// Is the connection for this adapter available (not necessarily connected)?
        /// For DBs tries to connect, else uses _IsAvailable
        /// </summary>
        /// <returns>True if available, false if not</returns>
        public bool IsAvailable() => this.cnn != null ? this.CheckDB() : this._Available;

        /// <summary>
        /// Checks a connection to DB server can be established and the DB is available
        /// </summary>
        /// <returns>True if connection to DB could be established</returns>
        public bool CheckDB() {
            var data = false;

            try {
                using (var con = this.provider.CreateConnection()) {
                    con.ConnectionString = this.connectionString;
                    var sql = @"IF DB_ID('" + con.Database + "') IS NOT NULL SELECT 1 ELSE SELECT 0";
                    using var cmd = this.provider.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = con;

                    if (cmd.Connection.State == ConnectionState.Closed) {
                        cmd.Connection.Open();
                    }

                    using var reader = cmd.ExecuteReader();
                    while (reader.Read()) {
                        data = Convert.ToBoolean(reader[0]);   //SQL command will return 1 or 0
                    }
                }
                return data;
            } catch (Exception) {
                //Logging.SendErrorMessage(System.Reflection.MethodInfo.GetCurrentMethod().Name, e, this.GetType());
                return false;
            }
        }

        #region C26841216
        #region Plate tracking
        /// <summary>
        /// Interface DB description
        /// </summary>
        public enum L2L3HeatTreatmentTracking841216Def {
            PKeyTracking,
            DateTimeInsert,
            DateTimeUpdate,
            PlateID,
            Location
        }

        /// <summary>
        /// Save the new tracking information to the interface DB
        /// </summary>
        /// <param name="tracking">Containing the plate ID and the location</param>
        /// <returns>-1 if an error occured, or the number of affected rows in the DB</returns>
        public int SaveTracking841216(TrackingData tracking) {
            try {
                if (tracking == null) {
                    return -1;
                }

                if (this.cnn != null) {
                    var nameDB = "L2L3_HeatTreatment_Tracking";

                    var databaseValues = new Dictionary<string, object> {
                        { L2L3HeatTreatmentTracking841216Def.DateTimeInsert.ToString(), Functions.GetDBStringDateTimeFromDateTime(DateTime.Now) },
                        { L2L3HeatTreatmentTracking841216Def.PlateID.ToString(), tracking.PlateID },
                        { L2L3HeatTreatmentTracking841216Def.Location.ToString(), tracking.Location }
                    };

                    var sql = CreateSQLCommandInsertText(nameDB, databaseValues);
                    if (string.IsNullOrWhiteSpace(sql)) {
                        Logging.SendMessage("Error: to create the SQL string to insert data into the L3 DB!", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, typeof(PDOAdapter));
                        return -1;
                    }

                    var cmd = this.provider.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = this.cnn;
                    if (this.cnn.State == ConnectionState.Closed) {
                        this.cnn.Open();
                    }

                    var result = cmd.ExecuteNonQuery();

                    if (result > 0) {
                        Logging.SendMessage("Info: Send tracking information to L3, SQL-String: " + sql, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, typeof(PDOAdapter));
                    } else {
                        Logging.SendMessage("Error: Failed sending tracking information to L3, SQL-String: " + sql, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, typeof(PDOAdapter));
                    }

                    return result;
                } else {
                    Logging.SendMessage("Error: failed because Connection to DB is not initialized!", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, typeof(PDOAdapter));
                    return -1;
                }
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return -1;
            }
        }
        #endregion Plate tracking

        #region Plate report
        /// <summary>
        /// Interface DB description
        /// </summary>
        public enum L2L3HeatTreatmentPlateReport841216Def {
            PKeyProd,
            DateTimeInsert,
            DateTimeUpdate,
            DateTimeProd,
            EModule,
            CassetteNB,
            PlateID,
            MaterialID,
            Leveling,
            NoOfLeveling,
            CassetteNo,
            SetLevelerInlet,
            SetLevelerOutlet,
            SetTiltLeft,
            SetTiltRight,
            SetMiddleHeight,
            SetCurveProfile,
            ActLevelerInlet,
            ActLevelerOutlet,
            ActTiltLeft,
            ActTiltRight,
            ActMiddleHeight,
            ActCurveProfile,
            TensileStrength,
            Temperature,
            ActTemperature,
            SetForceInlet,
            SetForceOutlet,
            ActForceInlet,
            ActForceOutlet,
            State,
        }

        /// <summary>
        /// Sending a plate report to the L3 interface DB
        /// </summary>
        /// <param name="pdi">Plate information</param>
        /// <param name="productionReport">Production report information</param>
        /// <returns>-1 if an error occured or the number of affected rows</returns>
        public int SavePlateReport841216(PDI pdi, ProductionReport productionReport) {
            try {
                if (pdi == null || productionReport == null) {
                    return -1;
                }

                if (this.cnn != null) {
                    var nameDB = "L2L3_HeatTreatment_PlateReport";

                    var databaseValues = new Dictionary<string, object> {
                        //databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.PKey_Prod.ToString(), plateData.Pkey_PDI); // Pkey should not be set in the program! Only by DB
                        { L2L3HeatTreatmentPlateReport841216Def.DateTimeInsert.ToString(), Functions.GetDBStringDateTimeFromDateTime(DateTime.Now) }
                    };
                    if (productionReport.ProdDate != null) {
                        // Use the DateTimeUpdate for the start time of the plate production. Just in case they need it
                        databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.DateTimeUpdate.ToString(), productionReport.ProdDate);
                    }

                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.DateTimeProd.ToString(), Functions.GetDBStringDateTimeFromDateTime(DateTime.Now));
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.EModule.ToString(), pdi.EModule);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.CassetteNB.ToString(), productionReport.CassetteNo);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.PlateID.ToString(), pdi.PlateID);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.MaterialID.ToString(), pdi.MaterialID);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.Leveling.ToString(), pdi.Leveling);
                    //databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.State.ToString(), plateData.Pkey_PDI); // this is somehow twice in the interface ...
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.NoOfLeveling.ToString(), productionReport.NoOfLeveling);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.CassetteNo.ToString(), productionReport.CassetteNo);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.SetLevelerInlet.ToString(), productionReport.SetLevelerInlet);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.SetLevelerOutlet.ToString(), productionReport.SetLevelerOutlet);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.SetTiltLeft.ToString(), productionReport.SetTiltLeft);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.SetTiltRight.ToString(), productionReport.SetTiltRight);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.SetMiddleHeight.ToString(), productionReport.SetMiddleHeight);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.SetCurveProfile.ToString(), productionReport.SetCurveProfile);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.ActLevelerInlet.ToString(), productionReport.ActLevelerInlet);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.ActLevelerOutlet.ToString(), productionReport.ActLevelerOutlet);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.ActTiltLeft.ToString(), productionReport.ActTiltLeft);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.ActTiltRight.ToString(), productionReport.ActTiltRight);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.ActMiddleHeight.ToString(), productionReport.ActMiddleHeight);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.ActCurveProfile.ToString(), productionReport.ActCurveProfile);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.TensileStrength.ToString(), pdi.TensileStrength);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.Temperature.ToString(), productionReport.Temperature);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.ActTemperature.ToString(), productionReport.ActTemperature);
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.SetForceInlet.ToString(), 0); // value does not exist in our data model
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.SetForceOutlet.ToString(), 0); // value does not exist in our data model
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.ActForceInlet.ToString(), 0); // value does not exist in our data model
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.ActForceOutlet.ToString(), 0); // value does not exist in our data model
                    databaseValues.Add(L2L3HeatTreatmentPlateReport841216Def.State.ToString(), 10);

                    var sql = CreateSQLCommandInsertText(nameDB, databaseValues);
                    if (string.IsNullOrWhiteSpace(sql)) {
                        Logging.SendMessage("Error: to create the SQL string to insert data into the L3 DB!", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, typeof(PDOAdapter));
                        return -1;
                    }

                    var cmd = this.provider.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = this.cnn;
                    if (this.cnn.State == ConnectionState.Closed) {
                        this.cnn.Open();
                    }

                    var result = cmd.ExecuteNonQuery();

                    if (result > 0) {
                        Logging.SendMessage("Info: Send Plate Report to L3 interface DB for PlateID " + pdi.PlateID + ", PDI.PKey_PDI: " + pdi.PKEY_PDI + ", SQL-String: " + sql, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Info, typeof(PDOAdapter));
                    } else {
                        Logging.SendMessage("Error: Failed sending Plate Report to L3 interface DB for PlateID " + pdi.PlateID + ", PDI.PKey_PDI: " + pdi.PKEY_PDI + ", SQL-String: " + sql, System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, typeof(PDOAdapter));
                    }

                    return result;
                } else {
                    Logging.SendMessage("Error: failed because Connection to DB is not initialized!", System.Reflection.MethodBase.GetCurrentMethod().Name, LoggerLevel.Error, typeof(PDOAdapter));
                    return -1;
                }
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return -1;
            }
        }
        #endregion Plate report
        #endregion C26841216

        #region Functions
        /// <summary>
        /// Create a command insert text for all given values in the value list.
        /// The created string can look like this: <![CDATA[INSERT <nameDB> (<name1>, <name2>, ...) VALUES (<value1>, <value2>, ...)]]>
        /// </summary>
        /// <param name="nameDB">Name of the accessed database</param>
        /// <param name="valueList">List of key (column name) and value elements</param>
        /// <returns>The created sql command</returns>
        public static string CreateSQLCommandInsertText(string nameDB, Dictionary<string, object> valueList) {
            try {
                if (string.IsNullOrWhiteSpace(nameDB) || valueList == null) {
                    return null;
                }

                var stringBuilder = new StringBuilder();
                var parameterList = new List<object>();
                _ = stringBuilder.Append("INSERT ");
                _ = stringBuilder.Append(nameDB);
                _ = stringBuilder.Append(" (");

                KeyValuePair<string, object> keyValuePair;
                for (var i = 0; i < valueList.Count; i++) {
                    keyValuePair = valueList.ElementAt(i);
                    if (keyValuePair.Key != null && keyValuePair.Value != null) {
                        if (parameterList.Count > 0) {
                            _ = stringBuilder.Append(", ");
                        }

                        _ = stringBuilder.Append(keyValuePair.Key);
                        parameterList.Add(keyValuePair.Value);
                    }
                }

                _ = stringBuilder.Append(") VALUES (");
                for (var i = 0; i < parameterList.Count; i++) {
                    if (parameterList[i] != null && Functions.IsNumericType(parameterList[i])) {
                        _ = stringBuilder.Append(Convert.ToString(parameterList[i], CultureInfo.InvariantCulture));
                    } else {
                        _ = stringBuilder.Append('\'');
                        _ = stringBuilder.Append(parameterList[i]);
                        _ = stringBuilder.Append('\'');
                    }

                    if (i < parameterList.Count - 1) {
                        _ = stringBuilder.Append(", ");
                    }
                }
                _ = stringBuilder.Append(')');

                var sqlString = string.Format(stringBuilder.ToString(), parameterList);
                return sqlString;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, typeof(PDOAdapter));
                return null;
            }
        }
        #endregion Functions
    }
}
