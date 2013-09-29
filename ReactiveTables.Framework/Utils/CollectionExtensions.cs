using System.Collections.Generic;

namespace ReactiveTables.Framework.Utils
{
    public static class CollectionExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> collection, T item)
        {
            var i = 0;
            foreach (var foo in collection)
            {
                if (foo.Equals(item))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }
}