using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hardware.Extension.EPSPaymentConnector
{
    public static class Logger
    {
        #region Private variables
        private const string FILE_EXT = ".log";
        private static readonly string datetimeFormat;
        private static readonly string logFilename;
        #endregion
        static Logger()
        {
            datetimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
            logFilename = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + FILE_EXT;

            // Log file header line
            string logHeader = logFilename + " is created.";
            if (!System.IO.File.Exists(logFilename))
            {
                WriteLog(System.DateTime.Now.ToString(datetimeFormat) + " " + logHeader, false);
            }

            WriteLog("Entered constructor for method: Logger ", true);
        }
        
        public static void WriteLog(string text,bool append=true)
        {
            //TODO:Make this configurable based on setting
            try
            {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logFilename, append, System.Text.Encoding.UTF8))
                {
                    if (!string.IsNullOrEmpty(text))
                    {
                        writer.WriteLine(DateTime.Now.ToString(datetimeFormat) +" " + text);
                    }
                }
            }
            catch(Exception ex)
            {
                throw;
            }
        }
    }
}
