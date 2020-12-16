namespace PCEFTPOS.EFTClient.IPInterface
{
    public static class StringBuilderExtension
    {
        public static string Substring(this System.Text.StringBuilder sb, int startIndex)
        {
            return sb.ToString(startIndex, sb.Length - startIndex);
        }

        public static string Substring(this System.Text.StringBuilder sb, int startIndex, int length)
        {
            return sb.ToString(startIndex, length);
        }
    }
}
