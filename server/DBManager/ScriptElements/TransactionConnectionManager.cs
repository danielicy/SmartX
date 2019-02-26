using Logger;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;

namespace DBTools.ScriptElements
{
    public class TransactionConnectionManager : IDisposable
    {
        #region Logger
        private static ILogger _logger;

        private void Log(string msg,Category cat)
        {

        }
        #endregion

        static MySqlTransaction s_transaction;
        static MySqlConnection s_connection;

        static bool s_rollback;

        bool m_originatingManager;
        bool m_commit;
        bool m_disposed;

        public TransactionConnectionManager()
        {
            _logger.Log( "starting TransactionConnectionManager", Category.Info);
            m_disposed = false;
            m_commit = false;
            if (s_connection == null)
            {
                m_originatingManager = true;
                s_rollback = false;
                s_connection = DBManager.GetNewConnection();
                s_transaction = s_connection.BeginTransaction();
            }
            _logger.Log( "TransactionConnectionManager started succesfully", Category.Info);
        }

        public MySqlConnection Connection
        {
            get
            {
                return s_connection;
            }
        }

        public int ExecuteScalar(string strSQL, params MySqlParameter[] sqlParams)
        {
            IDbCommand dbCmd = null;
            try
            {


                _logger.Log(ClassTitle, "Running SQL command", Category.Info);


                var regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                string[] lines = regex.Split(strSQL);

                dbCmd = new MySqlCommand(strSQL, s_connection, s_transaction);

                foreach (string line in lines.Where(l => l.Any()))
                {
                    dbCmd.CommandText = line;
                    var parameters = "No Parameters";
                    if (sqlParams.Any())
                    {
                        parameters = string.Empty;
                        sqlParams.ToList().ForEach(sp => dbCmd.Parameters.Add(sp));
                        sqlParams.ToList().ForEach(sp => parameters += sp + "\n");
                    }
                    try
                    {
                        var resultQuary = dbCmd.ExecuteScalar();
                        if (resultQuary != null)
                        {
                            return Convert.ToInt32(resultQuary.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(ClassTitle, $"Failed excute SqlScript: \n SqlScript:" +
                                               $" {line} \n Parameter: {parameters} \n Exception: {ex.Message}", Category.Exception);
                    }
                }
                return -1;
            }
            catch (Exception ex)
            {
                _logger.Log(ClassTitle, $"Failed run SqlScript with Exception {ex.Message}", Category.Exception);
                return -1;
            }
            finally
            {
                if (dbCmd != null)
                    dbCmd.Dispose();
            }
        }

        public void ExecuteMySQLCmd(string strSQL, int commandTimeout, params MySqlParameter[] sqlParams)
        {
            MySqlCommand dbCmd = null;
            try
            {
                _logger.Log(ClassTitle, "Executing SQL command", Category.Info);
                dbCmd = new MySqlCommand(strSQL, s_connection, s_transaction);
                ExecuteMySQLCmd(strSQL, commandTimeout, dbCmd, sqlParams);
            }
            finally
            {
                if (dbCmd != null)
                    dbCmd.Dispose();
            }
        }

        public static void ExecuteMySQLCmd(string strSQL, int commandTimeout, MySqlCommand dbCmd, params MySqlParameter[] sqlParams)
        {

            _logger.Log(ClassTitle, "Executing SQL command", Category.Info);

            Regex regex = new Regex("^GO", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            string[] lines = regex.Split(strSQL);

            if (commandTimeout > 0)
                dbCmd.CommandTimeout = commandTimeout;

            foreach (string line in lines.Where(l => l.Any()))
            {
                dbCmd.CommandText = line;
                if (sqlParams != null)
                {
                    sqlParams.ToList().ForEach(sp => dbCmd.Parameters.Add(sp));
                }
                dbCmd.ExecuteNonQuery();
            }
        }

        public void ExecuteMySqlScript(string strSQL)
        {
            var script = new MySqlScript(s_connection, strSQL);
            script.Execute();
        }

        /// <summary>
        /// runs sql scripts on db when there is no existing connection/db
        /// uses root user with 1234 pw
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="qtype"></param>
        /// <returns></returns>
        public static object RunSqlNoConex(string server, string sql, QueryType qtype)
        {
            string cs = "server=" + server + @";userid=root;password=1234;Allow User Variables=True";
            var conn = new MySqlConnection(cs);
            MySqlCommand cmd;
            object response; //SHOW DATABASES LIKE 'c4pssdb';
            try
            {
                conn.Open();
                cmd = new MySqlCommand(sql, conn);
                cmd.CommandTimeout = 0;
                switch (qtype)
                {
                    case QueryType.NonQuery:
                        response = cmd.ExecuteNonQuery();
                        break;
                    case QueryType.Scalar:
                        response = cmd.ExecuteScalar();
                        break;
                    default:
                        response = 0;
                        break;
                }

                return response == null ? 0 : response;

            }
            catch (MySqlException mysqlex)
            {
                _logger.Log(ClassTitle, "Failed creating DB" + mysqlex.Message, Category.Info);

            }
            catch (Exception ex)
            {
                _logger.Log(ClassTitle, "Failed creating DB" + ex.Message, Category.Info);
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }

            }

            return 0;
        }

        public string GetSingleLineMySqlCmd(string strSQL)
        {
            string str = "";
            using (MySqlCommand dbCmd = new MySqlCommand(strSQL, s_connection, s_transaction))
            {
                using (MySqlDataReader reader = dbCmd.ExecuteReader())
                {
                    int line = 0;
                    while (reader.Read())
                    {
                        str = reader.GetString(line);
                        line++;
                    }
                }
            }

            return str;
        }

        public DataTable GetAllResultMySqlCmd(string strSQL)
        {
            var dt = new DataTable();

            try
            {
                using (MySqlCommand dbCmd = new MySqlCommand(strSQL, s_connection, s_transaction))
                {
                    using (MySqlDataReader reader = dbCmd.ExecuteReader())
                    {
                        dt.Load(reader);
                        return dt;
                    }
                }
            }
            catch (MySqlException sqlEx)
            {
                _logger.Log(ClassTitle, $"Failed to get results with exception: {sqlEx.Message}", Category.Exception);
            }
            catch (Exception ex)
            {
                _logger.Log(ClassTitle, $"Failed to get results with exception: {ex.Message}", Category.Exception);
            }

            return dt;
        }

        public void Commit()
        {
            m_commit = true;
        }

        public void Rollback()
        {
            s_rollback = true;
        }

        ~TransactionConnectionManager()
        {
            Dispose(false);
        }
        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            _logger.Log(ClassTitle, "Disposing TransactionConnectionManager", Category.Info);
            if (!m_disposed)
            {
                m_disposed = true;
                if (!m_commit)
                {
                    s_rollback = true;
                }

                if (m_originatingManager)
                {
                    if (s_rollback)
                    {
                        s_transaction.Rollback();
                    }
                    else
                    {
                        s_transaction.Commit();
                    }

                    if (s_transaction != null)
                    {
                        s_transaction.Dispose();
                        s_transaction = null;
                    }

                    if (s_connection != null)
                    {
                        s_connection.Dispose();
                        s_connection = null;
                    }
                }

                // Suppress finalization of this disposed instance.
                if (disposing)
                {
                    GC.SuppressFinalize(this);
                }
            }
            _logger.Log(ClassTitle, "TransactionConnectionManager disposed succesfully", Category.Info);
        }
        private static string ClassTitle => "TransactionConnectionManager";
    }
}
