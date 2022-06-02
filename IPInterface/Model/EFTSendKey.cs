namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <summary>PC-EFTPOS key types.</summary>
    public enum EFTPOSKey
    {

        [Description("OK/Cancel ('0')")]
        /// <summary>The OK/CANCEL key.</summary>
        OkCancel = '0',

        [Description("Yes/Accept ('1')")]
        /// <summary>The YES/ACCEPT key.</summary>
        YesAccept = '1',

        [Description("No/Decline ('2')")]
        /// <summary>The NO/DECLINE key.</summary>
        NoDecline = '2',

        [Description("Authorise ('3')")]
        /// <summary>The AUTH key.</summary>
        Authorise = '3',

        [Filter("Woolworths"), Description("Barcode ('B')")]
        Barcode = 'B',

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
