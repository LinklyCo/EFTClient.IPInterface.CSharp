using System;
using System.Collections.Generic;
using System.Text;
using PCEFTPOS.EFTClient.IPInterface;
using Xunit;

namespace IPInterface.Test
{
    public class ExtensionTest
    {
        [Theory]
        [InlineData("", AccountType.Default)] // empty string, should return default
        [InlineData(null, AccountType.Default)] // null string, should return default
        [InlineData("Sasquatch", AccountType.Default)] // invalid string (not account type), should return default
        [InlineData("credit", AccountType.Credit)] // valid string (credit), should return credit
        [InlineData("CrEdIt", AccountType.Credit)] // valid string (credit) (mixed case), should return credit
        [InlineData("savings", AccountType.Savings)] // valid string (savings), should return savings
        [InlineData("SaViNgS", AccountType.Savings)] // valid string (savings) (mixed case), should return savings
        [InlineData("cheque", AccountType.Cheque)] // valid string (cheque), should return cheque
        [InlineData("ChEqUe", AccountType.Cheque)] // valid string (cheque) (mixed case), should return cheque
        public void FromString_ReturnsAccountType(string inputStr, AccountType expected)
        {
            AccountType t = AccountType.Default;
            var actual = t.FromString(inputStr);

            Assert.Equal(expected, actual);
        }
    }
}
