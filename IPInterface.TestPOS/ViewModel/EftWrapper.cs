using PCEFTPOS.EFTClient.IPInterface.Slave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel
{
    public class EftWrapper
    {
        readonly IEFTClientIPAsync eft = new EFTClientIPAsync();
        readonly ClientData data = null;
        CancellationTokenSource ct = null;
        CancellationTokenSource heartbeatTestCTS = null;

        /// <summary>
        /// Set to true when a request is in progress and false when a request ends. Used to limit access to SendRequest
        /// </summary>
        private HistoryViewModel _inProgress = null;
        public HistoryViewModel requestInProgress
        {
            get => _inProgress;
            set
            {
                previousMessage = null;
                _inProgress = value;
                data.InProgress = requestInProgressString;
            }
        }
        public HistoryViewModel previousMessage { get; set; } = null;

        public string requestInProgressString => (_inProgress == null) ? null : _inProgress.ToString().Split('.').Last();

        public EftWrapper(ClientData data)
        {
            this.data = data;
            eft.OnLog += Eft_OnLog;
        }

        private string _eftLogs = string.Empty;
        private void Eft_OnLog(object sender, LogEventArgs e)
        {
            if (data.SendKeyEnabled)
            {
                _eftLogs = e.Message;
            }
            else
            {
                LogType logType;
                switch (e.LogLevel)
                {
                    case LogLevel.Error:
                    case LogLevel.Fatal:
                        logType = LogType.Error;
                        break;
                    case LogLevel.Warn:
                        logType = LogType.Warning;
                        break;
                    default:
                        logType = LogType.Info;
                        break;
                }

                var msg = (logType == LogType.Error && e.Exception != null) ? $"{e.Message} [{e.Exception.Message}]" : e.Message;

                data.Log(msg, logType);
            }
        }

        #region Common
        private Dictionary<string, string> DictionaryFromType(object atype)
        {
            Dictionary<string, string> d = new Dictionary<string, string>();
            if (atype == null)
                return d;

            Type t = atype.GetType();
            try
            {
                foreach (PropertyInfo prp in t.GetProperties())
                {
                    var p = prp.GetValue(atype, new object[] { });

                    if (p != null
                            && prp?.GetCustomAttributes(false)?.FirstOrDefault(a => a.GetType() == typeof(ObsoleteAttribute)) == null)
                    {
                        string value = p.ToString();

                        Type tt = p.GetType();

                        if (p is TransactionType txn)
                        {
                            value = txn.ToTransactionString();
                        }
                        else if (p is PadField _pf)
                        {
                            value = _pf.ToFormattedString();
                        }
                        else if (tt.IsClass && tt != typeof(string) && !tt.IsArray)
                        {
                            var e = DictionaryFromType(p);
                            if (e.Count > 0)
                            {
                                value = string.Empty;
                                foreach (var i in e)
                                {
                                    value += $"{i.Key}: {i.Value};{Environment.NewLine}";
                                }
                            }
                        }
                        else if (tt == typeof(string[]))
                        {
                            value = string.Empty;
                            string[] arr = p as string[];
                            foreach (string s in arr)
                            {
                                value += s + Environment.NewLine;
                            }
                            data.Log(value);
                        }
                        d.Add(prp.Name, value);
                    }
                }
            }
            catch
            {
            }
            return d;
        }

        private void ShowError(string hr, string message)
        {
            data.LastTxnType = "Error";
            data.LastTxnResult.Clear();
            var x = new Dictionary<string, string>
            {
                { "Result", "Failed" },
                { "ResponseCode", hr },
                { "ResponseText", message }
            };
            data.LastTxnResult = x;

            data.Log($"Error: {hr} {message}");
        }

        private void AddMessageToHistory(HistoryViewModel message)
        {
            data.MessageHistory.Insert(0, message);
            previousMessage = message;
        }

        private async Task<bool> SendRequest<ResponseType>(EFTRequest request, bool autoApproveDialogs = false) where ResponseType : EFTResponse
        {
            var cancellationToken = new CancellationTokenSource(new TimeSpan(0, 5, 0)).Token; // 5 minute default timeout 
            return await SendRequest<ResponseType>(request, cancellationToken, autoApproveDialogs);
        }

        private async Task<bool> SendRequest<ResponseType>(EFTRequest request, CancellationToken cancellationToken, bool autoApproveDialogs = false) where ResponseType : EFTResponse
        {
            bool result = false;

            if (requestInProgress != null)
            {
                if (data.Settings.IsWoolworthsPOS && 
                    requestInProgress.MessageType.Equals(typeof(EFTQueryCardRequest)) && 
                    (request.GetType().Equals(typeof(EFTStatusRequest)) ||
                     request.GetType().Equals(typeof(EFTTransactionRequest)) ) )
                {
                    data.Log($"Request already in progress, but allowing {request.GetType()} through");
                }
                else
                {
                    data.Log($"Unable to process {request}. There is already a request in progress");
                    return false;
                }
            }
            // 
            if (!eft.IsConnected || data.ConnectedState == ConnectedStatus.Disconnected)
            {
                data.ConnectedState = ConnectedStatus.Disconnected;
                data.Log($"Unable to process {request}. Not connected to POS");
                return false;
            }

            try
            {
                HistoryViewModel requestHistory = new HistoryViewModel(request);
                requestInProgress = requestHistory;
                AddMessageToHistory(requestHistory);

                await eft.WriteRequestAsync(request);
                do
                {
                    if (request.GetType().Equals(typeof(EFTSlaveRequest)))
                        requestInProgress = null;
                    var r = await eft.ReadResponseAsync(cancellationToken);

                    if (r == null) // stream is busy
                    {
                        data.Log($"Unable to process {request}. Stream is busy.");
                    }
                    else
                    {
                        HistoryViewModel responseHistory = new HistoryViewModel(r, requestInProgress, previousMessage);
                        AddMessageToHistory(responseHistory);

                        string responseType = r.GetType().ToString();
                        data.LastTxnType = responseType.Substring(responseType.LastIndexOf('.') + 1);

                        if (r is EFTDisplayResponse displayResponse)
                        {
                            data.LastTxnResult = DictionaryFromType(r);

                            if (data.Settings.DemoDialogOption != DemoDialogMode.Hide)
                            {
                                data.DisplayDetails = displayResponse;
                                data.DisplayDialog(true);
                            }

                            if (autoApproveDialogs && (r as EFTDisplayResponse).OKKeyFlag)
                            {
                                EFTSendKeyRequest req = new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel };
                                AddMessageToHistory(new HistoryViewModel(req, requestInProgress, previousMessage));
                                await eft.WriteRequestAsync(req);
                            }
                        }
                        else if (r is EFTReceiptResponse || r is EFTReprintReceiptResponse)
                        {
                            // Hacked in some preprint and print response timeouts. It ain't pretty and it may not work, but it's my code.
                            #region PrintResponse Timeout
                            if (data.IsPrePrintTimeOut)
                            {
                                if ((r as EFTReceiptResponse).IsPrePrint)
                                {
                                    Thread.Sleep(59000);
                                }
                            }
                            else if (data.IsPrintTimeOut)
                            {
                                if (!(r as EFTReceiptResponse).IsPrePrint)
                                {
                                    Thread.Sleep(59000);
                                }
                            }
                            #endregion

                            string[] receiptText = (r is EFTReceiptResponse) ? (r as EFTReceiptResponse).ReceiptText : (r as EFTReprintReceiptResponse).ReceiptText;

                            StringBuilder receipt = new StringBuilder();
                            foreach (var s in receiptText)
                            {
                                if (s.Length > 0)
                                {
                                    receipt.AppendLine(s);
                                }
                            }

                            if (!string.IsNullOrEmpty(receipt.ToString()))
                            {
                                data.Receipt = receipt.ToString();
                            }

                            if (request is EFTReceiptRequest || (request is EFTReprintReceiptRequest && request.GetPairedResponseType() == r.GetType()))
                            {
                                requestInProgress = null;
                            }
                        }
                        else if (r is EFTQueryCardResponse cardResponse)
                        {
                            data.LastTxnResult = DictionaryFromType(r);

                            data.SelectedTrack2 = cardResponse.Track2;

                            if (data.MerchantNumber.Equals(ClientData.ONEBUTTON))
                                data.MerchantNumber = cardResponse.CardName.ToString();

                            data.DisplayDialog(false);
                            requestInProgress = null;
                        }
                        else if (r is EFTClientListResponse _)
                        {
                            int index = 1;
                            var x = (EFTClientListResponse)r;
                            Dictionary<string, string> ClientList = new Dictionary<string, string>();
                            foreach (var clnt in x.EFTClients)
                            {
                                ClientList.Add("Client " + index.ToString() + " " + nameof(clnt.Name), clnt.Name);
                                ClientList.Add("Client " + index.ToString() + " " + nameof(clnt.IPAddress), clnt.IPAddress);
                                ClientList.Add("Client " + index.ToString() + " " + nameof(clnt.Port), clnt.Port.ToString());
                                ClientList.Add("Client " + index.ToString() + " " + nameof(clnt.State), clnt.State.ToString());
                                index++;
                            }

                            data.LastTxnResult = ClientList;
                            data.DisplayDialog(false);
                            requestInProgress = null;
                        }
                        else if (r is EFTCloudTokenLogonResponse eftCloudTokenLogonResponse)
                        {
                            result = eftCloudTokenLogonResponse.Success;
                            requestInProgress = null;
                        }
                        else if (r is EFTCloudPairResponse eftCloudPairResponse)
                        {
                            data.Settings.CloudInfo.Token = eftCloudPairResponse.Token;
                            requestInProgress = null;
                            if (await CloudTokenLogon(eftCloudPairResponse.Token))
                            {
                                result = true;
                            }
                            else
                            {
                                Disconnect();
                            }
                        }
                        else
                        {
                            if (r is EFTTransactionResponse eftTransactionResponse
                                    && eftTransactionResponse.PurchaseAnalysisData.HasTag("RFN"))
                            {
                                data.LastTxnRFN = eftTransactionResponse.PurchaseAnalysisData.GetTag("RFN").Data;
                            }

                            data.LastTxnResult = DictionaryFromType(r);
                            string output = string.Empty;
                            data.LastTxnResult.TryGetValue("Success", out output);
                            result = output.Equals("True", StringComparison.OrdinalIgnoreCase);
                            data.DisplayDialog(false);
                            requestInProgress = null;
                        }
                    }
                }
                while (requestInProgress != null);

                if (requestInProgress == null)
                {
                    data.Log($"Request: {request} done!");
                }
            }
            catch (TaskCanceledException)
            {
                data.ConnectedState = ConnectedStatus.Disconnected;
                ShowError("EFT-Client Timeout", "");
            }
            catch (ConnectionException cx)
            {
                data.ConnectedState = ConnectedStatus.Disconnected;
                ShowError(cx.HResult.ToString(), cx.Message);
            }
            catch (Exception ex)
            {
                ShowError(ex.HResult.ToString(), ex.Message);
            }

            requestInProgress = null;
            return result;
        }

        #endregion

        #region Connect

        public async Task<bool> Connect(string ip, int port, bool useSSL)
        {
            if (data.ConnectedState == ConnectedStatus.Connected)
            {
                return true;
            }

            try
            {
                AddMessageToHistory(new HistoryViewModel("Connecting"));
                if (await eft.ConnectAsync(ip, port, useSSL))
                {
                    AddMessageToHistory(new HistoryViewModel("Connected"));
                    data.ConnectedState = ConnectedStatus.Connected;
                    //_checkStatusTimer.Enabled = true;
                    return true;
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.HResult.ToString(), ex.Message);
            }
            AddMessageToHistory(new HistoryViewModel("Connect Failed!"));
            return false;
        }

        public void Disconnect()
        {
            try
            {
                //_checkStatusTimer.Enabled = false;
                data.ConnectedState = ConnectedStatus.Disconnected;
                AddMessageToHistory(new HistoryViewModel("Disconnected"));
                eft.Disconnect();
            }
            catch (Exception ex)
            {
                ShowError(ex.HResult.ToString(), ex.Message);
            }
        }

        #endregion

        #region Logon
        public async Task Logon(EFTLogonRequest eftLogonRequest, bool autoApproveDialogs)
        {
            await SendRequest<EFTLogonResponse>(eftLogonRequest, autoApproveDialogs);
        }

        public async Task CloudLogon(string clientId, string password, string pairingCode)
        {
            await SendRequest<EFTCloudLogonResponse>(new EFTCloudLogonRequest()
            {
                ClientID = clientId,
                Password = password,
                PairingCode = pairingCode
            });
        }

        public async Task CloudPairingRequest(string clientId, string password, string pairingCode)
        {
            await SendRequest<EFTCloudPairResponse>(new EFTCloudPairRequest()
            {
                ClientID = clientId,
                Password = password,
                PairingCode = pairingCode
            });
        }

        public async Task<bool> CloudTokenLogon(string token)
        {
            return await SendRequest<EFTCloudTokenLogonResponse>(new EFTCloudTokenLogonRequest()
            {
                Token = token
            });
        }

        public async Task StartLogonTest(LogonType type, ReceiptCutModeType cutMode, ReceiptPrintModeType printMod)
        {
            ct = new CancellationTokenSource();
            await SpawnLogon(type, cutMode, printMod, ct.Token);
        }

        public async Task StartHeartbeatTest()
        {
            heartbeatTestCTS = new CancellationTokenSource();
            await SpawnHeartbeat(heartbeatTestCTS.Token);
        }
        

        public void StopLogonTest()
        {
            ct.Cancel();
            ct.Dispose();
        }


        public void StopHeartbeatTest()
        {
            heartbeatTestCTS.Cancel();
            heartbeatTestCTS.Dispose();
        }
        

        private async Task SpawnLogon(LogonType type, ReceiptCutModeType cutMode, ReceiptPrintModeType printMode, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await Task.Delay(10);
                await Logon(new EFTLogonRequest() { LogonType = type, CutReceipt = cutMode, ReceiptAutoPrint = printMode }, true);
            }
        }

        private async Task SpawnHeartbeat(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await SendRequest<EFTHeartbeatResponse>(new EFTHeartbeatRequest() { Reply = true }, token);
            }
        }

        #endregion

        #region ControlPanel
        public async Task DisplayControlPanel(ControlPanelType option, ReceiptCutModeType cutMode, ReceiptPrintModeType printMode)
        {
            // Bug in current EFT-Client - if the first request after connection is EFTControlPanelResponse the EFT-Client fails to respond. Work around is to have a short cancellation timeout
            await SendRequest<EFTControlPanelResponse>(new EFTControlPanelRequest()
            {
                ControlPanelType = option,
                ReceiptCutMode = cutMode,
                ReceiptPrintMode = printMode,
                ReturnType = ControlPanelReturnType.Immediately
            }, new CancellationTokenSource(new TimeSpan(0, 1, 0)).Token);
        }
        #endregion

        #region Last Transaction
        public async Task GetLastTransaction()
        {
            await SendRequest<EFTGetLastTransactionResponse>(new EFTGetLastTransactionRequest(data.OriginalTransactionReference)
            {
                Application = data.LastTransactionTerminal,
                Merchant = data.LastReceiptMerchantNumber
            });
        }

        public async Task LastReceipt(ReceiptCutModeType cutMode, ReceiptPrintModeType printMode, ReprintType type)
        {
            await SendRequest<EFTReprintReceiptResponse>(new EFTReprintReceiptRequest()
            {
                CutReceipt = cutMode,
                ReceiptAutoPrint = printMode,
                ReprintType = type,
                OriginalTxnRef = data.OriginalTransactionReference,
                Merchant = data.LastReceiptMerchantNumber,
                Application = data.LastTransactionTerminal
            });
        }
        #endregion

        #region Transaction
        public async Task DoTransaction(EFTTransactionRequest request)
        {
            if (data.PADAppendSKU)
            {
                data.PADSKUId = Guid.NewGuid().ToString("N");
                var amount = Convert.ToUInt32(request.AmtPurchase * 100);
                var count = 1;
                var random = new Random();

                // 1. Create random items until we have reached the total amount
                var basketItems = new List<EFTBasketItem>();
                var amountRemaining = amount;
                do
                {
                    var amountToAdd = (amountRemaining < 1000) ? amountRemaining : Convert.ToUInt32(random.Next(1, Convert.ToInt32(amountRemaining)));
                    basketItems.Add(new EFTBasketItem()
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        Name = $"Linkly - Product #{count++}",
                        Amount = amountToAdd
                    });
                    amountRemaining -= amountToAdd;
                } while (amountRemaining > 0);
                
                // 2. Create basket
                var basketDataCommand = new EFTBasketDataCommandCreate()
                {
                    Basket = new EFTBasket()
                    {
                        Id = data.PADSKUId,
                        Amount = amount,
                        Discount = 0,
                        Surcharge = 0,
                        Tax = 0,
                        Items = basketItems
                    }
                };

                // 3. Add to command and send
                var basketRequest = new EFTBasketDataRequest()
                {
                    Command = basketDataCommand
                };
                await SendRequest<EFTBasketDataResponse>(basketRequest);

                request.PurchaseAnalysisData.SetTag("SKU", data.PADSKUId);
            }


            bool result = await SendRequest<EFTTransactionResponse>(request);
            
        }
        #endregion

        #region ClientList

        public async Task DoClientList(EFTClientListRequest clientListRequest)
        {
            bool result = await SendRequest<EFTClientListResponse>(clientListRequest);
            if (result)
            {

            }

        }
        #endregion

        #region Set Dialog
        public async Task SetDialog(int x, int y, bool displayEvents, DialogPosition pos, string title, bool topMost, DialogType dialogType)
        {
            await SendRequest<SetDialogResponse>(new SetDialogRequest
            {
                DialogX = x,
                DialogY = y,
                DisableDisplayEvents = displayEvents,
                DialogPosition = pos,
                DialogTitle = title,
                EnableTopmost = topMost,
                DialogType = dialogType
            });
        }

        public async Task SetDialog(SetDialogRequest request)
        {
            await SendRequest<SetDialogResponse>(request);
        }

        #endregion

        #region Query Card
        public async Task QueryCard(PadField pad, QueryCardType cardType, string merchant = "00", TerminalApplication application = TerminalApplication.EFTPOS)
        {
            var request = new EFTQueryCardRequest
            {
                QueryCardType = cardType,
                PurchaseAnalysisData = pad,
                Merchant = merchant,
                Application = application
            };

            // Special case for '7'. No response event!
            if (request.QueryCardType == QueryCardType.PreSwipeStart)
            {
                AddMessageToHistory(new HistoryViewModel(request, requestInProgress, previousMessage));
                await eft.WriteRequestAsync(request);
            }
            else
            {
                await SendRequest<EFTQueryCardResponse>(request);
            }
        }

        public async Task QueryCard(PadField pad, QueryCardType cardType, string MerchantId)
        {
            await SendRequest<EFTQueryCardResponse>(new EFTQueryCardRequest
            {
                QueryCardType = cardType,
                PurchaseAnalysisData = pad,
                Merchant = MerchantId
            });
        }
        #endregion

        #region Status
        public async Task GetStatus(EFTStatusRequest request)
        {
            await SendRequest<EFTStatusResponse>(request, false);
        }
        #endregion

        #region Config Merchant
        public async Task ConfigureMerchant(int aiic, string merchantId, int nii, string terminalId, int timeout)
        {
            await SendRequest<EFTConfigureMerchantResponse>(new EFTConfigureMerchantRequest
            {
                AIIC = aiic,
                Caid = merchantId,
                NII = nii,
                Catid = terminalId,
                Timeout = timeout
            });
        }

        public async Task ConfigureMerchant(EFTConfigureMerchantRequest request)
        {
            await SendRequest<EFTConfigureMerchantResponse>(request);
        }
        #endregion

        #region Settlement
        public async Task DoSettlement(SettlementType settlement, ReceiptCutModeType cutMode,
            PadField padInfo, ReceiptPrintModeType printMode, bool resetTotals)
        {
            await SendRequest<EFTSettlementResponse>(new EFTSettlementRequest
            {
                SettlementType = settlement,
                CutReceipt = cutMode,
                PurchaseAnalysisData = padInfo,
                ReceiptAutoPrint = printMode,
                ResetTotals = resetTotals
            });
        }
        #endregion

        #region Cheque Auth
        public async Task DoVerifyCheque(EFTChequeAuthRequest request)
        {
            await SendRequest<EFTChequeAuthResponse>(request);
        }
        #endregion

        #region Slave Mode
        public async Task DoSlaveMode(string cmd)
        {
            await SendRequest<Slave.EFTSlaveResponse>(new Slave.EFTSlaveRequest
            {
                RawCommand = cmd
            });
            await Task.CompletedTask;
        }
        #endregion

        #region SendKey
        public async Task SendKey(EFTPOSKey option, string data = "")
        {
            try
            {
                EFTSendKeyRequest req = new EFTSendKeyRequest { Data = data, Key = option };
                AddMessageToHistory(new HistoryViewModel(req, requestInProgress, previousMessage));
                await eft.WriteRequestAsync(req);
            }
            catch (Exception ex)
            {
                ShowError(ex.HResult.ToString(), ex.Message);
            }
        }

        public async Task StartSendKeysTest(EFTPOSKey key)
        {
            ct = new CancellationTokenSource();

            var progress = new Progress<string>((s) =>
            {
                data.Log(s);
            });

            data.Progress = progress;

            await Task.Run(() => SpawnSendKeys(key, ct.Token, progress), ct.Token);
        }

        public void StopSendKeysTest()
        {
            ct.Cancel();
            ct.Dispose();
        }

        private async Task SpawnSendKeys(EFTPOSKey key, CancellationToken token, IProgress<string> p)
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(100);
                    EFTSendKeyRequest req = new EFTSendKeyRequest { Key = key };
                    AddMessageToHistory(new HistoryViewModel(req, requestInProgress, previousMessage));
                    await eft.WriteRequestAsync(req);
                    p.Report(_eftLogs);
                }
            }
            catch (Exception ex)
            {
                ShowError(ex.HResult.ToString(), ex.Message);
            }
        }
        #endregion

        #region PIN
        public async Task AuthPin()
        {
            await SendRequest<EFTTransactionResponse>(new EFTTransactionRequest
            {
                TxnType = TransactionType.AuthPIN
            });
        }

        public async Task ChangePin()
        {
            await SendRequest<EFTTransactionResponse>(new EFTTransactionRequest
            {
                TxnType = TransactionType.EnhancedPIN
            });
        }
        #endregion

    }
}
