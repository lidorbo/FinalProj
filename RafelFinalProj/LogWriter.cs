using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
namespace RafelFinalProj
{
    class LogWriter
    {
        FileStream logFS;
        MainScreen mainScreen;
        string logFilePath;
        string xmlFileName;
        string wireSharkFile;

        public LogWriter(string path, MainScreen mainScreen, string xmlFileName, string wireSharkFile)
        {
            logFilePath = path;
            this.xmlFileName = xmlFileName;
            this.mainScreen = mainScreen;
            this.wireSharkFile = wireSharkFile;
            CreateLogFile();
        }

        public void CreateLogFile()
        {
            string iniFileName = logFilePath + "\\LOG_FILE " + DateTime.Today.ToString("dd-MM-yyyy");
            try
            {
                logFS = new FileStream(iniFileName + ".txt", FileMode.CreateNew);
            }
            catch (IOException ex)
            {
                int count = 0;

                while (File.Exists(iniFileName + "_" + count + ".txt"))
                {
                    count++;
                }

                logFS = new FileStream(iniFileName + "_" + count + ".txt", FileMode.CreateNew);
            }
            catch (Exception e)
            {
                mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Error " + e.Message);
                return;
            }
            finally
            {
                mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Create a new Log file");
                string title = "Scan Date - " + DateTime.Today.ToString("dd-MM-yyyy") + Environment.NewLine 
                             + "Scan Time - " + DateTime.Now.ToString("HH:mm") + Environment.NewLine
                             + "Xml File Name - " + xmlFileName + Environment.NewLine
                             + "WireShark File Name - " + wireSharkFile + Environment.NewLine + Environment.NewLine 
                             + Environment.NewLine;
                WriteToLog(title);
            }

        }

        public void WriteToLog(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            logFS.Write(data, 0, data.Length);
        }

        public void Finish()
        {
            logFS.Flush();
            logFS.Close();
        }
    }
}
