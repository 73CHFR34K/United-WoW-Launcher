using System;
using System.Text;

namespace United_WoW_Launcher
{
    internal static class Extension
    {
        public static string ToHex(this byte[] bytes, bool upperCase)
        {
            StringBuilder stringBuilder = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                stringBuilder.Append(b.ToString(upperCase ? "X2" : "x2"));
            }
            return stringBuilder.ToString();
        }
    }
}
