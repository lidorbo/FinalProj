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
using RafelFinalProj.Properties;

namespace RafelFinalProj
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainScreen : Window
    {

        private XmlParser xmlParser;
        XmlDocument xmlFile;
        WireSharkParse wp;

        public MainScreen()
        {
            InitializeComponent();
            xmlFile = new XmlDocument();
            wiresharkLogTB.Text = Settings.Default.wireSharkFile;
            xmlLoadTB.Text = Settings.Default.xmlFile;
            try
            {
                xmlFile.Load(xmlLoadTB.Text);
                xmlParser = new XmlParser(this, xmlFile);
            }
            catch (Exception)
            {               
              xmlLoadTB.Text = String.Empty;
            }
            iniSaveTB.Text = Settings.Default.iniFile;
            logSaveTB.Text = Settings.Default.logFile;
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
                WriteNotification(ConstValues.WIRESHARK_LOADED);
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
                string statusMsg = ConstValues.XML_LOADED;
                try
                {
                    //checks if the XML file is valid
                    xmlFile.Load(xmlFD.FileName);
                    //checks if the format is correct
                    xmlParser = new XmlParser(this, xmlFile);
                    xmlLoadTB.Text = xmlFD.FileName;
                    
                }
                catch (XmlException exception)
                {
                   statusMsg = exception.Message;
                   xmlLoadTB.Text = String.Empty;
                }

                WriteNotification(statusMsg);
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
                WriteNotification(ConstValues.INVALID_SAVE_LOCATION_INI);
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
                WriteNotification(ConstValues.INVALID_SAVE_LOCATION_LOG);
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
        /// The method will start the scan, but beforehand, will check if all input is correct
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startScanBTN_Click(object sender, RoutedEventArgs e)
        {

           Protocol();
           EndianType();
           
            //checks if every field is valid
           bool ip = IsValidIP();
           bool port = IsPortValid();
           bool size = IsPacketSizeValid();
           bool offset = IsOffsetVaild();

           if(wiresharkLogTB.Text.Length == 0 || xmlLoadTB.Text.Length == 0 
               || iniSaveTB.Text.Length == 0 || logSaveTB.Text.Length == 0)
           {
               WriteNotification(ConstValues.FIELD_MISSING_ERROR);
               return;
           }


            if (ip && port && size && offset)
            {
                

                Settings.Default.wireSharkFile = wiresharkLogTB.Text;
                Settings.Default.xmlFile = xmlLoadTB.Text;
                Settings.Default.iniFile = iniSaveTB.Text;
                Settings.Default.logFile = logSaveTB.Text;
                Settings.Default.Save();
                List<FieldStructure> fieldStructure = xmlParser.GetFieldsList();
                if (fieldStructure != null)
                {
                    WriteNotification(ConstValues.SCAN_MSG);
                    LogWriter lw = new LogWriter(logSaveTB.Text, this, xmlLoadTB.Text, wiresharkLogTB.Text);
                    wp = new WireSharkParse(iniSaveTB.Text, wiresharkLogTB.Text, fieldStructure, this, lw);
                }
                else
                { 
                   WriteNotification(ConstValues.XML_ERROR);
                }
           }
           else
           {
                WriteNotification(ConstValues.SCAN_MSG_ERROR);
           }

        }

        /// <summary>
        /// Writes notifications to the list view notifications
        /// </summary>
        /// <param name="str">The string to write</param>
        public void WriteNotification(string str)
        {
            this.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => this.sysNotificationsLV.Items.Add(DateTime.Now.ToString(ConstValues.TIME_FORMAT) + " : " + str)));
           
            this.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                new Action(() => AutoScrollDown()));
        }

        public void AutoScrollDown()
        {
            if (VisualTreeHelper.GetChildrenCount(sysNotificationsLV) > 0)
            {
                Border border = (Border)VisualTreeHelper.GetChild(sysNotificationsLV, 0);
                ScrollViewer scrollViewer = (ScrollViewer)VisualTreeHelper.GetChild(border, 0);
                scrollViewer.ScrollToBottom();
            }
        }

     
        /// <summary>
        /// Restes the progress bar and shows it.
        /// </summary>
        public void ShowAndSetProgressBar()
        {
            this.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                 new Action(() => this.scanProgressBar.Visibility = System.Windows.Visibility.Visible));

            this.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
             new Action(() => this.progressValue.Visibility = System.Windows.Visibility.Visible));

            this.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
               new Action(() => this.scanProgressBar.Value = 0));

        }

        public void ProgressBarReportProgress(int currentValue, ulong total)
        {
            this.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
             new Action(() => this.scanProgressBar.Value = (100 * currentValue) / (int)total));
        }

        public void HideProgressBar()
        {
            this.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
                 new Action(() => this.scanProgressBar.Visibility = System.Windows.Visibility.Hidden));

            this.sysNotificationsLV.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal,
             new Action(() => this.progressValue.Visibility = System.Windows.Visibility.Hidden));
        }


        /// <summary>
        /// Checks if the inserted IPs are vaild and have a correct range. Will use the methods ValidateIP and IsValidIpRange.
        /// </summary>
        /// <returns></returns>
        private bool IsValidIP()
        {

            bool ipFromParse;
            bool ipToParse;
            
           ipFromParse = ValidateIPv4(ipFromTB.Text, true);
           ipToParse = ValidateIPv4(ipToTB.Text, false);
            
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
           WriteNotification(ConstValues.IP_MSG_ERROR);
           return false ;
        }

        /// <summary>
        /// Will set the protocol filter to udp, tcp or both.
        /// </summary>
        private void Protocol()
        {         
            if (udpRB.IsChecked.Value && !tcpRB.IsChecked.Value)
            {
                FiltersData.protocol = ConstValues.UDP_STR;
            }
            else if (tcpRB.IsChecked.Value && !udpRB.IsChecked.Value)
            {
                FiltersData.protocol = ConstValues.TCP_STR;
            }
            else
            {
                FiltersData.protocol = ConstValues.ALL_STR;
            }
        }

       
        /// <summary>
        /// Will set the Endian type fitler to little or big.
        /// </summary>
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
        public bool ValidateIPv4(string ipString, bool isSrc)
        {
            if (String.IsNullOrWhiteSpace(ipString))
            {
                if(isSrc)
                {
                    FiltersData.ipSrc = String.Empty;
                }
                else
                {
                    FiltersData.ipDest = String.Empty;
                }
                return true;
            }

            string[] splitValues = ipString.Split(ConstValues.IPV4_SEPERATOR);
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
            //each element in the array will contain a number in either 0-255 or 0-65535
            string[] splitIpTo;
            string[] splitIpFrom;

            splitIpTo = ipToTB.Text.Split(ConstValues.IPV4_SEPERATOR);
            splitIpFrom = ipFromTB.Text.Split(ConstValues.IPV4_SEPERATOR);       

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
                FiltersData.portFrom = 0;
                FiltersData.portTo = 0;
                return true;
            }

            try
            {
                int portFrom = int.Parse(portFromTB.Text);
                int portTo = int.Parse(portToTB.Text);
                if (IsRangeValid(portFrom, portTo))
                {
                    if ((portFrom < ConstValues.MAX_PORT) && (portTo < ConstValues.MAX_PORT) && (portFrom > 0) && (portTo > 0))
                    {
                        FiltersData.portFrom = ushort.Parse(portFromTB.Text);
                        FiltersData.portTo = ushort.Parse(portToTB.Text);
                        return true;
                    }
                    WriteNotification(ConstValues.PORT_MSG_ERROR);
                    return false;

                }
                WriteNotification(ConstValues.PORT_RANGE_MSG_ERROR);
                return false;
            }
            catch (Exception)
            {
                WriteNotification(ConstValues.PORT_MSG_ERROR);
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
                FiltersData.packetSizeFrom = -1;
                FiltersData.packetSizeTo = -1;
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
                    WriteNotification(ConstValues.PACKET_MSG_ERROR);
                    return false;
                }
                WriteNotification(ConstValues.PACKET_SIZE_RANGE_ERROR);
                return false;
            }
            catch (Exception)
            {          
                WriteNotification(ConstValues.PACKET_MSG_ERROR);
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
                FiltersData.offset = 0;
                return true;
            }

            try
            {
                int offset = int.Parse(offsetTB.Text);
                if (offset >= 0)
                {
                    FiltersData.offset = int.Parse(offsetTB.Text);
                    return true;
                }

                WriteNotification(ConstValues.OFFSET_MSG_ERROR);
                return false;
            }
            catch (Exception)
            {
                WriteNotification(ConstValues.OFFSET_MSG_ERROR);
                return false;
               
            }

        }

        /// <summary>
        /// Will stop the ongoing scan
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void stopScanBTN_Click(object sender, RoutedEventArgs e)
        {
            wp.worker.CancelAsync();
        }


    }
}
