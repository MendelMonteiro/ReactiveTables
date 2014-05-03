/*This file is part of ReactiveTables.

ReactiveTables is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ReactiveTables is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
*/

using System.Collections.Generic;

namespace ReactiveTables.Framework.Utils
{
    /// <summary>
    /// Helper class for working with dictionaries.
    /// </summary>
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Add a new default element if it does not already exist
        /// </summary>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static TValue GetOrAddNew<TKey, TValue> (this IDictionary<TKey, TValue> dictionary, TKey key) 
            where TValue : class, new()
        {
            TValue value;
            bool exists = dictionary.TryGetValue(key, out value);
            if (!exists)
            {
                value = new TValue();
                dictionary.Add(key, value);
            }
            return value;
        }

        /// <summary>
        /// Copy to another dictionary of the same type
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public static void CopyTo<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> target)
        {
            foreach (var keyValue in source)
            {
                target.Add(keyValue.Key, keyValue.Value);
            }
        }

        /// <summary>
        /// Converts a dictionary so that the values become the keys and the keys become the values.
        /// Note that the values must also be unique
        /// </summary>
        /// <param name="dictionary"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static Dictionary<TValue, TKey> InverseUniqueDictionary<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
        {
            var inverse = new Dictionary<TValue, TKey>(dictionary.Count);
            foreach (var value in dictionary)
            {
                inverse.Add(value.Value, value.Key);
            }
            return inverse;
        }
    }
}
