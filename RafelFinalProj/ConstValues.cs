using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RafelFinalProj
{
    static class ConstValues
    {

        #region Variables types size
        public const string CHAR = "char";
        public const string SHORT = "short";
        public const string INT32 = "int32";
        public const string INT64 = "int64";
        public const string LONG = "long";
        public const string UCHAR = "Uchar";
        public const string USHORT = "Ushort";
        public const string UINT32 = "Uint32";
        public const string UINT64 = "Uint64";
        public const string ULONG = "Ulong"; 
        #endregion

        #region Protocols
        public const string UDP_STR = "udp";
        public const string TCP_STR = "tcp";
        public const string ALL_STR = "all";
        #endregion

        #region Chars
        public const char IPV4_SEPERATOR = '.';
        public const char IPV6_SEPERATOR = ':';
        public const char FIELD_TYPE_SUFFIX = 'S';
        public const char VALUE_TIMETAG_SEPERATOR = ';';
        public const char ZERO_CHAR = '0';
        public const char NINE_CHAR = '9';
        #endregion

        #region System messages
        //***Wireshark Messages***
        public const string WIRESHARK_LOADED = "Wireshark file loaded";
        public const string WIRESHARK_ERROR = "Error - Wireshark file can't be loaded";
        public const string SCANNING_WIRESHARK_FILE = "Scanning Wireshark file";
        public const string SCANNING_WIRESHARK_COMPLETED = "Wireshark scan completed";
        public const string WIRESHARK_PARSE_ERROR = "Wireshark parse error: ";
        public const string BAD_PACKET = "Scan error - Unkown packet protocol. packet number: ";
        public const string NO_RESULTS = "Scan ended with no results";
        public const string BIGGER_THAN_XML = "This packet is bigger than the XML fields: ";

        //***ini Messages***
        public const string INVALID_SAVE_LOCATION_INI = "Invalid save path for ini";
        public const string INI_FILE_ERROR = "ini File error: ";
        public const string INI_FILE_CREATED = "ini file have been created";

        //***Log Messages***
        public const string INVALID_SAVE_LOCATION_LOG = "Invalid save path for log";
        public const string LOG_ERROR = "Log Error: ";
        public const string LOG_CREATED = "Log file have been created";

        //***Fiters Messages***
        public const string IP_MSG_ERROR = "Error - One or more IPs are not valid";
        public const string PORT_MSG_ERROR = "Error - Port is not valid";
        public const string PORT_RANGE_MSG_ERROR = "Error - Port range is not valid";
        public const string PACKET_MSG_ERROR = "Error - One or packet sizes are not valid";
        public const string PACKET_SIZE_RANGE_ERROR = "Error - Packet size range is not valid";
        public const string OFFSET_MSG_ERROR = "Error - Offset must be bigger than 0";


        //***XML Messages***
        public const string XML_LOADED = "XML file loaded";
        public const string XML_END_WITH_NUMBER_ERROR = "XML Error - the following field should end with a number: ";
        public const string XML_INVALID_TYPE_ERROR = "XML Error - Invalid type: ";
        public const string XML_NUMBER_AND_S_ERROR = "XML Error - The following field should have a number and S at the end: ";
        public const string XML_INVALID_FIELD_NAME_ERROR = "XML Error - The following field name is invalid: ";
        public const string XML_ERROR = "Error - The XML file is either empty or corrupt.";

        //***General Messages***
        public const string SCAN_MSG = "Scan have started";
        public const string SCAN_MSG_ERROR = "Error - Scan have not started!";
        public const string FIELD_MISSING_ERROR = "Error - One or more fields are missing.";
        public const string WRITING_TO_FILES = "Writing to results and log files";
        public const string WRITING_COMPLETE = "Writing complete, All done !";
        #endregion

        #region PCAP
        public const string PCAP_DST = "dst ";
        public const string PCAP_SRC = "src ";
        public const string PCAP_AND = " and ";
        public const string PCAP_PORT_RANGE = "portrange ";
        public const string PCAP_PORT_RANGE_SEPERATOR = "-";
        #endregion

        #region Other
        public const int MAX_PORT = 65535;
        public const string XML_FIELD_NAME = "FIELD";
        public const string STRING_WHITESPACE = " ";
        public const string TIME_FORMAT = "HH:mm";
        public const string DATE_FORMAT = "dd-MM-yyyy";
        public const string LOG_FILE_NAME = "\\LOG_FILE ";
        public const string LOG_FILE_EXTENTION = ".txt";
        public const string FILENAME_SEPERATOR = "_";
        public const string RESULTS_FILE_EXTENTION = ".ini";
        #endregion

        
    }
}
