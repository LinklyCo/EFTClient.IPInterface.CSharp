using System;
using System.Collections.Generic;
using System.Text;
using PCEFTPOS.EFTClient.IPInterface;
using Xunit;

namespace IPInterface.Test
{
    public class IntegerExtensionTest
    {
        [Theory]
        [InlineData(145, 5, "00145")]
        [InlineData(5, 4, "0005")]
        [InlineData(-333, 5, "-0333")]
        public void PadLeft_ReturnsPaddedString(int v, int totalWidth, string expected)
        {
            var actual = v.PadLeft(totalWidth);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(145, 1)]
        [InlineData(145, 0)]
        [InlineData(145, -3)]
        [InlineData(-2, 0)]
        [InlineData(-2, -8)]
        public void PadLeft_WidthLessThanRequired_ThrowsArgumentOutOfRange(int v, int width)
        {
            Action actual = () => v.PadLeft(width);

            Assert.Throws<ArgumentOutOfRangeException>(actual);
        }
    }
}
