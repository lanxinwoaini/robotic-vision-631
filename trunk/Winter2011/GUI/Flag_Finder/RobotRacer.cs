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
    public partial class RobotRacer : Form
    {
        public static StateVariables stateVariables = new StateVariables();


        // This delegate enables asynchronous calls for setting
        // the text property on a TextBox control.
        delegate void SetTextCallback(string text, bool printNewLine);
        delegate void SetTextCallback2(string text);
        delegate void SetTextCallback3();
        delegate void SetTextCallback4(ushort regId, int value);

        private Robot_Racers.Joystick.Joystick jst;
        private bool JoystickEnabled = false;
        public static bool courseLoaded = false;

        public enum Mode : int
        {
            DISABLED = 0,
            DEBUG = 1,
            RACE = 2,
            MANUAL = 3,
            JOYSTICK = 4
        }

        public enum NavDriveMode
        {
            GATHER = 0,	//Look for the next pylon and head for it
            CORRECT = 1,	//Keep heading for pylon, use vision to correct path
            BLIND_STRAIGHT = 2,	//Can't see pylon, but not time to turn yet
            BLIND_TURN = 3		//Can't see pylon and turning around it - switch to GATHER when done
        }

        public static Course currentCourse;
        public static SerialUtils serialPort;
        public static bool truckStarted = false;
        public static Mode mode = Mode.DISABLED;
        public static RegisterViewer registerViewer;

        public int greenConvolutionThreshold = 60;
        public int orangeConvolutionThreshold = 60;

        private PylonColor selectedColorHSV = PylonColor.Orange;

        Timer heartbeatTimer = new Timer();

        public RobotRacer()
        {
            serialPort = new SerialUtils();             // Initialize serial 
            currentCourse = new Course();               // Initialize empty course
            rawImage = new RawImage();
            resetHeartbeatTimer();                      //Set timer interval in milliseconds
            heartbeatTimer.Tick += new System.EventHandler(heartbeatTimer_Tick);    //Add tick event handler
            InitializeComponent();
            initializeCustomComponents();
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GUI_FormClosing);
            //this.KeyUp += new KeyEventHandler(RobotRacer_KeyUp);
        }

        private void GUI_FormClosing(object sender, FormClosingEventArgs e)
        {
            serialPort.closeConnections();
        }

        private void RobotRacer_Load(object sender, EventArgs e)
        {
            //grab the Joystick
            // grab the joystick
            jst = new Robot_Racers.Joystick.Joystick(this.Handle);
            string[] sticks = jst.FindJoysticks();
            if (sticks != null)
            {
                jst.AcquireJoystick(sticks[0]);
                JoystickEnabled = true;

                // add the axis controls to the axis container
                for (int i = 0; i < jst.AxisCount; i++)
                {
                    Joystick.JoyAxis ax = new Joystick.JoyAxis();
                    ax.AxisId = i + 1;
                }
                // add the button controls to the button container
                for (int i = 0; i < jst.Buttons.Length; i++)
                {
                    Robot_Racers.Joystick.JoyButton btn = new Joystick.JoyButton();
                    btn.ButtonId = i + 1;
                    btn.ButtonStatus = jst.Buttons[i];
                }
            }
        }

        private void tmrUpdateStick_Tick(object sender, EventArgs e)
        {

            // get status
            if (JoystickEnabled)
                jst.UpdateStatus(true);

            // update the axes positions
            /*
            foreach (Control ax in flpAxes.Controls)
            {
                if (ax is Axis)
                {
                    switch (((Axis)ax).AxisId)
                    {
                        case 1:
                            ((Axis)ax).AxisPos = jst.AxisA;
                            break;
                        case 2:
                            ((Axis)ax).AxisPos = jst.AxisB;
                            break;
                        case 3:
                            ((Axis)ax).AxisPos = jst.AxisC;
                            break;
                        case 4:
                            ((Axis)ax).AxisPos = jst.AxisD;
                            break;
                        case 5:
                            ((Axis)ax).AxisPos = jst.AxisE;
                            break;
                        case 6:
                            ((Axis)ax).AxisPos = jst.AxisF;
                            break;
                    }
                }
            }
            
            foreach (Control btn in flpButtons.Controls)
            {
                if (btn is Robot_Racers.Button)
                {
                    ((Robot_Racers.Button)btn).ButtonStatus =
                        jst.Buttons[((Robot_Racers.Button)btn).ButtonId - 1];
                }
            }
            */
        }

        public void resetHeartbeatTimer()
        {
            heartbeatTimer.Interval = 150;               //reset timer interval in milliseconds
            heartbeatTimer.Stop();
        }

        private void clearCourseButton_Click(object sender, EventArgs e)
        {
            courseTextBox.Text = "";
            currentCourse = new Course();
        }

        private void courseTabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (courseTabControl.SelectedTab.Name.Equals("courseGraphicTab"))
            {
                currentCourse = new Course(courseTextBox.Text);
            }
        }

        public void resetGUI()
        {
            if (this.startButton.InvokeRequired)
            {
                SetTextCallback3 d = new SetTextCallback3(resetGUICallback);
                try
                {
                    this.Invoke
                        (d, new object[] { });
                }
                catch (System.Exception ex) { Console.WriteLine(ex.Message); }
            }
            else
            {
                resetGUICallback();
            }
        }

        private void resetGUICallback()
        {
            RobotRacer.mode = Mode.DISABLED;
            startButton.Enabled = false;
            stopButton.Enabled = false;
            modeComboBox.SelectedIndex = 0;
            changeStateStatus(true);
            steeringSlider.Value = 0;
            speedSlider.Value = 5;
        }

        private void modeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            string modeSelectedLower = ((string)modeComboBox.SelectedItem).ToLower();

            truckStarted = false;
            enableAllButtons();
            mouseControlPanel.Invalidate();
            stopButton_Click(null, null);

            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.REGISTER;
            header.Subtype = (byte)SerialUtils.RegisterSubtype.SET_INT;
            RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.NUM_MILLISECONDS_TO_RUN_INT, 0);


            if (modeSelectedLower.Equals("disabled"))
            {
                RobotRacer.mode = Mode.DISABLED;
                startButton.Enabled = false;
                stopButton.Enabled = false;
                changeStateStatus(true);

            }
            else if (modeSelectedLower.Equals("debug mode"))
            {
                RobotRacer.mode = Mode.DEBUG;
                startButton.Enabled = true;
                stopButton.Enabled = true;
                changeStateStatus(true);

            }
            else if (modeSelectedLower.Equals("race mode"))
            {
                RobotRacer.mode = Mode.RACE;
                startButton.Enabled = true;
                stopButton.Enabled = true;
                changeStateStatus(true);
                disableRaceButtons1();
            }
            else if (modeSelectedLower.Equals("manual mode"))
            {
                RobotRacer.mode = Mode.MANUAL;
                startButton.Enabled = true;
                stopButton.Enabled = true;
                changeStateStatus(true);

            }
            else if (modeSelectedLower.Equals("joystick mode"))
            {
                RobotRacer.mode = Mode.JOYSTICK;
                startButton.Enabled = true;
                stopButton.Enabled = true;
                changeStateStatus(true);

            }
            else
            {
                RobotRacer.mode = Mode.DISABLED;
                startButton.Enabled = false;
                stopButton.Enabled = false;
                changeStateStatus(true);

            }
            Console.WriteLine(modeSelectedLower);
        }



        private void enableAllButtons()
        {
            saveImageButton.Enabled = true;
            startVideoButton.Enabled = true;
            stopVideoButton.Enabled = true;
            getImageButton.Enabled = true;
            messagesOffButton.Enabled = true;
            messagesOnButton.Enabled = true;
            viewRegistersButton.Enabled = true;
            txCheckbox.Enabled = true;
            rxCheckbox.Enabled = true;
            clearCourseButton.Enabled = true;
            loadCourseButton.Enabled = true;
            saveCourseButton.Enabled = true;
            sendCourseButton.Enabled = true;


        }
        private void disableRaceButtons1()
        {

            saveImageButton.Enabled = false;
            startVideoButton.Enabled = false;
            stopVideoButton.Enabled = false;
            getImageButton.Enabled = false;
            messagesOffButton.Enabled = false;
            messagesOnButton.Enabled = false;
            viewRegistersButton.Enabled = false;
            txCheckbox.Enabled = false;
            rxCheckbox.Enabled = false;


        }
        private void disableRaceButtons2()
        {
            clearCourseButton.Enabled = false;
            loadCourseButton.Enabled = false;
            saveCourseButton.Enabled = false;
            sendCourseButton.Enabled = false;
        }

        private void changeStateStatus(bool transmitState)
        {
            switch (RobotRacer.mode)
            {
                case Mode.DEBUG:
                    if (!truckStarted)
                    {
                        truckState.Text = "Debug Stopped";
                        truckState.ForeColor = Color.Red;
                    }
                    else
                    {
                        truckState.Text = "Debug Started";
                        truckState.ForeColor = Color.Green;
                    }
                    if (transmitState) setCarState(Mode.DEBUG);
                    break;
                case Mode.JOYSTICK:
                case Mode.MANUAL:
                    if (!truckStarted)
                    {
                        truckState.Text = "Manual Stopped";
                        truckState.ForeColor = Color.Red;
                    }
                    else
                    {
                        truckState.Text = "Manual Started";
                        truckState.ForeColor = Color.Green;
                    }
                    if (transmitState) setCarState(Mode.MANUAL);
                    break;
                case Mode.RACE:
                    if (!truckStarted)
                    {
                        truckState.Text = "Ready To Race";
                        truckState.ForeColor = Color.Orange;
                    }
                    else
                    {
                        truckState.Text = "The Knack is Racing";
                        truckState.ForeColor = Color.Green;
                    }
                    if (transmitState) setCarState(Mode.RACE);
                    break;
                case Mode.DISABLED:
                default:
                    truckState.Text = "Truck Disabled";
                    truckState.ForeColor = Color.Red;
                    if (transmitState) setCarState(Mode.DISABLED);
                    break;
            }
        }

        //send the car its updated mode
        private void setCarState(Mode mode)
        {
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
            header.Subtype = (byte)SerialUtils.CommandSubtype.MODE;
            byte[] data = new byte[1];
            data[0] = Convert.ToByte(mode);
            serialPort.transmitByteData(header, data);
        }

        private void saveCourseButton_Click(object sender, EventArgs e)
        {
            Course course = new Course(courseTextBox.Text);
            if (course.isCourseValid())
            {
                saveCourseDialog.ShowDialog();
            }
            else
            {
                MessageBox.Show("Course Data is Invalid");
            }
        }

        private void loadCourseButton_Click(object sender, EventArgs e)
        {
            loadCourseDialog.ShowDialog();
        }

        private void saveCourseDialog_FileOk(object sender, CancelEventArgs e)
        {
            TextWriter tw = new StreamWriter(saveCourseDialog.FileName);
            tw.WriteLine(courseTextBox.Text);
            tw.Close();
        }

        private void courseGraphicTab_Click(object sender, EventArgs e)
        {
            currentCourse = new Course(courseTextBox.Text);
        }

        private void loadCourseDialog_FileOk(object sender, CancelEventArgs e)
        {
            StreamReader sr = new StreamReader(loadCourseDialog.FileName);

            string currentCourseString = sr.ReadToEnd();
            sr.Close();
            currentCourse = new Course(currentCourseString);

            courseTextBox.Text = currentCourseString;
        }

        public void startButton_Click(object sender, EventArgs e)
        {
            if ((RobotRacer.mode == RobotRacer.Mode.RACE || RobotRacer.mode == RobotRacer.Mode.DEBUG) && !RobotRacer.courseLoaded)
            {
                MessageBox.Show("ABORT! ABORT! No course data is loaded on the truck.");
            }
            else
            {
                SerialHeader header = new SerialHeader();
                header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
                header.Subtype = (byte)SerialUtils.CommandSubtype.STARTSTOP;
                serialPort.transmitIntData(header, 1);

                truckStarted = true;
                heartbeatTimer.Start();
                changeStateStatus(false);
            }

            if (RobotRacer.mode == RobotRacer.Mode.RACE)
            {
                if (RobotRacer.courseLoaded)
                {
                    disableRaceButtons2();
                }
            }
            else if (RobotRacer.mode == RobotRacer.Mode.JOYSTICK) // start updating positions
                tmrUpdateStick.Enabled = true;
        }

        public void stopButton_Click(object sender, EventArgs e)
        {
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
            header.Subtype = (byte)SerialUtils.CommandSubtype.STARTSTOP; //Sending STARTSTOP with value 0 Stops the car
            serialPort.transmitIntData(header, 0);

            truckStarted = false;
            tmrUpdateStick.Enabled = false;
            heartbeatTimer.Stop();
            changeStateStatus(false);
        }

        private void heartbeatTimer_Tick(object sender, EventArgs e)
        {
            if (truckStarted)
            {
                heartbeatTimer.Start();
                SerialHeader header = new SerialHeader();
                header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
                header.Subtype = (byte)SerialUtils.CommandSubtype.HEARTBEAT;
                serialPort.transmitIntData(header, 1);
            }
            else
            {
                heartbeatTimer.Stop();
                SerialHeader header = new SerialHeader();
                header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
                header.Subtype = (byte)SerialUtils.CommandSubtype.STARTSTOP; //send STARTSTOP with value 0 to stop the car
                serialPort.transmitIntData(header, 0);
            }
        }

        private void txCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (txCheckbox.Checked)
                SerialUtils.showTx = true;
            else
                SerialUtils.showTx = false;
        }

        private void rxCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            if (rxCheckbox.Checked)
                SerialUtils.showRx = true;
            else
                SerialUtils.showRx = false;
        }

        private void eStopButton_Click(object sender, EventArgs e)
        {
            transmitAllStop();
            heartbeatTimer.Stop();
            rawImage.stopImageCleanup(); //stop loading the image (if we are)
            truckStarted = false;
            tmrUpdateStick.Enabled = false;
            stopTransmittingBinaryFile();
            changeStateStatus(false);
        }

        private void messagesOnButton_Click(object sender, EventArgs e)
        {
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
            header.Subtype = (byte)SerialUtils.CommandSubtype.MESSAGES;
            serialPort.transmitIntData(header, 1);
        }

        private void messagesOffButton_Click(object sender, EventArgs e)
        {
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
            header.Subtype = (byte)SerialUtils.CommandSubtype.MESSAGES;
            serialPort.transmitIntData(header, 0);
        }

        private void viewRegistersButton_Click(object sender, EventArgs e)
        {
            if (!registerTabControl.SelectedTab.Name.Equals("registerTab"))
            {
                registerTabControl.SelectTab("registerTab");
            }
            registerViewer.requestRegisterValues();
        }

        private void clearBtn_Click(object sender, EventArgs e)
        {
            truckMessageBox.Text = "";
        }

        public void speedSlider_Scroll(object sender, EventArgs e)
        {
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
            header.Subtype = (byte)SerialUtils.CommandSubtype.VELOCITY_MULT;
            //the slider ranges from 0 to 10 in it's value. (default at 5)
            //this modifies the car's speed DEVISOR (1/x)
            //the car starts at 8 (1/8), and should decrease value to go faster, and increase to go slower
            serialPort.transmitIntData(header, (5 - speedSlider.Value) + 8);
        }

        public void steeringSlider_Scroll(object sender, EventArgs e)
        {
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
            header.Subtype = (byte)SerialUtils.CommandSubtype.STEERING_TRIM;
            //the slider ranges from 0 to 10 in it's value. (default at 5)
            serialPort.transmitIntData(header, (-steeringSlider.Value));
        }

        private void getImageButton_Click(object sender, EventArgs e)
        {
            if (ddbImageType.SelectedItem == null) return;
            SerialUtils.ImageTypes imageType = ddbImageType.SelectedItem.Equals("RGB565") ? SerialUtils.ImageTypes.RGB565 :
                ddbImageType.SelectedItem.Equals("HSV") ? SerialUtils.ImageTypes.HSV :
                ddbImageType.SelectedItem.Equals("SEG_Or") ? SerialUtils.ImageTypes.SEGMENTED_1 :
                ddbImageType.SelectedItem.Equals("SEG_Gr") ? SerialUtils.ImageTypes.SEGMENTED_2 :
                ddbImageType.SelectedItem.Equals("GREYSCALE_Or") ? SerialUtils.ImageTypes.SEGMENTED8_1 :
                ddbImageType.SelectedItem.Equals("GREYSCALE_Gr") ? SerialUtils.ImageTypes.SEGMENTED8_2 :
                SerialUtils.ImageTypes.RGB565;
            if (RobotRacer.mode == Mode.RACE)
            {
                //getting HSV, RGB, and GREYSCALE images is disabled
                if (imageType == SerialUtils.ImageTypes.HSV ||
                    imageType == SerialUtils.ImageTypes.RGB565 ||
                    imageType == SerialUtils.ImageTypes.SEGMENTED8_1 ||
                    imageType == SerialUtils.ImageTypes.SEGMENTED8_2)
                {
                    return;
                }
            }
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.IMAGE;
            header.Subtype = (byte)((byte)SerialUtils.ImageSubtype.CAPTURE_AND_TRANSMIT << 4 | Convert.ToByte(imageType));
            serialPort.transmitData(header);
        }

        private void startVideoButton_Click(object sender, EventArgs e)
        {
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
            header.Subtype = (byte)SerialUtils.CommandSubtype.VIDEO;
            serialPort.transmitIntData(header, 1);
        }

        private void stopVideoButton_Click(object sender, EventArgs e)
        {
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
            header.Subtype = (byte)SerialUtils.CommandSubtype.VIDEO;
            serialPort.transmitIntData(header, 0);
        }

        //offset is the number of bytes to where the data begins
        internal void addImageData(int col, int row, byte[] data, int offset, SerialUtils.ImageTypes imageType)
        {
            if (imageType.Equals(SerialUtils.ImageTypes.RGB565))
                rawImage.AddRGBData(col, row, data, offset);
            else if (imageType.Equals(SerialUtils.ImageTypes.SEGMENTED_1) || imageType.Equals(SerialUtils.ImageTypes.SEGMENTED_2))
                rawImage.AddBINARYData(col, row, data, offset);
            else if (imageType.Equals(SerialUtils.ImageTypes.SEGMENTED8_1) || imageType.Equals(SerialUtils.ImageTypes.SEGMENTED8_2))
                rawImage.AddGREYSCALEData(col, row, data, offset);
            else if (imageType.Equals(SerialUtils.ImageTypes.HSV))
                rawImage.AddHSVData(col, row, data, offset);
        }
        internal void transmitIntData(SerialHeader header, int data)
        {
            serialPort.transmitIntData(header, data);
        }
        internal void newImageCapture(int width, int height, SerialUtils.ImageTypes imageType)
        {
            rawImage.newCapture(width, height, imageType);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            serialTextBox.Text = "";
            DataPacket.packetsLost_TX = 0;
            DataPacket.packetsLost_RX = 0;
            TheKnack.racer.writeToPacketLossBtn("pktLoss(t/r): " + (SerialUtils.requireAcknowledge ? DataPacket.packetsLost_TX.ToString() : "?") + "/" + DataPacket.packetsLost_RX.ToString());
        }

        private void saveImageButton_Click(object sender, EventArgs e)
        {
            saveImageDialog.ShowDialog();
        }

        private void saveImageDialog_FileOk(object sender, CancelEventArgs e)
        {
            rawImage.saveImage(saveImageDialog.FileName);
        }


        private void lstBoxUART_SelectedIndexChanged(object sender, EventArgs e)
        {
            string choice = lstBoxUART.Text;
            if (choice.Length > 0) serialPort.changeport(choice);
        }

        public void transmitAllStop()
        {
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.COMMAND;
            header.Subtype = (byte)SerialUtils.CommandSubtype.ALLSTOP;
            serialPort.transmitData(header);
        }

        private void sendPidButton_Click(object sender, EventArgs e)
        {
            try
            {
                float kP = float.Parse(kpTextbox.Text);
                float kD = float.Parse(kdTextbox.Text);
                float kI = float.Parse(kiTextbox.Text);
                float iMax = float.Parse(iMaxTextbox.Text);
                float iMin = float.Parse(iMinTextbox.Text);

                SerialHeader header = new SerialHeader();
                header.Type = (byte)SerialUtils.TransmissionType.REGISTER;
                header.Subtype = (byte)SerialUtils.RegisterSubtype.SET_FLOAT;

                RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.KP_REG_FLOAT, kP);
                RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.KD_REG_FLOAT, kD);
                RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.KI_REG_FLOAT, kI);
                RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.I_MIN_FLOAT, iMin);
                RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.I_MAX_FLOAT, iMax);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid format in PID constants.");
            }
        }

        private void resetPIDValuesButton_Click(object sender, EventArgs e)
        {
            kpTextbox.Text = "55.0";
            kdTextbox.Text = "0.05";
            kiTextbox.Text = "1.0";
            iMaxTextbox.Text = "1.5";
            iMinTextbox.Text = "-1.5";
        }

        private void runButton_Click(object sender, EventArgs e)
        {
            bool meters = false;
            float desiredVelocity = float.Parse(desiredVelocityTextbox.Text);
            int desiredAngle = int.Parse(turningTextbox.Text);
            int numMillisecondsToRun = int.Parse(numMillisecondsToRunTextbox.Text) / 10;
            float numMetersToRun = float.Parse(metersToRunTextbox.Text);

            if (numMetersToRun != 0 && numMillisecondsToRun != 0)
            {
                MessageBox.Show("Pick either meters or seconds to run");
                return;
            }
            else if (numMetersToRun != 0)
            {
                meters = true;
            }

            try
            {
                SerialHeader header = new SerialHeader();
                header.Type = (byte)SerialUtils.TransmissionType.REGISTER;
                header.Subtype = (byte)SerialUtils.RegisterSubtype.SET_FLOAT;

                RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.DESIRED_VELOCITY_FLOAT, desiredVelocity);

                header.Subtype = (byte)SerialUtils.RegisterSubtype.SET_INT;
                RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.DESIRED_ANGLE_INT, desiredAngle);

                if (!meters)
                {
                    RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.NUM_MILLISECONDS_TO_RUN_INT, numMillisecondsToRun);
                }
                else
                {
                    header.Subtype = (byte)SerialUtils.RegisterSubtype.SET_FLOAT;
                    RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.NUM_METERS_TO_RUN_FLOAT, numMetersToRun);
                }

            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid format in PID constants.");
            }
        }

        private void btn_packetLoss_Click(object sender, EventArgs e)
        {
            if (SerialUtils.requireAcknowledge)
            {
                SerialUtils.requireAcknowledge = false;
                btn_packetLoss.BackColor = Color.LightCoral;
            }
            else
            {
                SerialUtils.requireAcknowledge = true;
                btn_packetLoss.BackColor = Color.LightGreen;
            }
            TheKnack.racer.writeToPacketLossBtn("pktLoss(t/r): " + (SerialUtils.requireAcknowledge ? DataPacket.packetsLost_TX.ToString() : "?") + "/" + DataPacket.packetsLost_RX.ToString());
        }

        private void txt_binaryFileName_MouseClicked(object sender, EventArgs e)
        {
            loadBinaryDialog.ShowDialog();
        }

        private void loadBinaryDialog_FileOk(object sender, CancelEventArgs e)
        {
            changeBinaryFileName(loadBinaryDialog.FileName);
            //System.IO.Directory.SetCurrentDirectory(runtimeDirectory);
        }

        private void btn_transmitBinary_Click(object sender, EventArgs e)
        {
            if (loadBinaryDialog.FileName.Length == 0)
            {
                return; //this is where the elf filename is stored
            }
            String elfFileName = loadBinaryDialog.FileName;
            String arguments = "-O srec \"" + elfFileName + "\" executable.bin";

            Process p = null;
            try
            {
                p = new Process();
                p.StartInfo.WorkingDirectory = runtimeDirectory;
                p.StartInfo.FileName = "ppc-objcopy";
                p.StartInfo.Arguments = arguments;
                p.StartInfo.CreateNoWindow = true;
                p.Start();
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception Occurred running ppc-objcopy :{0},{1}",
                          ex.Message, ex.StackTrace.ToString());
            }
            Console.WriteLine("Converted ELF File");
            transmitBinaryFile();
        }

        private string binaryFileName = "executable.bin";
        private byte[] binaryFileData = null;
        private Int32 currentDataOffset = 0;
        private static Int16 dataChunkSize = 256;
        private Int32 carDataLocationStart = 0;
        private bool transmittingBinaryBegun = false;

        public void stopTransmittingBinaryFile()
        {
            serialPort.transmitBinary.Stop();
            transmittingBinaryBegun = false;
        }
        private void transmitBinaryFile()
        {
            //ask the car for the location to transmit to
            transmittingBinaryBegun = false;
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.DATA_TRANSFER;
            header.Subtype = (byte)SerialUtils.DataTransferSubtype.REQUEST_BIN;
            serialPort.transmitIntData(header, (Int32)0x0);
            serialPort.transmitBinary.Start();
        }

        //this is called when the car responds with carDataLoc
        public void startTransmittingBinaryFile(Int32 carDataLoc)
        {
            //initialize variables used for transferring
            if (transmittingBinaryBegun) return; //we've already begun
            binaryFileName = TheKnack.racer.runtimeDirectory + "\\executable.bin";
            carDataLocationStart = carDataLoc;
            transmittingBinaryBegun = true;
            currentDataOffset = 0;
            binaryFileData = System.IO.File.ReadAllBytes(binaryFileName);
            continueTransmittingBinaryFile();
        }
        public void incrementBinaryFileAndContinue(byte[] data)
        {
            if (carDataLocationStart + currentDataOffset != ((int)data[1] << 24 | data[2] << 16 | data[3] << 8 | data[4]))
            {
                return;//make sure we don't retransmit something that was sent successfully already
            }
            currentDataOffset += dataChunkSize; //we completed sending the last data to car, so increment
            if (currentDataOffset < binaryFileData.Length)
            {
                continueTransmittingBinaryFile(); //only continue if there is data left
            }
            else
            {
                serialPort.transmitBinary.Stop(); //we've got to the end, so stop transmitting
                Console.WriteLine("finished transmitting Binary file");
                SerialHeader header = new SerialHeader();
                header.Type = (byte)SerialUtils.TransmissionType.DATA_TRANSFER;
                header.Subtype = (byte)SerialUtils.DataTransferSubtype.RUN_BOOTLOADER;
                header.RequestAck = true;
                serialPort.transmitData(header);
            }
        }
        public void continueTransmittingBinaryFile()
        {
            if (!transmittingBinaryBegun)
            {
                return;
            }
            if (binaryFileData == null || binaryFileData.Length == 0) return;
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.DATA_TRANSFER;
            header.Subtype = (byte)SerialUtils.DataTransferSubtype.WRITE_DATA;
            int currentChunkSize = Math.Min(dataChunkSize, binaryFileData.Length - currentDataOffset);
            byte[] data = new byte[currentChunkSize + 4];
            byte[] newCarLoc = SerialUtils.littleToBigEndian(carDataLocationStart + currentDataOffset);
            data[0] = newCarLoc[0];
            data[1] = newCarLoc[1];
            data[2] = newCarLoc[2];
            data[3] = newCarLoc[3];
            serialPort.transmitByteData(header, data);
        }

        private void btn_bootload_Click(object sender, EventArgs e)
        {
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.DATA_TRANSFER;
            header.Subtype = (byte)SerialUtils.DataTransferSubtype.RUN_BOOTLOADER;
            header.RequestAck = true;
            serialPort.transmitData(header);
        }


        //parses data received from the car into a pylon array
        public Pylon[] processPylonData(int numPylons, byte[] data)
        {
            Pylon[] pylons = new Pylon[numPylons];
            for (int i = 0; i < numPylons; i++)
            {
                byte[] pylonBytes = new byte[10];
                for (int j = 0; j < 10; j++)
                {
                    pylonBytes[j] = data[i * 10 + j];
                }

                ushort height = BitConverter.ToUInt16(SerialUtils.reverseBytes(pylonBytes, 0, 1), 0);
                ushort width = BitConverter.ToUInt16(SerialUtils.reverseBytes(pylonBytes, 2, 3), 0);
                ushort center = BitConverter.ToUInt16(SerialUtils.reverseBytes(pylonBytes, 4, 5), 0);
                ushort middle = BitConverter.ToUInt16(SerialUtils.reverseBytes(pylonBytes, 6, 7), 0);
                PylonColor color = (PylonColor)pylonBytes[8];
                byte probability = pylonBytes[9];

                pylons[i] = new Pylon(height, width, center, middle, color, probability);
            }
            return pylons;
        }

        public void updateProcessedImage(int numPylons, byte[] data)
        {
            Pylon[] pylons = processPylonData(numPylons, data);

            if (TheKnack.xnaGame != null && TheKnack.xnaGame.getXnaDisplay().PylonGraphics() != null)
            {
                TheKnack.xnaGame.getXnaDisplay().PylonGraphics().setVisiblePylonData(
                    pylons,
                    TheKnack.xnaGame.getXnaDisplay().KnackTruck().ModelPosition(),
                    TheKnack.xnaGame.getXnaDisplay().KnackTruck().ModelRotation()); //xnaRoboRacerGame1 is attached to the game when running
            }

            if (TheKnack.game != null && TheKnack.game.getIsRunning())
            {
                TheKnack.game.setDoNotUpdate(true);
                TheKnack.game.getProcessedImage().clearPylons();

                for (int i = 0; i < numPylons; i++)
                {
                    TheKnack.game.getProcessedImage().addPylon(pylons[i]);

                    Console.WriteLine(pylons[i].toString(i));
                }

                TheKnack.game.getProcessedImage().setPylonsChanged();
                TheKnack.game.setDoNotUpdate(false);
            }
            else
            {
                processedImage.clearPylons();
                for (int i = 0; i < numPylons; i++)
                {
                    processedImage.addPylon(pylons[i]);

                    Console.WriteLine(pylons[i].toString(i));
                }
                processedImage.redrawImage();
            }
        }

        private void ddbImageType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!ddbImageType.Items.Contains(ddbImageType.SelectedItem))
            {
                ddbImageType.SelectedItem = 0;
            }
        }

        private void sendCourseButton_Click(object sender, EventArgs e)
        {
            Course course = new Course(courseTextBox.Text);
            if (course.getNumPoints() < 2)
            {
                MessageBox.Show("Not enough angles in course data.");
            }
            else
            {
                if (!course.isCourseValid())
                {
                    MessageBox.Show("Course Angles add up to " + course.getTotalDegrees() + " degrees (Off by " + course.getDegreesOff() + ").");
                }

                RobotRacer.serialPort.transmitCourseData(course);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {

            Course course = new Course(courseTextBox.Text);
            if (course.getNumPoints() < 2)
            {
                MessageBox.Show("Not enough angles in course data.");
            }
            else
            {
                if (!course.isCourseValid())
                {
                    MessageBox.Show("Course Angles add up to " + course.getTotalDegrees() + " degrees (Off by " + course.getDegreesOff() + ").");
                }

                RobotRacer.currentCourse = course;
                RobotRacer.serialPort.transmitCourseData(course);
                //RobotRacer.mode = Mode.JOYSTICK;
                //startButton.Enabled = true;
                //stopButton.Enabled = true;
                //changeStateStatus();
                TheKnack.game = new Game();
            }

        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                float blindTurn = float.Parse(blindTurnTextbox.Text);
                float blindStraight = float.Parse(blindStraightTextbox.Text);
                float straightSpeed = float.Parse(straightSpeedTextbox.Text);
                float turnRadius = float.Parse(turnRadiusTextbox.Text);

                int turnAngle = int.Parse(turnAngleTextbox.Text);
                int findRange = int.Parse(findRangeTextbox.Text);

                SerialHeader header = new SerialHeader();
                header.Type = (byte)SerialUtils.TransmissionType.REGISTER;
                header.Subtype = (byte)SerialUtils.RegisterSubtype.SET_FLOAT;

                RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.BLIND_TURN_FLOAT, blindTurn);
                RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.BLIND_STRAIGHT_FLOAT, blindStraight);
                RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.STRAIGHT_SPEED_FLOAT, straightSpeed);
                RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.TURN_RADIUS_FLOAT, turnRadius);

                header.Subtype = (byte)SerialUtils.RegisterSubtype.SET_INT;
                RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.TURN_ANGLE_INT, turnAngle);
                RobotRacer.serialPort.transmitRegData(header, (ushort)Register.RegisterIds.FIND_RANGE_INT, findRange);

            }
            catch (FormatException ex) { MessageBox.Show("Invalid Number Format" + ex.Message); }
        }

        private void turnRadiusTextbox_TextChanged(object sender, EventArgs e)
        {
            float WHEEL_BASE = 0.272f;
            turnAngleTextbox.Text = Math.Round((2 * WHEEL_BASE / float.Parse(turnRadiusTextbox.Text) * 180.0 / Math.PI)) + "";
        }

        private void buttonGetCriticalImage_Click(object sender, EventArgs e)
        {
            if (ddbImageType.SelectedItem == null) return;
            SerialUtils.ImageTypes imageType = ddbImageType.SelectedItem.Equals("RGB565") ? SerialUtils.ImageTypes.RGB565 :
                ddbImageType.SelectedItem.Equals("HSV") ? SerialUtils.ImageTypes.HSV :
                ddbImageType.SelectedItem.Equals("SEG_Or") ? SerialUtils.ImageTypes.SEGMENTED_1 :
                ddbImageType.SelectedItem.Equals("SEG_Gr") ? SerialUtils.ImageTypes.SEGMENTED_2 :
                ddbImageType.SelectedItem.Equals("GREYSCALE_Or") ? SerialUtils.ImageTypes.SEGMENTED8_1 :
                ddbImageType.SelectedItem.Equals("GREYSCALE_Gr") ? SerialUtils.ImageTypes.SEGMENTED8_2 :
                SerialUtils.ImageTypes.RGB565;
            if (RobotRacer.mode == Mode.RACE)
            {
                //getting HSV, RGB, and GREYSCALE images is disabled
                if (imageType == SerialUtils.ImageTypes.HSV ||
                    imageType == SerialUtils.ImageTypes.RGB565 ||
                    imageType == SerialUtils.ImageTypes.SEGMENTED8_1 ||
                    imageType == SerialUtils.ImageTypes.SEGMENTED8_2)
                {
                    return;
                }
            }
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.IMAGE;
            header.Subtype = (byte)((byte)SerialUtils.ImageSubtype.TRANSMIT_RECENT_CRITICAL << 4 | Convert.ToByte(imageType));
            serialPort.transmitData(header);
        }

        private void DynamicHSV_rx_all_Click(object sender, EventArgs e)
        {
            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.REGISTER;
            header.Subtype = (byte)((byte)SerialUtils.RegisterSubtype.GET_DYNAMIC_HSV);
            RobotRacer.serialPort.transmitData(header);

            /*HSVsettings dynamicsettings = new HSVsettings(dynamicHSVTextBox.Text);
            //transmit only the current values to the selected range (orange or green, for the current selected angle)
            byte[] data = new byte[14]; //plus two bytes for convolution thresholds.
            //populate the data array with the data from dynamicsettings
            int[] intarray = new int[14];
            dynamicsettings.getLine(intarray, selectHSVline1.SelectedIndex);
            for (int i = 0; i < 14; i++)
            {
                data[i] = (byte)intarray[i];
            }

            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.REGISTER;
            header.Subtype = (byte)((byte)SerialUtils.RegisterSubtype.SET_DYNAMIC_HSV);
            RobotRacer.serialPort.transmitByteData(header, data);*/
        }

    }
 
    
    public class StateVariables
    {
        float velocity;
        Int32 sentSteeringAngle;
        short headingAngle;

        byte pylonNum;
        byte carState;
        float distanceTraveled;
        byte angleToPylon;
        float distanceToPylon;
        float distanceToBlind;

        byte hwFPS;
        byte swFPS;

        public StateVariables(){

        }

        public byte PylonNumber
        {
            get { return pylonNum; }
            set { pylonNum = value; }
        }

        public byte SwFPS
        {
            get { return swFPS; }
            set { swFPS = value; }
        }
        public byte HwFPS
        {
            get { return hwFPS; }
            set { hwFPS = value; }
        }

        public byte CarState
        {
            get { return carState; }
            set { carState = value; }
        }

        public float DistanceToPylon
        {
            get { return distanceToPylon; }
            set { distanceToPylon = value; }
        }

        public float DistanceTraveled
        {
            get { return distanceTraveled; }
            set { distanceTraveled = value; }
        }

        public float DistanceToBlind
        {
            get { return distanceToBlind; }
            set { distanceToBlind = value; }
        }

        public byte AngleToPylon
        {
            get { return angleToPylon; }
            set { angleToPylon = value; }
        }

        public float Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public Int32 SentSteeringAngle
        {
            get { return sentSteeringAngle; }
            set { sentSteeringAngle = value; }
        }

        public short HeadingAngle
        {
            get { return headingAngle; }
            set { headingAngle = value; }
        }
    }
}
