using PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly ClientViewModel vm = new ClientViewModel();

        public MainWindow()
        {
            vm.Initialize();
            vm.OnLog += VM_OnLog;

            // Set up some defaults
            vm.Data.TransactionRequest.TxnType = TransactionType.PurchaseCash;
            vm.Data.TransactionRequest.AmtPurchase = 42.00M;
            vm.Data.POSVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            DataContext = vm;
            InitializeComponent();
        }

        private void VM_OnLog(string message)
        {
            tbLog.Dispatcher.Invoke(() =>
            {
                bool autoScroll = svLog.VerticalOffset == svLog.ScrollableHeight;

                tbLog.AppendText(message);
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

        private void tcUtilities_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tcUtilities == null || tbLog == null)
                return;

            if (vm.Data.Settings.IsLogShown && tcUtilities.SelectedIndex == 2)
            {
                tbLog.Focus();
                tbLog.SelectionStart = tbLog.Text.Length;
            }
        }

        private void chkPadItem_Checked(object sender, RoutedEventArgs e)
        {
            PadCheckBoxChecked(sender, e);
        }

        private void chkPadItem_Unchecked(object sender, RoutedEventArgs e)
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

        private void cboTPad_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
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

        private void cboTPad_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        private void ExUtilities_Collapsed(object sender, RoutedEventArgs e)
        {

        }
    }
}
