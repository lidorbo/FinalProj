using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;

namespace RafelFinalProj
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainScreen : Window
    {
        private const string WIRESHARK_LOADED = "Wireshark file loaded";
        private const string WIRESHARK_ERROR = "Wireshark file can't be loaded";
        private const string XML_LOADED = "XML file loaded";
        private const string INVALID_SAVE_LOCATION_INI = "Invalid save path for ini";
        private const string INVALID_SAVE_LOCATION_LOG = "Invalid save path for log";
        private const int MAX_PORT = 65535;

        private const string IP_MSG_ERROR = "One or more IPs are not valid";
        private const string PORT_MSG_ERROR = "Port is not valid";
        private const string PORT_RANGE_MSG_ERROR = "Port range is not valid";
        private const string PACKET_MSG_ERROR = "One or packet sizes are not valid";
        private const string PACKET_SIZE_RANGE_ERROR = "Packet size range is not valid";
        private const string OFFSET_MSG_ERROR = "Offset must be bigger than 0";
        private const string SCAN_MSG = "Scan have started";
        private const string SCAN_MSG_ERROR = "ERROR - Scan have not started!";



        public MainScreen()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Loads the Wireshark file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void wiresharkLogBrowseBTN_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog wiresharkFD = new OpenFileDialog();
            wiresharkFD.Title = "Select a Wireshark output file";
            wiresharkFD.Filter = "pcap|*.pcap";

            if (wiresharkFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                wiresharkLogTB.Text = wiresharkFD.FileName;
                sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + WIRESHARK_LOADED);
            }
            else
            {

            }

        }


        /// <summary>
        /// The method will load and parse the XML file.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void xmlLoadBTN_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog xmlFD = new OpenFileDialog();
            xmlFD.Title = "Select an XML file";
            xmlFD.Filter = "XML|*.xml";

            if (xmlFD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string statusMsg = XML_LOADED;
                try
                {
                    //checks if the XML file is valid
                    XmlDocument xmlFile = new XmlDocument();
                    xmlFile.Load(xmlFD.FileName);
                    //checks if the format is correct
                    XmlParser xmlParser = new XmlParser(this, xmlFile);
                    xmlLoadTB.Text = xmlFD.FileName;
                }
                catch (XmlException exception)
                {
                   statusMsg = exception.Message;
                }

                sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + statusMsg);
            }
            else
            {

            }
        }

        /// <summary>
        /// Picks the output location of the INI file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void iniSaveBTN_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog iniLocationFBD = new FolderBrowserDialog();
            
            if (iniLocationFBD.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(iniLocationFBD.SelectedPath))
            {
                iniSaveTB.Text = iniLocationFBD.SelectedPath;
            }
            else
            {
               sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + INVALID_SAVE_LOCATION_INI);
            }

        }

        /// <summary>
        /// Picks the location of the log file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void logSaveBTN_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog logLocationFBD = new FolderBrowserDialog();

            if (logLocationFBD.ShowDialog() == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(logLocationFBD.SelectedPath))
            {
                logSaveTB.Text = logLocationFBD.SelectedPath;
            }
            else
            {
                sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + INVALID_SAVE_LOCATION_LOG);
            }
        }

        /// <summary>
        /// Clears the notifications list view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void clearNotificationsBTN_Click(object sender, RoutedEventArgs e)
        {
            sysNotificationsLV.Items.Clear();
        }

        /// <summary>
        /// The method will start the scan, but beforehand, will check if all the input is correct
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startScanBTN_Click(object sender, RoutedEventArgs e)
        {
           bool ip = IsValidIP();
           bool port = IsPortValid();
           bool size = IsPacketSizeValid();
           bool offset = IsOffsetVaild();

           if(ip && port && size && offset)
           {
               Protocol();
               IpType();
               EndianType();
               sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + SCAN_MSG);
               WireSharkParse wp = new WireSharkParse("", wiresharkLogTB.Text, new Dictionary<string, int>(), this);

           }
           else
           {
               sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + SCAN_MSG_ERROR);
           }

        }

        /// <summary>
        /// Checks if the inserted IPs are vaild and have a correct range. Will use the methods ValidateIP and IsValidIpRange.
        /// </summary>
        /// <returns></returns>
        private bool IsValidIP()
        {
     
           bool ipFromParse = ValidateIPv4(ipFromTB.Text);
           bool ipToParse = ValidateIPv4(ipToTB.Text); 
  
           if (ipFromParse && ipToParse)
           {
               FiltersData.ipSrc = ipFromTB.Text;
               FiltersData.ipDest = ipToTB.Text;
               return true;
           }

           else if(ipFromParse && ipFromTB.Text.Length > 0)
           {
               FiltersData.ipSrc = ipFromTB.Text;
               return true;
           }

           else if(ipToParse && ipToTB.Text.Length > 0 )
           {
               FiltersData.ipDest = ipToTB.Text;
               return true;
           }
           sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + IP_MSG_ERROR);
           return false ;
        }

        private void Protocol()
        {
            if(udpRB.IsChecked.Value)
            {
                FiltersData.isUDP = true;
            }
            else
            {
                FiltersData.isUDP = false;
            }
        }

        private void IpType()
        {
            if (ipv4RB.IsChecked.Value)
            {
                FiltersData.isIpV4 = true;
            }
            else
            {
                FiltersData.isIpV4 = false;
            }
        }


        private void EndianType()
        {
            if (littleRB.IsChecked.Value)
            {
                FiltersData.isLitleEndian = true;
            }
            else
            {
                FiltersData.isLitleEndian = false;
            }
        }

        /// <summary>
        /// Checks if the IP is a valid IPv4 address
        /// </summary>
        /// <param name="ipString">The ip address</param>
        /// <returns></returns>
        public bool ValidateIPv4(string ipString)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                return true;
            }

            string[] splitValues = ipString.Split('.');
            if (splitValues.Length != 4)
            {
                return false;
            }

            byte tempForParsing;

            return splitValues.All(r => byte.TryParse(r, out tempForParsing));
        }

        /// <summary>
        /// Checks if the range of the inserted IPs is valid.
        /// </summary>
        /// <returns></returns>
        public bool IsValidIpRange()
        {
            //each element in the array will contain a number between 0-255
            string[] splitIpTo = ipToTB.Text.Split('.');
            string[] splitIpFrom = ipFromTB.Text.Split('.');

            for(int i =0; i < splitIpTo.Length; i++)
            {
                //checks in the correlate elements if To is less than From.
                if (Int32.Parse(splitIpTo[i]) < Int32.Parse(splitIpFrom[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if the port is bigger than 65535 or less than 0 and if the ports have a valid range. Uses the IsRangeValid method.
        /// </summary>
        /// <returns></returns>
        public bool IsPortValid()
        {
            if(portFromTB.Text.Length == 0 && portToTB.Text.Length == 0)
            {
                return true;
            }

            try
            {
                int portFrom = int.Parse(portFromTB.Text);
                int portTo = int.Parse(portToTB.Text);
                if (IsRangeValid(portFrom, portTo))
                {
                    if ((portFrom < MAX_PORT) && (portTo < MAX_PORT) && (portFrom > 0) && (portTo > 0))
                    {
                        FiltersData.portFrom = ushort.Parse(portFromTB.Text);
                        FiltersData.portTo = ushort.Parse(portToTB.Text);
                        return true;
                    }
                    sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + PORT_MSG_ERROR);
                    return false;

                }
                sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + PORT_RANGE_MSG_ERROR);
                return false;
            }
            catch (Exception)
            {
                sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + PORT_MSG_ERROR);
                return false;
            }
         
        }

        /// <summary>
        /// Checks if the packet size is less than 0 and if it has a valid range. Uses the IsRangeValid method
        /// </summary>
        /// <returns></returns>
        public bool IsPacketSizeValid()
        {
            if (packetSizeFromTB.Text.Length == 0 && packetSizeToTB.Text.Length == 0)
            {
                return true;
            }

            try
            {
                int packetSizeFrom = int.Parse(packetSizeFromTB.Text);
                int packetSizeTo = int.Parse(packetSizeToTB.Text);

                if (IsRangeValid(packetSizeFrom, packetSizeTo))
                {
                    if ((packetSizeFrom > 0) && (packetSizeTo > 0))
                    {
                        FiltersData.packetSizeFrom = int.Parse(packetSizeFromTB.Text);
                        FiltersData.packetSizeTo = int.Parse(packetSizeToTB.Text);
                        return true;
                    }
                    sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + PACKET_MSG_ERROR);
                    return false;
                }
                sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + PACKET_SIZE_RANGE_ERROR);
                return false;
            }
            catch (Exception)
            {          
               sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + PACKET_MSG_ERROR);
               return false;
            }
        }

        /// <summary>
        /// Checks if the second number is bigger than the first
        /// </summary>
        /// <param name="number1"></param>
        /// <param name="number2"></param>
        /// <returns></returns>
        public bool IsRangeValid(int number1, int number2)
        {
            return number2 >= number1;
        }

        /// <summary>
        /// Checks if the offset is bigger than 0
        /// </summary>
        /// <returns></returns>
        public bool IsOffsetVaild()
        {
            if(offsetTB.Text.Length == 0)
            {
                return true;
            }

            try
            {
                int offset = int.Parse(offsetTB.Text);
                if (offset > 0)
                {
                    FiltersData.offset = int.Parse(offsetTB.Text);
                    return true;
                }

                sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + OFFSET_MSG_ERROR);
                return false;
            }
            catch (Exception)
            {
                sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": " + OFFSET_MSG_ERROR);
                return false;
               
            }

        }


    }
}
