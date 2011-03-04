using System;
using System.IO.Ports;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;

namespace Robot_Racers
{

    public class SerialUtils
    {

        public enum TransmissionType : byte
        {
            TEXT = 0,
            PRIMITIVE = 1,
            STATE = 2,
            COMMAND = 3,
            IMAGE = 4,
            COURSE = 5,
            REGISTER = 6,
            ACKNOWLEDGE = 7,
            DATA_TRANSFER = 8
        }
        public enum TextSubtype : byte
        {
            GUI_W_NEWLINE = 0,
            GUI_WOUT_NEWLINE = 1
        }
        public enum PrimativeSubSubtype : byte //this is located as the first byte of data
        {
            PRIM_CHAR = 0, //only send 1 byte
            PRIM_UCHAR = 1, //only send 1 byte
            PRIM_INT = 2, //send 4 bytes
            PRIM_UINT = 3, //send 4 bytes
            PRIM_FLOAT = 4, //send 4 bytes
            PRIM_HEX4 = 5, //send 4 bytes
            PRIM_HEX1 = 6  //send 4 bytes
        }
        public enum CommandSubtype : byte
        {
            ALLSTOP = 0,
            STARTSTOP = 1,
            VELOCITY_STEERING = 2,
            REQ_STATE = 3,
            MODE = 4,
            EMOTION = 5,
            RESET = 6,
            HEARTBEAT = 7,
            MESSAGES = 8,
            VIDEO = 9,
            VELOCITY_MULT = 10,
            STEERING_TRIM = 11
        }
        public enum DataTransferSubtype : byte
        {
            REQUEST_BIN = 0, //Storage pointer for binary (4 bytes)
            BIN_INFO = 1, //Binary file exists (1 byte) Binary file size (4 bytes)
            WRITE_DATA = 2, //Binary data start loc (4 bytes) then 2 byte checksum of following data
            READ_DATA = 3, //data request location (4 bytes)
            RUN_BOOTLOADER = 4,
            READ_DATA_REPLY = 5, //Data sent with 2 byte checksum at beginning
            WRITE_SUCCESS = 6  //sent if the data checksum'd
        }

        public enum CourseSubtype : byte
        {
            SET_COURSE = 0,
            COURSE_RECEIVED = 1
        }
        public enum RegisterSubtype : byte
        {
            SET_INT = 0,
            SET_FLOAT = 1,
            GET_INT = 2,
            GET_FLOAT = 3,
            RECEIVE_INT = 4,
            RECEIVE_FLOAT = 5,
            SET_DYNAMIC_HSV = 6,
            GET_DYNAMIC_HSV = 7,
            RECEIVE_DYNAMIC_HSV = 8
        }
        public enum ImageSubtype : byte  //this is the most significant nibble of the subtype
        {   //if a frame is 'captured', any previous frame will be released first
            CAPTURE_AND_TRANSMIT = 0, //this captures and transmits an image (does not release)
            CAPTURE_CURRENT_FRAME = 1,
            RELEASE_CURRENT_FRAME = 2, //not needed to capture another frame, as by capturing, any
            // previously captured frames are released
            TRANSMIT_FRAME = 3, //sends the currently captured frame
            RETRANSMIT_SUB_FRAME = 4, //first 2 data bytes are the row to send, next 2 bytes are the starting
            // column to send. Will transmit from the starting column until
            // it reaches the end of the row.
            IMAGE_RESOLUTION = 5, //sent by the car to tell the GUI the resolution of image 2bytes w, 2bytes h
            IMAGE_DATA = 6,
            PROCESSED_IMAGE = 7,
            TRANSMIT_RECENT_CRITICAL = 8  //transmits the most recent critical capture (a frame that can be checked out and held for debug)
        }
        public enum ImageTypes : byte  //this is the least significant nibble of the subtype
        {
            RGB565 = 0, //5 bits RED, 6 bits GREEN, 5 bits BLUE
            SEGMENTED_1 = 1, //Binary Segmented image (0 or 1)									
            SEGMENTED_2 = 2, //Binary Segmented image (0 or 1)									
            SEGMENTED8_1 = 3, //8-bit Segmented image (each bit represents a threshold match)	
            SEGMENTED8_2 = 4, //8-bit Segmented image (each bit represents a threshold match)	
            HSV = 5 //32-bit HSV (HHSV)					
        }
        public void changeport(string whichport)
        {
            string oldport = comPort.PortName;
            if (comPort.IsOpen == true) comPort.Close();
            comPort.PortName = whichport;
            if (OpenPort())
            {
                return;
            }
            else
            {
                comPort.PortName = oldport;
                OpenPort();
            }
        }

        private SerialPort comPort;
        private Object comLock = new Object();
        private Object ackLock = new Object();

        private const int BAUD_RATE = 57600;//115200; //do not change this up unless ALL pipelines are increased
        private const Parity PARITY = Parity.None;
        private const StopBits STOP_BITS = StopBits.One;
        private const int DATA_BITS = 8;
        private const string PORT_NAME = "COM1";

        private uint numBytesDataRx = 0;
        private int currentIndex = 0;
        private SerialHeader header = null;
        private byte[] currentHeaderRx = new byte[SerialHeader.HEADER_BYTES];
        private byte[] currentDataRx;

        public static bool showTx = false;
        public static bool showRx = false;
        private List<DataPacket> dataAwaitingAcknowledge = new List<DataPacket>(10);
        public Timer acknowledgeTimer = new Timer();
        public Timer transmitBinary = new Timer();
        public Timer dataReceivedSecondCounter = new Timer();
        public static bool requireAcknowledge = false;
        private const int MAX_NUM_RETRIES = 5;

        private const int PROCESSED_PYLON_LEN = 10;


        public SerialUtils()
        {
            requireAcknowledge = false;
            comPort = new SerialPort();
            comPort.DataReceived += new SerialDataReceivedEventHandler(comPort_DataReceived);   // Add data received handler
            if (InitPort())
            {
                OpenPort();
            }
            else
            {
                Console.WriteLine("Cannot open port");
            }
            lock (ackLock)
            {
                dataAwaitingAcknowledge.Clear();
            }
            acknowledgeTimer.Tick += new System.EventHandler(acknowledgeTimer_Tick);    //Add tick event handler
            acknowledgeTimer.Interval = 150;               //reset timer interval in milliseconds
            acknowledgeTimer.Start(); //do not comment, the red button turns on and off acknowledge requests.
            transmitBinary.Tick += new System.EventHandler(transmitBinary_Tick);
            transmitBinary.Interval = 1000;
            transmitBinary.Stop();
            dataReceivedSecondCounter.Tick += new System.EventHandler(dataReceivedSecondCounter_Tick);
            dataReceivedSecondCounter.Interval = 1000;
            dataReceivedSecondCounter.Start(); //this always runs
        }



        public bool InitPort()
        {
            try
            {
                if (comPort.IsOpen == true) comPort.Close();    // Close the port if open to reset props

                comPort.BaudRate = BAUD_RATE;                   // Set serial comm properties
                comPort.DataBits = DATA_BITS;
                comPort.StopBits = STOP_BITS;
                comPort.Parity = PARITY;
                comPort.PortName = PORT_NAME;

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }

        }

        //search all available comports
        public void ScanPorts(ListBox.ObjectCollection objectCollection)
        {
            string oldport = comPort.PortName;  //save the old com port
            if (comPort.IsOpen == true) comPort.Close();

            List<object> goodPorts = new List<object>(); //these are available comports

            for (int i = 1; i < 21; i++)
            {
                comPort.PortName = "COM" + i.ToString();
                try
                {
                    comPort.Open();                                 // Open port with specified properties
                    Console.WriteLine("Com port " + comPort.PortName + " is available.");
                    goodPorts.Add(comPort.PortName);
                    comPort.Close();
                }
                catch (Exception ex)
                {
                    continue;
                }
            }
            comPort.PortName = oldport;
            if (goodPorts.Count == 0 || goodPorts.Contains(oldport))
            {
                comPort.PortName = oldport;
            }
            else
            {
                comPort.PortName = goodPorts[0].ToString();
            }
            OpenPort(); //reopen the original com port

            objectCollection.Clear();
            objectCollection.AddRange(goodPorts.ToArray());
        }

        public bool OpenPort()
        {
            try
            {
                if (comPort.IsOpen == true) comPort.Close();    // Close the port if open to reset props

                comPort.Open();                                 // Open port with specified properties

                if (TheKnack.debug)
                {
                    Console.WriteLine("Com port " + comPort.PortName + " is open.");
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        public bool ClosePort()
        {
            try
            {
                //comPort.Close();
                //comPort.DataReceived -= receiverHandler;
                comPort.Close();
                //comPort.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
        }

        private void dataReceivedSecondCounter_Tick(object sender, EventArgs e)
        {
            lock (lock_numBytesReceivedRecently)
            {
                string strNumBytesPerSecond = numBytesReceivedRecently_Total.ToString() + " B/s";
                TheKnack.racer.lblBytesPerSecond_Total.Text = strNumBytesPerSecond;
                TheKnack.racer.lblBytesPerSecond2.Text = numBytesReceivedRecently_Total.ToString() + " B/s"; ;
                TheKnack.racer.lblBytesPerSecond_TotalPackets.Text = numBytesReceivedRecently_TotalPackets.ToString() + " Packet/s"; ;
                TheKnack.racer.lblBytesPerSecond_Text.Text = numBytesReceivedRecently_Text.ToString() + " B/s"; ;
                TheKnack.racer.lblBytesPerSecond_Primative.Text = numBytesReceivedRecently_Primative.ToString() + " B/s"; ;
                TheKnack.racer.lblBytesPerSecond_State.Text = numBytesReceivedRecently_State.ToString() + " B/s"; ;
                TheKnack.racer.lblBytesPerSecond_Command.Text = numBytesReceivedRecently_Command.ToString() + " B/s"; ;
                TheKnack.racer.lblBytesPerSecond_Image.Text = numBytesReceivedRecently_Image.ToString() + " B/s"; ;
                TheKnack.racer.lblBytesPerSecond_Course.Text = numBytesReceivedRecently_Course.ToString() + " B/s"; ;
                TheKnack.racer.lblBytesPerSecond_Ack.Text = numBytesReceivedRecently_Acknowledge.ToString() + " B/s"; ;
                TheKnack.racer.lblBytesPerSecond_Register.Text = numBytesReceivedRecently_Register.ToString() + " B/s"; ;
                TheKnack.racer.lblBytesPerSecond_DataTrans.Text = numBytesReceivedRecently_Data_Transfer.ToString() + " B/s"; ;
                numBytesReceivedRecently_Total = 0;
                numBytesReceivedRecently_TotalPackets = 0;
                numBytesReceivedRecently_Text = 0;
                numBytesReceivedRecently_Primative = 0;
                numBytesReceivedRecently_State = 0;
                numBytesReceivedRecently_Command = 0;
                numBytesReceivedRecently_Image = 0;
                numBytesReceivedRecently_Course = 0;
                numBytesReceivedRecently_Acknowledge = 0;
                numBytesReceivedRecently_Register = 0;
                numBytesReceivedRecently_Data_Transfer = 0;
            }
        }
        private void transmitBinary_Tick(object sender, EventArgs e)
        {
            TheKnack.racer.continueTransmittingBinaryFile();
        }

        //this function is made because writing multiple items to the comport at same time causes issues
        private void transmitPacket(byte[] toWrite)
        {
            try
            {
                if (!(comPort.IsOpen == true))                              // Open Serial Port
                    comPort.Open();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            lock (comLock)
            {
                try
                {
                    comPort.Write(toWrite, 0, toWrite.Length);
                    if (showTx)
                    {
                        TheKnack.racer.writeToSerialTextBox(ByteToHex(toWrite), true);     // Write TX data to serial window in GUI
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }

        private void acknowledgeTimer_Tick(object sender, EventArgs e)
        {
            List<DataPacket> toRemove = new List<DataPacket>();
            lock (ackLock)
            {
                if (dataAwaitingAcknowledge.Count == 0) return;

                //resend all packets that haven't been acknowledged yet (and have waited more than .1 seconds)
                foreach (DataPacket ackPacket in dataAwaitingAcknowledge)
                {
                    ackPacket.NumRetries++;
                    if (ackPacket.NumRetries > 2) //make it a minimum wait of 2xInterval
                    {
                        transmitPacket(ackPacket.Data);
                        if (ackPacket.NumRetries > MAX_NUM_RETRIES)
                        {
                            DataPacket.packetsLost_TX++;
                            TheKnack.racer.writeToPacketLossBtn("pktLoss(t/r): " + (requireAcknowledge ? DataPacket.packetsLost_TX.ToString() : "?") + "/" + DataPacket.packetsLost_RX.ToString());
                            toRemove.Add(ackPacket);
                        }
                    }
                }
                foreach (DataPacket ackPacket in toRemove)
                {
                    dataAwaitingAcknowledge.Remove(ackPacket);
                }
            }
        }

        private void acknowledgePacketSend(byte[] packet, SerialHeader header)
        {
            List<DataPacket> toRemove = new List<DataPacket>();
            DataPacket newPacket = new DataPacket();
            newPacket.Head = header;
            newPacket.Data = packet;
            lock (ackLock)
            {
                foreach (DataPacket ackPacket in dataAwaitingAcknowledge)
                {
                    //if the packets are both commands and have the same subtype,
                    // remove the old one and put in the new one
                    if ((TransmissionType)header.Type == TransmissionType.COMMAND &&
                        (TransmissionType)ackPacket.Head.Type == TransmissionType.COMMAND &&
                        header.Subtype.Equals(ackPacket.Head.Subtype))
                    {
                        toRemove.Add(ackPacket);
                        DataPacket.packetsLost_TX++;
                    }
                }
                dataAwaitingAcknowledge.Add(newPacket);
                foreach (DataPacket ackPacket in toRemove)
                {
                    dataAwaitingAcknowledge.Remove(ackPacket);
                }
            }
        }

        static int numBytesReceivedRecently_Total = 0;
        static int numBytesReceivedRecently_TotalPackets = 0;
        static int numBytesReceivedRecently_Text = 0;
        static int numBytesReceivedRecently_Primative = 0;
        static int numBytesReceivedRecently_State = 0;
        static int numBytesReceivedRecently_Command = 0;
        static int numBytesReceivedRecently_Image = 0;
        static int numBytesReceivedRecently_Course = 0;
        static int numBytesReceivedRecently_Register = 0;
        static int numBytesReceivedRecently_Acknowledge = 0;
        static int numBytesReceivedRecently_Data_Transfer = 0;
        static object lock_numBytesReceivedRecently = new Object();
        public void comPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int bytesToRead = comPort.BytesToRead;              // Get number of bytes in the buffer

            byte[] comBuffer = new byte[bytesToRead];           // Create a byte array to hold the awaiting data

            comPort.Read(comBuffer, 0, bytesToRead);            // Populate the array comBuffer with RX data
            lock (lock_numBytesReceivedRecently)
            {
                numBytesReceivedRecently_Total += comBuffer.Length; //for reporting bytes per second
            }

            for (int i = 0; i < comBuffer.Length; i++)
            {
                //make sure the beginning of the header is caught
                if (currentIndex == 0 && comBuffer[i] != 0xFA) { currentIndex = 0; continue; }
                else if (currentIndex == 1 && comBuffer[i] != 0xCA) { currentIndex = 0; continue; }
                else if (currentIndex == 2 && comBuffer[i] != 0xDE) { currentIndex = 0; continue; }

                if (currentIndex < SerialHeader.HEADER_BYTES)
                {
                    currentHeaderRx[currentIndex] = comBuffer[i];   // Add data to the header array
                }
                currentIndex++;  //note the increment here, when currentDataRx is indexed, it will have already incremented

                if (currentIndex == SerialHeader.HEADER_BYTES)
                {
                    //we can now parse the header
                    header = new SerialHeader();
                    header.parseHeaderRx(currentHeaderRx);          // Populate the header object
                    if (header.CheckSum != 0)
                    { //the checksum didn't match up, so data loss
                        if (showRx)
                        {
                            TheKnack.racer.writeToSerialTextBox("CHKSUM(header):" + ByteToHex(currentHeaderRx), false);     // Write RX data to serial window in GUI
                        }
                        currentIndex = 0;// findNewStartIndex(currentHeaderRx, currentDataRx, currentIndex);//reparse the data we've received:
                        continue;
                    }

                    numBytesDataRx = header.DataLen;
                    currentDataRx = new byte[header.DataLen];       // Create data buffer for this packet
                }

                if (currentIndex > SerialHeader.HEADER_BYTES && numBytesDataRx > 0)
                {
                    //we're now receiving the data part of the packet (currentIndex has already been incremented)
                    currentDataRx[currentIndex - (SerialHeader.HEADER_BYTES + 1)] = comBuffer[i];   // Add packet data to the data array
                }

                if (currentIndex >= SerialHeader.HEADER_BYTES)
                {
                    if (currentIndex == SerialHeader.HEADER_BYTES + numBytesDataRx)
                    {
                        //we've completely received a packet
                        //the data is now completely received
                        if (getChecksum(currentDataRx) != 0)
                        {
                            if (showRx)
                            {
                                TheKnack.racer.writeToSerialTextBox("CHKSUM(data):" + ByteToHex(currentHeaderRx) + ByteToHex(currentDataRx), false);     // Write RX data to serial window in GUI
                            }
                            currentIndex = 0;
                            continue;
                        }
                        //the data's alright, so only grab the non-checksum part
                        if (showRx)
                        {
                            TheKnack.racer.writeToSerialTextBox(ByteToHex(currentHeaderRx) + ByteToHex(currentDataRx), false);     // Write RX data to serial window in GUI
                        }
                        if (currentDataRx != null && currentDataRx.Length > 2)
                        {
                            byte[] nonChecksum = new byte[currentDataRx.Length - 2];
                            for (int j = 0; j < nonChecksum.Length; j++)
                            {
                                nonChecksum[j] = currentDataRx[j];
                            }
                            currentDataRx = nonChecksum;
                        }
                        ProcessPacket(header, currentDataRx); //figure out what the packet is and act accordingly
                        currentIndex = 0;// Reset the data index for the data of the next packet
                    }


                }
            }
        }

        private void ProcessPacket(SerialHeader header, byte[] data)
        {
            if ((TransmissionType)header.Type != TransmissionType.ACKNOWLEDGE) //acknowledges have TX numbers
            {
                if (DataPacket.startedRXing)
                {
                    //int nextNum = (DataPacket.lastRxNum + 1) % 128;
                    int distanceBetween = 0;
                    int maxOffset = 30;
                    int lastNumCheck = (DataPacket.lastRxNum + maxOffset) % 128;
                    for (int i = DataPacket.lastRxNum; i != lastNumCheck && i != header.PacketNumRx; i = (i + 1) % 128)
                    {
                        distanceBetween++;
                    }
                    if (distanceBetween > 0 && distanceBetween < maxOffset)
                    {
                        DataPacket.lastRxNum = header.PacketNumRx;
                        DataPacket.packetsLost_RX += distanceBetween - 1;
                        if (TheKnack.racer != null)
                            TheKnack.racer.writeToPacketLossBtn("pktLoss(t/r): " + (requireAcknowledge ? DataPacket.packetsLost_TX.ToString() : "?") + "/" + DataPacket.packetsLost_RX.ToString());
                    }
                }
                else
                {
                    DataPacket.startedRXing = true;
                    DataPacket.lastRxNum = header.PacketNumRx;
                }
            }
            int totalPacketSize = SerialHeader.HEADER_BYTES + data.Length + 2;//data's checksum bytes were already removed, so add back on for stats
            lock (lock_numBytesReceivedRecently)
            {
                numBytesReceivedRecently_TotalPackets++;
                switch ((TransmissionType)header.Type)
                {
                    case TransmissionType.TEXT:
                        numBytesReceivedRecently_Text += totalPacketSize;
                        break;
                    case TransmissionType.COMMAND:
                        numBytesReceivedRecently_Command += totalPacketSize;
                        break;
                    case TransmissionType.COURSE:
                        numBytesReceivedRecently_Course += totalPacketSize;
                        break;
                    case TransmissionType.STATE:
                        numBytesReceivedRecently_State += totalPacketSize;
                        break;
                    case TransmissionType.PRIMITIVE:
                        numBytesReceivedRecently_Primative += totalPacketSize;
                        break;
                    case TransmissionType.REGISTER:
                        numBytesReceivedRecently_Register += totalPacketSize;
                        break;
                    case TransmissionType.IMAGE:
                        numBytesReceivedRecently_Image += totalPacketSize;
                        break;
                    case TransmissionType.ACKNOWLEDGE:
                        numBytesReceivedRecently_Acknowledge += totalPacketSize;
                        break;
                    case TransmissionType.DATA_TRANSFER:
                        numBytesReceivedRecently_Data_Transfer += totalPacketSize;
                        break;
                }
            }
            //send the data to the appropriate function based on the packet type
            switch ((TransmissionType)header.Type)
            {
                case TransmissionType.TEXT:
                    ProcessText(header, data);
                    break;
                case TransmissionType.COMMAND:
                    ProcessCommand(header, data);
                    break;
                case TransmissionType.COURSE:
                    RobotRacer.courseLoaded = true;
                    break;
                case TransmissionType.STATE:
                    ProcessState(header, data);
                    break;
                case TransmissionType.PRIMITIVE:
                    ProcessPrimative(header, data);
                    break;
                case TransmissionType.REGISTER:
                    ProcessRegister(header, data);
                    break;
                case TransmissionType.IMAGE:
                    ProcessImage(header, data);
                    break;
                case TransmissionType.ACKNOWLEDGE:
                    ProcessAcknowledge(header);
                    break;
                case TransmissionType.DATA_TRANSFER:
                    ProcessDataTransfer(header, data);
                    break;
            }
        }

        private void ProcessDataTransfer(SerialHeader header, byte[] data)
        {

            switch ((DataTransferSubtype)header.Subtype)
            {
                case DataTransferSubtype.WRITE_SUCCESS: //car received the data correctly, so continue
                    if (data.Length == 0 || data[0] == 0) //bad transmit
                    {
                        TheKnack.racer.continueTransmittingBinaryFile();
                    }
                    else //transmitted successfully
                    {
                        TheKnack.racer.incrementBinaryFileAndContinue(data);
                    }
                    break;
                case DataTransferSubtype.READ_DATA_REPLY: //car returned with data we asked for
                    //checksum and check data
                    break;
                case DataTransferSubtype.REQUEST_BIN:
                    TheKnack.racer.startTransmittingBinaryFile((int)data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]);
                    break;
            }
        }

        private void ProcessAcknowledge(SerialHeader header)
        {
            lock (ackLock)
            {
                foreach (DataPacket ackPacket in dataAwaitingAcknowledge)
                {
                    if (ackPacket.Head.PacketNumRx == header.PacketNumRx)
                    {
                        dataAwaitingAcknowledge.Remove(ackPacket);
                        break;
                    }
                }
            }
        }

        private void ProcessCommand(SerialHeader header, byte[] data)
        {

            if (header.Subtype == (byte)SerialUtils.CommandSubtype.MODE)
            {
                TheKnack.racer.resetGUI();
            }
            else
            {
                TheKnack.racer.resetHeartbeatTimer(); //this stops sending the all-stop signal
            }
        }

        private void ProcessImage(SerialHeader header, byte[] data)
        {
            if (TheKnack.racer == null) return;
            ImageSubtype imageSubtype = (ImageSubtype)((byte)header.Subtype >> 4); //the subtype is actually the first 4 bits
            ImageTypes imageType = (ImageTypes)((byte)header.Subtype & 0xF);
            if (imageSubtype == ImageSubtype.PROCESSED_IMAGE)
            {
                int numPylons = header.DataLen / PROCESSED_PYLON_LEN;
                //Console.WriteLine(numPylons + " is the number of pylons");
                TheKnack.racer.updateProcessedImage(numPylons, data);
            }
            else
            {

                switch (imageSubtype)
                {
                    case ImageSubtype.IMAGE_DATA: //it's the image data, so display it
                        TheKnack.racer.addImageData((int)data[2] << 8 | data[3],
                                                  (int)data[0] << 8 | data[1],
                                                  (byte[])data,
                                                  4, imageType);
                        break;
                    case ImageSubtype.IMAGE_RESOLUTION:
                        TheKnack.racer.newImageCapture((int)data[0] << 8 | data[1], //width
                                                       (int)data[2] << 8 | data[3], imageType);//height
                        break;
                }
            }
        }

        private void ProcessPrimative(SerialHeader header, byte[] data)
        {
            string strData = "??";
            //just print data to the message box
            switch ((PrimativeSubSubtype)data[0])
            {
                case PrimativeSubSubtype.PRIM_CHAR:
                case PrimativeSubSubtype.PRIM_UCHAR:
                    strData = ((char)data[1]).ToString();
                    break;
                case PrimativeSubSubtype.PRIM_FLOAT:
                    float fltData = (float)(data[1] << 24 | data[2] << 16 | data[3] << 8 | data[4]);
                    strData = fltData.ToString();
                    break;
                case PrimativeSubSubtype.PRIM_INT:
                    Int32 intData = (Int32)(data[1] << 24 | data[2] << 16 | data[3] << 8 | data[4]);
                    strData = intData.ToString();
                    break;
                case PrimativeSubSubtype.PRIM_UINT:
                    UInt32 uintData = (UInt32)(data[1] << 24 | data[2] << 16 | data[3] << 8 | data[4]);
                    strData = uintData.ToString();
                    break;
                case PrimativeSubSubtype.PRIM_HEX4:
                    intData = (Int32)(data[1] << 24 | data[2] << 16 | data[3] << 8 | data[4]);
                    strData = ByteToHex(intData);
                    break;
                case PrimativeSubSubtype.PRIM_HEX1:
                    strData = ByteToHex(data[4]);
                    break;
            }
            TheKnack.racer.writeToTruckMessageBox(" " + strData,
                (header.Subtype == (byte)SerialUtils.TextSubtype.GUI_W_NEWLINE ? true : false));
        }

        private void ProcessState(SerialHeader header, byte[] data)
        {
            //right now, just write the state to the message box
            //TheKnack.racer.writeToTruckMessageBox("STATE: " + ByteToHex(data),
            //(header.Subtype == (byte)SerialUtils.TextSubtype.GUI_W_NEWLINE ? true : false));

            float currentVelocity = BitConverter.ToSingle(SerialUtils.reverseBytes(data, 0, 3), 0);
            Int32 sentSteeringAngle = (((Int32)data[4]) << 24) >> 24; //sign extend
            short heading = BitConverter.ToInt16(SerialUtils.reverseBytes(data, 5, 6), 0);

            byte currentPylon = data[7];
            byte mode = data[8];
            float distTraveled = BitConverter.ToSingle(SerialUtils.reverseBytes(data, 9, 12), 0);
            char angleToPylon = (char)data[13];
            float distToPylon = BitConverter.ToSingle(SerialUtils.reverseBytes(data, 14, 17), 0);
            float distToBlind = BitConverter.ToSingle(SerialUtils.reverseBytes(data, 18, 21), 0);
            if (data.Length > 22)
            {
                byte hwFreq = data[22];
                byte swFreq = data[23];
                RobotRacer.stateVariables.HwFPS = hwFreq;
                RobotRacer.stateVariables.SwFPS = swFreq;
            }

            RobotRacer.stateVariables.Velocity = currentVelocity;
            RobotRacer.stateVariables.SentSteeringAngle = sentSteeringAngle;
            RobotRacer.stateVariables.HeadingAngle = heading;

            RobotRacer.stateVariables.PylonNumber = currentPylon;
            RobotRacer.stateVariables.CarState = mode;
            RobotRacer.stateVariables.DistanceTraveled = distTraveled;
            RobotRacer.stateVariables.AngleToPylon = (byte)angleToPylon;
            RobotRacer.stateVariables.DistanceToPylon = distToPylon;
            RobotRacer.stateVariables.DistanceToBlind = distToBlind;

            if (TheKnack.racer != null)
                TheKnack.racer.updateStateVariables();
        }

        private void ProcessText(SerialHeader header, byte[] data)
        {
            //just print text to the message box
            TheKnack.racer.writeToTruckMessageBox(ByteToString(data), (header.Subtype == (byte)SerialUtils.TextSubtype.GUI_W_NEWLINE ? true : false));
        }

        private void ProcessRegister(SerialHeader header, byte[] data)
        {
            //if (header.Subtype.Equals(RegisterSubtype.RECEIVE_FLOAT) || header.Subtype.Equals(RegisterSubtype.RECEIVE_INT))
            //{
            ushort regId = BitConverter.ToUInt16(reverseBytes(data, 0, 1), 0);

            try
            {
                if (regId <= Register.INT_FLOAT_ID_DIVIDER)
                {
                    int regValueInt = BitConverter.ToInt32(reverseBytes(data, 2, 5), 0);
                    RobotRacer.registerViewer.updateRegisterValue(regId, regValueInt, -1);
                }
                else
                {
                    float regValueFloat = BitConverter.ToSingle(reverseBytes(data, 2, 5), 0);
                    RobotRacer.registerViewer.updateRegisterValue(regId, -1, regValueFloat);
                }
            }
            catch (System.Exception e) { System.Console.WriteLine(e.Message + "\n\tSerialUtils:ProcessRegister call"); }
            //}
            //else if (header.Subtype.Equals(RegisterSubtype.RECEIVE_DYNAMIC_HSV))
            //{
            //    Console.WriteLine(ByteToHex(data));
            //}
        }

        public void transmitData(SerialHeader header) //no data transmit method
        {
            transmitData(header, null);
        }

        public void transmitByteData(SerialHeader header, byte[] packetData)
        {
            transmitData(header, packetData);
        }

        public void transmitIntData(SerialHeader header, int intData)          // Int transmit method
        {
            byte[] packetData = littleToBigEndian(intData);                 // Convert to bit endian byte[]
            transmitData(header, packetData);                               // Send to final transmit method
        }

        public void transmitStrData(SerialHeader header, string stringData)    // String transmit method
        {
            byte[] packetData = new byte[stringData.ToCharArray().Length];
            int i = 0;
            foreach (char ch in stringData.ToCharArray())                   // Convert char[] to byte[]
            {
                packetData[i] = (byte)ch;
                i++;
            }
            transmitData(header, packetData);                               // Send to final transmit method
        }

        public void transmitRegData(SerialHeader header, ushort regId, int regData)
        {
            byte[] regIdBigEndian = SerialUtils.littleToBigEndian(regId);
            byte[] regDataBigEndian = SerialUtils.littleToBigEndian(regData);

            transmitData(header, concatonateArrays(regIdBigEndian, regDataBigEndian));
        }

        public void transmitRegData(SerialHeader header, ushort regId, float regData)
        {
            byte[] regIdBigEndian = SerialUtils.littleToBigEndian(regId);
            byte[] regDataBigEndian = SerialUtils.littleToBigEndian(regData);

            transmitData(header, concatonateArrays(regIdBigEndian, regDataBigEndian));
        }

        public void transmitCourseData(Course course)
        {
            int index = 0;
            byte[] data = new byte[(course.getNumPoints() * 4 * 3) + 8]; //4 bytes per int, 3 ints per pylon (angle, trim, distanceMultiple) + 2 ints for starting angle and distanceMultiple

            byte[] tempInitial = SerialUtils.reverseBytes(BitConverter.GetBytes(course.getInitialAngle()), 0, 3);
            byte[] tempUnit = SerialUtils.reverseBytes(BitConverter.GetBytes(course.getUnitLength()), 0, 3);

            for (int j = 0; j < 4; j++)
            {
                data[index++] = tempInitial[j];
            }
            for (int j = 0; j < 4; j++)
            {
                data[index++] = tempUnit[j];
            }

            for (int i = 0; i < course.getNumPoints(); i++)
            {
                int angle = course.getAngleAt(i);
                int angleTrim = course.getTrimAt(i);
                int distance = course.getDistanceAt(i);
                byte[] tempData1 = SerialUtils.reverseBytes(BitConverter.GetBytes(angle), 0, 3);
                byte[] tempData2 = SerialUtils.reverseBytes(BitConverter.GetBytes(angleTrim), 0, 3);
                byte[] tempData3 = SerialUtils.reverseBytes(BitConverter.GetBytes(distance), 0, 3);

                for (int j = 0; j < 4; j++)
                {
                    data[index++] = tempData1[j];
                }
                for (int j = 0; j < 4; j++)
                {
                    data[index++] = tempData2[j];
                }
                for (int j = 0; j < 4; j++)
                {
                    data[index++] = tempData3[j];
                }
            }

            SerialHeader header = new SerialHeader();
            header.Type = (byte)SerialUtils.TransmissionType.COURSE;

            transmitData(header, data);
        }

        private void transmitData(SerialHeader header, byte[] packetData)   // Final Transmit method 
        {
            const int checksumLength = 2;

            if ((!requireAcknowledge && !((TransmissionType)header.Type == TransmissionType.COMMAND &&
                                          (CommandSubtype)header.Subtype == CommandSubtype.ALLSTOP)) || //if we don't require acknowledges, then set to false
                ((TransmissionType)header.Type == TransmissionType.COMMAND &&
                 (CommandSubtype)header.Subtype == CommandSubtype.HEARTBEAT) || //heartbeats are sent too often
                ((TransmissionType)header.Type == TransmissionType.IMAGE &&
                  header.Subtype >> 4 == Convert.ToInt16(ImageSubtype.RETRANSMIT_SUB_FRAME)))
            {
                ; //we don't need to request acknowledgement, they'll be sent again
            }
            else
            {
                header.RequestAck = true; //make all packets sent request an acknoweldge:
            }

            header.DataLen = Convert.ToUInt16((packetData != null) ? packetData.Length + checksumLength : 0); //checksum is 2 bytes long
            if (header.DataLen % 2 == 1) header.DataLen++;//it needs to be a multiple of 2

            byte[] headerBytes = header.getTxHeaderBytes();                 // Get the packet header bytes

            byte[] packet = new byte[headerBytes.Length + header.DataLen];   // Create final packet array of bytes

            int p = 0;
            for (int i = 0; i < headerBytes.Length; i++, p++)               // Add header to final packet
            {
                packet[p] = headerBytes[i];
            }
            for (int i = 0; packetData != null && i < packetData.Length; i++, p++)                // Add data to final packet
            {
                packet[p] = packetData[i];
            }
            if (header.DataLen > 0) //only do checksum for data if there is some :)
            {
                UInt16 dataChecksum = getChecksum(packetData);
                byte[] checkSumBytes = SerialUtils.littleToBigEndian(dataChecksum);
                packet[packet.Length - 2] = checkSumBytes[0];
                packet[packet.Length - 1] = checkSumBytes[1]; //store the checksum at the end of the data
            }

            if (header.RequestAck)
            {
                acknowledgePacketSend(packet, header);                   //setup auto-resend of packet
            }
            transmitPacket(packet);

        }

        public static string ByteToString(byte[] comByte)
        {
            StringBuilder builder = new StringBuilder(comByte.Length);
            foreach (byte data in comByte)
            {
                builder.Append(Convert.ToString((char)data));
            }
            return builder.ToString();
        }

        public static byte[] reverseBytes(byte[] bytes, int startIndex, int endIndex)
        {
            byte[] reversed = new byte[endIndex - startIndex + 1];      // Make correct sized byte array
            for (int i = 0, j = endIndex; j >= startIndex; i++, j--)    // Insert into array in reverse order
            {
                reversed[i] = bytes[j];
            }
            return reversed;
        }

        public static byte[] concatonateArrays(byte[] a, byte[] b)
        {
            byte[] result = new byte[a.Length + b.Length];
            int i = 0;

            for (; i < a.Length; i++)
            {
                result[i] = a[i];
            }
            for (int j = 0; j < b.Length; j++, i++)
            {
                result[i] = b[j];
            }
            return result;
        }

        public static string ByteToHex(byte comByte)
        {
            byte[] comByteArray = new byte[1];
            comByteArray[0] = comByte;
            return ByteToHex(comByteArray);
        }

        public static string ByteToHex(int comByte)
        {
            byte[] comByteArray = new byte[4];
            comByteArray[0] = Convert.ToByte(comByte >> 24 & 0xFF);
            comByteArray[1] = Convert.ToByte(comByte >> 16 & 0xFF);
            comByteArray[2] = Convert.ToByte(comByte >> 8 & 0xFF);
            comByteArray[3] = Convert.ToByte(comByte & 0xFF);
            return ByteToHex(comByteArray);
        }

        public static string ByteToHex(byte[] comByte)
        {
            StringBuilder builder = new StringBuilder(comByte.Length * 3);  //create a new StringBuilder object

            foreach (byte data in comByte)                                  //loop through each byte in the array
            {
                builder.Append(Convert.ToString(data, 16).PadLeft(2, '0').PadRight(3, ' ')); //convert the byte to a string and add to the stringbuilder
            }
            return builder.ToString().ToUpper();
        }


        public static byte[] littleToBigEndian(int value)
        {
            byte[] bigEndian = BitConverter.GetBytes(value);
            Array.Reverse(bigEndian);
            return bigEndian;
        }

        public static byte[] littleToBigEndian(float value)
        {
            byte[] bigEndian = BitConverter.GetBytes(value);
            Array.Reverse(bigEndian);
            return bigEndian;
        }

        public static byte[] littleToBigEndian(uint value)
        {
            byte[] bigEndian = BitConverter.GetBytes(value);
            Array.Reverse(bigEndian);
            return bigEndian;
        }

        public static byte[] littleToBigEndian(ushort value)
        {
            byte[] bigEndian = BitConverter.GetBytes(value);
            Array.Reverse(bigEndian);
            return bigEndian;
        }



        public void closeConnections()
        {
            comPort.Close();
        }

        public static UInt16 getChecksum(byte[] data)
        {
            UInt16 checksum = 0;
            for (int i = 0; i < data.Length; i++)
            {
                checksum += Convert.ToUInt16(i % 2 == 0 ? data[i] << 8 : data[i]);
            }
            return (UInt16)(-checksum);
        }

    }

    /// <summary>
    /// This stores the data sent to the car. Useful when having to resend packets when 
    /// not acknowledged by car
    /// </summary>
    public class DataPacket
    {
        public static Int32 packetsLost_TX = 0;
        public static Int32 packetsLost_RX = 0;
        public static bool startedRXing = false; //this is used to let us know if lastRxNum is valid
        public static Int32 lastRxNum = 0;

        private Int32 numRetries = 0;
        private byte[] data = null;
        private SerialHeader head = null;


        public Int32 PacketsLost_TX
        {
            get { return packetsLost_TX; }
        }
        public Int32 PacketsLost_RX
        {
            get { return packetsLost_RX; }
        }
        public Int32 NumRetries
        {
            get { return numRetries; }
            set { numRetries = value; }
        }
        public byte[] Data
        {
            get { return data; }
            set { data = value; }
        }
        public SerialHeader Head
        {
            get { return head; }
            set { head = value; }
        }

    }

    public class SerialHeader
    {
        public const int HEADER_BYTES = 10;

        private static byte packetNumTx = 0;

        private byte packetNumRx = 0;
        private bool requestAck = false;
        private byte type = 0;
        private byte subtype = 0;
        private UInt16 dataLen = 0;
        private UInt16 checkSum = 0;

        public SerialHeader() { }

        public byte PacketNumRx
        {
            get { return packetNumRx; }
        }
        public bool RequestAck
        {
            get { return requestAck; }
            set { requestAck = value; }
        }
        public byte PacketNumTx
        {
            get { return packetNumTx; }
        }
        public byte Type
        {
            get { return type; }
            set { type = value; }
        }
        public byte Subtype
        {
            get { return subtype; }
            set { subtype = value; }
        }
        public UInt16 DataLen
        {
            get { return dataLen; }
            set { dataLen = value; }
        }
        public UInt16 CheckSum
        {
            get { return checkSum; }
            set { checkSum = value; }
        }

        public byte[] getTxHeaderBytes()
        {
            packetNumTx++;

            byte[] header = new byte[HEADER_BYTES];
            header[0] = 0xFA;
            header[1] = 0xCA;
            header[2] = 0xDE;

            packetNumRx = Convert.ToByte(Convert.ToInt32(packetNumTx) & 0x7F);
            header[3] = Convert.ToByte(Convert.ToInt32(packetNumTx) & 0x7F); //the msb is used for requestAck
            if (requestAck) header[3] |= 0x80;
            header[4] = type;
            header[5] = subtype;

            byte[] dataLenBytes = SerialUtils.littleToBigEndian(dataLen);
            header[6] = dataLenBytes[0];
            header[7] = dataLenBytes[1];
            header[8] = 0; //set to zero so the checksum is fine
            header[9] = 0;

            //calculate the checksum
            checkSum = SerialUtils.getChecksum(header);
            byte[] checkSumBytes = SerialUtils.littleToBigEndian(checkSum);
            header[8] = checkSumBytes[0];
            header[9] = checkSumBytes[1];
            return header;
        }

        public void parseHeaderRx(byte[] rxHeader)
        {
            if (HEADER_BYTES != rxHeader.Length)
            {
                Console.WriteLine("Wrong number of bytes in RX header");
            }
            else if (rxHeader[0] == 0xFA && rxHeader[1] == 0xCA && rxHeader[2] == 0xDE)
            {
                packetNumRx = Convert.ToByte(Convert.ToInt32(rxHeader[3]) & 0x7F); //the msb is used for requestAck
                requestAck = (rxHeader[3] & 0x80) > 0 ? true : false;
                type = rxHeader[4];
                subtype = rxHeader[5];

                byte[] tempDataLen = new byte[] { rxHeader[7], rxHeader[6] }; // Bits switched to change big endian to little endian data
                dataLen = BitConverter.ToUInt16(tempDataLen, 0);
                checkSum = SerialUtils.getChecksum(rxHeader);
            }
            else
            {
                Console.WriteLine("Rx header doesn't have the magic");
            }
        }
    }
}
