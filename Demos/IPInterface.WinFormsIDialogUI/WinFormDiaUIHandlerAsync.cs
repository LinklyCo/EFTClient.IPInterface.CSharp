using PCEFTPOS.EFTClient.IPInterface;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace WinFormsIDialogUI
{
	class WinFormDialogUIHandlerAsync : IDialogUIHandlerAsync
	{
		WinFormDialog eftDialog = null;
		public IEFTClientIPAsync EFTClientIPAsync { get; set; }
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
				eftDialog = new WinFormDialog();
				eftDialog.BtnOK.Click += BtnOK_Click;
				eftDialog.BtnCancel.Click += BtnCancel_Click;
				eftDialog.Show();
			}
			LoadButtons(eftDisplayResponse);
			eftDialog.txtDisplayLine1.Text = eftDisplayResponse.DisplayText[0];
			eftDialog.txtDisplayLine2.Text = eftDisplayResponse.DisplayText[1];

			eftDialog.axWindowsMediaPlayer1.Visible = true;
			eftDialog.axWindowsMediaPlayer1.settings.setMode("loop", true);
			eftDialog.axWindowsMediaPlayer1.uiMode = "none";
			ChooseAVI(eftDisplayResponse);

			if (eftDisplayResponse.InputType != InputType.None)
			{
				eftDialog.txtInputData.Visible = true;
			}
			return Task.CompletedTask;
		}

		private void ChooseAVI(EFTDisplayResponse eftDisplayResponse)
		{
			switch (eftDisplayResponse.GraphicCode)
			{
				case GraphicCode.None:
					eftDialog.axWindowsMediaPlayer1.Visible = false;
					break;
				case GraphicCode.Processing:
					eftDialog.axWindowsMediaPlayer1.URL = @"C:\PC_EFT\AVI\PROCESS.AVI";
					break;
				case GraphicCode.Verify:
					eftDialog.axWindowsMediaPlayer1.URL = @"C:\PC_EFT\AVI\VERIFY.AVI";
					break;
				case GraphicCode.Question:
					eftDialog.axWindowsMediaPlayer1.URL = @"C:\PC_EFT\AVI\QUESTION.AVI";
					break;
				case GraphicCode.Card:
					eftDialog.axWindowsMediaPlayer1.URL = @"C:\PC_EFT\AVI\CARD.AVI";
					break;
				case GraphicCode.Account:
					eftDialog.axWindowsMediaPlayer1.URL = @"C:\PC_EFT\AVI\ACCOUNT.AVI";
					break;
				case GraphicCode.PIN:
					eftDialog.axWindowsMediaPlayer1.URL = @"C:\PC_EFT\AVI\PIN.AVI";
					break;
				case GraphicCode.Finished:
					eftDialog.axWindowsMediaPlayer1.URL = @"C:\PC_EFT\AVI\FINISHED.AVI";
					break;
			}
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			switch (eftDialog.BtnOK.Text)
			{
				case ("OK"):
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel, Data = (eftDialog.txtInputData.Text != "") ? eftDialog.txtInputData.Text : null });
					break;
				case ("Authorise"):
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.Authorise, Data = (eftDialog.txtInputData.Text != "") ? eftDialog.txtInputData.Text : null });
					break;
				case ("Yes"):
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.YesAccept, Data = (eftDialog.txtInputData.Text != "") ? eftDialog.txtInputData.Text : null });
					break;
				default:
					EFTClientIPAsync?.WriteRequestAsync(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel, Data = (eftDialog.txtInputData.Text != "") ? eftDialog.txtInputData.Text : null });
					break;

			}
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			switch (eftDialog.BtnCancel.Text)
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
			if (eftDisplayResponse.AcceptYesKeyFlag == true)
			{
				eftDialog.BtnOK.Text = "Yes";
				eftDialog.BtnOK.Visible = true;
			}
			else if (eftDisplayResponse.AuthoriseKeyFlag == true)
			{
				eftDialog.BtnOK.Text = "Authorise";
				eftDialog.BtnOK.Visible = true;
			}
			else if (eftDisplayResponse.OKKeyFlag == true)
			{
				eftDialog.BtnOK.Text = "OK";
				eftDialog.BtnOK.Visible = true;
			}
			else
			{
				eftDialog.BtnOK.Visible = false;
			}
			#endregion

			#region CancelButtons
			if (eftDisplayResponse.CancelKeyFlag == true)
			{
				eftDialog.BtnCancel.Text = "Cancel";
				eftDialog.BtnCancel.Visible = true;
			}
			else if (eftDisplayResponse.DeclineNoKeyFlag == true)
			{
				eftDialog.BtnCancel.Text = "No";
				eftDialog.BtnCancel.Visible = true;
			}
			else
			{
				eftDialog.BtnCancel.Visible = false;
			}
			#endregion
		}
	}
}
