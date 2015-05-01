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
using System.Reactive;
using System.Reactive.Subjects;
using ReactiveTables.Framework.Aggregate.Operations;
using ReactiveTables.Framework.Collections;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Aggregate
{
    /// <summary>
    /// A table which shows an aggregated view of another source table
    /// TODO: add allocation tests and remove allocations
    /// </summary>
    public class AggregatedTable : ReactiveTableBase, IDisposable
    {
        private readonly IReactiveTable _sourceTable;

        private readonly ColumnList _allColumns = new ColumnList(); 
        
        /// <summary>
        /// Columns the source table is grouped by
        /// </summary>
        private readonly Dictionary<string, IReactiveColumn> _groupColumns = new Dictionary<string, IReactiveColumn>();

        /// <summary>
        /// A way of getting the hashcode for each group column
        /// </summary>
        private readonly List<IHashcodeAccessor> _keyColumns = new List<IHashcodeAccessor>();

        /// <summary>
        /// The source rows grouped by <see cref="GroupByKey"/>
        /// </summary>
        private readonly IndexedDictionary<GroupByKey, List<int>> _groupedRows = new IndexedDictionary<GroupByKey, List<int>>();

        /// <summary>
        /// A map of source rows to the matching <see cref="GroupByKey"/>
        /// </summary>
        private readonly Dictionary<int, GroupByKey> _sourceRowsToKeys = new Dictionary<int, GroupByKey>();

        /// <summary>
        /// A map of each key to its external index
        /// </summary>
        private readonly Dictionary<GroupByKey, int> _keyPositions = new Dictionary<GroupByKey, int>();

        /// <summary>
        /// Columns added directly to this table
        /// </summary>
        private readonly Dictionary<string, IReactiveColumn> _localColumns = new Dictionary<string, IReactiveColumn>();

        private readonly List<IAggregateColumn> _aggregateColumns = new List<IAggregateColumn>();
        private readonly IDisposable _token;
        private readonly Subject<TableUpdate> _updates = new Subject<TableUpdate>();

        /// <summary>
        /// Aggregate the provided source table.
        /// </summary>
        /// <param name="sourceTable"></param>
        public AggregatedTable(IReactiveTable sourceTable)
        {
            _sourceTable = sourceTable;
            _token = sourceTable.Subscribe(OnSourceValue);
        }

        /// <summary>
        /// Use this when aggregating over tables that have existing data
        /// </summary>
        public void FinishInitialisation()
        {
            _sourceTable.ReplayRows(Observer.Create<TableUpdate>(OnSourceValue));
        }

        public override int RowCount
        {
            get { return _groupedRows.Count; }
        }

        public override IReadOnlyList<IReactiveColumn> Columns { get { return _allColumns.Columns; } }

        /// <summary>
        /// Group the source table by the given column
        /// </summary>
        /// <param name="columnId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public IReactiveColumn GroupBy<T>(string columnId)
        {
            var column = (IReactiveColumn<T>) _sourceTable.GetColumnByName(columnId);
            if (_groupColumns.ContainsKey(column.ColumnId))
            {
                throw new ArgumentException(string.Format("Column {0} is already in group by statement", column.ColumnId),
                                            "columnId");
            }

            var keyColumn = new HashcodeAccessor<T>(column);
            _keyColumns.Add(keyColumn);
            _groupColumns.Add(columnId, column);
            _allColumns.AddColumn(column);

            return column;
        }

        private void OnSourceValue(TableUpdate tableUpdate)
        {
            var columnUpdated = tableUpdate.Column;
            var sourceIndex = tableUpdate.RowIndex;
            int groupedIndex;
            bool groupChanged = false;
            if (tableUpdate.Action == TableUpdateAction.Add)
            {
                // New source row added
                var key = new GroupByKey(_keyColumns, sourceIndex);
                bool notifyOfAdd;
                groupedIndex = AddItemToGroup(key, sourceIndex, out notifyOfAdd);
                if (notifyOfAdd)
                {
                    NotifyOfGroupAdd(groupedIndex);                    
                }
                _sourceRowsToKeys.Add(sourceIndex, key);
                groupChanged = true;
            }
            else if (tableUpdate.Action == TableUpdateAction.Delete)
            {
                // Source row deleted
                var key = _sourceRowsToKeys[sourceIndex];
                var group = _groupedRows[key];
                groupedIndex = _keyPositions[key];

                if (RemoveItemFromGroup(@group, sourceIndex, key, groupedIndex))
                {
                    NotifyOfGroupDelete(groupedIndex);
                }

                _sourceRowsToKeys.Remove(sourceIndex);
                groupChanged = true;
            }
            else if (tableUpdate.Action == TableUpdateAction.Update &&
                     _groupColumns.ContainsKey(columnUpdated.ColumnId))
            {
                // Source row changing group
                var key = _sourceRowsToKeys[sourceIndex];
                // TODO: figure out how to do this without allocations
                var newKey = new GroupByKey(_keyColumns, sourceIndex);

                // Move the rowIndex from the old key to the new key
                var group = _groupedRows[key];
                var oldGroupIndex = _keyPositions[key];
                var notifyOfDelete = RemoveItemFromGroup(@group, sourceIndex, key, oldGroupIndex);
                bool notifyOfAdd;
                groupedIndex = AddItemToGroup(newKey, sourceIndex, out notifyOfAdd);
                NotifyOnGroupChange(notifyOfAdd, notifyOfDelete, oldGroupIndex, groupedIndex);

                // Replace the rowIndex to key mapping
                _sourceRowsToKeys[sourceIndex] = newKey;

                var column = FindKeyColumn(columnUpdated.ColumnId, _keyColumns);
                column.NotifyObserversOnNext(groupedIndex);
                _updates.OnNext(TableUpdate.NewColumnUpdate(groupedIndex, (IReactiveColumn) column));
//                Console.WriteLine("Grouped column updated");
                groupChanged = true;
            }
            else
            {
                var key = _sourceRowsToKeys[sourceIndex];
                groupedIndex = _keyPositions[key];
//                Console.WriteLine("Non grouped column updated");
            }

            // Aggregated column has changed or group changed which forces re-calc.
            foreach (var aggregateColumn in _aggregateColumns)
            {
                if (groupChanged ||
                    tableUpdate.Column.ColumnId == aggregateColumn.SourceColumn.ColumnId)
                {
                    aggregateColumn.ProcessValue(sourceIndex, groupedIndex);
                    _updates.OnNext(TableUpdate.NewColumnUpdate(groupedIndex, aggregateColumn));
                }
            }
        }

        /// <summary>
        /// Limit the number of row updates when the group index does not change.
        /// </summary>
        /// <param name="notifyOfAdd"></param>
        /// <param name="notifyOfDelete"></param>
        /// <param name="oldGroupIndex"></param>
        /// <param name="groupedIndex"></param>
        private void NotifyOnGroupChange(bool notifyOfAdd, bool notifyOfDelete, int oldGroupIndex, int groupedIndex)
        {
            if (notifyOfAdd || notifyOfDelete)
            {
                if (!notifyOfAdd)
                {
                    NotifyOfGroupDelete(oldGroupIndex);
                }
                if (!notifyOfDelete)
                {
                    NotifyOfGroupAdd(groupedIndex);
                }

                if (notifyOfAdd && notifyOfDelete && oldGroupIndex != groupedIndex)
                {
                    NotifyOfGroupDelete(oldGroupIndex);
                    NotifyOfGroupAdd(groupedIndex);
                }
            }
        }

        private int AddItemToGroup(GroupByKey key, int rowIndex, out bool notify)
        {
            notify = false;
            List<int> rowsInGroup;
            int groupedIndex;
            if (_groupedRows.TryGetValue(key, out rowsInGroup))
            {
                groupedIndex = _keyPositions[key];
                rowsInGroup.Add(rowIndex);
            }
            else
            {
                rowsInGroup = new List<int>();
                groupedIndex = _groupedRows.AddWithIndex(key, rowsInGroup);
                _keyPositions.Add(key, groupedIndex);
                rowsInGroup.Add(rowIndex);
                foreach (var aggregateColumn in _aggregateColumns)
                {
                    aggregateColumn.AddField(groupedIndex);
                }

                notify = true;
                // Make sure all the column values are sent too
                foreach (var keyColumn in _keyColumns)
                {
                    _updates.OnNext(TableUpdate.NewColumnUpdate(groupedIndex, (IReactiveColumn) keyColumn));
                }
            }
            return groupedIndex;
        }

        private void NotifyOfGroupAdd(int groupedIndex)
        {
            // Notify of new row appearing
            _updates.OnNext(TableUpdate.NewAddUpdate(groupedIndex));
        }

        private bool RemoveItemFromGroup(List<int> @group, int rowIndex, GroupByKey key, int groupedIndex)
        {
            group.Remove(rowIndex);
            var notifyRequired = RemoveEmptyGroup(group, key, groupedIndex);
            foreach (var aggregateColumn in _aggregateColumns)
            {
                aggregateColumn.RemoveOldValue(rowIndex, groupedIndex);
            }
            return notifyRequired;
        }

        private bool RemoveEmptyGroup(List<int> @group, GroupByKey key, int groupedIndex)
        {
            if (group.Count == 0)
            {
                // Remove the group
                _groupedRows.RemoveAt(groupedIndex);
//                _groupedRows.Remove(key);
                _keyPositions.Remove(key);

                // Remove the field from the aggregate column
                foreach (var aggregateColumn in _aggregateColumns)
                {
                    aggregateColumn.RemoveField(groupedIndex);
                }

                return true;
            }
            return false;
        }

        private void NotifyOfGroupDelete(int groupedIndex)
        {
            // Notify of grouped row being removed
            _updates.OnNext(TableUpdate.NewDeleteUpdate(groupedIndex));
        }

        public override IReactiveColumn AddColumn(IReactiveColumn column, bool shouldSubscribe = true)
        {
            // Handle calculated columns
            _localColumns.Add(column.ColumnId, column);
            return column;
        }

/*
        public void AddAggregate<TIn, TOut>(IReactiveColumn<TIn> sourceColumn, string columnId, Func<TIn, TOut, bool, TOut> accumulator)
        {
            _aggregateColumns.Add(new AggregateColumn<TIn, TOut>(sourceColumn, columnId, accumulator));
        }
*/

        public void AddAggregate<TIn, TOut>(IReactiveColumn<TIn> sourceColumn, string columnId, Func<IAccumulator<TIn, TOut>> accumulator)
        {
            var column = new AggregateColumn<TIn, TOut>(sourceColumn, columnId, accumulator);
            _aggregateColumns.Add(column);
            _allColumns.AddColumn(column);
        }

        public override T GetValue<T>(string columnId, int rowIndex)
        {
            var sourceColumn = FindKeyColumn(columnId, _keyColumns);
            if (sourceColumn != null)
            {
                IReactiveColumn<T> column = (IReactiveColumn<T>) sourceColumn;
                var sourceRowIndex = GetSourceRowIndex(rowIndex);
                return column.GetValue(sourceRowIndex);
            }
            IReactiveColumn localColumn;
            if (_localColumns.TryGetValue(columnId, out localColumn))
            {
                IReactiveColumn<T> localTyped = (IReactiveColumn<T>) localColumn;
                var sourceRowIndex = GetSourceRowIndex(rowIndex);
                return localTyped.GetValue(sourceRowIndex);
            }
            var aggregateColumn = FindAggregateColumn(columnId, _aggregateColumns);
            if (aggregateColumn != null)
            {
                var aggregateTyped = (IReactiveColumn<T>) aggregateColumn;
                return aggregateTyped.GetValue(rowIndex);
            }

            throw new ArgumentException(string.Format("Column {0} does not existing in the table", columnId), "columnId");
        }

        /// <summary>
        /// Avoid using a closure and thus an allocation
        /// </summary>
        /// <param name="columnId"></param>
        /// <param name="aggregateColumns"></param>
        /// <returns></returns>
        private static IAggregateColumn FindAggregateColumn(string columnId, IEnumerable<IAggregateColumn> aggregateColumns)
        {
            foreach (var column in aggregateColumns)
            {
                if (column.ColumnId == columnId)
                    return column;
            }
            return null;
        }

        /// <summary>
        /// Avoid using a closure and thus an allocation
        /// </summary>
        /// <param name="columnId"></param>
        /// <param name="keyColumns"></param>
        /// <returns></returns>
        private static IHashcodeAccessor FindKeyColumn(string columnId, IEnumerable<IHashcodeAccessor> keyColumns)
        {
            foreach (var column in keyColumns)
            {
                if (column.ColumnId == columnId)
                    return column;
            }
            return null;
        }

        public override object GetValue(string columnId, int rowIndex)
        {
            var keyColumn = FindKeyColumn(columnId, _keyColumns);
            if (keyColumn != null)
            {
                var sourceRowIndex = GetSourceRowIndex(rowIndex);
                return keyColumn.GetValue(sourceRowIndex);
            }
            IReactiveColumn localColumn;
            if (_localColumns.TryGetValue(columnId, out localColumn))
            {
                var sourceRowIndex = GetSourceRowIndex(rowIndex);
                return localColumn.GetValue(sourceRowIndex);
            }
            var aggregateColumn = FindAggregateColumn(columnId, _aggregateColumns);
            if (aggregateColumn != null)
            {
                return aggregateColumn.GetValue(rowIndex);
            }

            throw new ArgumentException(string.Format("Column {0} does not existing in the table", columnId), "columnId");
        }
        
        private int GetSourceRowIndex(int rowIndex)
        {
            // Doesn't matter which sub row we choose as we know they all have the same value
            var groupedRow = _groupedRows[rowIndex];
            var sourceRowIndex = groupedRow[0];
            return sourceRowIndex;
        }

        public override bool GetColumnByName(string columnId, out IReactiveColumn column)
        {
            return _allColumns.GetColumnByName(columnId, out column);
        }

        public override IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            return _updates.Subscribe(observer);
        }

        public override IReactiveColumn GetColumnByIndex(int index)
        {
            return _allColumns.GetColumnByIndex(index);
        }

        public override void ReplayRows(IObserver<TableUpdate> observer)
        {
            for (int i = 0; i < _groupedRows.Count; i++)
            {
                _updates.OnNext(TableUpdate.NewAddUpdate(i));
            }
        }

        public override int GetRowAt(int position)
        {
            // TODO: use row manager to avoid deletes in _groupedRows
            return position;
        }

        public override int GetPositionOfRow(int rowIndex)
        {
            // TODO: use row manager to avoid deletes in _groupedRows
            return rowIndex;
        }

        public override IReactiveColumn GetColumnByName(string columnId)
        {
            return _allColumns.GetColumnByName(columnId);
        }

        public void Dispose()
        {
            _token.Dispose();
        }
    }
}