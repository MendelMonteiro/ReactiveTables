using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Collections
{
    /// <summary>
    /// A list which does not delete from the underlying list instead leaving blank entries
    /// which are subsequently re-used for new adds.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class FieldRowList<T> : IList<T>
    {
        private readonly FieldRowManager _rowManager = new FieldRowManager();
        private readonly List<T> _list = new List<T>();
        private static readonly T DefaultFieldValue = default (T);

        public IEnumerator<T> GetEnumerator()
        {
            return _rowManager.GetRows().Select(i => _list[i]).GetEnumerator();
        }

        /*public IEnumerator<int> GetEnumerator()
        {
            return _rowManager.GetRows().GetEnumerator();
        }*/

        public int Add(T item)
        {
            var index = _rowManager.AddRow();
            if (index < _list.Count)
            {
                _list[index] = item;
            }
            else
            {
                _list.Add(item);
            }
            return index;
        }

        void ICollection<T>.Add(T item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            _rowManager.Reset();
            _list.Clear();
        }

        public bool Contains(T item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var i = 0;
            foreach (var row in _rowManager.GetRows())
            {
                array[i++] = _list[row];
            }
        }

        public bool Remove(T item)
        {
            throw new System.NotImplementedException();
        }

        public int Count => _rowManager.RowCount;

        public bool IsReadOnly => false;

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
            _rowManager.DeleteRow(index);
            _list[index] = DefaultFieldValue;
        }

        public T this[int index]
        {
            get { return _list[index]; }
            set { _list[index] = value; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}