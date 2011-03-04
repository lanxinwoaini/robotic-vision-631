using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


namespace Robot_Racers
{
    class MouseControl : Panel
    {
        bool isMouseDown = false;
        
        int y = 0;
        int x = 0;

        Timer mouseTimer = new Timer();

        private const int RADIUS = 70;              //Mouse control circle dimensions
        private const int DIAMETER = RADIUS * 2;
        private const int BUFFER = 5;

        public MouseControl()
        {
            mouseTimer.Interval = 100;              //Set timer interval in milliseconds
            mouseTimer.Tick += new System.EventHandler(mouseTimer_Tick);    //Add tick event handler
        }

        private void mouseTimer_Tick(object sender, EventArgs e)
        {
            if(isMouseDown){
                mouseTimer.Start();
            }
            else{
                mouseTimer.Stop();
                x = 0;
                y = 0;
            }
            if(RobotRacer.truckStarted && RobotRacer.mode == RobotRacer.Mode.MANUAL) // Only transmit in manual mode and when the start button has been pressed
                transmitMouseData();
        }

        protected override void OnPaint(PaintEventArgs paintEvnt)
        {
            Graphics g = paintEvnt.Graphics;
            Pen mypen = new Pen(Color.Black, 1);
            SolidBrush mybrush = new SolidBrush(Color.Blue);

            if (RobotRacer.mode == RobotRacer.Mode.MANUAL)
            {
                g.FillEllipse(mybrush, 0, 0, DIAMETER, DIAMETER);
                g.DrawLine(mypen, RADIUS - BUFFER, 1, RADIUS - BUFFER, DIAMETER - 2);
                g.DrawLine(mypen, RADIUS + BUFFER, 1, RADIUS + BUFFER, DIAMETER - 2);
                g.DrawLine(mypen, 1, RADIUS - BUFFER, DIAMETER - 2, RADIUS - BUFFER);
                g.DrawLine(mypen, 1, RADIUS + BUFFER, DIAMETER - 2, RADIUS + BUFFER);
            }
            else
            {
                try
                {
                    Image bigTruck = Image.FromFile(TheKnack.racer.runtimeDirectory+"\\bigTruck.gif");
                    g.DrawImage(bigTruck, 20, 0);
                }
                catch (Exception e) { Console.WriteLine("At the knack image " + e.Message); }
                
            }
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            isMouseDown = true;
            if (RobotRacer.truckStarted && RobotRacer.mode == RobotRacer.Mode.MANUAL)// Only transmit in manual mode and when the start button has been pressed
            {
                Cursor.Clip = new Rectangle(this.Parent.PointToScreen(this.Location), this.Size);
                mouseTimer.Start();
                transmitMouseData();
            }
            this.Capture = true;
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            this.Capture = false;
            isMouseDown = false;
            Cursor.Clip = new Rectangle();
            x = 0;
            y = 0;
            if (RobotRacer.truckStarted && RobotRacer.mode == RobotRacer.Mode.MANUAL)// Only transmit in manual mode and when the start button has been pressed
            {
                mouseTimer.Stop();
                transmitMouseData();
            }
        }
        
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            
            if (isMouseDown && RobotRacer.mode == RobotRacer.Mode.MANUAL)
            {
                x = e.X - RADIUS;
                y = e.Y - RADIUS;

                /*if (mouseTimer.Enabled == false)  // I don't think this is a very good idea right now because the truck could flip and break the camera
                {
                    if (Math.Sqrt(x * x + y * y) <= RADIUS)
                    {
                        mouseTimer.Start();     // Start truck once it enters the circle again
                    }
                }
                 */
            }
        }

        private void transmitMouseData()
        {
            int tempX = x;
            int tempY = y;

            //if (Math.Sqrt(x * x + y * y) > RADIUS)
            //{
            //    //x = tempX = 0;                              // Stop truck if mouse has left the circle
            //    //y = tempY = 0;
            //    //mouseTimer.Stop();
            //}
            //else
            //{
                if (Math.Abs(x) <= BUFFER)                  // Here only when the mouse is in the circle
                    tempX = 0;
                else if (x > 0)                             // Account for the zero buffers in the circle cross
                    tempX =  -tempX +5;
                else
                    tempX = -tempX - 5;

                if (Math.Abs(y) <= BUFFER)                  // Adjust values to reflect forward velocity with positive numbers
                    tempY = 0;
                else if (y > 0)
                    tempY = -tempY + BUFFER;
                else
                    tempY = -tempY - BUFFER;

                tempX = (int)(((double)tempX / (double)(RADIUS - BUFFER*2)) * 50.0);  // Change x and y to a value between 0 and 100
                tempY = (int)(((double)tempY / (double)(RADIUS - BUFFER*2)) * 50.0);
                if (tempX > 100)                            // Limit to 100 as rounding errors can give higher values
                    tempX = 100;
                if (tempX < -100)
                    tempX = -100;
                if (tempY > 100)
                    tempY = 100;
                if (tempY < -100)
                    tempY = -100;
                //scale down the mouse control if needed:
                
            //}

            SerialHeader header = new SerialHeader();                       // Setup transmission header
            header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
            header.Subtype = (byte)SerialUtils.CommandSubtype.VELOCITY_STEERING;

            int velocitySteering = (int)((tempY << 16) & 0xFFFF0000) | (0x0000FFFF & tempX); // Concatonate velocity and steering

            RobotRacer.serialPort.transmitIntData(header, velocitySteering);   // Transmit velocity and stering

            //Console.WriteLine(tempX + ", " + tempY);
        }
    }
}
