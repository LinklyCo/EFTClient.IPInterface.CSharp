using System.ComponentModel;
using System.Runtime.Serialization;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel
{
    public enum EndPointType { Undefined, Local, Cloud, CloudWithLegacyPairing };

    public class EndPointViewModel : INotifyPropertyChanged
    {
        private EndPointType _priorEndpointType = EndPointType.Undefined;
        private string _priorClientId = string.Empty;
        private string _priorPassword = string.Empty;
        private string _priorPaircode = string.Empty;


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

                // if paired successfully update values associated with last pairing
                if (!string.IsNullOrEmpty(token))
                    SetPriorValues();

                NotifyPropertyChanged(nameof(Token));
            }
        }

        public bool IsTokenStillValid()
        {
            // if the token is empty we unpaired or it has never been paired. In which case it is "valid" for
            // our purposes (that is, not confusing a token for a different set of credentials with the currently
            // entered ones)
            if (Token == string.Empty) return true;

            return (Type == _priorEndpointType
                    && ClientId == _priorClientId
                    && PairingCode == _priorPaircode
                    && Password == _priorPassword
                );
        }

        /// <summary>
        /// This is called when we deserialize this. We set the properties needed to track if changes have occurred on save
        /// to the values they were during deserialization in order to be able to track them
        /// </summary>
        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            SetPriorValues();
        }



        /// <summary>
        /// Called whenever the prior values used for tracking config changes need to be updated to the current values
        /// This is called on deserialization and when the Token changes ( i.e. when we pair successfully)
        /// </summary>
        private void SetPriorValues()
        {
            _priorClientId = ClientId;
            _priorEndpointType = Type;
            _priorPaircode = PairingCode;
            _priorPassword = Password;
        }
    }
}
