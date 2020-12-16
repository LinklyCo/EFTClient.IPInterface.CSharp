using PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel;
using System.Windows;


namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    /// <summary>
    /// Interaction logic for ProxyDialog.xaml
    /// </summary>
    public partial class ProxyDialog : Window
    {
        public ProxyDialog()
        {
            InitializeComponent();
        }

        private void Window_Closed(object sender, System.EventArgs e)
        {

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            var d = (ProxyViewModel)DataContext;
            if (d != null)
            {
                if (!d.ProxyWindowClosing)
                {
                    d.SendKeyFunc(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel });
                }
            }
        }
    }
}
