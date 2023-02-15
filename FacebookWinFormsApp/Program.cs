using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using FacebookWrapper;

// $G$ THE-001 (-7) Grade: 93 on patterns selection / accuracy in implementation / description / document / diagrams (50%) (see comments in document).
// $G$ DSN-999 (-15) It is possible to click on controls before login process and it causes the application to crash.
// $G$ CSS-999 (-5) StyleCop errors.

namespace BasicFacebookFeatures
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Clipboard.SetText("design.patterns20cc");
            FacebookService.s_UseForamttedToStrings = true;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}
