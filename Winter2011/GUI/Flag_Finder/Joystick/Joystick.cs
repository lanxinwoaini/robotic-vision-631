/******************************************************************************
 * C# Joystick Library - Copyright (c) 2006 Mark Harris - MarkH@rris.com.au
 ******************************************************************************
 * You may use this library in your application, however please do give credit
 * to me for writing it and supplying it. If you modify this library you must
 * leave this notice at the top of this file. I'd love to see any changes you
 * do make, so please email them to me :)
 *****************************************************************************/
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.DirectX.DirectInput;
using System.Diagnostics;

namespace Robot_Racers.Joystick
{
    /// <summary>
    /// Class to interface with a joystick device.
    /// </summary>
    public class Joystick
    {
        private Device joystickDevice;
        private JoystickState state;
        
        private int axisCount;
        /// <summary>
        /// Number of axes on the joystick.
        /// </summary>
        public int AxisCount
        {
            get { return axisCount; }
        }
        
        private int axisA;
        private int axisABias;
        /// <summary>
        /// The first axis on the joystick.
        /// </summary>
        public int AxisA
        {
            get { return axisA; }
        }
        public int AxisA_Biased
        {
            get { return axisA + axisABias; }
        }

        private int axisB;
        private int axisBBias;
        /// <summary>
        /// The second axis on the joystick.
        /// </summary>
        public int AxisB
        {
            get { return axisB; }
        }
        public int AxisB_Biased
        {
            get { return axisB + axisBBias; }
        }

        private int axisC;
        private int axisCBias;
        /// <summary>
        /// The third axis on the joystick.
        /// </summary>
        public int AxisC
        {
            get { return axisC; }
        }
        public int AxisC_Biased
        {
            get { return axisC + axisCBias; }
        }

        private int axisD;
        private int axisDBias;
        /// <summary>
        /// The fourth axis on the joystick.
        /// </summary>
        public int AxisD
        {
            get { return axisD; }
        }
        public int AxisD_Biased
        {
            get { return axisD + axisDBias; }
        }

        private int axisE;
        private int axisEBias;
        /// <summary>
        /// The fifth axis on the joystick.
        /// </summary>
        public int AxisE
        {
            get { return axisE; }
        }
        public int AxisE_Biased
        {
            get { return axisE + axisEBias; }
        }
        
        private int axisF;
        private int axisFBias;
        /// <summary>
        /// The sixth axis on the joystick.
        /// </summary>
        public int AxisF
        {
            get { return axisF; }
        }
        public int AxisF_Biased
        {
            get { return axisF + axisFBias; }
        }

        private IntPtr hWnd;

        private bool[] buttons;
        /// <summary>
        /// Array of buttons availiable on the joystick. This also includes PoV hats.
        /// </summary>
        public bool[] Buttons
        {
            get { return buttons; }
        }

        private string[] systemJoysticks;

        /// <summary>
        /// Constructor for the class.
        /// </summary>
        /// <param name="window_handle">Handle of the window which the joystick will be "attached" to.</param>
        public Joystick(IntPtr window_handle)
        {
            hWnd = window_handle;
            axisA = -1;
            axisB = -1;
            axisC = -1;
            axisD = -1;
            axisE = -1;
            axisF = -1;
            axisCount = 0;
        }

        private void Poll()
        {
            try
            {
                // poll the joystick
                joystickDevice.Poll();
                // update the joystick state field
                state = joystickDevice.CurrentJoystickState;
            }
            catch (Exception err)
            {
                // we probably lost connection to the joystick
                // was it unplugged or locked by another application?
                Debug.WriteLine("Poll()");
                Debug.WriteLine(err.Message);
                Debug.WriteLine(err.StackTrace);
            }
        }

        /// <summary>
        /// Retrieves a list of joysticks attached to the computer.
        /// </summary>
        /// <example>
        /// [C#]
        /// <code>
        /// JoystickInterface.Joystick jst = new JoystickInterface.Joystick(this.Handle);
        /// string[] sticks = jst.FindJoysticks();
        /// </code>
        /// </example>
        /// <returns>A list of joysticks as an array of strings.</returns>
        public string[] FindJoysticks()
        {
            systemJoysticks = null;

            try
            {
                // Find all the GameControl devices that are attached.
                DeviceList gameControllerList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
                
                // check that we have at least one device.
                if (gameControllerList.Count > 0)
                {
                    systemJoysticks = new string[gameControllerList.Count];
                    int i = 0;
                    // loop through the devices.
                    foreach (DeviceInstance deviceInstance in gameControllerList)
                    {
                        // create a device from this controller so we can retrieve info.
                        joystickDevice = new Device(deviceInstance.InstanceGuid);
                        joystickDevice.SetCooperativeLevel(hWnd,
                            CooperativeLevelFlags.Background |
                            CooperativeLevelFlags.NonExclusive);

                        systemJoysticks[i] = joystickDevice.DeviceInformation.InstanceName;

                        i++;
                    }
                }
            }
            catch (Exception err)
            {
                Debug.WriteLine("FindJoysticks()");
                Debug.WriteLine(err.Message);
                Debug.WriteLine(err.StackTrace);
            }

            return systemJoysticks;
             
        }
        
        /// <summary>
        /// Acquire the named joystick. You can find this joystick through the <see cref="FindJoysticks"/> method.
        /// </summary>
        /// <param name="name">Name of the joystick.</param>
        /// <returns>The success of the connection.</returns>
        public bool AcquireJoystick(string name)
        {
            try
            {
                DeviceList gameControllerList = Manager.GetDevices(DeviceClass.GameControl, EnumDevicesFlags.AttachedOnly);
                int i = 0;
                bool found = false;
                // loop through the devices.
                foreach (DeviceInstance deviceInstance in gameControllerList)
                {
                    if (deviceInstance.InstanceName == name)
                    {
                        found = true;
                        // create a device from this controller so we can retrieve info.
                        joystickDevice = new Device(deviceInstance.InstanceGuid);
                        joystickDevice.SetCooperativeLevel(hWnd,
                            CooperativeLevelFlags.Background |
                            CooperativeLevelFlags.NonExclusive);
                        break;
                    }

                    i++;
                }

                if (!found)
                    return false;
                
                // Tell DirectX that this is a Joystick.
                joystickDevice.SetDataFormat(DeviceDataFormat.Joystick);

                // Finally, acquire the device.
                joystickDevice.Acquire();

                // How many axes?
                // Find the capabilities of the joystick
                DeviceCaps cps = joystickDevice.Caps;
                Debug.WriteLine("Joystick Axis: " + cps.NumberAxes);
                Debug.WriteLine("Joystick Buttons: " + cps.NumberButtons);

                axisCount = cps.NumberAxes;

                ZeroAxis();
                UpdateStatus(false);
            }
            catch (Exception err)
            {
                Debug.WriteLine("FindJoysticks()");
                Debug.WriteLine(err.Message);
                Debug.WriteLine(err.StackTrace);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Set the zero values of the axis
        /// </summary>
        public void ZeroAxis()
        {
            Poll();
            int[] extraAxis = state.GetSlider();
            //Rz Rx X Y Axis1 Axis2
            axisABias = -state.Rz;
            axisBBias = -state.Rx;
            axisCBias = -state.X;
            axisDBias = -state.Y;
            axisEBias = -extraAxis[0];
            axisFBias = -extraAxis[1];
        }

        /// <summary>
        /// Unaquire a joystick releasing it back to the system.
        /// </summary>
        public void ReleaseJoystick()
        {
            joystickDevice.Unacquire();
        }

        byte[] jsButtonsOld = null;
        /// <summary>
        /// Update the properties of button and axis positions.
        /// </summary>
        public void UpdateStatus(bool sendPacket)
        {
            Poll();
            if (jsButtonsOld == null) jsButtonsOld = state.GetButtons();
            byte[] statebuttons = state.GetButtons();

            int[] extraAxis = state.GetSlider();
            //Rz Rx X Y Axis1 Axis2
            axisA = state.Rz;
            axisB = state.Rx;
            axisC = state.X;
            axisD = state.Y;
            axisE = extraAxis[0];
            axisF = extraAxis[1];

            // not using buttons, so don't take the tiny amount of time it takes to get/parse
            byte[] jsButtons = state.GetButtons();
            buttons = new bool[jsButtons.Length];

            int i = 0;
            foreach (byte button in jsButtons)
            {
                buttons[i] = button >= 128;
                i++;
            }

            int tempX = 0, tempY = 0;

            tempX = -(AxisC_Biased) / 1024;
            float temp = (float)tempX / (float)32.0;
            temp = temp * 0x30;
            tempX = (int)temp;
            tempY = -(AxisD_Biased - AxisA_Biased) / 1024; //ranges from -63 to 0, so bring it up

            if (sendPacket)
            {
                SerialHeader header = new SerialHeader();                       // Setup transmission header
                header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
                header.Subtype = (byte)SerialUtils.CommandSubtype.VELOCITY_STEERING;

                int velocitySteering = (int)((tempY << 16) & 0xFFFF0000) | (0x0000FFFF & tempX); // Concatonate velocity and steering
                //int velocitySteering = (int)((tempY << 16) & 0xFFFF0000) | (0x00000030);// & tempX); // Concatonate velocity and steering
                RobotRacer.serialPort.transmitIntData(header, velocitySteering);   // Transmit velocity and stering
            }
            
            //if speedMultiplier change:
            //if (statebuttons[12] != 0 && jsButtonsOld[12] == 0)
            //{

            //    if (TheKnack.racer.speedSlider.Value > TheKnack.racer.speedSlider.Minimum)
            //    {
            //        TheKnack.racer.speedSlider.Value--;
            //    }

            //    TheKnack.racer.speedSlider_Scroll(null, null);
            //}
            //else if (statebuttons[13] != 0 && jsButtonsOld[13] == 0)
            //{
            //    if (TheKnack.racer.speedSlider.Value < TheKnack.racer.speedSlider.Maximum)
            //        TheKnack.racer.speedSlider.Value++;
            //    TheKnack.racer.speedSlider_Scroll(null, null);
            //}
            //if trim change:
            if (statebuttons[5] != 0)
            {
                if (TheKnack.racer.steeringSlider.Value > TheKnack.racer.steeringSlider.Minimum)
                    TheKnack.racer.steeringSlider.Value--;
                TheKnack.racer.steeringSlider_Scroll(null, null);
            }
            else if (statebuttons[4] != 0)
            {
                if (TheKnack.racer.steeringSlider.Value < TheKnack.racer.steeringSlider.Maximum)
                    TheKnack.racer.steeringSlider.Value++;
                TheKnack.racer.steeringSlider_Scroll(null, null);
            }
            jsButtonsOld = state.GetButtons();
        }
    }
}
