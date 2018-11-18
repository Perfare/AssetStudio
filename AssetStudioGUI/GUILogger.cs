using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetStudio;

namespace AssetStudioGUI
{
    class GUILogger : ILogger
    {
        private Action<string> action;

        public GUILogger(Action<string> action)
        {
            this.action = action;
        }

        public void Log(LoggerEvent loggerEvent, string message)
        {
            action(message);
        }
    }
}
