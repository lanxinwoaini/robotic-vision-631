using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RoboSim
{
    public partial class frmMain : Form
    {
        private Bitmap mapImage;
        private bool boolSettingTheta = false;

        // Control
        private Control control = null;

        // Course
        private List<Pylon> pylons = new List<Pylon>();             // the course
        private List<Regions> visiblePylons = new List<Regions>();  // visible pylons
        
        // Car state
        private PointF carPosition;      // position in (inches, inches)  // PointF defined in System.Drawing
        private double carHeading;       // heading in degrees

        // Sensor readings
        public double distance;          // encoder distance 

        // Pylon parameters
        public const double PYLON_WIDTH  = 2.5;        // inches +/- 0.25
        public const double PYLON_HEIGHT = 28.5;       // inches +/- 0.5

        // Car parameters
        public const double WHEEL_BASE = 11.6875;      // inches +/- 0.125
        public const double CAM_HEIGHT = 11.25;        // inches +/- 0.25
        public const double MAX_STEERING_ANGLE = 30.0; // degrees

        // Camera parameters
        public const double MAX_VIEWING_DISTANCE = 1000;   // inches

        // Simulaton parameters
        private const double DELTA_T = 0.1;             // seconds
        private const double V_RESPONSE = 0.9;          // velocity response
        private const double S_RESPONSE = 0.9;          // steering response
        // RESPONSE = 1 - (1 - exp(-at))/at ~= at/2 - (at)^2/6 + (at)^3/24 - ...

        // Course parameters
        public const double THETA_ERROR = 10;          // degrees
        public const double MIN_PYLON_DISTANCE = 60;   // inches
        public const double MAX_PYLON_DISTANCE = 120;  // inches

        public frmMain()
        {
            InitializeComponent();
            mapImage = new Bitmap(picPathPlan.Width, picPathPlan.Height);

            // car state
            carPosition = new PointF(0, 0);     // position
            carHeading = 0.0;             // heading              // camera  

            // Sensor Readings
            distance = 0.0;

        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            picPathPlan.BackgroundImage = mapImage;
            updateDisplay();
        }

        // mouse click procedure to place pylons and car in map
        private void picPathPlan_MouseClick(object sender, MouseEventArgs e)
        {
                 // Place the waypoint on the map
                if (btnPlaceCar.Checked && !boolSettingTheta)
                {
                    boolSettingTheta = true;
                    if (control == null)
                    {
                        control = new Control(pylons);           // and its controller
                    }
                    carPosition = new PointF(e.Location.X, mapImage.Height - e.Location.Y);
                    lblStatus.Text = "Click which direction the car is heading";
                }
                else if (btnPlaceCar.Checked && boolSettingTheta)
                {
                    boolSettingTheta = false;
                    carHeading = AngleToWaypoint(carPosition, new PointF(e.Location.X, mapImage.Height - e.Location.Y));
                    btnPlaceCar.Checked = false;
                    lblStatus.Text = "Car direction set to " + carHeading;
                    // setup course
                    genCourseDesc();
                }
                else if (e.Button == MouseButtons.Left)
                {
                    Pylon p = new Pylon(new PointF(e.Location.X, mapImage.Height - e.Location.Y), PylonType.LEFT);
                    pylons.Add(p);
                    lblStatus.Text = "Added left pylon at (" + e.Location.X +
                        "," + (mapImage.Height - e.Location.Y) + ")";
                    control = null;
                    tmrRace.Enabled = false;
                    btnPlaceCar.Checked = false;
                    btnRun.Text = "&Run the car";
                }
                else if (e.Button == MouseButtons.Right)
                {
                    Pylon p = new Pylon(new PointF(e.Location.X, mapImage.Height - e.Location.Y), PylonType.RIGHT);
                    pylons.Add(p);
                    lblStatus.Text = "Added right pylon at (" + e.Location.X +
                        "," + (mapImage.Height - e.Location.Y) + ")";
                    control = null;
                    tmrRace.Enabled = false;
                    btnPlaceCar.Checked = false;
                    btnRun.Text = "&Run the car";
                }
            updateDisplay();
        }

        // Step the simulation
        // speed is in inches/sec
        // steerAngle is in degrees, positive = left, negative = right
        public void advanceClock(double speed, double steerAngle)
        {
            // speed and distance traveled as if DELTA_T sec has passed
            double newSpeed = control.carSpeed + V_RESPONSE * (speed - control.carSpeed);
            double aveSpeed = (control.carSpeed + newSpeed)/2.0;
            distance = DELTA_T * aveSpeed;                   // inches

            // change of heading
            double newSteer = control.steerAngle + S_RESPONSE * (steerAngle - control.steerAngle); // degrees
            double aveSteer = (control.steerAngle + newSteer) / 2.0 * Math.PI / 180.0;  // radians
            double curvature = Math.Sin(aveSteer) / WHEEL_BASE;                 // radians per inch
            double deltaTheta = curvature * distance;                                  // change of heading in radians
            double aveTheta = carHeading * Math.PI / 180.0 + deltaTheta / 2.0;    // average heading in radians
            deltaTheta = deltaTheta * 180.0 / Math.PI;                          // change of heading in degrees

            // Advance the truck position
            carPosition.X = (float)(carPosition.X + Math.Cos(aveTheta) * distance);
            carPosition.Y = (float)(carPosition.Y + Math.Sin(aveTheta) * distance);

            // Update heading, velocity, and wheel angle
            carHeading = carHeading + deltaTheta;
            carHeading = (carHeading > 180.0) ? carHeading - 360.0 : (carHeading <= -180.0) ? carHeading + 360.0 : carHeading;
            control.carSpeed = newSpeed;
            control.steerAngle = newSteer;
        }

        // computes the area visible by the car cam
        // called by updateDisplay
        public PointF[] CamaraArea()
        {
            double carAngle = carHeading / 180.0 * Math.PI;       // heading in radians
            double halfViewAngle = (float)(Control.ANGLE_OF_VIEW / 360 * Math.PI);
            double hypotonus = MAX_VIEWING_DISTANCE / Math.Cos(halfViewAngle);
            PointF UpperLeft = new PointF(
                (float)(carPosition.X + Math.Cos(carAngle + halfViewAngle) * hypotonus),
                (float)(carPosition.Y + Math.Sin(carAngle + halfViewAngle) * hypotonus));
            PointF LowerLeft = new PointF(
                (float)(carPosition.X + Math.Cos(carAngle - halfViewAngle) * hypotonus),
                (float)(carPosition.Y + Math.Sin(carAngle - halfViewAngle) * hypotonus));

            return new PointF[] { carPosition, UpperLeft, LowerLeft };  // in actual coord, not image coord
        }

        public static bool PointInsideShape(PointF[] shapeVertices, PointF pp)
        {
            PointF pa = shapeVertices[0];
            PointF pb = shapeVertices[1];
            PointF pc = shapeVertices[2];

            // Compute vectors   
            Vector a = new Vector(new double[] { pa.X, pa.Y });
            Vector b = new Vector(new double[] { pb.X, pb.Y });
            Vector c = new Vector(new double[] { pc.X, pc.Y });
            Vector p = new Vector(new double[] { pp.X, pp.Y });

            Vector v0 = c - a;
            Vector v1 = b - a;
            Vector v2 = p - a;

            double dot00 = v0 * v0;
            double dot01 = v0 * v1;
            double dot02 = v0 * v2;
            double dot11 = v1 * v1;
            double dot12 = v1 * v2;


            // Compute barycentric coordinates
            double invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
            double u = (dot11 * dot02 - dot01 * dot12) * invDenom;
            double v = (dot00 * dot12 - dot01 * dot02) * invDenom;

            // Check if point is in triangle
            return (u > 0) && (v > 0) && (u + v < 1);
        }

        // find and draw the visible pylons
        public Image getCarView(PointF[] camArea)
        {
            Bitmap retValue = new Bitmap(picCamera.Width, picCamera.Height);
            Bitmap debugImage = new Bitmap(retValue);
            Graphics g = Graphics.FromImage(retValue);
            Graphics gDebug = Graphics.FromImage(debugImage);
            g.FillRectangle(Brushes.White, 0, 0, picCamera.Width, picCamera.Height);

            // find visible pylons and make regions for them
            visiblePylons.Clear();      // start afresh
            foreach (Pylon pylon in pylons)
            {
                if (PointInsideShape(camArea, pylon.Coordinates))
                {
                    // Get distance from the car to the pylon
                    double distance = DistanceBetweenPoints(carPosition, pylon.Coordinates);

                    // Get the angle from the car to the pylon
                    // angle from car to pylon
                    double pylonAngle = Math.Atan2(pylon.Coordinates.Y - carPosition.Y, pylon.Coordinates.X - carPosition.X) * 180 / Math.PI;
                    double angle = pylonAngle - carHeading; // compute angle relative to heading of car
                    angle = (angle > 180.0) ? angle - 360.0 : (angle <= -180.0) ? angle + 360.0 : angle;    // normalize

                    // Add pylon to the list of regions
                    visiblePylons.Add(new Regions(distance, angle, pylon.Type));
                }
            }

            visiblePylons.Sort();   //-this gives closest pylon in position [0]
            gDebug.DrawString("Car Angle: " + carHeading.ToString("0.0000"), new Font("Arial", 6.5f), Brushes.Black, 0, 0);
            gDebug.DrawLine(Pens.Black, 0, 12, 100, 12);

            int debugHeight = 15;
            foreach (Regions oOI in visiblePylons)
            {
                // Add debug text to camera view image
                gDebug.DrawString( (oOI.Type == PylonType.LEFT ? "LEFT" : "RIGHT")
                    + "   Angle: " + oOI.Angle.ToString("000.00") + "   Distance: " + oOI.Distance.ToString("000.00"),
                    new Font("Arial", 6.5f),
                    Brushes.Black, 0, debugHeight);
                debugHeight += 12;

                RectangleF pylon = new RectangleF();
                double tanOmega2 = 2.0 * Math.Tan(Control.ANGLE_OF_VIEW * Math.PI / 360.0);   // 2 * Tan(angle/2)

                // (X,Y) is top,left point of rectangle of Height(in Y direction) and Width (in X direction)
                pylon.Height = (float)((double)picCamera.Height * PYLON_HEIGHT / (tanOmega2 * oOI.Distance));
                pylon.Width  = (float)((double)picCamera.Width  * PYLON_WIDTH  / (tanOmega2 * oOI.Distance));
                pylon.Y = (float)((0.5 - (PYLON_HEIGHT - CAM_HEIGHT) / (tanOmega2 * oOI.Distance)) * (double)picCamera.Height);
                pylon.X = (float)((0.5 - Math.Tan(oOI.Angle * Math.PI / 180.0) / tanOmega2) * (double)picCamera.Width);
                pylon.X = pylon.X - (pylon.Width / 2.0f);

                // draw rectangle on panal image
                g.FillRectangle( Control.pylonColor(oOI.Type), pylon);

                // compute pixel dimensions of object in camera view
                oOI.Center = (int)(0.5 + (0.5 - Math.Tan(oOI.Angle * Math.PI / 180.0) / tanOmega2) * (double)Control.cameraWidth);
                oOI.Height = (int)(0.5 + (double)Control.cameraHeight * PYLON_HEIGHT / (tanOmega2 * oOI.Distance));
                oOI.Width  = (int)(0.5 + (double)Control.cameraWidth  * PYLON_WIDTH  / (tanOmega2 * oOI.Distance));
                int Y   = (int)(0.5+ (0.5 - (PYLON_HEIGHT - CAM_HEIGHT) / (tanOmega2 * oOI.Distance)) * (double)Control.cameraHeight);
                if (Y < 0 || (Y + oOI.Height) >= Control.cameraHeight) oOI.Height = -1;  // Height is obcurred by frame
                if ((oOI.Center - oOI.Width / 2) < 0 || (oOI.Center + oOI.Width / 2) >= Control.cameraWidth) // Width is obcurred
                {
                    //oOI.Center = -1;
                    //oOI.Width = -1;
                }
                gDebug.DrawString(
                    "Center: " + oOI.Center.ToString() + 
                    "   Width: "  + oOI.Width.ToString() + 
                    "   Height: " + oOI.Height.ToString(),
                    new Font("Arial", 6.5f),
                    Brushes.Black, 0, debugHeight);
                debugHeight += 15;
                gDebug.DrawLine(Pens.Black, 0, debugHeight - 3, 100, debugHeight - 3);
            }
            g.DrawImage(debugImage, 0, 0);
            return retValue;
        }

        private PointF carMapCoord
        {
            get { return new PointF(carPosition.X, picPathPlan.Height - carPosition.Y); }
        }

        private PointF mapCoord(PointF coord)
        {
            return new PointF(coord.X, picPathPlan.Height - coord.Y);
        }

        // draw the map
        private void updateDisplay()
        {
            System.Drawing.Graphics g = Graphics.FromImage(mapImage);

            // Draw the Axies
            g.FillRectangle(Brushes.White, 0, 0, mapImage.Width, mapImage.Height);
            g.DrawLine(Pens.Black, mapImage.Width / 2.0f, 0, mapImage.Width / 2.0f, mapImage.Height);
            g.DrawLine(Pens.Black, 0, mapImage.Height / 2.0f, mapImage.Width, mapImage.Height / 2.0f);

            // Draw the pylons
            // The pylons are drawn about 4 times larger than they really are
            if (pylons != null)
            {
                uint pylonIndex = 1;
                foreach (Pylon curPylon in pylons)
                {
                    g.FillEllipse(Control.pylonColor(curPylon.Type),
                        curPylon.Coordinates.X - 5, mapImage.Height - curPylon.Coordinates.Y - 5, 10, 10);
                    g.DrawString(pylonIndex.ToString(), new Font("Arial", 9.5f),
                        curPylon.Type == PylonType.LEFT ?
                        Brushes.Black : Brushes.Black, mapCoord(curPylon.Coordinates));
                    ++pylonIndex;
                }
            }

            // Draw the car
            if (control != null)
            {
                System.Drawing.Drawing2D.Matrix rotMatrix = new System.Drawing.Drawing2D.Matrix();
                rotMatrix.RotateAt((float)(-carHeading), carMapCoord);
                g.Transform = rotMatrix;
                PointF carCorner = new PointF(carMapCoord.X, carMapCoord.Y);
                carCorner.X -= ilstIcons.Images[0].Width / 2;
                carCorner.Y -= ilstIcons.Images[0].Height / 2;
                try
                {
                    g.DrawImage(ilstIcons.Images[0], carCorner);
                }
                catch (OverflowException) { }
                rotMatrix.RotateAt((float)(carHeading), carMapCoord);
                g.Transform = rotMatrix;

                // Draw the visible area
                PointF[] CamArea = CamaraArea();      // updates truck.camArea
                PointF p1 = new PointF(CamArea[1].X, picPathPlan.Height - CamArea[1].Y);
                PointF p2 = new PointF(CamArea[2].X, picPathPlan.Height - CamArea[2].Y);
                g.DrawLine(Pens.Brown, carMapCoord, p1);
                g.DrawLine(Pens.Brown, carMapCoord, p2);

                // truck.getCarView (below) must be called after truck.CamaraArea() (above).
                // Draw pylons in camera view
                picCamera.BackgroundImage = getCarView(CamArea);
            }

            picPathPlan.Update();
            picPathPlan.Refresh();
        }

        private static double RadAngleBetweenPoints(PointF point1, PointF point2)
        {
            double retValue = Math.Atan2(point1.Y - point2.Y, point1.X - point2.X);
            retValue = (retValue > Math.PI) ? retValue - 2 * Math.PI : (retValue <= -Math.PI) ? retValue + 2 * Math.PI : retValue;
            return retValue;
        }

        private void btnCalcWaypoints_Click(object sender, EventArgs e)
        { }

        private void btnClearPylons_Click(object sender, EventArgs e)
        {
            pylons.Clear();
            control = null;
            tmrRace.Enabled = false;
            btnPlaceCar.Checked = false; 
            btnRun.Text = "&Run the car";
            updateDisplay();
            txtDebug.Text = "";
            txtCourseDescription.Text = "";
            lblStatus.Text = "Cleared Pylons, input a new course";
        }

        private void btnCarPlace_Click(object sender, EventArgs e)
        {
            if (boolSettingTheta)
            {
                lblStatus.Text = "Car Theta set to default of 0";
                carHeading = 0;
            }
            else if (btnPlaceCar.Checked == false)
            {
                lblStatus.Text = "Placement of car canceled";
            }
            else
            {
                lblStatus.Text = "Pick location to start car";
            }
            txtDebug.Text = (btnPlaceCar.Checked ? "Stop" : "Start") + " Placement of Car\r\n" + txtDebug.Text;
            boolSettingTheta = false;
        }

        private static double AngleToWaypoint(PointF pylon, PointF waypoint)
        {
            double retValue = Math.Atan2(waypoint.Y - pylon.Y, waypoint.X - pylon.X) / Math.PI * 180.0;
            retValue = (retValue > 180.0) ? retValue - 360.0 : (retValue <= -180.0) ? retValue + 360.0 : retValue;

            return retValue;
        }

        private static double DistanceBetweenPoints(PointF point1, PointF point2)
        {
            double retValue = Math.Sqrt((point1.X - point2.X) * (point1.X - point2.X) +
                (point1.Y - point2.Y) * (point1.Y - point2.Y));
            return retValue;
        }

        private void genCourseDesc()
        {
            txtCourseDescription.Text = "";

            for (int pylonIndex = 0; pylonIndex < pylons.Count; ++pylonIndex)
            {
                double AngleToNextPylon = AngleToWaypoint(pylons[pylonIndex].Coordinates,
                    pylons[(pylonIndex + 1) % pylons.Count].Coordinates);
                double AngleToPreviousPylon = AngleToWaypoint(pylons[pylonIndex - 1 < 0 ? 
                    pylons.Count - 1 : pylonIndex - 1].Coordinates,
                    pylons[pylonIndex].Coordinates);
                double PylonAngle = (-AngleToPreviousPylon + AngleToNextPylon) % 360;

                // make Left Pylons negative and Right Pylons positive
                if (pylons[pylonIndex].Type == PylonType.RIGHT && PylonAngle < 0)
                {
                    PylonAngle = (PylonAngle + 360) % 360;
                }
                else if (pylons[pylonIndex].Type == PylonType.LEFT && PylonAngle > 0)
                {
                    PylonAngle = (PylonAngle - 360) % 360;
                }

                pylons[pylonIndex].AngleToNextPylon = PylonAngle;
                pylons[pylonIndex].DistanceToNextPylon = DistanceBetweenPoints(pylons[pylonIndex].Coordinates,
                    pylons[(pylonIndex + 1) % pylons.Count].Coordinates);

                // write a line for pylon
                txtCourseDescription.Text += pylonIndex.ToString() + " " + PylonAngle.ToString("0.00") + "\r\n";

            }
            //ADDED - set the course data inside the control function
            control.updateAngles(pylons);
        }


        // timer Race Tick
        // Timer is set for 0.1 Seconds
        private void tmrRace_Tick(object sender, EventArgs e)
        {
            // Simulate a time step, update camera view
            advanceClock(control.carSpeed, control.steerAngle);
            // Compute Control
            control.computeControl(distance, visiblePylons);
            // Post Status
            txtCurrentPylon.Text = control.getCurrentPylon().ToString();
            txtState.Text = control.getCurrentState();
            txtDistance.Text = control.getCurrentDistance().ToString("0.0000");
            txtTheta.Text = control.getCurrentTheta().ToString("0.0000");
            txtDebug.Text = "" + txtDebug.Text + control.getDebugOutput();
            // Finally, update the display
            updateDisplay();    // updates camera view region
        }

        private void btnRun_Click(object sender, EventArgs e)
        {
            tmrRace.Enabled = !tmrRace.Enabled;
            btnRun.Text = tmrRace.Enabled ? "Halt the ca&r" : "&Run the car";
            lblStatus.Text = "Truck is " + (tmrRace.Enabled ? "running." : "stopped.");
            if (tmrRace.Enabled && pylons.Count == 0)
            {
                tmrRace.Enabled = false;
                btnPlaceCar.Checked = false;
                btnRun.Text = "&Run the car";
                lblStatus.Text = "First, you must create a course.";
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            control.StateReset();
        }

        private void txtDebug_TextChanged(object sender, EventArgs e)
        {
           
        }

        private void txtCurrentPylon_TextChanged(object sender, EventArgs e)
        {

        }

        private void lblCurrentPylon_Click(object sender, EventArgs e)
        {

        }


    }
}
