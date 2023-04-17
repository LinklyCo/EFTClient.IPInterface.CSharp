
using QRCoder;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel
{
    public class ProxyViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public event EventHandler<EFTSendKeyRequest> OnSendKey;
        public char[] TrimChars { get; set; } = { (char)0x02, '\0' };

        protected void OnPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private EFTDisplayResponse _displayDetails = new EFTDisplayResponse();
        public EFTDisplayResponse DisplayDetails
        {
            get
            {
                _displayDetails.DisplayText[0] = _displayDetails.DisplayText[0].Trim(TrimChars);
                _displayDetails.DisplayText[1] = _displayDetails.DisplayText[1].Trim(TrimChars);

                return _displayDetails;
            }
            set
            {
                _displayDetails = value;
                OnPropertyChanged(nameof(DisplayDetails));
                OnPropertyChanged(nameof(InputTypeString));
                OnPropertyChanged(nameof(QRImage));
            }
        }

        public string InputTypeString => Enum.GetName(typeof(InputType), DisplayDetails.InputType);

        private static BitmapImage ConvertToBitmapImage(Bitmap bitmap)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                return ConvertToBitmapImage(memory);
            }
        }

        private static BitmapImage ConvertToBitmapImage(Stream stream)
        {
            stream.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = stream;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        private static BitmapImage ConvertToBitmapImage(string filePath)
        {
            using (FileStream file = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                return ConvertToBitmapImage(file);
            }
        }

        public BitmapSource QRImage
        {
            get
            {
                if ((DisplayDetails?.PurchaseAnalysisData ?? null) != null)
                {
                    //When DAT.TAG is "QRI", display image located at DAT.PTH
                    foreach (PadTag tag in DisplayDetails.PurchaseAnalysisData.FindTags("DAT"))
                    {
                        PadField pf = new PadField(tag.Data);
                        int pathIdx = pf?.FindTag("PTH") ?? -1;
                        int tagIdx = pf?.FindTag("TAG") ?? -1;

                        if (tagIdx >= 0 && pathIdx >= 0)
                        {
                            if (pf[tagIdx].Data == "QRI")
                            {
                                return ConvertToBitmapImage(pf[pathIdx].Data);
                            }
                        }
                    }

                    //Generate QR code from QRC content
                    int contentIdx = DisplayDetails.PurchaseAnalysisData.FindTag("QRC");
                    if (contentIdx >= 0)
                    {
                        string qrContent = DisplayDetails.PurchaseAnalysisData[contentIdx].Data;
                        QRCodeGenerator qrGenerator = new QRCodeGenerator();
                        QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrContent, QRCodeGenerator.ECCLevel.H);
                        QRCode qrCode = new QRCode(qrCodeData);
                        Bitmap qrBitmap = qrCode.GetGraphic(20);
                        return ConvertToBitmapImage(qrBitmap);
                    }
                }
                return null;
            }
        }

        private bool EnumContains<T>(string val, out T item)
        {
            bool result = false;
            item = default;

            foreach (var i in Enum.GetValues(typeof(T)))
            {
                string s = i.ToString();
                if (s.ToUpper().Contains(val.ToUpper()))
                {
                    T x = (T)Enum.Parse(typeof(T), s);
                    item = x;
                    result = true;
                    break;
                }
            }

            return result;
        }

        EFTSendKeyRequest _keyRequest = new EFTSendKeyRequest();
        public EFTSendKeyRequest KeyRequest { get { return _keyRequest; } set { _keyRequest = value; OnPropertyChanged(nameof(KeyRequest)); } }

        EFTPOSKey _key;
        public EFTPOSKey Key { get { return _key; } set { _key = value; OnPropertyChanged(nameof(Key)); } }

        string _keyData = "";
        public string KeyData
        {
            get { return _keyData; }
            set { _keyData = value; OnPropertyChanged(nameof(KeyData)); }
        }
        RelayCommand _sendKeyCommand;
        public RelayCommand SendKeyCommand => _sendKeyCommand ?? (_sendKeyCommand = new RelayCommand((o) =>
              {
                  string name = o.ToString();
                  ProxyDialog window = Application.Current.Windows.OfType<ProxyDialog>().FirstOrDefault();
                  EFTPOSKey key = EFTPOSKey.OkCancel;
                  if (EnumContains(name, out key))
                  {
                      KeyRequest.Key = key;
                      KeyRequest.Data = KeyData.ToString() + window.pwordInput.Password;
                      OnSendKey?.Invoke(this, KeyRequest);
                      KeyData = "";
                      window.pwordInput.Password = "";
                  }
              }));


        public bool ProxyWindowClosing { get; set; } = false;


        public void SendKeyFunc(EFTSendKeyRequest key)
        {
            OnSendKey?.Invoke(this, key);
        }


        public IEFTClientIPAsync EFTClientIPAsync { get; set; }

    }
}
