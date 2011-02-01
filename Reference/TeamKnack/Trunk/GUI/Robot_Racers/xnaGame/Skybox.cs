//======================================================================
// XNA Terrain Editor
// Copyright (C) 2008 Eric Grossinger
// http://psycad007.spaces.live.com/
//======================================================================
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace Robot_Racers.XNAGame
{
    public class Skybox
    {
        Texture2D skyUp;
        Texture2D skyDown;
        Texture2D skyLeft;
        Texture2D skyRight;
        Texture2D skyFront;
        Texture2D skyBack;

        Model model;

        Matrix world;
        float scale = 10f;

        public Skybox()
        {
            model = xnaRoboRacerDisplay.content.Load<Model>(@"models\\skybox");
            LoadTextures();
        }

        public void LoadTextures()
        {
            skyBack = xnaRoboRacerDisplay.content.Load<Texture2D>(@"textures\\skybox\\clearblue_bk");
            skyFront = xnaRoboRacerDisplay.content.Load<Texture2D>(@"textures\\skybox\\clearblue_ft");
            skyDown = xnaRoboRacerDisplay.content.Load<Texture2D>(@"textures\\skybox\\clearblue_dn");
            skyUp = xnaRoboRacerDisplay.content.Load<Texture2D>(@"textures\\skybox\\clearblue_up");
            skyRight = xnaRoboRacerDisplay.content.Load<Texture2D>(@"textures\\skybox\\clearblue_lt");
            skyLeft = xnaRoboRacerDisplay.content.Load<Texture2D>(@"textures\\skybox\\clearblue_rt");
        }

        public void Update(Vector3 cameraPosition)
        {
            world = Matrix.CreateScale(scale) * Matrix.CreateRotationX(MathHelper.ToRadians(-90f)) * Matrix.CreateTranslation(cameraPosition);
        }

        public void Draw(Matrix view, Matrix projection)
        {
            xnaRoboRacerDisplay.device.RenderState.FogEnable = false;
            xnaRoboRacerDisplay.device.RenderState.CullMode = CullMode.CullCounterClockwiseFace;

            foreach (ModelMesh mesh in model.Meshes)
            {
                foreach (BasicEffect meshEffect in mesh.Effects)
                {
                    meshEffect.World = world;
                    meshEffect.View = view;
                    meshEffect.Projection = projection;
                    meshEffect.TextureEnabled = true;
                    meshEffect.LightingEnabled = false;

                    switch (mesh.Name)
                    {
                        case "up":
                            meshEffect.Texture = skyUp;
                            break;
                        case "left":
                            meshEffect.Texture = skyLeft;
                            break;
                        case "right":
                            meshEffect.Texture = skyRight;
                            break;
                        case "back":
                            meshEffect.Texture = skyBack;
                            break;
                        case "front":
                            meshEffect.Texture = skyFront;
                            break;
                        case "down":
                            meshEffect.Texture = skyDown;
                            break;
                    }

                    meshEffect.DiffuseColor = Vector3.One;
                    meshEffect.CommitChanges();
                }

                mesh.Draw();
            }
        }
    }
}
