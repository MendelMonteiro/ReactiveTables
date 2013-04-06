using System;
using System.Collections.Generic;

namespace ReactiveTables.Framework.Joins
{
    internal class DelegateEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _equalsFunc;
        private readonly Func<T, int> _getHashCodeFunc; 

        public DelegateEqualityComparer(Func<T, T, bool> equalsFunc, Func<T, int> getHashCodeFunc)
        {
            _equalsFunc = equalsFunc;
            _getHashCodeFunc = getHashCodeFunc;
        }

        public bool Equals(T x, T y)
        {
            return _equalsFunc(x, y);
        }

        public int GetHashCode(T obj)
        {
            return _getHashCodeFunc(obj);
        }
    }
}