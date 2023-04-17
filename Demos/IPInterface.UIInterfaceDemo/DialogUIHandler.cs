using PCEFTPOS.EFTClient.IPInterface;
using PCEFTPOS.EFTClient.IPInterface.DialogUI;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace IPInterface.UIInterfaceDemo
{
	class DialogUIHandler : IDialogUIHandler, INotifyPropertyChanged
	{
		WPFDialogUI eftDialog = null;
		private EFTDisplayResponse _displayResponse = new EFTDisplayResponse();
		public IEFTClientIP EFTClientIP { get; set; }


		public event PropertyChangedEventHandler PropertyChanged;

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

		public DialogUIHandler()
		{

		}

		public void HandleCloseDisplay()
		{
			eftDialog.Dispatcher.Invoke(() =>
			{
				eftDialog.Close();
				eftDialog.BtnOK.Click -= BtnOK_Click;
				eftDialog.BtnCancel.Click -= BtnCancel_Click;
				eftDialog = null;
			});

		}

		public void HandleDisplayResponse(EFTDisplayResponse eftDisplayResponse)
		{
			DisplayResponse = eftDisplayResponse;
			if (eftDialog == null)
			{
                eftDialog = new WPFDialogUI
                {
                    DataContext = this
                };
                eftDialog.BtnOK.Click += BtnOK_Click;
				eftDialog.BtnCancel.Click += BtnCancel_Click;
				eftDialog.Show();
			}
			LoadButtons(eftDisplayResponse);
			if (eftDisplayResponse.InputType != InputType.None)
			{
				eftDialog.txtInput.Visibility = System.Windows.Visibility.Visible;
			}
		}

		private void BtnOK_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			switch (eftDialog.BtnOK.Content)
			{
				case ("OK"):
					EFTClientIP?.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel, Data = (eftDialog.txtInput.Text != "") ? eftDialog.txtInput.Text : null });
					break;
				case ("Authorise"):
					EFTClientIP?.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.Authorise, Data = (eftDialog.txtInput.Text != "") ? eftDialog.txtInput.Text : null });
					break;
				case ("Yes"):
					EFTClientIP?.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.YesAccept, Data = (eftDialog.txtInput.Text != "") ? eftDialog.txtInput.Text : null });
					break;
				default:
					EFTClientIP?.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel, Data = (eftDialog.txtInput.Text != "") ? eftDialog.txtInput.Text : null });
					break;

			}
		}

		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			switch (eftDialog.BtnCancel.Content)
			{
				case ("Cancel"):
					EFTClientIP?.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel });
					break;
				case ("Decline"):
					EFTClientIP?.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.NoDecline });
					break;
				default:
					EFTClientIP?.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel });
					break;

			}
		}

		private void LoadButtons(EFTDisplayResponse eftDisplayResponse)
		{
			#region OKButtons
			if (eftDisplayResponse.AcceptYesKeyFlag == true)
			{
				eftDialog.BtnOK.Content = "Yes";
				eftDialog.BtnOK.Visibility = System.Windows.Visibility.Visible;
			}
			else if (eftDisplayResponse.AuthoriseKeyFlag == true)
			{
				eftDialog.BtnOK.Content = "Authorise";
				eftDialog.BtnOK.Visibility = System.Windows.Visibility.Visible;
			}
			else if (eftDisplayResponse.OKKeyFlag == true)
			{
				eftDialog.BtnOK.Content = "OK";
				eftDialog.BtnOK.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				eftDialog.BtnOK.Visibility = System.Windows.Visibility.Collapsed;
			}
			#endregion

			#region CancelButtons
			if (eftDisplayResponse.CancelKeyFlag == true)
			{
				eftDialog.BtnCancel.Content = "Cancel";
				eftDialog.BtnCancel.Visibility = System.Windows.Visibility.Visible;
			}
			else if (eftDisplayResponse.DeclineNoKeyFlag == true)
			{
				eftDialog.BtnCancel.Content = "No";
				eftDialog.BtnCancel.Visibility = System.Windows.Visibility.Visible;
			}
			else
			{
				eftDialog.BtnCancel.Visibility = System.Windows.Visibility.Collapsed;
			}
			#endregion
		}
	}
}
