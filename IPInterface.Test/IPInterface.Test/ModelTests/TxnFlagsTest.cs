using PCEFTPOS.EFTClient.IPInterface;
using Xunit;

namespace IPInterface.Test
{
    public class TxnFlagsTest
    {
        [Theory]
        // A variety of standard cases to ensure that no field is getting it's value from an incorrect position in the flag array
        [InlineData(false, true, CardEntryType.Contactless, CommsMethodType.X25, CurrencyStatus.AUD, PayPassStatus.NotSet, '$', '%'
        , new char[] { '0', '1', 'C', '3', '0', ' ', '$', '%'  })]
        [InlineData(true, false, CardEntryType.BarCode, CommsMethodType.X25, CurrencyStatus.Converted, PayPassStatus.PayPassUsed, '$', '%'
            , new char[] { '1', '0', 'B', '3', '1', '1', '$', '%' })]
        [InlineData(true, true, CardEntryType.BarCode, CommsMethodType.X25, CurrencyStatus.AUD, PayPassStatus.PayPassUsed, '$', '%'
            , new char[] { '1', '1', 'B', '3', '0', '1', '$', '%' })]
        [InlineData(true, true, CardEntryType.BarCode, CommsMethodType.X25, CurrencyStatus.Converted, PayPassStatus.PayPassNotUsed, '$', '%'
            , new char[] { '1', '1', 'B', '3', '1', '0', '$', '%' })]
        // use of values other than '0' and '1' for offline and receipt printed, should be false in object
        [InlineData(false, false, CardEntryType.BarCode, CommsMethodType.X25, CurrencyStatus.Converted, PayPassStatus.PayPassNotUsed, '$', '%'
            , new char[] { 'T', '9', 'B', '3', '1', '0', '$', '%' })]
        public void TxnFlags_Ctr_SetCorrectly(bool offline, bool receiptPrinted, CardEntryType cardEntry,
            CommsMethodType commsMethod, CurrencyStatus ccy, PayPassStatus payPass, char undf6, char undf7,
            char [] inputFlags)
        {
            var actual = new TxnFlags(inputFlags);

            Assert.Equal(offline, actual.Offline);
            Assert.Equal(receiptPrinted, actual.ReceiptPrinted);
            Assert.Equal(cardEntry, actual.CardEntry);
            Assert.Equal(commsMethod, actual.CommsMethod);
            Assert.Equal(ccy, actual.Currency);
            Assert.Equal(payPass, actual.PayPass);
            Assert.Equal(undf6, actual.UndefinedFlag6);
            Assert.Equal(undf7, actual.UndefinedFlag7);
        }

    }
}
