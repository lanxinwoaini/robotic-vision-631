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
    public partial class CourseGraphicsPanel : Panel
    {
        Course course;

        public const int PANE_WIDTH = 400;
        public const int PANE_HEIGHT = 172;
        public const int BUFFER = 50;
        public const int PYLON_DIAMETER = 8;
        

        public CourseGraphicsPanel()
        {
                course = RobotRacer.currentCourse;
        }
        
        protected override void OnPaint(PaintEventArgs paintEvnt)
        {
            Graphics gfx = paintEvnt.Graphics; // Get the graphics object
            course = RobotRacer.currentCourse;

            if (course != null && course.isCourseValid() && (course.getNumPoints() <= 3 || allSidesAreEqual()))
            {

                Graphics g = paintEvnt.Graphics;
                Pen mypen = new Pen(Color.Black, 1);
                SolidBrush greenBrush = new SolidBrush(Color.Green);
                SolidBrush redBrush = new SolidBrush(Color.Red);
                SolidBrush blackBrush = new SolidBrush(Color.Black);

                int startAngle = 0;

                for (int i = 0; i < course.getNumPoints(); i++)
                {
                    Point lastP;
                    Point p = course.getgraphicalPointAt(i);
                    Point nextP;
                    int lastAngle;
                    int angle = course.getAngleAt(i);
                    int nextAngle;
                    bool clockwise;
                    bool nextClockwise;
                    int clockwiseAngle;
                    int midArcAngle;

                    if (i == 0)
                    {
                        lastP = course.getgraphicalPointAt(course.getNumPoints() - 1);
                        nextP = course.getgraphicalPointAt(i + 1);
                        lastAngle = course.getAngleAt(course.getNumPoints() - 1);
                        nextAngle = course.getAngleAt(i + 1);
                        if (angle < 0)
                            startAngle = 180;
                        else
                            startAngle = 0;
                    }
                    else if (i == course.getNumPoints() - 1)
                    {
                        lastP = course.getgraphicalPointAt(i - 1);
                        nextP = course.getgraphicalPointAt(0);
                        lastAngle = course.getAngleAt(i - 1);
                        nextAngle = course.getAngleAt(0);
                    }
                    else
                    {
                        lastP = course.getgraphicalPointAt(i - 1);
                        nextP = course.getgraphicalPointAt(i + 1);
                        lastAngle = course.getAngleAt(i - 1);
                        nextAngle = course.getAngleAt(i + 1);
                    }

                    g.FillEllipse((course.getAngleAt(i) < 0) ? greenBrush : redBrush, p.X - PYLON_DIAMETER / 2, p.Y - PYLON_DIAMETER / 2, PYLON_DIAMETER, PYLON_DIAMETER);

                    if (angle < 0)
                    {
                        clockwise = true;
                    }
                    else
                    {
                        clockwise = false;
                    }

                    if (nextAngle < 0)
                    {
                        nextClockwise = true;
                    }
                    else
                    {
                        nextClockwise = false;
                    }

                    if (clockwise)
                    {
                        clockwiseAngle = -angle;
                        if (i != 0 && lastAngle > 0)
                            startAngle -= 180;
                    }
                    else
                    {
                        clockwiseAngle = -angle;
                        if(i != 0 && lastAngle < 0)
                            startAngle -= 180;
                    }


                    g.DrawArc(mypen, p.X - 15, p.Y - 15, 30, 30, startAngle, clockwiseAngle);

                    double clockwiseAngleRad = angle * Math.PI / 180;
                    double startAngleRad = startAngle * Math.PI / 180;
                    double nextAngleRad = nextAngle * Math.PI / 180;

                    int xOffset = (int)(Math.Cos(startAngleRad - clockwiseAngleRad) * 15.0);
                    int yOffset = (int)(Math.Sin(startAngleRad - clockwiseAngleRad) * 15.0);

                    
                    midArcAngle = startAngle - (int)(clockwiseAngle / 2.0);

                    startAngle += clockwiseAngle;

                    int nextStartAngle = startAngle;
                    if (nextClockwise)
                    {
                        if (angle > 0)
                            nextStartAngle -= 180;
                    }
                    else
                    {
                        if (angle < 0)
                            nextStartAngle -= 180;
                    }
                    double nextStartAngleRad = nextStartAngle * Math.PI / 180;

                    int nextXOffset = (int)(Math.Cos(nextStartAngleRad) * 15.0);
                    int nextYOffset = (int)(Math.Sin(nextStartAngleRad) * 15.0);

                    g.DrawLine(mypen, p.X + xOffset, p.Y + yOffset, nextP.X + nextXOffset, nextP.Y + nextYOffset);

                    double midArcAngleRad = midArcAngle * Math.PI / 180;
                    
                    double arrowRad = -25 * Math.PI / 180;
                    double finalMidArcRad = midArcAngleRad - clockwiseAngleRad;

                    Point[] trianglePoints = new Point[3];
                    if (clockwise)
                        arrowRad *= -1;

                    trianglePoints[0] = new Point(p.X + (int)(Math.Cos(finalMidArcRad + arrowRad) * 15.0), p.Y + (int)(Math.Sin(finalMidArcRad + arrowRad) * 15.0));
                    trianglePoints[1] = new Point(p.X + (int)(Math.Cos(finalMidArcRad) * 10.0), p.Y + (int)(Math.Sin(finalMidArcRad) * 10.0));
                    trianglePoints[2] = new Point(p.X + (int)(Math.Cos(finalMidArcRad) * 20.0), p.Y + (int)(Math.Sin(finalMidArcRad) * 20.0));
                    
                    g.FillPolygon(blackBrush, trianglePoints);
                    
                    //if (TheKnack.debug)
                    //    Console.WriteLine("StartAngle: " + startAngle + ", Angle: " + angle + ", Midarc: " + midArcAngle + ", TopArc: " + topOfArc.ToString());
                }
                Image truck = Image.FromFile("truck.gif");
                Point startPoint = course.getgraphicalPointAt(course.getNumPoints() - 1);
                startPoint.Y -= 50;
                if (course.getAngleAt(course.getNumPoints() - 1) < 0)
                    startPoint.X -= 30;

                g.DrawImage(truck, startPoint);
            }
            else if (course != null && !course.isCourseValid())
            {
                Brush brush = Brushes.Red;
                Font font = new Font("Arial", (float)12.0, FontStyle.Bold);
                gfx.DrawString("Course Data is Invalid", font, brush, 110, 75);
            }
            else
            {
                Brush brush = Brushes.Black;
                Font font = new Font("Arial", (float)12.0, FontStyle.Bold);
                gfx.DrawString("Not Implemented", font, brush, 110, 75);
            }
        }

        public void repaintGraphics()
        {
            this.OnPaint(null);
        }

        private bool allSidesAreEqual()
        {
            bool retval = true;
            int angle = course.getAngleAt(0);
            for (int i = 0; i < course.getNumPoints(); i++)
            {
                if (course.getAngleAt(i) != angle)
                {
                    retval = false;
                    break;
                }
            }
            return retval;
        }
    }

}
