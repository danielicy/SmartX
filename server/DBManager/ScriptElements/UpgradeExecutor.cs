using Logger;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace DBTools.ScriptElements
{
    public class UpgradeExecutor
    {
        #region Logger
        private static Logger.ILogger _logger;
        #endregion

        #region private members
        private string _dumpPath;
        private string _mySqlVersion;
        private string _server;
        private string _db;

        #endregion

        /// <summary>
        /// Gets all script elements from the embedded resources of the current assembly.
        /// All custom classes that should be run in parallel with scripts should be created and added to the scriptElements array in this method.
        /// </summary>
        public int UpdateDatabase(string version, string server, string db, int createDemo)
        {
            try
            {
                _server = server;
                _db = db;


                GetMysqlDirectoryPath();

                UpdateToDbUpgrade(createDemo);

                string[] names = Assembly.GetExecutingAssembly().GetManifestResourceNames();

                string[] sqlScriptNames = names.Where(n => Path.GetExtension(n).ToLower() == ".sql").ToArray();

                IScriptElement[] scriptElements = sqlScriptNames.Select(ssn => CreateDatabaseScriptElement(ssn)).ToArray();

                UpdateDatabase(scriptElements);

                UpdateDBVersion(version);

                return 0;
            }
            catch (Exception ex)
            {
                _logger.Log(ClassTitle, $"Failed to Update DB with exception: {ex.Message}", Category.Exception);
                return 1;
            }
        }

        /// <summary>
        /// updates database to Version 2.1.0 (first version to use DBUpgrade)
        /// </summary>
        private void UpdateToDbUpgrade(int createDemo)
        {
            try
            {
                bool dbVersionExists = false;

                bool dbExists = CheckIfDBExists();


                if (dbExists)
                    dbVersionExists = DBVersionTblExists();
                else
                    SetDumpImportOption(createDemo);

                if (dbExists && !dbVersionExists)
                {
                    DumpDB(IsDbPartitioned());

                    DropDB();
                }

                if (!dbVersionExists)
                {
                    CreateDBSchema();

                    ImportDump(_dumpPath);
                }

                _logger.Log(ClassTitle, "Finished  Updating ToDbUpgrade succesfully", Category.Info);

            }
            catch (Exception ex)
            {
                _logger.Log(ClassTitle, "Failed to UpdateToDbUpgrade c4pssdb: " + ex.Message, Category.Exception);
            }
        }

        private bool IsDbPartitioned()
        {
            _logger.Log(ClassTitle, "Checking database is partitioned", Category.Info);
            var query = TransactionConnectionManager.RunSqlNoConex(_server, "SELECT count(*) FROM information_schema.TABLES WHERE(TABLE_SCHEMA = 'c4pssdb') AND(TABLE_NAME = 'log_actions')", QueryType.Scalar);

            return ((Int64)query == 1) ? true : false;
        }

        public bool CheckIfDBExists()
        {
            try
            {
                var query = TransactionConnectionManager.RunSqlNoConex(_server, "SHOW DATABASES LIKE '" + _db + "';", QueryType.Scalar).ToString();

                if ((string)query == "c4pssdb")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.Log(ClassTitle, "Failed Checking if database exists: " + ex.Message, Category.Info);
                return false;
            }


        }

        /// <summary>
        /// imports data from backup dump to current schema
        /// </summary>
        public void ImportDump(string path)
        {
            try
            {
                _logger.Log(ClassTitle, "Importing Dump to c4pssdb", Category.Info);
                using (TransactionConnectionManager transMon = new TransactionConnectionManager())
                {
                    transMon.ExecuteMySqlScript("SET GLOBAL FOREIGN_KEY_CHECKS=0;");

                    RunCmdProcess(Path.GetDirectoryName(@"C:\Program Files\MySQL\MySQL Server " + _mySqlVersion + @"\bin\mysql.exe"), @"/c mysql --protocol=tcp --host=localhost --user=root --password=1234 --port=3306 --default-character-set=utf8 --comments --database=c4pssdb < " + "\"" + path + "\"");

                    transMon.ExecuteMySqlScript("SET GLOBAL FOREIGN_KEY_CHECKS=1;");
                }
                _logger.Log(ClassTitle, "Imported Dump to c4pssdb succesfully ", Category.Info);
            }
            catch (Exception ex)
            {
                _logger.Log(ClassTitle, "Failed Importing Dump c4pssdb: " + ex.Message, Category.Exception);
            }

        }

        private void DumpDB(bool isPartitioned)
        {
            try
            {
                _dumpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "c4backup" + DateTime.Now.Year.ToString() +
                    DateTime.Now.Month.ToString("00") + DateTime.Now.Day.ToString("00") + "_" + DateTime.Now.Hour.ToString("00") + DateTime.Now.Minute.ToString("00") + ".sql");
                _logger.Log(ClassTitle, "Dumping database", Category.Info);

                if (File.Exists(_dumpPath))
                {
                    try
                    {
                        File.Delete(_dumpPath);
                    }
                    catch (IOException ex)
                    {
                        _logger.Log(ClassTitle, $"Failed to delete the file ({_dumpPath}) with exception: {ex.Message}", Category.Exception);
                    }
                }
                string nonPartitionedParams = "--databases c4pssdb  --routines --triggers --add-drop-database --add-drop-table --add-locks --no-create-db --no-create-info --extended-insert --password=1234  --user=root --disable-keys --quick --comments --complete-insert  --result-file=" + "\"" + _dumpPath + "\"";
                string partitionedParams = "--databases c4pssdb --user=root --password=1234 --host=localhost --protocol=tcp --port=3306 --default-character-set=utf8 --single-transaction=TRUE --routines --no-create-info=TRUE --skip-triggers --result-file=" + "\"" + _dumpPath + "\"";

                RunCmdProcess(@"C:\Program Files\MySQL\MySQL Server " + _mySqlVersion + @"\bin\", @"/c mysqldump " + ((isPartitioned) ? partitionedParams : nonPartitionedParams));

                _logger.Log(ClassTitle, "dumped c4pssdb to " + _dumpPath + " succesfully", Category.Info);
            }
            catch (IOException ioex)
            {

                throw new Exception("Failed to dump Database:" + ioex.Message);

            }
            catch (Exception ex)
            {
                // Logger.Log(ClassTitle, "Failed to dump Database: " + ex.Message, Category.Exception);
                throw new Exception("Failed to dump Database:" + ex.Message);
            }


        }

        /// <summary>
        /// sets which script to run in ImportDump Method
        /// </summary>
        /// <param name="InitializeDB">1 creates demo DB,
        /// 2 creates creates new empty DB,
        /// 3 creates existing partitioned db
        /// 4 upgrades existing non partitioned db</param>
        private void SetDumpImportOption(int InitializeDB)
        {
            _dumpPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "InitData");

            switch (InitializeDB)
            {
                case 0:
                    // create release data                   
                    _dumpPath = Path.Combine(_dumpPath, "ReleaseData.sql");
                    break;
                case 1:
                    //create new db
                    _dumpPath = Path.Combine(_dumpPath, "DemoData.sql");
                    break;
            }

        }

        /// <summary>
        /// runs external procecces
        /// </summary>
        /// <param name="workingdirectory"></param>
        /// <param name="args"></param>
        private void RunCmdProcess(string workingdirectory, string args)
        {
            try
            {
                _logger.Log(ClassTitle, "running cmd process @ " + workingdirectory, Category.Info);
                Process sd = null;
                ProcessStartInfo processInfo = new ProcessStartInfo(workingdirectory, args);
                processInfo.CreateNoWindow = true;
                processInfo.WindowStyle = ProcessWindowStyle.Normal;
                processInfo.UseShellExecute = true;
                processInfo.RedirectStandardInput = false;
                processInfo.FileName = "cmd.exe";
                processInfo.Verb = "runas";
                processInfo.WorkingDirectory = workingdirectory;
                sd = Process.Start(processInfo);
                sd.WaitForExit();

                if (!sd.HasExited)
                {
                    sd.Close();
                }
                sd.Dispose();
                processInfo = null;
                sd = null;
                _logger.Log(ClassTitle, "cmd process ended", Category.Info);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to run external commande:" + ex.Message);
            }


        }

        /// <summary>
        /// Creates DB schema 
        /// </summary>
        private void CreateDBSchema()
        {
            //check if script table exists, adds if not
            try
            {
                _logger.Log(ClassTitle, "recreating " + _server + " schema", Category.Info);

                //TransactionConnectionManager.RunSqlNoConex(_server, Properties.Resources.CreateC4pssdb, QueryType.Scalar);

                _logger.Log(ClassTitle, "created  " + _server + " succesfully", Category.Info);
            }
            catch (Exception ex)
            {
                _logger.Log(ClassTitle, "Failed Creating c4pssdb: " + ex.Message, Category.Exception);
            }

        }

        /// <summary>
        /// Drops Databse
        /// </summary>
        private void DropDB()
        {
            using (TransactionConnectionManager transMon = new TransactionConnectionManager())
            {
                try
                {
                    _logger.Log(ClassTitle, "Dropping  DB", Category.Info);
                    transMon.ExecuteMySqlScript("DROP  SCHEMA IF EXISTS  c4pssdb");
                    _logger.Log(ClassTitle, "c4pssdb schema dropped succesfully", Category.Info);
                }
                catch (Exception ex)
                {
                    _logger.Log(ClassTitle, "failed dropping  DB", Category.Exception);
                    throw new Exception("failed dropping DB " + ex.Message);
                }
            }


        }

        /// <summary>
        /// this is the condition to upgrade db
        /// this happens only when upgrading versions prior to V 2.1.0
        /// </summary>
        /// <param name="transMon"></param>
        /// <returns></returns>
        private bool DBVersionTblExists()
        {
            _logger.Log(ClassTitle, "Checking if dbversion table exists", Category.Info);
            var query = TransactionConnectionManager.RunSqlNoConex(_server, "select count(*) from information_schema.tables where table_name = 'dbversion'", QueryType.Scalar);

            return ((Int64)query == 0) ? false : true;
        }

        private IScriptElement CreateDatabaseScriptElement(string filename)
        {
            _logger.Log(ClassTitle, "Creating database script elements", Category.Info);
            var assembly = Assembly.GetExecutingAssembly();
            using (Stream resourceStream = assembly.GetManifestResourceStream(filename))
            {
                using (StreamReader sr = new StreamReader(resourceStream))
                {
                    // assuming the filename ends in .sql, get the last section of '.', this is assuming that the filename comes from a manifest resource which has the form of
                    // assemblyname.directory.filename.sql
                    // we are looking to extract the filename.sql part
                    string scriptPureName = filename.Substring(filename.LastIndexOf('.', filename.Length - 5) + 1);
                    _logger.Log(ClassTitle, "Database script elements created", Category.Info);
                    return ScriptElementFactory.GetScriptElement(scriptPureName, sr.ReadToEnd());
                }
            }

        }

        /// <summary>
        /// Executes all script elements on the current database
        /// </summary>
        /// <param name="scripts"></param>
        private void UpdateDatabase(IScriptElement[] scripts)
        {
            bool updatedDb = false;
            _logger.Log(ClassTitle, "updating Database ", Category.Info);

            //make sure scripts are executed by date / order number
            var orderedScripts = scripts.OrderBy(se => se.Date.Date).ThenBy(se => se.OrderNumber);

            try
            {
                using (TransactionConnectionManager transMon = new TransactionConnectionManager())
                {

                    // VerifyUpgradeTablesExist(transMon);
                    UpdateTSinDb(transMon);
                }

                foreach (IScriptElement element in orderedScripts)
                {
                    using (TransactionConnectionManager transMon = new TransactionConnectionManager())
                    {
                        // check whether this script has been run before using the filename as the key
                        if (transMon.ExecuteScalar("select count(*) from DatabaseScript where Filename = @Filename", new MySqlParameter("@Filename", element.Filename)) == 0)
                        {
                            _logger.Log(ClassTitle, "Executing " + element.Filename, Category.Info);

                            element.Execute();
                            transMon.ExecuteMySQLCmd("INSERT INTO DatabaseScript (Filename, Description, DateRun) VALUES (@Filename, @Description, @DateRun)", 0,
                                new MySqlParameter("@Filename", element.Filename),
                                new MySqlParameter("@Description", ""), new MySqlParameter("@DateRun", DateTime.UtcNow));
                            updatedDb = true;
                        }

                        transMon.Commit();

                    }
                }
            }
            catch (Exception e)
            {
                _logger.Log(ClassTitle, "Exception while performing update script", Category.Exception);
                throw new Exception("Failed to Update Existing Schema " + e.Message);
            }
            if (updatedDb)
                _logger.Log(ClassTitle, "DB Upgraded succesfully", Category.Info);
        }

        /// <summary>
        ///     waits for partitioning script to end
        /// </summary>
        /// <param name="connection"></param>
        private static void UpdateTSinDb(TransactionConnectionManager transMon)
        {
            var tsInfo = new List<string>();

            try
            {
                DataRowCollection queryResults = transMon.GetAllResultMySqlCmd("SELECT id,domain FROM c4pssdb.transaction_servers where domain is not null;").Rows;


                if (queryResults.Count < 1)
                {
                    tsInfo = ReadTsInfoFromRegistry();

                    string updated = null;

                    int idx = 0;

                    //write ts info retrieved from registry to DB
                    foreach (string str in tsInfo)
                    {

                        transMon.ExecuteMySqlScript($"INSERT INTO `c4pssdb`.`transaction_servers` (`id`, `description`, `domain`, `uri`, `active`, `secure`) " +
                            $"VALUES ('{idx + 1}', 'TS #{idx + 1}','{tsInfo[idx]}', '/', '1','1')");



                        updated = updated + tsInfo[idx];
                        idx++;
                        _logger.Log(ClassTitle, $"TS Table  Value updated successfully: " + str, Category.Info);

                    }

                    transMon.Commit();
                }
                else
                {
                    ValidateRegistryValues(transMon, queryResults[0]);
                }



            }
            catch (Exception ex)
            {
                _logger.Log(ClassTitle, $"Failed to Update TS Value with exception:\n {ex.Message}", Category.Exception);
            }
        }

        private static void ValidateRegistryValues(TransactionConnectionManager transMon, DataRow dataRow)
        {

            var item = dataRow.ItemArray[1];


            if (item.ToString() != RegistryHelper.RegistryHelper.GetRegistryValue("TRANS1_DOMAIN"))
            {
                RegistryHelper.RegistryHelper.SetRegistryKey("TRANS1_DOMAIN", item.ToString());
            }
            _logger.Log(ClassTitle, $"Failed to Update TS Value with exception: ", Category.Exception);
        }

        private static List<string> ReadTsInfoFromRegistry()
        {
            var tsInfo = new List<string>();



            if (string.IsNullOrEmpty((RegistryHelper.RegistryHelper.GetRegistryValue("TRANS1_DOMAIN"))))
            {
                _logger.Log(ClassTitle, $"**********************************************\n" +
                                        "****************************************************************************************** \n" +
                                        "***********************  TS INFO MISSING!!!!!!!!!   ************************\n" +
                                        "********************************************************************************************\n" +
                                        "*********************************************************************************************", Category.Exception);
                throw new Exception("Ts info is missing, please contact system Administrator");

            }

            tsInfo.Add(RegistryHelper.RegistryHelper.GetRegistryValue("TRANS1_DOMAIN"));

            if (!string.IsNullOrEmpty((RegistryHelper.RegistryHelper.GetRegistryValue("TRANS2_DOMAIN"))))
            {
                tsInfo.Add(RegistryHelper.RegistryHelper.GetRegistryValue("TRANS2_DOMAIN"));
            }

            return tsInfo;

        }

        /// <summary>
        /// writes current assembly version to dbversion table
        /// </summary>
        /// <param name="version"></param>
        private void UpdateDBVersion(string version)
        {
            using (TransactionConnectionManager transMon = new TransactionConnectionManager())
            {
                _logger.Log(ClassTitle, "Updating DBVersion", Category.Info);
                if ((transMon.ExecuteScalar($"SELECT COUNT(*) FROM c4pssdb.dbversion WHERE Version = '{version}'")) != 1)
                {
                    transMon.ExecuteMySqlScript("INSERT INTO c4pssdb.dbversion (version) VALUES ('" + version + "');");
                }
                transMon.Commit();
                _logger.Log(ClassTitle, "Updated DBVersion", Category.Info);
            }
        }

        /// <summary>
        /// gets mysql server installation path
        /// </summary>
        private void GetMysqlDirectoryPath()
        {
            _logger.Log(ClassTitle, "Getting  Getting Mysql Directory Path", Category.Info);
            string[] _mySqlPath;

            _mySqlPath = RegistryHelper.RegistryHelper.GetRegistrySubKeyNames(@"SOFTWARE\Wow6432Node\MySQL AB\");
            foreach (string str in _mySqlPath)
            {
                if (str.Contains("MySQL Server"))
                {
                    _mySqlVersion = str.Replace("MySQL Server ", "");
                }
            }


        }



        private static string ClassTitle => "DBUpgrade";

    }

    public enum QueryType
    {
        Scalar = 1,
        NonQuery
    }
}
