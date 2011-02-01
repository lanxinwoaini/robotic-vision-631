using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;


namespace Robot_Racers
{
    class RawImage : Panel
    {
        private Object thisLock = new Object();
        private Object rawLock = new Object();
        delegate void InvalidateRegionCallback(Rectangle r);

        //SerialUtils.ImageTypes imageType;
        bool rawImageInitialized = false;
        Image imageData;
        Int32[] imageRaw;
        Graphics painter;  //used for painting the pixels
        Bitmap newImagePart; //used for updating pixels
        int WIDTH = 320;              //Window Dimensions
        int HEIGHT = 240;
        private int PIXELS_PER_TRANS = 160;
        private SerialUtils.ImageTypes currentImageType = SerialUtils.ImageTypes.RGB565;

        int TRANS_PER_ROW = 2; //this is the max # of transmissions needed to send a row of image

        Timer transmissionTimeoutTimer = new Timer();
        bool[] imageSegmentsComplete = null; //imageSegmentsComplete is used for correcting missing packets
        bool fillingInImage = false; //when true the GUI requests missing data from car
        
        bool receivedData = false; //used for timer, true if received data between ticks

        public RawImage()
        {
            this.DoubleBuffered = true;
            //imageType = SerialUtils.ImageTypes.RGB565;
            imageData = new Bitmap(WIDTH, HEIGHT);
            imageRaw  = new Int32[WIDTH*HEIGHT];
            painter = Graphics.FromImage(imageData);
            newImagePart = new Bitmap(imageData);
            currentImageType = SerialUtils.ImageTypes.RGB565;

            transmissionTimeoutTimer.Interval = 500;              //Set timer interval in milliseconds
            transmissionTimeoutTimer.Tick += new System.EventHandler(transmissionTimeoutTimer_Tick);    //Add tick event handler
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (!rawImageInitialized) return;

            //get the coordinates of the mouse and display them as well as the pixel color
            int imageY = e.Y * System.Math.Max(1,(HEIGHT/this.Height));
            int imageX = e.X * System.Math.Max(1, (WIDTH / this.Width));
            TheKnack.racer.lblImageCoords.Text = imageX.ToString() + "," + imageY.ToString();

            lock (rawLock) //imageData needs to be locked due to multi-threaded use of it
            {
                TheKnack.racer.lblImageColorHex.Text = ConvertInt24ToHexString(imageRaw[imageY * WIDTH + imageX]);
                TheKnack.racer.lblImageColorBinary.Text = ConvertByteToBinaryString(imageRaw[imageY * WIDTH + imageX]);
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);
            if (!rawImageInitialized) return;
            int imageY = e.Y * System.Math.Max(1,(HEIGHT/this.Height));
            int imageX = e.X * System.Math.Max(1, (WIDTH / this.Width));
            for (int y = System.Math.Max(0,imageY - 3); y < System.Math.Min(HEIGHT-1,imageY + 4); y++)
            {
                imageSegmentsComplete[y * (WIDTH / PIXELS_PER_TRANS) + imageX / PIXELS_PER_TRANS] = false;
            }
            fillingInImage = true;
            imageCleanupRequest();
        }
        
        protected override void OnPaint(PaintEventArgs paintEvnt)
        {
            lock (thisLock) //imageData needs to be locked due to multi-threaded use of it
            {
                Graphics g = paintEvnt.Graphics;
                g.DrawImage(imageData, new Rectangle(0, 0, 320, 240));
            }
        }

        public void stopImageCleanup() //stop the GUI from requesting image fill-in data from car
        {
            fillingInImage = false; 
        }

        public void newCapture(int width, int height, SerialUtils.ImageTypes imageType)
        {
            transmissionTimeoutTimer.Start();
            if (width % 320 > 0 || height % 240 > 0) return; //it's sending wrong data, just ignore initializing, and hopefully nothing bad will happen, lol
            currentImageType = imageType;
            switch (imageType)
            {
                case SerialUtils.ImageTypes.SEGMENTED_1:
                case SerialUtils.ImageTypes.SEGMENTED_2:
                    PIXELS_PER_TRANS = 640;
                    break;
                case SerialUtils.ImageTypes.SEGMENTED8_1:
                case SerialUtils.ImageTypes.SEGMENTED8_2:
                    PIXELS_PER_TRANS = 320;
                    break;
                case SerialUtils.ImageTypes.RGB565:
                case SerialUtils.ImageTypes.HSV:
                default:
                    PIXELS_PER_TRANS = 160;
                    break;
            }
            WIDTH = width;              //Window Dimensions
            HEIGHT = height;

            TRANS_PER_ROW = ((WIDTH + PIXELS_PER_TRANS - 1) / PIXELS_PER_TRANS); //this is the max # of transmissions needed to send a row of image
            imageSegmentsComplete = new bool[HEIGHT * WIDTH / PIXELS_PER_TRANS];

            lock (thisLock) //imageData needs to be locked due to multi-threaded use of it
            {
                imageRaw = new Int32[WIDTH * HEIGHT];
                imageData = new Bitmap(WIDTH, HEIGHT); //reset the current image
                painter = Graphics.FromImage(imageData);
                newImagePart = new Bitmap(imageData);
            }

            lock (rawLock)
            {
                for (int i = 0; i < WIDTH * HEIGHT; i++)
                {
                    imageRaw[i] = 0;
                }
            }
            for (int i = 0; i < imageSegmentsComplete.Length; i++)
            {
                imageSegmentsComplete[i] = false; //imageSegmentsComplete is used for correcting missing packets
            }
            fillingInImage = false;
            receivedData = true; //set to true in case the timer comes immediately after receiving "newCapture" signal
            rawImageInitialized = true;
        }

        public void saveImage(String fileName)
        {
            if (!rawImageInitialized) return;
            imageData.Save(fileName, System.Drawing.Imaging.ImageFormat.Bmp);
        }

        public void AddRGBData(int col, int row, byte[] data, int offset)
        {
            if (!rawImageInitialized || currentImageType != SerialUtils.ImageTypes.RGB565)
            {
                TheKnack.racer.writeToTruckMessageBox("[GUI] missed image header packet(RGB)", true);
                newCapture(640, 480, SerialUtils.ImageTypes.RGB565);//we haven't received anything before
            }
            if (row >= HEIGHT || row < 0 || col >= WIDTH || col < 0)
            {
                return;
            }
            receivedData = true;
            //transmissionTimeoutTimer.Start(); //in case image data is randomly received, make sure we get all of it

            lock (thisLock) //imageData needs to be locked due to multi-threaded use of it
            {
                // create an object that will do the drawing operations
                int dataRemaining = (data.Length - offset) / 2;//2 is the number of bytes per pixel
                int curData = offset;
                // draw the received data on the image
                lock (rawLock)
                {
                    for (int y = row; y < HEIGHT && dataRemaining > 0; y++)
                    {
                        for (int x = col; x < WIDTH && dataRemaining > 0; x++)
                        {
                            //RGB565 = 5 bits red, 6 bits green, 5 bits blue
                            int red = data[curData] >> 3;
                            int green = ((data[curData] & 0x7) << 3) | (data[curData + 1] >> 5);
                            int blue = data[curData + 1] & 0x1F;
                            newImagePart.SetPixel(x, y,
                                Color.FromArgb(red << 3,
                                               green << 2,
                                               blue << 3));
                            imageRaw[y * WIDTH + x] = red << (3 + 16) |
                                                      green << (2 + 8) |
                                                      blue << (3);
                            dataRemaining--;
                            curData += 2;
                        }
                    }
                }
                //record that we received this packet
                imageSegmentsComplete[row * (WIDTH / PIXELS_PER_TRANS) + col / PIXELS_PER_TRANS] = true;
                painter.DrawImage(newImagePart, new Rectangle(0, 0, WIDTH, HEIGHT));

            }

            //now invalidate the region
            Invalidate();
            if (fillingInImage) //request the next section from the car
            {
                imageCleanupRequest(); //request missing packets from car
            }
        }

        public void AddHSVData(int col, int row, byte[] data, int offset)
        {
            if (!rawImageInitialized || currentImageType != SerialUtils.ImageTypes.HSV)
            {
                TheKnack.racer.writeToTruckMessageBox("[GUI] missed image header packet(HSV)", true);
                newCapture(640, 480, SerialUtils.ImageTypes.HSV);//we haven't received anything before
            }
            if (row >= HEIGHT || row < 0 || col >= WIDTH || col < 0)
            {
                return;
            }
            receivedData = true;
            //transmissionTimeoutTimer.Start(); //in case image data is randomly received, make sure we get all of it

            lock (thisLock) //imageData needs to be locked due to multi-threaded use of it
            {
                // create an object that will do the drawing operations
                int dataRemaining = (data.Length - offset) / 2; //2 is the number of bytes per pixel
                int curData = offset;
                // draw the received data on the image
                lock (rawLock)
                {
                    for (int y = row; y < HEIGHT && dataRemaining > 0; y++)
                    {
                        for (int x = col; x < WIDTH && dataRemaining > 0; x++)
                        {
                            //RGB565 = 5 bits red, 6 bits green, 5 bits blue
                            int hue = data[curData];
                            int sat = data[curData+1];
                            int val = 0xC0;
                            double h = hue * 2.0;
                            double s = sat / 255.0;
                            double v = val / 255.0;
                            //convert to RGB (from Wikipedia) 
                            int h_cur = Convert.ToInt32(Math.Floor(h / 60.0) % 6);
                            double f = h / 60 - Math.Floor(h / 60.0);
                            double p = v * (1 - s);
                            double q = v * (1 - f * s);
                            double t = v * (1 - (1 - f) * s);
                            double red = 0.0;
                            double green = 0.0;
                            double blue = 0.0;
                            switch (h_cur)
                            {
                                case 0:
                                    red = v;
                                    green = t;
                                    blue = p;
                                    break;
                                case 1:
                                    red = q;
                                    green = v;
                                    blue = p;
                                    break;
                                case 2:
                                    red = p;
                                    green = v;
                                    blue = t;
                                    break;
                                case 3:
                                    red = p;
                                    green = q;
                                    blue = v;
                                    break;
                                case 4:
                                    red = t;
                                    green = p;
                                    blue = v;
                                    break;
                                case 5:
                                    red = v;
                                    green = p;
                                    blue = q;
                                    break;
                            }
                            red *= 255.0;
                            green *= 255.0;
                            blue *= 255.0;
                            newImagePart.SetPixel(x, y,
                                Color.FromArgb(Convert.ToInt32(red),
                                               Convert.ToInt32(green),
                                               Convert.ToInt32(blue)));
                            imageRaw[y * WIDTH + x] = Convert.ToInt32(red) << (16) |
                                                      Convert.ToInt32(green) << (8) |
                                                      Convert.ToInt32(blue);
                            dataRemaining--;
                            curData += 2;//2 bytes per pixel
                        }
                    }
                }
                //record that we received this packet
                imageSegmentsComplete[row * (WIDTH / PIXELS_PER_TRANS) + col / PIXELS_PER_TRANS] = true;
                painter.DrawImage(newImagePart, new Rectangle(0, 0, WIDTH, HEIGHT));

            }

            //now invalidate the region
            Invalidate();
            if (fillingInImage) //request the next section from the car
            {
                imageCleanupRequest(); //request missing packets from car
            }
        }

        public void AddBINARYData(int col, int row, byte[] data, int offset)
        {
            if (!rawImageInitialized || !(currentImageType == SerialUtils.ImageTypes.SEGMENTED_1 || currentImageType == SerialUtils.ImageTypes.SEGMENTED_2))
            {
                TheKnack.racer.writeToTruckMessageBox("[GUI] missed image header packet(BINARY)", true);
                newCapture(640, 480, SerialUtils.ImageTypes.SEGMENTED_1);//we haven't received anything before
            }
            if (row >= HEIGHT || row < 0 || col >= WIDTH || col < 0)
            {
                return;
            }
            receivedData = true;
            //transmissionTimeoutTimer.Start(); //in case image data is randomly received, make sure we get all of it

            lock (thisLock) //imageData needs to be locked due to multi-threaded use of it
            {
                // create an object that will do the drawing operations
                int dataRemaining = (data.Length - offset) * 8; //there are 8 pixels per byte
                int curData = offset;
                // draw the received data on the image
                lock (rawLock)
                {
                    for (int y = row; y < HEIGHT && dataRemaining > 0; y++)
                    {
                        for (int x = col; x < WIDTH && dataRemaining > 0; x++)
                        {
                            int whichBit = 7 - (x % 8);
                            int val = (int)(data[curData] >> whichBit & 0x1);
                            if (val > 0)
                            {
                                newImagePart.SetPixel(x, y,
                                    Color.White);
                                imageRaw[y * WIDTH + x] = 1;
                            }
                            else
                            {
                                newImagePart.SetPixel(x, y,
                                   Color.Black);
                                imageRaw[y * WIDTH + x] = 0;
                            }
                            dataRemaining--;
                            if (x % 8 == 7)
                            {
                                curData++;//3 bytes per pixel
                            }
                        }
                    }
                }
                //record that we received this packet
                imageSegmentsComplete[row * (WIDTH / PIXELS_PER_TRANS) + col / PIXELS_PER_TRANS] = true;
                painter.DrawImage(newImagePart, new Rectangle(0, 0, WIDTH, HEIGHT));

            }

            //now invalidate the region
            Invalidate();
            if (fillingInImage) //request the next section from the car
            {
                imageCleanupRequest(); //request missing packets from car
            }
        }

        public void AddGREYSCALEData(int col, int row, byte[] data, int offset)
        {
            if (!rawImageInitialized || !(currentImageType == SerialUtils.ImageTypes.SEGMENTED8_1 || currentImageType == SerialUtils.ImageTypes.SEGMENTED8_2))
            {
                TheKnack.racer.writeToTruckMessageBox("[GUI] missed image header packet(GREYSCALE)", true);
                newCapture(640, 480, SerialUtils.ImageTypes.SEGMENTED8_1);//we haven't received anything before
            }
            if (row >= HEIGHT || row < 0 || col >= WIDTH || col < 0)
            {
                return;
            }
            receivedData = true;
            //transmissionTimeoutTimer.Start(); //in case image data is randomly received, make sure we get all of it

            lock (thisLock) //imageData needs to be locked due to multi-threaded use of it
            {
                // create an object that will do the drawing operations
                int dataRemaining = (data.Length - offset); //there is 1 byte per pixel
                int curData = offset;
                // draw the received data on the image
                lock (rawLock)
                {
                    for (int y = row; y < HEIGHT && dataRemaining > 0; y++)
                    {
                        for (int x = col; x < WIDTH && dataRemaining > 0; x++)
                        {
                            int val = data[curData];
                            newImagePart.SetPixel(x, y,
                                Color.FromArgb(0, val, val));
                            imageRaw[y * WIDTH + x] = val;
                            dataRemaining--;
                            curData++;
                        }
                    }
                }
                //record that we received this packet
                imageSegmentsComplete[row * (WIDTH / PIXELS_PER_TRANS) + col / PIXELS_PER_TRANS] = true;
                painter.DrawImage(newImagePart, new Rectangle(0, 0, WIDTH, HEIGHT));

            }

            //now invalidate the region
            Invalidate();
            if (fillingInImage) //request the next section from the car
            {
                imageCleanupRequest(); //request missing packets from car
            }
        }


        // if it's been over 1 second since receiving data, fill in the image
        // NOTE: I COULDN"T GET THIS TO WORK!!
        private void transmissionTimeoutTimer_Tick(object sender, EventArgs e)
        {
            if (!rawImageInitialized) return;
            Console.WriteLine("timer");
            if (!receivedData)
            {
                //we will assume that it finished transmitting
                //so clean up the image
                fillingInImage = true;
                imageCleanupRequest();
                //transmissionTimeoutTimer.Stop();
            }
            else
            {
                receivedData = false;
                transmissionTimeoutTimer.Start();
            }
            
        }

        private void imageCleanupRequest()
        {
            if (!rawImageInitialized) return;
            bool stillNeedToFillIn = false;
            //imageSegmentsComplete is used for correcting missing packets
            for (int i = 0; i < imageSegmentsComplete.Length; i++)
            {
                if (!imageSegmentsComplete[i]) //the image data is missing so request it
                {
                    stillNeedToFillIn = true;
                    int resendRow = i / TRANS_PER_ROW;
                    int resendCol = (i % TRANS_PER_ROW) * PIXELS_PER_TRANS;
                    SerialHeader header = new SerialHeader();
                    header.Type = (byte)SerialUtils.TransmissionType.IMAGE;
                    header.Subtype = (byte)((byte)SerialUtils.ImageSubtype.RETRANSMIT_SUB_FRAME << 4 | (byte)currentImageType);
                    TheKnack.racer.transmitIntData(header, (resendRow << 16 | resendCol));
                    return;
                }
            }
            if (!stillNeedToFillIn)
            {
                fillingInImage = false;
                transmissionTimeoutTimer.Stop();
            }
        }

        private string ConvertInt32ToHexString(Int32 i)
        {
            byte[] int32Bytes;
            int32Bytes = BitConverter.GetBytes(i);
            return String.Format("{0}{1}{2}{3}",
                padString(int32Bytes[3].ToString("X")),
                padString(int32Bytes[2].ToString("X")),
                padString(int32Bytes[1].ToString("X")),
                padString(int32Bytes[0].ToString("X")));
        }
        private string ConvertInt24ToHexString(Int32 i)
        {
            byte[] int32Bytes;
            int32Bytes = BitConverter.GetBytes(i);
            return String.Format("{0}{1}{2}",
                padString(int32Bytes[2].ToString("X")),
                padString(int32Bytes[1].ToString("X")),
                padString(int32Bytes[0].ToString("X")));
        }

        private string ConvertInt16ToHexString(Int16 i)
        {
            byte[] int32Bytes;
            int32Bytes = BitConverter.GetBytes(i);
            return String.Format("{0}{1}",
                padString(int32Bytes[1].ToString("X")),
                padString(int32Bytes[0].ToString("X")));
        }

        private string ConvertByteToHexString(byte b)
        {
            return padString(String.Format("{0}", b.ToString("X")));
        }

        private string padString(string s)
        {
            while (s.Length < 2) s = "0" + s;
            while (s.Length < 3) s = s + " ";
            return s;
        }

        private string ConvertByteToBinaryString(long input)
        {
            string result = null;
            string bitstring = null;
            for (int i = 23; i >= 0; i--)
            {
                if ((i < 19 && i > 15) || (i < 10 && i > 7) || (i < 3)) continue;
                if (((input >> i) & 1) == 1) result = "1";
                else result = "0";
                bitstring += result;
                if ((i > 0) && (i == 16 || i == 8))
                {
                    bitstring += " ";
                }
            }
            return bitstring;
        }

    }
}
