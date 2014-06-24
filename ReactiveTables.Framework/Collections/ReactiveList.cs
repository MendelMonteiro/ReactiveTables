using System;
using System.Collections;
using System.Collections.Generic;

namespace ReactiveTables.Framework.Collections
{
    /// <summary>
    /// A list class optimised for adds, removeAt and minimal GC (no inserts, no removes)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class ReactiveList<T> : IList<T>
    {
        private T[] _items;
        private int _size;
        private readonly int _growthFactor = 4;
        private readonly SortedList<int, int> _deletedIndices = null;
        private readonly int _startingSize = 8;

        public ReactiveList()
        {
            
        }

        public ReactiveList(int growthFactor, int startingSize)
        {
            _growthFactor = growthFactor;
            _startingSize = startingSize;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new ReactiveListEnumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T item)
        {
            if (_items == null) _items = new T[_startingSize];

            if (_deletedIndices == null || _deletedIndices.Count == 0)
            {
                if (_size < _items.Length)
                {
                    _items[_size] = item;
                }
                else
                {
                    int nextIndex = ResizeArray();
                    _items[nextIndex] = item;
                }
            }
            ++_size;
        }

        private int ResizeArray()
        {
            T[] newArray = new T[_items.Length * _growthFactor];

            // Figure out segments to copy and copy each segment to new array so as to be contiguous
            int oldIndex = 0, newIndex = 0;
            if (_deletedIndices != null)
            {
                foreach (var deletedIndex in _deletedIndices)
                {
                    int segmentLength = deletedIndex.Key - oldIndex;
                    if (segmentLength > 0)
                    {
                        Array.Copy(_items, oldIndex, newArray, newIndex, segmentLength);
                        oldIndex += segmentLength;
                        newIndex += segmentLength;
                    }
                }
            }

            if (oldIndex < _items.Length)
            {
                var segmentLength = _items.Length - oldIndex;
                Array.Copy(_items, oldIndex, newArray, newIndex, segmentLength);
            }

            if (_deletedIndices != null) _deletedIndices.Clear();

            _items = newArray;
            return 1;
        }

        public void Clear()
        {
            throw new System.NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(T item)
        {
            throw new System.NotImplementedException();
        }

        public int Count { get { return _size; } }
        public bool IsReadOnly { get; private set; }
        public int IndexOf(T item)
        {
            throw new System.NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new System.NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            _deletedIndices.Add(index, index);
            _items[index] = default(T);
        }

        public T this[int index]
        {
            get
            {
                var internalIndex = GetInternalIndex(index);
                return _items[internalIndex];
            }
            set
            {
                var internalIndex = GetInternalIndex(index);
                _items[internalIndex] = value;
            }
        }

        private int GetInternalIndex(int index)
        {
            if (_deletedIndices == null || _deletedIndices.Count == 0) return index;

            int deletedPos = 0;
            int internalIndex = _deletedIndices.Keys[deletedPos++];
            int deletedCount = 0;
            while (internalIndex < index)
            {
                deletedCount++;
                internalIndex = _deletedIndices.Keys[deletedPos++];
            }

            return index - deletedCount;
        }

        struct ReactiveListEnumerator : IEnumerator<T>
        {
            public ReactiveListEnumerator(ReactiveList<T> reactiveList)
            {
                throw new System.NotImplementedException();
            }

            public void Dispose()
            {
                throw new System.NotImplementedException();
            }

            public bool MoveNext()
            {
                throw new System.NotImplementedException();
            }

            public void Reset()
            {
                throw new System.NotImplementedException();
            }

            public T Current { get; private set; }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }
}