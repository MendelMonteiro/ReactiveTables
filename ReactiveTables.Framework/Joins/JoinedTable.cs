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
using System.Reactive.Subjects;
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
        private readonly Subject<TableUpdate> _calculatedColumnSubject = new Subject<TableUpdate>();

        private readonly Dictionary<IObserver<TableUpdate>, Tuple<IDisposable, IDisposable>> _tokens =
            new Dictionary<IObserver<TableUpdate>, Tuple<IDisposable, IDisposable>>();

        private readonly ColumnList _columns = new ColumnList();
        private readonly Lazy<PropertyChangedNotifier> _changeNotifier;

        /// <summary>
        /// Create a new table joining two existing tables
        /// </summary>
        /// <param name="leftTable"></param>
        /// <param name="rightTable"></param>
        /// <param name="joiner"></param>
        public JoinedTable(IReactiveTable leftTable, IReactiveTable rightTable, IReactiveTableJoiner joiner)
        {
            _leftTable = leftTable;
            _rightTable = rightTable;
            _joiner = joiner;

            _joiner.SetObserver(_calculatedColumnSubject);
            _changeNotifier = new Lazy<PropertyChangedNotifier>(() => new PropertyChangedNotifier(this));
            AddBaseTableColumns(leftTable);
            AddBaseTableColumns(rightTable);
            // TODO: need to process all existing values in the tables
        }

        private void AddBaseTableColumns(IReactiveTable table)
        {
            foreach (var column in table.Columns)
            {
                _columns.AddColumn(column);
            }
        }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            return _calculatedColumnSubject.Subscribe(observer);
        }

        public IReactiveColumn AddColumn(IReactiveColumn column, bool shouldSubscribe)
        {
            // Add calc'ed columns
            _columns.AddColumn(column);
            
            var joinableCol = column as IReactiveJoinableColumn;
            if (joinableCol != null) joinableCol.SetJoiner(_joiner);

            // Need to subscribe to changes in calculated columns
            column.Subscribe(_calculatedColumnSubject);
            return column;
        }

        public T GetValue<T>(string columnId, int rowIndex)
        {
            IReactiveColumn column;
            // Use the joiner for when the column is defined directly on them
            // if the table is a joined table delegate the joining to it.
            if (_leftTable.GetColumnByName(columnId, out column))
            {
                return _leftTable.GetValue<T>(columnId, _joiner.GetRowIndex(column, rowIndex));
            }
            if (_rightTable.GetColumnByName(columnId, out column))
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
            if (_leftTable.GetColumnByName(columnId, out column))
            {
                return _leftTable.GetValue(columnId, _joiner.GetRowIndex(column, rowIndex));
            }
            if (_rightTable.GetColumnByName(columnId, out column))
            {
                return _rightTable.GetValue(columnId, _joiner.GetRowIndex(column, rowIndex));
            }
            // Otherwise return calc'ed columns
            return _columns.GetColumnByName(columnId).GetValue(rowIndex);
        }

        private IReactiveColumn<T> GetColumn<T>(string columnId)
        {
            return (IReactiveColumn<T>) _columns.GetColumnByName(columnId);
        }

        public int RowCount => _joiner.RowCount;

        public IReadOnlyList<IReactiveColumn> Columns => _columns.Columns;

        public IReactiveColumn GetColumnByIndex(int index)
        {
            return _columns.GetColumnByIndex(index);
        }

        public PropertyChangedNotifier ChangeNotifier => _changeNotifier.Value;

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
            rowAdds.AddRange(_joiner.GetRows().Select(row => new TableUpdate(TableUpdateAction.Add, row)));

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

        public IReactiveColumn GetColumnByName(string columnId)
        {
            return _columns.GetColumnByName(columnId);
        }

        public bool GetColumnByName(string columnId, out IReactiveColumn column)
        {
            return _columns.GetColumnByName(columnId, out column);
        }

        public void Dispose()
        {
            _joiner.Dispose();
        }
    }
}