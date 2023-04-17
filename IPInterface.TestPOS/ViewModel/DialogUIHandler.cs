using IPInterface.TestPOS.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel
{
    public class DialogUIHandler : IDialogUIHandlerAsync, INotifyPropertyChanged
    {
        public IEFTClientIPAsync EFTClientIPAsync { get; set; }
        private EFTDisplayResponse _displayResponse = new EFTDisplayResponse();
        public event PropertyChangedEventHandler PropertyChanged;
        public TestDialogUI EftDialog { get; set; } = null;
        public bool ProxyWindowClosing { get; set; } = false;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EFTDisplayResponse DisplayResponse
        {
            get
            {
                return _displayResponse;
            }
            set
            {
                _displayResponse = value;
                OnPropertyChanged(nameof(DisplayResponse));
            }
        }

        public Task HandleCloseDisplayAsync()
        {
            if (EftDialog != null)
            {
                EftDialog.Close();
                EftDialog.BtnOK.Click -= BtnOK_Click;
                EftDialog.BtnCancel.Click -= BtnCancel_Click;
                EftDialog = null;
            }
            return Task.FromResult(0);
        }

        public Task HandleDisplayResponseAsync(EFTDisplayResponse eftDisplayResponse)
        {
            DisplayResponse = eftDisplayResponse;
            if (EftDialog == null)
            {
                EftDialog = new TestDialogUI
                {
                    DataContext = this
                };
                EftDialog.BtnOK.Click += BtnOK_Click;
                EftDialog.BtnCancel.Click += BtnCancel_Click;
                EftDialog.txtResponseLine1.Text = eftDisplayResponse.DisplayText[0];
                EftDialog.txtResponseLine2.Text = eftDisplayResponse.DisplayText[1];
                EftDialog.Show();
            }
            LoadButtons(eftDisplayResponse);
            if (eftDisplayResponse.InputType != InputType.None)
            {
                EftDialog.txtInput.Visibility = System.Windows.Visibility.Visible;
            }
            return Task.FromResult(0);
        }

        private void BtnOK_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            switch (EftDialog.BtnOK.Content)
            {
                case ("OK"):
                    EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel, Data = (EftDialog.txtInput.Text != "") ? EftDialog.txtInput.Text : null });
                    break;
                case ("Authorise"):
                    EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.Authorise, Data = (EftDialog.txtInput.Text != "") ? EftDialog.txtInput.Text : null });
                    break;
                case ("Yes"):
                    EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.YesAccept, Data = (EftDialog.txtInput.Text != "") ? EftDialog.txtInput.Text : null });
                    break;
                default:
                    EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel, Data = (EftDialog.txtInput.Text != "") ? EftDialog.txtInput.Text : null });
                    break;

            }
        }

        private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            switch (EftDialog.BtnCancel.Content)
            {
                case ("Cancel"):
                    EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel });
                    break;
                case ("Decline"):
                    EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.NoDecline });
                    break;
                default:
                    EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel });
                    break;

            }
        }

        private void LoadButtons(EFTDisplayResponse eftDisplayResponse)
        {
            #region OKButtons
            if (eftDisplayResponse.AcceptYesKeyFlag)
            {
                EftDialog.BtnOK.Content = "Yes";
                EftDialog.BtnOK.Visibility = System.Windows.Visibility.Visible;
            }
            else if (eftDisplayResponse.AuthoriseKeyFlag)
            {
                EftDialog.BtnOK.Content = "Authorise";
                EftDialog.BtnOK.Visibility = System.Windows.Visibility.Visible;
            }
            else if (eftDisplayResponse.OKKeyFlag)
            {
                EftDialog.BtnOK.Content = "OK";
                EftDialog.BtnOK.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                EftDialog.BtnOK.Visibility = System.Windows.Visibility.Collapsed;
            }
            #endregion

            #region CancelButtons
            if (eftDisplayResponse.CancelKeyFlag)
            {
                EftDialog.BtnCancel.Content = "Cancel";
                EftDialog.BtnCancel.Visibility = System.Windows.Visibility.Visible;
            }
            else if (eftDisplayResponse.DeclineNoKeyFlag)
            {
                EftDialog.BtnCancel.Content = "No";
                EftDialog.BtnCancel.Visibility = System.Windows.Visibility.Visible;
            }
            else
            {
                EftDialog.BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
            }
            #endregion
        }
    }
}
