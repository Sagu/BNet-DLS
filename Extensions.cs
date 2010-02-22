//Most code taken from Patrick Sapinski's http://github.com/kow/Starcraft-2-Battle.Net-Wrapper repo

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BNet
{
    internal static class Extensions
    {
        public static string ToHexString(this byte[] byteArray)
        {
            string retStr = "";
            foreach (byte b in byteArray)
                retStr += b.ToString("x2");
            return retStr;
        }

        public static string ToHexDump(this byte[] byteArray)
        {
            string retStr = "";
            for (int i = 0; i < byteArray.Length; i++)
            {
                if (i > 0 && i % 16 == 0)
                    retStr += "\n";
                retStr += byteArray[i].ToString("x2") + " ";
            }
            return retStr;
        }

        public static uint ToBin(this String s, out byte[] bytes)
        {
            char[] ca = s.ToCharArray();
            bytes = new byte[4];
            bytes[0] = (byte)ca[0];
            bytes[1] = (byte)ca[1];
            bytes[2] = (byte)ca[2];
            bytes[3] = (byte)ca[3];
            var b0 = (uint)ca[0];
            var b1 = (uint)ca[1];
            var b2 = (uint)ca[2];
            var b3 = (uint)ca[3];
            uint r = b3 | (b2 << 8) | (b1 << 16) | (b0 << 24);
            return r;
        }
    }
}
