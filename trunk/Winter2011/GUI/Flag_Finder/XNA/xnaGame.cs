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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Robot_Racers.XNAGame
{
    class xnaGame : Form
    {
        private System.ComponentModel.IContainer components = null;
        private xnaRoboRacerDisplay xnaDisplay = null;
        private Timer gameTimer = null;
        private int millisecondsPassed = 0;
        private bool isRunning = false;

        long startTicks = 0;

        int pylonNum = 0;
        int score = 0;
        int lap = 1;
        Timer countdownTimer = new Timer();
        int countdown = 3;
        bool inCountdown = false;

        public xnaGame()
        {
            
            this.components = new System.ComponentModel.Container();
            this.Owner = TheKnack.racer;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Width = Screen.PrimaryScreen.Bounds.Width;
            this.Height = Screen.PrimaryScreen.Bounds.Height;
            this.Left = (Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2);
            this.Top = (Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2);

            isRunning = true;
            this.KeyUp += new KeyEventHandler(Game_KeyUp);

            this.gameTimer = new Timer();
            gameTimer.Tick += new EventHandler(gameTimer_Tick);
            gameTimer.Interval = 30;
        }

        public void startCountdown()
        {
            countdown = 3;
            inCountdown = true;
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
            if (countdown == 0)
            {
                startGame();
            }
            if (countdown <= -1)
            {
                inCountdown = false;
                countdownTimer.Stop();
            }
            updateGameDisplay();
        }

        public bool getIsRunning()
        {
            return isRunning;
        }

        public void updateGameDisplay()
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
            SpriteFontGraphics.setSprite(SpriteFontGraphics.StringTypes.TIME, time, Microsoft.Xna.Framework.Graphics.Color.RoyalBlue, new Vector2(xnaDisplay.Width - 200, 5));
            SpriteFontGraphics.setSprite(SpriteFontGraphics.StringTypes.SCORE, "Score: " + score, Microsoft.Xna.Framework.Graphics.Color.RoyalBlue, new Vector2(5, 5));
            SpriteFontGraphics.setSprite(SpriteFontGraphics.StringTypes.LAP, "Lap " + lap, Microsoft.Xna.Framework.Graphics.Color.RoyalBlue, new Vector2(xnaDisplay.Width / 3f, 5));

            int angle = RobotRacer.currentCourse.getAngleAt(pylonNum);
            if (angle < 0)
            {
                SpriteFontGraphics.setSprite(SpriteFontGraphics.StringTypes.PYLON_NUM, "Pylon " + (pylonNum + 1), Microsoft.Xna.Framework.Graphics.Color.Orange, new Vector2(xnaDisplay.Width / 2f, 5));
                SpriteFontGraphics.setSprite(SpriteFontGraphics.StringTypes.ANGLE, -angle + "" + (char)176, Microsoft.Xna.Framework.Graphics.Color.Orange, new Vector2(xnaDisplay.Width / 2f + 250, 5));
                SpriteGraphics.setTexture(1, new Vector2(xnaDisplay.Width / 2f - 46, 75), 1f,false);
            }
            else if (angle > 0)
            {
                SpriteFontGraphics.setSprite(SpriteFontGraphics.StringTypes.PYLON_NUM, "Pylon " + (pylonNum + 1), Microsoft.Xna.Framework.Graphics.Color.Green, new Vector2(xnaDisplay.Width / 2f, 5));
                SpriteFontGraphics.setSprite(SpriteFontGraphics.StringTypes.ANGLE, angle + "" + (char)176, Microsoft.Xna.Framework.Graphics.Color.Green, new Vector2(xnaDisplay.Width / 2f + 250, 5));
                SpriteGraphics.setTexture(0, new Vector2(xnaDisplay.Width / 2f - 46, 75), 1f, false);
            }
            else
            {
                SpriteGraphics.setActive(1, false);
                SpriteGraphics.setActive(0, false);
                SpriteFontGraphics.setActive(SpriteFontGraphics.StringTypes.PYLON_NUM, false);
                SpriteFontGraphics.setActive(SpriteFontGraphics.StringTypes.ANGLE, false);
            }

            SpriteFontGraphics.setSprite(SpriteFontGraphics.StringTypes.VELOCITY, String.Format("{0:F2}", RobotRacer.stateVariables.Velocity) + " m/s", Microsoft.Xna.Framework.Graphics.Color.RoyalBlue, new Vector2(5, 55));

            if (inCountdown)
            {
                string countdownStr = "  " + countdown + "  ";
                if (countdown == 0)
                    countdownStr = "GO!";

                SpriteFontGraphics.setSprite(SpriteFontGraphics.StringTypes.COUNT, countdownStr, Microsoft.Xna.Framework.Graphics.Color.White, new Vector2(xnaDisplay.Width / 2 - 400, xnaDisplay.Height / 2 - 250));
            }
            else
            {
                SpriteFontGraphics.setActive(SpriteFontGraphics.StringTypes.COUNT, false);
            }
        }

        private void gameTimer_Tick(object sender, EventArgs e){
            millisecondsPassed = (int)((DateTime.Now.Ticks - startTicks) / 10000);
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
            if (Robot_Racers.RobotRacer.truckStarted == false)
            {
                gameTimer.Stop();
            }
            updateGameDisplay();
        }

        public void CloseGame()
        {
            this.isRunning = false;
            countdownTimer.Stop();
            this.Controls.Remove(xnaDisplay);
            xnaDisplay.WindowedScreen(320,240);
            this.Dispose();
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

        public xnaRoboRacerDisplay getXnaDisplay()
        {
            return xnaDisplay;
        }

        internal void Game_KeyPress(Microsoft.Xna.Framework.Input.KeyboardState currentState)
        {
            if (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape) ||
                currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Q))
            {
                CloseGame();
            }
            else if (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.G))
            {
                startCountdown();
            }
        }
    }
}
