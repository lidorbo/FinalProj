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

        /// <summary>
        /// Creates the log file. The name will be: LOG_FILE_(Date)_(ini file name).txt
        /// If there's a file with the same name, a number will be added at the end.
        /// </summary>
        public void CreateLogFile()
        {
            string iniFileName = logFilePath + ConstValues.LOG_FILE_NAME + DateTime.Today.ToString(ConstValues.DATE_FORMAT);
            try
            {
                logFS = new FileStream(iniFileName + ConstValues.LOG_FILE_EXTENTION, FileMode.CreateNew);
            }
            catch (IOException ex)
            {
                int count = 0;

                while (File.Exists(iniFileName + ConstValues.FILENAME_SEPERATOR + count + ConstValues.LOG_FILE_EXTENTION))
                {
                    count++;
                }

                logFS = new FileStream(iniFileName + ConstValues.FILENAME_SEPERATOR + count + ConstValues.LOG_FILE_EXTENTION, FileMode.CreateNew);
            }
            catch (Exception e)
            {
                mainScreen.WriteNotification(ConstValues.LOG_ERROR + e.Message);
                return;
            }
            finally
            {
                mainScreen.WriteNotification(ConstValues.LOG_CREATED);
                string title = "Scan Date - " + DateTime.Today.ToString(ConstValues.DATE_FORMAT) + Environment.NewLine 
                             + "Scan Time - " + DateTime.Now.ToString(ConstValues.TIME_FORMAT) + Environment.NewLine
                             + "Xml File Name - " + xmlFileName + Environment.NewLine
                             + "WireShark File Name - " + wireSharkFile + Environment.NewLine + Environment.NewLine 
                             + Environment.NewLine;
                WriteToLog(title);
            }

        }

        /// <summary>
        /// Writes a string to the log file
        /// </summary>
        /// <param name="str">The string to write</param>
        public void WriteToLog(string str)
        {
            byte[] data = Encoding.UTF8.GetBytes(str);
            logFS.Write(data, 0, data.Length);
        }

        /// <summary>
        /// Closes the log file
        /// </summary>
        public void Finish()
        {
            logFS.Flush();
            logFS.Close();
        }
    }
}
