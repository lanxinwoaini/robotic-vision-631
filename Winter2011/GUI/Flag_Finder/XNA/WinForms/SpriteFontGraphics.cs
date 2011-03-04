#region File Description
//-----------------------------------------------------------------------------
// SpriteFontControl.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Robot_Racers.XNAGame.WinForms;
using System;
#endregion

namespace Robot_Racers.XNAGame
{
    public class SpriteFontGraphics
    {
        GraphicsDevice device;
        SpriteBatch spriteBatch;
        SpriteFont font_small;
        SpriteFont font_medium;
        SpriteFont font_large;

        public const int NUM_STRINGTYPES = 7;
        public enum StringTypes : byte
        {
            SCORE = 0,
            VELOCITY = 1,
            LAP = 2,
            PYLON_NUM = 3,
            ANGLE = 4,
            TIME = 5,
            COUNT = 6
        }

        public struct SpriteStringData
        {
            public Vector2 Position;
            public Color Color;
            public string Text;
            public bool Active;
            public int fontSize;
        }

        private static SpriteStringData[] spriteStrings;

        /// <summary>
        /// Initializes the control, creating the ContentManager
        /// and using it to load a SpriteFont.
        /// </summary>
        public void Initialize(GraphicsDevice device, ContentManager content)
        {
            this.device = device;
            spriteBatch = new SpriteBatch(device);

            font_small = content.Load<SpriteFont>("ArialSmall");
            font_medium = content.Load<SpriteFont>("ArialMedium");
            font_large = content.Load<SpriteFont>("ArialLarge");
            
            spriteStrings = new SpriteStringData[NUM_STRINGTYPES];
            InitializeStrings();
        }

        public static void setActive(StringTypes stype, bool active)
        {
            spriteStrings[(int)stype].Active = active;
        }
        public static void setText(StringTypes stype, string text)
        {
            spriteStrings[(int)stype].Text = text;
        }
        public static void setColor(StringTypes stype, Color c)
        {
            spriteStrings[(int)stype].Color = c;
        }
        public static void setPosition(StringTypes stype, Vector2 pos)
        {
            spriteStrings[(int)stype].Position = pos;
        }
        public static void setFontSize(StringTypes stype, int size)
        {
            spriteStrings[(int)stype].fontSize = size; //a value from 0 to 2;
        }
        public static void setSprite(StringTypes stype, string text, Color c, Vector2 pos, int fontSize)
        {
            spriteStrings[(int)stype].Active = true;
            spriteStrings[(int)stype].Text = text;
            spriteStrings[(int)stype].Color = c;
            spriteStrings[(int)stype].Position = pos;
            spriteStrings[(int)stype].fontSize = fontSize; //a value from 0 to 2;
        }
        public static void setSprite(StringTypes stype, string text, Color c)
        {
            spriteStrings[(int)stype].Active = true;
            spriteStrings[(int)stype].Text = text;
            spriteStrings[(int)stype].Color = c;
        }
        internal static void setSprite(StringTypes stype, string text, Color c, Vector2 pos)
        {
            spriteStrings[(int)stype].Active = true;
            spriteStrings[(int)stype].Text = text;
            spriteStrings[(int)stype].Color = c;
            spriteStrings[(int)stype].Position = pos;
        }

        public void setupHalfScreen()
        {
            spriteStrings[(int)StringTypes.SCORE].Active = false;
            spriteStrings[(int)StringTypes.LAP].Active = false;
            spriteStrings[(int)StringTypes.TIME].Active = false;
            spriteStrings[(int)StringTypes.COUNT].Active = false;

            spriteStrings[(int)StringTypes.VELOCITY].Active = false;
            spriteStrings[(int)StringTypes.PYLON_NUM].Active = false;
            spriteStrings[(int)StringTypes.ANGLE].Active = false;

            spriteStrings[(int)StringTypes.VELOCITY].Position = new Vector2(23, 3);
            spriteStrings[(int)StringTypes.PYLON_NUM].Position = new Vector2(123, 3);
            spriteStrings[(int)StringTypes.ANGLE].Position = new Vector2(223, 3);

            spriteStrings[(int)StringTypes.SCORE].fontSize = 0;
            spriteStrings[(int)StringTypes.LAP].fontSize = 0;
            spriteStrings[(int)StringTypes.TIME].fontSize = 0;
            spriteStrings[(int)StringTypes.VELOCITY].fontSize = 0;
            spriteStrings[(int)StringTypes.PYLON_NUM].fontSize = 0;
            spriteStrings[(int)StringTypes.ANGLE].fontSize = 0;
            spriteStrings[(int)StringTypes.COUNT].fontSize = 1;

        }
        public void setupFullScreen()
        {
            spriteStrings[(int)StringTypes.SCORE].Active = true;
            spriteStrings[(int)StringTypes.LAP].Active = true;
            spriteStrings[(int)StringTypes.TIME].Active = true;
            spriteStrings[(int)StringTypes.VELOCITY].Active = true;
            spriteStrings[(int)StringTypes.PYLON_NUM].Active = true;
            spriteStrings[(int)StringTypes.ANGLE].Active = true;
            spriteStrings[(int)StringTypes.COUNT].Active = false;

            //each are about 200 wide of text
            const int textWidth = 300;
            int increment = xnaRoboRacerDisplay.width / 6;
            int curLoc = Math.Max(0, increment - textWidth / 2);

            spriteStrings[(int)StringTypes.SCORE].Position = new Vector2(23, 23);
            spriteStrings[(int)StringTypes.VELOCITY].Position = new Vector2(23, 70);
            curLoc += increment;
            spriteStrings[(int)StringTypes.LAP].Position = new Vector2(curLoc, 23);
            curLoc += increment;
            spriteStrings[(int)StringTypes.PYLON_NUM].Position = new Vector2(curLoc, 23);
            curLoc += increment;
            spriteStrings[(int)StringTypes.ANGLE].Position = new Vector2(curLoc, 23);
            curLoc += increment;
            spriteStrings[(int)StringTypes.TIME].Position = new Vector2(xnaRoboRacerDisplay.width - textWidth, 23);

            spriteStrings[(int)StringTypes.SCORE].fontSize = 1;
            spriteStrings[(int)StringTypes.LAP].fontSize = 1;
            spriteStrings[(int)StringTypes.TIME].fontSize = 1;
            spriteStrings[(int)StringTypes.VELOCITY].fontSize = 1;
            spriteStrings[(int)StringTypes.PYLON_NUM].fontSize = 1;
            spriteStrings[(int)StringTypes.ANGLE].fontSize = 1;
            spriteStrings[(int)StringTypes.COUNT].fontSize = 2;
        }

        private void InitializeStrings()
        {
            spriteStrings[(int)StringTypes.SCORE].Color = Color.RoyalBlue;
            spriteStrings[(int)StringTypes.SCORE].Text = "Score: 0";
            spriteStrings[(int)StringTypes.SCORE].Position = new Vector2(23, 23);

            spriteStrings[(int)StringTypes.VELOCITY].Color = Color.RoyalBlue;
            spriteStrings[(int)StringTypes.VELOCITY].Text = "0.00 m/s";
            spriteStrings[(int)StringTypes.VELOCITY].Position = new Vector2(123, 23);

            spriteStrings[(int)StringTypes.LAP].Color = Color.White;
            spriteStrings[(int)StringTypes.LAP].Text = "Lap 1";
            spriteStrings[(int)StringTypes.LAP].Position = new Vector2(223, 23);

            spriteStrings[(int)StringTypes.PYLON_NUM].Color = Color.Green;
            spriteStrings[(int)StringTypes.PYLON_NUM].Text = "Pylon 1";
            spriteStrings[(int)StringTypes.PYLON_NUM].Position = new Vector2(323, 23);

            spriteStrings[(int)StringTypes.ANGLE].Color = Color.Green;
            spriteStrings[(int)StringTypes.ANGLE].Text = "180"+ '\u00b0'.ToString();
            spriteStrings[(int)StringTypes.ANGLE].Position = new Vector2(423, 23);

            spriteStrings[(int)StringTypes.TIME].Color = Color.White;
            spriteStrings[(int)StringTypes.TIME].Text = "0:00.00";
            spriteStrings[(int)StringTypes.TIME].Position = new Vector2(523, 23);
        }


        /// <summary>
        /// Draws the control, using SpriteBatch and SpriteFont.
        /// </summary>
        public void Draw()
        {
            spriteBatch.Begin();
            foreach (SpriteStringData data in spriteStrings)
            {
                if (data.Active)
                    spriteBatch.DrawString(
                        (data.fontSize == 2 ? font_large : data.fontSize == 1 ? font_medium : font_small),
                        data.Text, data.Position, data.Color);
            }
            spriteBatch.End();
        }



    }
}
