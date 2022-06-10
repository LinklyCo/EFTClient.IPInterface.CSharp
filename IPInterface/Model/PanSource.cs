namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <summary>The source of the card number.</summary>
    public enum PanSource
    {
        /// <summary>Indicates the customer will be prompted to swipe,insert or present their card.</summary>
        [Description("Default (' ')")]
        Default = ' ',

        /// <summary>Indicates the POS has captured the Track2 from the customer card and it is stored in the PAN property.</summary>
        [Description("POS Swiped ('S')")]
        POSSwiped = 'S',

        /// <summary>Indicates the POS operator has keyed in the card number and it is stored in the PAN property.</summary>
        [Description("POS Keyed ('K')")]
        POSKeyed = 'K',

        /// <summary>Indicates the card number was captured from the Internet and is stored in the PAN property.</summary>
        [Description("Internet ('0')")]
        Internet = '0',

        /// <summary>Indicates the card number was captured from a telephone order and it is stored in the PAN property.</summary>
        [Description("Tele Order ('1')")]
        TeleOrder = '1',

        /// <summary>Indicates the card number was captured from a mail order and it is stored in the PAN property.</summary>
        [Description("Mail Order ('2')")]
        MailOrder = '2',

        /// <summary>Indicates the POS operator has keyed in the card number and it is stored in the PAN property.</summary>
        [Description("Customer Present ('3')")]
        CustomerPresent = '3',

        /// <summary>Indicates the card number was captured for a recurring transaction and it is stored in the PAN property.</summary>
        [Description("Recurring Transaction ('4')")]
        RecurringTransaction = '4',

        /// <summary>Indicates the card number was captured for an installment payment and it is stored in the PAN property.</summary>
        [Description("Installment ('5')")]
        Installment = '5'
    }
}