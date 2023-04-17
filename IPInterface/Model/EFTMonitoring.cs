using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using PCEFTPOS.EFTClient.IPInterface;

namespace PCEFTPOS.EFTClient.IPInterface
{
    /// <summary>
    /// A Monitoring request object.
    /// <b>For internal use by Linkly only</b>
    /// </summary>

    public class EFTMonitoringRequest : EFTRequest
    {

        /// <summary>Two digit merchant code</summary>
        /// <value>Type: <see cref="string"/><para>The default is "00"</para></value>
        public string Merchant { get; set; } = "00";

        /// <summary>type of monitoring request to perform.</summary>
        public char MonitoringType { get; set; } = (char)IPInterface.MonitoringType.NotSet;

        /// <summary>Indicates where the request is to be sent to. Should normally be EFTPOS.</summary>
        /// <value>Type: <see cref="TerminalApplication"/><para>The default is <see cref="TerminalApplication.EFTPOS"/>.</para></value>
        public TerminalApplication Application { get; set; } = TerminalApplication.EFTPOS;

        /// <summary>Product code</summary>
        /// <value>Type: <see cref="string"/><para>The default is "00"</para></value>
        public string ProductCode { get; set; } = "00";

        /// <summary>The Version of the requester</summary>
        /// <value>Type: <see cref="Version"/><para>The default is the version of the executing assembly</para></value>
        public Version Version { get; set; }

        /// <summary>The monitoring request info, will differ in content and format depending on which product and <see cref="MonitoringType"/> is being requested</summary>
        /// <value>Type: <see cref="string"/></value>
        public string Data { get; set; } = "";

        
        /// <summary>
        /// Creates a monitoring request object
        /// </summary>
        /// <param name="ver">The version object to use for the version field, defaults to current executing assembly version</param>
        public EFTMonitoringRequest(Version ver = null) : base(true, typeof(EFTMonitoringResponse))
        {
            Version = ver ?? Assembly.GetExecutingAssembly().GetName().Version;
        }

    }

    /// <summary>
    /// A Monitoring response object.
    /// <b>For internal use by Linkly only</b>
    /// </summary>
    public class EFTMonitoringResponse : EFTResponse
    {

        /// <summary>Two digit merchant code</summary>
        /// <value>Type: <see cref="string"/><para>The default is "00"</para></value>
        public string Merchant { get; set; } = "00";

        /// <summary>type of monitoring response.</summary>
        public char MonitoringType { get; set; } = (char)IPInterface.MonitoringType.NotSet;


        /// <summary>The response code of the request.</summary>
        /// <value>Type: <see cref="System.String"/><para>A 2 character response code. "00" indicates a successful response.</para></value>
        public string ResponseCode { get; set; } = "";

        /// <summary>The response text for the response code.</summary>
        /// <value>Type: <see cref="System.String"/></value>
        public string ResponseText { get; set; } = "";

        /// <summary>Product code</summary>
        /// <value>Type: <see cref="string"/><para>The default is "00"</para></value>
        public string ProductCode { get; set; } = "00";

        /// <summary>The Version of the response</summary>
        /// <value>Type: <see cref="Version"/><para>The default is the version of the executing assembly</para></value>
        public Version Version { get; set; }


        /// <summary>The monitoring request info, will differ in content and format depending on which product and <see cref="MonitoringType"/> the response is for</summary>
        /// <value>Type: <see cref="string"/></value>
        public string Data { get; set; } = "";

        /// <summary>Constructs a default monitoring response object.</summary>

        public EFTMonitoringResponse() : base(typeof(EFTMonitoringRequest))
        {

        }
    }

    /// <summary>Indicates the type of monitoring message a <see cref="EFTMonitoringRequest"/> or <see cref="EFTMonitoringResponse"/> is.</summary>

    public enum MonitoringType
    {
        /// <summary>Monitoring Type hasn't been set.</summary>
        NotSet = ' ',

        /// <summary>Request an updated cert.</summary>
        CertificateRequest = '1',

        /// <summary>Request a new license.</summary>
        LicenseRequest = '2',

        /// <summary>Send Txn Advice</summary>
        TxnAdvice = '3'
    }

}
