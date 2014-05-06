using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReactiveTables.Demo.Utils
{
    /// <summary>
    /// An observable collection which is accessible via a key
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    public class IndexedObservableCollection<T, TKey> : ObservableCollection<T>
    {
        private readonly Func<T, TKey> _keySelector;
        readonly Dictionary<TKey, int> _keysToIndeces = new Dictionary<TKey, int>(); 

        public IndexedObservableCollection(Func<T, TKey> keySelector)
        {
            _keySelector = keySelector;
        }

        public int GetIndexForKey(TKey key)
        {
            return _keysToIndeces[key];
        }

        protected override void SetItem(int index, T item)
        {
            _keysToIndeces[_keySelector(item)] = index;
            base.SetItem(index, item);
        }

        protected override void InsertItem(int index, T item)
        {
            _keysToIndeces[_keySelector(item)] = index;
            base.InsertItem(index, item);
        }

        protected override void MoveItem(int oldIndex, int newIndex)
        {
            _keysToIndeces[_keySelector(this[oldIndex])] = newIndex;
            base.MoveItem(oldIndex, newIndex);
        }

        protected override void ClearItems()
        {
            _keysToIndeces.Clear();
            base.ClearItems();
        }

        protected override void RemoveItem(int index)
        {
            _keysToIndeces.Remove(_keySelector(this[index]));
            base.RemoveItem(index);
        }
    }
}
