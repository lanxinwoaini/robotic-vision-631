#region File Description
//-----------------------------------------------------------------------------
// KnackTruck.cs
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
    class KnackTruck
    {
        GraphicsDevice device;

        Vector3 modelPosition = new Vector3(0.0f, 0.0f, 0.0f);
        Vector3 modelPositionNoise = new Vector3(0.0f, 0.0f, 0.0f);
        Quaternion modelRotation = Quaternion.Identity;
        Model knackModel;

        bool demoMode = false;
        internal void Initialize(GraphicsDevice device, bool test)
        {
            this.device = device;
            //this is just for demo
            this.demoMode = test;

        }
        public void LoadContent(ContentManager content)
        {
            knackModel = content.Load<Model>("Models\\KnackTruck");//p1_wedge");
        }

        public Vector3 ModelPosition()
        {
            return modelPosition;
        }
        public Quaternion ModelRotation()
        {
            return modelRotation;
        }


        public void Draw(Matrix viewMatrix, Matrix projectionMatrix)
        {
            //copy any parent transforms
            Matrix[] transforms = new Matrix[knackModel.Bones.Count];
            Matrix meshTransforms = new Matrix();
            knackModel.CopyAbsoluteBoneTransformsTo(transforms);

            //draw the model. A model can have multiple meshes, so loop
            foreach (ModelMesh mesh in knackModel.Meshes)
            {
                if (mesh.Name.Contains("pTireLeft"))
                {
                    meshTransforms = Matrix.CreateRotationY(tireRotate);
                    meshTransforms *= Matrix.CreateRotationZ(tireDirection);
                    meshTransforms *= transforms[mesh.ParentBone.Index];
                }
                else if (mesh.Name.Contains("pTireRight"))
                {
                    meshTransforms = Matrix.CreateRotationY(-tireRotate);
                    meshTransforms *= Matrix.CreateRotationX(tireDirection);
                    meshTransforms *= transforms[mesh.ParentBone.Index];
                }
                else
                {
                    meshTransforms = transforms[mesh.ParentBone.Index];
                }
                meshTransforms *= Matrix.CreateFromQuaternion(modelRotation) * Matrix.CreateTranslation(modelPosition + modelPositionNoise);
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

        //Set the position of the model in world space, and set the rotation
        float tireDirection = MathHelper.ToRadians(0.0f);
        float tireRotate = MathHelper.ToRadians(15.0f);
        float tireSpeed = 0.0f;
        float modelNoise = 0.0f;


        public void Update(float gameTimeDiff)
        {
            //rotate the tire based on the tire speed
            tireRotate -= gameTimeDiff * tireSpeed;

            //slow the speed of the tire down (unless it's continually updated)
            tireSpeed *= (1.0f - gameTimeDiff * 0.95f);
            //tireDirection += demoDirectionTurn * MathHelper.ToRadians(1.0f);
            //if (tireDirection > MathHelper.ToRadians(25.0f))
            //    demoDirectionTurn = -1.0f;
            //if (tireDirection < MathHelper.ToRadians(-25.0f))
            //    demoDirectionTurn = 1.0f;
            //tireSpeed += gameTimeDiff * demoDirectionSpeed * 10.0f;
            //if (tireSpeed > MathHelper.ToRadians(360.0f))
            //    demoDirectionSpeed = -1.0f;
            //if (tireSpeed < MathHelper.ToRadians(50.0f))
            //    demoDirectionSpeed = 1.0f;

            //make the car bump up and down a bit with the velocity
            modelNoise += gameTimeDiff * tireSpeed;
            modelPositionNoise.Y = (float)Math.Sin(modelNoise) * 0.0028f;// 0.07f;
            modelPositionNoise.Z = (float)Math.Cos(modelNoise) * 0.0016f;// 0.04f;

            if (demoMode)
                ProcessKeyboard(gameTimeDiff); //allow the keyboard to navigate
            else
                UpdateRealtimeState(gameTimeDiff);
        }
        private void UpdateRealtimeState(float gameTimeDiff)
        {
            float moveSpeed = tireSpeed * 0.2f * gameTimeDiff;
            float turningSpeed = gameTimeDiff * 100.0f * moveSpeed;

            MoveForward(ref modelPosition, modelRotation, moveSpeed);
            modelPosition.Y = LandscapeGraphics.GetHeightAtWorldLocation(modelPosition);

            float leftRightRot = 0.0f;
            leftRightRot += turningSpeed * tireDirection;
            //Console.WriteLine("wheelAngle=" + tireDirection.ToString());
            //rotate the model
            Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), leftRightRot);
            modelRotation *= additionalRot;
        }
        private void ProcessKeyboard(float gameTimeDiff)
        {
            KeyboardState currentState = Keyboard.GetState();

            //process movement due to velocity
            if (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Down))
            {
                tireSpeed = 0.0f;
            }
            if (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Up))
            {
                tireSpeed = MathHelper.ToRadians(360.0f);
            }
            float moveSpeed = tireSpeed * 0.2f * gameTimeDiff;
            MoveForward(ref modelPosition, modelRotation, moveSpeed);
            modelPosition.Y = LandscapeGraphics.GetHeightAtWorldLocation(modelPosition);

            tireDirection = 0.0f;
            float leftRightRot = 0;

            // process rotation due to turning and velocity
            float turningSpeed = gameTimeDiff * 200.0f * moveSpeed;
            if (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Right))
            {
                tireDirection = MathHelper.ToRadians(-20.0f);
                leftRightRot += turningSpeed * tireDirection;
            }
            if (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Left))
            {
                tireDirection = MathHelper.ToRadians(20.0f);
                leftRightRot += turningSpeed * tireDirection;
            }

            Quaternion additionalRot = Quaternion.CreateFromAxisAngle(new Vector3(0, 1, 0), leftRightRot);
            modelRotation *= additionalRot;
        }

        private void MoveForward(ref Vector3 position, Quaternion rotationQuat, float speed)
        {
            Vector3 addVector = Vector3.Transform(new Vector3(0, 0, -1), rotationQuat);
            position += addVector * speed;
        }

        public void updateVehicleStateVariables(StateVariables stateVariables)
        {
            tireSpeed = stateVariables.Velocity * MathHelper.TwoPi / 2.0f; //not sure how many revolutions per meter
            int tireDirectionDegrees = stateVariables.SentSteeringAngle;
            if (tireDirectionDegrees > 45) tireDirectionDegrees = 45;
            if (tireDirectionDegrees < -45) tireDirectionDegrees = -45;
            tireDirection = MathHelper.ToRadians(tireDirectionDegrees);
            demoMode = false;
            //float velocity;
            //byte sentSteeringAngle;
            //byte pylonNum;
            //byte carState;
            //float distanceTraveled;
        }




        public void resetLocation()
        {
            modelPosition = new Vector3(0.0f, 0.0f, 0.0f);
            modelPositionNoise = new Vector3(0.0f, 0.0f, 0.0f);
            modelRotation = Quaternion.Identity;
        }
    }
}
