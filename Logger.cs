using System;
using System.Collections.Generic;
using System.Text;

namespace DLLFixer
{
    public class Logger
    {
        private List<string> logs = new List<string>();

        public void Log(string message)
        {
            string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}";
            logs.Add(logEntry);
            System.Diagnostics.Debug.WriteLine(logEntry);
        }

        public string GetLogs()
        {
            StringBuilder sb = new StringBuilder();
            foreach (string log in logs)
            {
                sb.AppendLine(log);
            }
            return sb.ToString();
        }

        public void ClearLogs()
        {
            logs.Clear();
        }
    }
}
