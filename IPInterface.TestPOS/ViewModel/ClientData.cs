using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel
{
    /// <summary>
    /// MerchantNumberDescription
    /// </summary>
    public class Mnd
    {
        public Mnd(string m, string d)
        {
            MerchantNumber = m;
            Description = d;
        }

        public string MerchantNumber { get; set; }
        public string Description { get; set; }
        public string Description2 => $"('{MerchantNumber}') {Description}";
    }

    public enum LogType { Info, Error, Warning };

    public enum ConnectedStatus { Connected, Disconnected, AutoConnect };

    public delegate void LogEvent(string message);
    public delegate void DisplayEvent(bool show);

    public class ClientData : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event LogEvent OnLog;
        public event DisplayEvent OnDisplay;
        public event EventHandler OnDisplayChanged;

        public const string ONEBUTTON = "99";

        public ClientData()
        {
        }

        #region Endpoints
        private DemoDialogMode? prevDialogMode = null;
        EndPointViewModel currentEndPoint = null;
        public EndPointViewModel CurrentEndPoint
        {
            set
            {
                if(DemoDialogType != DemoDialogMode.AlwaysShow && value?.Type != null)
                {
                    if (value.Type == EndPointType.Cloud || value.Type == EndPointType.CloudWithLegacyPairing)
                    {
                        prevDialogMode = DemoDialogType;
                        DemoDialogType = DemoDialogMode.ShowOnEvents;
                    }
                    else if (prevDialogMode != null)
                    {
                        DemoDialogType = (DemoDialogMode)prevDialogMode;
                    }
                }

                currentEndPoint = value;
                NotifyPropertyChanged(nameof(CurrentEndPoint));
            }
            get
            {
                return currentEndPoint;
            }
        }

        ObservableCollection<EndPointViewModel> endPoints = new ObservableCollection<EndPointViewModel>();
        public ObservableCollection<EndPointViewModel> EndPoints
        {
            set
            {
                endPoints = value;
                NotifyPropertyChanged(nameof(EndPoints));
            }
            get
            {
                return endPoints;
            }
        }
        #endregion

        #region Connect
        private ConnectedStatus _connectedState = ConnectedStatus.Disconnected;
        public ConnectedStatus ConnectedState
        {
            get
            {
                return _connectedState;
            }
            set
            {
                _connectedState = value;
                NotifyPropertyChanged(nameof(ConnectedState));
            }
        }

        #endregion

        #region Logon
        public LogonType SelectedLogon { get; set; } = LogonType.Standard;
        public LogonType[] LogonList => (LogonType[])Enum.GetValues(typeof(LogonType));

        private bool _logonTestEnabled = false;
        public bool LogonTestEnabled
        {
            set
            {
                _logonTestEnabled = value;
                NotifyPropertyChanged("LogonTestEnabled");
            }
            get
            {
                return _logonTestEnabled;
            }
        }
        #endregion

        #region Heartbeat
        private bool _heartbeatTestEnabled = false;
        public bool HeartbeatTestEnabled
        {
            set
            {
                _heartbeatTestEnabled = value;
                NotifyPropertyChanged(nameof(HeartbeatTestEnabled));
            }
            get
            {
                return _heartbeatTestEnabled;
            }
        }
        #endregion

        #region Receipt Options

        public bool CutReceipt { set; get; }
        public ReceiptCutModeType CutReceiptMode
        {
            get
            {
                return (CutReceipt ? ReceiptCutModeType.Cut : ReceiptCutModeType.DontCut);
            }
        }

        public ReceiptPrintModeType PrintMode { get; set; } = ReceiptPrintModeType.PinpadPrinter;
        public Array PrintModeList => Enum.GetValues(typeof(ReceiptPrintModeType));
        #endregion

        #region DemoDialog
        public DemoDialogMode DemoDialogType
        {
            get
            {
                return Settings.DemoDialogOption;
            }
            set
            {
                Settings.DemoDialogOption = value;
                DisplayDialog(value == DemoDialogMode.AlwaysShow);
                NotifyPropertyChanged(nameof(DemoDialogType));
            }
        }
        public Array DemoDialogTypeList => Enum.GetValues(typeof(DemoDialogMode));


        #endregion

        #region Transaction

        public bool IsETS
        {
            get => TerminalString.Contains(TerminalApplication.ETS.ToString());
        }

        public bool IsPrintTimeOut { get; set; } = false;
        public bool IsPrePrintTimeOut { get; set; } = false;

        readonly ObservableCollection<string> _terminalList = new ObservableCollection<string>();
        public ObservableCollection<string> TerminalList
        {
            get
            {
                if (_terminalList.Count == 0)
                {
                    foreach (var name in Enum.GetNames(typeof(TerminalApplication)))
                    {
                        if (Enum.TryParse(name, out TerminalApplication t))
                        {
                            _terminalList.Add(name + $" ({t.ToApplicationString()})");
                        }
                    }
                }

                return _terminalList;
            }
        }

        TerminalApplication application = TerminalApplication.EFTPOS;
        public TerminalApplication Application
        {
            get
            {
                return application;
            }
            set
            {
                application = value;
                NotifyPropertyChanged(nameof(Application));
            }
        }

        readonly string[] _terminalString = new string[] { TerminalApplication.EFTPOS.ToString(), TerminalApplication.EFTPOS.ToString() };

        public string TerminalString
        {
            get => _terminalString[0];
            set
            {
                if (value == null)
                    return;

                _terminalString[1] = _terminalString[0];
                _terminalString[0] = value;

                NotifyPropertyChanged(nameof(IsETS));
                NotifyPropertyChanged(nameof(TransactionTypes));

                if (IsETS)
                    TxnTypeIdx = 0; // Redemption (P)
                else
                    TxnTypeIdx = 1; // purchase cash (P)

                Application = GetEnumValue(value, TerminalApplication.EFTPOS);

                NotifyPropertyChanged(nameof(TerminalString));
            }
        }

        bool autoTransactionReference = true;
        public bool AutoTransactionReference
        {
            get
            {
                return autoTransactionReference;
            }
            set
            {
                autoTransactionReference = value;
                NotifyPropertyChanged(nameof(AutoTransactionReference));
            }
        }

        string transactionReference = null;
        public string TransactionReference
        {
            get
            {
                return transactionReference;
            }
            set
            {
                transactionReference = value;
                NotifyPropertyChanged(nameof(TransactionReference));
            }
        }

    private string _originalTransactionReference = "";
        public string OriginalTransactionReference
        {
            get
            {
                return _originalTransactionReference;
            }
            set
            {
                _originalTransactionReference = value;
                NotifyPropertyChanged(nameof(OriginalTransactionReference));
            }
        }

        private string _lastReceiptMerchantNumber = "00";

        public string LastReceiptMerchantNumber
        {
            get
            {
                return _lastReceiptMerchantNumber;
            }
            set
            {
                _lastReceiptMerchantNumber = value;
                NotifyPropertyChanged(nameof(LastReceiptMerchantNumber));
            }
        }

        readonly string[] _lastTransactionTerminalString = new string[] { TerminalApplication.EFTPOS.ToString(), TerminalApplication.EFTPOS.ToString() };
        public TerminalApplication LastTransactionTerminal { get; set; } = TerminalApplication.EFTPOS;
        public string LastTransactionTerminalString
        {
            get => _lastTransactionTerminalString[0];
            set
            {
                _lastTransactionTerminalString[1] = _lastTransactionTerminalString[0];
                _lastTransactionTerminalString[0] = value;
                LastTransactionTerminal = GetEnumValue(value, TerminalApplication.EFTPOS);
                NotifyPropertyChanged(nameof(LastTransactionTerminalString));
            }
        }

        bool _lastTxnIsETS = false;
        public bool LastTxnIsETS
        {
            get => _lastTxnIsETS;
            set
            {
                _lastTxnIsETS = value;
                var prev = _lastTransactionTerminalString[1];
                LastTransactionTerminalString = _lastTxnIsETS ? prev : TerminalApplication.EFTPOS.ToString();
            }
        }

        private EFTTransactionRequest _txnRequest = new EFTTransactionRequest();
        public EFTTransactionRequest TransactionRequest
        {
            get => _txnRequest;
            set
            {
                _txnRequest = value;
                NotifyPropertyChanged(nameof(TransactionRequest));
            }
        }

        public ObservableCollection<string> TransactionList { get => _transactionTypes; }
        string _selectedApplication = string.Empty;
        public string SelectedApplication
        {
            get { return _selectedApplication; }
            set { _selectedApplication = value; NotifyPropertyChanged(nameof(Application)); }
        }

        public PanSource[] CardSourceList => (PanSource[])Enum.GetValues(typeof(PanSource));

        public List<Mnd> MerchantNumberList => new List<Mnd>()
            {
              new Mnd("00", "EFTPOS"),
              new Mnd("51", "Wishlist"),
              new Mnd("52", "Give X"),
              new Mnd("53", "Blackhawk"),
              new Mnd("54", "Paypal"),
              new Mnd("55", "Zoo Republic"),
              new Mnd("56", "Reserved"),
              new Mnd("57", "FDI"),
              new Mnd("58", "Reserved"),
              new Mnd("59", "Wright Express"),
              new Mnd("60", "NAB Transact"),
              new Mnd("61", "Reserved"),
              new Mnd("62", "Qantas Loyalty"),
              new Mnd("63", "ePay Universal"),
              new Mnd("64", "Incomm"),
              new Mnd("65", "AfterPay"),
              new Mnd("66", "Alipay"),
              new Mnd("67", "Humm"),
              new Mnd("68", "FDI Giftcard"),
              new Mnd("69", "WeChat"),
              new Mnd("70", "OpenPay"),
              new Mnd("89", "ZipMoney"),
              new Mnd("90", "TruRating"),
              new Mnd("90", "Reserved"),
              new Mnd("98", "Oracle"),
              new Mnd("99", "App Hub")
            };

        public List<string> CurrencyCodeList => new List<string>()
        {
            "AUD",
            "NZD"
        };

        public AccountType[] AccountList => (AccountType[])Enum.GetValues(typeof(AccountType));

        ExternalDataList _track2Items = new ExternalDataList();
        public ExternalDataList Track2Items
        {
            get
            {
                return _track2Items;
            }
            set
            {
                _track2Items = value;
                NotifyPropertyChanged("Track2List");
            }
        }

        string _selectedTrack2 = string.Empty;
        public string SelectedTrack2
        {
            get { return _selectedTrack2; }
            set { _selectedTrack2 = value; NotifyPropertyChanged(nameof(SelectedTrack2)); }
        }

        public ObservableCollection<string> Track2List
        {
            get
            {
                var list = new ObservableCollection<string>();
                _track2Items.ForEach(x => list.Add(x.ToString()));
                return list;
            }
        }

        ObservableCollection<Pad> _padItemsList = new ObservableCollection<Pad>();
        public ObservableCollection<Pad> PadItemsList
        {
            get
            {
                _padItemsList.Clear();
                PadItems.ForEach(x => _padItemsList.Add(new Pad(x.ToString())));

                return _padItemsList;
            }
            set
            {
                _padItemsList = value;
                NotifyPropertyChanged(nameof(PadItemsList));
            }
        }

        ExternalDataList _padItems = new ExternalDataList();
        public ExternalDataList PadItems
        {
            get { return _padItems; }
            set
            {
                _padItems = value;
                SelectedPad = string.Empty;
                SelectedPads?.Clear();
                NotifyPropertyChanged(nameof(PadItemsList));
            }
        }

        public List<string> SelectedPads { get; set; } = new List<string>();

        string _selectedPad = string.Empty;
        public string SelectedPad
        {
            get => _selectedPad;
            set
            {
                if (value.Equals(typeof(Pad).ToString()))
                    return;

                PadField pf = new PadField(value);
                List<Pad> padItemsList = new List<Pad>(PadItemsList);
                SelectedPads?.Clear();
                if (pf.Count > 0)
                {
                    foreach (Pad pad in padItemsList)
                    {
                        string padData = pad.Item.Split(new string[] { " | " }, 2, StringSplitOptions.RemoveEmptyEntries)[1];
                        if (pf.Contains(padData))
                        {
                            pad.IsChecked = true;
                            SelectedPads.Add(padData);
                        }
                    }
                }

                _selectedPad = value;
                NotifyPropertyChanged(nameof(SelectedPad));
            }
        }

        string _merchantNumber = "00";
        public string MerchantNumber
        {
            get => _merchantNumber;
            set
            {
                _merchantNumber = value;
                TransactionRequest.Merchant = value;
                NotifyPropertyChanged(nameof(TransactionRequest));
                NotifyPropertyChanged(nameof(MerchantNumber));
            }
        }

        int _txnTypeIdx = -1;

        public int TxnTypeIdx
        {
            get => _txnTypeIdx;
            set
            {
                if (value < 0 || value >= TransactionTypes.Count)
                    return;

                string selectedTxnType = TransactionTypes[value];
                var index = selectedTxnType.IndexOf("(");
                if (index != -1)
                {
                    var item = selectedTxnType.Substring(0, index);
                    if (Enum.TryParse(item, out TransactionType txnType))
                    {
                        TransactionRequest.TxnType = txnType;
                    }
                }

                _txnTypeIdx = value;
                NotifyPropertyChanged(nameof(TxnTypeIdx));
                NotifyPropertyChanged(nameof(TransactionRequest));
            }
        }

        readonly string _origTxnType = string.Empty;
        public string OriginalTxnType
        {
            get => _origTxnType;
            set
            {
                if (value == null)
                    return;

                var item = value.Substring(0, value.IndexOf("("));
                if (Enum.TryParse(item, out TransactionType txnType))
                {
                    TransactionRequest.OriginalTxnType = txnType;
                    NotifyPropertyChanged(nameof(TransactionRequest));
                }
            }
        }

        #endregion

        #region ETS Transaction
        readonly ObservableCollection<string> _etsTransactionTypes = GetFilteredEnum<TransactionType>("ETS", true);
        readonly ObservableCollection<string> _transactionTypes = GetFilteredEnum<TransactionType>(true);
        public ObservableCollection<string> TransactionTypes
        {
            get => IsETS ? _etsTransactionTypes : _transactionTypes;
        }
        #endregion

        #region Status
        public StatusType SelectedStatus { get; set; }
        public Array StatusList => Enum.GetValues(typeof(StatusType));
        #endregion

        #region ClientList
        public EFTClientListRequest ClientListRequest { get; set; } = new EFTClientListRequest();
        #endregion

        #region RawData

        public string RawData { get; set; }
        public int RawDataWait { get; set; } = 10;

        #endregion

        #region Configure Merchant
        private EFTConfigureMerchantRequest _merchantDetails = new EFTConfigureMerchantRequest();
        public EFTConfigureMerchantRequest MerchantDetails
        {
            get => _merchantDetails;
            set
            {
                _merchantDetails = value;
                NotifyPropertyChanged(nameof(MerchantDetails));
            }
        } 
        #endregion

        #region Settlement
        public bool ResetTotals { get; set; } = false;
        public SettlementType SelectedSettlement { get; set; }
        public Array SettlementList => Enum.GetValues(typeof(SettlementType));
        #endregion

        #region ControlPanel
        public ControlPanelType SelectedDisplay { get; set; }
        public Array ControlPanelList => Enum.GetValues(typeof(ControlPanelType));

        public bool IsWoolworthsPOS
        {
            get => Settings.IsWoolworthsPOS;
            set
            {
                Settings.IsWoolworthsPOS = value;
                NotifyPropertyChanged(nameof(PosKeyList));
            }
        }

        #endregion

        #region QueryCard
        private QueryCardType _selectedQuery = QueryCardType.ReadCard;
        public QueryCardType SelectedQuery { get => _selectedQuery; set { _selectedQuery = value; NotifyPropertyChanged(nameof(SelectedQuery)); } }
        public QueryCardType[] QueryCardList => (QueryCardType[])Enum.GetValues(typeof(QueryCardType));
        #endregion

        #region Cheque Auth
        public EFTChequeAuthRequest ChequeRequest { get; set; } = new EFTChequeAuthRequest();
        public Array ChequeList => Enum.GetValues(typeof(ChequeType));
        #endregion

        #region Dialog
        public Array DialogTypeList => Enum.GetValues(typeof(DialogType));
        public Array DialogPositionList => Enum.GetValues(typeof(DialogPosition));

        public SetDialogRequest DialogRequest { get; set; } = new SetDialogRequest();
        #endregion

        #region Slave Mode

        string _commandRequest = string.Empty;
        public string CommandRequest
        {
            get => _commandRequest;
            set
            {
                _commandRequest = value;
                SelectedCommand = _commandsList.Find(x => x.Value == _commandRequest);
                NotifyPropertyChanged(nameof(CommandRequest));
            }
        }

        public string CommandRequestDisplay
        {
            get => _commandRequest.ToVisibleSpaces();
            set
            {
                CommandRequest = value.FromVisibleSpaces();
                NotifyPropertyChanged(nameof(CommandRequestDisplay));
            }
        }

        ExternalData _selectedCommand = null;
        public ExternalData SelectedCommand
        {
            get => _selectedCommand;
            set
            {
                _selectedCommand = value;
                if (_selectedCommand != null)
                {
                    _commandRequest = _selectedCommand.Value;
                    NotifyPropertyChanged(nameof(CommandRequest));
                    NotifyPropertyChanged(nameof(CommandRequestDisplay));
                }
                NotifyPropertyChanged(nameof(SelectedCommand));
            }
        }

        ExternalDataList _commandsList = new ExternalDataList();
        public ExternalDataList CommandsList
        {
            get => _commandsList;
            set
            {
                _commandsList = value;
                NotifyPropertyChanged("CommandsList");
            }
        }
        #endregion

        #region SendKey
        public EFTPOSKey SelectedPosKey { get; set; } = EFTPOSKey.OkCancel;
        private readonly ObservableCollection<EFTPOSKey> _posKeyList = GetFilteredEnumList<EFTPOSKey>();
        private readonly ObservableCollection<EFTPOSKey> _posKeyListWow = GetFilteredEnumList<EFTPOSKey>("Woolworths", false);
        public ObservableCollection<EFTPOSKey> PosKeyList => Settings.IsWoolworthsPOS ? _posKeyListWow : _posKeyList;

        public string PosData { get; set; } = string.Empty;
        bool _sendKeyEnabled = false;
        public bool SendKeyEnabled
        {
            set
            {
                _sendKeyEnabled = value;
                NotifyPropertyChanged("SendKeyEnabled");
            }
            get
            {
                return _sendKeyEnabled;
            }
        }
        #endregion

        #region Common

        private string _lastTxnType = null;
        public string LastTxnRespType
        {
            get
            {
                return _lastTxnType;
            }
            set
            {
                _lastTxnType = value;
                NotifyPropertyChanged(nameof(LastTxnRespType));
            }
        }

        public bool HasResult => (_lastTxnResult.Count > 0);
        
        private Dictionary<string, string> _lastTxnResult = new Dictionary<string, string>();
        public Dictionary<string, string> LastTxnResult
        {
            get
            {
                return _lastTxnResult;
            }
            set
            {
                _lastTxnResult = value;
                NotifyPropertyChanged(nameof(LastTxnResult));
                NotifyPropertyChanged(nameof(HasResult));
            }
        }

        public PadField LastTxnPAD { get; set; } = null;

        public ObservableCollection<HistoryViewModel> MessageHistory { get; } = new ObservableCollection<HistoryViewModel>();

        private string _messages = string.Empty;
        public string LogMessages
        {
            get
            {
                return _messages;
            }
            set
            {
                _messages = value;
                NotifyPropertyChanged(nameof(LogMessages));
            }
        }

        private string _eftLog = string.Empty;
        public string EftLog
        {
            get
            {
                return _eftLog;
            }
            set
            {
                _eftLog = value;
                NotifyPropertyChanged(nameof(EftLog));
            }
        }

        public bool EftLogEnabled { get; set; } = false;

        private bool isSettingsShown = true;
        public bool IsSettingsShown
        {
            get
            {
                return isSettingsShown;
            }
            set
            {
                isSettingsShown = value;
                NotifyPropertyChanged(nameof(IsSettingsShown));
            }
        }

        private string _inProgress = "";
        public string InProgress
        {
            get => _inProgress;
            set
            {
                _inProgress = string.IsNullOrWhiteSpace(value) ? "" : $"'{value}' in progress";
                NotifyPropertyChanged(nameof(InProgress));
            }
        }

        public void Log(string message, LogType logType = LogType.Info)
        {
            try
            {
                if (Settings.IsLogShown)
                {
                    OnLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff} [{logType}] : {message}{Environment.NewLine}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        public void DisplayDialog(bool show)
        {
            OnDisplay(show);
        }

        public IProgress<string> Progress { set; get; }

        protected void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private static string GetEnumName(FieldInfo x, bool includeValue = false)
        {
            string value = includeValue ? $" ({Convert.ToChar(x.GetRawConstantValue())})" : "";
            return x.Name + value;
        }

        private static ObservableCollection<string> GetFilteredEnum<T>(bool includeValue = false)
        {
            var list = typeof(T).GetFields()
                .Where(x => x.IsLiteral && ((FilterAttribute[])x.GetCustomAttributes(typeof(FilterAttribute), false)).Length == 0)
                .Select(x => GetEnumName(x, includeValue));
            return new ObservableCollection<string>(list);
        }

        private static ObservableCollection<string> GetFilteredEnum<T>(string filter, bool includeValue = false)
        {
            var list = typeof(T).GetFields()
                .Where(x => x.IsLiteral && ((FilterAttribute[])x.GetCustomAttributes(typeof(FilterAttribute), false)).Length > 0
                        && ((FilterAttribute[])x.GetCustomAttributes(typeof(FilterAttribute), false))[0].CustomString.Equals(filter))
                .Select(x => GetEnumName(x, includeValue));
            return new ObservableCollection<string>(list);
        }

        private static ObservableCollection<T> GetFilteredEnumList<T>()
        {
            var list = typeof(T).GetFields()
                .Where(x => x.IsLiteral && ((FilterAttribute[])x.GetCustomAttributes(typeof(FilterAttribute), false)).Length == 0)
                .Select(x => (T)x.GetValue(x));
            return new ObservableCollection<T>(list);
        }

        private static ObservableCollection<T> GetFilteredEnumList<T>(string filter, bool filteredOnly = true)
        {
            var list = typeof(T).GetFields()
                .Where(x => x.IsLiteral
                        && ((FilterAttribute[])x.GetCustomAttributes(typeof(FilterAttribute), false)).Length > 0
                        && ((FilterAttribute[])x.GetCustomAttributes(typeof(FilterAttribute), false))[0].CustomString.Equals(filter))
                .Select(x => (T)x.GetValue(x));
            if(!filteredOnly)
            {
                var nonFiltered = GetFilteredEnumList<T>();
                foreach (T filtered in list)
                    nonFiltered.Add(filtered);
                return nonFiltered;
            }
            return new ObservableCollection<T>(list);
        }

        private T GetEnumValue<T>(string value, T def) where T : struct, Enum
        {
            var index = value.IndexOf("(");
            if (index != -1)
            {
                var item = value.Substring(0, index);
                if (Enum.TryParse(item, out T t))
                {
                    return t;
                }
            }

            return def;
        }

        #endregion

        #region Proxy Dialog
        private EFTDisplayResponse _displayDetails = new EFTDisplayResponse();
        public EFTDisplayResponse DisplayDetails
        {
            get
            {
                _displayDetails.DisplayText[0] = _displayDetails.DisplayText[0].Trim();
                _displayDetails.DisplayText[1] = _displayDetails.DisplayText[1].Trim();

                return _displayDetails;
            }
            set
            {
                _displayDetails = value;
                NotifyPropertyChanged("DisplayDetails");
                OnDisplayChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        #endregion

        #region User Settings
        public UserSettings Settings { get; set; } = new UserSettings();
        #endregion

        public string Token
        {
            get => Settings.CloudInfo.Token;
            set  { Settings.CloudInfo.Token = value; NotifyPropertyChanged("txtToken"); }
        }

        #region PAD Data

        private bool padAppendRFN = true;
        public bool PADAppendRFN
        {
            get => padAppendRFN;
            set  { padAppendRFN = value; NotifyPropertyChanged(nameof(PADAppendRFN)); }
        }

        private bool padAppendSKU = true;
        public bool PADAppendSKU
        {
            get => padAppendSKU;
            set  { padAppendSKU = value; NotifyPropertyChanged(nameof(PADAppendSKU)); }
        }

        private bool padAppendOPR = true;
        public bool PADAppendOPR
        {
            get => padAppendOPR;
            set  { padAppendOPR = value; NotifyPropertyChanged(nameof(PADAppendOPR)); }
        }

        private bool padAppendAMT = true;
        public bool PADAppendAMT
        {
            get => padAppendAMT;
            set  { padAppendAMT = value; NotifyPropertyChanged(nameof(PADAppendAMT)); }
        }

        private bool padAppendUID = true;
        public bool PADAppendUID
        {
            get => padAppendUID;
            set  { padAppendUID = value; NotifyPropertyChanged(nameof(PADAppendUID)); }
        }

        private bool padAppendNME = true;
        public bool PADAppendNME
        {
            get => padAppendNME;
            set  { padAppendNME = value; NotifyPropertyChanged(nameof(PADAppendNME)); }
        }

        private bool padAppendVER = true;
        public bool PADAppendVER
        {
            get => padAppendVER;
            set  { padAppendVER = value; NotifyPropertyChanged(nameof(PADAppendVER)); }
        }

        private bool padAppendVND = true;
        public bool PADAppendVND
        {
            get => padAppendVND;
            set  { padAppendVND = value; NotifyPropertyChanged(nameof(PADAppendVND)); }
        }

        private bool padAppendPCM = true;
        public bool PADAppendPCM
        {
            get => padAppendPCM;
            set  { padAppendPCM = value; NotifyPropertyChanged(nameof(PADAppendPCM)); }
        }

        private bool padPCMBarcode = false;
        public bool PADPCMBarcode
        {
            get => padPCMBarcode;
            set  { padPCMBarcode = value; NotifyPropertyChanged(nameof(PADPCMBarcode)); }
        }
        #endregion

        #region Receipts
        private string _receiptInfo = string.Empty;

        public string Receipt
        {
            get
            {
                return _receiptInfo;
            }
            set
            {
                _receiptInfo = value;
                NotifyPropertyChanged("Receipt");
            }
        }

        public string POSVersion { get; internal set; }

        public Guid POSVendorId { get; set; } = new Guid("ab1299fc-ec39-480a-bab5-4390175c7535");
        public string PADSKUId { get; set; }
        public string LastTxnRFN { get; internal set; }
        #endregion
    }
}
