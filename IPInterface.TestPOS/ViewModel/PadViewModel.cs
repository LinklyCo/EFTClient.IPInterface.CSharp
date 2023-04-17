
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS.ViewModel
{


    public class PadTagViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        private string name = string.Empty;
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

        private string data = string.Empty;
        public string Data
        {
            get
            {
                return data;
            }
            set
            {
                data = value;
                NotifyPropertyChanged(nameof(Data));
            }
        }

        public override string ToString()
        {
            return $"{name}{data.Length.ToString().PadLeft(3, '0')}{data}";
        }
    }

    public class PadViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void NotifyPropertyChanged(string info)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(info));
        }

        readonly string _filename = "temp.json";
        ExternalDataList _items = new ExternalDataList();
        public ExternalDataList UpdatedExternalData
        {
            set
            {
                _items = value;
            }
            get
            {
                _items.Clear();
                _items.AddRange(PadContentList);

                return _items;
            }
        }


        public ObservableCollection<PadTagViewModel> PadTags { get; set; } = new ObservableCollection<PadTagViewModel>();
        public ObservableCollection<ExternalData> PadContentList { get; set; } = new ObservableCollection<ExternalData>();

        string _padName = string.Empty;
        public string PadName
        {
            get
            {
                return _padName;
            }
            set
            {
                _padName = value;
                NotifyPropertyChanged("PadName");
            }
        }

        string _padValue = string.Empty;
        public string PadValue
        {
            get
            {
                return _padValue;
            }
            set
            {
                _padValue = value;
                NotifyPropertyChanged("PadValue");
            }
        }

        public string PadValueDisplay
        {
            get => PadValue.ToVisibleSpaces();
            set
            {
                PadValue = value.FromVisibleSpaces();
                NotifyPropertyChanged("PadValueDisplay");
            }
        }

        string _padTagName = string.Empty;
        public string PadTagName
        {
            get
            {
                return _padTagName;
            }
            set
            {
                _padTagName = value;
                NotifyPropertyChanged("PadTagName");
            }
        }

        string _padTagValue = string.Empty;
        public string PadTagValue
        {
            get
            {
                return _padTagValue;
            }
            set
            {
                _padTagValue = value;
                NotifyPropertyChanged("PadTagValue");
            }
        }

        bool _editMode = false;
        public bool EditMode
        {
            get
            {
                return _editMode;
            }
            set
            {
                _editMode = value;
                NotifyPropertyChanged("EditMode");
            }
        }

        bool _dataButtonVisible = true;
        public bool DataButtonVisible
        {
            get
            {
                return _dataButtonVisible;
            }
            set
            {
                _dataButtonVisible = value;
                NotifyPropertyChanged("DataButtonVisible");
            }
        }

        string _collectionName = "PAD Tag Collection";
        public string CollectionName
        {
            get
            {
                return _collectionName;
            }
            set
            {
                _collectionName = value;
                NotifyPropertyChanged("CollectionName");
            }
        }

        int _currentPadIndex = -1;

        public PadViewModel(string filename)
        {
            if (!string.IsNullOrEmpty(filename))
            {
                _filename = filename;
                Load();
            }

            if (_items != null)
            {
                PadContentList = new ObservableCollection<ExternalData>(_items);
            }
        }

        public void Save()
        {
            try
            {
                string json = JsonConvert.SerializeObject(UpdatedExternalData.ToArray(), Formatting.Indented);
                File.WriteAllText(_filename, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        private void Load()
        {
            try
            {
                ExternalDataList list = (ExternalDataList)JsonConvert.DeserializeObject(File.ReadAllText(_filename), typeof(ExternalDataList));
                if (list != null && list.Count > 0)
                {
                    _items.Clear();
                    _items.AddRange(list);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message);
            }
        }

        public bool AddPadContentFunc()
        {
            if (string.IsNullOrEmpty(PadName))
                return false;

            if (PadContentList.ToList().Exists(x => x.Name.Equals(PadName)))
                return false;

            PadContentList.Add(new ExternalData(PadName, PadValue));
            PadName = string.Empty;
            PadValue = string.Empty;

            return true;
        }


        public bool AddPadTagFunc()
        {
            if (string.IsNullOrEmpty(PadTagName))
                return false;

            if (PadTags.ToList().Exists(x => x.Name.Equals(PadTagName)))
                return false;

            PadTags.Add(new PadTagViewModel() { Name = PadTagName, Data = PadTagValue });
            PadTagName = string.Empty;
            PadTagValue = string.Empty;

            return true;
        }

        public bool UpdatePadContentFunc(int index)
        {
            if (string.IsNullOrEmpty(PadName) || index < 0)
                return false;

            PadContentList[index].Name = PadName;
            PadContentList[index].Value = PadValue;

            return true;
        }

        public bool UpdatePadTagFunc(int index)
        {
            if (string.IsNullOrEmpty(PadTagName) || index < 0)
                return false;

            PadTags[index].Name = PadTagName;
            PadTags[index].Data = PadTagValue;
            return true;
        }

        public RelayCommand DeletePadContent
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    var index = (int)o;
                    if (index < 0)
                        return;

                    PadContentList.RemoveAt(index);
                    PadName = string.Empty;
                    PadValue = string.Empty;
                });
            }
        }

        public RelayCommand DeletePadTag
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    if (o == null)
                        return;

                    int index = (int)o;
                    if (index < 0)
                        return;

                    PadTags.RemoveAt(index);

                    PadTagName = string.Empty;
                    PadTagValue = string.Empty;
                });
            }
        }

        public RelayCommand AddPadTag
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    if (string.IsNullOrEmpty(PadName))
                        return;

                    PadTags.Add(new PadTagViewModel() { Name = PadName, Data = PadValue });

                    PadName = string.Empty;
                    PadValue = string.Empty;
                });
            }
        }

        public RelayCommand LoadEditor
        {
            get
            {
                return new RelayCommand((o) =>
                {
                    if (o == null)
                        return;

                    _currentPadIndex = (int)o;
                    if (_currentPadIndex < 0)
                        return;

                    PadTags.Clear();
                    PadTagName = string.Empty;
                    PadTagValue = string.Empty;

                    var externalData = PadContentList[_currentPadIndex];

                    PadTags.Clear();

                    var pt = new PadField(externalData?.Value);
                    foreach (var pf in pt)
                    {
                        PadTags.Add(new PadTagViewModel() { Name = pf.Name, Data = pf.Data });
                    }

                    EditMode = true;
                });
            }
        }

        public bool SavePadFieldFunc()
        {
            if (_currentPadIndex < 0)
                return false;

            // Convert ObservableCollection<PadTagViewModel> into PadField
            var pf = new PadField();
            foreach (var p in PadTags)
            {
                pf.Add(new PadTag(p.Name, p.Data));
            }

            PadContentList[_currentPadIndex].Value = pf.GetAsString(false);
            EditMode = false;

            return true;
        }

    }
}
