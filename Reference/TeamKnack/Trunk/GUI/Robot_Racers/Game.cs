using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;


namespace Robot_Racers
{
    class Game : Form
    {
        private System.ComponentModel.IContainer components = null;
        private ProcessedImage processedImage = null;
        private Timer gameTimer = null;
        private int millisecondsPassed = 0;
        private bool isRunning = false;

        Font timeFont = new Font(new FontFamily("Arial"), 36f, FontStyle.Bold);
        Font scoreFont = new Font(new FontFamily("Arial"), 36f, FontStyle.Bold);
        Font countdownFont = new Font(new FontFamily("Arial"), 300f, FontStyle.Bold);
        SolidBrush whiteBrush = new SolidBrush(Color.White);
        SolidBrush greenBrush = new SolidBrush(Color.DarkSlateGray);
        SolidBrush pylonGreenBrush = new SolidBrush(Color.Green);
        SolidBrush pylonOrangeBrush = new SolidBrush(Color.Orange);
        long startTicks = 0;

        int pylonNum = 0;
        int score = 0;
        int lap = 1;
        bool doNotUpdate = false;
        Timer countdownTimer = new Timer();
        int countdown = 3;
        bool inCountdown = false;

        public Game()
        {
            
            this.components = new System.ComponentModel.Container();
            this.Owner = TheKnack.racer;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Width = Screen.PrimaryScreen.Bounds.Width;
            this.Height = Screen.PrimaryScreen.Bounds.Height;
            this.Left = (Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2);
            this.Top = (Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2);
            
            this.processedImage = new ProcessedImage(true);
            this.processedImage.BackColor = System.Drawing.Color.Black;
            this.processedImage.Location = new System.Drawing.Point(0, 0);
            this.processedImage.Name = "processedImage";
            this.processedImage.Size = new System.Drawing.Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Width);
            this.processedImage.TabIndex = 0;
            this.Controls.Add(processedImage);

            this.Show();

            isRunning = true;
            this.KeyUp += new KeyEventHandler(Game_KeyUp);

            this.gameTimer = new Timer();
            gameTimer.Tick += new EventHandler(gameTimer_Tick);
            gameTimer.Interval = 30;
        }

        public void setDoNotUpdate(bool dnu)
        {
            doNotUpdate = dnu;
        }

        public void startCountdown()
        {
            countdown = 3;
            inCountdown = true;
            processedImage.Invalidate();
            countdownTimer.Interval = 1000;
            countdownTimer.Tick += new EventHandler(countdownTimer_Tick);
            countdownTimer.Start();
        }

        private void startGame()
        {
            TheKnack.racer.startButton_Click(null, null);

            startTicks = DateTime.Now.Ticks;
            gameTimer.Start();
        }

        private void countdownTimer_Tick(object sender, EventArgs e)
        {
            countdown--;
            processedImage.Invalidate();
            if (countdown == 0)
            {
                startGame();
            }
            if (countdown <= -1)
            {
                inCountdown = false;
            }
            else
            {
                countdownTimer.Start();
            }
        }

        public bool getIsRunning()
        {
            return isRunning;
        }

        public ProcessedImage getProcessedImage()
        {
            return processedImage;
        }

        public void updateGameDisplay(Graphics g)
        {
            int tempMills = millisecondsPassed;
            
            int secs = tempMills / 1000;
            int mins = secs / 60;
            secs = secs % 60;
            int hundredths = (tempMills / 10) % 100;

            string hunStr = hundredths + "";
            if (hundredths < 10)
                hunStr = "0" + hundredths;
            string secStr = secs + "";
            if (secs < 10)
                secStr = "0" + secs;


            string time = mins + ":" + secStr + "." + hunStr;

            g.DrawString(time, timeFont, whiteBrush, Screen.PrimaryScreen.Bounds.Width - 200, 5);
            g.DrawString("Score: " + score, scoreFont, greenBrush, 5, 5);
            g.DrawString("Lap " + lap, scoreFont, greenBrush, Screen.PrimaryScreen.Bounds.Width / 3f, 5);

            int angle = RobotRacer.currentCourse.getAngleAt(pylonNum);
            if (angle < 0)
            {
                g.DrawString("Pylon " + (pylonNum + 1), scoreFont, pylonOrangeBrush, Screen.PrimaryScreen.Bounds.Width / 2f, 5);
                g.DrawString(-angle + "" + (char)176, scoreFont, pylonOrangeBrush, Screen.PrimaryScreen.Bounds.Width / 2f + 250, 5);
                Image right = Image.FromFile("rightArrow.gif");
                g.DrawImage(right, Screen.PrimaryScreen.Bounds.Width / 2f + 200, 5);
            }
            else
            {
                g.DrawString("Pylon " + (pylonNum + 1), scoreFont, pylonGreenBrush, Screen.PrimaryScreen.Bounds.Width / 2f, 5);
                g.DrawString(angle + "" + (char)176, scoreFont, pylonGreenBrush, Screen.PrimaryScreen.Bounds.Width / 2f + 250, 5);
                Image left = Image.FromFile("leftArrow.gif");
                g.DrawImage(left, Screen.PrimaryScreen.Bounds.Width / 2f + 200, 5);
            }

            g.DrawString(String.Format("{0:F2}",RobotRacer.stateVariables.Velocity) + " m/s", scoreFont, greenBrush, 5, 50);

            if (inCountdown)
            {
                string countdownStr = "  " + countdown + "  ";
                if (countdown == 0)
                    countdownStr = "GO!";
                g.DrawString(countdownStr, countdownFont, whiteBrush, Screen.PrimaryScreen.Bounds.Width / 2 - 400, Screen.PrimaryScreen.Bounds.Height / 2 - 250);
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e){
            millisecondsPassed = (int)((DateTime.Now.Ticks - startTicks) / 10000);
            if (!doNotUpdate)
            {
                processedImage.Invalidate();
            }
            if (RobotRacer.stateVariables.PylonNumber != pylonNum)
            {
                pylonNum = RobotRacer.stateVariables.PylonNumber;
                score += 100;

                if (pylonNum == 0)
                {
                    score += 200;
                    lap++;
                    if (lap == 4)
                    {
                        TheKnack.racer.stopButton_Click(null, null);
                        return;
                    }
                }
            }
            gameTimer.Start();
        }

        public void Game_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode.ToString().Equals("Escape") || e.KeyCode.ToString().Equals("Q"))
            {
                this.isRunning = false;
                this.Dispose();
            }
            else if (e.KeyCode.ToString().Equals("G"))
            {
                startCountdown();
            }           
        }        
    }
}
