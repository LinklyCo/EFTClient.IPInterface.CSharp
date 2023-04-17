using System;
using System.Collections.Generic;
using System.Text;

namespace PCEFTPOS.EFTClient.IPInterface
{
    public static class EFTRequestCommandCode
    {
        public const char Monitoring = '|';
        public const char Logon = 'G';
        public const char CloudLogon = 'A';
        public const char Transaction = 'M';
        public const char PINAuth = 'W';
        public const char QueryCard = 'J';
        public const char ConfigureMerchant = '1';
        public const char ReprintReceipt = 'C';
        public const char GetLastTransaction = 'N';
        public const char Status = 'K';
        public const char SendKey = 'Y';
        public const char ChequeAuth = 'H';
        public const char Settlement = 'P';
        public const char DisplayControlPanel = '5';
        public const char SetDialog = '2';
        public const char GetClientList = 'Q';
        public const char Generic = 'X';
        public const char Heartbeat = 'F';
        public const char Receipt = '3';
    }
}
