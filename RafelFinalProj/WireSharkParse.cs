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
        private Dictionary<string, int> xmlStructure;
        private FileStream iniFile;
        private MainScreen mainScreen;
        private int packetIndex = 0;
        string temp = "";
        private ulong numOfPackets = 0;
        int currentPacket = 0;
        object senderProgress;

        public WireSharkParse(string iniPath, string wireSharkPath, Dictionary<string, int> xmlStructure, MainScreen mainScreen)
        {
            this.iniPath = iniPath;
            this.wireSharkPath = wireSharkPath;
            this.xmlStructure = xmlStructure;
            this.mainScreen = mainScreen;
            CalcNumberOfPackets();

            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += ParseWireShark;
            worker.ProgressChanged += worker_ProgressChanged;

            worker.RunWorkerAsync();

            //Thread t = new Thread(ParseWireShark);
            //t.IsBackground = true;
            //t.Start();
     
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
                iniFile = new FileStream(iniPath, FileMode.CreateNew);
            }
            catch (Exception e)
            {
                mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Error " + e.Message);
            }
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

                File.WriteAllText("scan.txt", temp);

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


                //Console.WriteLine("{0}", ethernetPacket.PayloadPacket.PrintHex());
                //Console.WriteLine("{0}", ethernetPacket.PayloadPacket.Bytes.Length.ToString());
                int i = 0;
               // mainScreen.sysNotificationsLV.Items.Add("Length = " + ethernetPacket.Bytes.Length.ToString() + " ");

                foreach (byte b in ethernetPacket.PayloadPacket.PayloadPacket.PayloadData)
                {
                    temp += b.ToString("x2") + " ";
                    i++;
                    if (i % 16 == 0)
                        temp += Environment.NewLine;
                }
                temp += Environment.NewLine;
                temp += Environment.NewLine;
                temp += Environment.NewLine;

                str += ethernetPacket.PayloadPacket.ToString() + "\n";
                packetIndex++;
                (senderProgress as BackgroundWorker).ReportProgress(currentPacket);
            }
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
