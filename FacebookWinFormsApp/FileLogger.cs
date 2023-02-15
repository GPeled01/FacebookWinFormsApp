using BasicFacebookFeatures.WithSingltonAppSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace BasicFacebookFeatures
{
    public class FileLogger: ILogger
    {
        private readonly string r_FileName;
        private Object m_LockWriteToLog = new Object();

        public ILoggerNotifier LoggerNotifier { get; set; }

        public FileLogger(ILoggerNotifier i_LoggerNotifier)
        {
            r_FileName = Application.ExecutablePath + ".log.txt";
            LoggerNotifier = i_LoggerNotifier;
            LoggerNotifier.m_ReportLoggers += this.writeToLog;
        }

        private void writeToLog(string i_Message)
        {
            try
            {
                lock (m_LockWriteToLog)
                {
                    if (!File.Exists(r_FileName))
                    {
                        using (StreamWriter stream = File.CreateText(r_FileName))
                        {
                            stream.WriteLine(i_Message);
                        }
                    }
                    else
                    {
                        using (StreamWriter stream = File.AppendText(r_FileName))
                        {
                            stream.WriteLine(i_Message);
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Failed to log to file.");
            }
        }
    }
}
