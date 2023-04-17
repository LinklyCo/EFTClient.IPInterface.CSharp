using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PCEFTPOS.EFTClient.IPInterface.DialogUI
{
	class DialogUIHandlerAsync : IDialogUIHandlerAsync, INotifyPropertyChanged
	{
		WPFDialogUI eftDialog = null;
		private EFTDisplayResponse _displayResponse = new EFTDisplayResponse();

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

		public DialogUIHandlerAsync()
		{

		}
		public IEFTClientIPAsync EFTClientIPAsync { get; set; }

		public Task HandleCloseDisplayAsync()
		{
			eftDialog.Close();
			eftDialog.BtnOK.Click -= BtnOK_Click;
			eftDialog.BtnCancel.Click -= BtnCancel_Click;
			eftDialog = null;
			return Task.CompletedTask;
		}

		public Task HandleDisplayResponseAsync(EFTDisplayResponse eftDisplayResponse)
		{
			DisplayResponse = eftDisplayResponse;
			if (eftDialog == null)
			{
				eftDialog = new WPFDialogUI();
				eftDialog.DataContext = this;
				eftDialog.BtnOK.Click += BtnOK_Click;
				eftDialog.BtnCancel.Click += BtnCancel_Click;
				eftDialog.Show();
			}
			loadButtons(eftDisplayResponse);
			if (eftDisplayResponse.InputType != InputType.None)
			{
				eftDialog.txtInput.Visibility = System.Windows.Visibility.Visible;
			}
			return Task.CompletedTask;
		}

		private void BtnOK_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			switch (eftDialog.BtnOK.Content)
			{
				case ("OK"):
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel, Data = (eftDialog.txtInput.Text != "") ? eftDialog.txtInput.Text : null });
					break;
				case ("Authorise"):
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.Authorise, Data = (eftDialog.txtInput.Text != "") ? eftDialog.txtInput.Text : null });
					break;
				case ("Yes"):
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.YesAccept, Data = (eftDialog.txtInput.Text != "") ? eftDialog.txtInput.Text : null });
					break;
				default:
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel, Data = (eftDialog.txtInput.Text != "") ? eftDialog.txtInput.Text : null });
					break;

			}
		}

		private void BtnCancel_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			switch (eftDialog.BtnCancel.Content)
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

		private void loadButtons(EFTDisplayResponse eftDisplayResponse)
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
