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
    class PylonGraphics
    {
        private static Object pylonDataLock = new Object();
        GraphicsDevice device;

        PylonData[] pylonData = null;
        Model orangePylon;
        Model greenPylon;

        Vector3 orangePylonPosition = new Vector3(-10.0f, 0.0f, -20.0f);
        Vector3 greenPylonPosition = new Vector3(10.0f, 0.0f, -20.0f);

        internal void Initialize(GraphicsDevice device, bool test)
        {
            this.device = device;
            //this is just for demo
            if (test)
            {
                Pylon[] pylons = new Pylon[4];
                pylons[0] = new Pylon(50, 3, 240, 300, PylonColor.Orange, 50);
                pylons[1] = new Pylon(100, 8, 100, 240, PylonColor.Green, 66);
                pylons[2] = new Pylon(150, 12, 600, 300, PylonColor.Green, 77);
                pylons[3] = new Pylon(350, 30, 350, 240, PylonColor.Orange, 88);
                setVisiblePylonData(pylons, Vector3.Zero, Quaternion.Identity);
            }
            
        }
        public void LoadContent(ContentManager Content)
        {
            orangePylon = Content.Load<Model>("Models\\OrangePylon");
            greenPylon = Content.Load<Model>("Models\\GreenPylon");
        }


        const float ANGLE_OF_VIEW = 53.0f;
        const float CAMERA_WIDTH = 640.0f;
        const float MAX_VISABLE_DISTANCE = 6.5f;

        static int getAngleToPylon(int pylonCenter)
        {
            //An approximation
            return (int)(((float)ANGLE_OF_VIEW / (float)CAMERA_WIDTH) *
                (float)(CAMERA_WIDTH / 2 - pylonCenter));
        }

        static float getDistanceToPylon(int targetHeight, int targetWidth)
        {
            //the algorithm we want to use for calculating distance is dependant on height
            //These equations were all determined experimentally and with linear regressions
            float height;
            float width;
            float distance;

            height = (float)targetHeight;
            width = (float)targetWidth;

            if (height < 60)		// 0-60	Too far
            {
                distance = MAX_VISABLE_DISTANCE;	//default far distance
            }
            else if (height < 99)	// 99-60 
            {
                distance = ((height * -4.3515f) + 866.2625f) / 100.0f;
            }
            else if (height < 124)	// 99-124 
            {
                distance = ((height * -3.23427f) + 749.0035f) / 100.0f;
            }
            else if (height < 151) 	// 124-151 
            {
                distance = ((height * -2.24102f) + 625.0989f) / 100.0f;
            }
            else if (height < 176)	// 151-176 
            {
                distance = ((height * -1.59301f) + 528.705f) / 100.0f;
            }
            else if (height < 200)	// 176-200
            {
                distance = ((height * -1.23457f) + 465.8642f) / 100.0f;
            }
            else if (height < 242)	// 200-242
            {
                distance = ((height * -.94072f) + 407.1459f) / 100.0f;
            }
            else if (height < 295)	// 242-295
            {
                distance = ((height * -.57485f) + 318.9151f) / 100.0f;
            }
            else if (height < 369)	// 295-369
            {
                distance = ((height * -.40315f) + 267.8368f) / 100.0f;
            }
            else if (targetWidth < 34)// 300-480 (1-2m) Width
            {
                distance = (float)(targetWidth - 52) / -17.9f;
            }
            else if (targetWidth < 55)// 300-480 (1-2m) Width
            {
                distance = ((width * -1.61111f) + 159.6111f) / 100.0f; //mjd
            }
            else if (targetWidth < 81)// 300-480 (1-2m) Width
            {
                distance = ((width * -1f) + 126f) / 100.0f; //mjd
            }
            else if (targetWidth < 137)// 300-480 (1-2m) Width
            {
                distance = ((width * -0.28571f) + 68.14286f) / 100.0f; //mjd
            }
            else //if (targetWidth < 229)// this should handle to the largest width
            {
                distance = ((width * -0.11957f) + 45.38043f) / 100.0f; //mjd
            }

            return distance;

        }

        public void Get3dWorldCoords(ref PylonData pylon, Vector3 carPosition, Quaternion carRotation)
        {
            //push the pylon out relative to car
            Vector3 pylonPosition = new Vector3(0.0f, 0.0f, -pylon.Distance);
            //rotate relative to position in front of car
            pylonPosition = Vector3.Transform(pylonPosition, Matrix.CreateRotationY(MathHelper.ToRadians(pylon.Angle)));
            //rotate pylon with car's rotation
            pylonPosition = Vector3.Transform(pylonPosition, Matrix.CreateFromQuaternion(carRotation));
            //add car's position
            pylonPosition += carPosition;
            pylonPosition.Y = LandscapeGraphics.GetHeightAtWorldLocation(pylonPosition);
            pylon.Position = pylonPosition;
        }

        public void setVisiblePylonData(Pylon[] pylons, Vector3 carPosition, Quaternion carRotation)
        {
            lock (pylonDataLock)
            {
                pylonData = new PylonData[pylons.Length];
                for (int i = 0; i < pylons.Length; i++)
                {
                    pylonData[i] = new PylonData();
                    pylonData[i].Pylon = pylons[i];
                    pylonData[i].Num = i;
                    pylonData[i].Distance = getDistanceToPylon(pylons[i].Height, pylons[i].Width);
                    pylonData[i].Angle = getAngleToPylon(pylons[i].Center);
                    Get3dWorldCoords(ref pylonData[i], carPosition, carRotation);
                }
            }
        }

        public void DrawPylons(Matrix viewMatrix, Matrix projectionMatrix)
        {
            lock (pylonDataLock)
            {
                if (pylonData == null) return;
                foreach (PylonData pylon in pylonData)
                {
                    if (((PylonColor)pylon.Pylon.Color).Equals(PylonColor.Green))
                    {
                        DrawPylon(greenPylon, pylon.Position, viewMatrix, projectionMatrix);
                    }
                    else if (((PylonColor)pylon.Pylon.Color).Equals(PylonColor.Orange))
                    {
                        DrawPylon(orangePylon, pylon.Position, viewMatrix, projectionMatrix);
                    }
                }
            }
        }

        public void DrawPylon(Model model, Vector3 position, Matrix viewMatrix, Matrix projectionMatrix)
        {
            //copy any parent transforms
            Matrix[] transforms = new Matrix[model.Bones.Count];
            Matrix meshTransforms = new Matrix();

            model.CopyAbsoluteBoneTransformsTo(transforms);
            foreach (ModelMesh mesh in model.Meshes)
            {
                meshTransforms = transforms[mesh.ParentBone.Index];
                meshTransforms *= Matrix.CreateTranslation(position);
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

        public class PylonData
        {
            private Pylon pylon;
            private float distance;
            private float angle;
            private int num;
            private Vector3 position;

            public Pylon Pylon
            {
                get { return pylon; }
                set { pylon = value; }
            }
            public float Angle
            {
                get { return angle; }
                set { angle = value; }
            }
            public float Distance
            {
                get { return distance; }
                set { distance = value; }
            }
            public int Num
            {
                get { return num; }
                set { num = value; }
            }
            public Vector3 Position
            {
                get { return position; }
                set { position = value; }
            }

        }



    }
}
