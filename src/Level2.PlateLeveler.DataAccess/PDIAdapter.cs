using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Level2.PlateLeveler.DataFunction;
using Level2.PlateLeveler.DataTypes;

namespace Level2.PlateLeveler.DataAccess {
    public class PDIAdapter {
        private readonly bool _Available;
        private readonly DbConnection cnn;
        private readonly DbProviderFactory provider;
        private readonly string connectionString;

        /// <summary>
        /// InitData, used for Order and Connections
        /// </summary>
        private readonly InitData _Initialization;

        /// <summary>
        /// Adapter for CoilPDIData from cutsomers L3
        /// </summary>
        /// <param name="con">ConnectionData with PDI connection. If null tries to use PDI-Connection initialization</param>
        /// <param name="initialization">InitData for Order and Connections</param>
        public PDIAdapter(ConnectionData con = null, InitData initialization = null) {
            this._Initialization = initialization;

            if (con == null && this._Initialization != null && this._Initialization.Connections.HasItem(ConnectionDef.PDI.ToString())) {
                con = this._Initialization.Connections.GetItem(ConnectionDef.PDI.ToString());
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
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return false;
            }
        }

        #region C26841216
        public enum L3L2HeatTreatment841216Def {
            PKeyPDI,
            DateTimeInsert,
            DateTimeUpdate,
            PlateID,
            MaterialID,
            SteelGrade,
            PlateLength,
            PlateWidth,
            PlateThickness,
            TensileStrength,
            YieldPoint,
            Ruler1MCross,
            Ruler1MLength,
            Ruler2MLength,
            EModule,
            Leveling,
            Location,
            State
        }

        /// <summary>
        /// Read PDIData from L3 interface DB
        /// </summary>
        /// <returns>A newly received PlateData</returns>
        public List<PDI> LoadPlateData841216() {
            var PDIDataList = new List<PDI>();
            PDI PDIDataNew;

            var counter = 1;
            var sql = "SELECT * FROM L3L2_HeatTreatment_PDI WHERE State = 10";
            var cmd = this.provider.CreateCommand();
            cmd.CommandText = sql;
            cmd.Connection = this.cnn;

            try {
                if (this.cnn.State == ConnectionState.Closed) {
                    this.cnn.Open();
                }

                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        PDIDataNew = new PDI {
                            PKEY_PDI = reader.IsDBNull((int)L3L2HeatTreatment841216Def.PKeyPDI) ? 0 : Convert.ToInt64(reader[(int)L3L2HeatTreatment841216Def.PKeyPDI]),
                            // DateTimeInsert
                            // DateTimeUpdate
                            PlateID = reader[(int)L3L2HeatTreatment841216Def.PlateID].ToString().Trim(),
                            MaterialID = reader[(int)L3L2HeatTreatment841216Def.MaterialID].ToString().Trim(),
                            SteelGrade = reader[(int)L3L2HeatTreatment841216Def.SteelGrade].ToString().Trim(),
                            Length = reader.IsDBNull((int)L3L2HeatTreatment841216Def.PlateLength) ? 0 : Convert.ToSingle(reader[(int)L3L2HeatTreatment841216Def.PlateLength]),
                            Width = reader.IsDBNull((int)L3L2HeatTreatment841216Def.PlateWidth) ? 0 : Convert.ToSingle(reader[(int)L3L2HeatTreatment841216Def.PlateWidth]),
                            Thickness = reader.IsDBNull((int)L3L2HeatTreatment841216Def.PlateThickness) ? 0 : Convert.ToSingle(reader[(int)L3L2HeatTreatment841216Def.PlateThickness]),
                            TensileStrength = reader.IsDBNull((int)L3L2HeatTreatment841216Def.TensileStrength) ? 0 : Convert.ToSingle(reader[(int)L3L2HeatTreatment841216Def.TensileStrength]),
                            YieldPoint = reader.IsDBNull((int)L3L2HeatTreatment841216Def.YieldPoint) ? 0 : Convert.ToSingle(reader[(int)L3L2HeatTreatment841216Def.YieldPoint]),
                            Ruler1MCrossPDI = reader.IsDBNull((int)L3L2HeatTreatment841216Def.Ruler1MCross) ? 0 : Convert.ToSingle(reader[(int)L3L2HeatTreatment841216Def.Ruler1MCross]),
                            Ruler1MLengthPDI = reader.IsDBNull((int)L3L2HeatTreatment841216Def.Ruler1MLength) ? 0 : Convert.ToSingle(reader[(int)L3L2HeatTreatment841216Def.Ruler1MLength]),
                            Ruler2MLengthPDI = reader.IsDBNull((int)L3L2HeatTreatment841216Def.Ruler2MLength) ? 0 : Convert.ToSingle(reader[(int)L3L2HeatTreatment841216Def.Ruler2MLength]),
                            EModule = reader.IsDBNull((int)L3L2HeatTreatment841216Def.EModule) ? 0 : Convert.ToSingle(reader[(int)L3L2HeatTreatment841216Def.EModule]),
                            Leveling = reader.IsDBNull((int)L3L2HeatTreatment841216Def.Leveling) ? (short)0 : Convert.ToInt16(reader[(int)L3L2HeatTreatment841216Def.Leveling]),
                            Location = reader.IsDBNull((int)L3L2HeatTreatment841216Def.Location) ? (short)0 : Convert.ToInt16(reader[(int)L3L2HeatTreatment841216Def.Location]),
                            State = reader.IsDBNull((int)L3L2HeatTreatment841216Def.State) ? (short)0 : Convert.ToInt16(reader[(int)L3L2HeatTreatment841216Def.State]),
                        };

                        counter++;
                        PDIDataList.Add(PDIDataNew);
                    }
                    this.cnn.Close();
                }
                return PDIDataList;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return null;
            }
        }

        public bool MarkPlateDataInL3AsProcessed841216(List<PDI> plateDataList) {
            try {
                foreach (var plate in plateDataList) {
                    var timestamp = Functions.GetDBStringDateTimeFromDateTime(DateTime.Now);
                    var sql = "UPDATE L3L2_HeatTreatment_PDI SET [" + nameof(L3L2HeatTreatment841216Def.State) + "] = '1', [" + nameof(L3L2HeatTreatment841216Def.DateTimeUpdate) + "] = '" + timestamp + "' WHERE [" + nameof(L3L2HeatTreatment841216Def.PKeyPDI) + "] = '" + plate.PKEY_PDI + "' AND [" + nameof(L3L2HeatTreatment841216Def.PlateID) + "] = '" + plate.PlateID + "' AND [" + nameof(L3L2HeatTreatment841216Def.State) + "] = " + plate.State;

                    var cmd = this.provider.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.Connection = this.cnn;

                    if (this.cnn.State == ConnectionState.Closed) {
                        this.cnn.Open();
                    }

                    _ = cmd.ExecuteNonQuery();
                }

                return true;
            } catch (Exception e) {
                Logging.SendErrorMessage(System.Reflection.MethodBase.GetCurrentMethod().Name, e, this.GetType());
                return false;
            }
        }
        #endregion C26841216
    }
}
