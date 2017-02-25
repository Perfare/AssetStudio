using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Unity_Studio
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            OpenTK.Toolkit.Init(new OpenTK.ToolkitOptions { EnableHighResolution = false });
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UnityStudioForm());
        }
    }
}
