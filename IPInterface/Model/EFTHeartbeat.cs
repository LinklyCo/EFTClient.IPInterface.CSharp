namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <summary>Defines a Linkly heartbeat request/response</summary>
	public class EFTHeartbeatRequest : EFTRequest
    {
        /// <summary>Constructs a default EFTDuplicateReceiptRequest object.</summary>
        public EFTHeartbeatRequest() : base(true, typeof(EFTHeartbeatResponse))
        {
        }

        /// <summary>True if the EFT-Client should send a reply</summary>
        public bool Reply { get; set; } = true;
    }

    /// <summary>A PC-EFTPOS duplicate receipt response object.</summary>
    public class EFTHeartbeatResponse : EFTResponse
    {
        /// <summary>Constructs a default duplicate receipt response object.</summary>
        public EFTHeartbeatResponse() : base(typeof(EFTHeartbeatRequest))
        {
        }

        /// <summary>Subcode from EFT-Client</summary>
        public char Subcode { get; set; }

        /// <summary>If we receive a heartbeat response, it worked!</summary>
        public bool Success => true;
    }
}