using System;

namespace ReactiveTables.Framework
{
    class RowUpdateAggregator
    {
        private readonly IReactiveTable _leftTable;
        private readonly IReactiveTable _rightTable;
        private readonly IObserver<RowUpdate> _observer;
        private int _leftUpdateCount = 0;
        private int _rightUpdateCount = 0;
        private bool _leftCompleted = false;
        private bool _rightCompleted = false;
        private readonly TableRowUpdateObserver _leftTableRowUpdateObserver;
        private readonly TableRowUpdateObserver _rightTableRowUpdateObserver;

        public RowUpdateAggregator(IReactiveTable leftTable, IReactiveTable rightTable, IObserver<RowUpdate> observer)
        {
            _leftTable = leftTable;
            _rightTable = rightTable;
            _observer = observer;

            _leftTableRowUpdateObserver = new TableRowUpdateObserver(this, _leftTable);
            _leftTable.Subscribe(_leftTableRowUpdateObserver);
            _rightTableRowUpdateObserver = new TableRowUpdateObserver(this, _rightTable);
            _rightTable.Subscribe(_rightTableRowUpdateObserver);
        }

        public void Unsubscribe()
        {
            _leftTable.Unsubscribe(_leftTableRowUpdateObserver);
            _rightTable.Unsubscribe(_rightTableRowUpdateObserver);
        }

        private void OnNext(IReactiveTable table, RowUpdate update)
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

        private class TableRowUpdateObserver : IObserver<RowUpdate>
        {
            private readonly RowUpdateAggregator _rowUpdateAggregator;
            private readonly IReactiveTable _table;

            public TableRowUpdateObserver(RowUpdateAggregator rowUpdateAggregator, IReactiveTable table)
            {
                _rowUpdateAggregator = rowUpdateAggregator;
                _table = table;
            }

            public void OnNext(RowUpdate update)
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