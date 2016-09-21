using System.Collections;
using System.Collections.Generic;
using ReactiveTables.Framework;

namespace ReactiveTables.Demo.Utils
{
    public class ReactiveTableEnumerator : IEnumerator<int>
    {
        private readonly ReactiveTable _table;
        private readonly IEnumerator<int> _rows;

        public ReactiveTableEnumerator(ReactiveTable table)
        {
            _table = table;
            _rows = _table.GetRows().GetEnumerator();
        }

        public bool MoveNext()
        {
            return _rows.MoveNext();
        }

        public void Reset()
        {
            _rows.Reset();
        }

        public int Current { get; private set; }

        object IEnumerator.Current => _rows.Current;

        public void Dispose()
        {
            _rows.Dispose();
        }
    }
}