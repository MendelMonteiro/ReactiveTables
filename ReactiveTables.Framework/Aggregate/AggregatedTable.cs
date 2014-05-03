using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using ReactiveTables.Framework.Collections;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Aggregate
{
    /// <summary>
    /// A table which shows an aggregated view of another source table
    /// </summary>
    public class AggregatedTable : ReactiveTableBase, IDisposable
    {
        private readonly IReactiveTable _sourceTable;
        /// <summary>
        /// Columns the source table is grouped by
        /// </summary>
        private readonly Dictionary<string, IReactiveColumn> _groupColumns = new Dictionary<string, IReactiveColumn>();
        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, IReactiveColumn> _aggregateColumns = new Dictionary<string, IReactiveColumn>();
        /// <summary>
        /// A way of getting the hashcode for each group column
        /// </summary>
        private readonly List<IHashcodeAccessor> _hashcodeAccessors = new List<IHashcodeAccessor>();
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
        private readonly IDisposable _token;
        private readonly Subject<TableUpdate> _updates = new Subject<TableUpdate>();

        public AggregatedTable(IReactiveTable sourceTable)
        {
            _sourceTable = sourceTable;
            _token = sourceTable.Subscribe(OnSourceValue);
        }

        /// <summary>
        /// Group the source table by the given column
        /// </summary>
        /// <param name="columnId"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public AggregatedTable GroupByColumn<T>(string columnId)
        {
            var column = (IReactiveColumn<T>) _sourceTable.Columns[columnId];
            if (_groupColumns.ContainsKey(column.ColumnId))
            {
                throw new ArgumentException(string.Format("Column {0} is already in group by statement", column.ColumnId),
                                            "columnId");
            }

            _hashcodeAccessors.Add(new HashcodeAccessor<T>(column));
            _groupColumns.Add(columnId, column);
            return this;
        }

        private void OnSourceValue(TableUpdate tableUpdate)
        {
            var sourceIndex = tableUpdate.RowIndex;
            if (tableUpdate.Action == TableUpdateAction.Add)
            {
                // New source row added
                var key = new GroupByKey(_hashcodeAccessors, sourceIndex);
                var groupedIndex = AddItemToGroup(key, sourceIndex);
                _sourceRowsToKeys.Add(sourceIndex, key);
            }
            else if (tableUpdate.Action == TableUpdateAction.Delete)
            {
                // Source row deleted
                var key = _sourceRowsToKeys[sourceIndex];
                var group = _groupedRows[key];
                var groupedIndex = _keyPositions[key];

                RemoveItemFromGroup(@group, sourceIndex, key, groupedIndex);

                _sourceRowsToKeys.Remove(sourceIndex);
            }
            else if (tableUpdate.Action == TableUpdateAction.Update)
            {
                // Source column value changed
                if (!_groupColumns.ContainsKey(tableUpdate.Column.ColumnId)) return;

                var key = _sourceRowsToKeys[sourceIndex];
                // TODO: figure out how to do this without allocations
                var newKey = new GroupByKey(_hashcodeAccessors, sourceIndex);
                    
                // Move the rowIndex from the old key to the new key
                var group = _groupedRows[key];
                var oldGroupIndex = _keyPositions[key];
                RemoveItemFromGroup(@group, sourceIndex, key, oldGroupIndex);
                var newGroupedIndex = AddItemToGroup(newKey, sourceIndex);

                // Replace the rowIndex to key mapping
                _sourceRowsToKeys[sourceIndex] = newKey;

                var column = _hashcodeAccessors.First(accessor => accessor.ColumnId == tableUpdate.Column.ColumnId);
                column.NotifyObserversOnNext(newGroupedIndex);
                _updates.OnNext(TableUpdate.NewColumnUpdate(newGroupedIndex, (IReactiveColumn) column));
            }

            // TODO: Notify of any aggregate calculation columns
        }

        private int AddItemToGroup(GroupByKey key, int rowIndex)
        {
            List<int> rowsInGroup;
            int groupedIndex;
            if (!_groupedRows.TryGetValue(key, out rowsInGroup))
            {
                rowsInGroup = new List<int>();
                groupedIndex = _groupedRows.AddWithIndex(key, rowsInGroup);
                _keyPositions.Add(key, groupedIndex);
                rowsInGroup.Add(rowIndex);

                // Notify of new row appearing
                _updates.OnNext(TableUpdate.NewAddUpdate(groupedIndex));
                // Make sure all the column values are sent too
                foreach (var accessor in _hashcodeAccessors)
                {
                    _updates.OnNext(TableUpdate.NewColumnUpdate(groupedIndex, (IReactiveColumn) accessor));
                }
            }
            else
            {
                groupedIndex = _keyPositions[key];
                rowsInGroup.Add(rowIndex);
            }
            return groupedIndex;
        }

        private void RemoveItemFromGroup(List<int> @group, int rowIndex, GroupByKey key, int groupedIndex)
        {
            group.Remove(rowIndex);
            RemoveEmptyGroup(group, key, groupedIndex);
        }

        private void RemoveEmptyGroup(List<int> @group, GroupByKey key, int groupedIndex)
        {
            if (group.Count == 0)
            {
                _groupedRows.Remove(key);
                _keyPositions.Remove(key);

                // Notify of grouped row being removed
                _updates.OnNext(TableUpdate.NewDeleteUpdate(groupedIndex));
            }
        }

        public void AddAggregatedColumn<TOutput, TInput>(string columnId, string sourceColumnId,
                                                         Func<TOutput, IEnumerable<TInput>> evaluator)
        {
            var col = new ReactiveColumn<TOutput>(columnId);
            _aggregateColumns.Add(sourceColumnId, col);
        }
        
        public override IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            return _updates.Subscribe(observer);
        }

        public override IReactiveColumn AddColumn(IReactiveColumn column)
        {
            // TODO: Handle calculated columns
            throw new NotImplementedException();
        }

        public override T GetValue<T>(string columnId, int rowIndex)
        {
            var hashcodeAccessor = _hashcodeAccessors.First(accessor => accessor.ColumnId == columnId);
            var column = (IReactiveColumn<T>)hashcodeAccessor;
            var sourceRowIndex = GetSourceRowIndex(rowIndex);
            return column.GetValue(sourceRowIndex);
        }

        public override object GetValue(string columnId, int rowIndex)
        {
            var hashcodeAccessor = _hashcodeAccessors.First(accessor => accessor.ColumnId == columnId);
            var sourceRowIndex = GetSourceRowIndex(rowIndex);
            return hashcodeAccessor.GetValue(sourceRowIndex);
        }

        private int GetSourceRowIndex(int rowIndex)
        {
            // Doesn't matter which sub row we choose as we know they all have the same value
            var sourceRowIndex = _groupedRows[rowIndex][0];
            return sourceRowIndex;
        }

        public override int RowCount { get { return _groupedRows.Count; } }
        public override IDictionary<string, IReactiveColumn> Columns { get { return _groupColumns; } }
        public override IReactiveColumn GetColumnByIndex(int index)
        {
            return (IReactiveColumn) _hashcodeAccessors[index];
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

        public void Dispose()
        {
            _token.Dispose();
        }
    }
}