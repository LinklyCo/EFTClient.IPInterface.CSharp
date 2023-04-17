using System;
using System.Collections.Generic;
using System.Text;
using PCEFTPOS.EFTClient.IPInterface;
using Xunit;

namespace IPInterface.Test
{
    public class DecimalExtensionTest
    {
        [Theory]
        [InlineData(1.45, 5, "00145")]
        [InlineData(5, 4, "0500")]
        [InlineData(-3.33, 8, "-0000333")]
        public void PadLeftAsInt_ReturnsPaddedStringOfDecimalAsInt(decimal v, int totalWidth, string expected)
        {
            var actual = v.PadLeftAsInt(totalWidth);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(1.45, 1)]
        [InlineData(1.45, 0)]
        [InlineData(1.45, -3)]
        [InlineData(-2.2, 0)]
        [InlineData(-2, -8)]
        public void PadLeftAsInt_WidthLessThanRequired_ThrowsArgumentOutOfRange(decimal v, int totalWidth)
        {
            
            Action actual = () => v.PadLeftAsInt(totalWidth);

            Assert.Throws<ArgumentOutOfRangeException>(actual);
        }
    }
}
