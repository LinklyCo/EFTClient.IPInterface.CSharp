using System.Collections.Generic;
using PCEFTPOS.EFTClient.IPInterface;
using Xunit;

namespace IPInterface.Test
{
    public class BasketDataTest
    {
        #region EFTBasketItem

        #region EFTBasketItem.Name Set

        [Theory]
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ")] // 24 chars, won't truncate
        [InlineData("ABCDEFGHIJKLMNOPQRSTUVWXYZ123456789")] // 33 chars, will
        [InlineData("")] // empty

        public void EFTBasketItem_SetName_TruncatesTo24Chars(string input)
        {
            EFTBasketItem item = new EFTBasketItem();
            var expectedLen = input.Length > 24 ? 24 : input.Length;
            var expectedValue = input.Substring(0, expectedLen);

            item.Name = input;

            Assert.Equal(expectedLen, item.Name.Length);
            Assert.Equal(expectedValue, item.Name);
        }

        [Fact]
        public void EFTBasketItem_SetName_NullInput_NoThrow()
        {
            EFTBasketItem item = new EFTBasketItem();
            item.Name = null;

            Assert.Null(item.Name);
        }
        #endregion

        #region EFTBasketItem.Description Set

        private const string TwoHundredFiftyFiveCharStr = @"l81U2yPc0jtpvm0HUFbfmemm6GAgxCYnQbC0WbFsDYwQ91NBlSabs" +
                                                          "6JyITmIjTLWN8zd5F1ktzlt8frNxvPBeSvePLsnm6vSbDczqTpUEGRX2pXWqDO" +
                                                          "0e2vzWK8bKieflUqPxKEXFummt8pysHAYPvjZVKWPRzNH7LdFfCOTpyh73uAz6" +
                                                          "0WupIdwd27pINf1NoYBbCaZBI6NN73xHc31aK5SUsLyD9FlFpAMsJt6h2et3Ff" +
                                                          "w3FqhSJlVmmVazAW";

        [Theory]
        [InlineData(TwoHundredFiftyFiveCharStr)] // 255 chars, won't truncate
        [InlineData(TwoHundredFiftyFiveCharStr + "12345")] //260 chars, will
        [InlineData("")] // empty
        public void EFTBasketItem_SetDescription_TruncatesTo255Chars(string input)
        {
            EFTBasketItem item = new EFTBasketItem();
            var expectedLen = input.Length > 255 ? 255 : input.Length;
            var expectedValue = input.Substring(0, expectedLen);

            item.Description = input;

            Assert.Equal(expectedLen, item.Description.Length);
            Assert.Equal(expectedValue, item.Description);
        }

        [Fact]
        public void EFTBasketItem_SetDescription_NullInput_NoThrow()
        {
            EFTBasketItem item = new EFTBasketItem();
            item.Name = null;

            Assert.Null(item.Name);
        }

        #endregion

        #region EFTBasketItem.Tag Get
        [Theory]
        [InlineData("A", new string[] { "A" })]
        [InlineData("A,B,C", new string[] { "A", "B", "C" })]
        [InlineData("", new string[] { "" })]
        public void EFTBasketItem_TagGet_ReturnsTagsInCsvStr(string expected, string[] tags)
        {
            EFTBasketItem item = new EFTBasketItem();
            item.TagList = new List<string>(); // is initialised as null, // todo should this be set in ctor?

            foreach (var tag in tags)
                item.TagList.Add(tag);

            Assert.Equal(expected, item.Tag);
        }

        #endregion

        #endregion
    }
}
