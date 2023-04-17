using PCEFTPOS.EFTClient.IPInterface.Slave;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace PCEFTPOS.EFTClient.IPInterface
{
    public interface IMessageParser
    {
        EFTResponse StringToEFTResponse(string msg);
        EFTResponse XMLStringToEFTResponse(string msg);
        string EFTRequestToString(EFTRequest eftRequest);
        string EFTRequestToXMLString(EFTRequest eftRequest);
    }

    public class DefaultMessageParser : IMessageParser
    {
        ReceiptType lastReceiptType;

        enum IPClientResponseType
        {
            Logon = 'G', Transaction = 'M', QueryCard = 'J', Configure = '1', ControlPanel = '5', SetDialog = '2', Settlement = 'P',
            DuplicateReceipt = 'C', GetLastTransaction = 'N', Status = 'K', Receipt = '3', Display = 'S', GenericPOSCommand = 'X', PINRequest = 'W',
            ChequeAuth = 'H', SendKey = 'Y', ClientList = 'Q', CloudLogon = 'A', Heartbeat = 'F', Monitoring = '|'
        }

        #region StringToEFTResponse
        /// <summary> Parses a string to an EFTResponse message </summary>
        /// <param name="msg">string to parse</param>
        /// <returns>An EFTResponse message</returns>
        /// <exception cref="ArgumentException">An ArgumentException is thrown if the contents of msg is invalid</exception>
        public EFTResponse StringToEFTResponse(string msg)
        {
            if (string.IsNullOrEmpty(msg))
            {
                throw new ArgumentException("msg is null or zero length", nameof(msg));
            }

            EFTResponse eftResponse;
            switch ((IPClientResponseType)msg[0])
            {
                case IPClientResponseType.Display:
                    eftResponse = ParseDisplayResponse(msg);
                    break;
                case IPClientResponseType.Receipt:
                    eftResponse = ParseReceiptResponse(msg);
                    break;
                case IPClientResponseType.Logon:
                    eftResponse = ParseEFTLogonResponse(msg);
                    break;
                case IPClientResponseType.Transaction:
                    eftResponse = ParseEFTTransactionResponse(msg);
                    break;
                case IPClientResponseType.SetDialog:
                    eftResponse = ParseSetDialogResponse(msg);
                    break;
                case IPClientResponseType.GetLastTransaction:
                    eftResponse = ParseEFTGetLastTransactionResponse(msg);
                    break;
                case IPClientResponseType.DuplicateReceipt:
                    eftResponse = ParseEFTReprintReceiptResponse(msg);
                    break;
                case IPClientResponseType.ControlPanel:
                    eftResponse = ParseControlPanelResponse(msg);
                    break;
                case IPClientResponseType.Settlement:
                    eftResponse = ParseEFTSettlementResponse(msg);
                    break;
                case IPClientResponseType.Status:
                    eftResponse = ParseEFTStatusResponse(msg);
                    break;
                case IPClientResponseType.ChequeAuth:
                    eftResponse = ParseChequeAuthResponse(msg);
                    break;
                case IPClientResponseType.QueryCard:
                    eftResponse = ParseQueryCardResponse(msg);
                    break;
                case IPClientResponseType.GenericPOSCommand:
                    eftResponse = ParseGenericPOSCommandResponse(msg);
                    break;
                case IPClientResponseType.Configure:
                    eftResponse = ParseConfigMerchantResponse(msg);
                    break;
                case IPClientResponseType.CloudLogon:
                    eftResponse = ParseCloudLogonResponse(msg);
                    break;
                case IPClientResponseType.ClientList:
                    eftResponse = ParseClientListResponse(msg);
                    break;
                case IPClientResponseType.Heartbeat:
                    eftResponse = ParseHeartbeatResponse(msg);
                    break;
                case IPClientResponseType.Monitoring:
                    eftResponse = ParseMonitoringResponse(msg);
                    break;
                case IPClientResponseType.PINRequest:
                    // 'W' response come from the EFT-Client when Transaction messages with a TXN Type of 'K' (EnhancedPIN) or 'X' (AuthPIN)
                    // are sent to it.
                    eftResponse = ParseEFTTransactionResponse(msg);
                    break;
                case IPClientResponseType.SendKey:
                default:
                    throw new ArgumentException($"Unknown message type: {msg}", nameof(msg));
            }

            return eftResponse;
        }

        static T TryParse<T>(string input, ref int index)
        {
            return TryParse<T>(input, input.Length - index, ref index);
        }
        
        static T TryParse<T>(string input, int length, ref int index)
        {
            return TryParse<T>(input, length, ref index, "");
        }

        static T TryParse<T>(string input, int length, ref int index, string format)
        {
            T result = default;

            if (input.Length - index >= length)
            {
                if (result is bool && length == 1)
                {
                    result = (T)Convert.ChangeType((input[index] == '1' || input[index] == 'Y'), typeof(T));
                    index += length;
                }
                else
                {
                    object data = input.Substring(index, length);
                    try
                    {
                        if (result is Enum && length == 1)
                            result = (T)Enum.ToObject(typeof(T), ((string)data)[0]);
                        else if (result is DateTime && format.Length > 1)
                            result = (T)(object)DateTime.ParseExact((string)data, format, null);
                        else
                            result = (T)Convert.ChangeType(data, typeof(T));
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex);
                    }
                    finally
                    {
                        index += length;
                    }
                }
            }
            else
                index = length;

            return result;
        }

        static EFTResponse ParseEFTTransactionResponse(string msg)
        {
            var index = 1;

            var r = new EFTTransactionResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);
            r.Merchant = TryParse<string>(msg, 2, ref index);
            r.TxnType = TryParse<TransactionType>(msg, 1, ref index);
            r.AccountType = r.AccountType.FromString(TryParse<string>(msg, 7, ref index));
            r.AmtCash = TryParse<decimal>(msg, 9, ref index) / 100;
            r.AmtPurchase = TryParse<decimal>(msg, 9, ref index) / 100;
            r.AmtTip = TryParse<decimal>(msg, 9, ref index) / 100;
            r.AuthCode = TryParse<int>(msg, 6, ref index);
            r.TxnRef = TryParse<string>(msg, 16, ref index);
            r.Stan = TryParse<int>(msg, 6, ref index);
            r.Caid = TryParse<string>(msg, 15, ref index);
            r.Catid = TryParse<string>(msg, 8, ref index);
            r.DateExpiry = TryParse<string>(msg, 4, ref index);
            r.DateSettlement = TryParse<DateTime>(msg, 4, ref index, "ddMM");
            r.Date = TryParse<DateTime>(msg, 12, ref index, "ddMMyyHHmmss");
            r.CardType = TryParse<string>(msg, 20, ref index);
            r.Pan = TryParse<string>(msg, 20, ref index);
            r.Track2 = TryParse<string>(msg, 40, ref index);
            r.RRN = TryParse<string>(msg, 12, ref index);
            r.CardName = TryParse<int>(msg, 2, ref index);
            r.TxnFlags = new TxnFlags(TryParse<string>(msg, 8, ref index).ToCharArray());
            r.BalanceReceived = TryParse<bool>(msg, 1, ref index);
            r.AvailableBalance = TryParse<decimal>(msg, 9, ref index) / 100;
            r.ClearedFundsBalance = TryParse<decimal>(msg, 9, ref index) / 100;
            r.PurchaseAnalysisData = new PadField(TryParse<string>(msg, ref index));

            return r;
        }

        static EFTResponse ParseEFTGetLastTransactionResponse(string msg)
        {
            var index = 1;

            var r = new EFTGetLastTransactionResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.LastTransactionSuccess = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);
            r.Merchant = TryParse<string>(msg, 2, ref index);

            if (char.IsLower(msg[index]))
            {
                r.IsTrainingMode = true;
                msg = msg.Substring(0, index) + char.ToUpper(msg[index]) + msg.Substring(index + 1);
            }
            r.TxnType = TryParse<TransactionType>(msg, 1, ref index);
            r.AccountType = r.AccountType.FromString(TryParse<string>(msg, 7, ref index));
            r.AmtCash = TryParse<decimal>(msg, 9, ref index) / 100;
            r.AmtPurchase = TryParse<decimal>(msg, 9, ref index) / 100;
            r.AmtTip = TryParse<decimal>(msg, 9, ref index) / 100;
            r.AuthCode = TryParse<int>(msg, 6, ref index);
            r.TxnRef = TryParse<string>(msg, 16, ref index);
            r.Stan = TryParse<int>(msg, 6, ref index);
            r.Caid = TryParse<string>(msg, 15, ref index);
            r.Catid = TryParse<string>(msg, 8, ref index);
            r.DateExpiry = TryParse<string>(msg, 4, ref index);
            r.DateSettlement = TryParse<DateTime>(msg, 4, ref index, "ddMM");
            r.BankDateTime = TryParse<DateTime>(msg, 12, ref index, "ddMMyyHHmmss");
            r.CardType = TryParse<string>(msg, 20, ref index);
            r.Pan = TryParse<string>(msg, 20, ref index);
            r.Track2 = TryParse<string>(msg, 40, ref index);
            r.RRN = TryParse<string>(msg, 12, ref index);
            r.CardName = TryParse<int>(msg, 2, ref index);
            r.TxnFlags = new TxnFlags(TryParse<string>(msg, 8, ref index).ToCharArray());
            r.BalanceReceived = TryParse<bool>(msg, 1, ref index);
            r.AvailableBalance = TryParse<decimal>(msg, 9, ref index) / 100;
            r.ClearedFundsBalance = TryParse<decimal>(msg, 9, ref index) / 100;
            r.PurchaseAnalysisData = new PadField(TryParse<string>(msg, ref index));

            return r;
        }

        static EFTResponse ParseSetDialogResponse(string msg)
        {
            var index = 1;

            var r = new SetDialogResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);

            return r;
        }

        static EFTResponse ParseEFTLogonResponse(string msg)
        {
            var index = 1;

            var r = new EFTLogonResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);

            if (msg.Length > 25)
            {
                r.Catid = TryParse<string>(msg, 8, ref index);
                r.Caid = TryParse<string>(msg, 15, ref index);
                r.Date = TryParse<DateTime>(msg, 12, ref index, "ddMMyyHHmmss");
                r.Stan = TryParse<int>(msg, 6, ref index);
                r.PinPadVersion = TryParse<string>(msg, 16, ref index);
                r.PurchaseAnalysisData = new PadField(TryParse<string>(msg, ref index));
            }
            return r;
        }

        static EFTResponse ParseDisplayResponse(string msg)
        {
            int index = 1;

            var r = new EFTDisplayResponse();
            index++; // Skip sub code.
            r.NumberOfLines = TryParse<int>(msg, 2, ref index);
            r.LineLength = TryParse<int>(msg, 2, ref index);
            for (int i = 0; i < r.NumberOfLines; i++)
                r.DisplayText[i] = TryParse<string>(msg, r.LineLength, ref index);
            r.CancelKeyFlag = TryParse<bool>(msg, 1, ref index);
            r.AcceptYesKeyFlag = TryParse<bool>(msg, 1, ref index);
            r.DeclineNoKeyFlag = TryParse<bool>(msg, 1, ref index);
            r.AuthoriseKeyFlag = TryParse<bool>(msg, 1, ref index);
            r.InputType = TryParse<InputType>(msg, 1, ref index);
            r.OKKeyFlag = TryParse<bool>(msg, 1, ref index);
            index += 2;
            r.GraphicCode = TryParse<GraphicCode>(msg, 1, ref index);
            int padLength = TryParse<int>(msg, 3, ref index);
            r.PurchaseAnalysisData = new PadField(TryParse<string>(msg, padLength, ref index));

            return r;
        }

        static EFTResponse ParseMonitoringResponse(string msg)
        {
            int index = 1;

            var r = new EFTMonitoringResponse();
            r.MonitoringType = TryParse<char>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);
            r.Merchant = TryParse<string>(msg, 2, ref index);
            r.ProductCode = TryParse<string>(msg, 2, ref index);
            var versionStr = TryParse<string>(msg, 12, ref index);
            r.Version = new Version(versionStr.Trim());
            r.Data = msg.Substring(index); // the rest of the message is the data

            return r;
        }

        EFTResponse ParseReceiptResponse(string msg)
        {
            int index = 1;

            var r = new EFTReceiptResponse
            {
                Type = TryParse<ReceiptType>(msg, 1, ref index)
            };

            r.IsPrePrint = (r.Type != ReceiptType.ReceiptText);

            // Return early for a pre-print
            if (r.IsPrePrint)
            {
                lastReceiptType = r.Type;
                return r;
            }

            // Unpack receipt text from \r\n separated string to an array of string
            var receiptLines = new List<string>();
            bool done = false;
            while (!done)
            {
                var lineLength = msg.IndexOf("\r\n", index) - index;
                if (lineLength > 0)
                {
                    receiptLines.Add(msg.Substring(index, lineLength));
                    index += lineLength + 2;
                    if (index >= msg.Length)
                        done = true;
                }
                else
                { 
                    done = true;
                }
            }

            r.ReceiptText = receiptLines.ToArray();
            r.Type = lastReceiptType;

            return r;
        }

        static EFTResponse ParseControlPanelResponse(string msg)
        {
            int index = 1;

            var r = new EFTControlPanelResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);

            return r;
        }

        static EFTResponse ParseEFTReprintReceiptResponse(string msg)
        {
            int index = 1;

            var r = new EFTReprintReceiptResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);
            // index will be 20 if the TryParse above fails i.e. if the msg is shorter than index + 20. So index is 20 if there is no more message. If there's no more message then we return what we have and don't try to parse a receipt.
            if (index == 20)
            {
                return r;
            }
            
            // If we get here then there is more message and so we should try to parse a receipt.
            var receiptLines = new List<string>();
            bool done = false;
            while (!done)
            {
                var lineLength = msg.IndexOf("\r\n", index) - index;
                if (lineLength > 0)
                {
                    receiptLines.Add(msg.Substring(index, lineLength));
                    index += lineLength + 2;
                    if (index >= msg.Length)
                        done = true;
                }
                else
                {
                    done = true;
                }
            }

            r.ReceiptText = receiptLines.ToArray();

            return r;
        }

        static EFTResponse ParseEFTSettlementResponse(string msg)
        {
            var index = 1;

            var r = new EFTSettlementResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);

            r.SettlementData = TryParse<string>(msg, ref index);

            return r;
        }

        static EFTResponse ParseQueryCardResponse(string msg)
        {
            int index = 1;

            var r = new EFTQueryCardResponse
            {
                AccountType = (AccountType)msg[index++],
                Success = TryParse<bool>(msg, 1, ref index),
                ResponseCode = TryParse<string>(msg, 2, ref index),
                ResponseText = TryParse<string>(msg, 20, ref index)
            };

            r.Track2 = TryParse<string>(msg, 40, ref index);

            string track1or3 = TryParse<string>(msg, 80, ref index);
            char trackFlag = TryParse<char>(msg, 1, ref index);
            switch (trackFlag)
            {
                case '1':
                    r.TrackFlags = TrackFlags.Track1;
                    r.Track1 = track1or3;
                    break;
                case '2':
                    r.TrackFlags = TrackFlags.Track2;
                    break;
                case '3':
                    r.TrackFlags = TrackFlags.Track1 | TrackFlags.Track2;
                    r.Track1 = track1or3;
                    break;
                case '4':
                    r.TrackFlags = TrackFlags.Track3;
                    r.Track3 = track1or3;
                    break;
                case '6':
                    r.TrackFlags = TrackFlags.Track2 | TrackFlags.Track3;
                    r.Track3 = track1or3;
                    break;
            }

            r.CardName = TryParse<int>(msg, 2, ref index);
            r.PurchaseAnalysisData = new PadField(TryParse<string>(msg, ref index));

            return r;
        }

        static EFTResponse ParseClientListResponse(string msg)
        {
            var r = new EFTClientListResponse();
            // Get rid of the junk at the beginning, will look like #Q000001
            string trimmedMsg = msg.Substring(msg.IndexOf('{') + 1);
            // Each client coming in will be in format {CLIENTNAME,IPADDRESS,PORT,STATUS}|{CLIENTNAME,IPADDRESS,PORT,STATUS} So we split on the | character
            if (trimmedMsg.IndexOf('|') > -1)
            {
                string[] clients = trimmedMsg.Split('|');
                foreach (string client in clients)
                {
                    EFTClientListResponse.EFTClient newClient = new EFTClientListResponse.EFTClient();
                    // Each client 'string' will be surrounded by {} so we get rid of them
                    string trimmedClient = client.TrimEnd('}').TrimStart('{');
                    // That leaves us with the CLIENTNAME,IPADDRESS,PORT,STATUS and we split on ',' to get individual properties
                    string[] newClientProps = trimmedClient.Split(',');
                    newClient.Name = newClientProps[0];
                    newClient.IPAddress = newClientProps[1];
                    newClient.Port = Convert.ToInt32(newClientProps[2]);
                    // EFTClientListResponse.Client.State is an enum, so we need to check the string and parse into the enum
                    if (newClientProps[3].Equals("AVAILABLE"))
                    {
                        newClient.State = EFTClientListResponse.EFTClientState.Available;
                    }
                    else
                        newClient.State = EFTClientListResponse.EFTClientState.Unavailable;
                    // Add the new client to the response list
                    r.EFTClients.Add(newClient);
                }
            }
            else if (!msg.Contains('|'))
            {
                EFTClientListResponse.EFTClient newClient = new EFTClientListResponse.EFTClient();
                // Each client 'string' will be surrounded by {} so we get rid of them
                string trimmedClient = trimmedMsg.TrimEnd('}').TrimStart('{');
                // That leaves us with the CLIENTNAME,IPADDRESS,PORT,STATUS and we split on ',' to get individual properties
                string[] newClientProps = trimmedClient.Split(',');
                newClient.Name = newClientProps[0];
                newClient.IPAddress = newClientProps[1];
                newClient.Port = Convert.ToInt32(newClientProps[2]);
                // EFTClientListResponse.Client.State is an enum, so we need to check the string and parse into the enum
                if (newClientProps[3].Equals("AVAILABLE"))
                {
                    newClient.State = EFTClientListResponse.EFTClientState.Available;
                }
                else
                    newClient.State = EFTClientListResponse.EFTClientState.Unavailable;
                // Add the new client to the response list
                r.EFTClients.Add(newClient);
            }
            return r;
        }

        static EFTResponse ParseHeartbeatResponse(string msg)
        {
            int index = 1;

            var r = new EFTHeartbeatResponse()
            {
                Subcode = TryParse<char>(msg, 1, ref index),
                Success = true
            };
            return r;
        }

        static EFTResponse ParseEFTConfigureMerchantResponse(string msg)
        {
            int index = 1;

            var r = new EFTConfigureMerchantResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);

            return r;
        }

        static EFTResponse ParseEFTStatusResponse(string msg)
        {
            int index = 1;

            var r = new EFTStatusResponse
            {
                StatusType = TryParse<StatusType>(msg, 1, ref index),
                Success = TryParse<bool>(msg, 1, ref index),
                ResponseCode = TryParse<string>(msg, 2, ref index),
                ResponseText = TryParse<string>(msg, 20, ref index)
            };

            if (r.StatusType == StatusType.Standard) // todo: subcodes 1-5 should be unpacked
            {
                if (index >= msg.Length) return r;
                r.Merchant = TryParse<string>(msg, 2, ref index);
                r.AIIC = TryParse<string>(msg, 11, ref index);
                r.NII = TryParse<int>(msg, 3, ref index);
                r.Caid = TryParse<string>(msg, 15, ref index);
                r.Catid = TryParse<string>(msg, 8, ref index);
                r.Timeout = TryParse<int>(msg, 3, ref index);
                r.LoggedOn = TryParse<bool>(msg, 1, ref index);
                r.PinPadSerialNumber = TryParse<string>(msg, 16, ref index);
                r.PinPadVersion = TryParse<string>(msg, 16, ref index);
                r.BankDescription = TryParse<string>(msg, 32, ref index);
                int padLength = TryParse<int>(msg, 3, ref index);
                if (msg.Length - index < padLength)
                    return r;

                r.SAFCount = TryParse<int>(msg, 4, ref index);
                r.NetworkType = TryParse<NetworkType>(msg, 1, ref index);
                r.HardwareSerial = TryParse<string>(msg, 16, ref index);
                r.RetailerName = TryParse<string>(msg, 40, ref index);
                r.OptionsFlags = ParseStatusOptionFlags(msg.Substring(index, 32).ToCharArray()); index += 32;
                r.SAFCreditLimit = TryParse<decimal>(msg, 9, ref index) / 100;
                r.SAFDebitLimit = TryParse<decimal>(msg, 9, ref index) / 100;
                r.MaxSAF = TryParse<int>(msg, 3, ref index);
                r.KeyHandlingScheme = ParseKeyHandlingType(msg[index++]);
                r.CashoutLimit = TryParse<decimal>(msg, 9, ref index) / 100;
                r.RefundLimit = TryParse<decimal>(msg, 9, ref index) / 100;
                r.CPATVersion = TryParse<string>(msg, 6, ref index);
                r.NameTableVersion = TryParse<string>(msg, 6, ref index);
                r.TerminalCommsType = ParseTerminalCommsType(msg[index++]);
                r.CardMisreadCount = TryParse<int>(msg, 6, ref index);
                r.TotalMemoryInTerminal = TryParse<int>(msg, 4, ref index);
                r.FreeMemoryInTerminal = TryParse<int>(msg, 4, ref index);
                r.EFTTerminalType = ParseEFTTerminalType(msg.Substring(index, 4)); index += 4;
                r.NumAppsInTerminal = TryParse<int>(msg, 2, ref index);
                r.NumLinesOnDisplay = TryParse<int>(msg, 2, ref index);
                r.HardwareInceptionDate = TryParse<DateTime>(msg, 6, ref index, "ddMMyy");
            }
            return r;
        }

        static TerminalCommsType ParseTerminalCommsType(char CommsType)
        {
            TerminalCommsType commsType = TerminalCommsType.Unknown;

            if (CommsType == '0') commsType = TerminalCommsType.Cable;
            else if (CommsType == '1') commsType = TerminalCommsType.Infrared;

            return commsType;
        }

        static KeyHandlingType ParseKeyHandlingType(char KeyHandlingScheme)
        {
            KeyHandlingType keyHandlingType = KeyHandlingType.Unknown;

            if (KeyHandlingScheme == '0') keyHandlingType = KeyHandlingType.SingleDES;
            else if (KeyHandlingScheme == '1') keyHandlingType = KeyHandlingType.TripleDES;

            return keyHandlingType;
        }

        static EFTTerminalType ParseEFTTerminalType(string terminalType)
        {
            var eftTerminalType = EFTTerminalType.Unknown;
            switch(terminalType.ToLower())
            {
                case "albt": eftTerminalType = EFTTerminalType.Albert;                break;

                case "0006": eftTerminalType = EFTTerminalType.IngenicoIxx250;        break;
                case "0062": eftTerminalType = EFTTerminalType.IngenicoNPT710;        break;
                case "0069": eftTerminalType = EFTTerminalType.IngenicoPX328;         break;
                case "5100": eftTerminalType = EFTTerminalType.Ingenicoi5100;         break;
                case "5110": eftTerminalType = EFTTerminalType.Ingenicoi5110;         break;
                case "7010": eftTerminalType = EFTTerminalType.Ingenicoi3070;         break;
                case "i050": eftTerminalType = EFTTerminalType.IngenicoMove5000;      break;
                case "i051": eftTerminalType = EFTTerminalType.IngenicoMove3500;      break;
                case "i052": eftTerminalType = EFTTerminalType.IngenicoMove2500;      break;
                case "i060": eftTerminalType = EFTTerminalType.IngenicoDesk5000;      break;
                case "i061": eftTerminalType = EFTTerminalType.IngenicoDesk3000;      break;
                case "i062": eftTerminalType = EFTTerminalType.IngenicoDesk1000;      break;
                case "i070": eftTerminalType = EFTTerminalType.IngenicoLane7000;      break;
                case "i071": eftTerminalType = EFTTerminalType.IngenicoLane5000;      break;
                case "i072": eftTerminalType = EFTTerminalType.IngenicoLane3000;      break;
                case "i080": eftTerminalType = EFTTerminalType.IngenicoAxiumD7;       break;
                case "ia80": eftTerminalType = EFTTerminalType.IngenicoA8;            break;

                case "x300": eftTerminalType = EFTTerminalType.PaxA30;                break;
                case "x770": eftTerminalType = EFTTerminalType.PaxA77;                break;
                case "x920": eftTerminalType = EFTTerminalType.PaxA920;               break;
                case "x92p": eftTerminalType = EFTTerminalType.PaxA920Pro;            break;

                case "p010": eftTerminalType = EFTTerminalType.PCEFTPOSVirtualPinpad; break;

                case "sp2l": eftTerminalType = EFTTerminalType.SunmiP2Lite;           break;
                case "sp2p": eftTerminalType = EFTTerminalType.SunmiP2Pro;            break;

                case "0820": eftTerminalType = EFTTerminalType.VerifoneVx820;         break;
                case "p400": eftTerminalType = EFTTerminalType.VerifoneP400;          break; //Woolies special
                case "v050": eftTerminalType = EFTTerminalType.VerifoneP400;          break;
                case "v051": eftTerminalType = EFTTerminalType.VerifoneP200;          break;
                case "v060": eftTerminalType = EFTTerminalType.VerifoneP400C;         break;
                case "v061": eftTerminalType = EFTTerminalType.VerifoneP200C;         break;
                case "v070": eftTerminalType = EFTTerminalType.VerifoneCarbon10;      break;
                case "v071": eftTerminalType = EFTTerminalType.VerifoneCarbon8;       break;
                case "v072": eftTerminalType = EFTTerminalType.VerifoneCarbon5;       break;
                case "v424": eftTerminalType = EFTTerminalType.VerifoneM424;          break;
                case "v630": eftTerminalType = EFTTerminalType.VerifoneP630;          break;
                case "v650": eftTerminalType = EFTTerminalType.VerifoneT650P;         break;
                case "x690": eftTerminalType = EFTTerminalType.VerifoneVx690;         break;

                case "z001": eftTerminalType = EFTTerminalType.ZellerNewPos9220;      break;
            }

            return eftTerminalType;
        }

        static PINPadOptionFlags ParseStatusOptionFlags(char[] Flags)
        {
            PINPadOptionFlags flags = 0;
            int index = 0;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Tipping;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.PreAuth;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Completions;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.CashOut;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Refund;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Balance;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Deposit;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Voucher;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.MOTO;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.AutoCompletion;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.EFB;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.EMV;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Training;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Withdrawal;
            if (Flags[index++] == '1') flags |= PINPadOptionFlags.Transfer;
            if (Flags[index] == '1') flags |= PINPadOptionFlags.StartCash;
            return flags;
        }

        static EFTResponse ParseChequeAuthResponse(string msg)
        {
            int index = 1;

            var r = new EFTChequeAuthResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);

            if (msg.Length > 25)
            {
                r.Merchant = TryParse<string>(msg, 2, ref index);
                try { r.Amount = decimal.Parse(msg.Substring(index, 9)) / 100; }
                catch { r.Amount = 0; }
                finally { index += 9; }
                try { r.AuthNumber = int.Parse(msg.Substring(index, 6)); }
                catch { r.AuthNumber = 0; }
                finally { index += 6; }
                r.ReferenceNumber = msg.Substring(index, 12); index += 12;
            }

            return r;
        }

        static EFTResponse ParseGenericPOSCommandResponse(string msg)
        {
            // Validate response length
            if (string.IsNullOrEmpty(msg))
            {
                return null;
            }

            int index = 1;

            CommandType commandType = (CommandType)msg[index++];
            switch (commandType)
            {
                case CommandType.GetPassword:
                    var pwdResponse = new EFTGetPasswordResponse
                    {
                        ResponseCode = msg.Substring(index, 2)
                    };
                    index += 2;
                    pwdResponse.Success = pwdResponse.ResponseCode == "00";
                    pwdResponse.ResponseText = msg.Substring(index, 20); index += 20;

                    if (msg.Length > 25)
                    {
                        int pwdLength = 0;
                        try { pwdLength = int.Parse(msg.Substring(index, 2)); }
                        finally { index += 2; }
                        pwdResponse.Password = msg.Substring(index, pwdLength);
                        index += pwdLength;
                    }
                    return pwdResponse;
                case CommandType.Slave:
                    var slaveResponse = new EFTSlaveResponse
                    {
                        ResponseCode = msg.Substring(index, 2)
                    };
                    index += 2;
                    slaveResponse.Response = msg.Substring(index);
                    return slaveResponse;

                case CommandType.PayAtTable:
                    var patResponse = new EFTPayAtTableResponse();
                    index = 22;

                    var headerLength = msg.Substring(index, 6); index += 6;
                    int.TryParse(headerLength, out int len);

                    patResponse.Header = msg.Substring(index, len); index += len;
                    patResponse.Content = msg.Substring(index, msg.Length - index);

                    return patResponse;

                case CommandType.BasketData:
                    return ParseBasketDataResponse(msg);
            }

            return null;
        }

        static EFTResponse ParseConfigMerchantResponse(string msg)
        {
            int index = 1;

            var r = new EFTConfigureMerchantResponse();
            index++; // Skip sub code.
            r.Success = TryParse<bool>(msg, 1, ref index);
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);

            return r;
        }

        static EFTResponse ParseCloudLogonResponse(string msg)
        {
            int index = 1;
            char subcode = TryParse<char>(msg, 1, ref index);
            if (subcode == 'P')
            {
                var resp = new EFTCloudPairResponse
                {
                    Success = TryParse<bool>(msg, 1, ref index),
                    ResponseCode = TryParse<string>(msg, 2, ref index),
                    ResponseText = TryParse<string>(msg, 20, ref index)
                };

                // v1 cloud would return 6 byte "redirect port" followed by 3 byte "redirect address length"
                // v2 cloud 2 removed these. temp fix here is to support both

                var port = TryParse<int>(msg, 6, ref index);
                if(port != 443)
                {
                    index -= 6;
                }
                else // Cloud v1
                {
                    var addressLength = TryParse<int>(msg, 3, ref index);
                    TryParse<string>(msg, addressLength, ref index);
                }

                int tokenLength = TryParse<int>(msg, 3, ref index);
                resp.Token = TryParse<string>(msg, tokenLength, ref index);
                return resp;
            }
            else if (subcode == 'T')
            {
                return new EFTCloudTokenLogonResponse
                {
                    Success = TryParse<bool>(msg, 1, ref index),
                    ResponseCode = TryParse<string>(msg, 2, ref index),
                    ResponseText = TryParse<string>(msg, 20, ref index)
                };
            }

            return new EFTCloudLogonResponse
            {
                Success = TryParse<bool>(msg, 1, ref index),
                ResponseCode = TryParse<string>(msg, 2, ref index),
                ResponseText = TryParse<string>(msg, 20, ref index)
            };
        }

        static EFTResponse ParseBasketDataResponse(string msg)
        {
            int index = 1; // msg[0] is the command code

            var r = new EFTBasketDataResponse();
            index++; // Skip sub code.
            r.ResponseCode = TryParse<string>(msg, 2, ref index);
            r.ResponseText = TryParse<string>(msg, 20, ref index);

            r.Success = r.ResponseCode == "00";

            return r;
        }


        /// <summary>
        /// Convert a PC-EFTPOS message (e.g. #0010K0000) to a human readable debug string
        /// </summary>
        public static string MsgToDebugString(string msg)
        {
            if (msg == null || msg.Length < 2)
            {
                return $"Unable to parse msg{Environment.NewLine}ContentLength={msg?.Length ?? 0}{Environment.NewLine}Content={msg}";
            }

            // Remove the header if one exists
            if (msg[0] == '#')
            {
                if (msg.Length < 7)
                    return $"Unable to parse msg{Environment.NewLine}ContentLength={msg?.Length ?? 0}{Environment.NewLine}Content={msg}";

                msg = msg.Substring(5);
            }

            var messageParser = new DefaultMessageParser();
            try
            {
                var eftResponse = messageParser.StringToEFTResponse(msg);
                return PrintProperties(eftResponse);
            }
            catch (ArgumentException)
            {
                // Try StringToEFTRequest()
                return $"Unable to parse msg{Environment.NewLine}ContentLength={msg.Length}{Environment.NewLine}Content={msg}";
            }
        }

        static string PrintProperties(object obj)
        {
            if (obj == null)
                return "NULL";

            var sb = new StringBuilder();

            var objType = obj.GetType();
            var properties = objType.GetProperties();
            foreach (var p in properties)
            {
                sb.AppendFormat("{0}: {1}", p.Name, p.ToString());
            }

            return sb.ToString();
        }
        #endregion

        #region EFTRequestToString

        public string EFTRequestToString(EFTRequest eftRequest)
        {
            // Build the request string.
            var request = BuildRequest(eftRequest);
            var len = request.Length + 5;
            request.Insert(0, '#');
            request.Insert(1, len.PadLeft(4));
            return request.ToString();
        }

        private static StringBuilder BuildRequest(EFTRequest eftRequest)
        {
            switch (eftRequest)
            {
                case EFTMonitoringRequest r:
                    return BuildEFTMonitoringRequest(r);
                case EFTLogonRequest r:
                    return BuildEFTLogonRequest(r);
                case EFTTransactionRequest r:
                    return BuildEFTTransactionRequest(r);
                case EFTGetLastTransactionRequest r:
                    return BuildEFTGetLastTransactionRequest(r);
                case EFTReprintReceiptRequest r:
                    return BuildEFTReprintReceiptRequest(r);
                case SetDialogRequest r:
                    return BuildSetDialogRequest(r);
                case EFTControlPanelRequest r:
                    return BuildControlPanelRequest(r);
                case EFTSettlementRequest r:
                    return BuildSettlementRequest(r);
                case EFTStatusRequest r:
                    return BuildStatusRequest(r);
                case EFTChequeAuthRequest r:
                    return BuildChequeAuthRequest(r);
                case EFTQueryCardRequest r:
                    return BuildQueryCardRequest(r);
                case EFTGetPasswordRequest r:
                    return BuildGetPasswordRequest(r);
                case EFTSlaveRequest r:
                    return BuildSlaveRequest(r);
                case EFTConfigureMerchantRequest r:
                    return BuildConfigMerchantRequest(r);
                case EFTCloudLogonRequest r:
                    return BuildCloudLogonRequest(r);
                case EFTCloudPairRequest r:
                    return BuildCloudPairRequest(r);
                case EFTCloudTokenLogonRequest r:
                    return BuildCloudTokenLogonRequest(r);
                case EFTClientListRequest r:
                    return BuildGetClientListRequest(r);
                case EFTSendKeyRequest r:
                    return BuildSendKeyRequest(r);
                case EFTReceiptRequest r:
                    return BuildReceiptRequest(r);
                case EFTPayAtTableRequest r:
                    return BuildPayAtTableRequest(r);
                case EFTBasketDataRequest r:
                    return BuildBasketDataRequest(r);
                case EFTHeartbeatRequest r:
                    return BuildHeartbeatRequest(r);
                default:
                    break;
            }

            throw new ArgumentException("Unknown EFTRequest type", nameof(eftRequest));
        }

        static StringBuilder BuildEFTTransactionRequest(EFTTransactionRequest v)
        {
            var r = new StringBuilder();

            // 
            if (v.TxnType == TransactionType.EnhancedPIN || v.TxnType == TransactionType.AuthPIN)
                r.Append(EFTRequestCommandCode.PINAuth);
            else
                r.Append(EFTRequestCommandCode.Transaction);

            r.Append('0');
            r.Append(v.Merchant.PadRightAndCut(2));
            r.Append((char)v.TxnType);
            r.Append(v.TrainingMode ? '1' : '0');
            r.Append(v.EnableTip ? '1' : '0');
            r.Append(v.AmtCash.PadLeftAsInt(9));
            r.Append(v.AmtPurchase.PadLeftAsInt(9));
            r.Append(v.AuthCode.PadLeft(6));
            r.Append(v.TxnRef.PadRightAndCut(16));
            r.Append((char)v.ReceiptAutoPrint);
            r.Append((char)v.CutReceipt);
            r.Append((char)v.PanSource);
            r.Append(v.Pan.PadRightAndCut(20));
            r.Append(v.DateExpiry.PadRightAndCut(4));
            r.Append(v.Track2.PadRightAndCut(40));
            r.Append((char)v.AccountType);
            r.Append(v.Application.ToApplicationString());
            r.Append(v.RRN.PadRightAndCut(12));

            if (v.TxnType == TransactionType.EnhancedPIN || v.TxnType == TransactionType.AuthPIN)
            {
                // these message types have a different format than usual. The only thing included
                // after the RRN is the DataField
                r.Append(v.DataField.Length.PadLeft(3));
                r.Append(v.DataField);
            }
            else
            {
                r.Append(v.CurrencyCode.PadRightAndCut(3));
                r.Append((char)v.OriginalTxnType);
                r.Append(v.Date != null ? v.Date.Value.ToString("ddMMyy") : "      ");
                r.Append(v.Time != null ? v.Time.Value.ToString("HHmmss") : "      ");
                r.Append(" ".PadRightAndCut(8)); // Reserved
                r.Append(v.PurchaseAnalysisData.GetAsString(true));
            }

            return r;
        }

        static StringBuilder BuildEFTLogonRequest(EFTLogonRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.Logon);
            r.Append((char)v.LogonType);
            r.Append(v.Merchant.PadRightAndCut(2));
            r.Append((char)v.ReceiptAutoPrint);
            r.Append((char)v.CutReceipt);
            r.Append(v.Application.ToApplicationString());
            r.Append(v.PurchaseAnalysisData.GetAsString(true));
            return r;
        }

        static StringBuilder BuildEFTMonitoringRequest(EFTMonitoringRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.Monitoring);
            r.Append(v.MonitoringType);
            r.Append(v.Merchant.PadRightAndCut(2));
            r.Append(v.Application.ToApplicationString());
            r.Append(v.ProductCode);
            r.Append($"{v.Version.Major}.{v.Version.Minor}.{v.Version.Revision}".PadRightAndCut(12));
            if (v.Data.Length > 9994 - r.Length) // 9994 to account for the '#' and length field at the start of *every* message
            {
                throw new ArgumentException($"{nameof(v.Data)} exceeds max size of {9994 - r.Length}", nameof(v));
            }

            r.Append(v.Data);
            return r;
        }

        static StringBuilder BuildEFTReprintReceiptRequest(EFTReprintReceiptRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.ReprintReceipt);
            r.Append((char)v.ReprintType);
            r.Append(v.Merchant.PadRightAndCut(2));
            r.Append((char)v.CutReceipt);
            r.Append((char)v.ReceiptAutoPrint);
            r.Append(v.Application.ToApplicationString());
            r.Append(v.OriginalTxnRef.Length > 0 ? v.OriginalTxnRef.PadRightAndCut(16) : "");
            return r;
        }

        static StringBuilder BuildEFTGetLastTransactionRequest(EFTGetLastTransactionRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.GetLastTransaction);
            r.Append('0');
            r.Append(v.Application.ToApplicationString());
            r.Append(v.Merchant.PadRightAndCut(2));
            r.Append(v.TxnRef.Length > 0 ? v.TxnRef.PadRightAndCut(16) : "");
            return r;
        }

        static StringBuilder BuildSetDialogRequest(SetDialogRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.SetDialog);
            r.Append(v.DisableDisplayEvents ? '5' : ' ');
            r.Append((char)v.DialogType);
            r.Append(v.DialogX.PadLeft(4));
            r.Append(v.DialogY.PadLeft(4));
            r.Append(v.DialogPosition.ToString().PadRightAndCut(12));
            r.Append(v.EnableTopmost ? '1' : '0');
            r.Append(v.DialogTitle.PadRightAndCut(32));
            return r;
        }

        static StringBuilder BuildControlPanelRequest(EFTControlPanelRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.DisplayControlPanel); // ControlPanel
            r.Append((char)v.ControlPanelType);
            r.Append((char)v.ReceiptAutoPrint);
            r.Append((char)v.CutReceipt);
            r.Append((char)v.ReturnType);
            return r;
        }

        static StringBuilder BuildSettlementRequest(EFTSettlementRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.Settlement);
            r.Append((char)v.SettlementType);
            r.Append(v.Merchant.PadRightAndCut(2));
            r.Append((char)v.ReceiptAutoPrint);
            r.Append((char)v.CutReceipt);
            r.Append(v.ResetTotals ? '1' : '0');
            r.Append(v.Application.ToApplicationString());
            r.Append(v.PurchaseAnalysisData.GetAsString(true));
            return r;
        }

        static StringBuilder BuildQueryCardRequest(EFTQueryCardRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.QueryCard);
            r.Append((char)v.QueryCardType);
            r.Append(v.Application.ToApplicationString());
            r.Append(v.Merchant.PadRightAndCut(2));
            r.Append(v.PurchaseAnalysisData.GetAsString(true));
            return r;
        }

        static StringBuilder BuildConfigMerchantRequest(EFTConfigureMerchantRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.ConfigureMerchant);
            r.Append('0');
            r.Append(v.Merchant.PadRightAndCut(2));
            r.Append(v.AIIC.PadLeft(11));
            r.Append(v.NII.PadLeft(3));
            r.Append(v.Caid.PadRightAndCut(15));
            r.Append(v.Catid.PadRightAndCut(8));
            r.Append(v.Timeout.PadLeft(3));
            r.Append(v.Application.ToApplicationString());
            return r;
        }

        static StringBuilder BuildStatusRequest(EFTStatusRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.Status);
            r.Append((char)v.StatusType);
            r.Append(v.Merchant.PadRightAndCut(2));
            r.Append(v.Application.ToApplicationString());
            return r;
        }

        static StringBuilder BuildChequeAuthRequest(EFTChequeAuthRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.ChequeAuth);
            r.Append('0');
            r.Append(v.Application.ToApplicationString());
            r.Append(' ');
            r.Append(v.BranchCode.PadRightAndCut(6));
            r.Append(v.AccountNumber.PadRightAndCut(14));
            r.Append(v.SerialNumber.PadRightAndCut(14));
            r.Append(v.Amount.PadLeftAsInt(9));
            r.Append((char)v.ChequeType);
            r.Append(v.ReferenceNumber.PadRightAndCut(12));

            return r;
        }

        static StringBuilder BuildGetPasswordRequest(EFTGetPasswordRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.Generic);
            r.Append((char)CommandType.GetPassword);
            r.Append(v.MinPasswordLength.PadLeft(2));
            r.Append(v.MaxPassworkLength.PadLeft(2));
            r.Append(v.Timeout.PadLeft(3));
            r.Append("0" + (char)v.PasswordDisplay);
            return r;
        }

        static StringBuilder BuildSlaveRequest(EFTSlaveRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.Generic);
            r.Append((char)CommandType.Slave);
            r.Append(v.RawCommand);

            return r;
        }

        static StringBuilder BuildGetClientListRequest(EFTClientListRequest _)
        {
            return new StringBuilder(EFTRequestCommandCode.GetClientList + "0");
        }

        static StringBuilder BuildCloudLogonRequest(EFTCloudLogonRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.CloudLogon);
            r.Append(' ');
            r.Append(v.ClientID.PadRightAndCut(16));
            r.Append(v.Password.PadRightAndCut(16));
            r.Append(v.PairingCode.PadRightAndCut(16));
            return r;
        }

        static StringBuilder BuildCloudPairRequest(EFTCloudPairRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.CloudLogon);
            r.Append('P');
            r.Append(v.ClientID.PadRightAndCut(16));
            r.Append(v.Password.PadRightAndCut(16));
            r.Append(v.PairingCode.PadRightAndCut(16));
            return r;
        }

        static StringBuilder BuildCloudTokenLogonRequest(EFTCloudTokenLogonRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.CloudLogon);
            r.Append('T');
            r.Append(v.Token.Length.PadLeft(3));
            r.Append(v.Token);
            return r;
        }

        static StringBuilder BuildSendKeyRequest(EFTSendKeyRequest v)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.SendKey);
            r.Append('0');
            r.Append((char)v.Key);
            if (v.Data?.Length > 0)
            {
                r.Append(v.Data?.PadRightAndCut(60));
            }

            return r;
        }

        static StringBuilder BuildReceiptRequest(EFTReceiptRequest _)
        {
            return new StringBuilder(EFTRequestCommandCode.Receipt + " ");
        }

        static StringBuilder BuildPayAtTableRequest(EFTPayAtTableRequest request)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.Generic);
            r.Append((char)CommandType.PayAtTable);
            r.Append(request.Header);
            r.Append(request.Content);

            return r;
        }

        static StringBuilder BuildHeartbeatRequest(EFTHeartbeatRequest request)
        {
            var r = new StringBuilder();
            r.Append(EFTRequestCommandCode.Heartbeat);
            r.Append(request.Reply ? '1' : '0');
            
            return r;
        }

        static StringBuilder BuildBasketDataRequest(EFTBasketDataRequest request)
        {
            var jsonContent = "{}";

            switch (request.Command)
            {
                case EFTBasketDataCommandCreate c:
                    // Serializer the Basket object to JSON
                    using (var ms = new System.IO.MemoryStream())
                    {
                        // Would be better to use use Newtonsoft.Json here, but we don't want the dependency, so we are stuck with DataContractJsonSerializer
                        // We need to add the possible known types from c.Basket.Items to the "known types" of the serializer otherwise it throws an exception
                        var serializerSettings = new DataContractJsonSerializerSettings()
                        {
                            EmitTypeInformation = System.Runtime.Serialization.EmitTypeInformation.Never,
                            IgnoreExtensionDataObject = false,
                            SerializeReadOnlyTypes = true,
                            KnownTypes = (c?.Basket?.Items?.Count > 0) ? c.Basket.Items.Select(bi => bi.GetType()).Distinct() : new List<Type>() { typeof(EFTBasketItem) }
                        };
                        var serializer = new DataContractJsonSerializer((c?.Basket != null) ? c.Basket.GetType() : typeof(EFTBasket), serializerSettings);
                        serializer.WriteObject(ms, c.Basket);
                        var json = ms.ToArray();
                        ms.Close();
                        jsonContent = System.Text.Encoding.UTF8.GetString(json, 0, json.Length);
                    }
                    break;

                case EFTBasketDataCommandAdd c:
                    // Serializer the Basket object to JSON
                    using (var ms = new System.IO.MemoryStream())
                    {
                        // Would be better to use use Newtonsoft.Json here, but we don't want the dependency, so we are stuck with DataContractJsonSerializer
                        // We need to add the possible known types from c.Basket.Items to the "known types" of the serializer otherwise it throws an exception
                        var serializerSettings = new DataContractJsonSerializerSettings()
                        {
                            EmitTypeInformation = System.Runtime.Serialization.EmitTypeInformation.Never,
                            IgnoreExtensionDataObject = false,
                            SerializeReadOnlyTypes = true,
                            KnownTypes = (c?.Basket?.Items?.Count > 0) ? c.Basket.Items.Select(bi => bi.GetType()).Distinct() : new List<Type>() { typeof(EFTBasketItem) }
                        };
                        var serializer = new DataContractJsonSerializer((c?.Basket != null) ? c.Basket.GetType() : typeof(EFTBasket), serializerSettings);
                        serializer.WriteObject(ms, c.Basket);
                        var json = ms.ToArray();
                        ms.Close();
                        jsonContent = System.Text.Encoding.UTF8.GetString(json, 0, json.Length);
                    }
                    break;

                case EFTBasketDataCommandDelete c:
                    // Build our fake basket
                    var b = new EFTBasket()
                    {
                        Id = c.BasketId,
                        Items = new List<EFTBasketItem>()
                        {
                            new EFTBasketItem()
                            {
                                Id = c.BasketItemId
                            }
                        }
                    };

                    // Serializer the Basket object to JSON
                    using (var ms = new System.IO.MemoryStream())
                    {
                        var serializer = new DataContractJsonSerializer(typeof(EFTBasket), new DataContractJsonSerializerSettings() { IgnoreExtensionDataObject = false, SerializeReadOnlyTypes = true });
                        serializer.WriteObject(ms, b);
                        var json = ms.ToArray();
                        ms.Close();
                        jsonContent = System.Text.Encoding.UTF8.GetString(json, 0, json.Length);
                    }
                    break;

                case EFTBasketDataCommandRaw c:
                    jsonContent = c.BasketContent;
                    break;
            }

            var r = new StringBuilder();
            r.Append('X');
            r.Append((char)CommandType.BasketData);
            r.Append(jsonContent.Length.PadLeft(6));
            r.Append(jsonContent);
            return r;
        }
        #endregion

        public EFTResponse XMLStringToEFTResponse(string msg)
        {
            return XMLSerializer.Deserialize<EFTPosAsPinpadResponse>(msg);
        }

        public string EFTRequestToXMLString(EFTRequest eftRequest)
        {
            if (eftRequest is EFTPosAsPinpadRequest r)
            {
                return EFTRequestToXMLString(r);
            }

            return string.Empty;
        }

        private static string EFTRequestToXMLString(EFTPosAsPinpadRequest eftRequest)
        {
            try
            {
                var response = XMLSerializer.Serialize(eftRequest);
                return response.Insert(0, $"&{response.Length + 7:000000}");
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }

}
