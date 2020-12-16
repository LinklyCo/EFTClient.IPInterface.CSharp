namespace PCEFTPOS.EFTClient.IPInterface
{
    public static class StringExtension
    {
        /// <summary>
        /// Pad or cut a string so that the length is equal to totalWidth
        /// </summary>
        /// <param name="v"></param>
        /// <param name="totalWidth">The width of the string to return</param>
        /// <returns>A string of length totalWidth</returns>
        public static string PadRightAndCut(this string v, int totalWidth)
        {
            if (v == null)
                return "".PadRight(totalWidth);

            if (v?.Length == totalWidth)
                return v;
            else if (v?.Length < totalWidth)
                return v?.PadRight(totalWidth);
            else
                return v?.Substring(0, totalWidth);
        }

        /// <summary>
        /// Pad or cut a string so that the length is equal to totalWidth
        /// </summary>
        /// <param name="v"></param>
        /// <param name="totalWidth">The width of the string to return</param>
        /// <param name="paddingChar">The char to use for padding if required</param>/// 
        /// <returns>A string of length totalWidth</returns>
        public static string PadRightAndCut(this string v, int totalWidth, char paddingChar)
        {
            if (v == null)
                return "".PadRight(totalWidth, paddingChar);

            if (v?.Length == totalWidth)
                return v;
            else if (v?.Length < totalWidth)
                return v?.PadRight(totalWidth, paddingChar);
            else
                return v?.Substring(0, totalWidth);
        }

        /// <summary>
        /// Returns a substring of totalWidth length, or if input length is less than, leaves it alone
        /// </summary>
        /// <param name="v"></param>
        /// <param name="totalWidth"></param>
        /// <returns></returns>
        public static string CutAndLeave(this string v, int totalWidth)
        {
            if (v?.Length > totalWidth)
                return v?.Substring(0, totalWidth);
            else
                return v;
        }

        /// <summary>
        /// Returns the length of the string until a null terminator or end of string (whichever comes first)
        /// </summary>
        /// <param name="s">String to get length of</param>
        /// <returns></returns>
        public static int StrLen(this string v) => StrLen(v, 0);

        /// <summary>
        /// Returns the length of the string until a null terminator or end of string (whichever comes first)
        /// </summary>
        /// <param name="s">String to get length of</param>
        /// <param name="startIndex">Offset into the string to start counting from</param>
        /// <returns></returns>
        public static int StrLen(this string v, int startIndex)
        {
            int idx = v.IndexOf('\0', startIndex);
            return (idx < 0) ? v.Length : idx;
        }

        /// <summary>
        /// Replaces any whitespace with a viewable character.
        /// ' ' to '˽'
        /// '\t' to '→'
        /// "\r\n" to "↙"
        /// '\n' to '↓'
        /// '\r' to '←'
        /// </summary>
        /// <param name="original">String to convert</param>
        /// <returns>Converted string</returns>
        public static string ToVisibleSpaces(this string original)   => original.Replace(' ', '˽').Replace('\t', '→').Replace("\r\n", "↙").Replace('\n', '↓').Replace('\r', '←');

        /// <summary>
        /// Removes any visible whitespace with its equivalent.
        /// '˽' to ' '
        /// '→' to '\t'
        /// "↙" to "\r\n"
        /// '↓' to '\n'
        /// '←' to '\r'
        /// </summary>
        /// <param name="original">String to convert</param>
        /// <returns>Converted string</returns>
        public static string FromVisibleSpaces(this string original) => original.Replace('˽', ' ').Replace('→', '\t').Replace("↙", "\r\n").Replace('↓', '\n').Replace('←', '\r');
    }
}
