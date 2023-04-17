using System;
using PCEFTPOS.EFTClient.IPInterface;
using Xunit;

namespace IPInterface.Test
{
    public class StringExtensionTests
    {

        #region PadRightAndCut
        [Theory]
        // check zero target width
        [InlineData("", 0, "")] // empty, zero target width
        [InlineData(null, 0, "")] // null, zero target width
        [InlineData("Leopard", 0, "")] // non-empty, zero target width
        // check (positive) non-zero target Width
        [InlineData("", 5, "     ")] // empty, non-zero target width
        [InlineData(null, 5, "     ")] // null, non-zero target width
        [InlineData("Puma", 5, "Puma ")] // non-empty, input length < target width
        [InlineData("Lion", 3, "Lio")] // non-empty, input length > target width
        [InlineData("Panther", 7, "Panther")] // non-empty, input length = target width
        public void PadRightAndCut_ReturnsPaddedOrCut(string inputStr, int targetWidth, string expected)
        {
            var actual = inputStr.PadRightAndCut(targetWidth);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void PadRightAndCut_ThrowsArgumentOutOfRange()
        {
            const string inputStr = "Turtle";
            const int negativeWidth = -1;

            Action actual = () => inputStr.PadRightAndCut(negativeWidth);

            Assert.Throws<ArgumentOutOfRangeException>(actual);

        }

        [Theory]
        // check zero target width
        [InlineData("", 0, 'X', "")] // empty, zero target width
        [InlineData(null, 0, 'X', "")] // null, zero target width
        [InlineData("Leopard", 0, 'X', "")] // non-empty, zero target width
        // check (positive) non-zero target Width
        [InlineData("", 5, 'X', "XXXXX")] // empty, non-zero target width
        [InlineData(null, 5, 'X', "XXXXX")] // null, non-zero target width
        [InlineData("Puma", 5, 'X', "PumaX")] // non-empty, input length < target width
        [InlineData("Lion", 3, 'X', "Lio")] // non-empty, input length > target width
        [InlineData("Panther", 7, 'X', "Panther")] // non-empty, input length = target width
        public void PadRightAndCut_SpecifiedPadChar_ReturnsPaddedOrCut(string inputStr, int inputWidth, char padChar, string expected)
        {
            var actual = inputStr.PadRightAndCut(inputWidth, padChar);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void PadRightAndCut_SpecifiedPadChar_ThrowsArgumentOutOfRange()
        {
            const string inputStr = "Turtle";
            const int negativeWidth = -1;
            const char padChar = '+';
            void Actual() => inputStr.PadRightAndCut(negativeWidth, padChar);

            Assert.Throws<ArgumentOutOfRangeException>((Action) Actual);

        }

        #endregion

        #region CutAndLeave
        [Theory]
        // check zero target width
        [InlineData("", 0, "")] // empty, zero target width
        [InlineData(null, 0, null)] // null, zero target width
        [InlineData("Leopard", 0, "")] // non-empty, zero target width
        // check (positive) non-zero target Width
        [InlineData("", 5, "")] // empty, non-zero target width
        [InlineData(null, 5, null)] // null, non-zero target width
        [InlineData("Puma", 5, "Puma")] // non-empty, input length < target width
        [InlineData("Lion", 3, "Lio")] // non-empty, input length > target width
        [InlineData("Panther", 7, "Panther")] // non-empty, input length = target width


        public void CutAndLeave_ReturnsCut(string inputStr, int targetWidth, string expected)
        {
            var actual = inputStr.CutAndLeave(targetWidth);
            Assert.Equal(expected, actual);

        }


        [Fact]
        public void CutAndLeave_ThrowsArgumentOutOfRange()
        {
            const string inputStr = "Turtle";
            const int negativeWidth = -1;
            void Actual() => inputStr.CutAndLeave(negativeWidth);

            Assert.Throws<ArgumentOutOfRangeException>((Action)Actual);

        }

        #endregion

        #region StrLen
        [Theory]
        [InlineData("", 0)] // empty, so expected is 0
        [InlineData("\0ShouldBeIgnored", 0)] // Non-empty but starts with null term, so expected is 0
        [InlineData("Four", 4)] // Non-empty, no null term, so expected is "Four".Length (4)
        [InlineData("Thr\0ee", 3)] // Non-empty, null term at idx 3, so expected is 3
        [InlineData("Test\0", 4)] // Non-empty, null term is last character of string so expected is input.Length-1 (4)

        public void StrLen_ReturnsIdxOfNullTermOrDotNetLength(string inputStr, int expected)
        {
            var actual = inputStr.StrLen();

            Assert.Equal(expected, actual);
        }


        [Theory]
        [InlineData("", 0, 0)] // empty, 0 index, so expected is 0
        [InlineData("\0ShouldBeIgnored", 0, 0)] // Non-empty but index is 0 and starts with null term, so expected is 0
        [InlineData("\0Four", 1, 4)] // Starts with null term, but  should be skipped over because index = 1, so expected is 4
        [InlineData("Four",0, 4)] // Non-empty, no null term, index of 0, so expected is "Four".Length (4)
        [InlineData("Four\0", 0, 4)] // Non-empty, null term is last character of string so expected is input.Length-1 (4)
        [InlineData("Thr\0ee",0, 3)] // Non-empty, null term at idx 3, so expected is 3
        [InlineData("Thr\0ee",4, 2)] // Non-empty, null term at idx 3 but start index is 4, so expected is 2
        [InlineData("abc\0defg", 0, 3)]
        [InlineData("abc\0defg", 1, 2)]
        [InlineData("abc\0defg", 4, 4)]
        [InlineData("abc\0defg", 5, 3)]
        [InlineData("abc\0defabc\0def", 4, 6)]
        public void StrLen_WithStartIdx_ReturnsIdxOfNullTermOrDotNetLength(string inputStr, int startIdx, int expected)
        {
            var actual = inputStr.StrLen(startIdx);

            Assert.Equal(expected, actual);
        }

        #endregion

        #region ToVisibleSpaces/FromVisibleSpaces

        [Fact]
        public void ToVisibleSpaces_ReturnsViewableSpaces()
        {
            const string inputStr = " \t\r\n\n\r";
            const string expected = "˽→↙↓←";

            var actual = inputStr.ToVisibleSpaces();
            Assert.Equal(expected, actual);

        }

        [Fact]
        public void FromVisibleSpaces_ReturnsOriginalSpaces()
        {
            const string inputStr = "˽→↙↓←";
            const string expected = " \t\r\n\n\r";

            var actual = inputStr.FromVisibleSpaces();
            Assert.Equal(expected, actual);
        }

        #endregion

        #region SplitLast

        [Theory]
        [InlineData("",      new []{'.'},     new []{""})]
        [InlineData("a.b.c", new []{'.'},     new []{"a.b", "c"})]
        [InlineData("a.b.",  new []{'.'},     new []{"a.b", ""})]
        [InlineData("a",     new []{'.'},     new []{"a"})]
        [InlineData("a.b.c", new []{'.',','}, new []{"a.b", "c"})]
        [InlineData("a.b,c", new []{'.',','}, new []{"a.b", "c"})]
        public void SplitLast_DefaultParams(string inputStr, char[] delims, string[] expected)
        {
            var actual = inputStr.SplitLast(delims);

            Assert.Equal(actual, expected);
        }

        [Theory]
        [InlineData("",      new []{'.'},     1, new []{""})]
        [InlineData("a.b.c", new []{'.'},     1, new []{"a.b", "c"})]
        [InlineData("a.b.c", new []{'.'},     2, new []{"a", "b", "c"})]
        [InlineData("a.b.c", new []{'.'},     3, new []{"a", "b", "c"})]
        [InlineData("a.b.c", new []{'.'},     4, new []{"a", "b", "c"})]
        [InlineData("a.b,c", new []{'.',','}, 4, new []{"a", "b", "c"})]
        public void SplitLast_CountParams(string inputStr, char[] delims, int count, string[] expected)
        {
            var actual = inputStr.SplitLast(delims, count);

            Assert.Equal(actual, expected);
        }
        
        [Theory]
        [InlineData("",      new []{'.'},      StringSplitOptions.None,               new []{""})]
        [InlineData("",      new []{'.'},      StringSplitOptions.RemoveEmptyEntries, new string[0])]
        [InlineData("a.b.c", new []{'.'},      StringSplitOptions.None,               new []{"a.b", "c"})]
        [InlineData("a.b.c", new []{'.'},      StringSplitOptions.RemoveEmptyEntries, new []{"a.b", "c"})]
        [InlineData(".c",    new []{'.'},      StringSplitOptions.None,               new []{"", "c"})]
        [InlineData(".c",    new []{'.'},      StringSplitOptions.RemoveEmptyEntries, new []{"c"})]
        [InlineData("a.b.",  new []{'.', ','}, StringSplitOptions.None,               new []{"a.b", ""})]
        [InlineData("a.b,",  new []{'.', ','}, StringSplitOptions.RemoveEmptyEntries, new []{"a.b"})]
        public void SplitLast_OptionsParams(string inputStr, char[] delims, StringSplitOptions options, string[] expected)
        {
            var actual = inputStr.SplitLast(delims, options);

            Assert.Equal(actual, expected);
        }
        
        [Theory]
        [InlineData("a.b.c", new []{'.'},        1, StringSplitOptions.None,               new []{"a.b", "c"})]
        [InlineData("a.b.c", new []{'.'},        1, StringSplitOptions.RemoveEmptyEntries, new []{"a.b", "c"})]
        [InlineData("a.b.c", new []{'.'},        2, StringSplitOptions.None,               new []{"a", "b", "c"})]
        [InlineData("a.b.c", new []{'.'},        2, StringSplitOptions.RemoveEmptyEntries, new []{"a", "b", "c"})]
        [InlineData("a,b.c", new []{'.',','},    2, StringSplitOptions.None,               new []{"a", "b", "c"})]
        [InlineData("a,b.c", new []{'.',','},    2, StringSplitOptions.RemoveEmptyEntries, new []{"a", "b", "c"})]
        [InlineData(",b.,d.",   new []{'.',','}, 4, StringSplitOptions.None,               new []{"", "b", "","d",""})]
        [InlineData(",b.,d.",   new []{'.',','}, 4, StringSplitOptions.RemoveEmptyEntries, new []{"b","d"})]
        public void SplitLast_AllParams(string inputStr, char[] delims, int count, StringSplitOptions options, string[] expected)
        {
            var actual = inputStr.SplitLast(delims, count, options);

            Assert.Equal(actual, expected);
        }

        [Theory]
        [InlineData(null,    new []{'.'}, 1,  typeof(NullReferenceException))]
        [InlineData("a.b.c", null,        1,  typeof(ArgumentNullException))]
        [InlineData("a.b.c", new []{'.'}, -1, typeof(ArgumentException))]
        public void SplitLast_Exceptions(string inputStr, char[] delims, int count, Type expectedException)
        {
            Assert.Throws(expectedException, () => inputStr.SplitLast(delims, count));
        }
        #endregion
    }
}
