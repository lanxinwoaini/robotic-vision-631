using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Robot_Racers.Joystick
{
    class JoyAxis
    {
        private int axisPos;
        private int axisId;
        private int axisZero;
        public JoyAxis()
        {
            axisId = -1;
            axisPos = 0;
            axisZero = 0;
        }


        public int AxisPos
        {
            set
            {
                axisPos = value;
            }
            get
            {
                return axisPos;
            }
        }

        public int AxisZero
        {
            set
            {
                axisZero = value;
            }
            get
            {
                return axisZero;
            }
        }

        public int AxisId
        {
            set
            {
                
                axisId = value;
            }
            get
            {
                return axisId;
            }
        }
    }
}
