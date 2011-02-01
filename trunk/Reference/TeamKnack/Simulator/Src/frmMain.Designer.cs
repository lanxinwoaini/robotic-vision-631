namespace RoboSim
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.picPathPlan = new System.Windows.Forms.PictureBox();
            this.ilstIcons = new System.Windows.Forms.ImageList(this.components);
            this.statStripMain = new System.Windows.Forms.StatusStrip();
            this.lblStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.tmrRace = new System.Windows.Forms.Timer(this.components);
            this.lblDebug = new System.Windows.Forms.Label();
            this.txtDebug = new System.Windows.Forms.TextBox();
            this.btnPlaceCar = new System.Windows.Forms.CheckBox();
            this.btnClearPylons = new System.Windows.Forms.Button();
            this.btnReset = new System.Windows.Forms.Button();
            this.txtTheta = new System.Windows.Forms.TextBox();
            this.txtDistance = new System.Windows.Forms.TextBox();
            this.txtState = new System.Windows.Forms.TextBox();
            this.lblTheta = new System.Windows.Forms.Label();
            this.lblDistance = new System.Windows.Forms.Label();
            this.lblState = new System.Windows.Forms.Label();
            this.lblCurrentPylon = new System.Windows.Forms.Label();
            this.txtCurrentPylon = new System.Windows.Forms.TextBox();
            this.btnRun = new System.Windows.Forms.Button();
            this.lblCamara = new System.Windows.Forms.Label();
            this.picCamera = new System.Windows.Forms.PictureBox();
            this.lblCourseDesctiprion = new System.Windows.Forms.Label();
            this.txtCourseDescription = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.picPathPlan)).BeginInit();
            this.statStripMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCamera)).BeginInit();
            this.SuspendLayout();
            // 
            // picPathPlan
            // 
            this.picPathPlan.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.picPathPlan.Location = new System.Drawing.Point(12, 11);
            this.picPathPlan.Name = "picPathPlan";
            this.picPathPlan.Size = new System.Drawing.Size(375, 467);
            this.picPathPlan.TabIndex = 0;
            this.picPathPlan.TabStop = false;
            this.picPathPlan.MouseClick += new System.Windows.Forms.MouseEventHandler(this.picPathPlan_MouseClick);
            // 
            // ilstIcons
            // 
            this.ilstIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("ilstIcons.ImageStream")));
            this.ilstIcons.TransparentColor = System.Drawing.Color.White;
            this.ilstIcons.Images.SetKeyName(0, "Car.bmp");
            // 
            // statStripMain
            // 
            this.statStripMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblStatus});
            this.statStripMain.Location = new System.Drawing.Point(0, 482);
            this.statStripMain.Name = "statStripMain";
            this.statStripMain.Size = new System.Drawing.Size(739, 22);
            this.statStripMain.TabIndex = 10;
            this.statStripMain.Text = "statStripMain";
            // 
            // lblStatus
            // 
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(0, 17);
            // 
            // tmrRace
            // 
            this.tmrRace.Interval = 30;
            this.tmrRace.Tick += new System.EventHandler(this.tmrRace_Tick);
            // 
            // lblDebug
            // 
            this.lblDebug.AutoSize = true;
            this.lblDebug.Location = new System.Drawing.Point(394, 349);
            this.lblDebug.Name = "lblDebug";
            this.lblDebug.Size = new System.Drawing.Size(74, 13);
            this.lblDebug.TabIndex = 19;
            this.lblDebug.Text = "Debug Output";
            // 
            // txtDebug
            // 
            this.txtDebug.Location = new System.Drawing.Point(396, 365);
            this.txtDebug.Multiline = true;
            this.txtDebug.Name = "txtDebug";
            this.txtDebug.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtDebug.Size = new System.Drawing.Size(331, 108);
            this.txtDebug.TabIndex = 18;
            this.txtDebug.TextChanged += new System.EventHandler(this.txtDebug_TextChanged);
            // 
            // btnPlaceCar
            // 
            this.btnPlaceCar.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnPlaceCar.AutoSize = true;
            this.btnPlaceCar.Location = new System.Drawing.Point(397, 21);
            this.btnPlaceCar.MinimumSize = new System.Drawing.Size(125, 25);
            this.btnPlaceCar.Name = "btnPlaceCar";
            this.btnPlaceCar.Size = new System.Drawing.Size(125, 25);
            this.btnPlaceCar.TabIndex = 36;
            this.btnPlaceCar.Text = "&Place Car";
            this.btnPlaceCar.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnPlaceCar.UseVisualStyleBackColor = true;
            // 
            // btnClearPylons
            // 
            this.btnClearPylons.Location = new System.Drawing.Point(397, 114);
            this.btnClearPylons.Name = "btnClearPylons";
            this.btnClearPylons.Size = new System.Drawing.Size(125, 25);
            this.btnClearPylons.TabIndex = 35;
            this.btnClearPylons.Text = "&Clear Map";
            this.btnClearPylons.UseVisualStyleBackColor = true;
            this.btnClearPylons.Click += new System.EventHandler(this.btnClearPylons_Click);
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(397, 83);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(125, 25);
            this.btnReset.TabIndex = 34;
            this.btnReset.Text = "Reset the Car";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // txtTheta
            // 
            this.txtTheta.Location = new System.Drawing.Point(396, 304);
            this.txtTheta.Name = "txtTheta";
            this.txtTheta.ReadOnly = true;
            this.txtTheta.Size = new System.Drawing.Size(125, 20);
            this.txtTheta.TabIndex = 33;
            // 
            // txtDistance
            // 
            this.txtDistance.Location = new System.Drawing.Point(396, 265);
            this.txtDistance.Name = "txtDistance";
            this.txtDistance.ReadOnly = true;
            this.txtDistance.Size = new System.Drawing.Size(125, 20);
            this.txtDistance.TabIndex = 32;
            // 
            // txtState
            // 
            this.txtState.Location = new System.Drawing.Point(396, 226);
            this.txtState.Name = "txtState";
            this.txtState.ReadOnly = true;
            this.txtState.Size = new System.Drawing.Size(125, 20);
            this.txtState.TabIndex = 31;
            // 
            // lblTheta
            // 
            this.lblTheta.AutoSize = true;
            this.lblTheta.Location = new System.Drawing.Point(393, 288);
            this.lblTheta.Name = "lblTheta";
            this.lblTheta.Size = new System.Drawing.Size(88, 13);
            this.lblTheta.TabIndex = 30;
            this.lblTheta.Text = "Remaining Theta";
            // 
            // lblDistance
            // 
            this.lblDistance.AutoSize = true;
            this.lblDistance.Location = new System.Drawing.Point(393, 249);
            this.lblDistance.Name = "lblDistance";
            this.lblDistance.Size = new System.Drawing.Size(102, 13);
            this.lblDistance.TabIndex = 29;
            this.lblDistance.Text = "Remaining Distance";
            // 
            // lblState
            // 
            this.lblState.AutoSize = true;
            this.lblState.Location = new System.Drawing.Point(394, 209);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(88, 13);
            this.lblState.TabIndex = 28;
            this.lblState.Text = "Current Car State";
            // 
            // lblCurrentPylon
            // 
            this.lblCurrentPylon.AutoSize = true;
            this.lblCurrentPylon.Location = new System.Drawing.Point(394, 170);
            this.lblCurrentPylon.Name = "lblCurrentPylon";
            this.lblCurrentPylon.Size = new System.Drawing.Size(70, 13);
            this.lblCurrentPylon.TabIndex = 27;
            this.lblCurrentPylon.Text = "Current Pylon";
            this.lblCurrentPylon.Click += new System.EventHandler(this.lblCurrentPylon_Click);
            // 
            // txtCurrentPylon
            // 
            this.txtCurrentPylon.Location = new System.Drawing.Point(396, 186);
            this.txtCurrentPylon.Name = "txtCurrentPylon";
            this.txtCurrentPylon.ReadOnly = true;
            this.txtCurrentPylon.Size = new System.Drawing.Size(125, 20);
            this.txtCurrentPylon.TabIndex = 26;
            this.txtCurrentPylon.TextChanged += new System.EventHandler(this.txtCurrentPylon_TextChanged);
            // 
            // btnRun
            // 
            this.btnRun.Location = new System.Drawing.Point(397, 52);
            this.btnRun.Name = "btnRun";
            this.btnRun.Size = new System.Drawing.Size(125, 25);
            this.btnRun.TabIndex = 25;
            this.btnRun.Text = "&Run the car";
            this.btnRun.UseVisualStyleBackColor = true;
            this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
            // 
            // lblCamara
            // 
            this.lblCamara.AutoSize = true;
            this.lblCamara.Location = new System.Drawing.Point(528, 170);
            this.lblCamara.Name = "lblCamara";
            this.lblCamara.Size = new System.Drawing.Size(69, 13);
            this.lblCamara.TabIndex = 24;
            this.lblCamara.Text = "Camera View";
            // 
            // picCamera
            // 
            this.picCamera.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.picCamera.Location = new System.Drawing.Point(531, 186);
            this.picCamera.Name = "picCamera";
            this.picCamera.Size = new System.Drawing.Size(196, 160);
            this.picCamera.TabIndex = 23;
            this.picCamera.TabStop = false;
            // 
            // lblCourseDesctiprion
            // 
            this.lblCourseDesctiprion.AutoSize = true;
            this.lblCourseDesctiprion.Location = new System.Drawing.Point(531, 9);
            this.lblCourseDesctiprion.Name = "lblCourseDesctiprion";
            this.lblCourseDesctiprion.Size = new System.Drawing.Size(96, 13);
            this.lblCourseDesctiprion.TabIndex = 22;
            this.lblCourseDesctiprion.Text = "Course Description";
            // 
            // txtCourseDescription
            // 
            this.txtCourseDescription.Location = new System.Drawing.Point(531, 25);
            this.txtCourseDescription.Multiline = true;
            this.txtCourseDescription.Name = "txtCourseDescription";
            this.txtCourseDescription.Size = new System.Drawing.Size(196, 136);
            this.txtCourseDescription.TabIndex = 21;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(739, 504);
            this.Controls.Add(this.btnPlaceCar);
            this.Controls.Add(this.btnClearPylons);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.txtTheta);
            this.Controls.Add(this.txtDistance);
            this.Controls.Add(this.txtState);
            this.Controls.Add(this.lblTheta);
            this.Controls.Add(this.lblDistance);
            this.Controls.Add(this.lblState);
            this.Controls.Add(this.lblCurrentPylon);
            this.Controls.Add(this.txtCurrentPylon);
            this.Controls.Add(this.btnRun);
            this.Controls.Add(this.lblCamara);
            this.Controls.Add(this.picCamera);
            this.Controls.Add(this.lblCourseDesctiprion);
            this.Controls.Add(this.txtCourseDescription);
            this.Controls.Add(this.lblDebug);
            this.Controls.Add(this.txtDebug);
            this.Controls.Add(this.statStripMain);
            this.Controls.Add(this.picPathPlan);
            this.Name = "frmMain";
            this.Text = "Robot Racer Control Simulator";
            this.Load += new System.EventHandler(this.frmMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picPathPlan)).EndInit();
            this.statStripMain.ResumeLayout(false);
            this.statStripMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picCamera)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox picPathPlan;
        private System.Windows.Forms.ImageList ilstIcons;
        private System.Windows.Forms.StatusStrip statStripMain;
        private System.Windows.Forms.ToolStripStatusLabel lblStatus;
        private System.Windows.Forms.Timer tmrRace;
        private System.Windows.Forms.Label lblDebug;
        private System.Windows.Forms.TextBox txtDebug;
        private System.Windows.Forms.CheckBox btnPlaceCar;
        private System.Windows.Forms.Button btnClearPylons;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.TextBox txtTheta;
        private System.Windows.Forms.TextBox txtDistance;
        private System.Windows.Forms.TextBox txtState;
        private System.Windows.Forms.Label lblTheta;
        private System.Windows.Forms.Label lblDistance;
        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.Label lblCurrentPylon;
        private System.Windows.Forms.TextBox txtCurrentPylon;
        private System.Windows.Forms.Button btnRun;
        private System.Windows.Forms.Label lblCamara;
        private System.Windows.Forms.PictureBox picCamera;
        private System.Windows.Forms.Label lblCourseDesctiprion;
        private System.Windows.Forms.TextBox txtCourseDescription;
    }
}

