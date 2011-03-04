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
    public class SpriteGraphics
    {
        GraphicsDevice device;
        SpriteBatch spriteBatch;

        public struct SpriteTextureData
        {
            public Vector2 Position;
            public bool Active;
            public float Scale;
            public Texture2D Texture;
        }

        static SpriteTextureData[] spriteTextureData;
        
        /// <summary>
        /// Initializes the control, creating the ContentManager
        /// and using it to load a SpriteFont.
        /// </summary>
        public void Initialize(GraphicsDevice device, ContentManager content)
        {
            this.device = device;
            spriteBatch = new SpriteBatch(device);

            spriteTextureData = new SpriteTextureData[2];
            spriteTextureData[0].Active = false;
            spriteTextureData[0].Position = Vector2.Zero;
            spriteTextureData[0].Scale = 1.0f;
            spriteTextureData[0].Texture = content.Load<Texture2D>("Textures\\ArrowLeft");

            spriteTextureData[1].Active = false;
            spriteTextureData[1].Position = Vector2.Zero;
            spriteTextureData[1].Scale = 1.0f;
            spriteTextureData[1].Texture = content.Load<Texture2D>("Textures\\ArrowRight");

            mAlphaValue = 255;
            pulsed = true;
            mFadeIncrement = 10;
        }

        private static bool pulsed;
        private static int mAlphaValue;
        private static int mFadeIncrement;
        public void update(float gameTimeDiff)
        {
                //Increment/Decrement the fade value for the image
                mAlphaValue += (int)((float)mFadeIncrement * gameTimeDiff/.035f);

                //If the AlphaValue is equal or above the max Alpha value or
                //has dropped below or equal to the min Alpha value, then 
                //reverse the fade
                if (mAlphaValue >= 255)
                {
                    mAlphaValue = 255;
                    mFadeIncrement *= -1;
                } else if (mAlphaValue <= 100){
                    mAlphaValue = 100;
                    mFadeIncrement *= -1;
                }
        }


        public static void setActive(int index, bool active)
        {
            spriteTextureData[index].Active = active;
        }
        public static void setTexture(int index, Vector2 pos, float scale, bool reset)
        {
            spriteTextureData[index].Active = true;
            spriteTextureData[index].Position = pos;
            spriteTextureData[index].Scale = scale;
            if (reset) mAlphaValue = 255;
        }
        public void setupHalfScreen()
        {
            spriteTextureData[0].Active = false;
            spriteTextureData[1].Active = false;
        }
        public void setupFullScreen()
        {
            spriteTextureData[0].Active = false;
            spriteTextureData[1].Active = false;
        }


        /// <summary>
        /// Draws the control, using SpriteBatch and SpriteFont.
        /// </summary>
        public void Draw()
        {
            device.RenderState.AlphaBlendEnable = true;
            device.RenderState.SourceBlend = Blend.One;
            device.RenderState.DestinationBlend = Blend.One;

            spriteBatch.Begin();
            foreach (SpriteTextureData data in spriteTextureData)
            {
                if (data.Active){
                    if (pulsed){
                        spriteBatch.Draw(data.Texture, data.Position, new Color(255, 255, 255, (byte)MathHelper.Clamp(mAlphaValue, 0, 255)));
                    } else{
                        spriteBatch.Draw(data.Texture, data.Position, Color.White);
                    }
                }
            }
            spriteBatch.End();
        }


    }
}
