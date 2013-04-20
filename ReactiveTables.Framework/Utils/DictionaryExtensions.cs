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

namespace ReactiveTables.Utils
{
    public static class DictionaryExtensions
    {
        public static TValue AddNewIfNotExists<TKey, TValue> (this Dictionary<TKey, TValue> dictionary, TKey key) 
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

        public static void CopyTo<TKey, TValue>(this Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> target)
        {
            foreach (var keyValue in source)
            {
                target.Add(keyValue.Key, keyValue.Value);
            }
        }
    }
}
