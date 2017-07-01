using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RafelFinalProj
{
    class WireSharkParse
    {
        private string iniPath;
        private string wireSharkPath;
        private List<FieldStructure> fieldsList;
        private FileStream iniFile;
        private MainScreen mainScreen;
        private ulong numOfPackets = 0;
        int currentPacket = 0;
        object senderProgress;
        long firstPacketTime;
        List<string> keyStr = null;
        Dictionary<string, List<string>> scanResults;
        private ICaptureDevice getFirstPacket;
        LogWriter logWriter;
        public BackgroundWorker worker { set; get; }
        DoWorkEventArgs workerEvent;
        private int xmlFieldsSize = 0;

        public WireSharkParse(string iniPath, string wireSharkPath, List<FieldStructure> fieldsList, MainScreen mainScreen, LogWriter logWriter)
        {
            this.iniPath = iniPath;
            this.wireSharkPath = wireSharkPath;
            this.fieldsList = fieldsList;
            this.mainScreen = mainScreen;
            this.logWriter = logWriter;
            GetFirstPacketTime();
            CalcNumberOfPackets();
            keyStr = new List<string>();
            InitScanResults();

            worker = new BackgroundWorker();
            worker.WorkerSupportsCancellation = true;
            worker.WorkerReportsProgress = true;
            worker.DoWork += ParseWireShark;
            worker.RunWorkerAsync();
     
        }

        /// <summary>
        /// Sets the dictionary for the scan. Each field from the XML will be added as key and will have a corresponding list
        /// </summary>
        private void InitScanResults()
        {
            scanResults = new Dictionary<string, List<string>>();       
            foreach(var f in fieldsList)
            {
                List<string> str = new List<string>();
                str.Add(f.type);
                scanResults.Add(f.fieldName, str);
                xmlFieldsSize += f.size;
            }
        }

        /// <summary>
        /// Will get the TICKS time of the first packet in order to determine the time tag of each packet afterwards
        /// </summary>
        private void GetFirstPacketTime()
        {

            try
            {
                getFirstPacket = new CaptureFileReaderDevice(wireSharkPath);
                getFirstPacket.Open();
                getFirstPacket.OnPacketArrival +=
                                          new PacketArrivalEventHandler(GetInitTime);//sets the event to capture the first packet
                getFirstPacket.Capture();
                getFirstPacket.Close();
            }
            catch (Exception e)
            {
                mainScreen.WriteNotification(ConstValues.WIRESHARK_PARSE_ERROR + e.Message);
                return;
            }
        }

        /// <summary>
        /// This event will return the TICKS time of the first packet
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GetInitTime(object sender, CaptureEventArgs e)
        {
             firstPacketTime = e.Packet.Timeval.Date.Ticks;
             getFirstPacket.OnPacketArrival -=
                                          new PacketArrivalEventHandler(GetInitTime);//canceling the event after we got the time tick         
        }

        /// <summary>
        /// Will calculate the amount of packets in the wireshark file in order to set the progress bar
        /// </summary>
        private void CalcNumberOfPackets()
        {
            ICaptureDevice wireSharkFile;

            try
            {
                wireSharkFile = new CaptureFileReaderDevice(wireSharkPath);
                wireSharkFile.Open();
                wireSharkFile.Filter = BuildFilterString();
                wireSharkFile.OnPacketArrival +=
                                          new PacketArrivalEventHandler(totalNumberOfPackets);
                wireSharkFile.Capture();
                wireSharkFile.Close();
            }
            catch (Exception e)
            {
                mainScreen.WriteNotification(ConstValues.WIRESHARK_PARSE_ERROR + e.Message);
                return;
            }
        }

        /// <summary>
        /// Will create the ini file. The name will be: (Wireshark file)_(date).ini
        /// If a file with the same name already exists, a number will be added at the end
        /// </summary>
        public void CreateIniFile()
        {
            string iniFileName = iniPath + "\\" + Path.GetFileNameWithoutExtension(wireSharkPath) + ConstValues.FILENAME_SEPERATOR + DateTime.Today.ToString(ConstValues.DATE_FORMAT);
            try
            {
                iniFile = new FileStream(iniFileName + ConstValues.RESULTS_FILE_EXTENTION , FileMode.CreateNew);
            }
            catch(IOException ex)
            {
                int count = 0;

                while (File.Exists(iniFileName + ConstValues.FILENAME_SEPERATOR + count + ConstValues.RESULTS_FILE_EXTENTION))
                {
                    count++;
                }

                iniFile = new FileStream(iniFileName + ConstValues.FILENAME_SEPERATOR + count + ConstValues.RESULTS_FILE_EXTENTION, FileMode.CreateNew);
            }
            catch (Exception e)
            {
                mainScreen.WriteNotification(ConstValues.INI_FILE_ERROR + e.Message);
            }
            finally
            {
                mainScreen.WriteNotification(ConstValues.INI_FILE_CREATED);
            }

        }

        /// <summary>
        /// Will write the results of the scan into the ini and log file in accordance to their format.
        /// </summary>
        public void WriteToFiles()
        {
            string[] temp = {String.Empty, String.Empty};
            int totalNumOfValues = 0;
            int numOfEntries = scanResults[fieldsList[0].fieldName].Count;

            CreateIniFile();
            mainScreen.WriteNotification(ConstValues.WRITING_TO_FILES);

            //resets the progress bar
            mainScreen.ShowAndSetProgressBar();
         
            foreach(var packet in scanResults)
            {
                totalNumOfValues += packet.Value.Count;
            }

            numOfPackets = (ulong)totalNumOfValues;
            totalNumOfValues = 0;
            WriteToIniFile("[Sum Of Packets]" + Environment.NewLine + (numOfEntries - 1) + Environment.NewLine + Environment.NewLine);
            logWriter.WriteToLog("Time tags of packets:" + Environment.NewLine);

            int numOfFields = scanResults.Count;

            for (int i = 1; i < numOfEntries; i++)
            {
                totalNumOfValues++;
                for (int j = 0; j < numOfFields; j++)
                {
                    //temp[0] = value of the entry, temp[1] = time tag of the packet
                    temp = scanResults[fieldsList[j].fieldName][i].Split(ConstValues.VALUE_TIMETAG_SEPERATOR);
                    WriteToIniFile("[" + fieldsList[j].fieldName + "_" + i + "]" + Environment.NewLine + temp[0] + Environment.NewLine);
                }

                WriteToIniFile(Environment.NewLine);
                logWriter.WriteToLog(temp[1] + Environment.NewLine);
                mainScreen.ProgressBarReportProgress(totalNumOfValues, numOfPackets);
            }

            logWriter.Finish();
            iniFile.Flush();
            iniFile.Close();

            mainScreen.WriteNotification(ConstValues.WRITING_COMPLETE);
        }

        /// <summary>
        /// This method will write to the ini file
        /// </summary>
        /// <param name="str">The string to write</param>
        public void WriteToIniFile(string str)
        {
            byte[] data = Encoding.Unicode.GetBytes(str);
            iniFile.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Will start to parse the Wireshark file
        /// </summary>
        /// <param name="senderProgress"></param>
        /// <param name="e"></param>
        public void ParseWireShark(object senderProgress, DoWorkEventArgs e)
        {
            bool gotResults = false;
            ICaptureDevice wireSharkFile;
            this.senderProgress = senderProgress;
            try
            {
                wireSharkFile = new CaptureFileReaderDevice(wireSharkPath);
                wireSharkFile.Open();
                wireSharkFile.Filter = BuildFilterString();//sets the filters
                workerEvent = e;
                wireSharkFile.OnPacketArrival += 
                                          new PacketArrivalEventHandler(OnPacketArrival);

                mainScreen.ShowAndSetProgressBar();
                mainScreen.WriteNotification(ConstValues.SCANNING_WIRESHARK_FILE);
                wireSharkFile.Capture();
                mainScreen.WriteNotification(ConstValues.SCANNING_WIRESHARK_COMPLETED);
                wireSharkFile.Close();

                //checks if there are results in the scan
                foreach (var field in scanResults)
                {
                    if(field.Value.Count > 1)
                    {
                        gotResults = true;
                        break;
                    }
                }

                if (gotResults)
                {
                    WriteToFiles();
                }
                else
                {
                    mainScreen.WriteNotification(ConstValues.NO_RESULTS);
                    logWriter.WriteToLog(ConstValues.NO_RESULTS);
                    logWriter.Finish();
                }
              
                mainScreen.HideProgressBar();

            }
            catch (Exception ex)
            {
                mainScreen.WriteNotification(ConstValues.WIRESHARK_PARSE_ERROR + ex.Message);
                return;
            }


        }

        /// <summary>
        /// Sets the total number of packets. Will be used to report progress to the progress bar
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void totalNumberOfPackets(object sender, CaptureEventArgs e)
        {
            ++numOfPackets;
        }

       
        /// <summary>
        /// Scans a packet and parse it's results
        /// </summary>
        private void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            try
            {
                //checks if the scan was cancelled
                if (this.worker.CancellationPending)
                {
                    workerEvent.Cancel = true;
                    return;
                }

                currentPacket++;
                if (e.Packet.LinkLayerType == PacketDotNet.LinkLayers.Ethernet)
                {
                    var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                    var ethernetPacket = (PacketDotNet.EthernetPacket)packet;
                    int loopCounter = 0;
                    int packetSize = ethernetPacket.PayloadPacket.PayloadPacket.PayloadData.Length;
                    int fieldSize = 0;
                    byte[] packetData = ethernetPacket.PayloadPacket.PayloadPacket.PayloadData;
                    byte[] currentField;
                    int tempIndex = 0;
                    long timeDelta;
                    double ticks;
                    timeDelta = e.Packet.Timeval.Date.Ticks - firstPacketTime;
                    ticks = TimeSpan.FromTicks(timeDelta).TotalSeconds;

                    //checks if the packet is smaller than the minimum size
                    if (packetSize < xmlFieldsSize)
                    {
                        return;
                    }

                    //checks the size filter
                    if (FiltersData.packetSizeFrom != -1 && FiltersData.packetSizeTo != -1)
                    {
                        if (packetSize < FiltersData.packetSizeFrom || packetSize > FiltersData.packetSizeTo)
                        {
                            return;
                        }
                    }


                    //checks if the the size of the packet is bigger than the XML total size
                    if (packetSize > xmlFieldsSize)
                    {
                        mainScreen.WriteNotification(ConstValues.BIGGER_THAN_XML + ticks);
                    }

                    //copies the current data according to the field's size
                    for (int i = FiltersData.offset; i < packetSize; i += fieldSize)
                    {
                        //checks if there are any other fields to scan
                        if (loopCounter > fieldsList.Count - 1)
                        {
                            break;
                        }

                        fieldSize = fieldsList[loopCounter].size;
                        currentField = new byte[fieldSize];
                        tempIndex = 0;

                        if ((i + fieldSize) <= packetData.Length)
                        {
                            for (int j = i; j < (i + fieldSize); j++)
                            {
                                currentField[tempIndex] = packetData[j];
                                tempIndex++;
                            }
                     
                            ConvertBytesToNumber(currentField, fieldsList[loopCounter].type, fieldsList[loopCounter].fieldName, ticks.ToString());
                        }
                        else
                        {
                            break;
                        }

                        loopCounter++;
                        mainScreen.ProgressBarReportProgress(currentPacket, numOfPackets);
                    }
                }
            }
            catch (Exception ex)
            {
                mainScreen.WriteNotification(ConstValues.BAD_PACKET + currentPacket.ToString() + ex.Message);
            }
        }

        /// <summary>
        /// Will convert the bytes array to a value according to its type
        /// </summary>
        /// <param name="data">the byte array</param>
        /// <param name="type">the type of the field</param>
        /// <param name="fieldName">the name of the field</param>
        /// <param name="timeTag">the current timetag</param>
        private void ConvertBytesToNumber(byte[] data, string type, string fieldName, string timeTag)
        {
            //little endian should be read in reverse
            if(FiltersData.isLitleEndian)
            {
                Array.Reverse(data);
            }

            string value = string.Empty;

            switch(type)
            {
                case ConstValues.CHAR:
                    sbyte[] signed = data.Select(b => (sbyte)b).ToArray();
                    value = signed[0].ToString();
                    break;
                case ConstValues.INT32:
                   value = BitConverter.ToInt32(data, 0).ToString();
                    break;
                case ConstValues.INT64:
                    value = BitConverter.ToInt64(data, 0).ToString();
                    break;
                case ConstValues.SHORT:
                    value = BitConverter.ToInt16(data, 0).ToString();
                    break;
                case ConstValues.UCHAR:
                    value = data[0].ToString();
                    break;
                case ConstValues.UINT32:
                    value = BitConverter.ToUInt32(data, 0).ToString();
                    break;
                case ConstValues.UINT64:
                    value = BitConverter.ToUInt64(data, 0).ToString();
                    break;
                case ConstValues.USHORT:
                   value = BitConverter.ToUInt16(data, 0).ToString();
                    break;
                    
            }

           scanResults[fieldName].Add(value + ConstValues.VALUE_TIMETAG_SEPERATOR + timeTag);

        }

        /// <summary>
        /// Sets the filters for the scan in pcap format
        /// </summary>
        /// <returns></returns>
        public string BuildFilterString()
        {
            string filterStr = String.Empty;
            string temp = String.Empty;
            List<string> filter = new List<string>();

            if(FiltersData.ipSrc != string.Empty)
            {
                temp = ConstValues.PCAP_SRC + FiltersData.ipSrc;
                filter.Add(temp);
            }

            if (FiltersData.ipDest != string.Empty)
            {
                temp = ConstValues.PCAP_DST + FiltersData.ipDest;
                filter.Add(temp);
            }

            if(FiltersData.protocol != ConstValues.ALL_STR)
            {
                temp = FiltersData.protocol + ConstValues.STRING_WHITESPACE;
                filter.Add(temp);
            }
            
            if (FiltersData.portTo != 0 && FiltersData.portTo != 0)
            {
                temp = ConstValues.PCAP_PORT_RANGE + FiltersData.portFrom + ConstValues.PCAP_PORT_RANGE_SEPERATOR + FiltersData.portTo;
                filter.Add(temp);
            }

            for (int i = 0; i < filter.Count; i++ )
            {
                if(i != filter.Count -1)
                {
                    filterStr += filter[i] + ConstValues.PCAP_AND;
                }

                else
                {
                    filterStr += filter[i];
                }

            }
                return filterStr;
        }

    }
}
