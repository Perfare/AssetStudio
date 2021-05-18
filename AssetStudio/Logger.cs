using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AssetStudio
{
    public static class Logger
    {
        public static List<ILogger> LoggerList = new List<ILogger>();
        static Logger()
        {

        }

        public static void RegistLogger(ILogger logger)
        {
            if (logger == null)
                return;

            LoggerList.Add(logger);
        }


        private static void Log(LoggerEvent loggerEvent, string message)
        {
            foreach (var log in LoggerList)
            {
                log.Log(loggerEvent, message);
            }
        }

        public static void Verbose(string message) => Log(LoggerEvent.Verbose, message);
        public static void Debug(string message) => Log(LoggerEvent.Debug, message);
        public static void Info(string message) => Log(LoggerEvent.Info, message);
        public static void Warning(string message) => Log(LoggerEvent.Warning, message);
        public static void Error(string message) => Log(LoggerEvent.Error, message);
    }
}
