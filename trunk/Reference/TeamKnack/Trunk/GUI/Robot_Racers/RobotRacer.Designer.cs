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
            if (this.truckVelocityTextbox.InvokeRequired ||
                this.truckSentAngle.InvokeRequired )
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
            this.truckSentAngle.Text = RobotRacer.stateVariables.SentSteeringAngle + " Deg";

            this.pylonNumLabel.Text = RobotRacer.stateVariables.PylonNumber + "";

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
            this.distanceToPylonLabel.Text = String.Format("{0:F2}",RobotRacer.stateVariables.DistanceToPylon) + " m";
            this.distanceTraveledLabel.Text = String.Format("{0:F2}",RobotRacer.stateVariables.DistanceTraveled) + " m";
            this.angleOfPylonLabel.Text = RobotRacer.stateVariables.AngleToPylon + " deg";
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
            this.courseGraphicTab = new System.Windows.Forms.TabPage();
            this.courseGraphicalPanel = new Robot_Racers.CourseGraphicsPanel();
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
            this.tab3DRacer = new System.Windows.Forms.TabPage();
            this.xnaRoboRacerDisplay1 = new Robot_Racers.XNAGame.xnaRoboRacerDisplay();
            this.getImageButton = new System.Windows.Forms.Button();
            this.saveImageButton = new System.Windows.Forms.Button();
            this.truckMessageBox = new System.Windows.Forms.TextBox();
            this.truckMessageBoxLabel = new System.Windows.Forms.Label();
            this.stateLabel = new System.Windows.Forms.Label();
            this.speedLabel = new System.Windows.Forms.Label();
            this.sentAngleLabel = new System.Windows.Forms.Label();
            this.truckState = new System.Windows.Forms.Label();
            this.truckVelocityTextbox = new System.Windows.Forms.Label();
            this.truckSentAngle = new System.Windows.Forms.Label();
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
            this.registerTabControl = new System.Windows.Forms.TabControl();
            this.navigationTab = new System.Windows.Forms.TabPage();
            this.label22 = new System.Windows.Forms.Label();
            this.label21 = new System.Windows.Forms.Label();
            this.findRangeTextbox = new System.Windows.Forms.TextBox();
            this.label20 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.turnAngleTextbox = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.label = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.turnRadiusTextbox = new System.Windows.Forms.TextBox();
            this.straightSpeedTextbox = new System.Windows.Forms.TextBox();
            this.blindTurnTextbox = new System.Windows.Forms.TextBox();
            this.blindStraightTextbox = new System.Windows.Forms.TextBox();
            this.distanceToBlindLabel = new System.Windows.Forms.Label();
            this.distanceTraveledLabel = new System.Windows.Forms.Label();
            this.angleOfPylonLabel = new System.Windows.Forms.Label();
            this.distanceToPylonLabel = new System.Windows.Forms.Label();
            this.carStateLabel = new System.Windows.Forms.Label();
            this.pylonNumLabel = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.registerTab = new System.Windows.Forms.TabPage();
            this.pidTab = new System.Windows.Forms.TabPage();
            this.runButton = new System.Windows.Forms.Button();
            this.orLabel = new System.Windows.Forms.Label();
            this.distanceToRunLabel = new System.Windows.Forms.Label();
            this.metersToRunTextbox = new System.Windows.Forms.TextBox();
            this.turningLabel = new System.Windows.Forms.Label();
            this.turningTextbox = new System.Windows.Forms.TextBox();
            this.numMillisecondsLabel = new System.Windows.Forms.Label();
            this.numMillisecondsToRunTextbox = new System.Windows.Forms.TextBox();
            this.resetPIDValuesButton = new System.Windows.Forms.Button();
            this.desiredVelocityLabel = new System.Windows.Forms.Label();
            this.desiredVelocityTextbox = new System.Windows.Forms.TextBox();
            this.iMinLabel = new System.Windows.Forms.Label();
            this.iMaxLabel = new System.Windows.Forms.Label();
            this.iMinTextbox = new System.Windows.Forms.TextBox();
            this.iMaxTextbox = new System.Windows.Forms.TextBox();
            this.kiLabel = new System.Windows.Forms.Label();
            this.kdLabel = new System.Windows.Forms.Label();
            this.kpLabel = new System.Windows.Forms.Label();
            this.sendPidButton = new System.Windows.Forms.Button();
            this.kiTextbox = new System.Windows.Forms.TextBox();
            this.kdTextbox = new System.Windows.Forms.TextBox();
            this.kpTextbox = new System.Windows.Forms.TextBox();
            this.hsvTab = new System.Windows.Forms.TabPage();
            this.convThreshold_text = new System.Windows.Forms.Label();
            this.convThreshold_trackbar = new System.Windows.Forms.TrackBar();
            this.label35 = new System.Windows.Forms.Label();
            this.label36 = new System.Windows.Forms.Label();
            this.label37 = new System.Windows.Forms.Label();
            this.label38 = new System.Windows.Forms.Label();
            this.label39 = new System.Windows.Forms.Label();
            this.label40 = new System.Windows.Forms.Label();
            this.label41 = new System.Windows.Forms.Label();
            this.label42 = new System.Windows.Forms.Label();
            this.label43 = new System.Windows.Forms.Label();
            this.label44 = new System.Windows.Forms.Label();
            this.label45 = new System.Windows.Forms.Label();
            this.label46 = new System.Windows.Forms.Label();
            this.label47 = new System.Windows.Forms.Label();
            this.setAllDynamicRangeFromSlider = new System.Windows.Forms.Button();
            this.mirrorHSVtextbox = new System.Windows.Forms.TextBox();
            this.setDynamicRangeFromSlider = new System.Windows.Forms.Button();
            this.selectHSVline2 = new System.Windows.Forms.ListBox();
            this.greenRadio = new System.Windows.Forms.RadioButton();
            this.orangeRadio = new System.Windows.Forms.RadioButton();
            this.vRangeLabel = new System.Windows.Forms.Label();
            this.sRangeLabel = new System.Windows.Forms.Label();
            this.hRangeLabel = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.vLowSlider = new System.Windows.Forms.TrackBar();
            this.vHighSlider = new System.Windows.Forms.TrackBar();
            this.sLowSlider = new System.Windows.Forms.TrackBar();
            this.sHighSlider = new System.Windows.Forms.TrackBar();
            this.hLowSlider = new System.Windows.Forms.TrackBar();
            this.hHighSlider = new System.Windows.Forms.TrackBar();
            this.DynamicHSVPage = new System.Windows.Forms.TabPage();
            this.getHSVline = new System.Windows.Forms.Button();
            this.HSVDataGrid = new System.Windows.Forms.DataGridView();
            this.GHlo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GHhi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GSlo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GShi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GVlo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.GVhi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OHlo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OHhi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OSlo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OShi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OVlo = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.OVhi = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.setHSVline = new System.Windows.Forms.Button();
            this.selectHSVline1 = new System.Windows.Forms.ListBox();
            this.label34 = new System.Windows.Forms.Label();
            this.label33 = new System.Windows.Forms.Label();
            this.label32 = new System.Windows.Forms.Label();
            this.label31 = new System.Windows.Forms.Label();
            this.label30 = new System.Windows.Forms.Label();
            this.label29 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.label26 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.label23 = new System.Windows.Forms.Label();
            this.loadHSVButton = new System.Windows.Forms.Button();
            this.saveHSVbutton = new System.Windows.Forms.Button();
            this.dynamicHSVTextBox = new System.Windows.Forms.TextBox();
            this.DynamicHSV_xmit_all = new System.Windows.Forms.Button();
            this.DynamicHSV_rx_all = new System.Windows.Forms.Button();
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
            this.courseTabControl.SuspendLayout();
            this.courseTextTab.SuspendLayout();
            this.serialTab.SuspendLayout();
            this.courseGraphicTab.SuspendLayout();
            this.uartControlTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.speedSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.steeringSlider)).BeginInit();
            this.imageTabControl.SuspendLayout();
            this.rawImageTab.SuspendLayout();
            this.tab3DRacer.SuspendLayout();
            this.registerTabControl.SuspendLayout();
            this.navigationTab.SuspendLayout();
            this.pidTab.SuspendLayout();
            this.hsvTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.convThreshold_trackbar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.vLowSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.vHighSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sLowSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sHighSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hLowSlider)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.hHighSlider)).BeginInit();
            this.DynamicHSVPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HSVDataGrid)).BeginInit();
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
            this.courseTabControl.Controls.Add(this.courseGraphicTab);
            this.courseTabControl.Controls.Add(this.uartControlTab);
            this.courseTabControl.Location = new System.Drawing.Point(468, 0);
            this.courseTabControl.Name = "courseTabControl";
            this.courseTabControl.SelectedIndex = 0;
            this.courseTabControl.Size = new System.Drawing.Size(596, 198);
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
            this.courseTextTab.Size = new System.Drawing.Size(588, 172);
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
            // courseGraphicTab
            // 
            this.courseGraphicTab.Controls.Add(this.courseGraphicalPanel);
            this.courseGraphicTab.Location = new System.Drawing.Point(4, 22);
            this.courseGraphicTab.Name = "courseGraphicTab";
            this.courseGraphicTab.Padding = new System.Windows.Forms.Padding(3);
            this.courseGraphicTab.Size = new System.Drawing.Size(588, 172);
            this.courseGraphicTab.TabIndex = 1;
            this.courseGraphicTab.Text = "Course Image";
            this.courseGraphicTab.UseVisualStyleBackColor = true;
            // 
            // courseGraphicalPanel
            // 
            this.courseGraphicalPanel.Location = new System.Drawing.Point(0, 0);
            this.courseGraphicalPanel.Name = "courseGraphicalPanel";
            this.courseGraphicalPanel.Size = new System.Drawing.Size(400, 172);
            this.courseGraphicalPanel.TabIndex = 0;
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
            this.imageTabControl.Controls.Add(this.tab3DRacer);
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
            // tab3DRacer
            // 
            this.tab3DRacer.Controls.Add(this.xnaRoboRacerDisplay1);
            this.tab3DRacer.Location = new System.Drawing.Point(4, 22);
            this.tab3DRacer.Name = "tab3DRacer";
            this.tab3DRacer.Padding = new System.Windows.Forms.Padding(3);
            this.tab3DRacer.Size = new System.Drawing.Size(320, 240);
            this.tab3DRacer.TabIndex = 2;
            this.tab3DRacer.Text = "3D Racer";
            this.tab3DRacer.UseVisualStyleBackColor = true;
            // 
            // xnaRoboRacerDisplay1
            // 
            this.xnaRoboRacerDisplay1.Location = new System.Drawing.Point(0, 0);
            this.xnaRoboRacerDisplay1.Name = "xnaRoboRacerDisplay1";
            this.xnaRoboRacerDisplay1.Size = new System.Drawing.Size(320, 240);
            this.xnaRoboRacerDisplay1.TabIndex = 0;
            this.xnaRoboRacerDisplay1.Text = "xnaRoboRacerGame1";
            this.xnaRoboRacerDisplay1.Click += new System.EventHandler(this.xnaRoboRacerGame1_Click);
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
            this.truckMessageBox.Size = new System.Drawing.Size(323, 449);
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
            // sentAngleLabel
            // 
            this.sentAngleLabel.AutoSize = true;
            this.sentAngleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.sentAngleLabel.Location = new System.Drawing.Point(104, 315);
            this.sentAngleLabel.Name = "sentAngleLabel";
            this.sentAngleLabel.Size = new System.Drawing.Size(87, 16);
            this.sentAngleLabel.TabIndex = 21;
            this.sentAngleLabel.Text = "Sent Angle:";
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
            // truckSentAngle
            // 
            this.truckSentAngle.AutoSize = true;
            this.truckSentAngle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.truckSentAngle.Location = new System.Drawing.Point(197, 315);
            this.truckSentAngle.Name = "truckSentAngle";
            this.truckSentAngle.Size = new System.Drawing.Size(44, 16);
            this.truckSentAngle.TabIndex = 25;
            this.truckSentAngle.Text = "0 Deg";
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
            // saveHSVDialog
            // 
            this.saveHSVDialog.Filter = "HSV range files (*.hsv)|*.hsv";
            this.saveHSVDialog.InitialDirectory = "C:\\HSVranges";
            this.saveHSVDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.saveHSVDialog_FileOk);
            // 
            // openHSVDialog
            // 
            this.openHSVDialog.DefaultExt = "nak";
            this.openHSVDialog.FileName = "openHSVDialog";
            this.openHSVDialog.Filter = "HSV range files (*.hsv)|*.hsv";
            this.openHSVDialog.InitialDirectory = "C:\\HSVRanges";
            this.openHSVDialog.FileOk += new System.ComponentModel.CancelEventHandler(this.openHSVDialog_FileOk);
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
            // registerTabControl
            // 
            this.registerTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.registerTabControl.Controls.Add(this.navigationTab);
            this.registerTabControl.Controls.Add(this.registerTab);
            this.registerTabControl.Controls.Add(this.pidTab);
            this.registerTabControl.Controls.Add(this.hsvTab);
            this.registerTabControl.Controls.Add(this.DynamicHSVPage);
            this.registerTabControl.Location = new System.Drawing.Point(429, 351);
            this.registerTabControl.Name = "registerTabControl";
            this.registerTabControl.SelectedIndex = 0;
            this.registerTabControl.Size = new System.Drawing.Size(635, 449);
            this.registerTabControl.TabIndex = 36;
            // 
            // navigationTab
            // 
            this.navigationTab.Controls.Add(this.label22);
            this.navigationTab.Controls.Add(this.label21);
            this.navigationTab.Controls.Add(this.findRangeTextbox);
            this.navigationTab.Controls.Add(this.label20);
            this.navigationTab.Controls.Add(this.label19);
            this.navigationTab.Controls.Add(this.turnAngleTextbox);
            this.navigationTab.Controls.Add(this.label18);
            this.navigationTab.Controls.Add(this.label17);
            this.navigationTab.Controls.Add(this.label16);
            this.navigationTab.Controls.Add(this.label15);
            this.navigationTab.Controls.Add(this.label14);
            this.navigationTab.Controls.Add(this.label13);
            this.navigationTab.Controls.Add(this.button2);
            this.navigationTab.Controls.Add(this.label);
            this.navigationTab.Controls.Add(this.label12);
            this.navigationTab.Controls.Add(this.turnRadiusTextbox);
            this.navigationTab.Controls.Add(this.straightSpeedTextbox);
            this.navigationTab.Controls.Add(this.blindTurnTextbox);
            this.navigationTab.Controls.Add(this.blindStraightTextbox);
            this.navigationTab.Controls.Add(this.distanceToBlindLabel);
            this.navigationTab.Controls.Add(this.distanceTraveledLabel);
            this.navigationTab.Controls.Add(this.angleOfPylonLabel);
            this.navigationTab.Controls.Add(this.distanceToPylonLabel);
            this.navigationTab.Controls.Add(this.carStateLabel);
            this.navigationTab.Controls.Add(this.pylonNumLabel);
            this.navigationTab.Controls.Add(this.label10);
            this.navigationTab.Controls.Add(this.label9);
            this.navigationTab.Controls.Add(this.label8);
            this.navigationTab.Controls.Add(this.label7);
            this.navigationTab.Controls.Add(this.label6);
            this.navigationTab.Controls.Add(this.label5);
            this.navigationTab.Location = new System.Drawing.Point(4, 22);
            this.navigationTab.Name = "navigationTab";
            this.navigationTab.Size = new System.Drawing.Size(627, 423);
            this.navigationTab.TabIndex = 3;
            this.navigationTab.Text = "Navigation";
            this.navigationTab.UseVisualStyleBackColor = true;
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
            // label21
            // 
            this.label21.AutoSize = true;
            this.label21.Location = new System.Drawing.Point(269, 150);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(62, 13);
            this.label21.TabIndex = 33;
            this.label21.Text = "Find Range";
            // 
            // findRangeTextbox
            // 
            this.findRangeTextbox.Location = new System.Drawing.Point(337, 143);
            this.findRangeTextbox.Name = "findRangeTextbox";
            this.findRangeTextbox.Size = new System.Drawing.Size(38, 20);
            this.findRangeTextbox.TabIndex = 32;
            this.findRangeTextbox.Text = "250";
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
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(272, 123);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(59, 13);
            this.label19.TabIndex = 30;
            this.label19.Text = "Turn Angle";
            // 
            // turnAngleTextbox
            // 
            this.turnAngleTextbox.Location = new System.Drawing.Point(337, 116);
            this.turnAngleTextbox.Name = "turnAngleTextbox";
            this.turnAngleTextbox.Size = new System.Drawing.Size(38, 20);
            this.turnAngleTextbox.TabIndex = 29;
            this.turnAngleTextbox.Text = "55";
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
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(383, 70);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(25, 13);
            this.label17.TabIndex = 27;
            this.label17.Text = "m/s";
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
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(383, 16);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(25, 13);
            this.label15.TabIndex = 25;
            this.label15.Text = "m/s";
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
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(254, 70);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(77, 13);
            this.label13.TabIndex = 23;
            this.label13.Text = "Straight Speed";
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
            // label
            // 
            this.label.AutoSize = true;
            this.label.Location = new System.Drawing.Point(276, 43);
            this.label.Name = "label";
            this.label.Size = new System.Drawing.Size(55, 13);
            this.label.TabIndex = 21;
            this.label.Text = "Blind Turn";
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
            // turnRadiusTextbox
            // 
            this.turnRadiusTextbox.Location = new System.Drawing.Point(337, 89);
            this.turnRadiusTextbox.Name = "turnRadiusTextbox";
            this.turnRadiusTextbox.Size = new System.Drawing.Size(39, 20);
            this.turnRadiusTextbox.TabIndex = 19;
            this.turnRadiusTextbox.Text = "0.55";
            this.turnRadiusTextbox.TextChanged += new System.EventHandler(this.turnRadiusTextbox_TextChanged);
            // 
            // straightSpeedTextbox
            // 
            this.straightSpeedTextbox.Location = new System.Drawing.Point(337, 63);
            this.straightSpeedTextbox.Name = "straightSpeedTextbox";
            this.straightSpeedTextbox.Size = new System.Drawing.Size(39, 20);
            this.straightSpeedTextbox.TabIndex = 18;
            this.straightSpeedTextbox.Text = "2.0";
            // 
            // blindTurnTextbox
            // 
            this.blindTurnTextbox.Location = new System.Drawing.Point(337, 36);
            this.blindTurnTextbox.Name = "blindTurnTextbox";
            this.blindTurnTextbox.Size = new System.Drawing.Size(39, 20);
            this.blindTurnTextbox.TabIndex = 17;
            this.blindTurnTextbox.Text = "1.5";
            // 
            // blindStraightTextbox
            // 
            this.blindStraightTextbox.Location = new System.Drawing.Point(337, 10);
            this.blindStraightTextbox.Name = "blindStraightTextbox";
            this.blindStraightTextbox.Size = new System.Drawing.Size(39, 20);
            this.blindStraightTextbox.TabIndex = 16;
            this.blindStraightTextbox.Text = "1.75";
            // 
            // distanceToBlindLabel
            // 
            this.distanceToBlindLabel.AutoSize = true;
            this.distanceToBlindLabel.Location = new System.Drawing.Point(124, 177);
            this.distanceToBlindLabel.Name = "distanceToBlindLabel";
            this.distanceToBlindLabel.Size = new System.Drawing.Size(33, 13);
            this.distanceToBlindLabel.TabIndex = 15;
            this.distanceToBlindLabel.Text = "0.0 m";
            // 
            // distanceTraveledLabel
            // 
            this.distanceTraveledLabel.AutoSize = true;
            this.distanceTraveledLabel.Location = new System.Drawing.Point(125, 150);
            this.distanceTraveledLabel.Name = "distanceTraveledLabel";
            this.distanceTraveledLabel.Size = new System.Drawing.Size(33, 13);
            this.distanceTraveledLabel.TabIndex = 14;
            this.distanceTraveledLabel.Text = "0.0 m";
            // 
            // angleOfPylonLabel
            // 
            this.angleOfPylonLabel.AutoSize = true;
            this.angleOfPylonLabel.Location = new System.Drawing.Point(124, 121);
            this.angleOfPylonLabel.Name = "angleOfPylonLabel";
            this.angleOfPylonLabel.Size = new System.Drawing.Size(34, 13);
            this.angleOfPylonLabel.TabIndex = 13;
            this.angleOfPylonLabel.Text = "0 deg";
            // 
            // distanceToPylonLabel
            // 
            this.distanceToPylonLabel.AutoSize = true;
            this.distanceToPylonLabel.Location = new System.Drawing.Point(124, 89);
            this.distanceToPylonLabel.Name = "distanceToPylonLabel";
            this.distanceToPylonLabel.Size = new System.Drawing.Size(33, 13);
            this.distanceToPylonLabel.TabIndex = 12;
            this.distanceToPylonLabel.Text = "0.0 m";
            // 
            // carStateLabel
            // 
            this.carStateLabel.AutoSize = true;
            this.carStateLabel.Location = new System.Drawing.Point(124, 60);
            this.carStateLabel.Name = "carStateLabel";
            this.carStateLabel.Size = new System.Drawing.Size(81, 13);
            this.carStateLabel.TabIndex = 11;
            this.carStateLabel.Text = "Unknown State";
            // 
            // pylonNumLabel
            // 
            this.pylonNumLabel.AutoSize = true;
            this.pylonNumLabel.Location = new System.Drawing.Point(125, 29);
            this.pylonNumLabel.Name = "pylonNumLabel";
            this.pylonNumLabel.Size = new System.Drawing.Size(13, 13);
            this.pylonNumLabel.TabIndex = 10;
            this.pylonNumLabel.Text = "0";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(10, 177);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(108, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "Distance to Blind:";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(3, 150);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(115, 13);
            this.label9.TabIndex = 8;
            this.label9.Text = "Distance Traveled:";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(25, 121);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(93, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Angle of Pylon:";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(8, 89);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(110, 13);
            this.label7.TabIndex = 6;
            this.label7.Text = "Distance to pylon:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(54, 60);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(64, 13);
            this.label6.TabIndex = 5;
            this.label6.Text = "Car State:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(64, 30);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(54, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Pylon #:";
            // 
            // registerTab
            // 
            this.registerTab.BackColor = System.Drawing.Color.White;
            this.registerTab.Location = new System.Drawing.Point(4, 22);
            this.registerTab.Name = "registerTab";
            this.registerTab.Padding = new System.Windows.Forms.Padding(3);
            this.registerTab.Size = new System.Drawing.Size(627, 423);
            this.registerTab.TabIndex = 0;
            this.registerTab.Text = "Registers";
            this.registerTab.UseVisualStyleBackColor = true;
            // 
            // pidTab
            // 
            this.pidTab.Controls.Add(this.runButton);
            this.pidTab.Controls.Add(this.orLabel);
            this.pidTab.Controls.Add(this.distanceToRunLabel);
            this.pidTab.Controls.Add(this.metersToRunTextbox);
            this.pidTab.Controls.Add(this.turningLabel);
            this.pidTab.Controls.Add(this.turningTextbox);
            this.pidTab.Controls.Add(this.numMillisecondsLabel);
            this.pidTab.Controls.Add(this.numMillisecondsToRunTextbox);
            this.pidTab.Controls.Add(this.resetPIDValuesButton);
            this.pidTab.Controls.Add(this.desiredVelocityLabel);
            this.pidTab.Controls.Add(this.desiredVelocityTextbox);
            this.pidTab.Controls.Add(this.iMinLabel);
            this.pidTab.Controls.Add(this.iMaxLabel);
            this.pidTab.Controls.Add(this.iMinTextbox);
            this.pidTab.Controls.Add(this.iMaxTextbox);
            this.pidTab.Controls.Add(this.kiLabel);
            this.pidTab.Controls.Add(this.kdLabel);
            this.pidTab.Controls.Add(this.kpLabel);
            this.pidTab.Controls.Add(this.sendPidButton);
            this.pidTab.Controls.Add(this.kiTextbox);
            this.pidTab.Controls.Add(this.kdTextbox);
            this.pidTab.Controls.Add(this.kpTextbox);
            this.pidTab.Location = new System.Drawing.Point(4, 22);
            this.pidTab.Name = "pidTab";
            this.pidTab.Padding = new System.Windows.Forms.Padding(3);
            this.pidTab.Size = new System.Drawing.Size(627, 423);
            this.pidTab.TabIndex = 1;
            this.pidTab.Text = "PID Controller";
            this.pidTab.UseVisualStyleBackColor = true;
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
            // orLabel
            // 
            this.orLabel.AutoSize = true;
            this.orLabel.Location = new System.Drawing.Point(329, 39);
            this.orLabel.Name = "orLabel";
            this.orLabel.Size = new System.Drawing.Size(23, 13);
            this.orLabel.TabIndex = 22;
            this.orLabel.Text = "OR";
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
            // metersToRunTextbox
            // 
            this.metersToRunTextbox.Location = new System.Drawing.Point(345, 56);
            this.metersToRunTextbox.Name = "metersToRunTextbox";
            this.metersToRunTextbox.Size = new System.Drawing.Size(40, 20);
            this.metersToRunTextbox.TabIndex = 20;
            this.metersToRunTextbox.Text = "0.0";
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
            // turningTextbox
            // 
            this.turningTextbox.Location = new System.Drawing.Point(206, 39);
            this.turningTextbox.Name = "turningTextbox";
            this.turningTextbox.Size = new System.Drawing.Size(40, 20);
            this.turningTextbox.TabIndex = 17;
            this.turningTextbox.Text = "0";
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
            // numMillisecondsToRunTextbox
            // 
            this.numMillisecondsToRunTextbox.Location = new System.Drawing.Point(345, 13);
            this.numMillisecondsToRunTextbox.Name = "numMillisecondsToRunTextbox";
            this.numMillisecondsToRunTextbox.Size = new System.Drawing.Size(40, 20);
            this.numMillisecondsToRunTextbox.TabIndex = 15;
            this.numMillisecondsToRunTextbox.Text = "0";
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
            // desiredVelocityLabel
            // 
            this.desiredVelocityLabel.AutoSize = true;
            this.desiredVelocityLabel.Location = new System.Drawing.Point(130, 19);
            this.desiredVelocityLabel.Name = "desiredVelocityLabel";
            this.desiredVelocityLabel.Size = new System.Drawing.Size(71, 13);
            this.desiredVelocityLabel.TabIndex = 12;
            this.desiredVelocityLabel.Text = "Velocity (m/s)";
            // 
            // desiredVelocityTextbox
            // 
            this.desiredVelocityTextbox.Location = new System.Drawing.Point(206, 13);
            this.desiredVelocityTextbox.Name = "desiredVelocityTextbox";
            this.desiredVelocityTextbox.Size = new System.Drawing.Size(40, 20);
            this.desiredVelocityTextbox.TabIndex = 11;
            this.desiredVelocityTextbox.Text = "0.0";
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
            // iMaxLabel
            // 
            this.iMaxLabel.AutoSize = true;
            this.iMaxLabel.Location = new System.Drawing.Point(7, 100);
            this.iMaxLabel.Name = "iMaxLabel";
            this.iMaxLabel.Size = new System.Drawing.Size(29, 13);
            this.iMaxLabel.TabIndex = 9;
            this.iMaxLabel.Text = "iMax";
            // 
            // iMinTextbox
            // 
            this.iMinTextbox.Location = new System.Drawing.Point(43, 121);
            this.iMinTextbox.Name = "iMinTextbox";
            this.iMinTextbox.Size = new System.Drawing.Size(66, 20);
            this.iMinTextbox.TabIndex = 8;
            this.iMinTextbox.Text = "-1.5";
            // 
            // iMaxTextbox
            // 
            this.iMaxTextbox.Location = new System.Drawing.Point(43, 94);
            this.iMaxTextbox.Name = "iMaxTextbox";
            this.iMaxTextbox.Size = new System.Drawing.Size(66, 20);
            this.iMaxTextbox.TabIndex = 7;
            this.iMaxTextbox.Text = "1.5";
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
            // kdLabel
            // 
            this.kdLabel.AutoSize = true;
            this.kdLabel.Location = new System.Drawing.Point(7, 45);
            this.kdLabel.Name = "kdLabel";
            this.kdLabel.Size = new System.Drawing.Size(21, 13);
            this.kdLabel.TabIndex = 5;
            this.kdLabel.Text = "kD";
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
            // kiTextbox
            // 
            this.kiTextbox.Location = new System.Drawing.Point(43, 68);
            this.kiTextbox.Name = "kiTextbox";
            this.kiTextbox.Size = new System.Drawing.Size(66, 20);
            this.kiTextbox.TabIndex = 2;
            this.kiTextbox.Text = "1.0";
            // 
            // kdTextbox
            // 
            this.kdTextbox.Location = new System.Drawing.Point(43, 39);
            this.kdTextbox.Name = "kdTextbox";
            this.kdTextbox.Size = new System.Drawing.Size(66, 20);
            this.kdTextbox.TabIndex = 1;
            this.kdTextbox.Text = "0.05";
            // 
            // kpTextbox
            // 
            this.kpTextbox.Location = new System.Drawing.Point(43, 13);
            this.kpTextbox.Name = "kpTextbox";
            this.kpTextbox.Size = new System.Drawing.Size(66, 20);
            this.kpTextbox.TabIndex = 0;
            this.kpTextbox.Text = "55.5";
            // 
            // hsvTab
            // 
            this.hsvTab.Controls.Add(this.convThreshold_text);
            this.hsvTab.Controls.Add(this.convThreshold_trackbar);
            this.hsvTab.Controls.Add(this.label35);
            this.hsvTab.Controls.Add(this.label36);
            this.hsvTab.Controls.Add(this.label37);
            this.hsvTab.Controls.Add(this.label38);
            this.hsvTab.Controls.Add(this.label39);
            this.hsvTab.Controls.Add(this.label40);
            this.hsvTab.Controls.Add(this.label41);
            this.hsvTab.Controls.Add(this.label42);
            this.hsvTab.Controls.Add(this.label43);
            this.hsvTab.Controls.Add(this.label44);
            this.hsvTab.Controls.Add(this.label45);
            this.hsvTab.Controls.Add(this.label46);
            this.hsvTab.Controls.Add(this.label47);
            this.hsvTab.Controls.Add(this.setAllDynamicRangeFromSlider);
            this.hsvTab.Controls.Add(this.mirrorHSVtextbox);
            this.hsvTab.Controls.Add(this.setDynamicRangeFromSlider);
            this.hsvTab.Controls.Add(this.selectHSVline2);
            this.hsvTab.Controls.Add(this.greenRadio);
            this.hsvTab.Controls.Add(this.orangeRadio);
            this.hsvTab.Controls.Add(this.vRangeLabel);
            this.hsvTab.Controls.Add(this.sRangeLabel);
            this.hsvTab.Controls.Add(this.hRangeLabel);
            this.hsvTab.Controls.Add(this.label4);
            this.hsvTab.Controls.Add(this.label3);
            this.hsvTab.Controls.Add(this.label2);
            this.hsvTab.Controls.Add(this.vLowSlider);
            this.hsvTab.Controls.Add(this.vHighSlider);
            this.hsvTab.Controls.Add(this.sLowSlider);
            this.hsvTab.Controls.Add(this.sHighSlider);
            this.hsvTab.Controls.Add(this.hLowSlider);
            this.hsvTab.Controls.Add(this.hHighSlider);
            this.hsvTab.Location = new System.Drawing.Point(4, 22);
            this.hsvTab.Name = "hsvTab";
            this.hsvTab.Size = new System.Drawing.Size(627, 423);
            this.hsvTab.TabIndex = 2;
            this.hsvTab.Text = "HSV";
            this.hsvTab.UseVisualStyleBackColor = true;
            // 
            // convThreshold_text
            // 
            this.convThreshold_text.AutoSize = true;
            this.convThreshold_text.Location = new System.Drawing.Point(573, 38);
            this.convThreshold_text.Name = "convThreshold_text";
            this.convThreshold_text.Size = new System.Drawing.Size(13, 13);
            this.convThreshold_text.TabIndex = 52;
            this.convThreshold_text.Text = "0";
            // 
            // convThreshold_trackbar
            // 
            this.convThreshold_trackbar.BackColor = System.Drawing.SystemColors.Window;
            this.convThreshold_trackbar.Location = new System.Drawing.Point(457, 38);
            this.convThreshold_trackbar.Maximum = 100;
            this.convThreshold_trackbar.Name = "convThreshold_trackbar";
            this.convThreshold_trackbar.Size = new System.Drawing.Size(110, 45);
            this.convThreshold_trackbar.TabIndex = 50;
            this.convThreshold_trackbar.TickFrequency = 25;
            this.convThreshold_trackbar.Scroll += new System.EventHandler(this.ConvThreshold_trackbar_Scroll);
            // 
            // label35
            // 
            this.label35.AutoSize = true;
            this.label35.Location = new System.Drawing.Point(11, 375);
            this.label35.Name = "label35";
            this.label35.Size = new System.Drawing.Size(46, 13);
            this.label35.TabIndex = 49;
            this.label35.Text = "315-344";
            this.label35.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label36
            // 
            this.label36.AutoSize = true;
            this.label36.Location = new System.Drawing.Point(11, 362);
            this.label36.Name = "label36";
            this.label36.Size = new System.Drawing.Size(46, 13);
            this.label36.TabIndex = 48;
            this.label36.Text = "285-314";
            this.label36.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label37
            // 
            this.label37.AutoSize = true;
            this.label37.Location = new System.Drawing.Point(11, 349);
            this.label37.Name = "label37";
            this.label37.Size = new System.Drawing.Size(46, 13);
            this.label37.TabIndex = 47;
            this.label37.Text = "255-284";
            this.label37.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label38
            // 
            this.label38.AutoSize = true;
            this.label38.Location = new System.Drawing.Point(11, 336);
            this.label38.Name = "label38";
            this.label38.Size = new System.Drawing.Size(46, 13);
            this.label38.TabIndex = 46;
            this.label38.Text = "225-254";
            this.label38.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label39
            // 
            this.label39.AutoSize = true;
            this.label39.Location = new System.Drawing.Point(11, 323);
            this.label39.Name = "label39";
            this.label39.Size = new System.Drawing.Size(46, 13);
            this.label39.TabIndex = 45;
            this.label39.Text = "195-224";
            this.label39.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Location = new System.Drawing.Point(11, 310);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(46, 13);
            this.label40.TabIndex = 44;
            this.label40.Text = "165-194";
            this.label40.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label41
            // 
            this.label41.AutoSize = true;
            this.label41.Location = new System.Drawing.Point(11, 297);
            this.label41.Name = "label41";
            this.label41.Size = new System.Drawing.Size(46, 13);
            this.label41.TabIndex = 43;
            this.label41.Text = "135-164";
            this.label41.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(11, 284);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(46, 13);
            this.label42.TabIndex = 42;
            this.label42.Text = "105-134";
            this.label42.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(17, 271);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(40, 13);
            this.label43.TabIndex = 41;
            this.label43.Text = "75-104";
            this.label43.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label44
            // 
            this.label44.AutoSize = true;
            this.label44.Location = new System.Drawing.Point(23, 258);
            this.label44.Name = "label44";
            this.label44.Size = new System.Drawing.Size(34, 13);
            this.label44.TabIndex = 40;
            this.label44.Text = "45-74";
            this.label44.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label45
            // 
            this.label45.AutoSize = true;
            this.label45.Location = new System.Drawing.Point(23, 245);
            this.label45.Name = "label45";
            this.label45.Size = new System.Drawing.Size(34, 13);
            this.label45.TabIndex = 39;
            this.label45.Text = "15-44";
            this.label45.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label46
            // 
            this.label46.AutoSize = true;
            this.label46.Location = new System.Drawing.Point(17, 232);
            this.label46.Name = "label46";
            this.label46.Size = new System.Drawing.Size(40, 13);
            this.label46.TabIndex = 38;
            this.label46.Text = "345-14";
            this.label46.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label47
            // 
            this.label47.AutoSize = true;
            this.label47.Location = new System.Drawing.Point(454, 19);
            this.label47.Name = "label47";
            this.label47.Size = new System.Drawing.Size(113, 13);
            this.label47.TabIndex = 36;
            this.label47.Text = "Convolution Threshold";
            // 
            // setAllDynamicRangeFromSlider
            // 
            this.setAllDynamicRangeFromSlider.Location = new System.Drawing.Point(457, 229);
            this.setAllDynamicRangeFromSlider.Name = "setAllDynamicRangeFromSlider";
            this.setAllDynamicRangeFromSlider.Size = new System.Drawing.Size(132, 34);
            this.setAllDynamicRangeFromSlider.TabIndex = 33;
            this.setAllDynamicRangeFromSlider.Text = "Set Dynamic Range for ALL ANGLES";
            this.setAllDynamicRangeFromSlider.UseVisualStyleBackColor = true;
            this.setAllDynamicRangeFromSlider.Click += new System.EventHandler(this.setAllDynamicRangeFromSlider_Click);
            // 
            // mirrorHSVtextbox
            // 
            this.mirrorHSVtextbox.Location = new System.Drawing.Point(63, 229);
            this.mirrorHSVtextbox.Multiline = true;
            this.mirrorHSVtextbox.Name = "mirrorHSVtextbox";
            this.mirrorHSVtextbox.Size = new System.Drawing.Size(380, 180);
            this.mirrorHSVtextbox.TabIndex = 20;
            this.mirrorHSVtextbox.Text = "\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n";
            this.mirrorHSVtextbox.WordWrap = false;
            // 
            // setDynamicRangeFromSlider
            // 
            this.setDynamicRangeFromSlider.Location = new System.Drawing.Point(457, 284);
            this.setDynamicRangeFromSlider.Name = "setDynamicRangeFromSlider";
            this.setDynamicRangeFromSlider.Size = new System.Drawing.Size(132, 37);
            this.setDynamicRangeFromSlider.TabIndex = 19;
            this.setDynamicRangeFromSlider.Text = "Set Dynamic Range for SELECTED ANGLE";
            this.setDynamicRangeFromSlider.UseVisualStyleBackColor = true;
            this.setDynamicRangeFromSlider.Click += new System.EventHandler(this.setDynamicRangeFromSlider_Click);
            // 
            // selectHSVline2
            // 
            this.selectHSVline2.ColumnWidth = 60;
            this.selectHSVline2.FormattingEnabled = true;
            this.selectHSVline2.Items.AddRange(new object[] {
            "345-14",
            "15-44",
            "45-74",
            "75-104",
            "105-134",
            "135-164",
            "165-194",
            "195-224",
            "225-254",
            "255-284",
            "285-314",
            "315-344"});
            this.selectHSVline2.Location = new System.Drawing.Point(457, 327);
            this.selectHSVline2.MultiColumn = true;
            this.selectHSVline2.Name = "selectHSVline2";
            this.selectHSVline2.Size = new System.Drawing.Size(132, 82);
            this.selectHSVline2.TabIndex = 18;
            // 
            // greenRadio
            // 
            this.greenRadio.AutoSize = true;
            this.greenRadio.Location = new System.Drawing.Point(252, 15);
            this.greenRadio.Name = "greenRadio";
            this.greenRadio.Size = new System.Drawing.Size(54, 17);
            this.greenRadio.TabIndex = 13;
            this.greenRadio.TabStop = true;
            this.greenRadio.Text = "Green";
            this.greenRadio.UseVisualStyleBackColor = true;
            this.greenRadio.Click += new System.EventHandler(this.greenRadio_CheckedChanged);
            this.greenRadio.CheckedChanged += new System.EventHandler(this.greenRadio_CheckedChanged);
            // 
            // orangeRadio
            // 
            this.orangeRadio.AutoSize = true;
            this.orangeRadio.Checked = true;
            this.orangeRadio.Location = new System.Drawing.Point(185, 15);
            this.orangeRadio.Name = "orangeRadio";
            this.orangeRadio.Size = new System.Drawing.Size(60, 17);
            this.orangeRadio.TabIndex = 12;
            this.orangeRadio.TabStop = true;
            this.orangeRadio.Text = "Orange";
            this.orangeRadio.UseVisualStyleBackColor = true;
            this.orangeRadio.Click += new System.EventHandler(this.orangeRadio_CheckedChanged);
            this.orangeRadio.CheckedChanged += new System.EventHandler(this.orangeRadio_CheckedChanged);
            // 
            // vRangeLabel
            // 
            this.vRangeLabel.AutoSize = true;
            this.vRangeLabel.Location = new System.Drawing.Point(237, 207);
            this.vRangeLabel.Name = "vRangeLabel";
            this.vRangeLabel.Size = new System.Drawing.Size(22, 13);
            this.vRangeLabel.TabIndex = 11;
            this.vRangeLabel.Text = "0-0";
            // 
            // sRangeLabel
            // 
            this.sRangeLabel.AutoSize = true;
            this.sRangeLabel.Location = new System.Drawing.Point(237, 147);
            this.sRangeLabel.Name = "sRangeLabel";
            this.sRangeLabel.Size = new System.Drawing.Size(22, 13);
            this.sRangeLabel.TabIndex = 10;
            this.sRangeLabel.Text = "0-0";
            // 
            // hRangeLabel
            // 
            this.hRangeLabel.AutoSize = true;
            this.hRangeLabel.Location = new System.Drawing.Point(237, 86);
            this.hRangeLabel.Name = "hRangeLabel";
            this.hRangeLabel.Size = new System.Drawing.Size(22, 13);
            this.hRangeLabel.TabIndex = 9;
            this.hRangeLabel.Text = "0-0";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(44, 167);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(14, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "V";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(44, 112);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(14, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "S";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(43, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(15, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "H";
            // 
            // vLowSlider
            // 
            this.vLowSlider.BackColor = System.Drawing.SystemColors.Window;
            this.vLowSlider.Location = new System.Drawing.Point(60, 183);
            this.vLowSlider.Maximum = 255;
            this.vLowSlider.Name = "vLowSlider";
            this.vLowSlider.Size = new System.Drawing.Size(384, 45);
            this.vLowSlider.TabIndex = 5;
            this.vLowSlider.TickFrequency = 20;
            this.vLowSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.vLowSlider.Scroll += new System.EventHandler(this.vLowSlider_Scroll);
            // 
            // vHighSlider
            // 
            this.vHighSlider.BackColor = System.Drawing.SystemColors.Window;
            this.vHighSlider.Location = new System.Drawing.Point(60, 159);
            this.vHighSlider.Maximum = 255;
            this.vHighSlider.Name = "vHighSlider";
            this.vHighSlider.Size = new System.Drawing.Size(384, 45);
            this.vHighSlider.TabIndex = 4;
            this.vHighSlider.TickFrequency = 20;
            this.vHighSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.vHighSlider.Scroll += new System.EventHandler(this.vHighSlider_Scroll);
            // 
            // sLowSlider
            // 
            this.sLowSlider.BackColor = System.Drawing.SystemColors.Window;
            this.sLowSlider.Location = new System.Drawing.Point(59, 122);
            this.sLowSlider.Maximum = 255;
            this.sLowSlider.Name = "sLowSlider";
            this.sLowSlider.Size = new System.Drawing.Size(384, 45);
            this.sLowSlider.TabIndex = 3;
            this.sLowSlider.TickFrequency = 20;
            this.sLowSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.sLowSlider.Scroll += new System.EventHandler(this.sLowSlider_Scroll);
            // 
            // sHighSlider
            // 
            this.sHighSlider.BackColor = System.Drawing.SystemColors.Window;
            this.sHighSlider.Location = new System.Drawing.Point(59, 99);
            this.sHighSlider.Maximum = 255;
            this.sHighSlider.Name = "sHighSlider";
            this.sHighSlider.Size = new System.Drawing.Size(384, 45);
            this.sHighSlider.TabIndex = 2;
            this.sHighSlider.TickFrequency = 20;
            this.sHighSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.sHighSlider.Scroll += new System.EventHandler(this.sHighSlider_Scroll);
            // 
            // hLowSlider
            // 
            this.hLowSlider.BackColor = System.Drawing.SystemColors.Window;
            this.hLowSlider.Location = new System.Drawing.Point(59, 60);
            this.hLowSlider.Maximum = 179;
            this.hLowSlider.Name = "hLowSlider";
            this.hLowSlider.Size = new System.Drawing.Size(384, 45);
            this.hLowSlider.TabIndex = 1;
            this.hLowSlider.TickFrequency = 20;
            this.hLowSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.hLowSlider.Scroll += new System.EventHandler(this.hLowSlider_Scroll);
            // 
            // hHighSlider
            // 
            this.hHighSlider.BackColor = System.Drawing.SystemColors.Window;
            this.hHighSlider.Location = new System.Drawing.Point(58, 38);
            this.hHighSlider.Maximum = 179;
            this.hHighSlider.Name = "hHighSlider";
            this.hHighSlider.Size = new System.Drawing.Size(384, 45);
            this.hHighSlider.TabIndex = 0;
            this.hHighSlider.TickFrequency = 20;
            this.hHighSlider.TickStyle = System.Windows.Forms.TickStyle.None;
            this.hHighSlider.Scroll += new System.EventHandler(this.hHighSlider_Scroll);
            // 
            // DynamicHSVPage
            // 
            this.DynamicHSVPage.Controls.Add(this.getHSVline);
            this.DynamicHSVPage.Controls.Add(this.HSVDataGrid);
            this.DynamicHSVPage.Controls.Add(this.setHSVline);
            this.DynamicHSVPage.Controls.Add(this.selectHSVline1);
            this.DynamicHSVPage.Controls.Add(this.label34);
            this.DynamicHSVPage.Controls.Add(this.label33);
            this.DynamicHSVPage.Controls.Add(this.label32);
            this.DynamicHSVPage.Controls.Add(this.label31);
            this.DynamicHSVPage.Controls.Add(this.label30);
            this.DynamicHSVPage.Controls.Add(this.label29);
            this.DynamicHSVPage.Controls.Add(this.label28);
            this.DynamicHSVPage.Controls.Add(this.label27);
            this.DynamicHSVPage.Controls.Add(this.label26);
            this.DynamicHSVPage.Controls.Add(this.label25);
            this.DynamicHSVPage.Controls.Add(this.label24);
            this.DynamicHSVPage.Controls.Add(this.label23);
            this.DynamicHSVPage.Controls.Add(this.loadHSVButton);
            this.DynamicHSVPage.Controls.Add(this.saveHSVbutton);
            this.DynamicHSVPage.Controls.Add(this.dynamicHSVTextBox);
            this.DynamicHSVPage.Controls.Add(this.DynamicHSV_xmit_all);
            this.DynamicHSVPage.Controls.Add(this.DynamicHSV_rx_all);
            this.DynamicHSVPage.Location = new System.Drawing.Point(4, 22);
            this.DynamicHSVPage.Name = "DynamicHSVPage";
            this.DynamicHSVPage.Padding = new System.Windows.Forms.Padding(3);
            this.DynamicHSVPage.Size = new System.Drawing.Size(627, 423);
            this.DynamicHSVPage.TabIndex = 4;
            this.DynamicHSVPage.Text = "Dynamic HSV";
            this.DynamicHSVPage.UseVisualStyleBackColor = true;
            // 
            // getHSVline
            // 
            this.getHSVline.Location = new System.Drawing.Point(319, 265);
            this.getHSVline.Name = "getHSVline";
            this.getHSVline.Size = new System.Drawing.Size(117, 26);
            this.getHSVline.TabIndex = 20;
            this.getHSVline.Text = "Get Selected Range";
            this.getHSVline.UseVisualStyleBackColor = true;
            this.getHSVline.Click += new System.EventHandler(this.getHSVline_Click);
            // 
            // HSVDataGrid
            // 
            this.HSVDataGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.GHlo,
            this.GHhi,
            this.GSlo,
            this.GShi,
            this.GVlo,
            this.GVhi,
            this.OHlo,
            this.OHhi,
            this.OSlo,
            this.OShi,
            this.OVlo,
            this.OVhi});
            this.HSVDataGrid.Location = new System.Drawing.Point(3, 324);
            this.HSVDataGrid.Name = "HSVDataGrid";
            this.HSVDataGrid.RowHeadersWidth = 10;
            this.HSVDataGrid.Size = new System.Drawing.Size(615, 73);
            this.HSVDataGrid.TabIndex = 19;
            // 
            // GHlo
            // 
            this.GHlo.HeaderText = "Gr H lo";
            this.GHlo.Name = "GHlo";
            this.GHlo.ToolTipText = "Green Hue Low Value";
            this.GHlo.Width = 50;
            // 
            // GHhi
            // 
            this.GHhi.HeaderText = "Gr H hi";
            this.GHhi.Name = "GHhi";
            this.GHhi.ToolTipText = "Green Hue High Value";
            this.GHhi.Width = 50;
            // 
            // GSlo
            // 
            this.GSlo.HeaderText = "Gr S lo";
            this.GSlo.Name = "GSlo";
            this.GSlo.ToolTipText = "Green Saturation Low Value";
            this.GSlo.Width = 50;
            // 
            // GShi
            // 
            this.GShi.HeaderText = "Gr S hi";
            this.GShi.Name = "GShi";
            this.GShi.ToolTipText = "Green Saturation High Value";
            this.GShi.Width = 50;
            // 
            // GVlo
            // 
            this.GVlo.HeaderText = "Gr V lo";
            this.GVlo.Name = "GVlo";
            this.GVlo.ToolTipText = "Green Value Low";
            this.GVlo.Width = 50;
            // 
            // GVhi
            // 
            this.GVhi.HeaderText = "Gr V hi";
            this.GVhi.Name = "GVhi";
            this.GVhi.ToolTipText = "Green Value High";
            this.GVhi.Width = 50;
            // 
            // OHlo
            // 
            this.OHlo.HeaderText = "Or H lo";
            this.OHlo.Name = "OHlo";
            this.OHlo.ToolTipText = "Orange Hue Low";
            this.OHlo.Width = 50;
            // 
            // OHhi
            // 
            this.OHhi.HeaderText = "Or H hi";
            this.OHhi.Name = "OHhi";
            this.OHhi.ToolTipText = "Orange Hue High";
            this.OHhi.Width = 50;
            // 
            // OSlo
            // 
            this.OSlo.HeaderText = "Or S lo";
            this.OSlo.Name = "OSlo";
            this.OSlo.ToolTipText = "Orange Saturation Low";
            this.OSlo.Width = 50;
            // 
            // OShi
            // 
            this.OShi.HeaderText = "Or S hi";
            this.OShi.Name = "OShi";
            this.OShi.ToolTipText = "Orange Saturation High";
            this.OShi.Width = 50;
            // 
            // OVlo
            // 
            this.OVlo.HeaderText = "Or V lo";
            this.OVlo.Name = "OVlo";
            this.OVlo.ToolTipText = "Orange Value Low";
            this.OVlo.Width = 50;
            // 
            // OVhi
            // 
            this.OVhi.HeaderText = "Or V hi";
            this.OVhi.Name = "OVhi";
            this.OVhi.ToolTipText = "Orange Value High";
            this.OVhi.Width = 50;
            // 
            // setHSVline
            // 
            this.setHSVline.Location = new System.Drawing.Point(319, 225);
            this.setHSVline.Name = "setHSVline";
            this.setHSVline.Size = new System.Drawing.Size(117, 25);
            this.setHSVline.TabIndex = 18;
            this.setHSVline.Text = "Set Selected Range";
            this.setHSVline.UseVisualStyleBackColor = true;
            this.setHSVline.Click += new System.EventHandler(this.setHSVline_Click);
            // 
            // selectHSVline1
            // 
            this.selectHSVline1.ColumnWidth = 60;
            this.selectHSVline1.FormattingEnabled = true;
            this.selectHSVline1.Items.AddRange(new object[] {
            "345-14",
            "15-44",
            "45-74",
            "75-104",
            "105-134",
            "135-164",
            "165-194",
            "195-224",
            "225-254",
            "255-284",
            "285-314",
            "315-344"});
            this.selectHSVline1.Location = new System.Drawing.Point(443, 225);
            this.selectHSVline1.MultiColumn = true;
            this.selectHSVline1.Name = "selectHSVline1";
            this.selectHSVline1.Size = new System.Drawing.Size(132, 82);
            this.selectHSVline1.TabIndex = 17;
            // 
            // label34
            // 
            this.label34.AutoSize = true;
            this.label34.Location = new System.Drawing.Point(6, 176);
            this.label34.Name = "label34";
            this.label34.Size = new System.Drawing.Size(46, 13);
            this.label34.TabIndex = 16;
            this.label34.Text = "315-344";
            this.label34.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label33
            // 
            this.label33.AutoSize = true;
            this.label33.Location = new System.Drawing.Point(6, 163);
            this.label33.Name = "label33";
            this.label33.Size = new System.Drawing.Size(46, 13);
            this.label33.TabIndex = 15;
            this.label33.Text = "285-314";
            this.label33.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label32
            // 
            this.label32.AutoSize = true;
            this.label32.Location = new System.Drawing.Point(6, 150);
            this.label32.Name = "label32";
            this.label32.Size = new System.Drawing.Size(46, 13);
            this.label32.TabIndex = 14;
            this.label32.Text = "255-284";
            this.label32.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label31
            // 
            this.label31.AutoSize = true;
            this.label31.Location = new System.Drawing.Point(6, 137);
            this.label31.Name = "label31";
            this.label31.Size = new System.Drawing.Size(46, 13);
            this.label31.TabIndex = 13;
            this.label31.Text = "225-254";
            this.label31.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(6, 124);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(46, 13);
            this.label30.TabIndex = 12;
            this.label30.Text = "195-224";
            this.label30.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(6, 111);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(46, 13);
            this.label29.TabIndex = 11;
            this.label29.Text = "165-194";
            this.label29.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(6, 98);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(46, 13);
            this.label28.TabIndex = 10;
            this.label28.Text = "135-164";
            this.label28.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(6, 85);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(46, 13);
            this.label27.TabIndex = 9;
            this.label27.Text = "105-134";
            this.label27.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(12, 72);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(40, 13);
            this.label26.TabIndex = 8;
            this.label26.Text = "75-104";
            this.label26.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(18, 59);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(34, 13);
            this.label25.TabIndex = 7;
            this.label25.Text = "45-74";
            this.label25.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // label24
            // 
            this.label24.AutoSize = true;
            this.label24.Location = new System.Drawing.Point(18, 46);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(34, 13);
            this.label24.TabIndex = 6;
            this.label24.Text = "15-44";
            this.label24.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label23
            // 
            this.label23.AutoSize = true;
            this.label23.Location = new System.Drawing.Point(12, 33);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(40, 13);
            this.label23.TabIndex = 5;
            this.label23.Text = "345-14";
            this.label23.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // loadHSVButton
            // 
            this.loadHSVButton.Location = new System.Drawing.Point(456, 66);
            this.loadHSVButton.Name = "loadHSVButton";
            this.loadHSVButton.Size = new System.Drawing.Size(117, 32);
            this.loadHSVButton.TabIndex = 4;
            this.loadHSVButton.Text = "Load HSV Settings";
            this.loadHSVButton.UseVisualStyleBackColor = true;
            this.loadHSVButton.Click += new System.EventHandler(this.loadHSVButton_Click);
            // 
            // saveHSVbutton
            // 
            this.saveHSVbutton.Location = new System.Drawing.Point(456, 30);
            this.saveHSVbutton.Name = "saveHSVbutton";
            this.saveHSVbutton.Size = new System.Drawing.Size(119, 32);
            this.saveHSVbutton.TabIndex = 3;
            this.saveHSVbutton.Text = "Save HSV Settings";
            this.saveHSVbutton.UseVisualStyleBackColor = true;
            this.saveHSVbutton.Click += new System.EventHandler(this.saveHSVbutton_Click);
            // 
            // dynamicHSVTextBox
            // 
            this.dynamicHSVTextBox.Location = new System.Drawing.Point(58, 30);
            this.dynamicHSVTextBox.Multiline = true;
            this.dynamicHSVTextBox.Name = "dynamicHSVTextBox";
            this.dynamicHSVTextBox.Size = new System.Drawing.Size(380, 180);
            this.dynamicHSVTextBox.TabIndex = 2;
            this.dynamicHSVTextBox.Text = "\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n\r\n";
            this.dynamicHSVTextBox.WordWrap = false;
            this.dynamicHSVTextBox.TextChanged += new System.EventHandler(this.dynamicHSVTextBox_TextChanged);
            // 
            // DynamicHSV_xmit_all
            // 
            this.DynamicHSV_xmit_all.Location = new System.Drawing.Point(456, 137);
            this.DynamicHSV_xmit_all.Name = "DynamicHSV_xmit_all";
            this.DynamicHSV_xmit_all.Size = new System.Drawing.Size(117, 32);
            this.DynamicHSV_xmit_all.TabIndex = 1;
            this.DynamicHSV_xmit_all.Text = "Transmit ALL";
            this.DynamicHSV_xmit_all.UseVisualStyleBackColor = true;
            this.DynamicHSV_xmit_all.Click += new System.EventHandler(this.DynamicHSV_xmit_all_Click);
            // 
            // DynamicHSV_rx_all
            // 
            this.DynamicHSV_rx_all.Location = new System.Drawing.Point(456, 177);
            this.DynamicHSV_rx_all.Name = "DynamicHSV_rx_all";
            this.DynamicHSV_rx_all.Size = new System.Drawing.Size(117, 33);
            this.DynamicHSV_rx_all.TabIndex = 0;
            this.DynamicHSV_rx_all.Text = "Receive All (console)";
            this.DynamicHSV_rx_all.UseVisualStyleBackColor = true;
            this.DynamicHSV_rx_all.Click += new System.EventHandler(this.DynamicHSV_rx_all_Click);
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
            // RobotRacer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(1066, 801);
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
            this.Controls.Add(this.truckSentAngle);
            this.Controls.Add(this.truckVelocityTextbox);
            this.Controls.Add(this.truckState);
            this.Controls.Add(this.sentAngleLabel);
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
            this.courseGraphicTab.ResumeLayout(false);
            this.uartControlTab.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.speedSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.steeringSlider)).EndInit();
            this.imageTabControl.ResumeLayout(false);
            this.rawImageTab.ResumeLayout(false);
            this.tab3DRacer.ResumeLayout(false);
            this.registerTabControl.ResumeLayout(false);
            this.navigationTab.ResumeLayout(false);
            this.navigationTab.PerformLayout();
            this.pidTab.ResumeLayout(false);
            this.pidTab.PerformLayout();
            this.hsvTab.ResumeLayout(false);
            this.hsvTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.convThreshold_trackbar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.vLowSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.vHighSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sLowSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sHighSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hLowSlider)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.hHighSlider)).EndInit();
            this.DynamicHSVPage.ResumeLayout(false);
            this.DynamicHSVPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.HSVDataGrid)).EndInit();
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
        private System.Windows.Forms.TabPage courseGraphicTab;
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
        private System.Windows.Forms.Label sentAngleLabel;
        private System.Windows.Forms.Label truckState;
        private System.Windows.Forms.Label truckVelocityTextbox;
        private System.Windows.Forms.Label truckSentAngle;
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
        

        private CourseGraphicsPanel courseGraphicalPanel;
        private System.Windows.Forms.TabPage serialTab;
        private System.Windows.Forms.TextBox serialTextBox;
        private System.Windows.Forms.CheckBox rxCheckbox;
        private System.Windows.Forms.CheckBox txCheckbox;
        private Button clearBtn;
        private TabControl registerTabControl;
        private TabPage registerTab;
        private TabPage pidTab;
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
        private Label kiLabel;
        private Label kdLabel;
        private Label kpLabel;
        private Button sendPidButton;
        private TextBox kiTextbox;
        private TextBox kdTextbox;
        private TextBox kpTextbox;
        private Label iMinLabel;
        private Label iMaxLabel;
        private TextBox iMinTextbox;
        private TextBox iMaxTextbox;
        private Label desiredVelocityLabel;
        private TextBox desiredVelocityTextbox;
        private Button resetPIDValuesButton;
        private TextBox numMillisecondsToRunTextbox;
        private Label numMillisecondsLabel;
        private Label turningLabel;
        private TextBox turningTextbox;
        private Label distanceToRunLabel;
        private TextBox metersToRunTextbox;
        private Label orLabel;
        private Button runButton;
        public Button btn_packetLoss;
        private Button btn_transmitBinary;
        private TextBox txt_binaryFileName;
        private OpenFileDialog loadBinaryDialog;
        private Button btn_bootload;
        private Label label1;
        private ProcessedImage processedImage;
        private ComboBox ddbImageType;
        private TabPage hsvTab;
        private TrackBar hLowSlider;
        private TrackBar hHighSlider;
        private TrackBar vLowSlider;
        private TrackBar vHighSlider;
        private TrackBar sLowSlider;
        private TrackBar sHighSlider;
        private Label label4;
        private Label label3;
        private Label label2;
        private Label vRangeLabel;
        private Label sRangeLabel;
        private Label hRangeLabel;
        private RadioButton greenRadio;
        private RadioButton orangeRadio;
        private TabPage navigationTab;
        private Label label8;
        private Label label7;
        private Label label6;
        private Label label5;
        private Label label10;
        private Label label9;
        private Label distanceToBlindLabel;
        private Label distanceTraveledLabel;
        private Label angleOfPylonLabel;
        private Label distanceToPylonLabel;
        private Label carStateLabel;
        private Label pylonNumLabel;
        private Button button1;
        private Label label11;
        private Label lblFPS_HW;
        public TabPage tab3DRacer;
        public XNAGame.xnaRoboRacerDisplay xnaRoboRacerDisplay1;
        private Label label12;
        private TextBox turnRadiusTextbox;
        public TextBox straightSpeedTextbox;
        public TextBox blindTurnTextbox;
        public TextBox blindStraightTextbox;
        private Label label;
        private Button button2;
        private Label label18;
        private Label label17;
        private Label label16;
        private Label label15;
        private Label label14;
        private Label label13;
        public TextBox courseTextBox;
        private Label label19;
        public TextBox turnAngleTextbox;
        private Label label20;
        private Label label22;
        private Label label21;
        private TextBox findRangeTextbox;
        private Button buttonGetCriticalImage;
        public Label lblBytesPerSecond_Total;
        public Label lblBytesPerSecond2;
        private TabPage DynamicHSVPage;
        private Button DynamicHSV_rx_all;
        private Button DynamicHSV_xmit_all;
        private TextBox dynamicHSVTextBox;
        private Button saveHSVbutton;
        private Button loadHSVButton;
        private Label label25;
        private Label label24;
        private Label label23;
        private Label label34;
        private Label label33;
        private Label label32;
        private Label label31;
        private Label label30;
        private Label label29;
        private Label label28;
        private Label label27;
        private Label label26;
        private ListBox selectHSVline1;
        private Button setHSVline;
        private DataGridView HSVDataGrid;
        private DataGridViewTextBoxColumn GHlo;
        private DataGridViewTextBoxColumn GHhi;
        private DataGridViewTextBoxColumn GSlo;
        private DataGridViewTextBoxColumn GShi;
        private DataGridViewTextBoxColumn GVlo;
        private DataGridViewTextBoxColumn GVhi;
        private DataGridViewTextBoxColumn OHlo;
        private DataGridViewTextBoxColumn OHhi;
        private DataGridViewTextBoxColumn OSlo;
        private DataGridViewTextBoxColumn OShi;
        private DataGridViewTextBoxColumn OVlo;
        private DataGridViewTextBoxColumn OVhi;
        private Button getHSVline;
        private ListBox selectHSVline2;
        private Button setDynamicRangeFromSlider;
        private TextBox mirrorHSVtextbox;
        private Button setAllDynamicRangeFromSlider;
        private Label label47;
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
        private Label label35;
        private Label label36;
        private Label label37;
        private Label label38;
        private Label label39;
        private Label label40;
        private Label label41;
        private Label label42;
        private Label label43;
        private Label label44;
        private Label label45;
        private Label label46;
        private Label lblFPS_SW;
        private Label label60;
        private TrackBar convThreshold_trackbar;
        private Label convThreshold_text;
        

        
    }

    
}

