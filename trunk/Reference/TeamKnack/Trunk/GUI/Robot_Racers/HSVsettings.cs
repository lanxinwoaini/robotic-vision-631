using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using System.Text;

namespace Robot_Racers
{
    class HSVsettings
    {
        List<int> hsvEntries;
        int numentries;
        int greenThreshold;
        int orangeThreshold;

        public HSVsettings()
        {
            hsvEntries = new List<int>();
            numentries = 144;
            greenThreshold = TheKnack.racer.greenConvolutionThreshold;
            orangeThreshold = TheKnack.racer.orangeConvolutionThreshold;
 
        } 
        public HSVsettings(string hsvText)
        {
            hsvEntries = new List<int>();
            numentries = 144;
            greenThreshold = TheKnack.racer.greenConvolutionThreshold;
            orangeThreshold = TheKnack.racer.orangeConvolutionThreshold;
            int entrycount = 0;

            hsvText = hsvText.Trim();
            char[] splitLine = new char[] { '\n' };
            char[] splitSpace= new char[] { ' ' };
            string[] hsvLines = hsvText.Split(splitLine);

            try
            {
                for (int i = 0; i < 12; i++) //each line contains 12 integers representing the green and orange hsv ranges.
                {
                    string tempLine = hsvLines[i].Trim();
                    string[] lineEntries = hsvLines[i].Split(splitSpace);
                    for (int k = 0; k < 12; k++) //parse each line of hsv entries.
                    {
                        int entry = (int)int.Parse(lineEntries[k]);
                        hsvEntries.Add(entry);
                        entrycount++;
                    }
                }
                if(numentries != entrycount)
                {
                    Console.WriteLine("Invalid number of  entries");
                    MessageBox.Show("Invalid number of dynamic HSV range entries!!!");
                }
                
               
            }
            catch (FormatException e) { Console.WriteLine(e.Message); }

        }
        public bool hsvRangesValid()
        {
            if (hsvEntries.Count == numentries)
            {
                int[] entries = new int[numentries];
                hsvEntries.CopyTo(entries, 0);
                for (int i = 0; i < numentries; i++)
                {
                    int entry = entries[i];
                    if (i % 6 == 0 || i % 6 == 1)
                    {
                        //this is a hue entry, it should be between 0 and 179.
                        if (entry < 0 || entry > 179)
                        {
                            MessageBox.Show("Hue range entry is out of range 0-179.");
                            return false;
                        }
                    }
                    else
                    {
                        if (entry < 0 || entry > 255)
                        {
                            MessageBox.Show("Saturation or value range entry is out of range 0-255.");
                            return false;
                        }

                    }
                }
                return true;

            }
            else
            {
                return false;  
            }
        }
        public void getArray(int[] returnArray)
        {
            int[] temparray = new int[144];
            hsvEntries.CopyTo(temparray);

            int index = 0;

            for (int i = 0; i < 12; i++)
            {
                for (int k = 0; k < 12; k++)
                {
                    returnArray[index++] = temparray[i * 12 + k];
                }
                returnArray[index++] = greenThreshold;
                returnArray[index++] = orangeThreshold;
            }
        }
        public void getLine(int[] returnArray, int line)
        {
            int[] temparray = new int[144];
            hsvEntries.CopyTo(temparray);

            int index = 0;

            
            for (int k = 0; k < 12; k++)
            {
                returnArray[index++] = temparray[12*line + k];  //get the designated line
            }
            returnArray[index++] = greenThreshold;
            returnArray[index++] = orangeThreshold;
           

        }


    }
}
