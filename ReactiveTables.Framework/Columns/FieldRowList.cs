using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveTables.Framework.Columns
{
    internal class FieldRowList<T> : /*IEnumerable<int>,*/ IEnumerable<T>
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

        public void Clear()
        {
            _rowManager.Reset();
            _list.Clear();
        }

        public int Count
        {
            get { return _rowManager.RowCount; }
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