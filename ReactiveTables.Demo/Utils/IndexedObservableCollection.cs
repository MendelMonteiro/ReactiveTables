// This file is part of ReactiveTables.
// 
// ReactiveTables is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReactiveTables is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
            var key = _keySelector(this[index]);
            _keysToIndeces.Remove(key);
            base.RemoveItem(index);
        }
    }
}
