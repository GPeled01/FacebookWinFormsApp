using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BasicFacebookFeatures
{
    public class ConsoleLogger : ILogger
    {
        private Object m_LockWriteToLog = new Object();

        public ILoggerNotifier LoggerNotifier { get; set; } 

        public ConsoleLogger(ILoggerNotifier i_LoggerNotifier)
        {
            LoggerNotifier = i_LoggerNotifier;
            LoggerNotifier.m_ReportLoggers += this.writeToLog;
        }

        private void writeToLog(string i_Message)
        {
            try
            {
                lock (m_LockWriteToLog)
                {
                    Console.WriteLine(i_Message);
                }
            }
            catch
            {
                Console.WriteLine("Failed to log to console.");
            }
        }
    }
}
