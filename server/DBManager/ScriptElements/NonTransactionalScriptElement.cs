using MySql.Data.MySqlClient;

namespace DBTools.ScriptElements
{
    public class NonTransactionalScriptElement : DatabaseScriptElement
    {
        public NonTransactionalScriptElement(string filename, string content)
          : base(filename, content)
        {
        }

        public override void Execute()
        {
            MySqlConnection conn = null;
            MySqlCommand command = null;
            try
            {
                conn = DBManager.GetNewConnection();
                command = new MySqlCommand(ScriptContent, conn);
                TransactionConnectionManager.ExecuteMySQLCmd(ScriptContent, 3600, command);
            }
            finally
            {
                command.Dispose();
                conn.Dispose();
            }
        }
    }
}
