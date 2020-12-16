namespace PCEFTPOS.EFTClient.IPInterface
{
    public static class TerminalApplicationExtension
    {
        public static string ToApplicationString(this TerminalApplication v)
        {
            switch (v)
            {
                case TerminalApplication.EFTPOS:
                    return "00";
                case TerminalApplication.Agency:
                    return "01";
                case TerminalApplication.Loyalty:
                case TerminalApplication.ETS:
                    return "02";                // PCEFTCSA
                case TerminalApplication.GiftCard:
                    return "03";
                case TerminalApplication.Fuel:
                    return "04";
                case TerminalApplication.Medicare:
                    return "05";
                case TerminalApplication.Amex:
                    return "06";
                case TerminalApplication.ChequeAuth:
                    return "07";
            }

            return "00";    // Default to EFTPOS.
        }

        //public static string ToMerchantString(this TerminalApplication v)
        //{
        //    switch (v)
        //    {
        //        case TerminalApplication.EFTPOS:
        //        case TerminalApplication.Agency:
        //            return "00";
        //        case TerminalApplication.GiftCard:
        //            return "01";
        //        case TerminalApplication.Loyalty:
        //            return "02";
        //        case TerminalApplication.ChequeAuth:
        //            return "03";
        //        case TerminalApplication.PrePaidCard:
        //            return "04";
        //        case TerminalApplication.Medicare:
        //            return "05";
        //        case TerminalApplication.Amex:
        //            return "08";
        //    }

        //    return "00";    // Default to EFTPOS.
        //}
    }

    public static class AccountTypeExtension
    {
        public static AccountType FromString(this AccountType _, string s)
        {
            if (s.ToUpper().TrimEnd() == "CREDIT")
                return AccountType.Credit;
            else if (s.ToUpper().TrimEnd() == "SAVINGS")
                return AccountType.Savings;
            else if (s.ToUpper().TrimEnd() == "CHEQUE")
                return AccountType.Cheque;

            return AccountType.Default;
        }
    }

    public static class TransactionTypeExtension
    {
        public static string ToTransactionString(this TransactionType t)
        {
            return (t == TransactionType.None) ? "0" : new string((char)t, 1);
        }
    }
}
