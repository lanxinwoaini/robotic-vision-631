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

    

    public enum PylonColor : byte
    {
        Orange = 1,
        Green = 0
    }

    class ProcessedImage : Panel
    {
        //bool is640 = true;
        bool fullScreen = false;
        const double X_METERS = 3.0;
        const double Y_METERS = 3.0;
        bool changed = true;

        List<Pylon> pylons;

        Pen whitePen = new Pen(Color.White, 1);
        int X_CENTER = 160;
        int HORIZON = 125;
        int BOTTOM = 240;
        int bottomLine;
        int topXInc = 5;
        float bottomXInc = 100f;
        int numVert = 15;
        double yInc = 0.68;

        int initialBlockHeight = 40;

        float widthRatio = 0f;
        float heightRatio = 0f;

        SolidBrush orange = new SolidBrush(Color.Orange);
        SolidBrush green = new SolidBrush(Color.Green);
        SolidBrush blue = new SolidBrush(Color.Blue);
        SolidBrush fillColor;
        SolidBrush whiteBrush = new SolidBrush(Color.White);
        SolidBrush blackBrush = new SolidBrush(Color.Black);
        Font backFont = new Font(new FontFamily("Arial"), 9f, FontStyle.Bold);
        Font foreFont = new Font(new FontFamily("Arial"), 9f);

        public ProcessedImage(bool fullScreen)
        {
            if (fullScreen)
            {
                X_CENTER = Screen.PrimaryScreen.Bounds.Width / 2;
                HORIZON = (int)(Screen.PrimaryScreen.Bounds.Height * (125f / 240f));
                BOTTOM = Screen.PrimaryScreen.Bounds.Height;
                initialBlockHeight = (int)(Screen.PrimaryScreen.Bounds.Height / 6);
                this.fullScreen = true;
                bottomXInc = (int)(Screen.PrimaryScreen.Bounds.Width * (10f/32f));
                topXInc = (int)(Screen.PrimaryScreen.Bounds.Width * (5f / 320f));
                numVert = 20;
                yInc = 0.66;
                widthRatio = Screen.PrimaryScreen.Bounds.Width / 640f;
                heightRatio = Screen.PrimaryScreen.Bounds.Height / 480f;
            }
         
            bottomLine = BOTTOM - initialBlockHeight;
            pylons = new List<Pylon>();
            this.DoubleBuffered = true;
           // pylons.Add(new Pylon(40, 4, 75, 120, PylonColor.Green, 25));
           // pylons.Add(new Pylon(95, 10, 280, 123, PylonColor.Orange, 56));
        }

        public void addPylon(Pylon p)
        {
            pylons.Add(p);
        }

        public void clearPylons()
        {
            pylons.Clear();
        }

        public void redrawImage()
        {

            this.Invalidate();
        }

        protected override void OnPaint(PaintEventArgs paintEvnt)
        {
            Graphics g = paintEvnt.Graphics;

            drawLines(g);
            drawPylons(g);
            if (fullScreen)
            {
                TheKnack.game.updateGameDisplay(g);
            }
        }

        public Pylon getPylonAt(int index)
        {
            return pylons.ElementAt(index);
        }

        private void drawLines(Graphics g)
        {
            int botHeight = BOTTOM - bottomLine;

            bottomXInc = 100f;
            if(fullScreen)
                bottomXInc = (int)(Screen.PrimaryScreen.Bounds.Width * (10f / 32f));
            for (int i = 0; i < numVert; i++)
            {
                g.DrawLine(whitePen, X_CENTER - i * topXInc, HORIZON, X_CENTER - i * bottomXInc, BOTTOM);
                g.DrawLine(whitePen, X_CENTER + i * topXInc, HORIZON, X_CENTER + i * bottomXInc, BOTTOM);
                bottomXInc += 10f;
            }

            double nextLine = BOTTOM - initialBlockHeight;
            double drawLine = BOTTOM;
            double lineDiff = drawLine - nextLine;

            while (lineDiff >= 1 && drawLine >= HORIZON)
            {
                g.DrawLine(whitePen, 0, (int)drawLine, X_CENTER * 2, (int)drawLine);
                double lastLine = drawLine;
                drawLine = nextLine;
                nextLine = drawLine - (lastLine - drawLine) * yInc;
                lineDiff = drawLine - nextLine;
            }
            g.DrawLine(whitePen, 0, HORIZON, X_CENTER * 2, HORIZON);
            g.DrawLine(whitePen, 0, HORIZON+1, X_CENTER * 2, HORIZON+1);
        }

        private void drawPylons(Graphics g)
        {
            
            for (int i = 0; i < pylons.Count; i++)
            {
                Pylon p = pylons[i];
                if (p.Color == (byte)PylonColor.Orange)
                    fillColor = orange;
                else if (p.Color == (byte)PylonColor.Green)
                    fillColor = green;
                else fillColor = blue;

                if(!fullScreen){
                    p.Center /= 2;
                    p.Middle /= 2;
                    p.Height /= 2;
                    p.Width  /= 2;
                }
                else 
                {
                    if (changed)
                    {
                        p.Center = (ushort)(p.Center * widthRatio);
                        p.Width = (ushort)(p.Width * widthRatio);
                        p.Height = (ushort)(p.Height * heightRatio);
                        p.Middle = (ushort)(p.Middle * heightRatio);
                        if(i >= pylons.Count - 1)
                            changed = false;
                    }
                    
                }

                int top = p.Middle - (p.Height / 2);
                int left = p.Center - (p.Width / 2);

                g.FillRectangle(fillColor, left, top, p.Width, p.Height);

                g.DrawString(p.Probability + "", backFont, blackBrush, p.Center - 8, top - 13);
                g.DrawString(p.Probability + "", foreFont, whiteBrush, p.Center - 8, top - 13);
            }
        }
        public void setPylonsChanged()
        {
            changed = true;
        }
    }

    public class Pylon
    {
        private ushort height;
        private ushort width;
        private ushort center;
        private ushort middle;
        private byte color;
        private byte probability;

        public Pylon(ushort height, ushort width, ushort center, ushort middle, PylonColor color, byte probability)
        {
            this.height = height;
            this.width = width;
            this.center = center;
            this.middle = middle;
            this.color = (byte)color;
            this.probability = probability;
        }

        public ushort Center
        {
            get { return center; }
            set { center = value; }
        }

        public ushort Height
        {
            get { return height; }
            set { height = value; }
        }

        public ushort Width
        {
            get { return width; }
            set { width = value; }
        }

        public ushort Middle
        {
            get { return middle; }
            set { middle = value; }
        }

        public byte Color
        {
            get { return color; }
            set { color = value; }
        }

        public byte Probability
        {
            get { return probability; }
            set { probability = value; }
        }

        public string toString(int pylonNum){
            string s = "\nPylon " + pylonNum;
            s += "\t\nHeight: " + height;
            s += "\t\nWidth: " + width;
            s += "\t\nCenter: " + center;
            s += "\t\nMiddle: " + middle;
            s += "\t\nColor: " + ((color == (byte)PylonColor.Green)? "Green" : (color == (byte)PylonColor.Orange) ? "Orange" : "Blue");
            s += "\t\nProbability: " + probability;
            return s;
        }
    }
}
