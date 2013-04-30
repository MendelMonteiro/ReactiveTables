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

using System.Collections.Generic;

namespace ReactiveTables.Framework.Utils
{
    /// <summary>
    /// Can we use a HashSet<Tuple<K, V>> where the hashcode is distributed well enought?
    /// </summary>
    /// <typeparam name="TLeft"></typeparam>
    /// <typeparam name="TRight"></typeparam>
    internal class BidirectionalDictionary<TLeft, TRight>
    {
        private readonly Dictionary<TLeft, TRight> _leftToRight = new Dictionary<TLeft, TRight>();
        private readonly Dictionary<TRight, TLeft> _rightToLeft = new Dictionary<TRight, TLeft>();

        public void Add(KeyValuePair<TLeft, TRight> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _leftToRight.Clear();
            _rightToLeft.Clear();
        }

        public int Count
        {
            get { return _leftToRight.Count; }
        }

        public void Add(TLeft key, TRight value)
        {
            _leftToRight.Add(key, value);
            _rightToLeft.Add(value, key);
        }

        public bool ContainsKey(TLeft key)
        {
            return _leftToRight.ContainsKey(key);
        }

        public bool RemoveByKey(TLeft key)
        {
            var value = _leftToRight[key];
            _rightToLeft.Remove(value);
            return _leftToRight.Remove(key);
        }

        public bool TryGetValue(TLeft key, out TRight value)
        {
            return _leftToRight.TryGetValue(key, out value);
        }

        public bool ContainsValue(TRight value)
        {
            return _rightToLeft.ContainsKey(value);
        }

        public bool RemoveByValue(TRight value)
        {
            var key = _rightToLeft[value];
            _leftToRight.Remove(key);
            return _rightToLeft.Remove(value);
        }

        public bool TryGetKey(TRight value, out TLeft key)
        {
            return _rightToLeft.TryGetValue(value, out key);
        }

        public TRight this[TLeft key]
        {
            get { return _leftToRight[key]; }
            set
            {
                _leftToRight[key] = value;
                _rightToLeft[value] = key;
            }
        }
    }
}