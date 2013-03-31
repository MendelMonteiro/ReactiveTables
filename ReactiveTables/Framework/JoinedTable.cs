using System;
using System.Collections.Generic;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.UI;
using ReactiveTables.Utils;

namespace ReactiveTables.Framework
{
    public class JoinedTable : IReactiveTable
    {
        private readonly IReactiveTable _leftTable;
        private readonly IReactiveTable _rightTable;
        private readonly IReactiveTableJoiner _joiner;
        private RowUpdateAggregator _rowUpdateAggregator;
//        private readonly Dictionary<string, IReactiveColumn> internalColumns = new Dictionary<string, IReactiveColumn>();

        public JoinedTable(IReactiveTable leftTable, IReactiveTable rightTable, IReactiveTableJoiner joiner)
        {
            _leftTable = leftTable;
            _rightTable = rightTable;
            _joiner = joiner;

            Columns = new Dictionary<string, IReactiveColumn>();
            _leftTable.Columns.CopyTo(Columns);
            _rightTable.Columns.CopyTo(Columns);

            // Get the row updates to add to our
//            _leftTable.Subscribe(this);
        }

        public IDisposable Subscribe(IObserver<RowUpdate> observer)
        {
            _rowUpdateAggregator = new RowUpdateAggregator(_leftTable, _rightTable, observer);
            return new SubscriptionToken<JoinedTable, IObserver<RowUpdate>>(this, observer);
        }

        public void Unsubscribe(IObserver<RowUpdate> observer)
        {
            _rowUpdateAggregator.Unsubscribe();
        }

        public IDisposable Subscribe(IObserver<ColumnUpdate> observer)
        {
            _leftTable.Subscribe(observer);
            _rightTable.Subscribe(observer);
            return new SubscriptionToken<JoinedTable, IObserver<ColumnUpdate>>(this, observer);
        }

        public void Unsubscribe(IObserver<ColumnUpdate> observer)
        {
            _leftTable.Unsubscribe(observer);
            _rightTable.Unsubscribe(observer);
        }

        public IReactiveColumn AddColumn(IReactiveColumn column)
        {
            // Add calc'ed columns
            Columns.Add(column.ColumnId, column);
            var joinableCol = column as IReactiveJoinableColumn;
            if (joinableCol != null)
                joinableCol.SetJoiner(_joiner);
                
            return column;
        }

        public T GetValue<T>(string columnId, int rowIndex)
        {
            IReactiveColumn column;
            // Use the joiner for when the column is defined directly on them
            // if the table is a joined table delegate the joining to it.
            if (_leftTable.Columns.TryGetValue(columnId, out column))
            {
                return _leftTable.GetValue<T>(columnId, _joiner.GetRowIndex(column, rowIndex));
            }
            if (_rightTable.Columns.TryGetValue(columnId, out column))
            {
                return _rightTable.GetValue<T>(columnId, _joiner.GetRowIndex(column, rowIndex));
            }
            // Otherwise return calc'ed columns
            return GetColumn<T>(columnId).GetValue(rowIndex).Value;
        }

        public IReactiveColumn<T> GetColumn<T>(string columnId)
        {
            return (IReactiveColumn<T>) Columns[columnId];
        }

        public void RegisterPropertyNotifiedConsumer(IReactivePropertyNotifiedConsumer consumer, int rowIndex)
        {
            _leftTable.RegisterPropertyNotifiedConsumer(consumer, rowIndex);
            _rightTable.RegisterPropertyNotifiedConsumer(consumer, rowIndex);
        }

        public void UnregisterPropertyNotifiedConsumer(IReactivePropertyNotifiedConsumer consumer, int rowIndex)
        {
            _leftTable.UnregisterPropertyNotifiedConsumer(consumer, rowIndex);
            _rightTable.UnregisterPropertyNotifiedConsumer(consumer, rowIndex);
        }

        public int RowCount
        {
            get
            {
                // Delegate to the joiner
                return _joiner.RowCount;
            }
        }

        public Dictionary<string, IReactiveColumn> Columns { get; private set; }

        public IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner)
        {
            return new JoinedTable(this, otherTable, joiner);
        }
    }
}