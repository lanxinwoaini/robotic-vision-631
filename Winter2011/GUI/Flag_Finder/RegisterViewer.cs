using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace Robot_Racers
{


    public class RegisterViewer : UserControl
    {
        private BindingSource regBindingSource;
        private IContainer components;
        RegisterCollection coll;
        private DataGridView registerGridView;

        public const string REGISTERS_FILENAME = "registers.regs";
        //private BindingSource fullRegisterListBindingSource;
        private DataGridViewComboBoxColumn RegisterEntry;
        private BindingSource registerCollectionBindingSource;
        private DataGridViewTextBoxColumn RegValueHex;
        private DataGridViewTextBoxColumn RegValueString;

        public static FullRegisterList regList = new FullRegisterList(null);

        public RegisterViewer()
        {
            InitializeComponent();
            initSource();
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.registerGridView = new System.Windows.Forms.DataGridView();
            this.RegisterEntry = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.RegValueHex = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.RegValueString = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.regBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.registerCollectionBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.registerGridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.regBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.registerCollectionBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // registerGridView
            // 
            this.registerGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.registerGridView.AutoGenerateColumns = false;
            this.registerGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.registerGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.RegisterEntry,
            this.RegValueHex,
            this.RegValueString});
            this.registerGridView.DataSource = this.regBindingSource;
            this.registerGridView.Location = new System.Drawing.Point(3, 3);
            this.registerGridView.Name = "registerGridView";
            this.registerGridView.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.registerGridView.Size = new System.Drawing.Size(1001, 488);
            this.registerGridView.TabIndex = 1;
            // 
            // RegisterEntry
            // 
            this.RegisterEntry.Name = "RegisterEntry";
            // 
            // RegValueHex
            // 
            this.RegValueHex.DataPropertyName = "RegValueHex";
            this.RegValueHex.HeaderText = "Value (Hex)";
            this.RegValueHex.Name = "RegValueHex";
            this.RegValueHex.ReadOnly = true;
            // 
            // RegValueString
            // 
            this.RegValueString.DataPropertyName = "RegValueString";
            this.RegValueString.HeaderText = "Decimal Value";
            this.RegValueString.Name = "RegValueString";
            // 
            // regBindingSource
            // 
            this.regBindingSource.DataSource = typeof(Robot_Racers.RegisterCollection);
            // 
            // RegisterViewer
            // 
            this.Controls.Add(this.registerGridView);
            this.Name = "RegisterViewer";
            this.Size = new System.Drawing.Size(1001, 494);
            ((System.ComponentModel.ISupportInitialize)(this.registerGridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.regBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.registerCollectionBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        private void initSource(){
            coll = new RegisterCollection(true);

            this.regBindingSource.DataSource = coll;
            this.regBindingSource.ListChanged += new ListChangedEventHandler(regBindingSource_ListChanged);
            this.registerGridView.DataError += new DataGridViewDataErrorEventHandler(registerGridView_DataError);

            RegisterViewer.regList = new FullRegisterList(coll);

            this.RegisterEntry.DataPropertyName = "RegTag";
            this.RegisterEntry.DataSource = RegisterViewer.regList;
            this.RegisterEntry.Width = 150;

            requestRegisterValues();
        }

        private void regBindingSource_ListChanged(object sender, EventArgs e)
        {   
            TextWriter tw = new StreamWriter(REGISTERS_FILENAME);
            foreach (RegisterInfo info in coll)
            {
                tw.Write(info.ToString() + "\n");
            }
            tw.Close();
        }

        private void registerGridView_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            //MessageBox.Show("Invalid entry in data table.  Fix your freaking mistake.");
        }

        public void updateRegisterValue(int regId, int intValue, float floatValue)
        {
            coll.updateRegisterValue(regId, intValue, floatValue);
        }

        public void requestRegisterValues()
        {
            foreach (RegisterInfo r in coll)
            {
                try
                {
                    SerialHeader header = new SerialHeader();
                    header.Type = (byte)SerialUtils.TransmissionType.REGISTER;
                    if (r.IsTypeInt)
                        header.Subtype = (byte)SerialUtils.RegisterSubtype.GET_INT;
                    else
                        header.Subtype = (byte)SerialUtils.RegisterSubtype.GET_FLOAT;

                    RobotRacer.serialPort.transmitRegData(header, (ushort)r.RegId, 0);
                }
                catch (Exception ex) { Console.WriteLine(ex.Message + " in refresh all registers transmit."); }
            }
        }

        public void regViewer_BindingComplete(object sender, BindingCompleteEventArgs e)
        {
            if (e.BindingCompleteState != BindingCompleteState.Success)
                MessageBox.Show("partNumberBinding: " + e.ErrorText);
        }    
    }

    public class RegisterInfo           // Holds simple register information
    {
        private int regId = 0;
        private string regTag = "";

        private int regValueInt = 0;
        private float regValueFloat = 0f;

        private string regValueHex = "00 00 00 00";
        private string regValueString = "0";
        private bool isTypeInt = true;  // false = float
        

        public RegisterInfo(){}

        public bool populate(string registerString)     // Populate regId and regTag with CSV values passed in
        {
            string[] regData = registerString.Split(new char[]{','});   
            if (regData.Length == 2)
            {

                regId = int.Parse(regData[0]);
                RegTag = regData[1];
                if (regId > Register.INT_FLOAT_ID_DIVIDER)
                {
                    IsTypeInt = false;
                    regValueString = "0.0";
                }
                else
                {
                    IsTypeInt = true;
                    regValueString = "0";
                }
                return true;
            }
            return false;
        }

        public int RegId{                       // Get and Set methods
            get { return regId; }
            set { regId = value; }
        }

        public int RegValueInt
        {
            get { return regValueInt; }
            set { regValueInt = value; }
        }

        public float RegValueFloat
        {
            get { return regValueFloat; }
            set { regValueFloat = value; }
        }

        public string RegValueHex
        {
            get { return regValueHex; }
            set { regValueHex = value; }
        }

        public string RegValueString
        {
            get { return regValueString; }
            set { regValueString = value; }
        }

        public string RegTag
        {
            get { return regTag; }
            set 
            { 
                regTag = value;
                Register r = RegisterViewer.regList.getRegister(regTag);
                if (r != null)
                {
                    regId = r.Id;
                    if (regId > Register.INT_FLOAT_ID_DIVIDER)
                        isTypeInt = false;
                    else isTypeInt = true;
                }
            }
        }

        public bool IsTypeInt
        {
            get { return isTypeInt; }
            set { isTypeInt = value; }
        }


        public override string  ToString()      // Used for saving the file in CSV format
        {
             return regId + "," + regTag;
        }

        public int compareTo(RegisterInfo info){
            return this.RegId - info.regId;
        }
    }

    public class RegisterCollection : BindingList<RegisterInfo>
    {

        delegate void SetRegisterCallback();

        public RegisterCollection(bool populate)
        {
            if (populate)
            {
                populateCollection(RegisterViewer.REGISTERS_FILENAME);
            }
        }

        private void populateCollection(string filename)                    // Populate the collection with registers saved in filename
        {
            StreamReader sr = new StreamReader(filename);                   // Read in the register data from memory
            string registerString = sr.ReadToEnd();
            sr.Close();

            string[] registers = registerString.Split(new char[] { '\n' });    // Get each register in a string
            foreach (string r in registers)
            {
                RegisterInfo registerInfo = new RegisterInfo();
                if (registerInfo.populate(r))                                // Add the register to the collection if it's valid
                    this.Add(registerInfo);
            }
        }

        public void updateRegisterValue(int regId, int intValue, float floatValue)               // Insert value and valueHex into the row of regId in the table
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (Items[i].RegId == regId)
                {
                    Items[i].RegValueInt = intValue;
                    Items[i].RegValueFloat = floatValue;
                    

                    if (regId <= Register.INT_FLOAT_ID_DIVIDER)
                    {
                        Items[i].RegValueString = intValue + "";
                        Items[i].RegValueHex = SerialUtils.ByteToHex(SerialUtils.littleToBigEndian(intValue));
                    }
                    else
                    {
                        Items[i].RegValueString = floatValue + "";
                        Items[i].RegValueHex = SerialUtils.ByteToHex(SerialUtils.littleToBigEndian(floatValue));
                    }

                    if (RobotRacer.registerViewer.InvokeRequired)
                    {
                        // It's on a different thread, so use Invoke.
                        SetRegisterCallback d = new SetRegisterCallback(updateRegisterViewerBindings);
                        try
                        {
                            RobotRacer.registerViewer.Invoke(d, null);
                        }
                        catch (System.Exception ex) { Console.WriteLine("Error in invoke in registerviewer: " + ex.Message); }
                    }
                    else
                    {
                        ResetBindings();
                    }
                }
            }
        }

        private void updateRegisterViewerBindings()
        {
            ResetBindings();
        }

        protected override int FindCore(PropertyDescriptor prop, object key)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (Items[i].RegId == (int)key)
                    return i;
            }
            return -1;
        }

        protected override bool SupportsSortingCore
        {
            get{ return true; }
        }

        protected override void ApplySortCore(PropertyDescriptor prop, ListSortDirection direction)
        {
            sortProperty = prop;
            sortDirection = direction;

            List<RegisterInfo> list = (List<RegisterInfo>)Items;
            list.Sort(delegate(RegisterInfo lhs, RegisterInfo rhs)
            {
                if (sortProperty != null)
                {
                    object lhsValue = lhs == null ? null :
                    sortProperty.GetValue(lhs);
                    object rhsValue = rhs == null ? null :
                    sortProperty.GetValue(rhs);
                    int result;
                    if (lhsValue == null)
                    {
                        result = -1;
                    }
                    else if (rhsValue == null)
                    {
                        result = 1;
                    }
                    else
                    {
                        result = Comparer<object>.Default.Compare(lhsValue, rhsValue);
                    }
                    if (sortDirection == ListSortDirection.Descending)
                    {
                        result = -result;
                    }
                    return result;
                }
                else
                {
                    return 0;
                }
            });
        }

        private PropertyDescriptor sortProperty;
        protected override PropertyDescriptor SortPropertyCore
        {
            get{ return sortProperty; }
        }

        private ListSortDirection sortDirection;
        protected override ListSortDirection SortDirectionCore
        {
            get{ return sortDirection; }
        }
    }
}
