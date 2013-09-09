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
using System.Collections.Generic;
using System.Linq;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Filters;

namespace ReactiveTables.Framework.Joins
{
    /// <summary>
    /// Joins the output of two tables using the given <see cref="IReactiveTableJoiner"/>.
    /// </summary>
    public class JoinedTable : IReactiveTable, IDisposable
    {
        private readonly IReactiveTable _leftTable;
        private readonly IReactiveTable _rightTable;
        private readonly IReactiveTableJoiner _joiner;
        private readonly HashSet<IObserver<TableUpdate>> _calculatedColumnObservers = new HashSet<IObserver<TableUpdate>>();

        private readonly Dictionary<IObserver<TableUpdate>, Tuple<IDisposable, IDisposable>> _tokens =
            new Dictionary<IObserver<TableUpdate>, Tuple<IDisposable, IDisposable>>();

        public JoinedTable(IReactiveTable leftTable, IReactiveTable rightTable, IReactiveTableJoiner joiner)
        {
            _leftTable = leftTable;
            _rightTable = rightTable;
            _joiner = joiner;

            Columns = new Dictionary<string, IReactiveColumn>();
            ChangeNotifier = new PropertyChangedNotifier(this);
            AddBaseTableColumns(leftTable);
            AddBaseTableColumns(rightTable);
            // TODO: need to process all existing values in the tables
        }

        private void AddBaseTableColumns(IReactiveTable table)
        {
            foreach (var column in table.Columns)
            {
                Columns.Add(column.Value.ColumnId, column.Value);
            }
        }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            _joiner.AddObserver(observer);
            _calculatedColumnObservers.Add(observer);

            var subscriptionToken = new SubscriptionToken<JoinedTable, IObserver<TableUpdate>>(this, observer);
//            _tokens.Add(observer, new Tuple<IDisposable, IDisposable>(null, null));
            return subscriptionToken;
        }

        public void Unsubscribe(IObserver<TableUpdate> observer)
        {
            /*var tokens = _tokens[observer];
            if (tokens != null)
            {
                if (tokens.Item1 != null) tokens.Item1.Dispose();
                if (tokens.Item2 != null) tokens.Item2.Dispose();
            }*/

            _calculatedColumnObservers.Remove(observer);
        }

        public void AddColumn(IReactiveColumn column)
        {
            // Add calc'ed columns
            Columns.Add(column.ColumnId, column);
            var joinableCol = column as IReactiveJoinableColumn;
            if (joinableCol != null) joinableCol.SetJoiner(_joiner);

            // Need to subscribe to changes in calculated columns
            column.Subscribe(new ColumnChangePublisher(column, _calculatedColumnObservers));
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
            return GetColumn<T>(columnId).GetValue(rowIndex);
        }

        public object GetValue(string columnId, int rowIndex)
        {
            IReactiveColumn column;
            // Use the joiner for when the column is defined directly on them
            // if the table is a joined table delegate the joining to it.
            if (_leftTable.Columns.TryGetValue(columnId, out column))
            {
                return _leftTable.GetValue(columnId, _joiner.GetRowIndex(column, rowIndex));
            }
            if (_rightTable.Columns.TryGetValue(columnId, out column))
            {
                return _rightTable.GetValue(columnId, _joiner.GetRowIndex(column, rowIndex));
            }
            // Otherwise return calc'ed columns
            return Columns[columnId].GetValue(rowIndex);
        }

        private IReactiveColumn<T> GetColumn<T>(string columnId)
        {
            return (IReactiveColumn<T>) Columns[columnId];
        }

        public int RowCount
        {
            get
            {
                // Delegate to the joiner
                return _joiner.RowCount;
            }
        }

        public IDictionary<string, IReactiveColumn> Columns { get; private set; }

        public IReactiveColumn GetColumnByIndex(int index)
        {
            throw new NotImplementedException();
        }

        public PropertyChangedNotifier ChangeNotifier { get; private set; }

        public IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner)
        {
            return new JoinedTable(this, otherTable, joiner);
        }

        public IReactiveTable Filter(IReactivePredicate predicate)
        {
            return new FilteredTable(this, predicate);
        }

        public void ReplayRows(IObserver<TableUpdate> observer)
        {
            var rowAdds = new List<TableUpdate>(_joiner.RowCount);
            rowAdds.AddRange(_joiner.GetRows().Select(row => new TableUpdate(TableUpdate.TableUpdateAction.Add, row)));

            foreach (var rowAdd in rowAdds)
            {
                observer.OnNext(rowAdd);
            }
        }

        public int GetRowAt(int position)
        {
            return _joiner.GetRowAt(position);
        }

        public int GetPositionOfRow(int rowIndex)
        {
            return _joiner.GetPositionOfRow(rowIndex);
        }

        public void Dispose()
        {
            _joiner.Dispose();
        }
    }
}