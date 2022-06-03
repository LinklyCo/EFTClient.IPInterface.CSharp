using System.ComponentModel;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    public class Notifier : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }
    }

    public class Pad : Notifier
    {

        string _item = string.Empty;
        public string Item
        {
            get => _item;
            set
            {
                _item = value;
                NotifyPropertyChanged(nameof(Item));
            }
        }

        bool _isChecked = false;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                NotifyPropertyChanged(nameof(IsChecked));
            }
        }
        public Pad(string item)
        {
            Item = item;
            IsChecked = false;
        }

        public Pad() { }
    }

}
