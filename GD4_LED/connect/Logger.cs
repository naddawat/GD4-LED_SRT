using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GD4_LED.connect
{
    class Logger
    {
        public void WriteLog(string strDesc, string filename)
        {
            StreamWriter logStr = default(StreamWriter);
            string logName = null;
            // logName = My.Settings.Logs & Now.ToString("yyyyMMdd") & "_" & filename & ".log"
            logName = "Logs\\" + filename + "_" + DateTime.Now.ToString("yyyyMMdd") + ".log";
            if (File.Exists(logName))
            {
                logStr = File.AppendText(logName);
                logStr.WriteLine("[" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "] " + strDesc);
                logStr.Flush();
                logStr.Close();
            }
            else
            {
                //File.Create(logName);
                logStr = File.CreateText(logName);
                logStr.WriteLine("[" + DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") + "] " + strDesc);
                logStr.Flush();
                logStr.Close();
            }
        }
    }
}
