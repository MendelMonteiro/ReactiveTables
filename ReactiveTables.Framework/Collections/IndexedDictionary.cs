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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveTables.Framework.Collections
{
    /// <summary>
    /// A dictionary which has values which can be accessed by key or by index 
    /// (created in order of addition to the dictionary).
    /// Note that this uses <see cref="FieldRowList{T}"/> and therefore exhibits the same behaviour
    /// of leaving gaps in the list instead of re-shuffling to make the list contiguous upon deletes.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    class IndexedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IList<TValue>
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
//        private readonly List<KeyValuePair<TKey, TValue>> _list = new List<KeyValuePair<TKey, TValue>>();
        private readonly FieldRowList<KeyValuePair<TKey, TValue>> _list = new FieldRowList<KeyValuePair<TKey, TValue>>();

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return _list.Select(i => i.Value).GetEnumerator();
        }

        public void Add(TValue item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(TValue item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(TValue item)
        {
            throw new NotImplementedException();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _dictionary.Clear();
            _list.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public int Count { get { return _dictionary.Count; } }

        public bool IsReadOnly { get { return false; } }

        public int IndexOf(TValue item)
        {
            throw new NotImplementedException();
//            return _list.FindIndex(i => item.Equals(i.Value));
        }

        public void Insert(int index, TValue item)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public bool ContainsKey(TKey key)
        {
            return _dictionary.ContainsKey(key);
        }

        public void Add(TKey key, TValue value)
        {
            AddWithIndex(key, value);
        }

        public int AddWithIndex(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            return _list.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        /// <summary>
        /// This is an O(N) operation - use wisely
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool Remove(TKey key)
        {
            _list.Remove(new KeyValuePair<TKey, TValue>(key, _dictionary[key]));
            return _dictionary.Remove(key);
        }

        /// <summary>
        /// This is an O(1) operation
        /// </summary>
        /// <param name="index"></param>
        public void RemoveAt(int index)
        {
            var item = _list[index];
            _list.RemoveAt(index);
            _dictionary.Remove(item.Key);
        }

        /// <summary>
        /// O(N) operation
        /// </summary>
        /// <param name="key"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public bool RemoveWithIndex(TKey key, out int index)
        {
            index = _list.IndexOf(new KeyValuePair<TKey, TValue>(key, _dictionary[key]));
            if (index >= 0)
            {
                _list.RemoveAt(index);
                _dictionary.Remove(key);
                return true;
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public TValue this[int index]
        {
            get { return _list[index].Value; }
            set { throw new NotImplementedException(); }
        }

        public TValue this[TKey key]
        {
            get { return _dictionary[key]; }
            set
            {
                if (_dictionary.ContainsKey(key))
                {
                    var index = _list.IndexOf(new KeyValuePair<TKey, TValue>(key, _dictionary[key]));
                    _list[index] = new KeyValuePair<TKey, TValue>(key, value);
                }
                else
                {
                    _list.Add(new KeyValuePair<TKey, TValue>(key, value));
                }
                _dictionary[key] = value;
            }
        }

        public ICollection<TKey> Keys { get { return _dictionary.Keys; } }
        public ICollection<TValue> Values { get { return _dictionary.Values; } }
    }
}
