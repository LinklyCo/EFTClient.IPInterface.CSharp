using System;
using Microsoft.Win32;

namespace IPInterface.TestPOS.Utils
{
    public static class EFTRegistry
    {
        public static RegistryKey OpenRegistryBaseKey() =>
            Registry.LocalMachine.OpenSubKey("SOFTWARE\\CullenSoftwareDesign\\EFTCLIENT", RegistryKeyPermissionCheck.ReadSubTree) ??
            Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\CullenSoftwareDesign\\EFTCLIENT", RegistryKeyPermissionCheck.ReadSubTree);
        public static bool TryOpenRegistryBaseKey(out RegistryKey baseKey) =>
            (baseKey = OpenRegistryBaseKey()) != null;

        public static RegistryKey OpenRegistrySubKey(string subKeyName) =>
            TryOpenRegistryBaseKey(out var baseKey) ? baseKey.OpenSubKey(subKeyName) : null;
        public static bool TryOpenRegistrySubKey(string subKeyName, out RegistryKey subKey) =>
            (subKey = OpenRegistrySubKey(subKeyName)) != null;

        public static string GetSubkeyStringValue(string subKeyName, string valueName, string defaultValue = null)
        {
            string value = null;
            using (var subKey = OpenRegistrySubKey(subKeyName))
            {
                try
                {
                    if (subKey?.GetValueKind(valueName) == RegistryValueKind.String)
                    {
                        value = subKey.GetValue(valueName) as string;
                    }
                }
                catch (Exception)
                {
                    // Use default value
                }
            }

            return value ?? defaultValue;
        }
    }
}
