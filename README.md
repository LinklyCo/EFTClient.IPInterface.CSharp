# IPInterface - a wrapper for the EFT-Client TCP/IP interface

## Project structure

| Folder                      | Description   
| ---------------------------  | -------------
| IPInterface                 | A project with both event (`EFTClientIP`) and async/await based (`EFTClientIPAsync`) wrapper objects for the EFT-Client TCP/IP interface.
| IPInterface.SimpleDemoAsync | A basic sample app using the `EFTClientIPAsync` component
| IPInterface.SimpleDemo      | A basic sample app using the `EFTClientIP` component
| IPInterface.TestPOS         | A full featured sample app using the `EFTClientIPAsync` component

## Getting started
* Clone this repository or grab the [PCEFTPOS.EFTClient.IPInterface](https://www.nuget.org/packages/PCEFTPOS.EFTClient.IPInterface/) package from NuGet
* Decide which component to use. `EFTClientIP` is event based pattern and `EFTClientIPAsync` uses the async/await pattern
* Look at the `SimpleDemo` and `SimpleDemoAsync` samples. There are also some simple examples listed below.

##### Example usage for EFTClientIP
``` csharp
class EFTClientIPDemo
    {
        ManualResetEvent txnFired = new ManualResetEvent(false);

        public void Run()
        {
            // Create new connection to EFT-Client
            var eft = new EFTClientIP()
            {
                HostName = "127.0.0.1",
                HostPort = 2011,
                UseSSL = false
            };
            // Hook up events
            eft.OnReceipt += Eft_OnReceipt;
            eft.OnTransaction += Eft_OnTransaction;
            eft.OnTerminated += Eft_OnTerminated;
            // Connect
            if (!eft.Connect())
            {
                // Handle failed connection
                Console.WriteLine("Connect failed");
                return;
            }

            // Build transaction request
            var r = new EFTTransactionRequest()
            {
                // TxnType is required
                TxnType = TransactionType.PurchaseCash,
                // Set TxnRef to something unique
                TxnRef = DateTime.Now.ToString("YYMMddHHmmsszzz"),
                // Set AmtCash for cash out, and AmtPurchase for purchase/refund
                AmtPurchase = 1.00M,
                AmtCash = 0.00M,
                // Set POS or pinpad printer
                ReceiptPrintMode = ReceiptPrintModeType.POSPrinter,
                // Set application. Used for gift card & 3rd party payment
                Application = TerminalApplication.EFTPOS
            };
            // Send transaction
            if (!eft.DoTransaction(r))
            {
                // Handle failed send
                Console.WriteLine("Send failed");
                return;
            }

            txnFired.WaitOne();
            eft.Disconnect();
            eft.Dispose();
        }

        private void Eft_OnTerminated(object sender, SocketEventArgs e)
        {
            // Handle socket close
            Console.WriteLine($"Socket closed");
            txnFired.Reset();
        }

        private void Eft_OnReceipt(object sender, EFTEventArgs<EFTReceiptResponse> e)
        {
            // Handle receipt
            Console.WriteLine($"{e.Response.Type} receipt");
            Console.WriteLine($"{e.Response.ReceiptText}");
        }

        private void Eft_OnTransaction(object sender, EFTEventArgs<EFTTransactionResponse> e)
        {
            // Handle transaction event 
            var displayText = e.Response.Success ? "successful" : "unsuccessful";
            Console.WriteLine($"Transaction was {displayText}");
            txnFired.Set();
        }
    }
    
    class Program
    {
        static async void Main(string[] args)
        {
            (new EFTClientIPDemo()).Run();
            Console.WriteLine("Press any key to quit");
            Console.ReadLine();
        }
    }    
```

##### Example usage for EFTClientIPAsync
``` csharp
class EFTClientIPDemoAsync
    {
        public async Task RunAsync()
        {
            // Create new connection to EFT-Client
            var eft = new EFTClientIPAsync();
            var connected = await eft.ConnectAsync("127.0.0.1", 2011, false);
            if(!connected)
            {
                // Handle failed connection
                Console.WriteLine("Connect failed"); 
            }

            // Build transaction request
            var r = new EFTTransactionRequest()
            {
                // TxnType is required
                TxnType = TransactionType.PurchaseCash,
                // Set TxnRef to something unique
                TxnRef = DateTime.Now.ToString("YYMMddHHmmsszzz"),
                // Set AmtCash for cash out, and AmtPurchase for purchase/refund
                AmtPurchase = 1.00M,
                AmtCash = 0.00M,
                // Set POS or pinpad printer
                ReceiptPrintMode = ReceiptPrintModeType.POSPrinter,
                // Set application. Used for gift card & 3rd party payment
                Application = TerminalApplication.EFTPOS
            };
            
            // Send transaction
            if (await eft.WriteRequestAsync(r) == false)
            {
                // Handle failed send
                Console.WriteLine("Send failed");
                return;
            }

            // Wait for response
            var waitingForResponse = true;
            do
            {
                EFTResponse eftResponse = null;
                try
                {
                    var timeoutToken = new CancellationTokenSource(new TimeSpan(0, 5, 0)).Token; // 5 minute timeout
                    eftResponse = await eft.ReadResponseAsync(timeoutToken);

                    switch(eftResponse)
                    {
                        case EFTReceiptResponse eftReceiptResponse:
                            Console.WriteLine($"{eftReceiptResponse.Type} receipt");
                            Console.WriteLine($"{eftReceiptResponse.ReceiptText}");
                            break;
                        case EFTTransactionResponse eftTransactionResponse:
                            var displayText = eftTransactionResponse.Success ? "successful" : "unsuccessful";
                            Console.WriteLine($"Transaction was {displayText}");
                            waitingForResponse = false;
                            break;
                        case null:
                            Console.WriteLine("Error reading response");
                            break;
                    }
                }
                catch(TaskCanceledException)
                {
                    Console.WriteLine("EFT-Client timeout waiting for response");
                    waitingForResponse = false;
                }
                catch(ConnectionException)
                {
                    Console.WriteLine("Socket closed");
                    waitingForResponse = false;
                }
                catch(Exception)
                {
                    Console.WriteLine("Unhandled exception");
                    waitingForResponse = false;
                }
            }
            while (waitingForResponse); 

            eft.Disconnect();
        }
    }

    class Program
    {
        static async void Main(string[] args)
        {
            await (new EFTClientIPDemoAsync()).RunAsync();
            Console.WriteLine("Press any key to quit");
            Console.ReadLine();
        }
    }
```

## Release notes

### 1.7.3 (2023-03-31)
* [STRY0228186] Removing length check from EFTQueryCardRespons, will parse response if it doesn't have PAD data
* [STRY0228186] Correctly parsing `EFTGetLastResponse` by adding missing `ClearedFundsBalance`
  * This fixes an issue where PAD data was being omitted

### 1.7.2 (2022-09-13)
* Add "Simulated Host" pinpad network type option.

### 1.7.1 (2022-08-31)
* Add ability to open the client GUI main dialog.

### 1.7.0 (2022-07-18)
* Adds handling for sending and recieving Monitoring ('|') messages

### 1.6.11 (2022-06-28)
* Adds DataField property to EFTTransactionRequest, required for AuthPIN ('X') and EnhancedPIN ('K') Transaction Types
* Adds required special handling of AuthPIN ('X') and EnhancedPIN ('K') Transaction Types to DefaultMessageParser
* Similarly adds (provisional) handling of recieving PINRequest ('W') responses to DefaultMessageParser, which is recieved from client in response to Transaction Rquests of the above two transaction types.

### 1.6.10 (2022-06-22)
* Implementing hardening changes suggested by Static Code Analysis
		
### 1.6.9 (2022-06-16)
* Update IPInterface to compile against .NET 6

### 1.6.8 (2022-06-07)
* Fixes for SonarCloud Static Code Analysis
* Added SSL Certificate checking for non-async EFT-Client Interface
* Fixing syntax for some XML comments

### 1.6.7 (2022-04-22)
* Consolidating IPInterface version numbers

### 1.6.6 (2022-03-29)
* 'StringExtension.StrLen' function now returns correct values when startIndex argument has a non-zero value
* 'IntegerExtension.PadLeft' no longer throws an exception if called on a negative
* 'DecimalExtension.PadLeft' no longer throws an exception if called on a negative
* 'AccountTypeExtension.FromString' now returns AccountType.Default if input string is null instead of throwing exception
* 'EFTClientIPAsync.FromString' now now returns the result returned to it by it's enclosed client stream instead of just always returning true unless there is an exception
* 'DirectEncoding.GetBytes' functions now correctly handle the charCount argument and raise more helpful exceptions
* Created static class 'EFTRequestCommandCode' which has publicly accessible constants for EFTRequest command codes
* Removed (many of but not all) of the "magic strings" used by DefaultMessageParser when generating EFTRequests
* Updated property names on ControlPanelRequest and EFTControlPanelRequest to be in line with spec and other EFTRequests. Old property names have been marked obsolete, but are still supported.
* EFTClientIPAsync now correctly disposes it's wrapped ITCPSocketAsync when it replaces it with a new connection and when it itself is disposed

### 1.6.5 (2022-03-15)
* Aligning `EFTTerminalType` enum list with spec

### 1.6.4.0 (2022-02-24)
* Fixing Null reference exception when receiving data using EFTClientIP and no subscribed DialogUIHandler

### 1.6.3.0 (16/12/2021)
* Adding StanVb field to resolve ambiguity in VB.Net due to case-insensitivity

### 1.6.2.0 (15/12/2021)
* Fixed CancellationTokenSource memory leak

### 1.4.4.0 (27/09/2019)
* Added new IPnterface calls for CloudPairing Request/Response and CloudTokenLogon Request/Response
* Updated TestPOS to support new Cloud pairing and cloud token request/response

### 1.4.3.0 (2018-10-09)
* Deleted a hard-coded TxnRef in TestPOS GetLast and ReprintReceipt command
* Fixed bug in MessageParser that padded the TxnRef rather than leaving it blank, so the EFTClient didn't like it

### 1.4.3.0 (2018-10-09)
* Deleted a hard-coded TxnRef in TestPOS GetLast and ReprintReceipt command
* Fixed bug in MessageParser that padded the TxnRef rather than leaving it blank, so the EFTClient didn't like it

### 1.4.2.0 (2018-09-19)
* Added new ReceiptAutoPrint modes for EFTRequests
* Updated MessageParser to use non-deprecated properties
* Updated TestPOS ClientViewModel to do the same

### 1.4.1.3 (2018-09-12)
* Fixed for EFTTransactionResponse and typo

### 1.4.1.2 (2018-09-12)
* Changes to fields ReceiptAutoPrint, CutReceipt, AccountType and DateSettlement

### 1.4.1.1 (2018-08-29)
* Added support for EFTGetLastTransactionRequest by TxnRef

### 1.4.1.0 (2018-07-17)
* Updated PadField to support IList<PadTag>

### 1.4.0.0 (2018-04-30)
* Added IDialogUIHandler for easier handling of POS custom dialogs.
* Updated MessageParser to allow for custom parsing.

### 1.3.5.0 (2018-02-16)
* Added support for .NET Standard 2.0
* Added support for basket data API
* Updated some property names to bring EFTClientIP more inline with the existing ActiveX interface. Old property names have been marked obsolete, but are still supported.

### 1.3.3.0 (2017-10-26)
* Changed internal namespaces from `PCEFTPOS.*` (`PCEFTPOS.Net`, `PCEFTPOS.Messaging` etc) to `PCEFTPOS.EFTClient.IPInterface`. This was causing issues when combining the EFTClientIP Nuget package with the actual PCEFTPOS lib. EFTClientIP needs to remain totally self-contained. 

### 1.3.2.0 (2017-19-09)
* Updated nuspec for v1.3.2.0 release.

### 1.3.1.0 (2017-09-13)
* Changed namespace from `PCEFTPOS.API.IPInterface` to `PCEFTPOS.EFTClient.IPInterface` for new package
* Created signed NuGet package

### 1.2.1.0 (2017-07-11)
* Added CloudLogon event to EFTClientIP

### 1.1.0.0 (2017-06-30)
* Fixed a bug that would cause the component to hang if an unknown message was received 
* Improved handling of messages received across multiple IP packets
* Added support for Pay at Table

### 1.0.0.1 (2016-10-28)
* Initial release