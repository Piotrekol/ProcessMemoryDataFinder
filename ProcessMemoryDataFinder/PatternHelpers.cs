using System;

namespace ProcessMemoryDataFinder
{
    public static class PatternHelpers
    {
        public static byte[] UnpackStr(string str)
        {
            return StringToBytePatternArray(str).Bytes;
        }

        public static (byte[] Bytes, string Mask) StringToBytePatternArray(string hex)
        {
            var numberChars = hex.Length;
            var mask = string.Empty;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
            {
                var substring = hex.Substring(i, 2);
                if (substring == "??")
                {
                    bytes[i / 2] = 0;
                    mask += "?";
                }
                else
                {
                    bytes[i / 2] = Convert.ToByte(substring, 16);
                    mask += "x";
                }
            }

            return (bytes, mask);
        }
    }
}