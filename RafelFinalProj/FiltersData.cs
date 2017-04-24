using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RafelFinalProj
{
    static class FiltersData
    {
        public static string ipDestFrom { get; set; }
        public static string ipDestTo { get; set; }
        public static string ipSrcFrom { get; set; }
        public static string ipSrcTo { get; set; }
        public static bool isIpV4 { get; set; }
        public static bool isUDP { get; set; }
        public static int packetSizeFrom { get; set; }
        public static int packetSizeTo { get; set; }
        public static ushort portFrom { get; set; }
        public static ushort portTo { get; set; }
        public static int offset { get; set; }

    }
}
