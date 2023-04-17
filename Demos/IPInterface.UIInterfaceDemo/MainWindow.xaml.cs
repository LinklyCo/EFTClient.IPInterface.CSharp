using PCEFTPOS.EFTClient.IPInterface;
using PCEFTPOS.EFTClient.IPInterface.DialogUI;
using System;
using System.Threading;
using System.Windows;

namespace IPInterface.UIInterfaceDemo
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		bool txnInProgress = false;
		EFTClientIP eftIP = new EFTClientIP()
		{
			HostName = "127.0.0.1",
			HostPort = 2011,
			UseSSL = false,
			DialogUIHandler = new DialogUIHandler()
		};

		public MainWindow()
		{
			InitializeComponent();
			eftIP.OnDisplay += Eft_OnDisplay;
			eftIP.OnLogon += EftIP_OnLogon;
			eftIP.OnReceipt += EftIP_OnReceipt;
		}

		private void EftIP_OnReceipt(object sender, EFTEventArgs<EFTReceiptResponse> e)
		{
			foreach (var s in e.Response.ReceiptText)
			{
				Console.WriteLine(s);
			}
		}

		private void EftIP_OnLogon(object sender, EFTEventArgs<EFTLogonResponse> e)
		{
			Console.WriteLine(e.Response);
		}

		private void Eft_OnDisplay(object sender, EFTEventArgs<EFTDisplayResponse> e)
		{
			eftIP.DialogUIHandler.HandleDisplayResponse(e.Response);
		}

		private async void BtnLogon_Click(object sender, RoutedEventArgs e)
		{
			if (txnInProgress)
			{
				return;
			}

			txnInProgress = true;
			var eft = new EFTClientIPAsync() { DialogUIHandlerAsync = new DialogUIHandlerAsync(), Parser = new DefaultMessageParser() };
			await eft.ConnectAsync("127.0.0.1", 2011, false);
			await eft.WriteRequestAndWaitAsync<SetDialogResponse>(new SetDialogRequest() { DialogType = DialogType.Hidden }, new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token);
			await eft.WriteRequestAndWaitAsync<EFTTransactionResponse>(new EFTTransactionRequest() { TxnType = TransactionType.PurchaseCash, AmtPurchase = 1.00m, Merchant = "00", Application = TerminalApplication.EFTPOS }, new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token);
			//EFTClientListResponse clientLists = await eft.WriteRequestAndWaitAsync<EFTClientListResponse>(new EFTClientListRequest(), new CancellationTokenSource(TimeSpan.FromMinutes(5)).Token);
			//foreach (var client in clientLists.EFTClients)
			//{
			//	Console.WriteLine($"IP Address: {0} \n\r Name: {1} \n\r Port: {2} \r\n State: {3}", client.IPAddress, client.Name, client.Port, client.State);
			//}
			eft.Disconnect();
			txnInProgress = false;
		}

		private void BtnNonAsync_Click(object sender, RoutedEventArgs e)
		{
			if (txnInProgress)
			{
				return;
			}
			if (eftIP.Connect())
			{
				txnInProgress = true;
				eftIP.DoHideDialogs();
				eftIP.DoLogon();
			}
			txnInProgress = false;
		}
	}
}
