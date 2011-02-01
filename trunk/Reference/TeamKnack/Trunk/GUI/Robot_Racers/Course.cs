using System;
using System.Collections.Generic;
using System.Drawing;

namespace Robot_Racers
{

    public class Course
    {
        List<int> courseAngles;
        List<int> courseAngleTrims;
        List<int> courseDistances;
        List<Point> graphicalPoints;
        int initialAngle = 0;
        int unitLength = 3;

        public Course()
        {
            courseAngles = new List<int>();
            courseAngleTrims = new List<int>();
            courseDistances = new List<int>();
            graphicalPoints = new List<Point>();
            initialAngle = 0;
        }

        public Course(string courseString)
        {
            courseAngles = new List<int>();
            courseAngleTrims = new List<int>();
            courseDistances = new List<int>();
            graphicalPoints = new List<Point>();

            char[] splitChars = new char[] { '\n' };
            char[] space = new char[] { ' ' };
            courseString = courseString.Trim();
            string[] stringAngles = courseString.Split(splitChars);
            try
            {
                string[] calibrationData = stringAngles[0].Split(space);
                initialAngle = (int)int.Parse(calibrationData[0].Trim());
                if (calibrationData.Length > 1)
                {
                    unitLength = int.Parse(calibrationData[1].Trim());
                }
                for (int i = 1; i < stringAngles.Length; i++)
                {
                    string[] pylonData = stringAngles[i].Split(space);
                    string tempAngle = pylonData[0].Trim();
                    int distanceInt = 1;
                    int angleTrim = 0;
                    if (pylonData.Length > 1)
                    {
                        string tempDistance = pylonData[1].Trim();
                        if (tempDistance[0] == '+')
                        {
                            tempDistance = tempDistance.TrimStart('+');
                            angleTrim = int.Parse(tempDistance);
                            if (pylonData.Length > 2)
                            {
                                tempDistance = pylonData[2].Trim();
                                distanceInt = int.Parse(tempDistance);
                            }
                        }
                        else
                        {
                            distanceInt = int.Parse(tempDistance);
                        }
                    }
                    int angleInt = (int)int.Parse(tempAngle);

                    courseAngles.Add(angleInt);
                    courseAngleTrims.Add(angleTrim);
                    courseDistances.Add(distanceInt);
                }
                createGraphicalPoints();
            }
            catch (FormatException e) { Console.WriteLine(e.Message); }
        }

        public int getAngleAt(int index)
        {
            if (index >= courseAngles.Count) return 0;
            return courseAngles[index];
        }

        public int getDistanceAt(int index)
        {
            return courseDistances[index];
        }

        public int getTrimAt(int index)
        {
            return courseAngleTrims[index];
        }

        public Point getgraphicalPointAt(int index)
        {
            return graphicalPoints[index];
        }

        public int getNumPoints()
        {
            return courseAngles.Count;
        }

        public int getInitialAngle()
        {
            return initialAngle;
        }

        public int getUnitLength()
        {
            return unitLength;
        }

        private void createGraphicalPoints()
        {
            if (isCourseValid())
            {
                if (courseAngles.Count == 2)
                {
                    int first = courseAngles[0];
                    int second = courseAngles[1];

                    graphicalPoints.Add(new Point(0, 0));
                    graphicalPoints.Add(new Point(0, 100));
                }
                else if (courseAngles.Count > 2)
                {

                    Point firstPoint = new Point(0, 0);
                    Point lastPoint = firstPoint;

                    graphicalPoints.Add(firstPoint);

                    int startAngle = 0;

                    for (int i = 0; i < courseAngles.Count - 1; i++)
                    {
                        Point nextPoint = new Point();
                        int insideAngle = 180 - Math.Abs(startAngle + courseAngles[i]);
                        double insideAngleRad = insideAngle * Math.PI / 180;

                        if (courseAngles[i] < 0)
                        {
                            nextPoint.X = lastPoint.X + (int)(Math.Sin(insideAngleRad) * 100);
                            nextPoint.Y = lastPoint.Y + (int)(Math.Cos(insideAngleRad) * 100);
                        }
                        else
                        {
                            nextPoint.X = lastPoint.X - (int)(Math.Sin(insideAngleRad) * 100);
                            nextPoint.Y = lastPoint.Y + (int)(Math.Cos(insideAngleRad) * 100);
                        }
                        graphicalPoints.Add(nextPoint);

                        startAngle += courseAngles[i];
                        lastPoint = nextPoint;
                    }

                    int xDifference = lastPoint.X;
                    int yDifference = lastPoint.Y - 100;

                    Console.WriteLine(xDifference);
                    if (xDifference > 0) //lengthen last segment
                    {
                        lastPoint.X = lastPoint.X - xDifference;
                        lastPoint.Y = lastPoint.Y - (int)(Math.Sin(degToRad(courseAngles[courseAngles.Count - 2]) * xDifference));
                        graphicalPoints[graphicalPoints.Count - 1] = lastPoint;
                    }
                    else // shorten last segment
                    {
                        lastPoint.X = lastPoint.X - xDifference;
                        lastPoint.Y = lastPoint.Y - (int)(Math.Sin(degToRad(courseAngles[courseAngles.Count - 2]) * xDifference));
                        graphicalPoints[graphicalPoints.Count - 1] = lastPoint;
                    }
                }
                adjustGraphicalSize();
                transposeGraphic();
            }
        }

        private double degToRad(int degree)
        {
            return degree * Math.PI / 180;
        }

        private void adjustGraphicalSize()
        {
            int xMax = 0;
            int xMin = 0;
            int yMax = 0;
            int yMin = 0;

            foreach (Point p in graphicalPoints)
            {
                if (p.X > xMax)
                    xMax = p.X;
                else if (p.X < xMin)
                    xMin = p.X;
                if (p.Y > yMax)
                    yMax = p.Y;
                else if (p.Y < yMin)
                    yMin = p.Y;
            }
            int width = xMax - xMin;
            int height = yMax - yMin;

            double heightMultiplier = (double)(CourseGraphicsPanel.PANE_HEIGHT - CourseGraphicsPanel.BUFFER) / (double)height;
            double widthMultiplier = (double)(CourseGraphicsPanel.PANE_WIDTH - CourseGraphicsPanel.BUFFER) / (double)width;
            double multiplier = 0;

            if (heightMultiplier < widthMultiplier)
                multiplier = heightMultiplier;
            else
                multiplier = widthMultiplier;


            for (int i = 0; i < graphicalPoints.Count; i++)
            {
                Point p = graphicalPoints[i];
                p.X = (int)((double)p.X * multiplier);
                p.Y = (int)((double)p.Y * multiplier);
                graphicalPoints[i] = p;
            }
        }

        private void transposeGraphic()
        {
            int xMax = 0;
            int xMin = 0;
            int yMax = 0;
            int yMin = 0;

            foreach (Point p in graphicalPoints)
            {
                if (p.X > xMax)
                    xMax = p.X;
                else if (p.X < xMin)
                    xMin = p.X;
                if (p.Y > yMax)
                    yMax = p.Y;
                else if (p.Y < yMin)
                    yMin = p.Y;
            }
            int width = xMax - xMin;
            int height = yMax - yMin;

            int xAdder = (CourseGraphicsPanel.PANE_WIDTH / 2) - (xMin + (width / 2));
            int yAdder = (CourseGraphicsPanel.PANE_HEIGHT / 2) - (yMin + (height / 2));

            for (int i = 0; i < graphicalPoints.Count; i++)
            {
                Point p = graphicalPoints[i];
                p.X += xAdder;
                p.Y += yAdder;
                graphicalPoints[i] = p;
            }
        }

        public bool isCourseValid()
        {
            bool retval = false;
            if (courseAngles.Count >= 2)
            {
                int sum = 0;
                bool greaterThan180 = false;
                foreach (int angle in courseAngles)
                {
                    sum += angle;
                    if (Math.Abs(angle) >= 180)
                        greaterThan180 = true;
                }
                if (sum % 360 == 0)
                {
                    if (sum == 0 && courseAngles.Count == 3)
                    {
                        if (greaterThan180)
                            retval = true;
                    }
                    else retval = true;
                }
            }
            return retval;
        }

        public int getDegreesOff()
        {
            int sum = 0;
            foreach (int angle in courseAngles)
            {
                sum += angle;
            }
            return sum % 360;
        }

        public int getTotalDegrees()
        {
            int sum = 0;
            foreach (int angle in courseAngles)
            {
                sum += angle;
            }
            return sum;
        }
    }
}
