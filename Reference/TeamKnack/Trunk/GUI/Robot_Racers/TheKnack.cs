using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Robot_Racers
{
    //SVN Repository:  trac.ee.byu.edu/svn/490Group1_w09
    static class TheKnack
    {
        public static bool debug = true;
        public static RobotRacer racer = null;
        public static Game game = null;
        public static XNAGame.xnaGame xnaGame = null;
       
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            racer = new RobotRacer();


            //READ THIS FIRST!!!!!!
            //ERROR DETAILS:
            //in order to run this project in debug mode, LoaderLock exception checking must be
            //disabled.
            //Click on (in Visual Studio) Debug->Exceptions
            //under Managed Debug Assistants UNCHECK Loader Lock. This will allow the directx dll's to load
            //ALSO: If you are running on an old machine that doesn't support directx9
            // when you click on the 3d-tab the project will crash
            Application.Run(racer);            
        }
    }
}
