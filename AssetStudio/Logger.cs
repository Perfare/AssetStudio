using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public static class Logger
    {
        public static ILogger Default = new DummyLogger();

        public static void Verbose(string message) => Default.Log(LoggerEvent.Verbose, message);
        public static void Debug(string message) => Default.Log(LoggerEvent.Debug, message);
        public static void Info(string message) => Default.Log(LoggerEvent.Info, message);
        public static void Warning(string message) => Default.Log(LoggerEvent.Warning, message);
        public static void Error(string message) => Default.Log(LoggerEvent.Error, message);

        public static void Error(string message, Exception e)
        {
            var sb = new StringBuilder();
            sb.AppendLine(message);
            sb.AppendLine(e.ToString());
            Default.Log(LoggerEvent.Error, sb.ToString());
        }
    }
}
