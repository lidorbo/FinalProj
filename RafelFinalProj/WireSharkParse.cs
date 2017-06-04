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
            CreateIniFile();

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
                                          new PacketArrivalEventHandler(GetInitTime);
                getFirstPacket.Capture();
                getFirstPacket.Close();
            }
            catch (Exception e)
            {
                mainScreen.WriteNotification("Error " + e.Message);
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
                                          new PacketArrivalEventHandler(GetInitTime);           
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
                mainScreen.WriteNotification("Error " + e.Message);
                return;
            }
        }

        /// <summary>
        /// Will create the ini file. the name will be the name of the wireshark file and the date of the scan.
        /// If a file with the same name already exists, a number will be added at the end
        /// </summary>
        public void CreateIniFile()
        {
            string iniFileName = iniPath + "\\" + Path.GetFileNameWithoutExtension(wireSharkPath) + "_" + DateTime.Today.ToString("dd-MM-yyyy");
            try
            {
                iniFile = new FileStream(iniFileName +".ini", FileMode.CreateNew);
            }
            catch(IOException ex)
            {
                int count = 0;

                while (File.Exists(iniFileName + "_" + count + ".ini"))
                {
                    count++;
                }

                iniFile = new FileStream(iniFileName + "_" + count + ".ini", FileMode.CreateNew);
            }
            catch (Exception e)
            {
                mainScreen.WriteNotification(": Error " + e.Message);
            }
            finally
            {
                mainScreen.WriteNotification("ini File have been created");

            }

        }

        /// <summary>
        /// Will write the results of the scan into the ini and log file in accordance to their format.
        /// </summary>
        public void WriteToFiles()
        {
            string[] temp;
            int totalNumOfValues = 0;
            //mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
            //     new Action(() => mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ":  Writing results to files")));

            mainScreen.WriteNotification("Writing results to files");

            mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(() => mainScreen.scanProgressBar.Value = 0));

            foreach(var packet in scanResults)
            {
                totalNumOfValues += packet.Value.Count;
            }
            numOfPackets = (ulong)totalNumOfValues;
            totalNumOfValues = 0;

            foreach (var packet in scanResults)
            {
                WriteToIniFile("//" + packet.Value[0] + Environment.NewLine);
                WriteToIniFile("[" + packet.Key + "]" + Environment.NewLine);
                logWriter.WriteToLog("[" + packet.Key + "]" + Environment.NewLine);

                for (int i = 0; i < packet.Value.Count; i++)
                {
                    totalNumOfValues++;
                    if(i == 0)
                    {
                        WriteToIniFile("Number of entries - " + (packet.Value.Count - 1) + Environment.NewLine);
                    }
                    else
                    {
                        temp = packet.Value[i].Split(';');
                        WriteToIniFile(temp[0] + Environment.NewLine);
                        logWriter.WriteToLog(temp[1] + Environment.NewLine);
                    }

                    mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
             new Action(() => mainScreen.scanProgressBar.Value = (100 * totalNumOfValues) / (int)numOfPackets));

                }

                WriteToIniFile(Environment.NewLine + Environment.NewLine);
                logWriter.WriteToLog(Environment.NewLine + Environment.NewLine);
            }

            logWriter.Finish();
            iniFile.Flush();
            iniFile.Close();
            //mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
            //     new Action(() => mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ":  Writing complete, All done!")));

            mainScreen.WriteNotification("Writing complete, All done !");
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

        public void ParseWireShark(object senderProgress, DoWorkEventArgs e)
        {
            ICaptureDevice wireSharkFile;
            this.senderProgress = senderProgress;
            try
            {
                wireSharkFile = new CaptureFileReaderDevice(wireSharkPath);
                wireSharkFile.Open();
                wireSharkFile.Filter = BuildFilterString();
                workerEvent = e;
                wireSharkFile.OnPacketArrival += 
                                          new PacketArrivalEventHandler(OnPacketArrival);              

                mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => mainScreen.scanProgressBar.Visibility = System.Windows.Visibility.Visible));

                mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                 new Action(() => mainScreen.progressValue.Visibility = System.Windows.Visibility.Visible));

                mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() => mainScreen.sysNotificationsLV.Items.Add("Scanning")));

                wireSharkFile.Capture();
                mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(() => mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Scan completed")));

                wireSharkFile.Close();
                WriteToFiles();

            }
            catch (Exception ex)
            {
                mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                   new Action(() => mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Error " + ex.Message)));
                return;
            }


        }

        private void totalNumberOfPackets(object sender, CaptureEventArgs e)
        {
            ++numOfPackets;
        }

       
        /// <summary>
        /// Prints the source and dest MAC addresses of each received Ethernet frame
        /// </summary>
        private void OnPacketArrival(object sender, CaptureEventArgs e)
        {
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
                string currentTimeTag = null;
                long timeDelta;
                double second;

                if (FiltersData.packetSizeFrom != -1 && FiltersData.packetSizeTo != -1)
                {
                    if(packetSize < FiltersData.packetSizeFrom || packetSize > FiltersData.packetSizeTo)
                    {
                        return;
                    }
                }

                for (int i = FiltersData.offset; i < packetSize; i += fieldSize)
                {
                    if (loopCounter > fieldsList.Count - 1)
                    {
                        break;
                    }

                    fieldSize = fieldsList[loopCounter].size;
                    currentField = new byte[fieldSize];
                    tempIndex = 0;

                    if ((i + fieldSize) < packetData.Length)
                    {
                        for (int j = i; j < (i + fieldSize); j++)
                        {
                            currentField[tempIndex] = packetData[j];
                            tempIndex++;
                        }

                        currentTimeTag = e.Packet.Timeval.Seconds + "." + e.Packet.Timeval.MicroSeconds;
                        timeDelta = e.Packet.Timeval.Date.Ticks - firstPacketTime;
                        second = TimeSpan.FromTicks(timeDelta).TotalSeconds;
                        ConvertBytesToNumber(currentField, fieldsList[loopCounter].type, fieldsList[loopCounter].fieldName, second.ToString());
                        
                    }
                    else
                    {
                        break;
                    }

                    loopCounter++;
                    mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                            new Action(() => mainScreen.scanProgressBar.Value = (100 * currentPacket) / (int)numOfPackets));
                }           
            }
        }

        private void ConvertBytesToNumber(byte[] data, string type, string fieldName, string timeTag)
        {
            if(FiltersData.isLitleEndian)
            {
                Array.Reverse(data);
            }

            string value = string.Empty;

            switch(type)
            {
                case ConstValues.CHAR:
                    value = data[0].ToString();
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
                    value = Convert.ToChar(data).ToString();
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

           scanResults[fieldName].Add(value + ";" + timeTag);

        }

        public string BuildFilterString()
        {
            string filterStr = "";
            string temp = "";
            List<string> filter = new List<string>();

            if(FiltersData.ipSrc != string.Empty)
            {
                temp = "src " + FiltersData.ipSrc;
                filter.Add(temp);
            }

            if (FiltersData.ipDest != string.Empty)
            {
                temp = "dst " + FiltersData.ipDest;
                filter.Add(temp);
            }

            if(FiltersData.protocol != "all")
            {
                temp = FiltersData.protocol + " ";
                filter.Add(temp);
            }
            
            if (FiltersData.portTo != 0 && FiltersData.portTo != 0)
            {
                temp = "portrange " + FiltersData.portFrom + "-" + FiltersData.portTo;
                filter.Add(temp);
            }

            for (int i = 0; i < filter.Count; i++ )
            {
                if(i != 0 && i != filter.Count -1)
                {
                    filterStr += filter[i] + " and";
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
