using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace PCEFTPOS.EFTClient.IPInterface
{
    class SslValidator
    {
        public delegate void DoLog(LogLevel level, Action<TraceRecord> traceAction);

        private readonly DoLog _log;

        private readonly bool _isDebug = false;

        private readonly string[] _customRootCerts;

        public SslValidator(DoLog log = null)
        {
#if (DEBUG)
            _isDebug = true;
#endif
            _log = log;
            _customRootCerts = FindCustomRootCerts();
        }

        private string[] FindCustomRootCerts()
        {
            try
            {
                var extensions = new [] { ".der", ".pem" };
                var certs = Directory.EnumerateFiles(Directory.GetCurrentDirectory())
                    .Where(f => extensions.Contains(new FileInfo(f).Extension));
                return certs.ToArray();
            }
            catch (Exception ex)
            {
                Log(LogLevel.Error, tr => tr.Set($"Failed to get certs: {ex.Message}"));
            }

            return null;
        }

        private void Log(LogLevel level, Action<TraceRecord> traceAction)
        {
            if (_log != null)
                _log(level, traceAction);
        }

        private void LogRemoteCertificateFailure(X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            Log(LogLevel.Error, tr =>
            {
                var msg = new System.Text.StringBuilder();
                msg.AppendLine($"SslPolicyErrors={sslPolicyErrors}");
                msg.AppendLine($"{chain.ChainElements.Count} certificates in chain");
                int i = 0;
                foreach (var c in chain.ChainElements)
                {
                    msg.AppendLine($"Idx:\"{i++}\", Subject:\"{c.Certificate.Subject}\", Issuer:\"{c.Certificate.Issuer}\", Serial:\"{c.Certificate.SerialNumber}\", Before:\"{c.Certificate.NotBefore}\", After:\"{c.Certificate.NotAfter}\", Thumbprint:\"{c.Certificate.Thumbprint}\"");
                }

                tr.Message = msg.ToString();
            });
        }

        /// <summary>
        /// Validate the PC-EFTPOS Cloud server certificate. 
        /// </summary>
        /// <returns>TRUE if the certificate is valid, FALSE otherwise</returns>
        public bool RemoteCertificateValidationCallback(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (_isDebug)
            {
                return true;
            }

            // Certificate chain is valid via a commercial 3rd party chain 
            if (sslPolicyErrors == SslPolicyErrors.None)
            {
                Log(LogLevel.Info, tr => tr.Set("Remote certificate validated successful by installed CA"));
                return true;
            }

            // Certificate has an invalid CN or isn't available from the server
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateNotAvailable || sslPolicyErrors == SslPolicyErrors.RemoteCertificateNameMismatch)
            {
                LogRemoteCertificateFailure(certificate, chain, sslPolicyErrors);
                return false;
            }

            // The certificate is invalid due to an invalid chain. If we have included custom certificates we can attempt to validate here
            if (sslPolicyErrors == SslPolicyErrors.RemoteCertificateChainErrors && _customRootCerts?.Length > 0)
            {
                // Load custom certificates
                var x509Certificates = new List<X509Certificate2>();
                foreach (var certFilename in _customRootCerts)
                {
                    try
                    {
                        x509Certificates.Add(new X509Certificate2(certFilename));
                    }
                    catch (System.Security.Cryptography.CryptographicException e)
                    {
                        Log(LogLevel.Error, tr => tr.Set($"Error loading certificate ({certFilename})", e));
                        return false;
                    }
                }

                var c = new X509Chain();
                try
                {
                    c.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                    c.ChainPolicy.VerificationFlags = X509VerificationFlags.IgnoreWrongUsage;
                    c.ChainPolicy.ExtraStore.AddRange(x509Certificates.ToArray());
                    // Check if the chain is valid
                    if (c.Build((X509Certificate2)certificate))
                    {
                        Log(LogLevel.Info, tr => tr.Set($"Remote certificate validated successful by custom cert chain"));
                        return true;
                    }
                    // The chain may not be valid, but if the only fault is an UntrustedRoot we can check if we have the custom root
                    else
                    {
                        if (c.ChainStatus?.Length > 0 && c.ChainStatus[0].Status == X509ChainStatusFlags.UntrustedRoot)
                        {
                            var root = c.ChainElements[c.ChainElements.Count - 1];
                            if (x509Certificates.Find(x509Certificate => x509Certificate.Thumbprint == root.Certificate.Thumbprint) != null)
                            {
                                Log(LogLevel.Info, tr => tr.Set($"Remote certificate validated successful by custom cert chain and root"));
                                return true;
                            }
                        }
                    }
                }
                finally
                {
                    c.Reset();
                }
            }

            LogRemoteCertificateFailure(certificate, chain, sslPolicyErrors);
            return false;
        }
    }
}
