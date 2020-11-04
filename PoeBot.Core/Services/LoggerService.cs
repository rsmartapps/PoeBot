using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PoeBot.Core.Services
{
    public class LoggerService
    {
        public string LOGS_PATH = @"C:\Users\Ruben\Desktop\tests\logs\log.txt";
        StreamWriter writer;
        FileStream fileStream;
        public LoggerService(string log_path = null)
        {
            if (!string.IsNullOrWhiteSpace(log_path))
            {
                LOGS_PATH = log_path;
            }
            if (!File.Exists(LOGS_PATH))
            {
                Directory.CreateDirectory(LOGS_PATH);
            }

            fileStream = new FileStream(LOGS_PATH, FileMode.OpenOrCreate);
            writer = new StreamWriter(fileStream);
        }
        public void Log(string logMessage)
        {
            try{
                logMessage = DateTime.Now.ToLongDateString() + " " + logMessage;
                writer.WriteLine(logMessage);
                writer.Flush();
            }
            catch
            {

            }
        }
    }
}
