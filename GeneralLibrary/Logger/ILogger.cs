using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
    public interface ILogger
    {
        void StartLog(string prefix);
        void Log(string message, Category category, string user = "");

        void Log(string prefix, string message, Category category, string user = "");

        void EndLog();
    }
}
