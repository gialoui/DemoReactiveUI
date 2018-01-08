using DemoAppReactiveUI.Model;
using log4net;
using Npgsql;
using System;
using System.Data;
using System.Threading.Tasks;

namespace DemoAppReactiveUI.Helper
{
    public class DataConnection
    {
        // log
        public static readonly ILog logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static void ArchiveWALBackup()
        {
            try
            {
                var posIDName = "pos" + Properties.Settings.Default.posID;
                var archiveName = posIDName + "_" + DateTime.Now.ToString("MM_dd_yyyy_HH_mm");
                var query = "select pg_start_backup('" + archiveName + "');";
                var result = ExecuteNonQuery(GetCommand(query));

                // stop backup
                query = "select pg_stop_backup();";
                result = ExecuteNonQuery(GetCommand(query));
            }
            catch (Exception ex)
            {
                logger.Error("ArchiveWALBackup ERROR: " + ex.Message);
            }
        }

        public static bool DatabaseExist()
        {
            using (NpgsqlConnection con = GetConnection())
            {
                try
                {
                    string sql = "SELECT * FROM information_schema.tables WHERE table_name = 'version'";
                    using (var cmd = new NpgsqlCommand(sql))
                    {
                        if (cmd.Connection == null)
                            cmd.Connection = con;
                        if (cmd.Connection.State != ConnectionState.Open)
                            cmd.Connection.Open();

                        using (NpgsqlDataReader rdr = cmd.ExecuteReader())
                        {
                            try
                            {
                                if (rdr != null && rdr.HasRows)
                                {
                                    return true;
                                }
                                else
                                {
                                    logger.Error("Database exist BUT tables structure is wrong.");
                                    return false;
                                }
                            }
                            catch (Exception)
                            {
                                logger.Error("Can't connect to database.");
                                return false;
                            }
                            finally
                            {
                                if (con != null) con.Close();
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("Error on DatabaseExist: " + ex.Message);
                    return false;
                }
                finally
                {
                    if (con != null) con.Close();
                }
            }
        }

        public static NpgsqlCommand GetCommand(string sql)
        {
            if (string.IsNullOrEmpty(sql))
                throw new ArgumentNullException("sql");
            return new NpgsqlCommand(sql, null);
        }

        public static int ExecuteNonQuery(NpgsqlCommand command)
        {
            if (command == null)
            {
                logger.Error("ExecuteNonQuery FAILED with command == NULL");
                throw new ArgumentNullException("command");
            }
            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();

                command.Connection = connection;
                command.Prepare();

                int rowsUpdated = command.ExecuteNonQuery();
                connection.Close();
                return rowsUpdated;
            }
        }

        public static async Task<int> ExecuteNonQueryAsync(NpgsqlCommand command)
        {
            if (command == null)
            {
                logger.Error("ExecuteNonQuery FAILED with command == NULL");
                throw new ArgumentNullException("command");
            }
            using (NpgsqlConnection connection = GetConnection())
            {
                connection.Open();

                command.Connection = connection;
                command.Prepare();

                int rowsUpdated = await command.ExecuteNonQueryAsync();
                connection.Close();
                return rowsUpdated;
            }
        }

        public static DataTable GetDataTable(NpgsqlCommand command)
        {
            if (command == null) throw new ArgumentNullException("command");
            using (NpgsqlConnection connection = GetConnection())
            {
                //Console.WriteLine("GetDataTable command == " + command);
                DataTable result = new DataTable();
                connection.Open();

                command.Connection = connection;
                command.Prepare();

                using (NpgsqlDataReader reader = command.ExecuteReader())
                {
                    result.Load(reader);
                }
                connection.Close();

                return result;
            }
        }

        public static DataTable GetDataTable(string sql)
        {
            var command = GetCommand(sql);
            return GetDataTable(command);
        }

        public static NpgsqlConnection GetConnection()
        {
            // PostgeSQL-style connection string
            string connstring = MyDbContext.GetConnectionString();
            //logger.Debug("NpgsqlConnection GetConnection= " + connstring);
            // Making connection with Npgsql provider
            NpgsqlConnection result = new NpgsqlConnection(connstring);
            return result;
        }
    }
}