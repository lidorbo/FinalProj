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
        private int packetIndex = 0;
        private ulong numOfPackets = 0;
        int currentPacket = 0;
        object senderProgress;
        List<string> keyStr = null;
        Dictionary<string, List<string>> scanResults;

        public WireSharkParse(string iniPath, string wireSharkPath, List<FieldStructure> fieldsList, MainScreen mainScreen)
        {
            this.iniPath = iniPath;
            this.wireSharkPath = wireSharkPath;
            this.fieldsList = fieldsList;
            this.mainScreen = mainScreen;
            CalcNumberOfPackets();
            keyStr = new List<string>();
            InitScanResults();
            CreateIniFile();

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += ParseWireShark;
            worker.ProgressChanged += worker_ProgressChanged;
            worker.RunWorkerAsync();
     
        }

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

        private void worker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() => mainScreen.scanProgressBar.Value = (100 * e.ProgressPercentage) / (int)numOfPackets));
        }

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
                mainScreen.sysNotificationsLV.Items.Add("Number of packets: " + numOfPackets);

                wireSharkFile.Close();
            }
            catch (Exception e)
            {
                mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Error " + e.Message);
                return;
            }
        }


        public void CreateIniFile()
        {
            try
            {
                iniFile = new FileStream(iniPath + "\\iniFile.txt", FileMode.CreateNew);
            }
            catch (Exception e)
            {
                mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Error " + e.Message);
            }
        }

        public void WriteToIniFile()
        {
            int i = 0;
            string str = "";
            foreach (var b in scanResults)
            {
                str += "//" + b.Value[0] + Environment.NewLine + Environment.NewLine;
                str += "[" + b.Key + "]" + Environment.NewLine + Environment.NewLine;
                foreach (var s in b.Value)
                {
                    str += s + Environment.NewLine;
                    i++;
                }
            }
            byte[] data = Encoding.Unicode.GetBytes(str);

            iniFile.Write(data, 0, data.Length);
            iniFile.Flush();
            iniFile.Close();

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
                wireSharkFile.OnPacketArrival +=
                                          new PacketArrivalEventHandler(OnPacketArrival);
                mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => mainScreen.scanProgressBar.Visibility = System.Windows.Visibility.Visible));

                mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                 new Action(() => mainScreen.progressValue.Visibility = System.Windows.Visibility.Visible));

                mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(() => mainScreen.sysNotificationsLV.Items.Add("Scanning")));

                wireSharkFile.Capture();
                WriteToIniFile();
                mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(() => mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Scan completed")));

                wireSharkFile.Close();

                mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                 new Action(() => mainScreen.scanProgressBar.Visibility = System.Windows.Visibility.Hidden));

                mainScreen.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                 new Action(() => mainScreen.progressValue.Visibility = System.Windows.Visibility.Hidden));
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
            string str = null;
            currentPacket++;
            if (e.Packet.LinkLayerType == PacketDotNet.LinkLayers.Ethernet)
            {
                var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                var ethernetPacket = (PacketDotNet.EthernetPacket)packet;
                int loopCounter = 0;
                int packetSize = ethernetPacket.PayloadPacket.PayloadPacket.PayloadData.Length;
                int fieldCount = 0, fieldSize = 0;
                byte[] packetData = ethernetPacket.PayloadPacket.PayloadPacket.PayloadData;
                byte[] currentField;
                int tempIndex = 0;

                if (FiltersData.packetSizeFrom != -1 && FiltersData.packetSizeTo != -1)
                {
                    if(packetSize < FiltersData.portFrom || packetSize > FiltersData.portTo)
                    {
                        return;
                    }
                }

                for (int i = 0; i < packetSize; i += fieldSize)
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
                        ConvertBytesToNumber(currentField, fieldsList[loopCounter].type, fieldsList[loopCounter].fieldName);
                        
                    }
                    else
                    {
                        break;
                    }

                    loopCounter++;

                }           
            }
        }

        public void print()
        {
            string str = "";

            foreach(var x in scanResults)
            {
                str += x.Key + "\n";
                foreach(var s in x.Value)
                {
                    str += s + "\n";
                }

                str += "\n\n\n";
            }
            File.WriteAllText("scan1.txt", str);
        }

        private void ConvertBytesToNumber(byte[] data, string type, string fieldName)
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

           scanResults[fieldName].Add(value);


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

            if(FiltersData.isUDP)
            {
                temp = "udp";           
            }
            else
            {
                temp = "tcp";
            }
            filter.Add(temp);

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
