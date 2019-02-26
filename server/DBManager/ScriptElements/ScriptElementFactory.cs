using System.Linq;

namespace DBTools.ScriptElements
{
    public class ScriptElementFactory
    {
        const string NON_TRANSACTIONAL = "NotInTransaction";

        public static IScriptElement GetScriptElement(string filename, string scriptContent)
        {
            if (filename.Split('-').Any(s => s == NON_TRANSACTIONAL))
                return new NonTransactionalScriptElement(filename, scriptContent);
            else
                return new DatabaseScriptElement(filename, scriptContent);
        }
    }
}
