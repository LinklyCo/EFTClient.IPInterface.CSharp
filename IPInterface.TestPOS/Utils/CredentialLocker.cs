using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    public interface ICredentialLocker
    {
        void LoadCredentials();
        void SaveCredentials(string input);
        string Password { get; }
    }

    public class CredentialLocker : ICredentialLocker
    {
        [JsonProperty]
        private readonly byte[] _entropy = new byte[20];
        private byte[] _decryptedData = new byte[0];

        [JsonProperty]
        private string _entropyString = "";

        [JsonIgnore]
        public string Password
        {
            get
            {
                if (_decryptedData == null)
                {
                    LoadCredentials();
                }

                return Encoding.Unicode.GetString(_decryptedData);
            }
        }

        public void LoadCredentials()
        {
            if (!string.IsNullOrEmpty(_entropyString))
            {
                _decryptedData = ProtectedData.Unprotect(Convert.FromBase64String(_entropyString), _entropy, DataProtectionScope.CurrentUser);
            }
        }

        public void SaveCredentials(string input)
        {
            using (RNGCryptoServiceProvider r = new RNGCryptoServiceProvider()) { r.GetBytes(_entropy); }
            byte[] encryptedData = ProtectedData.Protect(Encoding.Unicode.GetBytes(input), _entropy, DataProtectionScope.CurrentUser);
            _entropyString = Convert.ToBase64String(encryptedData);
        }
    }
}
