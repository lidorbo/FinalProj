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

        private MainScreen mainScreen;
        private XmlDocument xmlFile;
        private List<FieldStructure> fieldsList;
        private Dictionary<string, int> sizeOfTypes;


        public XmlParser(MainScreen mainScreen, XmlDocument xmlFile)
        {
            this.mainScreen = mainScreen;
            this.xmlFile = xmlFile;
            fieldsList = new List<FieldStructure>();
            InitSizeOfDictionary();
        }

        /// <summary>
        /// Returns the fields list. will return null if there's an error with the XML or it's empty
        /// </summary>
        /// <returns></returns>
        public List<FieldStructure> GetFieldsList()
        {
            if(ParseXml() && fieldsList.Count > 0)
            {
                return fieldsList;
            }

            return null;
        }

        /// <summary>
        /// This method will validate the format of the XML file according to the requested format
        /// <Message> //the name of the message is irrelevant
        /// <Field#></Field#> //in an even position
        /// <Field#S></Field#S> //in an odd position
        /// </Message>
        /// </summary>
        /// <returns>true if the format is valid</returns>
        private bool ParseXml()
        {
            string fieldName = String.Empty;
            string keyValue = String.Empty;
            int typeSize = 0;
            XmlElement root = xmlFile.DocumentElement;
            XmlNodeList elements = root.ChildNodes;
            FieldStructure fs;
            fs = new FieldStructure();
            fieldsList.Clear();

            for (int i = 0; i < elements.Count; i++)
            {
                fieldName = elements[i].Name.ToUpper();
                if (fieldName.StartsWith(ConstValues.XML_FIELD_NAME))
                {
                    string checkDigits = fieldName.TrimStart(ConstValues.XML_FIELD_NAME.ToCharArray());
                    if (i % 2 == 0)
                    {
                        if (IsDigitsOnly(checkDigits))
                        {
                            keyValue = elements[i].InnerText;
                            fs.fieldName = keyValue;
                        }
                        else
                        {
                            mainScreen.WriteNotification(ConstValues.XML_END_WITH_NUMBER_ERROR + elements[i].Name);
                            return false;
                        }
                    }
                    else
                    {
                        if (checkDigits[checkDigits.Length - 1] == ConstValues.FIELD_TYPE_SUFFIX && IsDigitsOnly(checkDigits.Substring(0, checkDigits.Length - 1)))
                        {
                            typeSize = ConvertTypeToInt(elements[i].InnerText, fs);

                            if (typeSize == 0)
                            {
                                mainScreen.WriteNotification(ConstValues.XML_INVALID_TYPE_ERROR + elements[i].InnerText);
                                return false;
                            }

                            fs.size = typeSize;
                            fieldsList.Add(fs);
                            fs = new FieldStructure();

                        }
                        else
                        {
                            mainScreen.WriteNotification(ConstValues.XML_NUMBER_AND_S_ERROR + elements[i].Name);
                            return false;
                        }
                    }


                }
                else
                {
                    mainScreen.WriteNotification(ConstValues.XML_INVALID_FIELD_NAME_ERROR + elements[i].Name);
                    return false;
                }
            }

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
                
                if (c < ConstValues.ZERO_CHAR || c > ConstValues.NINE_CHAR)
                    return false;
            }

            return true;
        }


        /// <summary>
        /// The method will initialize a new dictionary for different types of size.
        /// </summary>
        private void InitSizeOfDictionary()
        {
            sizeOfTypes = new Dictionary<string, int>();

            //Signed
            sizeOfTypes.Add(ConstValues.CHAR, 1);
            sizeOfTypes.Add(ConstValues.SHORT, 2);
            sizeOfTypes.Add(ConstValues.INT32, 4);
            sizeOfTypes.Add(ConstValues.INT64, 8);
            sizeOfTypes.Add(ConstValues.LONG, 16);

            //Unsigned
            sizeOfTypes.Add(ConstValues.UCHAR, 1);
            sizeOfTypes.Add(ConstValues.USHORT, 2);
            sizeOfTypes.Add(ConstValues.UINT32, 4);
            sizeOfTypes.Add(ConstValues.UINT64, 8);
            sizeOfTypes.Add(ConstValues.ULONG, 16);
        }

        /// <summary>
        /// The method match key to it's size.
        /// </summary>
        /// <param name="key"></param>
        /// <returns>The acctual size in bytes. Return 0 if size is invalid</returns>
        private int ConvertTypeToInt(string key, FieldStructure fs)
        {
            foreach (var e in sizeOfTypes)
            {
                if (e.Key.ToUpper().CompareTo(key.ToUpper()) == 0)
                {
                    fs.type = e.Key;
                    return e.Value;
                }
            }

            return 0;
        }


    }
}
