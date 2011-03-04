using System.Linq;
using System.Windows.Forms;
using System;
using System.Drawing;

namespace Robot_Racers
{
    partial class RobotRacer
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        //private bool begunDisposing = false;
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            /*if (!begunDisposing)
            {
                begunDisposing = true;
               //stop the port from receiving data
                this.truckMessageBox.AppendText("closing Port\r\n");
                RobotRacer.serialPort.ClosePort();
                this.truckMessageBox.AppendText("port closed\r\n");
                System.Threading.Thread.Sleep(1000);
            }*/
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        //This is thread-safe
        public void writeToSerialTextBox(string text, bool isTrans)
        {
            string str_toPrint = (isTrans ? "Tx: " : "Rx: ") + text + "\r\n\r\n";
            if (this.serialTextBox.InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                SetTextCallback d = new SetTextCallback(setTextToSerialTextBox);
                try //and this is a hack to prevent application from being weird when it's closed while
                //lots of data is being transmitted
                {
                    this.Invoke
                        (d, new object[] { str_toPrint, isTrans });
                }
                catch (System.Exception ex) { Console.WriteLine(ex.Message); }
            }
            else
            {
                // It's on the same thread, no need for Invoke
                this.serialTextBox.AppendText(str_toPrint);
            }
        }

        private void setTextToSerialTextBox(string text, bool isTrans)
        {
            serialTextBox.AppendText(text);
        }

        //This is thread-safe
        public void writeToTruckMessageBox(string text, bool printNewLine)
        {
            if (this.serialTextBox.InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                SetTextCallback d = new SetTextCallback(setTextToTruckMessageBox);
                try //and this is a hack to prevent application from being weird when it's closed while
                //lots of data is being transmitted
                {
                    this.Invoke(d, new object[] { text, printNewLine });
                }
                catch (System.Exception ex) { Console.WriteLine("Error in invoke in writeToTruckMessageBox: " + ex.Message); }
            }
            else
            {
                // It's on the same thread, no need for Invoke
                this.truckMessageBox.AppendText(text + (printNewLine?"\r\n":""));
            }
        }
        private void setTextToTruckMessageBox(string text, bool printNewLine)
        {
            truckMessageBox.AppendText(text + (printNewLine ? "\r\n" : ""));
        }
        //This is thread-safe
        public void writeToPacketLossBtn(string text)
        {
            if (this.btn_packetLoss.InvokeRequired)
            {
                // It's on a different thread, so use Invoke.
                SetTextCallback2 d = new SetTextCallback2(setTextToPacketLossBtn);
                try //and this is a hack to prevent application from being weird when it's closed while
                //lots of data is being transmitted
                {
                    this.Invoke(d, new object[] { text });
                }
                catch (System.Exception ex) { Console.WriteLine("Error in invoke in writeToPacketLossBtn: " + ex.Message); }
            }
            else
            {
                // It's on the same thread, no need for Invoke
                this.btn_packetLoss.Text = text;
            }
        }
        private void setTextToPacketLossBtn(string text)
        {
            btn_packetLoss.Text = text;
        }


        public void updateStateVariables()
        {
            if (this.truckVelocityTextbox.InvokeRequired )
            {
                SetTextCallback3 d = new SetTextCallback3(updateStateVariablesCallback);
                try 
                {
                    this.Invoke(d, new object[] { });
                }
                catch (System.Exception ex) { Console.WriteLine("Error in invoke in updateStateVariablesCallback: " + ex.Message); }
            }
            else
            {
                updateStateVariablesCallback();
            }
        }
        private void updateStateVariablesCallback()
        {
            this.truckVelocityTextbox.Text = String.Format("{0:F2}", RobotRacer.stateVariables.Velocity) + " m/s";

            String stateText = "Unknown State";
            if(RobotRacer.stateVariables.CarState == (byte)RobotRacer.NavDriveMode.GATHER){
                stateText = "Gather";
            }
            else if(RobotRacer.stateVariables.CarState == (byte)RobotRacer.NavDriveMode.CORRECT){
                stateText = "Correct";
            }
            else if(RobotRacer.stateVariables.CarState == (byte)RobotRacer.NavDriveMode.BLIND_STRAIGHT){
                stateText = "Blind Straight";
            }
            else if(RobotRacer.stateVariables.CarState == (byte)RobotRacer.NavDriveMode.BLIND_TURN){
                stateText = "Blind Turn";
            }
            
            this.carStateLabel.Text = stateText;
            this.distanceTraveledLabel.Text = String.Format("{0:F2}",RobotRacer.stateVariables.DistanceTraveled) + " m";
            this.distanceToBlindLabel.Text = String.Format("{0:F2}", RobotRacer.stateVariables.DistanceToBlind) + " m";

            this.lblFPS_HW.Text = RobotRacer.stateVariables.HwFPS + " fps";
            this.lblFPS_SW.Text = RobotRacer.stateVariables.SwFPS + " fps";
        }

        public void changeBinaryFileName(String newName)
        {
            this.txt_binaryFileName.Text = newName.Length > 45 ? newName.Substring(0,3)+"[...]" + newName.Substring(newName.Length - 45) : newName;
            this.loadBinaryDialog.FileName = newName;
        }
        public String runtimeDirectory = System.IO.Directory.GetCurrentDirectory();

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tmrUpdateStick = new System.Windows.Forms.Timer(this.components);
            this.startButton = new System.Windows.Forms.Button();
            this.stopButton = new System.Windows.Forms.Button();
            this.clearCourseButton = new System.Windows.Forms.Button();
            this.loadCourseButton = new System.Windows.Forms.Button();
            this.saveCourseButton = new System.Windows.Forms.Button();
            this.sendCourseButton = new System.Windows.Forms.Button();
            this.modeComboBox = new System.Windows.Forms.ComboBox();
            this.courseTabControl = new System.Windows.Forms.TabControl();
            this.courseTextTab = new System.Windows.Forms.TabPage();
            this.lblBytesPerSecond2 = new System.Windows.Forms.Label();
            this.courseTextBox = new System.Windows.Forms.TextBox();
            this.serialTab = new System.Windows.Forms.TabPage();
            this.lblBytesPerSecond_TotalPackets = new System.Windows.Forms.Label();
            this.label58 = new System.Windows.Forms.Label();
            this.lblBytesPerSecond_Ack = new System.Windows.Forms.Label();
            this.lblBytesPerSecond_Text = new System.Windows.Forms.Label();
            this.lblBytesPerSecond_Primative = new System.Windows.Forms.Label();
            this.lblBytesPerSecond_State = new System.Windows.Forms.Label();
            this.lblBytesPerSecond_Command = new System.Windows.Forms.Label();
            this.lblBytesPerSecond_Image = new System.Windows.Forms.Label();
            this.lblBytesPerSecond_Course = new System.Windows.Forms.Label();
            this.lblBytesPerSecond_Register = new System.Windows.Forms.Label();
            this.lblBytesPerSecond_DataTrans = new System.Windows.Forms.Label();
            this.label57 = new System.Windows.Forms.Label();
            this.label56 = new System.Windows.Forms.Label();
            this.label55 = new System.Windows.Forms.Label();
            this.label54 = new System.Windows.Forms.Label();
            this.label53 = new System.Windows.Forms.Label();
            this.label52 = new System.Windows.Forms.Label();
            this.label51 = new System.Windows.Forms.Label();
            this.label50 = new System.Windows.Forms.Label();
            this.label49 = new System.Windows.Forms.Label();
            this.lblBytesPerSecond_Total = new System.Windows.Forms.Label();
            this.btn_packetLoss = new System.Windows.Forms.Button();
            this.clearSerialButton = new System.Windows.Forms.Button();
            this.rxCheckbox = new System.Windows.Forms.CheckBox();
            this.txCheckbox = new System.Windows.Forms.CheckBox();
            this.serialTextBox = new System.Windows.Forms.TextBox();
            this.uartControlTab = new System.Windows.Forms.TabPage();
            this.lstBoxUART = new System.Windows.Forms.ListBox();
            this.speedSlider = new System.Windows.Forms.TrackBar();
            this.steeringSlider = new System.Windows.Forms.TrackBar();
            this.speedSliderLabel = new System.Windows.Forms.Label();
            this.steeringSliderLabel = new System.Windows.Forms.Label();
            this.imageTabControl = new System.Windows.Forms.TabControl();
            this.rawImageTab = new System.Windows.Forms.TabPage();
            this.rawImage = new Robot_Racers.RawImage();
            this.processedImageTab = new System.Windows.Forms.TabPage();
            this.getImageButton = new System.Windows.Forms.Button();
            this.saveImageButton = new System.Windows.Forms.Button();
            this.truckMessageBox = new System.Windows.Forms.TextBox();
            this.truckMessageBoxLabel = new System.Windows.Forms.Label();
            this.stateLabel = new System.Windows.Forms.Label();
            this.speedLabel = new System.Windows.Forms.Label();
            this.truckState = new System.Windows.Forms.Label();
            this.truckVelocityTextbox = new System.Windows.Forms.Label();
            this.eStopButton = new System.Windows.Forms.Button();
            this.startVideoButton = new System.Windows.Forms.Button();
            this.messagesOnButton = new System.Windows.Forms.Button();
            this.messagesOffButton = new System.Windows.Forms.Button();
            this.viewRegistersButton = new System.Windows.Forms.Button();
            this.stopVideoButton = new System.Windows.Forms.Button();
            this.saveCourseDialog = new System.Windows.Forms.SaveFileDialog();
            this.loadCourseDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveHSVDialog = new System.Windows.Forms.SaveFileDialog();
            this.openHSVDialog = new System.Windows.Forms.OpenFileDialog();
            this.clearBtn = new System.Windows.Forms.Button();
            this.saveImageDialog = new System.Windows.Forms.SaveFileDialog();
            this.lblImageCoords = new System.Windows.Forms.Label();
            this.lblImageColorHex = new System.Windows.Forms.Label();
            this.lblImageColorBinary = new System.Windows.Forms.Label();
            this.btn_transmitBinary = new System.Windows.Forms.Button();
            this.txt_binaryFileName = new System.Windows.Forms.TextBox();
            this.loadBinaryDialog = new System.Windows.Forms.OpenFileDialog();
            this.btn_bootload = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ddbImageType = new System.Windows.Forms.ComboBox();
            this.button1 = new System.Windows.Forms.Button();
            this.label11 = new System.Windows.Forms.Label();
            this.lblFPS_HW = new System.Windows.Forms.Label();
            this.buttonGetCriticalImage = new System.Windows.Forms.Button();
            this.mouseControlPanel = new Robot_Racers.MouseControl();
            this.lblFPS_SW = new System.Windows.Forms.Label();
            this.label60 = new System.Windows.Forms.Label();
            this.pidTab = new System.Windows.Forms.TabPage();
            this.kpTextbox = new System.Windows.Forms.TextBox();
            this.kdTextbox = new System.Windows.Forms.TextBox();
            this.kiTextbox = new System.Windows.Forms.TextBox();
            this.sendPidButton = new System.Windows.Forms.Button();
            this.kpLabel = new System.Windows.Forms.Label();
            this.kdLabel = new System.Windows.Forms.Label();
            this.kiLabel = new System.Windows.Forms.Label();
            this.iMaxTextbox = new System.Windows.Forms.TextBox();
            this.iMinTextbox = new System.Windows.Forms.TextBox();
            this.iMaxLabel = new System.Windows.Forms.Label();
            this.iMinLabel = new System.Windows.Forms.Label();
            this.desiredVelocityTextbox = new System.Windows.Forms.TextBox();
            this.desiredVelocityLabel = new System.Windows.Forms.Label();
            this.resetPIDValuesButton = new System.Windows.Forms.Button();
            this.numMillisecondsToRunTextbox = new System.Windows.Forms.TextBox();
            this.numMillisecondsLabel = new System.Windows.Forms.Label();
            this.turningTextbox = new System.Windows.Forms.TextBox();
            this.turningLabel = new System.Windows.Forms.Label();
            this.metersToRunTextbox = new System.Windows.Forms.TextBox();
            this.distanceToRunLabel = new System.Windows.Forms.Label();
            this.orLabel = new System.Windows.Forms.Label();
            this.runButton = new System.Windows.Forms.Button();
            this.registerTab = new System.Windows.Forms.TabPage();
            this.navigationTab = new System.Windows.Forms.TabPage();
            this.label6 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.carStateLabel = new System.Windows.Forms.Label();
            this.distanceTraveledLabel = new System.Windows.Forms.Label();
            this.distanceToBlindLabel = new System.Windows.Forms.Label();
            this.blindStraightTextbox = new System.Windows.Forms.TextBox();
            this.blindTurnTextbox = new System.Windows.Forms.TextBox();
            this.straightSpeedTextbox = new System.Windows.Forms.TextBox();
            this.turnRadiusTextbox = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label18 = new System.Windows.Forms.Label();
            this.turnAngleTextbox = new System.Windows.Forms.TextBox();
            this.label19 = new System.Windows.Forms.Label();
            this.label20 = new System.Windows.Forms.Label();
            this.findRangeTextbox = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.registerTabControl = new System.Windows.Forms.TabControl();
            this.courseTabControl.SuspendLayout();
            this.courseTextTab.SuspendLayout();
            this.serialTab.SuspendLayout();
            this.uartControlTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.speedSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.steeringSlider)).BeginInit();
            this.imageTabControl.SuspendLayout();
            this.rawImageTab.SuspendLayout();
            this.pidTab.SuspendLayout();
            this.navigationTab.SuspendLayout();
            this.registerTabControl.SuspendLayout();
            this.SuspendLayout();
            // 
            // tmrUpdateStick
            // 
            this.tmrUpdateStick.Tick += new System.EventHandler(this.tmrUpdateStick_Tick);
            // 
            // startButton
            // 
            this.startButton.Enabled = false;
            this.startButton.Location = new System.Drawing.Point(772, 250);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(89, 23);
            this.startButton.TabIndex = 1;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // stopButton
            // 
            this.stopButton.Enabled = false;
            this.stopButton.Location = new System.Drawing.Point(772, 279);
            this.stopButton.Name = "stopButton";
            this.stopButton.Size = new System.Drawing.Size(89, 23);
            this.stopButton.TabIndex = 2;
            this.stopButton.Text = "Stop";
            this.stopButton.UseVisualStyleBackColor = true;
            this.stopButton.Click += new System.EventHandler(this.stopButton_Click);
            // 
            // clearCourseButton
            // 
            this.clearCourseButton.Location = new System.Drawing.Point(308, 69);
            this.clearCourseButton.Name = "clearCourseButton";
            this.clearCourseButton.Size = new System.Drawing.Size(89, 23);
            this.clearCourseButton.TabIndex = 3;
            this.clearCourseButton.Text = "Clear Course";
            this.clearCourseButton.UseVisualStyleBackColor = true;
            this.clearCourseButton.Click += new System.EventHandler(this.clearCourseButton_Click);
            // 
            // loadCourseButton
            // 
            this.loadCourseButton.AllowDrop = true;
            this.loadCourseButton.Location = new System.Drawing.Point(308, 10);
            this.loadCourseButton.Name = "loadCourseButton";
            this.loadCourseButton.Size = new System.Drawing.Size(89, 23);
            this.loadCourseButton.TabIndex = 4;
            this.loadCourseButton.Text = "Load Course";
            this.loadCourseButton.UseVisualStyleBackColor = true;
            this.loadCourseButton.Click += new System.EventHandler(this.loadCourseButton_Click);
            // 
            // saveCourseButton
            // 
            this.saveCourseButton.Location = new System.Drawing.Point(308, 39);
            this.saveCourseButton.Name = "saveCourseButton";
            this.saveCourseButton.Size = new System.Drawing.Size(89, 23);
            this.saveCourseButton.TabIndex = 5;
            this.saveCourseButton.Text = "Save Course";
            this.saveCourseButton.UseVisualStyleBackColor = true;
            this.saveCourseButton.Click += new System.EventHandler(this.saveCourseButton_Click);
            // 
            // sendCourseButton
            // 
            this.sendCourseButton.Location = new System.Drawing.Point(308, 98);
            this.sendCourseButton.Name = "sendCourseButton";
            this.sendCourseButton.Size = new System.Drawing.Size(89, 23);
            this.sendCourseButton.TabIndex = 6;
            this.sendCourseButton.Text = "Send Course";
            this.sendCourseButton.UseVisualStyleBackColor = true;
            this.sendCourseButton.Click += new System.EventHandler(this.sendCourseButton_Click);
            // 
            // modeComboBox
            // 
            this.modeComboBox.FormattingEnabled = true;
            this.modeComboBox.Items.AddRange(new object[] {
            "Disabled",
            "Debug Mode",
            "Manual Mode",
            "Race Mode",
            "Joystick Mode"});
            this.modeComboBox.Location = new System.Drawing.Point(772, 308);
            this.modeComboBox.Name = "modeComboBox";
            this.modeComboBox.Size = new System.Drawing.Size(89, 21);
            this.modeComboBox.TabIndex = 8;
            this.modeComboBox.Text = "Disabled";
            this.modeComboBox.SelectedIndexChanged += new System.EventHandler(this.modeComboBox_SelectedIndexChanged);
            // 
            // courseTabControl
            // 
            this.courseTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.courseTabControl.Controls.Add(this.courseTextTab);
            this.courseTabControl.Controls.Add(this.serialTab);
            this.courseTabControl.Controls.Add(this.uartControlTab);
            this.courseTabControl.Location = new System.Drawing.Point(468, 0);
            this.courseTabControl.Name = "courseTabControl";
            this.courseTabControl.SelectedIndex = 0;
            this.courseTabControl.Size = new System.Drawing.Size(452, 198);
            this.courseTabControl.TabIndex = 9;
            this.courseTabControl.SelectedIndexChanged += new System.EventHandler(this.courseTabControl_SelectedIndexChanged);
            // 
            // courseTextTab
            // 
            this.courseTextTab.Controls.Add(this.lblBytesPerSecond2);
            this.courseTextTab.Controls.Add(this.courseTextBox);
            this.courseTextTab.Controls.Add(this.clearCourseButton);
            this.courseTextTab.Controls.Add(this.loadCourseButton);
            this.courseTextTab.Controls.Add(this.saveCourseButton);
            this.courseTextTab.Controls.Add(this.sendCourseButton);
            this.courseTextTab.Location = new System.Drawing.Point(4, 22);
            this.courseTextTab.Name = "courseTextTab";
            this.courseTextTab.Padding = new System.Windows.Forms.Padding(3);
            this.courseTextTab.Size = new System.Drawing.Size(444, 172);
            this.courseTextTab.TabIndex = 0;
            this.courseTextTab.Text = "Course Text";
            this.courseTextTab.UseVisualStyleBackColor = true;
            // 
            // lblBytesPerSecond2
            // 
            this.lblBytesPerSecond2.AutoSize = true;
            this.lblBytesPerSecond2.Location = new System.Drawing.Point(311, 156);
            this.lblBytesPerSecond2.Name = "lblBytesPerSecond2";
            this.lblBytesPerSecond2.Size = new System.Drawing.Size(33, 13);
            this.lblBytesPerSecond2.TabIndex = 9;
            this.lblBytesPerSecond2.Text = "0 B/s";
            // 
            // courseTextBox
            // 
            this.courseTextBox.Location = new System.Drawing.Point(0, 0);
            this.courseTextBox.Multiline = true;
            this.courseTextBox.Name = "courseTextBox";
            this.courseTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.courseTextBox.Size = new System.Drawing.Size(305, 172);
            this.courseTextBox.TabIndex = 0;
            // 
            // serialTab
            // 
            this.serialTab.Controls.Add(this.lblBytesPerSecond_TotalPackets);
            this.serialTab.Controls.Add(this.label58);
            this.serialTab.Controls.Add(this.lblBytesPerSecond_Ack);
            this.serialTab.Controls.Add(this.lblBytesPerSecond_Text);
            this.serialTab.Controls.Add(this.lblBytesPerSecond_Primative);
            this.serialTab.Controls.Add(this.lblBytesPerSecond_State);
            this.serialTab.Controls.Add(this.lblBytesPerSecond_Command);
            this.serialTab.Controls.Add(this.lblBytesPerSecond_Image);
            this.serialTab.Controls.Add(this.lblBytesPerSecond_Course);
            this.serialTab.Controls.Add(this.lblBytesPerSecond_Register);
            this.serialTab.Controls.Add(this.lblBytesPerSecond_DataTrans);
            this.serialTab.Controls.Add(this.label57);
            this.serialTab.Controls.Add(this.label56);
            this.serialTab.Controls.Add(this.label55);
            this.serialTab.Controls.Add(this.label54);
            this.serialTab.Controls.Add(this.label53);
            this.serialTab.Controls.Add(this.label52);
            this.serialTab.Controls.Add(this.label51);
            this.serialTab.Controls.Add(this.label50);
            this.serialTab.Controls.Add(this.label49);
            this.serialTab.Controls.Add(this.lblBytesPerSecond_Total);
            this.serialTab.Controls.Add(this.btn_packetLoss);
            this.serialTab.Controls.Add(this.clearSerialButton);
            this.serialTab.Controls.Add(this.rxCheckbox);
            this.serialTab.Controls.Add(this.txCheckbox);
            this.serialTab.Controls.Add(this.serialTextBox);
            this.serialTab.Location = new System.Drawing.Point(4, 22);
            this.serialTab.Name = "serialTab";
            this.serialTab.Size = new System.Drawing.Size(588, 172);
            this.serialTab.TabIndex = 2;
            this.serialTab.Text = "Serial";
            this.serialTab.UseVisualStyleBackColor = true;
            // 
            // lblBytesPerSecond_TotalPackets
            // 
            this.lblBytesPerSecond_TotalPackets.AutoSize = true;
            this.lblBytesPerSecond_TotalPackets.Location = new System.Drawing.Point(439, 136);
            this.lblBytesPerSecond_TotalPackets.Name = "lblBytesPerSecond_TotalPackets";
            this.lblBytesPerSecond_TotalPackets.Size = new System.Drawing.Size(60, 13);
            this.lblBytesPerSecond_TotalPackets.TabIndex = 28;
            this.lblBytesPerSecond_TotalPackets.Text = "0 Packet/s";
            // 
            // label58
            // 
            this.label58.AutoSize = true;
            this.label58.Location = new System.Drawing.Point(431, 110);
            this.label58.Name = "label58";
            this.label58.Size = new System.Drawing.Size(29, 13);
            this.label58.TabIndex = 27;
            this.label58.Text = "Ack:";
            // 
            // lblBytesPerSecond_Ack
            // 
            this.lblBytesPerSecond_Ack.AutoSize = true;
            this.lblBytesPerSecond_Ack.Location = new System.Drawing.Point(466, 110);
            this.lblBytesPerSecond_Ack.Name = "lblBytesPerSecond_Ack";
            this.lblBytesPerSecond_Ack.Size = new System.Drawing.Size(33, 13);
            this.lblBytesPerSecond_Ack.TabIndex = 26;
            this.lblBytesPerSecond_Ack.Text = "0 B/s";
            // 
            // lblBytesPerSecond_Text
            // 
            this.lblBytesPerSecond_Text.AutoSize = true;
            this.lblBytesPerSecond_Text.Location = new System.Drawing.Point(466, 6);
            this.lblBytesPerSecond_Text.Name = "lblBytesPerSecond_Text";
            this.lblBytesPerSecond_Text.Size = new System.Drawing.Size(33, 13);
            this.lblBytesPerSecond_Text.TabIndex = 25;
            this.lblBytesPerSecond_Text.Text = "0 B/s";
            // 
            // lblBytesPerSecond_Primative
            // 
            this.lblBytesPerSecond_Primative.AutoSize = true;
            this.lblBytesPerSecond_Primative.Location = new System.Drawing.Point(466, 19);
            this.lblBytesPerSecond_Primative.Name = "lblBytesPerSecond_Primative";
            this.lblBytesPerSecond_Primative.Size = new System.Drawing.Size(33, 13);
            this.lblBytesPerSecond_Primative.TabIndex = 24;
            this.lblBytesPerSecond_Primative.Text = "0 B/s";
            // 
            // lblBytesPerSecond_State
            // 
            this.lblBytesPerSecond_State.AutoSize = true;
            this.lblBytesPerSecond_State.Location = new System.Drawing.Point(466, 32);
            this.lblBytesPerSecond_State.Name = "lblBytesPerSecond_State";
            this.lblBytesPerSecond_State.Size = new System.Drawing.Size(33, 13);
            this.lblBytesPerSecond_State.TabIndex = 23;
            this.lblBytesPerSecond_State.Text = "0 B/s";
            // 
            // lblBytesPerSecond_Command
            // 
            this.lblBytesPerSecond_Command.AutoSize = true;
            this.lblBytesPerSecond_Command.Location = new System.Drawing.Point(466, 45);
            this.lblBytesPerSecond_Command.Name = "lblBytesPerSecond_Command";
            this.lblBytesPerSecond_Command.Size = new System.Drawing.Size(33, 13);
            this.lblBytesPerSecond_Command.TabIndex = 22;
            this.lblBytesPerSecond_Command.Text = "0 B/s";
            // 
            // lblBytesPerSecond_Image
            // 
            this.lblBytesPerSecond_Image.AutoSize = true;
            this.lblBytesPerSecond_Image.Location = new System.Drawing.Point(466, 58);
            this.lblBytesPerSecond_Image.Name = "lblBytesPerSecond_Image";
            this.lblBytesPerSecond_Image.Size = new System.Drawing.Size(33, 13);
            this.lblBytesPerSecond_Image.TabIndex = 21;
            this.lblBytesPerSecond_Image.Text = "0 B/s";
            // 
            // lblBytesPerSecond_Course
            // 
            this.lblBytesPerSecond_Course.AutoSize = true;
            this.lblBytesPerSecond_Course.Location = new System.Drawing.Point(466, 71);
            this.lblBytesPerSecond_Course.Name = "lblBytesPerSecond_Course";
            this.lblBytesPerSecond_Course.Size = new System.Drawing.Size(33, 13);
            this.lblBytesPerSecond_Course.TabIndex = 20;
            this.lblBytesPerSecond_Course.Text = "0 B/s";
            // 
            // lblBytesPerSecond_Register
            // 
            this.lblBytesPerSecond_Register.AutoSize = true;
            this.lblBytesPerSecond_Register.Location = new System.Drawing.Point(466, 84);
            this.lblBytesPerSecond_Register.Name = "lblBytesPerSecond_Register";
            this.lblBytesPerSecond_Register.Size = new System.Drawing.Size(33, 13);
            this.lblBytesPerSecond_Register.TabIndex = 19;
            this.lblBytesPerSecond_Register.Text = "0 B/s";
            // 
            // lblBytesPerSecond_DataTrans
            // 
            this.lblBytesPerSecond_DataTrans.AutoSize = true;
            this.lblBytesPerSecond_DataTrans.Location = new System.Drawing.Point(466, 97);
            this.lblBytesPerSecond_DataTrans.Name = "lblBytesPerSecond_DataTrans";
            this.lblBytesPerSecond_DataTrans.Size = new System.Drawing.Size(33, 13);
            this.lblBytesPerSecond_DataTrans.TabIndex = 18;
            this.lblBytesPerSecond_DataTrans.Text = "0 B/s";
            // 
            // label57
            // 
            this.label57.AutoSize = true;
            this.label57.Location = new System.Drawing.Point(400, 97);
            this.label57.Name = "label57";
            this.label57.Size = new System.Drawing.Size(60, 13);
            this.label57.TabIndex = 17;
            this.label57.Text = "DataTrans:";
            // 
            // label56
            // 
            this.label56.AutoSize = true;
            this.label56.Location = new System.Drawing.Point(411, 84);
            this.label56.Name = "label56";
            this.label56.Size = new System.Drawing.Size(49, 13);
            this.label56.TabIndex = 16;
            this.label56.Text = "Register:";
            // 
            // label55
            // 
            this.label55.AutoSize = true;
            this.label55.Location = new System.Drawing.Point(417, 71);
            this.label55.Name = "label55";
            this.label55.Size = new System.Drawing.Size(43, 13);
            this.label55.TabIndex = 15;
            this.label55.Text = "Course:";
            // 
            // label54
            // 
            this.label54.AutoSize = true;
            this.label54.Location = new System.Drawing.Point(421, 58);
            this.label54.Name = "label54";
            this.label54.Size = new System.Drawing.Size(39, 13);
            this.label54.TabIndex = 14;
            this.label54.Text = "Image:";
            // 
            // label53
            // 
            this.label53.AutoSize = true;
            this.label53.Location = new System.Drawing.Point(403, 45);
            this.label53.Name = "label53";
            this.label53.Size = new System.Drawing.Size(57, 13);
            this.label53.TabIndex = 13;
            this.label53.Text = "Command:";
            // 
            // label52
            // 
            this.label52.AutoSize = true;
            this.label52.Location = new System.Drawing.Point(425, 32);
            this.label52.Name = "label52";
            this.label52.Size = new System.Drawing.Size(35, 13);
            this.label52.TabIndex = 12;
            this.label52.Text = "State:";
            // 
            // label51
            // 
            this.label51.AutoSize = true;
            this.label51.Location = new System.Drawing.Point(407, 19);
            this.label51.Name = "label51";
            this.label51.Size = new System.Drawing.Size(53, 13);
            this.label51.TabIndex = 11;
            this.label51.Text = "Primative:";
            // 
            // label50
            // 
            this.label50.AutoSize = true;
            this.label50.Location = new System.Drawing.Point(429, 6);
            this.label50.Name = "label50";
            this.label50.Size = new System.Drawing.Size(31, 13);
            this.label50.TabIndex = 10;
            this.label50.Text = "Text:";
            // 
            // label49
            // 
            this.label49.AutoSize = true;
            this.label49.Location = new System.Drawing.Point(408, 123);
            this.label49.Name = "label49";
            this.label49.Size = new System.Drawing.Size(52, 13);
            this.label49.TabIndex = 9;
            this.label49.Text = "Total RX:";
            // 
            // lblBytesPerSecond_Total
            // 
            this.lblBytesPerSecond_Total.AutoSize = true;
            this.lblBytesPerSecond_Total.Location = new System.Drawing.Point(466, 123);
            this.lblBytesPerSecond_Total.Name = "lblBytesPerSecond_Total";
            this.lblBytesPerSecond_Total.Size = new System.Drawing.Size(33, 13);
            this.lblBytesPerSecond_Total.TabIndex = 8;
            this.lblBytesPerSecond_Total.Text = "0 B/s";
            // 
            // btn_packetLoss
            // 
            this.btn_packetLoss.BackColor = System.Drawing.Color.LightCoral;
            this.btn_packetLoss.Location = new System.Drawing.Point(195, 143);
            this.btn_packetLoss.Name = "btn_packetLoss";
            this.btn_packetLoss.Size = new System.Drawing.Size(106, 23);
            this.btn_packetLoss.TabIndex = 7;
            this.btn_packetLoss.Text = "pLoss(t/r): ?/0";
            this.btn_packetLoss.UseVisualStyleBackColor = false;
            this.btn_packetLoss.Click += new System.EventHandler(this.btn_packetLoss_Click);
            // 
            // clearSerialButton
            // 
            this.clearSerialButton.Location = new System.Drawing.Point(307, 143);
            this.clearSerialButton.Name = "clearSerialButton";
            this.clearSerialButton.Size = new System.Drawing.Size(90, 23);
            this.clearSerialButton.TabIndex = 5;
            this.clearSerialButton.Text = "Clear Serial Box";
            this.clearSerialButton.UseVisualStyleBackColor = true;
            this.clearSerialButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // rxCheckbox
            // 
            this.rxCheckbox.AutoSize = true;
            this.rxCheckbox.Location = new System.Drawing.Point(73, 147);
            this.rxCheckbox.Name = "rxCheckbox";
            this.rxCheckbox.Size = new System.Drawing.Size(65, 17);
            this.rxCheckbox.TabIndex = 4;
            this.rxCheckbox.Text = "View Rx";
            this.rxCheckbox.UseVisualStyleBackColor = true;
            this.rxCheckbox.CheckedChanged += new System.EventHandler(this.rxCheckbox_CheckedChanged);
            // 
            // txCheckbox
            // 
            this.txCheckbox.AutoSize = true;
            this.txCheckbox.Location = new System.Drawing.Point(3, 147);
            this.txCheckbox.Name = "txCheckbox";
            this.txCheckbox.Size = new System.Drawing.Size(64, 17);
            this.txCheckbox.TabIndex = 3;
            this.txCheckbox.Text = "View Tx";
            this.txCheckbox.UseVisualStyleBackColor = true;
            this.txCheckbox.CheckedChanged += new System.EventHandler(this.txCheckbox_CheckedChanged);
            // 
            // serialTextBox
            // 
            this.serialTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.serialTextBox.Location = new System.Drawing.Point(4, 0);
            this.serialTextBox.Multiline = true;
            this.serialTextBox.Name = "serialTextBox";
            this.serialTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.serialTextBox.Size = new System.Drawing.Size(393, 140);
            this.serialTextBox.TabIndex = 2;
            // 
            // uartControlTab
            // 
            this.uartControlTab.Controls.Add(this.lstBoxUART);
            this.uartControlTab.Location = new System.Drawing.Point(4, 22);
            this.uartControlTab.Name = "uartControlTab";
            this.uartControlTab.Padding = new System.Windows.Forms.Padding(3);
            this.uartControlTab.Size = new System.Drawing.Size(588, 172);
            this.uartControlTab.TabIndex = 3;
            this.uartControlTab.Text = "UART Control";
            this.uartControlTab.UseVisualStyleBackColor = true;
            // 
            // lstBoxUART
            // 
            this.lstBoxUART.AllowDrop = true;
            this.lstBoxUART.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstBoxUART.FormattingEnabled = true;
            this.lstBoxUART.Items.AddRange(new object[] {
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "COM9",
            "COM10",
            "COM11",
            "COM12",
            "COM13",
            "COM14",
            "COM15",
            "COM16",
            "COM17",
            "COM18",
            "COM19",
            "COM20",
            "COM21"});
            this.lstBoxUART.Location = new System.Drawing.Point(19, 11);
            this.lstBoxUART.MultiColumn = true;
            this.lstBoxUART.Name = "lstBoxUART";
            this.lstBoxUART.Size = new System.Drawing.Size(295, 121);
            this.lstBoxUART.TabIndex = 2;
            this.lstBoxUART.SelectedIndexChanged += new System.EventHandler(this.lstBoxUART_SelectedIndexChanged);
            // 
            // speedSlider
            // 
            this.speedSlider.Location = new System.Drawing.Point(560, 237);
            this.speedSlider.Name = "speedSlider";
            this.speedSlider.Size = new System.Drawing.Size(189, 45);
            this.speedSlider.TabIndex = 10;
            this.speedSlider.Value = 5;
            this.speedSlider.Visible = false;
            this.speedSlider.Scroll += new System.EventHandler(this.speedSlider_Scroll);
            // 
            // steeringSlider
            // 
            this.steeringSlider.Location = new System.Drawing.Point(560, 273);
            this.steeringSlider.Minimum = -10;
            this.steeringSlider.Name = "steeringSlider";
            this.steeringSlider.Size = new System.Drawing.Size(189, 45);
            this.steeringSlider.TabIndex = 11;
            this.steeringSlider.Tag = "";
            this.steeringSlider.Scroll += new System.EventHandler(this.steeringSlider_Scroll);
            // 
            // speedSliderLabel
            // 
            this.speedSliderLabel.AutoSize = true;
            this.speedSliderLabel.BackColor = System.Drawing.SystemColors.Control;
            this.speedSliderLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.speedSliderLabel.Location = new System.Drawing.Point(488, 246);
            this.speedSliderLabel.Name = "speedSliderLabel";
            this.speedSliderLabel.Size = new System.Drawing.Size(54, 16);
            this.speedSliderLabel.TabIndex = 12;
            this.speedSliderLabel.Text = "Speed";
            this.speedSliderLabel.Visible = false;
            // 
            // steeringSliderLabel
            // 
            this.steeringSliderLabel.AutoSize = true;
            this.steeringSliderLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.steeringSliderLabel.Location = new System.Drawing.Point(488, 282);
            this.steeringSliderLabel.Name = "steeringSliderLabel";
            this.steeringSliderLabel.Size = new System.Drawing.Size(66, 16);
            this.steeringSliderLabel.TabIndex = 13;
            this.steeringSliderLabel.Text = "Steering";
            // 
            // imageTabControl
            // 
            this.imageTabControl.Controls.Add(this.rawImageTab);
            this.imageTabControl.Controls.Add(this.processedImageTab);
            this.imageTabControl.Location = new System.Drawing.Point(1, 0);
            this.imageTabControl.Name = "imageTabControl";
            this.imageTabControl.SelectedIndex = 0;
            this.imageTabControl.Size = new System.Drawing.Size(328, 266);
            this.imageTabControl.TabIndex = 14;
            // 
            // rawImageTab
            // 
            this.rawImageTab.Controls.Add(this.rawImage);
            this.rawImageTab.Location = new System.Drawing.Point(4, 22);
            this.rawImageTab.Name = "rawImageTab";
            this.rawImageTab.Padding = new System.Windows.Forms.Padding(3);
            this.rawImageTab.Size = new System.Drawing.Size(320, 240);
            this.rawImageTab.TabIndex = 0;
            this.rawImageTab.Text = "Raw";
            this.rawImageTab.UseVisualStyleBackColor = true;
            // 
            // rawImage
            // 
            this.rawImage.Location = new System.Drawing.Point(0, 0);
            this.rawImage.Name = "rawImage";
            this.rawImage.Size = new System.Drawing.Size(320, 240);
            this.rawImage.TabIndex = 0;
            // 
            // processedImageTab
            // 
            this.processedImageTab.Location = new System.Drawing.Point(4, 22);
            this.processedImageTab.Name = "processedImageTab";
            this.processedImageTab.Padding = new System.Windows.Forms.Padding(3);
            this.processedImageTab.Size = new System.Drawing.Size(320, 240);
            this.processedImageTab.TabIndex = 1;
            this.processedImageTab.Text = "Processed";
            this.processedImageTab.UseVisualStyleBackColor = true;
            // 
            // getImageButton
            // 
            this.getImageButton.Location = new System.Drawing.Point(335, 18);
            this.getImageButton.Name = "getImageButton";
            this.getImageButton.Size = new System.Drawing.Size(64, 23);
            this.getImageButton.TabIndex = 15;
            this.getImageButton.Text = "Get Image";
            this.getImageButton.UseVisualStyleBackColor = true;
            this.getImageButton.Click += new System.EventHandler(this.getImageButton_Click);
            // 
            // saveImageButton
            // 
            this.saveImageButton.Location = new System.Drawing.Point(335, 47);
            this.saveImageButton.Name = "saveImageButton";
            this.saveImageButton.Size = new System.Drawing.Size(75, 23);
            this.saveImageButton.TabIndex = 16;
            this.saveImageButton.Text = "Save Image";
            this.saveImageButton.UseVisualStyleBackColor = true;
            this.saveImageButton.Click += new System.EventHandler(this.saveImageButton_Click);
            // 
            // truckMessageBox
            // 
            this.truckMessageBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.truckMessageBox.Location = new System.Drawing.Point(1, 351);
            this.truckMessageBox.Multiline = true;
            this.truckMessageBox.Name = "truckMessageBox";
            this.truckMessageBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.truckMessageBox.Size = new System.Drawing.Size(323, 248);
            this.truckMessageBox.TabIndex = 17;
            // 
            // truckMessageBoxLabel
            // 
            this.truckMessageBoxLabel.AutoSize = true;
            this.truckMessageBoxLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.truckMessageBoxLabel.Location = new System.Drawing.Point(27, 335);
            this.truckMessageBoxLabel.Name = "truckMessageBoxLabel";
            this.truckMessageBoxLabel.Size = new System.Drawing.Size(128, 13);
            this.truckMessageBoxLabel.TabIndex = 18;
            this.truckMessageBoxLabel.Text = "Messages from Truck";
            // 
            // stateLabel
            // 
            this.stateLabel.AutoSize = true;
            this.stateLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.stateLabel.Location = new System.Drawing.Point(143, 275);
            this.stateLabel.Name = "stateLabel";
            this.stateLabel.Size = new System.Drawing.Size(48, 16);
            this.stateLabel.TabIndex = 19;
            this.stateLabel.Text = "State:";
            // 
            // speedLabel
            // 
            this.speedLabel.AutoSize = true;
            this.speedLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.speedLabel.Location = new System.Drawing.Point(112, 250);
            this.speedLabel.Name = "speedLabel";
            this.speedLabel.Size = new System.Drawing.Size(58, 16);
            this.speedLabel.TabIndex = 20;
            this.speedLabel.Text = "Speed:";
            // 
            // truckState
            // 
            this.truckState.AutoSize = true;
            this.truckState.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.truckState.ForeColor = System.Drawing.Color.Red;
            this.truckState.Location = new System.Drawing.Point(197, 275);
            this.truckState.Name = "truckState";
            this.truckState.Size = new System.Drawing.Size(71, 16);
            this.truckState.TabIndex = 23;
            this.truckState.Text = "Disabled";
            // 
            // truckVelocityTextbox
            // 
            this.truckVelocityTextbox.AutoSize = true;
            this.truckVelocityTextbox.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.truckVelocityTextbox.Location = new System.Drawing.Point(197, 294);
            this.truckVelocityTextbox.Name = "truckVelocityTextbox";
            this.truckVelocityTextbox.Size = new System.Drawing.Size(50, 16);
            this.truckVelocityTextbox.TabIndex = 24;
            this.truckVelocityTextbox.Text = "0.0 m/s";
            // 
            // eStopButton
            // 
            this.eStopButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.eStopButton.Location = new System.Drawing.Point(335, 134);
            this.eStopButton.Name = "eStopButton";
            this.eStopButton.Size = new System.Drawing.Size(127, 64);
            this.eStopButton.TabIndex = 27;
            this.eStopButton.Text = "E-STOP";
            this.eStopButton.UseVisualStyleBackColor = true;
            this.eStopButton.Click += new System.EventHandler(this.eStopButton_Click);
            // 
            // startVideoButton
            // 
            this.startVideoButton.Location = new System.Drawing.Point(335, 76);
            this.startVideoButton.Name = "startVideoButton";
            this.startVideoButton.Size = new System.Drawing.Size(75, 23);
            this.startVideoButton.TabIndex = 29;
            this.startVideoButton.Text = "Start Video";
            this.startVideoButton.UseVisualStyleBackColor = true;
            this.startVideoButton.Click += new System.EventHandler(this.startVideoButton_Click);
            // 
            // messagesOnButton
            // 
            this.messagesOnButton.Location = new System.Drawing.Point(335, 358);
            this.messagesOnButton.Name = "messagesOnButton";
            this.messagesOnButton.Size = new System.Drawing.Size(88, 23);
            this.messagesOnButton.TabIndex = 30;
            this.messagesOnButton.Text = "Messages On";
            this.messagesOnButton.UseVisualStyleBackColor = true;
            this.messagesOnButton.Click += new System.EventHandler(this.messagesOnButton_Click);
            // 
            // messagesOffButton
            // 
            this.messagesOffButton.Location = new System.Drawing.Point(335, 387);
            this.messagesOffButton.Name = "messagesOffButton";
            this.messagesOffButton.Size = new System.Drawing.Size(88, 23);
            this.messagesOffButton.TabIndex = 31;
            this.messagesOffButton.Text = "Messages Off";
            this.messagesOffButton.UseVisualStyleBackColor = true;
            this.messagesOffButton.Click += new System.EventHandler(this.messagesOffButton_Click);
            // 
            // viewRegistersButton
            // 
            this.viewRegistersButton.Location = new System.Drawing.Point(335, 433);
            this.viewRegistersButton.Name = "viewRegistersButton";
            this.viewRegistersButton.Size = new System.Drawing.Size(88, 26);
            this.viewRegistersButton.TabIndex = 32;
            this.viewRegistersButton.Text = "View Registers";
            this.viewRegistersButton.UseVisualStyleBackColor = true;
            this.viewRegistersButton.Click += new System.EventHandler(this.viewRegistersButton_Click);
            // 
            // stopVideoButton
            // 
            this.stopVideoButton.Location = new System.Drawing.Point(335, 105);
            this.stopVideoButton.Name = "stopVideoButton";
            this.stopVideoButton.Size = new System.Drawing.Size(75, 23);
            this.stopVideoButton.TabIndex = 33;
            this.stopVideoButton.Text = "Stop Video";
            this.stopVideoButton.UseVisualStyleBackColor = true;
            this.stopVideoButton.Click += new System.EventHandler(this.stopVideoButton_Click);
            // 
            // saveCourseDialog
            // 
            this.saveCourseDialog.Filter = "The Knack Course Files (*.nak)|*.nak";
            this.saveCourseDialog.InitialDirectory = "C:\\Program Files\\Microsoft Visual Studio 9.0\\Common7\\IDE";
            this.saveCourseDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveCourseDialog_FileOk);
            // 
            // loadCourseDialog
            // 
            this.loadCourseDialog.DefaultExt = "nak";
            this.loadCourseDialog.FileName = "loadCourseDialog";
            this.loadCourseDialog.Filter = "The Knack Course Files|*.nak";
            this.loadCourseDialog.InitialDirectory = "C:\\Program Files\\Microsoft Visual Studio 9.0\\Common7\\IDE";
            this.loadCourseDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.loadCourseDialog_FileOk);
            // 
            // clearBtn
            // 
            this.clearBtn.Location = new System.Drawing.Point(335, 465);
            this.clearBtn.Name = "clearBtn";
            this.clearBtn.Size = new System.Drawing.Size(88, 26);
            this.clearBtn.TabIndex = 35;
            this.clearBtn.Text = "Clear Msgs";
            this.clearBtn.UseVisualStyleBackColor = true;
            this.clearBtn.Click += new System.EventHandler(this.clearBtn_Click);
            // 
            // saveImageDialog
            // 
            this.saveImageDialog.Filter = "BMP Images (*.bmp)|*.bmp";
            this.saveImageDialog.InitialDirectory = "C:\\Program Files\\Microsoft Visual Studio 9.0\\Common7\\IDE";
            this.saveImageDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveImageDialog_FileOk);
            // 
            // lblImageCoords
            // 
            this.lblImageCoords.AutoSize = true;
            this.lblImageCoords.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblImageCoords.Location = new System.Drawing.Point(26, 275);
            this.lblImageCoords.Name = "lblImageCoords";
            this.lblImageCoords.Size = new System.Drawing.Size(0, 14);
            this.lblImageCoords.TabIndex = 37;
            this.lblImageCoords.Visible = false;
            // 
            // lblImageColorHex
            // 
            this.lblImageColorHex.AutoSize = true;
            this.lblImageColorHex.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblImageColorHex.Location = new System.Drawing.Point(26, 290);
            this.lblImageColorHex.Name = "lblImageColorHex";
            this.lblImageColorHex.Size = new System.Drawing.Size(0, 14);
            this.lblImageColorHex.TabIndex = 38;
            this.lblImageColorHex.Visible = false;
            // 
            // lblImageColorBinary
            // 
            this.lblImageColorBinary.AutoSize = true;
            this.lblImageColorBinary.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblImageColorBinary.Location = new System.Drawing.Point(27, 304);
            this.lblImageColorBinary.Name = "lblImageColorBinary";
            this.lblImageColorBinary.Size = new System.Drawing.Size(0, 14);
            this.lblImageColorBinary.TabIndex = 39;
            this.lblImageColorBinary.Visible = false;
            // 
            // btn_transmitBinary
            // 
            this.btn_transmitBinary.Location = new System.Drawing.Point(772, 201);
            this.btn_transmitBinary.Name = "btn_transmitBinary";
            this.btn_transmitBinary.Size = new System.Drawing.Size(88, 23);
            this.btn_transmitBinary.TabIndex = 40;
            this.btn_transmitBinary.Text = "Transmit Binary";
            this.btn_transmitBinary.UseVisualStyleBackColor = true;
            this.btn_transmitBinary.Click += new System.EventHandler(this.btn_transmitBinary_Click);
            // 
            // txt_binaryFileName
            // 
            this.txt_binaryFileName.Cursor = System.Windows.Forms.Cursors.Hand;
            this.txt_binaryFileName.Location = new System.Drawing.Point(472, 203);
            this.txt_binaryFileName.Name = "txt_binaryFileName";
            this.txt_binaryFileName.ReadOnly = true;
            this.txt_binaryFileName.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.txt_binaryFileName.Size = new System.Drawing.Size(293, 20);
            this.txt_binaryFileName.TabIndex = 41;
            this.txt_binaryFileName.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // loadBinaryDialog
            // 
            this.loadBinaryDialog.Filter = "ELF Files (*.elf)|*.elf";
            this.loadBinaryDialog.InitialDirectory = "C:\\TheKNACKsource\\Trunk\\GUI";
            this.loadBinaryDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.loadBinaryDialog_FileOk);
            // 
            // btn_bootload
            // 
            this.btn_bootload.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.btn_bootload.Location = new System.Drawing.Point(860, 201);
            this.btn_bootload.Name = "btn_bootload";
            this.btn_bootload.Size = new System.Drawing.Size(16, 23);
            this.btn_bootload.TabIndex = 42;
            this.btn_bootload.UseVisualStyleBackColor = false;
            this.btn_bootload.Click += new System.EventHandler(this.btn_bootload_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(123, 294);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(68, 16);
            this.label1.TabIndex = 43;
            this.label1.Text = "Velocity:";
            // 
            // ddbImageType
            // 
            this.ddbImageType.FormattingEnabled = true;
            this.ddbImageType.Items.AddRange(new object[] {
            "RGB565",
            "HSV",
            "SEG_Or",
            "SEG_Gr",
            "GREYSCALE_Or",
            "GREYSCALE_Gr"});
            this.ddbImageType.Location = new System.Drawing.Point(1, 266);
            this.ddbImageType.Name = "ddbImageType";
            this.ddbImageType.Size = new System.Drawing.Size(121, 21);
            this.ddbImageType.TabIndex = 44;
            this.ddbImageType.Text = "RGB565";
            this.ddbImageType.SelectedIndexChanged += new System.EventHandler(this.ddbImageType_SelectedIndexChanged);
            // 
            // button1
            // 
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button1.Location = new System.Drawing.Point(417, 18);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(45, 110);
            this.button1.TabIndex = 45;
            this.button1.Text = "Drive         G       A       M      E";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click_1);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(0, 293);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(37, 15);
            this.label11.TabIndex = 46;
            this.label11.Text = "H/W:";
            // 
            // lblFPS_HW
            // 
            this.lblFPS_HW.AutoSize = true;
            this.lblFPS_HW.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblFPS_HW.Location = new System.Drawing.Point(43, 294);
            this.lblFPS_HW.Name = "lblFPS_HW";
            this.lblFPS_HW.Size = new System.Drawing.Size(30, 13);
            this.lblFPS_HW.TabIndex = 47;
            this.lblFPS_HW.Text = "0 fps";
            // 
            // buttonGetCriticalImage
            // 
            this.buttonGetCriticalImage.Location = new System.Drawing.Point(398, 18);
            this.buttonGetCriticalImage.Name = "buttonGetCriticalImage";
            this.buttonGetCriticalImage.Size = new System.Drawing.Size(12, 23);
            this.buttonGetCriticalImage.TabIndex = 48;
            this.buttonGetCriticalImage.Text = "C";
            this.buttonGetCriticalImage.UseVisualStyleBackColor = true;
            this.buttonGetCriticalImage.Click += new System.EventHandler(this.buttonGetCriticalImage_Click);
            // 
            // mouseControlPanel
            // 
            this.mouseControlPanel.Location = new System.Drawing.Point(328, 204);
            this.mouseControlPanel.Name = "mouseControlPanel";
            this.mouseControlPanel.Size = new System.Drawing.Size(141, 141);
            this.mouseControlPanel.TabIndex = 34;
            // 
            // lblFPS_SW
            // 
            this.lblFPS_SW.AutoSize = true;
            this.lblFPS_SW.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
            this.lblFPS_SW.Location = new System.Drawing.Point(43, 308);
            this.lblFPS_SW.Name = "lblFPS_SW";
            this.lblFPS_SW.Size = new System.Drawing.Size(30, 13);
            this.lblFPS_SW.TabIndex = 50;
            this.lblFPS_SW.Text = "0 fps";
            // 
            // label60
            // 
            this.label60.AutoSize = true;
            this.label60.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label60.Location = new System.Drawing.Point(0, 307);
            this.label60.Name = "label60";
            this.label60.Size = new System.Drawing.Size(36, 15);
            this.label60.TabIndex = 49;
            this.label60.Text = "S/W:";
            // 
            // pidTab
            // 
            this.pidTab.Controls.Add(this.runButton);
            this.pidTab.Controls.Add(this.orLabel);
            this.pidTab.Controls.Add(this.distanceToRunLabel);
            this.pidTab.Controls.Add(this.metersToRunTextbox);
            this.pidTab.Controls.Add(this.turningTextbox);
            this.pidTab.Controls.Add(this.numMillisecondsToRunTextbox);
            this.pidTab.Controls.Add(this.desiredVelocityTextbox);
            this.pidTab.Controls.Add(this.iMinTextbox);
            this.pidTab.Controls.Add(this.iMaxTextbox);
            this.pidTab.Controls.Add(this.kiTextbox);
            this.pidTab.Controls.Add(this.kdTextbox);
            this.pidTab.Controls.Add(this.kpTextbox);
            this.pidTab.Controls.Add(this.turningLabel);
            this.pidTab.Controls.Add(this.numMillisecondsLabel);
            this.pidTab.Controls.Add(this.resetPIDValuesButton);
            this.pidTab.Controls.Add(this.desiredVelocityLabel);
            this.pidTab.Controls.Add(this.iMinLabel);
            this.pidTab.Controls.Add(this.iMaxLabel);
            this.pidTab.Controls.Add(this.kiLabel);
            this.pidTab.Controls.Add(this.kdLabel);
            this.pidTab.Controls.Add(this.kpLabel);
            this.pidTab.Controls.Add(this.sendPidButton);
            this.pidTab.Location = new System.Drawing.Point(4, 22);
            this.pidTab.Name = "pidTab";
            this.pidTab.Padding = new System.Windows.Forms.Padding(3);
            this.pidTab.Size = new System.Drawing.Size(627, 234);
            this.pidTab.TabIndex = 1;
            this.pidTab.Text = "PID Controller";
            this.pidTab.UseVisualStyleBackColor = true;
            // 
            // kpTextbox
            // 
            this.kpTextbox.Location = new System.Drawing.Point(43, 13);
            this.kpTextbox.Name = "kpTextbox";
            this.kpTextbox.Size = new System.Drawing.Size(66, 20);
            this.kpTextbox.TabIndex = 0;
            this.kpTextbox.Text = "55.5";
            // 
            // kdTextbox
            // 
            this.kdTextbox.Location = new System.Drawing.Point(43, 39);
            this.kdTextbox.Name = "kdTextbox";
            this.kdTextbox.Size = new System.Drawing.Size(66, 20);
            this.kdTextbox.TabIndex = 1;
            this.kdTextbox.Text = "0.05";
            // 
            // kiTextbox
            // 
            this.kiTextbox.Location = new System.Drawing.Point(43, 68);
            this.kiTextbox.Name = "kiTextbox";
            this.kiTextbox.Size = new System.Drawing.Size(66, 20);
            this.kiTextbox.TabIndex = 2;
            this.kiTextbox.Text = "1.0";
            // 
            // sendPidButton
            // 
            this.sendPidButton.Location = new System.Drawing.Point(6, 147);
            this.sendPidButton.Name = "sendPidButton";
            this.sendPidButton.Size = new System.Drawing.Size(103, 23);
            this.sendPidButton.TabIndex = 3;
            this.sendPidButton.Text = "Update PID";
            this.sendPidButton.UseVisualStyleBackColor = true;
            this.sendPidButton.Click += new System.EventHandler(this.sendPidButton_Click);
            // 
            // kpLabel
            // 
            this.kpLabel.AutoSize = true;
            this.kpLabel.Location = new System.Drawing.Point(7, 19);
            this.kpLabel.Name = "kpLabel";
            this.kpLabel.Size = new System.Drawing.Size(20, 13);
            this.kpLabel.TabIndex = 4;
            this.kpLabel.Text = "kP";
            // 
            // kdLabel
            // 
            this.kdLabel.AutoSize = true;
            this.kdLabel.Location = new System.Drawing.Point(7, 45);
            this.kdLabel.Name = "kdLabel";
            this.kdLabel.Size = new System.Drawing.Size(21, 13);
            this.kdLabel.TabIndex = 5;
            this.kdLabel.Text = "kD";
            // 
            // kiLabel
            // 
            this.kiLabel.AutoSize = true;
            this.kiLabel.Location = new System.Drawing.Point(7, 74);
            this.kiLabel.Name = "kiLabel";
            this.kiLabel.Size = new System.Drawing.Size(16, 13);
            this.kiLabel.TabIndex = 6;
            this.kiLabel.Text = "kI";
            // 
            // iMaxTextbox
            // 
            this.iMaxTextbox.Location = new System.Drawing.Point(43, 94);
            this.iMaxTextbox.Name = "iMaxTextbox";
            this.iMaxTextbox.Size = new System.Drawing.Size(66, 20);
            this.iMaxTextbox.TabIndex = 7;
            this.iMaxTextbox.Text = "1.5";
            // 
            // iMinTextbox
            // 
            this.iMinTextbox.Location = new System.Drawing.Point(43, 121);
            this.iMinTextbox.Name = "iMinTextbox";
            this.iMinTextbox.Size = new System.Drawing.Size(66, 20);
            this.iMinTextbox.TabIndex = 8;
            this.iMinTextbox.Text = "-1.5";
            // 
            // iMaxLabel
            // 
            this.iMaxLabel.AutoSize = true;
            this.iMaxLabel.Location = new System.Drawing.Point(7, 100);
            this.iMaxLabel.Name = "iMaxLabel";
            this.iMaxLabel.Size = new System.Drawing.Size(29, 13);
            this.iMaxLabel.TabIndex = 9;
            this.iMaxLabel.Text = "iMax";
            // 
            // iMinLabel
            // 
            this.iMinLabel.AutoSize = true;
            this.iMinLabel.Location = new System.Drawing.Point(7, 127);
            this.iMinLabel.Name = "iMinLabel";
            this.iMinLabel.Size = new System.Drawing.Size(26, 13);
            this.iMinLabel.TabIndex = 10;
            this.iMinLabel.Text = "iMin";
            // 
            // desiredVelocityTextbox
            // 
            this.desiredVelocityTextbox.Location = new System.Drawing.Point(206, 13);
            this.desiredVelocityTextbox.Name = "desiredVelocityTextbox";
            this.desiredVelocityTextbox.Size = new System.Drawing.Size(40, 20);
            this.desiredVelocityTextbox.TabIndex = 11;
            this.desiredVelocityTextbox.Text = "0.0";
            // 
            // desiredVelocityLabel
            // 
            this.desiredVelocityLabel.AutoSize = true;
            this.desiredVelocityLabel.Location = new System.Drawing.Point(130, 19);
            this.desiredVelocityLabel.Name = "desiredVelocityLabel";
            this.desiredVelocityLabel.Size = new System.Drawing.Size(71, 13);
            this.desiredVelocityLabel.TabIndex = 12;
            this.desiredVelocityLabel.Text = "Velocity (m/s)";
            // 
            // resetPIDValuesButton
            // 
            this.resetPIDValuesButton.Location = new System.Drawing.Point(6, 175);
            this.resetPIDValuesButton.Name = "resetPIDValuesButton";
            this.resetPIDValuesButton.Size = new System.Drawing.Size(103, 23);
            this.resetPIDValuesButton.TabIndex = 14;
            this.resetPIDValuesButton.Text = "Reset Values";
            this.resetPIDValuesButton.UseVisualStyleBackColor = true;
            this.resetPIDValuesButton.Click += new System.EventHandler(this.resetPIDValuesButton_Click);
            // 
            // numMillisecondsToRunTextbox
            // 
            this.numMillisecondsToRunTextbox.Location = new System.Drawing.Point(345, 13);
            this.numMillisecondsToRunTextbox.Name = "numMillisecondsToRunTextbox";
            this.numMillisecondsToRunTextbox.Size = new System.Drawing.Size(40, 20);
            this.numMillisecondsToRunTextbox.TabIndex = 15;
            this.numMillisecondsToRunTextbox.Text = "0";
            // 
            // numMillisecondsLabel
            // 
            this.numMillisecondsLabel.AutoSize = true;
            this.numMillisecondsLabel.Location = new System.Drawing.Point(290, 18);
            this.numMillisecondsLabel.Name = "numMillisecondsLabel";
            this.numMillisecondsLabel.Size = new System.Drawing.Size(49, 13);
            this.numMillisecondsLabel.TabIndex = 16;
            this.numMillisecondsLabel.Text = "Run (ms)";
            // 
            // turningTextbox
            // 
            this.turningTextbox.Location = new System.Drawing.Point(206, 39);
            this.turningTextbox.Name = "turningTextbox";
            this.turningTextbox.Size = new System.Drawing.Size(40, 20);
            this.turningTextbox.TabIndex = 17;
            this.turningTextbox.Text = "0";
            // 
            // turningLabel
            // 
            this.turningLabel.AutoSize = true;
            this.turningLabel.Location = new System.Drawing.Point(131, 45);
            this.turningLabel.Name = "turningLabel";
            this.turningLabel.Size = new System.Drawing.Size(70, 13);
            this.turningLabel.TabIndex = 19;
            this.turningLabel.Text = "Turning (deg)";
            // 
            // metersToRunTextbox
            // 
            this.metersToRunTextbox.Location = new System.Drawing.Point(345, 56);
            this.metersToRunTextbox.Name = "metersToRunTextbox";
            this.metersToRunTextbox.Size = new System.Drawing.Size(40, 20);
            this.metersToRunTextbox.TabIndex = 20;
            this.metersToRunTextbox.Text = "0.0";
            // 
            // distanceToRunLabel
            // 
            this.distanceToRunLabel.AutoSize = true;
            this.distanceToRunLabel.Location = new System.Drawing.Point(295, 61);
            this.distanceToRunLabel.Name = "distanceToRunLabel";
            this.distanceToRunLabel.Size = new System.Drawing.Size(44, 13);
            this.distanceToRunLabel.TabIndex = 21;
            this.distanceToRunLabel.Text = "Run (m)";
            // 
            // orLabel
            // 
            this.orLabel.AutoSize = true;
            this.orLabel.Location = new System.Drawing.Point(329, 39);
            this.orLabel.Name = "orLabel";
            this.orLabel.Size = new System.Drawing.Size(23, 13);
            this.orLabel.TabIndex = 22;
            this.orLabel.Text = "OR";
            // 
            // runButton
            // 
            this.runButton.Location = new System.Drawing.Point(310, 117);
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(75, 23);
            this.runButton.TabIndex = 23;
            this.runButton.Text = "Run Now";
            this.runButton.UseVisualStyleBackColor = true;
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // registerTab
            // 
            this.registerTab.BackColor = System.Drawing.Color.White;
            this.registerTab.Location = new System.Drawing.Point(4, 22);
            this.registerTab.Name = "registerTab";
            this.registerTab.Padding = new System.Windows.Forms.Padding(3);
            this.registerTab.Size = new System.Drawing.Size(627, 234);
            this.registerTab.TabIndex = 0;
            this.registerTab.Text = "Registers";
            this.registerTab.UseVisualStyleBackColor = true;
            // 
            // navigationTab
            // 
            this.navigationTab.Controls.Add(this.label22);
            this.navigationTab.Controls.Add(this.label21);
            this.navigationTab.Controls.Add(this.findRangeTextbox);
            this.navigationTab.Controls.Add(this.turnAngleTextbox);
            this.navigationTab.Controls.Add(this.turnRadiusTextbox);
            this.navigationTab.Controls.Add(this.straightSpeedTextbox);
            this.navigationTab.Controls.Add(this.blindTurnTextbox);
            this.navigationTab.Controls.Add(this.blindStraightTextbox);
            this.navigationTab.Controls.Add(this.label20);
            this.navigationTab.Controls.Add(this.label19);
            this.navigationTab.Controls.Add(this.label18);
            this.navigationTab.Controls.Add(this.label17);
            this.navigationTab.Controls.Add(this.label16);
            this.navigationTab.Controls.Add(this.label15);
            this.navigationTab.Controls.Add(this.label14);
            this.navigationTab.Controls.Add(this.label13);
            this.navigationTab.Controls.Add(this.button2);
            this.navigationTab.Controls.Add(this.label);
            this.navigationTab.Controls.Add(this.label12);
            this.navigationTab.Controls.Add(this.distanceToBlindLabel);
            this.navigationTab.Controls.Add(this.distanceTraveledLabel);
            this.navigationTab.Controls.Add(this.carStateLabel);
            this.navigationTab.Controls.Add(this.label10);
            this.navigationTab.Controls.Add(this.label9);
            this.navigationTab.Controls.Add(this.label6);
            this.navigationTab.Location = new System.Drawing.Point(4, 22);
            this.navigationTab.Name = "navigationTab";
            this.navigationTab.Size = new System.Drawing.Size(483, 226);
            this.navigationTab.TabIndex = 3;
            this.navigationTab.Text = "Navigation";
            this.navigationTab.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(54, 26);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Car State:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(3, 55);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(115, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "Distance Traveled:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(10, 82);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(108, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "Distance to Blind:";
            // 
            // carStateLabel
            // 
            this.carStateLabel.AutoSize = true;
            this.carStateLabel.Location = new System.Drawing.Point(124, 26);
            this.carStateLabel.Name = "carStateLabel";
            this.carStateLabel.Size = new System.Drawing.Size(81, 13);
            this.carStateLabel.TabIndex = 11;
            this.carStateLabel.Text = "Unknown State";
            // 
            // distanceTraveledLabel
            // 
            this.distanceTraveledLabel.AutoSize = true;
            this.distanceTraveledLabel.Location = new System.Drawing.Point(125, 55);
            this.distanceTraveledLabel.Name = "distanceTraveledLabel";
            this.distanceTraveledLabel.Size = new System.Drawing.Size(33, 13);
            this.distanceTraveledLabel.TabIndex = 14;
            this.distanceTraveledLabel.Text = "0.0 m";
            // 
            // distanceToBlindLabel
            // 
            this.distanceToBlindLabel.AutoSize = true;
            this.distanceToBlindLabel.Location = new System.Drawing.Point(124, 82);
            this.distanceToBlindLabel.Name = "distanceToBlindLabel";
            this.distanceToBlindLabel.Size = new System.Drawing.Size(33, 13);
            this.distanceToBlindLabel.TabIndex = 15;
            this.distanceToBlindLabel.Text = "0.0 m";
            // 
            // blindStraightTextbox
            // 
            this.blindStraightTextbox.Location = new System.Drawing.Point(337, 10);
            this.blindStraightTextbox.Name = "blindStraightTextbox";
            this.blindStraightTextbox.Size = new System.Drawing.Size(39, 20);
            this.blindStraightTextbox.TabIndex = 16;
            this.blindStraightTextbox.Text = "1.75";
            // 
            // blindTurnTextbox
            // 
            this.blindTurnTextbox.Location = new System.Drawing.Point(337, 36);
            this.blindTurnTextbox.Name = "blindTurnTextbox";
            this.blindTurnTextbox.Size = new System.Drawing.Size(39, 20);
            this.blindTurnTextbox.TabIndex = 17;
            this.blindTurnTextbox.Text = "1.5";
            // 
            // straightSpeedTextbox
            // 
            this.straightSpeedTextbox.Location = new System.Drawing.Point(337, 63);
            this.straightSpeedTextbox.Name = "straightSpeedTextbox";
            this.straightSpeedTextbox.Size = new System.Drawing.Size(39, 20);
            this.straightSpeedTextbox.TabIndex = 18;
            this.straightSpeedTextbox.Text = "2.0";
            // 
            // turnRadiusTextbox
            // 
            this.turnRadiusTextbox.Location = new System.Drawing.Point(337, 89);
            this.turnRadiusTextbox.Name = "turnRadiusTextbox";
            this.turnRadiusTextbox.Size = new System.Drawing.Size(39, 20);
            this.turnRadiusTextbox.TabIndex = 19;
            this.turnRadiusTextbox.Text = "0.55";
            this.turnRadiusTextbox.TextChanged += new System.EventHandler(this.turnRadiusTextbox_TextChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(262, 17);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(69, 13);
            this.label12.TabIndex = 20;
            this.label12.Text = "Blind Straight";
            // 
            // label
            // 
            this.label.AutoSize = true;
            this.label.Location = new System.Drawing.Point(276, 43);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(55, 13);
            this.label.TabIndex = 21;
            this.label.Text = "Blind Turn";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(300, 191);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 22;
            this.button2.Text = "Update";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(254, 70);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(77, 13);
            this.label13.TabIndex = 23;
            this.label13.Text = "Straight Speed";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(266, 96);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(65, 13);
            this.label14.TabIndex = 24;
            this.label14.Text = "Turn Radius";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(383, 16);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(25, 13);
            this.label15.TabIndex = 25;
            this.label15.Text = "m/s";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(383, 43);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(25, 13);
            this.label16.TabIndex = 26;
            this.label16.Text = "m/s";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(383, 70);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(25, 13);
            this.label17.TabIndex = 27;
            this.label17.Text = "m/s";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(383, 97);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(15, 13);
            this.label18.TabIndex = 28;
            this.label18.Text = "m";
            // 
            // turnAngleTextbox
            // 
            this.turnAngleTextbox.Location = new System.Drawing.Point(337, 116);
            this.turnAngleTextbox.Name = "turnAngleTextbox";
            this.turnAngleTextbox.Size = new System.Drawing.Size(38, 20);
            this.turnAngleTextbox.TabIndex = 29;
            this.turnAngleTextbox.Text = "55";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(272, 123);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(59, 13);
            this.label19.TabIndex = 30;
            this.label19.Text = "Turn Angle";
            // 
            // label20
            // 
            this.label20.AutoSize = true;
            this.label20.Location = new System.Drawing.Point(382, 122);
            this.label20.Name = "label20";
            this.label20.Size = new System.Drawing.Size(25, 13);
            this.label20.TabIndex = 31;
            this.label20.Text = "deg";
            // 
            // findRangeTextbox
            // 
            this.findRangeTextbox.Location = new System.Drawing.Point(337, 143);
            this.findRangeTextbox.Name = "findRangeTextbox";
            this.findRangeTextbox.Size = new System.Drawing.Size(38, 20);
            this.findRangeTextbox.TabIndex = 32;
            this.findRangeTextbox.Text = "250";
            // 
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(269, 150);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(62, 13);
            this.label21.TabIndex = 33;
            this.label21.Text = "Find Range";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(382, 149);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(18, 13);
            this.label22.TabIndex = 34;
            this.label22.Text = "px";
            // 
            // registerTabControl
            // 
            this.registerTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.registerTabControl.Controls.Add(this.navigationTab);
            this.registerTabControl.Controls.Add(this.registerTab);
            this.registerTabControl.Controls.Add(this.pidTab);
            this.registerTabControl.Location = new System.Drawing.Point(429, 351);
            this.registerTabControl.Name = "registerTabControl";
            this.registerTabControl.SelectedIndex = 0;
            this.registerTabControl.Size = new System.Drawing.Size(491, 252);
            this.registerTabControl.TabIndex = 36;
            // 
            // RobotRacer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(922, 615);
            this.Controls.Add(this.lblFPS_SW);
            this.Controls.Add(this.label60);
            this.Controls.Add(this.registerTabControl);
            this.Controls.Add(this.buttonGetCriticalImage);
            this.Controls.Add(this.lblFPS_HW);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.ddbImageType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_bootload);
            this.Controls.Add(this.txt_binaryFileName);
            this.Controls.Add(this.btn_transmitBinary);
            this.Controls.Add(this.lblImageColorBinary);
            this.Controls.Add(this.lblImageColorHex);
            this.Controls.Add(this.lblImageCoords);
            this.Controls.Add(this.clearBtn);
            this.Controls.Add(this.mouseControlPanel);
            this.Controls.Add(this.stopVideoButton);
            this.Controls.Add(this.viewRegistersButton);
            this.Controls.Add(this.messagesOffButton);
            this.Controls.Add(this.messagesOnButton);
            this.Controls.Add(this.startVideoButton);
            this.Controls.Add(this.eStopButton);
            this.Controls.Add(this.truckVelocityTextbox);
            this.Controls.Add(this.truckState);
            this.Controls.Add(this.speedSliderLabel);
            this.Controls.Add(this.stateLabel);
            this.Controls.Add(this.truckMessageBoxLabel);
            this.Controls.Add(this.truckMessageBox);
            this.Controls.Add(this.saveImageButton);
            this.Controls.Add(this.getImageButton);
            this.Controls.Add(this.imageTabControl);
            this.Controls.Add(this.steeringSliderLabel);
            this.Controls.Add(this.speedLabel);
            this.Controls.Add(this.steeringSlider);
            this.Controls.Add(this.speedSlider);
            this.Controls.Add(this.courseTabControl);
            this.Controls.Add(this.modeComboBox);
            this.Controls.Add(this.stopButton);
            this.Controls.Add(this.startButton);
            this.Name = "RobotRacer";
            this.Text = "RobotRacer";
            this.Load += new System.EventHandler(this.RobotRacer_Load);
            this.courseTabControl.ResumeLayout(false);
            this.courseTextTab.ResumeLayout(false);
            this.courseTextTab.PerformLayout();
            this.serialTab.ResumeLayout(false);
            this.serialTab.PerformLayout();
            this.uartControlTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.speedSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.steeringSlider)).EndInit();
            this.imageTabControl.ResumeLayout(false);
            this.rawImageTab.ResumeLayout(false);
            this.pidTab.ResumeLayout(false);
            this.pidTab.PerformLayout();
            this.navigationTab.ResumeLayout(false);
            this.navigationTab.PerformLayout();
            this.registerTabControl.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        
        private System.Windows.Forms.Timer tmrUpdateStick;

        private void initializeCustomComponents()
        {
            RobotRacer.registerViewer = new RegisterViewer();
            this.registerTab.Controls.Add(RobotRacer.registerViewer);
            serialPort.ScanPorts(this.lstBoxUART.Items);

            String initialDirectory = System.IO.Directory.GetCurrentDirectory();// "C:\\TheKNACK\\Trunk\\RobotRacers_RTOS\\AutonomousCar";
            this.loadBinaryDialog.InitialDirectory = initialDirectory;

            String[] elfFiles = System.IO.Directory.GetFiles(initialDirectory, "*.elf");//System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory());
            String initialElfFile = (elfFiles.Length > 0 ? elfFiles[0] : "");
            changeBinaryFileName(initialElfFile); //update the elf filename
            this.txt_binaryFileName.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txt_binaryFileName_MouseClicked);


            this.processedImage = new ProcessedImage(false);
            this.processedImage.BackColor = Color.Black;
            this.processedImage.Location = new System.Drawing.Point(0, 0);
            this.processedImage.Name = "processedImage";
            this.processedImage = new ProcessedImage(false);
            this.processedImage.BackColor = Color.Black;
            this.processedImage.Location = new System.Drawing.Point(0, 0);
            this.processedImage.Name = "processedImage";
            this.processedImage.Size = new System.Drawing.Size(320, 240);
            this.processedImage.Size = new System.Drawing.Size(320, 240);
            this.processedImage.TabIndex = 0;
            this.processedImage.TabIndex = 0;
            ;
            this.processedImageTab.Controls.Add(this.processedImage);
            
            this.processedImageTab.Controls.Add(this.processedImage);
        }

        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.Button stopButton;
        private System.Windows.Forms.Button clearCourseButton;
        private System.Windows.Forms.Button loadCourseButton;
        private System.Windows.Forms.Button saveCourseButton;
        private System.Windows.Forms.Button sendCourseButton;
        private System.Windows.Forms.ComboBox modeComboBox;
        private System.Windows.Forms.TabControl courseTabControl;
        private System.Windows.Forms.TabPage courseTextTab;
        private System.Windows.Forms.Label speedSliderLabel;
        private System.Windows.Forms.Label steeringSliderLabel;
        private System.Windows.Forms.TabControl imageTabControl;
        private System.Windows.Forms.TabPage rawImageTab;
        private System.Windows.Forms.TabPage processedImageTab;
        private System.Windows.Forms.Button getImageButton;
        private System.Windows.Forms.Button saveImageButton;
        private System.Windows.Forms.TextBox truckMessageBox;
        private System.Windows.Forms.Label truckMessageBoxLabel;
        private System.Windows.Forms.Label stateLabel;
        private System.Windows.Forms.Label speedLabel;
        private System.Windows.Forms.Label truckState;
        private System.Windows.Forms.Label truckVelocityTextbox;
        private System.Windows.Forms.Button eStopButton;
        private System.Windows.Forms.Button startVideoButton;
        private System.Windows.Forms.Button messagesOnButton;
        private System.Windows.Forms.Button messagesOffButton;
        private System.Windows.Forms.Button viewRegistersButton;
        private System.Windows.Forms.Button stopVideoButton;
        private MouseControl mouseControlPanel;
        private System.Windows.Forms.SaveFileDialog saveCourseDialog;
        private System.Windows.Forms.OpenFileDialog loadCourseDialog;
        private SaveFileDialog saveHSVDialog;
        private OpenFileDialog openHSVDialog;
        private System.Windows.Forms.TabPage serialTab;
        private System.Windows.Forms.TextBox serialTextBox;
        private System.Windows.Forms.CheckBox rxCheckbox;
        private System.Windows.Forms.CheckBox txCheckbox;
        private Button clearBtn;
        private Button clearSerialButton;
        private SaveFileDialog saveImageDialog;
        private TabPage uartControlTab;
        public ListBox lstBoxUART;
        public Label lblImageCoords;
        public Label lblImageColorHex;
        private RawImage rawImage;
        public Label lblImageColorBinary;
        public TrackBar speedSlider;
        public TrackBar steeringSlider;
        public Button btn_packetLoss;
        private Button btn_transmitBinary;
        private TextBox txt_binaryFileName;
        private OpenFileDialog loadBinaryDialog;
        private Button btn_bootload;
        private Label label1;
        private ProcessedImage processedImage;
        private ComboBox ddbImageType;
        private Button button1;
        private Label label11;
        private Label lblFPS_HW;
        public TextBox courseTextBox;
        private Button buttonGetCriticalImage;
        public Label lblBytesPerSecond_Total;
        public Label lblBytesPerSecond2;
        private Label label49;
        private Label label57;
        private Label label56;
        private Label label55;
        private Label label54;
        private Label label53;
        private Label label52;
        private Label label51;
        private Label label50;
        public Label lblBytesPerSecond_Text;
        public Label lblBytesPerSecond_Primative;
        public Label lblBytesPerSecond_State;
        public Label lblBytesPerSecond_Command;
        public Label lblBytesPerSecond_Image;
        public Label lblBytesPerSecond_Course;
        public Label lblBytesPerSecond_Register;
        public Label lblBytesPerSecond_DataTrans;
        private Label label58;
        public Label lblBytesPerSecond_Ack;
        public Label lblBytesPerSecond_TotalPackets;
        private Label lblFPS_SW;
        private Label label60;
        private TabPage pidTab;
        private Button runButton;
        private Label orLabel;
        private Label distanceToRunLabel;
        private TextBox metersToRunTextbox;
        private TextBox turningTextbox;
        private TextBox numMillisecondsToRunTextbox;
        private TextBox desiredVelocityTextbox;
        private TextBox iMinTextbox;
        private TextBox iMaxTextbox;
        private TextBox kiTextbox;
        private TextBox kdTextbox;
        private TextBox kpTextbox;
        private Label turningLabel;
        private Label numMillisecondsLabel;
        private Button resetPIDValuesButton;
        private Label desiredVelocityLabel;
        private Label iMinLabel;
        private Label iMaxLabel;
        private Label kiLabel;
        private Label kdLabel;
        private Label kpLabel;
        private Button sendPidButton;
        private TabPage registerTab;
        private TabPage navigationTab;
        private Label label22;
        private Label label21;
        private TextBox findRangeTextbox;
        public TextBox turnAngleTextbox;
        private TextBox turnRadiusTextbox;
        public TextBox straightSpeedTextbox;
        public TextBox blindTurnTextbox;
        public TextBox blindStraightTextbox;
        private Label label20;
        private Label label19;
        private Label label18;
        private Label label17;
        private Label label16;
        private Label label15;
        private Label label14;
        private Label label13;
        private Button button2;
        private Label label;
        private Label label12;
        private Label distanceToBlindLabel;
        private Label distanceTraveledLabel;
        private Label carStateLabel;
        private Label label10;
        private Label label9;
        private Label label6;
        private TabControl registerTabControl;
        

        
    }

    
}

