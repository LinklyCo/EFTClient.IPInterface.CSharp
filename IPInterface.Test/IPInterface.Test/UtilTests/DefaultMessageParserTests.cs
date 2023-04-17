using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using PCEFTPOS.EFTClient.IPInterface;
using PCEFTPOS.EFTClient.IPInterface.Slave;
using Xunit;

namespace IPInterface.Test
{
    public class DefaultMessageParserTests
    {
        #region EFTLogonRequest

        [Fact]
        public void EFTRequestToString_EFTLogonRequest_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTLogonRequest()
            {
                PurchaseAnalysisData = new PadField("XXX001aYYY003ABCZZZ00245"),
                Application = TerminalApplication.GiftCard,
                ReceiptAutoPrint = ReceiptPrintModeType.PinpadPrinter,
                CutReceipt = ReceiptCutModeType.Cut,
                Merchant = "TT"
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTLogonRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTLogonRequest_MatchesString(EFTLogonRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // command code is 'G' for logon
            Assert.Equal(EFTRequestCommandCode.Logon, actual[5]);

            // sub code
            Assert.Equal((char) expected.LogonType, actual[6]);

            // Merchant
            Assert.Equal(expected.Merchant.PadRightAndCut(2), actual.Substring(7, 2));

            // ReceiptAutoPrint
            Assert.Equal((char) expected.ReceiptAutoPrint, actual[9]);

            // CutReceipt
            Assert.Equal((char) expected.CutReceipt, actual[10]);

            // App
            Assert.Equal(expected.Application.ToApplicationString(), actual.Substring(11, 2));

            // PAD
            Assert.True(PadFieldIsValid(actual.Substring(13, actual.Length - 13)));

        }

        #endregion

        #region EFTReprintReceiptRequest

        [Fact]
        public void EFTRequestToString_EFTReprintReceiptRequest_NoOriginalTXn_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTReprintReceiptRequest()
            {
                Application = TerminalApplication.GiftCard,
                ReprintType = ReprintType.GetLast,
                ReceiptAutoPrint = ReceiptPrintModeType.PinpadPrinter,
                CutReceipt = ReceiptCutModeType.Cut,
                Merchant = "TT"
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTReprintReceiptRequest_MatchesString(expected, actual);
        }

        [Fact]
        public void EFTRequestToString_EFTReprintReceiptRequest_WithOriginalTXn_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTReprintReceiptRequest()
            {
                Application = TerminalApplication.GiftCard,
                ReprintType = ReprintType.GetLast,
                ReceiptAutoPrint = ReceiptPrintModeType.PinpadPrinter,
                CutReceipt = ReceiptCutModeType.Cut,
                Merchant = "TT",
                OriginalTxnRef = "ABCDEFGHIJKLMNOP"
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTReprintReceiptRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTReprintReceiptRequest_MatchesString(EFTReprintReceiptRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // command code is 'C' for ReprintReceipt
            Assert.Equal(EFTRequestCommandCode.ReprintReceipt, actual[5]);

            // sub code
            Assert.Equal((char) expected.ReprintType, actual[6]);

            // Merchant
            Assert.Equal(expected.Merchant.PadRightAndCut(2), actual.Substring(7, 2));

            // CutReceipt
            Assert.Equal((char) expected.CutReceipt, actual[9]);

            // ReceiptAutoPrint
            Assert.Equal((char) expected.ReceiptAutoPrint, actual[10]);

            // App
            Assert.Equal(expected.Application.ToApplicationString(), actual.Substring(11, 2));

            // Original TXN
            if (expected.OriginalTxnRef.Length > 0)
                Assert.Equal(expected.OriginalTxnRef.PadRightAndCut(16), actual.Substring(13, 16));
            else
                Assert.Equal(13, actual.Length); // string should stop if no Original TXN

        }

        #endregion

        #region EFTGetLastTransaction

        [Fact]
        public void EFTRequestToString_EFTGetLastTransactionRequest_NoOriginalTXn_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTGetLastTransactionRequest()
            {
                Application = TerminalApplication.GiftCard,
                Merchant = "TT",
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTGetLastTransactionRequest_MatchesString(expected, actual);
        }

        [Fact]
        public void EFTRequestToString_EFTGetLastTransactionRequest_WithOriginalTXn_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTGetLastTransactionRequest("ABCDEFGHIJKLMNOP")
            {
                Application = TerminalApplication.GiftCard,
                Merchant = "TT",
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTGetLastTransactionRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTGetLastTransactionRequest_MatchesString(EFTGetLastTransactionRequest expected,
            string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // command code is 'N' for GetLastTransaction
            Assert.Equal(EFTRequestCommandCode.GetLastTransaction, actual[5]);

            // sub code, always  0 for GetLastTransaction
            Assert.Equal('0', actual[6]);

            // App
            Assert.Equal(expected.Application.ToApplicationString(), actual.Substring(7, 2));

            // Merchant
            Assert.Equal(expected.Merchant.PadRightAndCut(2), actual.Substring(9, 2));


            // OriginalTxnRef, optional
            if (expected.TxnRef.Length > 0)
                Assert.Equal(expected.TxnRef.PadRightAndCut(16), actual.Substring(11, 16));
            else
                Assert.Equal(11, actual.Length); // string should stop if no Original TXN
        }

        #endregion

        #region SetDialogRequest

        [Fact]
        public void EFTRequestToString_SetDialogRequest_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new SetDialogRequest()
            {
                DialogPosition = DialogPosition.BottomRight,
                DialogX = 111,
                DialogY = 222,
                DialogType = DialogType.TouchScreen,
                DialogTitle = "Test",
                DisableDisplayEvents = true
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_SetDialogRequest_MatchesString(expected, actual);
        }

        [Fact]
        public void EFTRequestToString_SetDialogRequestWithTooLongFields_ReturnsCorrespondingStringWithFieldsCut()
        {
            var parser = new DefaultMessageParser();
            var expected = new SetDialogRequest()
            {
                DialogPosition = DialogPosition.BottomRight,
                DialogX = 111,
                DialogY = 222,
                DialogType = DialogType.TouchScreen,
                DialogTitle = "ABCDEFGHIJKLMNOPRSTUVWXYZABCDEFGHIJKLMNOPRSTUVWXYZ", //limited to 32
                DisableDisplayEvents = true
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_SetDialogRequest_MatchesString(expected, actual);
        }

        private void Assert_SetDialogRequest_MatchesString(SetDialogRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // command code is '2' for GetLastTransaction
            Assert.Equal(EFTRequestCommandCode.SetDialog, actual[5]);

            // sub code, is '5' if DisableDisplayEvents is true, otherwise is ' ' (space)
            if (expected.DisableDisplayEvents)
                Assert.Equal('5', actual[6]);
            else
                Assert.Equal(' ', actual[6]);

            // Type
            Assert.Equal((char) expected.DialogType, actual[7]);

            // DialogX
            Assert.True(int.TryParse(actual.Substring(8, 4), out var x));
            Assert.Equal(expected.DialogX, x);

            // DialogY
            Assert.True(int.TryParse(actual.Substring(12, 4), out var y));
            Assert.Equal(expected.DialogY, y);

            // DialogPosition
            Assert.Equal(expected.DialogPosition.ToString().PadRightAndCut(12), actual.Substring(16, 12));

            // TopMost
            Assert.Equal(expected.EnableTopmost ? '1' : '0', actual[28]);

            // DialogTitle
            Assert.Equal(expected.DialogTitle.PadRightAndCut(32), actual.Substring(29, 32));

        }

        #endregion

        #region EFTQueryCardRequest

        [Fact]
        public void EFTRequestToString_EFTQueryCard_WithOriginalTXn_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTQueryCardRequest()
            {
                Application = TerminalApplication.GiftCard,
                Merchant = "FG",
                QueryCardType = QueryCardType.PreSwipeDeposit,
                PurchaseAnalysisData = new PadField("XXX001aYYY003ABCZZZ00245")
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTQueryCardRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTQueryCardRequest_MatchesString(EFTQueryCardRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // command code is 'J' for QueryCard
            Assert.Equal(EFTRequestCommandCode.QueryCard, actual[5]);

            // SubCode
            Assert.Equal((char) expected.QueryCardType, actual[6]);

            // App
            Assert.Equal(expected.Application.ToApplicationString(), actual.Substring(7, 2));

            // Merchant
            Assert.Equal(expected.Merchant.PadRightAndCut(2), actual.Substring(9, 2));

            // PAD
            Assert.True(PadFieldIsValid(actual.Substring(11, actual.Length - 11)));
        }

        #endregion

        #region EFTTransactionRequest

        [Fact]
        public void EFTRequestToString_EFTTransactionRequest_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var dt = new DateTime(2022, 3, 4, 5, 6, 7);
            var expected = new EFTTransactionRequest()
            {
                Date = dt,
                Time = dt,
                TrainingMode = true,
                EnableTip = true,
                AmtCash = 100,
                AmtPurchase = 200,
                TxnRef = dt.ToString("yyMMddHHmmssffff"),
                PanSource = PanSource.POSSwiped,
                Pan = "ABCDEFG",
                DateExpiry = "0125",
                Track2 = "Track2Test",
                AccountType = AccountType.Credit,
                RRN = "ABCDEFGHIJKL",
                PurchaseAnalysisData = new PadField("XXX001aYYY003ABCZZZ00245"),
                Application = TerminalApplication.GiftCard,
                ReceiptAutoPrint = ReceiptPrintModeType.PinpadPrinter,
                CutReceipt = ReceiptCutModeType.Cut
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTTransactionRequest_MatchesString(expected, actual);
        }


        [Fact]
        public void EFTRequestToString_EFTTransactionRequest_DataFieldIgnoredUnlessAuthPinOrEnhancedPin()
        {
            var parser = new DefaultMessageParser();
            var dt = new DateTime(2022, 3, 4, 5, 6, 7);
            var expected = new EFTTransactionRequest()
            {
                Date = dt,
                Time = dt,
                TrainingMode = true,
                EnableTip = true,
                AmtCash = 100,
                AmtPurchase = 200,
                TxnRef = dt.ToString("yyMMddHHmmssffff"),
                PanSource = PanSource.POSSwiped,
                Pan = "ABCDEFG",
                DateExpiry = "0125",
                Track2 = "Track2Test",
                AccountType = AccountType.Credit,
                RRN = "ABCDEFGHIJKL",
                PurchaseAnalysisData = new PadField("XXX001aYYY003ABCZZZ00245"),
                Application = TerminalApplication.GiftCard,
                ReceiptAutoPrint = ReceiptPrintModeType.PinpadPrinter,
                CutReceipt = ReceiptCutModeType.Cut,
                DataField = "Test Data Field Data Here"
            };

            foreach (var t in Enum.GetValues(typeof(TransactionType)).Cast<TransactionType>())
            {
                // skip the two types the *should* include the data field
                if (t == TransactionType.AuthPIN || t == TransactionType.EnhancedPIN)
                    continue;

                expected.TxnType = t;
                var actual = parser.EFTRequestToString(expected);
                Assert_EFTTransactionRequest_MatchesString(expected, actual);

                // Assert the data field is not contained
                Assert.DoesNotContain(actual, expected.DataField);
            }
        }

        [Fact]
        public void EFTRequestToString_EFTTransactionRequest_X_DataFieldIncludedRightPropertiesSkipped()
        {
            var parser = new DefaultMessageParser();
            var dt = new DateTime(2022, 3, 4, 5, 6, 7);
            var padStr = "XXX001aYYY003ABCZZZ00245";
            var expected = new EFTTransactionRequest()
            {
                Date = dt,
                Time = dt,
                TrainingMode = true,
                EnableTip = true,
                AmtCash = 100,
                AmtPurchase = 200,
                TxnRef = dt.ToString("yyMMddHHmmssffff"),
                PanSource = PanSource.POSSwiped,
                Pan = "ABCDEFG",
                DateExpiry = "0125",
                Track2 = "Track2Test",
                AccountType = AccountType.Credit,
                RRN = "ABCDEFGHIJKL",
                PurchaseAnalysisData = new PadField(padStr),
                Application = TerminalApplication.GiftCard,
                ReceiptAutoPrint = ReceiptPrintModeType.PinpadPrinter,
                CutReceipt = ReceiptCutModeType.Cut,
                DataField = "Test Data Field Data Here",
                TxnType = TransactionType.AuthPIN,
                CurrencyCode = "ZZZ"
            };

            var actual = parser.EFTRequestToString(expected);
            Assert_EFTTransactionRequest_MatchesString(expected, actual);
            Assert.DoesNotContain(padStr, actual);
            Assert.DoesNotContain(expected.CurrencyCode.PadRightAndCut(3), actual);
            Assert.Contains(expected.DataField, actual);
        }


        [Fact]
        public void EFTRequestToString_EFTTransactionRequest_K_DataFieldIncludedRightPropertiesSkipped()
        {
            var parser = new DefaultMessageParser();
            var dt = new DateTime(2022, 3, 4, 5, 6, 7);
            var padStr = "XXX001aYYY003ABCZZZ00245";
            var expected = new EFTTransactionRequest()
            {
                Date = dt,
                Time = dt,
                TrainingMode = true,
                EnableTip = true,
                AmtCash = 100,
                AmtPurchase = 200,
                TxnRef = dt.ToString("yyMMddHHmmssffff"),
                PanSource = PanSource.POSSwiped,
                Pan = "ABCDEFG",
                DateExpiry = "0125",
                Track2 = "Track2Test",
                AccountType = AccountType.Credit,
                RRN = "ABCDEFGHIJKL",
                PurchaseAnalysisData = new PadField(padStr),
                Application = TerminalApplication.GiftCard,
                ReceiptAutoPrint = ReceiptPrintModeType.PinpadPrinter,
                CutReceipt = ReceiptCutModeType.Cut,
                DataField = "Test Data Field Data Here",
                TxnType = TransactionType.EnhancedPIN,
                CurrencyCode = "ZZZ"
            };

            var actual = parser.EFTRequestToString(expected);
            Assert_EFTTransactionRequest_MatchesString(expected, actual);
            Assert.DoesNotContain(padStr, actual);
            Assert.DoesNotContain( expected.CurrencyCode.PadRightAndCut(3), actual);
            Assert.Contains(expected.DataField, actual);
        }


        [Fact]
        public void EFTRequestToString_EFTTransactionRequestWithTooLongFields_ReturnsCorrespondingStringWithFieldsCut()
        {
            var parser = new DefaultMessageParser();
            var dt = new DateTime(2022, 3, 4, 5, 6, 7);
            var expected = new EFTTransactionRequest()
            {
                Date = dt,
                Time = dt,
                TrainingMode = true,
                EnableTip = true,
                AmtCash = 100.12345m,
                AmtPurchase = 200.45678m,
                TxnRef = dt.ToString("yyMMddHHmmssffff"),
                PanSource = PanSource.POSSwiped,
                Pan = "ABCDEFGHIJKLMNOPRSTUVWXYZ", // limited to 20
                DateExpiry = "0125",
                Track2 = "ABCDEFGHIJKLMNOPRSTUVWXYZABCDEFGHIJKLMNOPRSTUVWXYZ", // limited to 40
                AccountType = AccountType.Credit,
                RRN = "ABCDEFGHIJKLMNOPRSTUVWXYZ", // limited to 12
                PurchaseAnalysisData = new PadField("XXX001aYYY003ABCZZZ00245"),
                Application = TerminalApplication.GiftCard,
                ReceiptAutoPrint = ReceiptPrintModeType.PinpadPrinter,
                CutReceipt = ReceiptCutModeType.Cut
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTTransactionRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTTransactionRequest_MatchesString(EFTTransactionRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // command code is 'W' for PINAuth types but is otherwise the standard 'M'
            if (expected.TxnType == TransactionType.AuthPIN || expected.TxnType == TransactionType.EnhancedPIN)
                Assert.Equal(EFTRequestCommandCode.PINAuth, actual[5]);
            else
                Assert.Equal(EFTRequestCommandCode.Transaction, actual[5]);

            // SubCode always 0 for Transactions
            Assert.Equal('0', actual[6]);

            // TxnType
            Assert.Equal((char)expected.TxnType, actual[9]);

            // TrainingMode
            Assert.Equal(expected.TrainingMode ? '1' : '0', actual[10]);

            // EnableTip
            Assert.Equal(expected.EnableTip ? '1' : '0', actual[11]);

            // AmtCash
            // Have to do this pad left as int nonsense to handle decimals with more than two decimal digits
            // which are technically possible inputs
            Assert.True(decimal.TryParse(actual.Substring(12, 9), out var amtCash));
            Assert.Equal(expected.AmtCash.PadLeftAsInt(9), actual.Substring(12, 9));

            // AmtPurchase
            // Have to do this pad left as int nonsense to handle decimals with more than two decimal digits
            // which are technically possible inputs
            Assert.True(decimal.TryParse(actual.Substring(21, 9), out var amtPurchase));
            Assert.Equal(expected.AmtPurchase.PadLeftAsInt(9), actual.Substring(21, 9));


            // auth code
            Assert.True(int.TryParse(actual.Substring(30, 6), out var authCode));
            Assert.Equal(expected.AuthCode, authCode);

            // TxnRef
            Assert.Equal(expected.TxnRef.PadRightAndCut(16), actual.Substring(36, 16));

            // ReceiptAutoPrint
            Assert.Equal((char)expected.ReceiptAutoPrint, actual[52]);

            // CutReceipt
            Assert.Equal((char)expected.CutReceipt, actual[53]);

            // PanSource
            Assert.Equal((char)expected.PanSource, actual[54]);

            // TxnRef
            Assert.Equal(expected.Pan.PadRightAndCut(20), actual.Substring(55, 20));

            // DateExpiry
            Assert.Equal(expected.DateExpiry.PadRightAndCut(4), actual.Substring(75, 4));

            // Track2
            Assert.Equal(expected.Track2.PadRightAndCut(40), actual.Substring(79, 40));

            // AccountType
            Assert.Equal((char)expected.AccountType, actual[119]);

            // App
            Assert.Equal(expected.Application.ToApplicationString(), actual.Substring(120, 2));

            // RRN
            Assert.Equal(expected.RRN.PadRightAndCut(12), actual.Substring(122, 12));
            
            if (expected.TxnType == TransactionType.AuthPIN || expected.TxnType == TransactionType.EnhancedPIN)
            {
                // DataField
                Assert.True(DataFieldIsValid(actual.Substring(134, actual.Length - 134)));
            }
            else
            {
                // CurrencyCode
                Assert.Equal(expected.CurrencyCode.PadRightAndCut(3), actual.Substring(134, 3));

                // OriginalTxnType
                Assert.Equal((char)expected.OriginalTxnType, actual[137]);


                // Date should be valid or null
                var dateStr = actual.Substring(138, 6);
                if (DateTime.TryParseExact(dateStr, "ddMMyy", null, DateTimeStyles.None,
                        out var date))
                {
                    Assert.Equal(expected.Date?.Date, date.Date);
                }
                else
                {
                    Assert.Null(expected.Date);
                    Assert.Equal("      ", dateStr);
                }

                // Time
                var timeStr = actual.Substring(144, 6);
                if (DateTime.TryParseExact(timeStr, "HHmmss", null, DateTimeStyles.None,
                        out var time))
                {
                    Assert.Equal(expected.Time?.TimeOfDay, time.TimeOfDay);
                }
                else
                {
                    Assert.Null(expected.Date);
                    Assert.Equal("      ", actual.Substring(144, 6));
                }

                // PAD
                Assert.True(PadFieldIsValid(actual.Substring(158, actual.Length - 158)));
            }
        }

        #endregion

        #region EFTControlPanelRequest
        [Fact]
        public void EFTRequestToString_EFTControlPanelRequest_WithOriginalTXn_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTControlPanelRequest()
            {
                ReceiptAutoPrint = ReceiptPrintModeType.MerchantInternalEFTClientPrinter,
                CutReceipt = ReceiptCutModeType.Cut,
                ReturnType = ControlPanelReturnType.ImmediatelyAndWhenClosed,
                ControlPanelType = ControlPanelType.Status
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTControlPanelRequest_MatchesString(expected, actual);
        }
        private void Assert_EFTControlPanelRequest_MatchesString(EFTControlPanelRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // command code is '5' for DisplayControlPanel
            Assert.Equal(EFTRequestCommandCode.DisplayControlPanel, actual[5]);

            // ControlPanelType
            Assert.Equal((char)expected.ControlPanelType, actual[6]);

            // ReceiptPrintMode
            Assert.Equal((char)expected.ReceiptAutoPrint, actual[7]);

            // CutReceipt
            Assert.Equal((char)expected.CutReceipt, actual[8]);

            // ReturnType
            Assert.Equal((char)expected.ReturnType, actual[9]);

            // total length
            Assert.Equal(10, actual.Length);
        }

        #endregion

        #region EftSettlementRequest
        [Fact]
        public void EFTRequestToString_EFTSettlementRequest_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTSettlementRequest()
            {
                PurchaseAnalysisData = new PadField("XXX001aYYY003ABCZZZ00245"),
                Application = TerminalApplication.GiftCard,
                ReceiptAutoPrint = ReceiptPrintModeType.PinpadPrinter,
                CutReceipt = ReceiptCutModeType.Cut,
                Merchant = "MM",
                ResetTotals = false,
                SettlementType = SettlementType.Settlement
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTSettlementRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTSettlementRequest_MatchesString(EFTSettlementRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // command code is 'P' for Settlement
            Assert.Equal(EFTRequestCommandCode.Settlement, actual[5]);

            // SubCode
            Assert.Equal((char)expected.SettlementType, actual[6]);

            // Merchant
            Assert.Equal(expected.Merchant.PadRightAndCut(2), actual.Substring(7, 2));

            // ReceiptPrintMode
            Assert.Equal((char)expected.ReceiptAutoPrint, actual[9]);

            // CutReceipt
            Assert.Equal((char)expected.CutReceipt, actual[10]);

            // ResetTotals
            Assert.Equal(expected.ResetTotals ? '1' : '0', actual[11]);

            // App
            Assert.Equal(expected.Application.ToApplicationString(), actual.Substring(12, 2));


            // PAD
            Assert.True(PadFieldIsValid(actual.Substring(14, actual.Length - 14)));
        }

        #endregion

        #region EftConfigureMerchantRequest
        [Fact]
        public void EFTRequestToString_EFTConfigureMerchangeRequest_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTConfigureMerchantRequest()
            {
                Application = TerminalApplication.Agency,
                Caid = "ABCDEFGHIJK",
                Catid = "123456",
                Timeout = 123,
                AIIC = 987654321,
                Merchant = "MM",
                NII = 777
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTConfigureMerchantRequest_MatchesString(expected, actual);
        }

        [Fact]
        public void EFTRequestToString_EFTConfigureMerchangeRequestWithTooLongFields_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTConfigureMerchantRequest()
            {
                Application = TerminalApplication.Agency,
                Caid = "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                Catid = "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                Timeout = 123,
                AIIC = 987654321,
                Merchant = "ABCDEFGHIJKLMNOPQRSTUVWXYZ",
                NII = 777
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTConfigureMerchantRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTConfigureMerchantRequest_MatchesString(EFTConfigureMerchantRequest expected,
            string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // command code is '$' for ConfigureMerchant
            Assert.Equal(EFTRequestCommandCode.ConfigureMerchant, actual[5]);

            // SubCode
            Assert.Equal('0', actual[6]);

            // Merchant
            Assert.Equal(expected.Merchant.PadRightAndCut(2), actual.Substring(7, 2));

            // AIIC
            Assert.True(int.TryParse(actual.Substring(9, 11), out var aaic));
            Assert.Equal(expected.AIIC, aaic);

            // Nii
            Assert.True(int.TryParse(actual.Substring(20, 3), out var nii));
            Assert.Equal(expected.NII, nii);

            // Caid
            Assert.Equal(expected.Caid.PadRightAndCut(15), actual.Substring(23, 15));

            // Catid
            Assert.Equal(expected.Catid.PadRightAndCut(8), actual.Substring(38, 8));

            // Timeout
            Assert.True(int.TryParse(actual.Substring(46, 3), out var timeout));
            Assert.Equal(expected.Timeout, timeout);

            // App
            Assert.Equal(expected.Application.ToApplicationString(), actual.Substring(49, 2));
        }

        #endregion

        #region EFTStatusRequest

        [Fact]
        public void EFTRequestToString_EFTStatusRequestRequest_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTStatusRequest()
            {
                Application = TerminalApplication.Agency,
                Merchant = "MM",
                StatusType = StatusType.AppNameTable
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTStatusRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTStatusRequest_MatchesString(EFTStatusRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // command code is 'K' for Status
            Assert.Equal(EFTRequestCommandCode.Status, actual[5]);

            // SubCode
            Assert.Equal((char)expected.StatusType, actual[6]);

            // Merchant
            Assert.Equal(expected.Merchant.PadRightAndCut(2), actual.Substring(7, 2));

            // App
            Assert.Equal(expected.Application.ToApplicationString(), actual.Substring(9, 2));
        }

        #endregion

        #region EFTChequeAuthRequest

        [Fact]
        public void EFTRequestToString_EFTChequeAuthRequestRequestWithTooLongFields_ReturnsCorrespondingStringWithFieldsCut()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTChequeAuthRequest()
            {
                BranchCode = "ZZZZZZZZZZ", // limited to 6
                AccountNumber = "0123456789101112131415161718192021", // limited to 14
                SerialNumber = "abcdefghijklmnopqrstuvwxyz", // limited to 14
                Application = TerminalApplication.GiftCard,
                Amount = 1098765.4321m, // limited to 9
                ChequeType = ChequeType.PersonalAppraisal,
                ReferenceNumber = "ABCDEFGHIJKLMNOPQRSTUVWXYZ" // limited to 12
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTChequeAuthRequest_MatchesString(expected, actual);
        }

        [Fact]
        public void EFTRequestToString_EFTChequeAuthRequestRequest_ReturnsCorresponding()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTChequeAuthRequest()
            {
                BranchCode = "XXYYZZ", // limited to 6
                AccountNumber = "123456789ABCDE", // limited to 14
                SerialNumber = "123456789ABCDE", // limited to 14
                Application = TerminalApplication.GiftCard,
                Amount = 123.456m,
                ChequeType = ChequeType.PersonalAppraisal,
                ReferenceNumber = "123456789ABC" // limited to 12
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTChequeAuthRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTChequeAuthRequest_MatchesString(EFTChequeAuthRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // command code is 'H' for ChequeAuth
            Assert.Equal(EFTRequestCommandCode.ChequeAuth, actual[5]);

            // SubCode, always '0' for Cheque auth
            Assert.Equal('0', actual[6]);

            // App
            Assert.Equal(expected.Application.ToApplicationString(), actual.Substring(7, 2));

            // Txn Type always ' ' (space) for Cheque Auth
            Assert.Equal(' ', actual[9]);

            // BranchCode
            Assert.Equal(expected.BranchCode.PadRightAndCut(6), actual.Substring(10, 6));

            // Account Number
            Assert.Equal(expected.AccountNumber.PadRightAndCut(14), actual.Substring(16, 14));

            // SerialNumber
            Assert.Equal(expected.SerialNumber.PadRightAndCut(14), actual.Substring(30, 14));

            // Amount
            Assert.True(decimal.TryParse(actual.Substring(44, 9), out var amt));
            Assert.Equal(expected.Amount.PadLeftAsInt(9), actual.Substring(44, 9));

            // Cheque Type
            Assert.Equal((char)expected.ChequeType, actual[53]);

            // ReferenceNumber
            Assert.Equal(expected.ReferenceNumber.PadRightAndCut(12), actual.Substring(54, 12));
        }

        #endregion

        #region EFTGetPasswordRequest

        [Fact]
        public void EFTRequestToString_EFTGetPasswordRequest_ReturnsCorresponding()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTGetPasswordRequest()
            {
                MaxPassworkLength = 33,
                MinPasswordLength = 55,
                PasswordDisplay = PasswordDisplay.Enter_Code,
                Timeout = 444
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTGetPasswordRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTGetPasswordRequest_MatchesString(EFTGetPasswordRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // EFTGetPasswordRequest uses Generic ('X') for the command code
            Assert.Equal(EFTRequestCommandCode.Generic, actual[5]);

            // Command type should be get password
            Assert.Equal((char)CommandType.GetPassword, actual[6]);

            // min password length
            Assert.True(int.TryParse(actual.Substring(7, 2), out var minPwLen));
            Assert.Equal(expected.MinPasswordLength, minPwLen);

            // max password length
            Assert.True(int.TryParse(actual.Substring(9, 2), out var maxPwLen));
            Assert.Equal(expected.MaxPassworkLength, maxPwLen);

            // Timeout
            Assert.True(int.TryParse(actual.Substring(11, 3), out var timeout));
            Assert.Equal(expected.Timeout, timeout);

            // password display
            Assert.Equal("0" + (char)expected.PasswordDisplay, actual.Substring(14, 2));
        }

        #endregion

        #region EFTSlaveRequest

        [Fact]
        public void EFTRequestToString_EFTSlaveRequest_ReturnsCorresponding()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTSlaveRequest()
            {
                RawCommand = "TestRawCommandShoobyDoobyDoo"
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTSlaveRequest_MatchesString(expected, actual);
        }
        private void Assert_EFTSlaveRequest_MatchesString(EFTSlaveRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // EFTSlaveRequest uses Generic ('X') for the command code
            Assert.Equal(EFTRequestCommandCode.Generic, actual[5]);

            // Command type should be Slave
            Assert.Equal((char)CommandType.Slave, actual[6]);

            // RawCommand
            Assert.Equal(expected.RawCommand, actual.Substring(7, actual.Length - 7));

        }

        #endregion

        #region EFTClientListRequest

        [Fact]
        public void EFTRequestToString_EFTClientListRequest_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTClientListRequest();

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTClientListRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTClientListRequest_MatchesString(EFTClientListRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // EFTClientListRequest uses 'Q'
            Assert.Equal(EFTRequestCommandCode.GetClientList, actual[5]);

            // Command type should be Slave
            Assert.Equal('0', actual[6]);
        }

        #endregion

        #region EFTCloudLogonRequest

        [Fact]
        public void EFTRequestToString_EFTCloudLogonRequest_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTCloudLogonRequest()
            {
                ClientID = "123456789",
                Password = "ABCDEFGHI",
                PairingCode = "abcdefghi"
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTCloudLogonRequest_MatchesString(expected, actual);
        }

        [Fact]
        public void EFTRequestToString_EFTCloudLogonRequest_WithTooLongFields_ReturnsCorrespondingStringWithFieldsCut()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTCloudLogonRequest()
            {
                ClientID = "123456789987654321", // limited to 16
                Password = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", // limited to 16
                PairingCode = "abcdefghijklmnopqrstuvwxyz" // limited to 16
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTCloudLogonRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTCloudLogonRequest_MatchesString(EFTCloudLogonRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // EFTCloudLogonRequest uses 'A'
            Assert.Equal(EFTRequestCommandCode.CloudLogon, actual[5]);

            // should be ' ' (space)
            Assert.Equal(' ', actual[6]);

            // ClientID
            Assert.Equal(expected.ClientID.PadRightAndCut(16), actual.Substring(7, 16));

            // Password
            Assert.Equal(expected.Password.PadRightAndCut(16), actual.Substring(23, 16));

            // Pair Code
            Assert.Equal(expected.PairingCode.PadRightAndCut(16), actual.Substring(39, 16));
        }

        #endregion

        #region EFTCloudPairRequest

        [Fact]
        public void EFTRequestToString_EFTCloudPairRequest_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTCloudPairRequest()
            {
                ClientID = "123456789",
                Password = "ABCDEFGHI",
                PairingCode = "abcdefghi"
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTCloudPairRequest_MatchesString(expected, actual);
        }

        [Fact]
        public void EFTRequestToString_EFTCloudPairRequest_WithTooLongFields_ReturnsCorrespondingStringWithFieldsCut()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTCloudPairRequest()
            {
                ClientID = "123456789987654321", // limited to 16
                Password = "ABCDEFGHIJKLMNOPQRSTUVWXYZ", // limited to 16
                PairingCode = "abcdefghijklmnopqrstuvwxyz" // limited to 16
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTCloudPairRequest_MatchesString(expected, actual);
        }
        private void Assert_EFTCloudPairRequest_MatchesString(EFTCloudPairRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // EFTCloudPairRequest uses 'A'
            Assert.Equal(EFTRequestCommandCode.CloudLogon, actual[5]);

            // should be 'P' for EFTCloudPairRequest
            Assert.Equal('P', actual[6]);

            // ClientID
            Assert.Equal(expected.ClientID.PadRightAndCut(16), actual.Substring(7, 16));

            // Password
            Assert.Equal(expected.Password.PadRightAndCut(16), actual.Substring(23, 16));

            // Pair Code
            Assert.Equal(expected.PairingCode.PadRightAndCut(16), actual.Substring(39, 16));
        }

        #endregion

        #region EFTCloudTokenLogonRequest

        [Fact]
        public void EFTRequestToString_EFTCloudTokenLogonRequest_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTCloudTokenLogonRequest()
            {
                Token = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTCloudTokenLogonRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTCloudTokenLogonRequest_MatchesString(EFTCloudTokenLogonRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // EFTCloudTokenLogonRequest uses 'A'
            Assert.Equal(EFTRequestCommandCode.CloudLogon, actual[5]);

            // should be 'T' for EFTCloudTokenLogonRequest
            Assert.Equal('T', actual[6]);

            // Token
            Assert.True(int.TryParse(actual.Substring(7, 3), out var lenField));
            Assert.Equal(expected.Token.Length, lenField);
            Assert.Equal(expected.Token, actual.Substring(10, lenField));
        }

        #endregion

        #region EFTSendKeyRequest

        [Fact]
        public void EFTRequestToString_EFTSendKeyRequest_WithTooLongFields_ReturnsCorrespondingStringWithFieldsCut()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTSendKeyRequest()
            {
                Key = EFTPOSKey.Keyed,
                // limited to 60
                Data = "ABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZABCDEFGHIJKLMNOPQRSTUVWXYZ"
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTSendKeyRequest_MatchesString(expected, actual);
        }



        [Fact]
        public void EFTRequestToString_EFTSendKeyRequest_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTSendKeyRequest()
            {
                Key = EFTPOSKey.Keyed,
                Data = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTSendKeyRequest_MatchesString(expected, actual);
        }
        private void Assert_EFTSendKeyRequest_MatchesString(EFTSendKeyRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // EFTSendKeyRequest uses 'Y'
            Assert.Equal(EFTRequestCommandCode.SendKey, actual[5]);

            // should be '0' for EFTSendKeyRequest
            Assert.Equal('0', actual[6]);

            // Key
            Assert.Equal((char)expected.Key, actual[7]);

            // Data field
            if (expected.Data != null)
                Assert.Equal(expected.Data.PadRightAndCut(60), actual.Substring(8, actual.Length - 8));
            else
                Assert.Equal(8, actual.Length);
        }

        #endregion

        #region EFTReceiptRequest

        [Fact]
        public void EFTRequestToString_EFTReceiptRequest_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTReceiptRequest();

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTReceiptRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTReceiptRequest_MatchesString(EFTReceiptRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // EFTReceiptRequest uses '3'
            Assert.Equal(EFTRequestCommandCode.Receipt, actual[5]);

            // should be ' ' (space) for EFTReceiptRequest
            Assert.Equal(' ', actual[6]);
        }

        #endregion

        #region EFTHeartBeatRequest

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void EFTRequestToString_EFTHeartbeatRequest_ReturnsCorrespondingString(bool reply)
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTHeartbeatRequest()
            {
                Reply = reply
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTHeartbeatRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTHeartbeatRequest_MatchesString(EFTHeartbeatRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // EFTHeartbeatRequest uses Generic ('F')
            Assert.Equal(EFTRequestCommandCode.Heartbeat, actual[5]);

            // Reply required
            Assert.Equal(expected.Reply ? '1' : '0', actual[6]);

        }

        #endregion

        #region EFTMonitoringRequest

        [Fact]
        public void EFTRequestToString_EFTMonitoringRequest_ReturnsCorrespondingString()
        {
            var parser = new DefaultMessageParser();
            var expected = new EFTMonitoringRequest()
            {
                Application = TerminalApplication.GiftCard,
                Merchant = "TT",
                ProductCode = "54",
                MonitoringType = (char)MonitoringType.LicenseRequest,
                Data = "TestData"
            };

            var actual = parser.EFTRequestToString(expected);

            Assert_EFTMonitoringRequest_MatchesString(expected, actual);
        }

        private void Assert_EFTMonitoringRequest_MatchesString(EFTMonitoringRequest expected, string actual)
        {
            // starts with #
            Assert.Equal('#', actual[0]);

            // length field is valid
            Assert.True(RequestLengthFieldIsValid(actual));

            // command code is '|' for monitoring
            Assert.Equal(EFTRequestCommandCode.Monitoring, actual[5]);

            // sub code
            Assert.Equal((char)expected.MonitoringType, actual[6]);

            // Merchant
            Assert.Equal(expected.Merchant.PadRightAndCut(2), actual.Substring(7, 2));


            // App
            Assert.Equal(expected.Application.ToApplicationString(), actual.Substring(9, 2));

            // Product Code
            Assert.Equal(expected.ProductCode, actual.Substring(11, 2));

            // Version
            var verStr = $"{expected.Version.Major}.{expected.Version.Minor}.{expected.Version.Revision}".PadRightAndCut(12);
            Assert.Equal(verStr, actual.Substring(13, 12));

            // Data
            Assert.Equal(expected.Data, actual.Substring(25));

        }

        #endregion

        #region Util Functions

        private bool RequestLengthFieldIsValid(string r)
        {
            if (r?.Length < 5)
                return false;

            if (!int.TryParse(r.Substring(1, 4), out var lenField))
                return false;
            if (lenField != r.Length)
                return false;

            return true;
        }

        private bool PadFieldIsValid(string padString)
        {
            if (padString?.Length < 3)
                return false;

            if (!int.TryParse(padString.Substring(0, 3), out var lenField))
                return false;
            if (lenField != padString.Length - 3)
                return false;

            return true;
        }

        private bool DataFieldIsValid(string dataFieldStr)
        {
            if (dataFieldStr?.Length < 3)
                return false;

            if (!int.TryParse(dataFieldStr.Substring(0, 3), out var lenField))
                return false;
            if (lenField != dataFieldStr.Length - 3)
                return false;

            return true;
        }


        private bool CharIsOneOf(char c, params char[] validValues)
        {
            foreach (var v in validValues)
            {
                if (c.Equals(v))
                    return true;
            }
            return false;
        }

        #endregion
    }
}
