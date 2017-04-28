using SharpPcap;
using SharpPcap.LibPcap;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        public WireSharkParse(string iniPath, string wireSharkPath, Dictionary<string, int> xmlStructure, MainScreen mainScreen)
        {
            this.iniPath = iniPath;
            this.wireSharkPath = wireSharkPath;
            this.xmlStructure = xmlStructure;
            this.mainScreen = mainScreen;

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

        public void ParseWireShark()
        {
            ICaptureDevice wireSharkFile;

            try
            {
                wireSharkFile = new CaptureFileReaderDevice(wireSharkPath);       
                wireSharkFile.Open();
                wireSharkFile.Filter = BuildFilterString();
                wireSharkFile.OnPacketArrival +=
                                          new PacketArrivalEventHandler(OnPacketArrival);

                wireSharkFile.Capture();
                wireSharkFile.Close();
            }
            catch (Exception e)
            {
                mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Error " + e.Message);
                return;
            }


        }


        /// <summary>
        /// Prints the source and dest MAC addresses of each received Ethernet frame
        /// </summary>
        private void OnPacketArrival(object sender, CaptureEventArgs e)
        {
            string str = null;
            if (e.Packet.LinkLayerType == PacketDotNet.LinkLayers.Ethernet && packetIndex < 20)
            {
                var packet = PacketDotNet.Packet.ParsePacket(e.Packet.LinkLayerType, e.Packet.Data);
                var ethernetPacket = (PacketDotNet.EthernetPacket)packet;


                Console.WriteLine("{0}", ethernetPacket.PayloadPacket.PrintHex());
                //Console.WriteLine("{0}", ethernetPacket.PayloadPacket.Bytes.Length.ToString());
                int i = 0;
                Console.WriteLine("Length = " + ethernetPacket.Bytes.Length.ToString() + " ");
                string temp = "";
                foreach (byte b in ethernetPacket.Bytes)
                {
                    temp = b.ToString("x2") + " ";
                    i++;
                    if (i % 16 == 0)
                        temp += Environment.NewLine;

                }

                str += ethernetPacket.PayloadPacket.ToString() + "\n";
                packetIndex++;
            }
     

        }

        public string BuildFilterString()
        {
            string filterStr = "";
            string temp = "";
            List<string> filter = new List<string>();

            if(FiltersData.ipSrc != string.Empty && FiltersData.ipDest != string.Empty)
            {
                temp = "src " + FiltersData.ipSrc + " and dst " + FiltersData.ipDest;
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
