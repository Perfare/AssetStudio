using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AssetStudio;

namespace AssetStudioGUI
{
    class GUIProgress : IProgress
    {
        private Action<int> action;

        public GUIProgress(Action<int> action)
        {
            this.action = action;
        }

        public void Report(int value)
        {
            action(value);
        }
    }
}
