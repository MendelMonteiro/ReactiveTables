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

namespace ReactiveTables.Framework.Utils
{
    /// <summary>
    /// Extensions for general collection types
    /// </summary>
    public static class CollectionExtensions
    {
        /// <summary>
        /// Limited to IList so as not to enumerate over types that would create allocations.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="collection"></param>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public static int IndexOf<T>(this IList<T> collection, Predicate<T> predicate)
        {
            var i = 0;
            for (var index = 0; index < collection.Count; index++)
            {
                var foo = collection[index];
                if (predicate(foo))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }
}