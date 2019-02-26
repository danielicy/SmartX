using DBTools.ScriptElements;
using Logger;
 
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Configuration;

namespace DBTools
{
    public class DBManager
    {

        #region Logger
        private static ILogger _logger;
        
        #endregion

        Dictionary<string, string> _connectionContainer = new Dictionary<string, string>();
        public static IDbConnection m_conn;
        public static IDbTransaction m_trans;

        private UpgradeExecutor executor;

        private static string _connectionString;

        public DBManager(string connectionstring)
        {
            _connectionString = connectionstring;

            fillConnectionContainer(_connectionString);

            executor = new UpgradeExecutor();
        } 
        

        private void fillConnectionContainer(string connectionString)
        {
            string[] connectionparser = connectionString.Split(';');
            int arraylenght = connectionparser.Length;

            foreach (string str in connectionparser)
            {

                string[] v = str.Split('=');
                if (!string.IsNullOrEmpty(v[0]))
                    _connectionContainer.Add(v[0], v[1]);
            }
        }

        private static string ClassTitle => "DBUpgrade";

        /// <summary>
        /// runs 
        /// </summary>
        /// <param name="version"></param>
        /// <param name="initVal"></param>
        /// <returns></returns>
        public int RunUpgradeProcess(string version, int initVal)
        {
            _logger.Log(ClassTitle, "Starting DB Upgrade process", Category.Info);

            int retVal = 0;

            if (retVal == 0)
            {
                try
                {
                    executor.UpdateDatabase(version, _connectionContainer["server"], _connectionContainer["database"], initVal);
                }
                catch (Exception e)
                {
                    retVal = 1;
                    _logger.Log(ClassTitle, "Exception during Upgrade: " + e.Message, Category.Exception);
                }
            }
            return retVal;
        }

        public static MySqlConnection GetNewConnection()
        {
            var conn = new MySqlConnection(_connectionString);
            try
            {
                _logger.Log(ClassTitle, "Openning connection to DB", Category.Info);


                conn.Open();


            }
            catch (MySqlException ex)
            {

                _logger.Log(ClassTitle, "MySqlException during Upgrade: " + ex.Message, Category.Exception);
            }
            catch (Exception ex)
            {
                _logger.Log(ClassTitle, "Exception during Upgrade: " + ex.Message, Category.Exception);
            }
            return conn;

        }

        public bool CheckDbConnection(string connectionString)
        {
            var ifConnOk = true;

            var _connString = connectionString;

            var sqlUsername = GetUsername(_connString);
            var sqlPassword = GetPassword(_connString);

            var conn = new MySqlConnection(_connString);
            try
            {
                _logger.Log("CheckDbConnection: ", $"Trying to connection to DB with User {sqlUsername} to Server {GetServerName(_connString)}", Category.Info);
                conn.Open();
                conn.Close();
                _logger.Log("CheckDbConnection: ", $"Successfully connected to DB with User {sqlUsername} to Server {GetServerName(_connString)}", Category.Info);
            }
            catch (MySqlException ex)
            {
                var errorLog = string.Empty;
                if (ex.Number == 0)
                {
                    errorLog = $"Cannot connect to server. Contact administrator\nError:\n{ex.Message}\nAdditional Info:\nUsername: {sqlUsername}\nPassword: {sqlPassword}";
                }
                else if (ex.Number == 1045)
                {
                    errorLog =
                        $"Invalid Username {sqlUsername} / password {sqlPassword}, \nPlease set the correct user/password in app.config file";
                }
                if (errorLog != string.Empty)
                {
                    _logger.Log("CheckDbConnection: ", $"Something wrong with SqlServer:\n{errorLog}", Category.Exception);
                    //MessageBox.Show(errorLog);
                }
                else
                {
                    _logger.Log("CheckDbConnection: ", $"Something wrong with SqlServer: {ex.Message}", Category.Exception);
                    //MessageBox.Show($"Something wrong with SqlServer: {ex.Message}");
                }
                ifConnOk = false;
            }
            return ifConnOk;
        }

        #region SqlConnectionParser

        /// <summary>
        /// ////////////////////This Section is tool to parsing sql connection string using aliass name for diffrent sqlConn types
        /// </summary>
        private static readonly string[] ServerAliases = { "server", "host", "data source", "datasource", "address", "addr", "network address" };
        private static readonly string[] DatabaseAliases = { "database", "initial catalog" };
        private static readonly string[] UsernameAliases = { "user id", "uid", "username", "user name", "user", "userid" };
        private static readonly string[] PasswordAliases = { "password", "pwd" };

        public string GetPassword(string connectionString)
        {
            return GetValue(connectionString, PasswordAliases);
        }

        public string GetUsername(string connectionString)
        {
            return GetValue(connectionString, UsernameAliases);
        }

        public string GetDatabaseName(string connectionString)
        {
            return GetValue(connectionString, DatabaseAliases);
        }

        public string GetServerName(string connectionString)
        {
            return GetValue(connectionString, ServerAliases);
        }

        private static string GetValue(string connectionString, params string[] keyAliases)
        {
            var keyValuePairs = connectionString.Split(';')
                                                .Where(kvp => kvp.Contains('='))
                                                .Select(kvp => kvp.Split(new char[] { '=' }, 2))
                                                .ToDictionary(kvp => kvp[0].Trim(),
                                                              kvp => kvp[1].Trim(),
                                                              StringComparer.InvariantCultureIgnoreCase);
            foreach (var alias in keyAliases)
            {
                string value;
                if (keyValuePairs.TryGetValue(alias, out value))
                    return value;
            }
            return string.Empty;
        }

      

        #endregion
    }
}
