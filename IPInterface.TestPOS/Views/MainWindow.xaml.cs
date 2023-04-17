using PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using IPInterface.TestPOS.Properties;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Controls.Primitives;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ClientViewModel vm;

        public MainWindow()
        {
            vm = new ClientViewModel(this);
            vm.Initialize();
            vm.OnLog += VM_OnLog;

            // Set up some defaults
            RestoreControlValues();
            vm.Data.POSVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            DataContext = vm;
            InitializeComponent();
        }

        private void VM_OnLog(string message)
        {
            tbLog.Dispatcher.Invoke(() =>
            {
                bool autoScroll = svLog.VerticalOffset == svLog.ScrollableHeight;

                vm.Data.LogMessages += message;
                if (!vm.Data.SendKeyEnabled && tcUtilities.SelectedIndex == 2)
                {
                    tbLog.Focus();
                    tbLog.SelectionStart = tbLog.Text.Length;
                }

                if(autoScroll)
                {
                    svLog.ScrollToBottom();
                }
            });
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                SaveControlValues();
                vm.SaveSettings();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }
            finally
            {
                Application.Current.Shutdown();
            }
        }



        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (vm.Data.Settings.DemoDialogOption == DemoDialogMode.AlwaysShow)
            {
                vm.ShowProxyDialog(true);
            }
            svLog.ScrollToBottom();
        }

        private void TcUtilities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcUtilities == null || tbLog == null)
                return;

            if (vm.Data.Settings.IsLogShown && tcUtilities.SelectedIndex == 2)
            {
                tbLog.Focus();
                tbLog.SelectionStart = tbLog.Text.Length;
            }
        }

        private void ChkPadItem_Checked(object sender, RoutedEventArgs e)
        {
            PadCheckBoxChecked(sender, e);
        }

        private void ChkPadItem_Unchecked(object sender, RoutedEventArgs e)
        {
            PadCheckBoxUnchecked(sender, e);
        }


        bool padsLocked = false;
        void RefreshSelectedPadText()
        {
            if (!padsLocked)
            {
                var sb = new StringBuilder();
                vm.Data.SelectedPads.ForEach(i => sb.Append(i));
                vm.Data.SelectedPad = sb.ToString();
            }
        }

        private void CboTPad_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter
                    && sender is ComboBox cbo
                    && (vm.Data.PadItems.FindIndex(x => x.ToString().Equals(cbo.Text)) == -1))
            {
                padsLocked = true;
                e.Handled = true;

                for (int i = 0; i < vm.Data.PadItemsList.Where(x => !x.IsChecked)?.Count(); i++)
                {
                    vm.Data.PadItemsList[i].IsChecked = false;
                }

                vm.Data.SelectedPads.Clear();
                padsLocked = false;
            }
        }

        private void CboTPad_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;
        }

        private void PadCheckBoxChecked(object sender, RoutedEventArgs e)
        {
            if (padsLocked)
                return;

            if (sender is CheckBox ch && ch.DataContext is Pad chd)
            {
                var item = vm.Data.PadItems.Find(x => x.ToString().Equals(chd.Item));
                if (item != null && !vm.Data.SelectedPads.Exists(x => x.Equals(item.Value)))
                {
                    vm.Data.SelectedPads?.Add(item.Value);
                    RefreshSelectedPadText();
                }
            }
        }

        private void PadCheckBoxUnchecked(object sender, RoutedEventArgs e)
        {
            if (padsLocked)
                return;

            if (sender is CheckBox ch && ch.DataContext is Pad chd)
            {
                var item = vm.Data.PadItems.Find(x => x.ToString().Equals(chd.Item));
                if (item != null)
                {
                    vm.Data.SelectedPads.Remove(item.Value);
                    RefreshSelectedPadText();
                }
            }
        }

        private void Button_ResetFields(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("This will reset all saved field values\nand restart the application\n\nContinue?", "Reset All Fields?", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                ControlValues.Default.Reset();
                ControlValues.Default.Save();
                Application.Current.Shutdown();
                Process.Start(Environment.GetCommandLineArgs()[0]);
            }
        }

        private sealed class ControlValue
        {
            public ControlValue(Action store, Action restore)
            {
                Store = store;
                Restore = restore;
            }
            public Action Store { get; }
            public Action Restore { get; }
        }
        private List<ControlValue> _controlValueActions;

        private void InitControlValues()
        {
            if(_controlValueActions == null)
            {
                _controlValueActions = new List<ControlValue>
                {
                    new ControlValue(() => ControlValues.Default.LogonTypeIdx           = cboLogonType.SelectedIndex              , () => vm.Data.SelectedLogon                   = vm.Data.LogonList[ControlValues.Default.LogonTypeIdx]),
                    new ControlValue(() => ControlValues.Default.TranTxnRef             = vm.Data.TransactionReference            , () => vm.Data.TransactionReference            = ControlValues.Default.TranTxnRef),
                    new ControlValue(() => ControlValues.Default.TranAutoTxnRef         = vm.Data.AutoTransactionReference        , () => vm.Data.AutoTransactionReference        = ControlValues.Default.TranAutoTxnRef),
                    new ControlValue(() => ControlValues.Default.TranTxnTypeIdx         = vm.Data.TxnTypeIdx                      , () => vm.Data.TxnTypeIdx                      = ControlValues.Default.TranTxnTypeIdx),
                    new ControlValue(() => ControlValues.Default.TranAmtPurch           = vm.Data.TransactionRequest.AmtPurchase  , () => vm.Data.TransactionRequest.AmtPurchase  = ControlValues.Default.TranAmtPurch),
                    new ControlValue(() => ControlValues.Default.TranAmtCash            = vm.Data.TransactionRequest.AmtCash      , () => vm.Data.TransactionRequest.AmtCash      = ControlValues.Default.TranAmtCash),
                    new ControlValue(() => ControlValues.Default.TranAuthNo             = vm.Data.TransactionRequest.AuthCode     , () => vm.Data.TransactionRequest.AuthCode     = ControlValues.Default.TranAuthNo),
                    new ControlValue(() => ControlValues.Default.TranRRN                = vm.Data.TransactionRequest.RRN          , () => vm.Data.TransactionRequest.RRN          = ControlValues.Default.TranRRN),
                    new ControlValue(() => ControlValues.Default.TranPANSrcIdx          = cboPanSource.SelectedIndex              , () => vm.Data.TransactionRequest.PanSource    = vm.Data.CardSourceList[ControlValues.Default.TranPANSrcIdx]),
                    new ControlValue(() => ControlValues.Default.TranAccountTypeIdx     = cboAcctType.SelectedIndex               , () => vm.Data.TransactionRequest.AccountType  = vm.Data.AccountList[ControlValues.Default.TranAccountTypeIdx]),
                    new ControlValue(() => ControlValues.Default.TranPAN                = vm.Data.TransactionRequest.Pan          , () => vm.Data.TransactionRequest.Pan          = ControlValues.Default.TranPAN),
                    new ControlValue(() => ControlValues.Default.TranExpiry             = vm.Data.TransactionRequest.DateExpiry   , () => vm.Data.TransactionRequest.DateExpiry   = ControlValues.Default.TranExpiry),
                    new ControlValue(() => ControlValues.Default.TranCurrCodeIdx        = cboTCurrCode.SelectedIndex              , () => vm.Data.TransactionRequest.CurrencyCode = vm.Data.CurrencyCodeList[ControlValues.Default.TranCurrCodeIdx]),
                    new ControlValue(() => ControlValues.Default.TranOriginalTxnTypeIdx = cboOrigTxnType.SelectedIndex            , () => vm.Data.OriginalTxnType                 = vm.Data.TransactionList[ControlValues.Default.TranOriginalTxnTypeIdx]),
                    new ControlValue(() => ControlValues.Default.TranTrainingMode       = vm.Data.TransactionRequest.TrainingMode , () => vm.Data.TransactionRequest.TrainingMode = ControlValues.Default.TranTrainingMode),
                    new ControlValue(() => ControlValues.Default.TranEnableTip          = vm.Data.TransactionRequest.EnableTip    , () => vm.Data.TransactionRequest.EnableTip    = ControlValues.Default.TranEnableTip),
                    new ControlValue(() => ControlValues.Default.TranPrintTimeout       = vm.Data.IsPrintTimeOut                  , () => vm.Data.IsPrintTimeOut                  = ControlValues.Default.TranPrintTimeout),
                    new ControlValue(() => ControlValues.Default.TranPrePrintTimeout    = vm.Data.IsPrePrintTimeOut               , () => vm.Data.IsPrePrintTimeOut               = ControlValues.Default.TranPrePrintTimeout),
                    new ControlValue(() => ControlValues.Default.TranAppendLastRFN      = vm.Data.PADAppendRFN                    , () => vm.Data.PADAppendRFN                    = ControlValues.Default.TranAppendLastRFN),
                    new ControlValue(() => ControlValues.Default.TranAppendSKU          = vm.Data.PADAppendSKU                    , () => vm.Data.PADAppendSKU                    = ControlValues.Default.TranAppendSKU),
                    new ControlValue(() => ControlValues.Default.TranAppendOPR          = vm.Data.PADAppendOPR                    , () => vm.Data.PADAppendOPR                    = ControlValues.Default.TranAppendOPR),
                    new ControlValue(() => ControlValues.Default.TranAppendAMT          = vm.Data.PADAppendAMT                    , () => vm.Data.PADAppendAMT                    = ControlValues.Default.TranAppendAMT),
                    new ControlValue(() => ControlValues.Default.TranAppendUID          = vm.Data.PADAppendUID                    , () => vm.Data.PADAppendUID                    = ControlValues.Default.TranAppendUID),
                    new ControlValue(() => ControlValues.Default.TranAppendNME          = vm.Data.PADAppendNME                    , () => vm.Data.PADAppendNME                    = ControlValues.Default.TranAppendNME),
                    new ControlValue(() => ControlValues.Default.TranAppendVER          = vm.Data.PADAppendVER                    , () => vm.Data.PADAppendVER                    = ControlValues.Default.TranAppendVER),
                    new ControlValue(() => ControlValues.Default.TranAppendVND          = vm.Data.PADAppendVND                    , () => vm.Data.PADAppendVND                    = ControlValues.Default.TranAppendVND),
                    new ControlValue(() => ControlValues.Default.TranAppendPCM          = vm.Data.PADAppendPCM                    , () => vm.Data.PADAppendPCM                    = ControlValues.Default.TranAppendPCM),
                    new ControlValue(() => ControlValues.Default.TranPCMBarcode         = vm.Data.PADPCMBarcode                   , () => vm.Data.PADPCMBarcode                   = ControlValues.Default.TranPCMBarcode),
                    new ControlValue(() => ControlValues.Default.TranSelectedTrack2     = vm.Data.SelectedTrack2                  , () => vm.Data.SelectedTrack2                  = ControlValues.Default.TranSelectedTrack2),
                    new ControlValue(() => ControlValues.Default.QueryTypeIdx           = cboQueryCardType.SelectedIndex          , () => vm.Data.SelectedQuery                   = vm.Data.QueryCardList[ControlValues.Default.QueryTypeIdx]),
                    new ControlValue(() => ControlValues.Default.ConfigTerminalId       = vm.Data.MerchantDetails.Catid           , () => vm.Data.MerchantDetails.Catid           = ControlValues.Default.ConfigTerminalId),
                    new ControlValue(() => ControlValues.Default.ConfigMerchantId       = vm.Data.MerchantDetails.Caid            , () => vm.Data.MerchantDetails.Caid            = ControlValues.Default.ConfigMerchantId),
                    new ControlValue(() => ControlValues.Default.ConfigNII              = vm.Data.MerchantDetails.NII             , () => vm.Data.MerchantDetails.NII             = ControlValues.Default.ConfigNII),
                    new ControlValue(() => ControlValues.Default.ConfigAIIC             = vm.Data.MerchantDetails.AIIC            , () => vm.Data.MerchantDetails.AIIC            = ControlValues.Default.ConfigAIIC),
                    new ControlValue(() => ControlValues.Default.ConfigTimeout          = vm.Data.MerchantDetails.Timeout         , () => vm.Data.MerchantDetails.Timeout         = ControlValues.Default.ConfigTimeout),
                    new ControlValue(() => ControlValues.Default.CommonAppNumIdx        = cboEtsType.SelectedIndex                , () => vm.Data.TerminalString                  = vm.Data.TerminalList[ControlValues.Default.CommonAppNumIdx].ToString()),
                    new ControlValue(() => ControlValues.Default.CommonMerchNum         = vm.Data.MerchantNumber                  , () => vm.Data.MerchantNumber                  = ControlValues.Default.CommonMerchNum),
                    new ControlValue(() => ControlValues.Default.CommonSelectedPad      = vm.Data.SelectedPad                     , () => vm.Data.SelectedPad                     = ControlValues.Default.CommonSelectedPad),
                };
            }
        }

        private void RestoreControlValues()
        {
            InitControlValues();
            foreach(ControlValue controlValue in _controlValueActions)
            {
                try
                {
                    controlValue.Restore();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex);
                }
            }
        }

        private void SaveControlValues()
        {
            InitControlValues();
            foreach (ControlValue controlValue in _controlValueActions)
            {
                controlValue.Store();
            }
            ControlValues.Default.Save();
        }

        public TabItem GetCurrentTabItem()
        {
            var tc = tbMenu;
            while (tc.SelectedItem is TabControl newTc)
            {
                tc = newTc;
            }

            return tc.SelectedItem as TabItem;
        }
    }
}
