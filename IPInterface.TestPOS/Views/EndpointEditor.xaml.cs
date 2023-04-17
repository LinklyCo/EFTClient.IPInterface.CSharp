
using Microsoft.Win32;
using Newtonsoft.Json;
using PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using IPInterface.TestPOS.Utils;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    /// <summary>
    /// Interaction logic for PadEditor.xaml
    /// </summary>
    public partial class EndpointEditor : Window
    {
        private readonly string filename;
        private readonly EndPointEditorViewModel vm = null;

        public EndpointEditor(string filename)
        {
            this.filename = filename;

            vm = File.Exists(filename) ? JsonConvert.DeserializeObject<EndPointEditorViewModel>(File.ReadAllText(filename)) : new EndPointEditorViewModel();
            if (vm.EndPoints.Count == 0)
            {
                vm.EndPoints.Add(new EndPointViewModel() { Name = "Cloud (sandbox)", Address = "pos.sandbox.cloud.pceftpos.com", Port = 443, Type = EndPointType.Cloud });
            }

            vm.CurrentItem = vm.EndPoints[0];

            DataContext = vm;
            InitializeComponent();
        }

        public ObservableCollection<EndPointViewModel> EndPoints => vm?.EndPoints.InsertRange(0, LoadLocalEndpoints());

        private EndPointViewModel CI => vm?.CurrentItem;

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            vm.EndPoints.Add(new EndPointViewModel() { Name = "[NEW]" });

            LstAccountView.SelectedIndex = vm.EndPoints.Count - 1;

            txtName.Focus();
        }

        private void Save()
        {
            // on save if we updated any of the credentials fields without re-pairing then unpair that connection
            // to prevent confusion
            foreach (var endPoint in vm.EndPoints)
            {
                if (!endPoint.IsTokenStillValid())
                {
                    endPoint.Token = "";
                }
            }

            var svm = new EndPointEditorViewModel()
            {
                EndPoints = new ObservableCollection<EndPointViewModel>(vm.EndPoints.Where(ep => !ep.AutoLoadedKey))
            };

            var content = JsonConvert.SerializeObject(svm);
            File.WriteAllText(filename, content);
        }
        private void BtnApply_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            Save();
            DialogResult = true;
            Close();
        }

        private async void BtnPairPinpad_Click(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(CI?.Address))
            {
                MessageBox.Show($"Invalid Address", "Invalid Address", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            BtnPairPinpad.IsEnabled = false;
            Mouse.OverrideCursor = Cursors.AppStarting;

            try
            {
                var eft = new EFTClientIPAsync();

                if (await eft.ConnectAsync(CI.Address, CI.Port, true, true))
                {
                    var r = await eft.WriteRequestAndWaitAsync<EFTCloudPairResponse>(new EFTCloudPairRequest() { ClientID = CI.ClientId, Password = CI.Password, PairingCode = CI.PairingCode }, new CancellationTokenSource(TimeSpan.FromSeconds(45)).Token);

                    var description = "";
                    if (r.ResponseCode.Equals("CX", StringComparison.OrdinalIgnoreCase))
                    {
                        description = "\n\nEnter the correct pairing code from the terminal\nscreen and click 'Pair PINpad' again";
                    }

                    if (r.Success)
                    {
                        CI.Token = r.Token;
                    }

                    MessageBox.Show($"Success: {r.Success}\nResponse Code: {r.ResponseCode}\nResponse Text: {r.ResponseText}{description}", "PINpad Pairing Result", MessageBoxButton.OK, r.Success ? MessageBoxImage.Information : MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
            Mouse.OverrideCursor = Cursors.Arrow;
            BtnPairPinpad.IsEnabled = true;
        }

        private void BtnUnpair_Click(object sender, RoutedEventArgs e)
        {
            CI.Token = "";
            CI.PairingCode = "";
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (LstAccountView.SelectedIndex >= 0)
            {
                vm.EndPoints.RemoveAt(LstAccountView.SelectedIndex);

                if (vm.EndPoints.Count > 0)
                {
                    LstAccountView.SelectedIndex = vm.EndPoints.Count - 1;
                }
            }
        }

        private void BtnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            if (LstAccountView.SelectedIndex <= 0 || vm.EndPoints.Count <= 1)
            {
                return;
            }

            var idx = LstAccountView.SelectedIndex;
            var tmp = vm.EndPoints[idx];
            vm.EndPoints.RemoveAt(idx);
            vm.EndPoints.Insert(idx - 1, tmp);
            LstAccountView.SelectedIndex = idx - 1;
        }

        private void BtnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            if (LstAccountView.SelectedIndex >= vm.EndPoints.Count - 1)
            {
                return;
            }

            var idx = LstAccountView.SelectedIndex;
            var tmp = vm.EndPoints[idx];
            vm.EndPoints.RemoveAt(idx);
            vm.EndPoints.Insert(idx + 1, tmp);
            LstAccountView.SelectedIndex = idx + 1;

        }

        private ObservableCollection<EndPointViewModel> LoadLocalEndpoints()
        {
            var result = new ObservableCollection<EndPointViewModel>();

            RegistryKey csdKey = EFTRegistry.OpenRegistryBaseKey();
            try
            {
                var eftClientKeys = csdKey?.GetSubKeyNames()?.Where(n => n.StartsWith("EFTCLIENT"));
                if (eftClientKeys != null)
                {
                    foreach (var eftClientKey in eftClientKeys)
                    {
                        var clientKey = csdKey.OpenSubKey($"{eftClientKey}\\CLIENT");
                        try
                        {
                            var v = clientKey?.GetValue("IP_INTERFACE_PORT", 2011);
                            if (v is int port)
                            {
                                result.Add(new EndPointViewModel() { Name = $"[AUTO] localhost:({port})", Type = EndPointType.Local, Address = "localhost", Port = port, AutoLoadedKey = true });
                            }
                        }
                        finally
                        {
                            clientKey?.Close();
                        }
                    }
                }
            }
            catch
            {
                // Suppressed - invalid registry config could blow up here
            }
            finally
            {
                csdKey?.Close();
            }


            // make sure we always have at least the local client
            if(result.Count == 0)
            {
                result.Add(new EndPointViewModel() { Name = $"[AUTO] localhost:(2011)", Type = EndPointType.Local, Address = "localhost", Port = 2011, AutoLoadedKey = true });
            }
            return result;
        }
    }

}
