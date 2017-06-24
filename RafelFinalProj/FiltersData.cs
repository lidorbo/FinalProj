using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RafelFinalProj
{
    static class FiltersData
    {
        public static string ipDest { get; set; }
        public static string ipSrc { get; set; }
        public static string protocol { get; set; }
        public static int packetSizeFrom { get; set; }
        public static int packetSizeTo { get; set; }
        public static ushort portFrom { get; set; }
        public static ushort portTo { get; set; }
        public static int offset { get; set; }
        public static bool isLitleEndian { get; set; }

    }
}
