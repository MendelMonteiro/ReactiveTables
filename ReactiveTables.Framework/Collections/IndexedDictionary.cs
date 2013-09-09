using System;
using System.Collections;
using System.Collections.Generic;

namespace ReactiveTables.Framework.Collections
{
    /// <summary>
    /// A dictionary which has values which can be accessed by key or by index (created 
    /// in order of addition to the dictionary).
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    class IndexedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IList<TValue>
    {
        private readonly Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
        private readonly List<TValue> _list = new List<TValue>();


        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public void Add(TValue item)
        {
            throw new NotImplementedException();
        }

        void ICollection<TValue>.Clear()
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

        int ICollection<TValue>.Count { get { return _dictionary.Count; } }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Clear()
        {
            _dictionary.Clear();
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

        int ICollection<KeyValuePair<TKey, TValue>>.Count { get { return _dictionary.Count; } }

        public bool IsReadOnly { get { return false; } }

        public int IndexOf(TValue item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, TValue item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
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
            _dictionary.Add(key, value);
            _list.Add(value);
        }

        public bool Remove(TKey key)
        {
            _list.Remove(_dictionary[key]);
            return _dictionary.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        TValue IList<TValue>.this[int index]
        {
            get { return _list[index]; }
            set { throw new NotImplementedException(); }
        }

        public TValue this[TKey key]
        {
            get { return _dictionary[key]; }
            set
            {
                if (_dictionary.ContainsKey(key))
                {
                    _list[_list.IndexOf(_dictionary[key])] = value;
                }
                else
                {
                    _list.Add(value);
                }
                _dictionary[key] = value;
            }
        }

        public ICollection<TKey> Keys { get { return _dictionary.Keys; } }
        public ICollection<TValue> Values { get { return _dictionary.Values; } }
    }
}
