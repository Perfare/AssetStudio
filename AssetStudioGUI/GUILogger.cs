using AssetStudio;
using System;
using System.Windows.Forms;

namespace AssetStudioGUI
{
    class GUILogger : ILogger
    {
        public bool ShowErrorMessage = true;
        private Action<string> action;

        public GUILogger(Action<string> action)
        {
            this.action = action;
        }

        public void Log(LoggerEvent loggerEvent, string message)
        {
            switch (loggerEvent)
            {
                case LoggerEvent.Error:
                    if (ShowErrorMessage)
                    {
                        MessageBox.Show(message);
                    }
                    break;
                default:
                    action(message);
                    break;
            }

        }
    }
}
