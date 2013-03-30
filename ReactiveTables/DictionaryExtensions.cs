using System.Collections.Generic;

namespace ReactiveTables
{
    public static class DictionaryExtensions
    {
        public static TValue AddNewIfNotExists<TKey, TValue> (this Dictionary<TKey, TValue> dictionary, TKey key) 
            where TValue : class, new()
        {
            TValue value;
            if (!dictionary.TryGetValue(key, out value));
            {
                value = new TValue();
                dictionary.Add(key, value);
            }
            return value;
        }
    }
}