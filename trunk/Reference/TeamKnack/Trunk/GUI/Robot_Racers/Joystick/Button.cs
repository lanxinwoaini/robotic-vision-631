using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Robot_Racers.Joystick
{
    class JoyButton
    {
        private int buttonId;
        private bool buttonStatus;
        public JoyButton()
        {
            buttonId = -1;
            buttonStatus = false;
        }

        
        public int ButtonId
        {
            get { return buttonId; }
            set
            {
                buttonId = value;
            }
        }

        
        public bool ButtonStatus
        {
            get { return buttonStatus; }
            set
            {
                buttonStatus = value;
            }
        }
    }
}
