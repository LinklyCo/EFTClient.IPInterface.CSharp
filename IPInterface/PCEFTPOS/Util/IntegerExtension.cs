using System;

namespace PCEFTPOS.EFTClient.IPInterface
{
    public static class IntegerExtension
    {
        /// <summary>
        /// Pad an integer string
        /// </summary>
        /// <param name="v"></param>
        /// <param name="totalWidth">The width of the string to return</param>
        /// <example>
        /// <code>
        /// // Working example
        /// var i = 145;
        /// var s = d.PadLeft(5);
        /// Debug.Assert(s == "00145");
        /// 
        /// // Error
        /// var d = 145;
        /// var s = d.PadLeft(1); // throws ArgumentOutOfRangeException 
        /// </code>
        /// </example>
        /// <returns>A string of length totalWidth</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the number is too long for the totalWidth provided</exception>
        public static string PadLeft(this int v, int totalWidth)
        {
            // have to account for the "-" character for negative inputs
            var s =  String.Format($"{{0:D{(v < 0 ? totalWidth-1 : totalWidth)}}}", v);
            if (s.Length > totalWidth)
                throw new ArgumentOutOfRangeException(nameof(totalWidth), $"The number '{v}' cannot fit in a padded string of length:{totalWidth}");
            return s;
        }
    }

}
