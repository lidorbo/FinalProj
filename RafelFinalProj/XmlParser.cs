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
        Dictionary<string, int> xmlStructure;
        Dictionary<string, int> sizeOfTypes;


        public XmlParser(MainScreen mainScreen, XmlDocument xmlFile)
        {
            this.mainScreen = mainScreen;
            this.xmlFile = xmlFile;
            xmlStructure = new Dictionary<string, int>();
            InitSizeOfDictionary();
            ParseXml();

        }

        /// <summary>
        /// This method will validate the format of the XML file according to a specific format
        /// <Message> //the name of the message is irrelevant
        /// <Field#></Field#> //in an even position
        /// <Field#S></Field#S> //in an odd position
        /// </Message>
        /// </summary>
        /// <returns>true if the format is valid</returns>
        private bool ParseXml()
        {
            string fieldName = "";
            string keyValue = "";
            int typeSize = 0;
            XmlElement root = xmlFile.DocumentElement;
            XmlNodeList elements = root.ChildNodes;

            for (int i = 0; i < elements.Count; i++)
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
                            keyValue = elements[i].InnerText;
                            xmlStructure.Add(keyValue, 0);
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
                            typeSize = ConvertTypeToInt(elements[i].InnerText);
                            if (typeSize == 0)
                            {
                                mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Invalid type " + elements[i].InnerText);
                                return false;
                            }
                            xmlStructure[keyValue] = typeSize;
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
                    mainScreen.sysNotificationsLV.Items.Add(DateTime.Now.ToString("HH:mm") + ": Field name " + elements[i].Name + " is invalid.");
                    return false;
                }
            }

            string str = "";
            foreach (var e in xmlStructure)
            {
                str += e.Key + " " + e.Value + "\n";
            }

            mainScreen.sysNotificationsLV.Items.Add(str);


            return true;
        }

        /// <summary>
        ///  This method get a string and check if it contains only digits.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>True if only digits</returns>
        private bool IsDigitsOnly(string str)
        {
            foreach (char c in str)
            {
                if (c < '0' || c > '9')
                    return false;
            }

            return true;
        }

        private void AddElementToDictionary(XmlElement element)
        {
            string fieldName = element.InnerText;
            int value = ConvertTypeToInt(element.InnerText);


        }

        private void InitSizeOfDictionary()
        {
            sizeOfTypes = new Dictionary<string, int>();

            //Signed
            sizeOfTypes.Add("char", 1);
            sizeOfTypes.Add("short", 2);
            sizeOfTypes.Add("int32", 4);
            sizeOfTypes.Add("int64", 8);
            sizeOfTypes.Add("long", 16);

            //Unsigned
            sizeOfTypes.Add("Uchar", 1);
            sizeOfTypes.Add("Ushort", 2);
            sizeOfTypes.Add("Uint32", 4);
            sizeOfTypes.Add("Uint64", 8);
            sizeOfTypes.Add("Ulong", 16);


        }

        private int ConvertTypeToInt(string key)
        {
            foreach (var e in sizeOfTypes)
            {
                if (e.Key.ToUpper().CompareTo(key.ToUpper()) == 0)
                {
                    return e.Value;
                }
            }

            return 0;
        }


    }
}
