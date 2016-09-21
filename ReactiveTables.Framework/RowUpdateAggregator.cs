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
using Exception = System.Exception;
using System.Reactive.Linq;

namespace ReactiveTables.Framework
{
    internal class RowUpdateAggregator
    {
        private readonly IReactiveTable _leftTable;
        private readonly IReactiveTable _rightTable;
        private readonly IObserver<TableUpdate> _observer;
        private int _leftUpdateCount;
        private int _rightUpdateCount;
        private bool _leftCompleted;
        private bool _rightCompleted;
        private readonly IDisposable _leftToken;
        private readonly IDisposable _rightToken;

        public RowUpdateAggregator(IReactiveTable leftTable, IReactiveTable rightTable, IObserver<TableUpdate> observer)
        {
            _leftTable = leftTable;
            _rightTable = rightTable;
            _observer = observer;

            var leftTableRowUpdateObserver = new TableRowUpdateObserver(this, _leftTable);
            _leftToken = _leftTable.Where(TableUpdate.IsRowUpdate).Subscribe(leftTableRowUpdateObserver);
            var rightTableRowUpdateObserver = new TableRowUpdateObserver(this, _rightTable);
            _rightToken = _rightTable.Where(TableUpdate.IsRowUpdate).Subscribe(rightTableRowUpdateObserver);
        }

        public void Unsubscribe()
        {
            _leftToken.Dispose();
            _rightToken.Dispose();
        }

        private void OnNext(IReactiveTable table, TableUpdate update)
        {
            if (table == _leftTable)
            {
                _leftUpdateCount++;
                if (_leftUpdateCount > _rightUpdateCount)
                    _observer.OnNext(update);
            }
            if (table == _rightTable)
            {
                _rightUpdateCount++;
                if (_rightUpdateCount > _leftUpdateCount)
                    _observer.OnNext(update);
            }
        }

        private void OnError(IReactiveTable table, Exception error)
        {
            _observer.OnError(error);
        }

        private void OnCompleted(IReactiveTable table)
        {
            if (table == _leftTable) _leftCompleted = true;
            if (table == _rightTable) _rightCompleted = true;

            if (_leftCompleted && _rightCompleted) _observer.OnCompleted();
        }

        private class TableRowUpdateObserver : IObserver<TableUpdate>
        {
            private readonly RowUpdateAggregator _rowUpdateAggregator;
            private readonly IReactiveTable _table;

            public TableRowUpdateObserver(RowUpdateAggregator rowUpdateAggregator, IReactiveTable table)
            {
                _rowUpdateAggregator = rowUpdateAggregator;
                _table = table;
            }

            public void OnNext(TableUpdate update)
            {
                _rowUpdateAggregator.OnNext(_table, update);
            }

            public void OnError(Exception error)
            {
                _rowUpdateAggregator.OnError(_table, error);
            }

            public void OnCompleted()
            {
                _rowUpdateAggregator.OnCompleted(_table);
            }
        }
    }
}