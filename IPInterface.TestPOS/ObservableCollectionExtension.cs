using System.Collections.ObjectModel;

namespace PCEFTPOS.EFTClient.IPInterface.TestPOS
{
    public static class ObservableCollectionExtension
    {
        public static ObservableCollection<T> AddRange<T>(this ObservableCollection<T> observableCollection, ObservableCollection<T> collection) where T : class
        {
            foreach (T item in collection)
                observableCollection.Add(item);

            return observableCollection;
        }

        public static ObservableCollection<T> InsertRange<T>(this ObservableCollection<T> observableCollection, int index, ObservableCollection<T> collection) where T : class
        {
            for(int i = 0; i < collection.Count; i++)
            {
                observableCollection.Insert(index+i, collection[i]);
            }

            return observableCollection;
        }
    }
}
