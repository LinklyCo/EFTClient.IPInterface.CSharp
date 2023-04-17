using PCEFTPOS.EFTClient.IPInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IPInterface.SimpleConsoleDemo
{

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
                // Set ReferenceNumber to something unique
                TxnRef = DateTime.Now.ToString("YYMMddHHmmssfff"),
                // Set AmountCash for cash out, and AmountPurchase for purchase/refund
                AmtPurchase = 1.00M,
                AmtCash = 0.00M,
                // Set POS or pinpad printer
                ReceiptAutoPrint = ReceiptPrintModeType.POSPrinter,
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
            foreach (var s in e.Response.ReceiptText) { Console.WriteLine($"{s}"); }

        }

        private void Eft_OnTransaction(object sender, EFTEventArgs<EFTTransactionResponse> e)
        {
            // Handle transaction event 
            var displayText = e.Response.Success ? "successful" : "unsuccessful";
            Console.WriteLine($"Transaction was {displayText}");
            txnFired.Set();
        }
    }

    class EFTClientIPDemoAsync
    {
        public async Task RunAsync()
        {
            // Create new connection to EFT-Client
            var eft = new EFTClientIPAsync();
            var connected = await eft.ConnectAsync("127.0.0.1", 2011, false);
            if (!connected)
            {
                // Handle failed connection
                Console.WriteLine("Connect failed");
            }

            // Build transaction request
            var r = new EFTTransactionRequest()
            {
                // TxnType is required
                TxnType = TransactionType.PurchaseCash,
                // Set ReferenceNumber to something unique
                TxnRef = DateTime.Now.ToString("YYMMddHHmmssfff"),
                // Set AmountCash for cash out, and AmountPurchase for purchase/refund
                AmtPurchase = 1.00M,
                AmtCash = 0.00M,
                // Set POS or pinpad printer
                ReceiptAutoPrint = ReceiptPrintModeType.POSPrinter,
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
                    var timeoutToken = new CancellationTokenSource(new TimeSpan(0, 1, 0)).Token; // 5 minute timeout
                    eftResponse = await eft.ReadResponseAsync(timeoutToken);

                    switch (eftResponse)
                    {
                        case EFTReceiptResponse eftReceiptResponse:
                            Console.WriteLine($"{eftReceiptResponse.Type} receipt");
                            foreach (var s in eftReceiptResponse.ReceiptText) { Console.WriteLine($"{s}"); }
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
                catch (TaskCanceledException)
                {
                    Console.WriteLine("EFT-Client timeout waiting for response");
                    waitingForResponse = false;
                }
                catch (ConnectionException)
                {
                    Console.WriteLine("Socket closed");
                    waitingForResponse = false;
                }
                catch (Exception)
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
        static void Main(string[] args)
        {
            (new EFTClientIPDemo()).Run();
            (new EFTClientIPDemoAsync()).RunAsync().GetAwaiter().GetResult();
            Console.WriteLine("Press any key to quit");
            Console.ReadLine();
        }
    }
}
