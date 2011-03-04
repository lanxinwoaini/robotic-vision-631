#region File Description
//-----------------------------------------------------------------------------
// Pylons.cs
//
// TheKnack GUI
// Matt Diehl
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Diagnostics;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using Robot_Racers.XNAGame.WinForms;
using System;
#endregion

namespace Robot_Racers.XNAGame
{
    class ModelGraphics
    {
        GraphicsDevice device;

        Model[] models;
        ModelData[] modelData;
        float[] modelRotateToStraight;
        const int NUM_MODELS = 3; //number of different models
        const int NUM_MODELDATA = 7; //number of objects to place randomly on map

        internal void Initialize(GraphicsDevice device)
        {
            this.device = device;

            models = new Model[NUM_MODELS];
            modelData = new ModelData[NUM_MODELDATA];
            modelRotateToStraight = new float[NUM_MODELS];

        }
        public void LoadContent(ContentManager content)
        {
            models[0] = content.Load<Model>("Models\\TheKnack1");
            modelRotateToStraight[0] = 0.0f;
            models[1] = content.Load<Model>("Models\\TheKnack2");
            modelRotateToStraight[1] = 0.0f;
            models[2] = content.Load<Model>("Models\\Dilbert");
            modelRotateToStraight[2] = MathHelper.PiOver2;

            Random num = new Random();
            for (int i = 0; i < NUM_MODELDATA; i++)
            {
                int modelNum = num.Next(0, NUM_MODELS);
                modelData[i].model = models[modelNum]; //choose random object

                //calculate it's position
                float distanceFromTruck = (float)num.NextDouble() * ((float)LandscapeGraphics.terrainWidth - 50.0f) / 2.0f + 50.0f; //keep it outside a certain region
                float turnAngle = (float)num.NextDouble() * MathHelper.Pi - MathHelper.PiOver2 + modelRotateToStraight[modelNum]; //keep it within a 180degree arc facing us
                float distanceAngle = (float)num.NextDouble() * MathHelper.TwoPi;
                float scale = (float)num.NextDouble() * 3.0f + 5.0f;
                Matrix world = Matrix.CreateScale(scale) * Matrix.CreateRotationY(turnAngle) * Matrix.CreateTranslation(new Vector3(0, 0, distanceFromTruck)) *
                    Matrix.CreateRotationY(distanceAngle);
                Vector3 scale1, translation1;
                Quaternion rotation1;
                world.Decompose(out scale1,out rotation1,out translation1);
                
                //we need it to rest on the ground:
                float groundheight = LandscapeGraphics.GetHeightAtWorldLocation(translation1);
                float amountToAddToY = groundheight - translation1.Y - 0.2f; //and put it a little under the surface
                //Vector3 loc = new Vector3((int)((float)num.NextDouble() * (float)LandscapeGraphics.terrainWidth) - LandscapeGraphics.terrainWidth / 2, 0,
                //                          (int)((float)num.NextDouble() * (float)LandscapeGraphics.terrainHeight) - LandscapeGraphics.terrainHeight / 2);
                //while (Math.Abs(loc.X) < 50.0f) loc.X *= 2.0f;
                //while (Math.Abs(loc.Z) < 50.0f) loc.Z *= 2.0f;//keep it a min distance from car
                //loc.Y = LandscapeGraphics.GetHeightAtWorldLocation(loc)-0.2f;
                //modelData[i].position = loc;
                //modelData[i].scale = 
                modelData[i].world = world * Matrix.CreateTranslation(new Vector3(0f,amountToAddToY,0f));

            }


        }



        public struct ModelData
        {
            public Model model;
            public Matrix world;
        }

        public void DrawModels(Matrix viewMatrix, Matrix projectionMatrix)
        {
            foreach (ModelData modelD in modelData)
            {
                Draw(modelD.model, modelD.world, viewMatrix, projectionMatrix);
            }
        }

        public void Draw(Model model, Matrix world, Matrix viewMatrix, Matrix projectionMatrix)
        {
            //copy any parent transforms
            Matrix[] transforms = new Matrix[model.Bones.Count];
            Matrix meshTransforms = new Matrix();

            model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                meshTransforms = transforms[mesh.ParentBone.Index];
                meshTransforms *= world;
                //this is where the mesh orientation is set, as well as our camera and projection
                foreach (BasicEffect effect in mesh.Effects)
                {
                    effect.EnableDefaultLighting();
                    effect.World = meshTransforms;
                    effect.View = viewMatrix;
                    effect.Projection = projectionMatrix;
                }
                //draw the mesh, using the effects set above
                mesh.Draw();
            }
        }

    }
}
