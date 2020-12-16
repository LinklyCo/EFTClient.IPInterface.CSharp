using System.ComponentModel;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel
{
    public enum EndPointType { Undefined, Local, Cloud, CloudWithLegacyPairing };

    public class EndPointViewModel : INotifyPropertyChanged
    {
        public override string ToString()
        {
            return name;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }


        EndPointType endpointType = EndPointType.Undefined;
        public EndPointType Type
        {
            get
            {
                return endpointType;
            }
            set
            {
                endpointType = value;
                NotifyPropertyChanged(nameof(Type));
                NotifyPropertyChanged(nameof(TypeAsInt));
            }
        }

        // TODO: CHANGE TO CONVERTER!
        public int TypeAsInt
        {
            get
            {
                return (int)endpointType;
            }
            set
            {
                endpointType = (EndPointType)value;
                NotifyPropertyChanged(nameof(Type));
                NotifyPropertyChanged(nameof(TypeAsInt));
            }
        }

        string name = string.Empty;
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                NotifyPropertyChanged(nameof(Name));
            }
        }

        string address = string.Empty;
        public string Address
        {
            get
            {
                return address;
            }
            set
            {
                address = value;
                NotifyPropertyChanged(nameof(Address));
            }
        }

        int port = 0;
        public int Port
        {
            get
            {
                return port;
            }
            set
            {
                port = value;
                NotifyPropertyChanged(nameof(Port));
            }
        }

        bool autoLoadedKey = false;
        public bool AutoLoadedKey
        {
            get
            {
                return autoLoadedKey;
            }
            set
            {
                autoLoadedKey = value;
                NotifyPropertyChanged(nameof(AutoLoadedKey));
            }
        }
        

        public bool UseSSL => Type == EndPointType.Cloud || Type == EndPointType.CloudWithLegacyPairing;


        string clientId = string.Empty;
        public string ClientId
        {
            get
            {
                return clientId;
            }
            set
            {
                clientId = value;
                NotifyPropertyChanged(nameof(ClientId));
            }
        }


        string password = string.Empty;
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
                NotifyPropertyChanged(nameof(Password));
            }
        }

        string pairingCode = string.Empty;
        public string PairingCode
        {
            get
            {
                return pairingCode;
            }
            set
            {
                pairingCode = value;
                NotifyPropertyChanged(nameof(PairingCode));
            }
        }

        string token = string.Empty;
        public string Token
        {
            get
            {
                return token;
            }
            set
            {
                token = value;
                NotifyPropertyChanged(nameof(Token));
            }
        }

    }
}
