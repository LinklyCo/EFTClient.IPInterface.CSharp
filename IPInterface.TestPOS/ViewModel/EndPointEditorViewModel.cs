
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel
{
    public class EndPointEditorViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<EndPointViewModel> EndPoints { get; set; }


        public EndPointEditorViewModel()
        {
            EndPoints = new ObservableCollection<EndPointViewModel>();
            AccountTypes = new ObservableCollection<string>(new string[] { "Undefined", "Local", "Cloud", "CloudWithLegacyPairing" });
        }

        protected void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        [JsonIgnore]
        public ObservableCollection<string> AccountTypes { get; }

        EndPointViewModel currentItem = null;
        public EndPointViewModel CurrentItem
        {
            get
            {
                return currentItem;
            }
            set
            {
                currentItem = value;
                NotifyPropertyChanged(nameof(CurrentItem));
            }
        }

        public bool AddAccountFunc()
        {
            EndPoints.Add(new EndPointViewModel() { Name = "[NEW]", Type = 0 });
            return true;
        }

        public RelayCommand ResetDefaultValues
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    switch (currentItem?.Type)
                    {
                        case EndPointType.Undefined:
                            CurrentItem.Address = string.Empty;
                            CurrentItem.Port = 0;
                            break;
                        case EndPointType.Local:
                            CurrentItem.Address = "127.0.0.1";
                            CurrentItem.Port = 2011;
                            break;
                        case EndPointType.Cloud:
                        case EndPointType.CloudWithLegacyPairing:
                            CurrentItem.Address = "pos.sandbox.cloud.pceftpos.com";
                            CurrentItem.Port = 443;
                            break;
                    }
                });
            }
        }
    }
}