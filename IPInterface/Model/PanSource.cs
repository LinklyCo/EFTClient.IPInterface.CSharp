namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <summary>The source of the card number.</summary>
    public enum PanSource
    {
        [Description("Default (' ')")]
        /// <summary>Indicates the customer will be prompted to swipe,insert or present their card.</summary>
        Default = ' ',

        [Description("POS Swiped ('S')")]
        /// <summary>Indicates the POS has captured the Track2 from the customer card and it is stored in the PAN property.</summary>
        POSSwiped = 'S',

        [Description("POS Keyed ('K')")]
        /// <summary>Indicates the POS operator has keyed in the card number and it is stored in the PAN property.</summary>
        POSKeyed = 'K',

        [Description("Internet ('0')")]
        /// <summary>Indicates the card number was captured from the Internet and is stored in the PAN property.</summary>
        Internet = '0',

        [Description("Tele Order ('1')")]
        /// <summary>Indicates the card number was captured from a telephone order and it is stored in the PAN property.</summary>
        TeleOrder = '1',

        [Description("Mail Order ('2')")]
        /// <summary>Indicates the card number was captured from a mail order and it is stored in the PAN property.</summary>
        MailOrder = '2',

        [Description("Customer Present ('3')")]
        /// <summary>Indicates the POS operator has keyed in the card number and it is stored in the PAN property.</summary>
        CustomerPresent = '3',

        [Description("Recurring Transaction ('4')")]
        /// <summary>Indicates the card number was captured for a recurring transaction and it is stored in the PAN property.</summary>
        RecurringTransaction = '4',

        [Description("Installment ('5')")]
        /// <summary>Indicates the card number was captured for an installment payment and it is stored in the PAN property.</summary>
        Installment = '5'
    }
}