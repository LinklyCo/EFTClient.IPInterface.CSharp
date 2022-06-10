namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <summary>PC-EFTPOS key types.</summary>
    public enum EFTPOSKey
    {
        /// <summary>The OK/CANCEL key.</summary>
        [Description("OK/Cancel ('0')")]
        OkCancel = '0',

        /// <summary>The YES/ACCEPT key.</summary>
        [Description("Yes/Accept ('1')")]
        YesAccept = '1',

        /// <summary>The NO/DECLINE key.</summary>
        [Description("No/Decline ('2')")]
        NoDecline = '2',

        /// <summary>The AUTH key.</summary>
        [Description("Authorise ('3')")]
        Authorise = '3',

        /// <summary>Used by Woolworths to indicate Data came from a barcode.</summary>
        [Filter("Woolworths"), Description("Barcode ('B')")]
        Barcode = 'B',

        /// <summary>Used by Woolworths to indicate Data came from keyed entry.</summary>
        [Filter("Woolworths"), Description("Keyed ('K')")]
        Keyed = 'K'
    }

    /// <summary>A PC-EFTPOS client list request object.</summary>
    public class EFTSendKeyRequest : EFTRequest
    {
        /// <summary>Constructs a default client list object.</summary>
        public EFTSendKeyRequest() : base(false, null)
        {
            isStartOfTransactionRequest = false;
        }

        /// <summary> The type of key to send </summary>
        public EFTPOSKey Key { get; set; } = EFTPOSKey.OkCancel;

        /// <summary> Data entered by the POS (e.g. for an 'input entry' dialog type) </summary>
        public string Data { get; set; } = "";
    }
}
