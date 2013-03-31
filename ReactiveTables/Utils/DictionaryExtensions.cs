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