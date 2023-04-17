using PCEFTPOS.EFTClient.IPInterface;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using System.Windows.Threading;

namespace WinFormsIDialogUI
{
	class WinFormDialogUIHandler : INotifyPropertyChanged, IDialogUIHandler
	{
		public IEFTClientIP EFTClientIP { get; set; }
		public WinFormDialog eftDialog = null;
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

		public WinFormDialogUIHandler()
		{

		}



		public void HandleCloseDisplay()
		{
			eftDialog.Invoke(new MethodInvoker( delegate
			{
				eftDialog.Close();
				eftDialog.BtnOK.Click -= BtnOK_Click;
				eftDialog.BtnCancel.Click -= BtnCancel_Click;
				eftDialog = null;
			}));
		}

		public void HandleDisplayResponse(EFTDisplayResponse eftDisplayResponse)
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
			if (eftDisplayResponse.InputType != InputType.None)
			{
				eftDialog.txtInputData.Visible = true;
			}
		}

		private void BtnOK_Click(object sender, EventArgs e)
		{
			switch (eftDialog.BtnOK.Text)
			{
				case ("OK"):
					EFTClientIP?.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel, Data = (eftDialog.txtInputData.Text != "") ? eftDialog.txtInputData.Text : null });
					break;
				case ("Authorise"):
					EFTClientIP?.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.Authorise, Data = (eftDialog.txtInputData.Text != "") ? eftDialog.txtInputData.Text : null });
					break;
				case ("Yes"):
					EFTClientIP?.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.YesAccept, Data = (eftDialog.txtInputData.Text != "") ? eftDialog.txtInputData.Text : null });
					break;
				default:
					EFTClientIP?.DoSendKey(new EFTSendKeyRequest() { Key = EFTPOSKey.OkCancel, Data = (eftDialog.txtInputData.Text != "") ? eftDialog.txtInputData.Text : null });
					break;

			}
		}

		private void BtnCancel_Click(object sender, EventArgs e)
		{
			switch (eftDialog.BtnCancel.Text)
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
