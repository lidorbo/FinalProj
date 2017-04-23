using System;
using System.Collections.Generic;
using System.Linq;
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

        public MainScreen()
        {
            InitializeComponent();
        }

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

        private void clearNotificationsBTN_Click(object sender, RoutedEventArgs e)
        {
            sysNotificationsLV.Items.Clear();
        }
    }
}
