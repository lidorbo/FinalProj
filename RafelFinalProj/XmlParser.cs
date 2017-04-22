using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace RafelFinalProj
{
    class XmlParser
    {
        const string FIELD = "FIELD";

        MainScreen mainScreen;
        XmlDocument xmlFile;

        public XmlParser(MainScreen mainScreen, XmlDocument xmlFile)
        {
            this.mainScreen = mainScreen;
            this.xmlFile = xmlFile;
            IsValidXmlFormat();
        }

        /// <summary>
        /// This method will validate the format of the XML file according to a specific format
        /// <Message> //the name of the message is irrelevant
        /// <Field#></Field#> //in an even position
        /// <Field#S></Field#S> //in an odd position
        /// </Message>
        /// </summary>
        /// <returns>true if the format is valid</returns>
        private bool IsValidXmlFormat()
        {
            string fieldName = "";
            XmlElement root = xmlFile.DocumentElement;
            XmlNodeList elements = root.ChildNodes;
           for (int i=0; i < elements.Count; i++)
           {
                fieldName = elements[i].Name.ToUpper();
                if (fieldName.StartsWith(FIELD))
                {
                    string checkDigits = fieldName.TrimStart(FIELD.ToCharArray());
                    if (i % 2 == 0)
                    {
                            if (IsDigitsOnly(checkDigits))
                            {
                                mainScreen.sysNotificationsLV.Items.Add(fieldName + " " + elements[i].InnerText);
                            }
                            else
                            {
                                mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Field name " + elements[i].Name + " should end with a number");
                                return false;
                            }
                    }
                    else
                    {
                            if (checkDigits[checkDigits.Length - 1] == 'S' && IsDigitsOnly(checkDigits.Substring(0, checkDigits.Length - 1)))
                            {
                                mainScreen.sysNotificationsLV.Items.Add(fieldName + " " + elements[i].InnerText);
                            }
                            else
                            {
                                mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Field name " + elements[i].Name + " should have a number and S at the end");
                                return false;
                            }                      
                    } 
                }
                else
                {
                    mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Field name " + elements[i].Name + "is invalid.");
                    return false;
                }
            }
         
          return true;
        }

        bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

    }
}
