using System;
using System.Globalization;
using System.IO;

namespace DBTools.ScriptElements
{
    public class DatabaseScriptElement : IScriptElement
    {
        /// <summary>
        /// Initializes a script element, assumes that the filename starts with the filename in the following format:
        /// YYYYMMDD-ORDERNUM-Name of script.sql
        /// Otherwise uses the file date for the ordering
        /// </summary>
        /// <param name="filename"></param>
        public DatabaseScriptElement(string filename, string scriptContent)
        {
            string[] fileSplit = filename.Split('-');

            DateTime fileDate;
            if (!DateTime.TryParseExact(fileSplit[0], "yyyyMMdd", CultureInfo.CurrentCulture, DateTimeStyles.None, out fileDate))
            {
                fileDate = File.GetLastWriteTime(filename);
            }

            Date = fileDate;

            int orderNumber;
            if (!int.TryParse(fileSplit[1], out orderNumber))
            {
                orderNumber = 0;
            }

            OrderNumber = orderNumber;

            Filename = filename;
            ScriptContent = scriptContent;
        }

        public string Filename { get; set; }
        public int OrderNumber { get; set; }
        public string ScriptContent { get; set; }
        public DateTime Date { get; set; }

        /// <summary>
        /// Executes the ScriptContent on the sql server
        /// </summary>
        public virtual void Execute()
        {
            using (TransactionConnectionManager tcm = new TransactionConnectionManager())
            {
                tcm.ExecuteMySQLCmd(ScriptContent, 3600);
                tcm.Commit();
            }
        }
    }
}
