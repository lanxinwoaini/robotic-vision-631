#region File Description
//-----------------------------------------------------------------------------
// xnaRoboRacerDisplay.cs
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
    /// <summary>
    /// Example control inherits from GraphicsDeviceControl, which allows it to
    /// render using a GraphicsDevice. This control shows how to draw animating
    /// 3D graphics inside a WinForms application. It hooks the Application.Idle
    /// event, using this to invalidate the control, which will cause the animation
    /// to constantly redraw.
    /// </summary>
    public class xnaRoboRacerDisplay : GraphicsDeviceControl
    {
        Stopwatch timer;
        public static ContentManager content;
        public static GraphicsDevice device;

        public static Skybox skybox;
        KnackTruck knackTruck;
        SpriteFontGraphics spriteFont;
        SpriteGraphics spriteTexture;
        LandscapeGraphics landscapeGraphics;
        PylonGraphics pylonGraphics;
        ModelGraphics modelGraphics;

        //set the position of the camera in world space, for our view matrix
        Vector3 cameraPosition = new Vector3(0.0f, 0.258f, 0.256f);//6.2f, 6.4f);
        Vector3 cameraLookatOffset = new Vector3(0.0f, 0.202f, -0.144f);//2.8f, -3.6f);
        Vector3 currentLookatOffset = new Vector3(0.0f, 0.202f, -0.144f);//2.8f, -3.6f);
        Matrix viewMatrix;
        Matrix projectionMatrix;

        public static bool demoMode = true;

        /// <summary>
        /// Initializes the control.
        /// </summary>
        protected override void Initialize()
        {
            content = new ContentManager(Services, "Content");
            device = GraphicsDevice;
            reSetupWindow = false;
            demoMode = true;

            content.RootDirectory = "Content";

            LoadContent();

            // Start the animation timer.
            timer = Stopwatch.StartNew();
            lastTime = (float)timer.Elapsed.TotalSeconds;

            // setup the camera
            UpdateCamera();
            isWireframe = false;
            isFullScreen = false;


            skybox = new Skybox();
            
            // Hook the idle event to constantly redraw our animation.
            Application.Idle += delegate { Invalidate(); };
        }

        public void resetTruckLocation()
        {
            knackTruck.resetLocation();
        }
        bool reSetupWindow = false;
        float nearPlane = 0.05f;
        float farPlane = 10000f;
        private void UpdateCamera()
        {
            Vector3 campos = cameraPosition;
            campos = Vector3.Transform(campos, Matrix.CreateFromQuaternion(knackTruck.ModelRotation()));
            campos += knackTruck.ModelPosition();

            Vector3 camup = new Vector3(0, 1, 0);
            camup = Vector3.Transform(camup, Matrix.CreateFromQuaternion(knackTruck.ModelRotation()));

            currentLookatOffset = Vector3.Transform(cameraLookatOffset, knackTruck.ModelRotation());

            viewMatrix = Matrix.CreateLookAt(campos, knackTruck.ModelPosition() + currentLookatOffset, camup);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, nearPlane, farPlane);
        }

        public static int width;
        public static int height;
        public static bool isFullScreen;
        public void fullScreen()
        {
            this.Width = Screen.PrimaryScreen.Bounds.Width;
            this.Height = Screen.PrimaryScreen.Bounds.Height;
            this.Left = (Screen.PrimaryScreen.Bounds.Width / 2 - this.Width / 2);
            this.Top = (Screen.PrimaryScreen.Bounds.Height / 2 - this.Height / 2);
            width = this.Width;
            height = this.Height;
            spriteFont.setupFullScreen();
            spriteTexture.setupFullScreen();
            reSetupWindow = true; //setupcamera must be called after current process is complete
            isFullScreen = true;
            //SetUpCamera();
        }
        public void WindowedScreen(int width, int height)
        {
            this.Width = width;
            this.Height = height;
            this.Left = 0;
            this.Top = 0;
            width = this.Width;
            height = this.Height;
            spriteFont.setupHalfScreen();
            spriteTexture.setupHalfScreen();
            reSetupWindow = true; //setupcamera must be called after current process is complete
            isFullScreen = false;
            //SetUpCamera();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected void LoadContent()
        {

            landscapeGraphics = new LandscapeGraphics();
            landscapeGraphics.Initialize(device);
            landscapeGraphics.LoadContent(content);

            pylonGraphics = new PylonGraphics();
            pylonGraphics.Initialize(device, demoMode);
            pylonGraphics.LoadContent(content);

            knackTruck = new KnackTruck();
            knackTruck.Initialize(device, demoMode);
            knackTruck.LoadContent(content);

            spriteFont = new SpriteFontGraphics();
            spriteFont.Initialize(device, content);
            spriteFont.setupHalfScreen();

            spriteTexture = new SpriteGraphics();
            spriteTexture.Initialize(device, content);
            spriteTexture.setupHalfScreen();

            modelGraphics = new ModelGraphics();
            modelGraphics.Initialize(device);
            modelGraphics.LoadContent(content);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected void Update(float gameTimeDiff)
        {
            knackTruck.Update(gameTimeDiff);

            UpdateInput(gameTimeDiff);
            UpdateCamera();
            skybox.Update(cameraPosition);
            spriteTexture.update(gameTimeDiff);


        }
        private void UpdateInput(float gameTimeDiff)
        {
            if (!this.Focused) return;
            MouseState currentMouseState = Mouse.GetState();
            KeyboardState currentState = Keyboard.GetState();
            //if (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            //    null;

            //do something with the clicker?
            //if (currentMouseState.LeftButton.Equals(Microsoft.Xna.Framework.Input.ButtonState.Pressed))
            {
                //if (TheKnack.xnaGame!=null) TheKnack.xnaGame.CloseGame();
            }


            //move camera position
            if (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.W))
            {
                cameraPosition.Y += 0.004f;
                if (cameraPosition.Y > 0.49f) cameraPosition.Y = 0.49f;
            }
            if (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.S))
            {
                cameraPosition.Y -= 0.004f;
                if (cameraPosition.Y < 0.258f) cameraPosition.Y = 0.258f;
            }
            if (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.D))
            {
                cameraLookatOffset.Z += cameraLookatOffset.Z / 30.0f;
                if (cameraLookatOffset.Z < -1.28) cameraLookatOffset.Z = -1.28f;
            }
            if (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.A))
            {
                cameraLookatOffset.Z -= cameraLookatOffset.Z / 30.0f;
                if (cameraLookatOffset.Z > -0.144) cameraLookatOffset.Z = -0.144f;
            }

            if (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P) &&
                (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift) ||
                currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.RightShift)))
            {
                isWireframe = true;
            } else if (currentState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P))
            {
                isWireframe = false;
            }

            //UpdateCamera(); //always update the camera

            //send the keypresses back down to the game
            if (TheKnack.xnaGame!=null && currentState.GetPressedKeys().Length > 0)
            {
                TheKnack.xnaGame.Game_KeyPress(currentState);
            }
        }

        bool isWireframe = false;
        float lastTime = 0.0f;
        protected override void Draw()
        {
            if (isWireframe) device.RenderState.FillMode = FillMode.WireFrame;
            else device.RenderState.FillMode = FillMode.Solid;
            device.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1.0f, 0);
            device.RenderState.CullMode = CullMode.None;

            device.RenderState.DepthBufferEnable = true;
            device.RenderState.AlphaBlendEnable = false;
            device.RenderState.AlphaTestEnable = false;
            device.SamplerStates[0].AddressU = TextureAddressMode.Wrap;
            device.SamplerStates[0].AddressV = TextureAddressMode.Wrap;

            float time = (float)timer.Elapsed.TotalSeconds;
            Update(time - lastTime);
            lastTime = time;
            if (isFullScreen) TheKnack.xnaGame.updateGameDisplay();

            if (reSetupWindow) UpdateCamera();

            skybox.Draw(viewMatrix, projectionMatrix);

            //draw the landscape
            landscapeGraphics.DrawLandscape(viewMatrix, projectionMatrix);
            modelGraphics.DrawModels(viewMatrix, projectionMatrix);
            
            pylonGraphics.DrawPylons(viewMatrix, projectionMatrix);
            
            knackTruck.Draw(viewMatrix, projectionMatrix);

            spriteFont.Draw();
            spriteTexture.Draw();
        }

        public void ReSetupWindow()
        {
            reSetupWindow = true;
        }

        internal PylonGraphics PylonGraphics()
        {
            return pylonGraphics;
        }
        internal LandscapeGraphics LandscapeGraphics()
        {
            return landscapeGraphics;
        }
        internal KnackTruck KnackTruck()
        {
            return knackTruck;
        }

    }
}
