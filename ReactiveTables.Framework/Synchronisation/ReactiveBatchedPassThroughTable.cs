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
using System.Diagnostics;
using System.Linq;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Filters;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Utils;

namespace ReactiveTables.Framework.Synchronisation
{
    /// <summary>
    /// Buffers access to a table by batching all changes (adds, updates and deletes) and replays them 
    /// according to the delay specified in the constructor.
    /// This class is thread safe and can be written to by multiple threads.
    /// </summary>
    public class ReactiveBatchedPassThroughTable: IWritableReactiveTable, IDisposable
    {
        private readonly Queue<TableUpdate> _rowUpdatesAdd = new Queue<TableUpdate>();
        private readonly Queue<TableUpdate> _rowUpdatesDelete = new Queue<TableUpdate>();
        private readonly Dictionary<Type, ITableColumnUpdater> _columnUpdaters = new Dictionary<Type, ITableColumnUpdater>();
        private readonly FieldRowManager _rowManager = new FieldRowManager();
        private readonly IWritableReactiveTable _targetTable;
        private readonly IThreadMarshaller _marshaller;
        private readonly object _shared = new object();
        private readonly System.Timers.Timer _timer;
        private readonly bool _onlyKeepLastValue;
        
        /// <summary>
        /// Not implemented as the table is read only
        /// </summary>
        /// <param name="columnId"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public object GetValue(string columnId, int rowIndex)
        {
            throw new NotImplementedException();
        }

        public int RowCount
        {
            get { return _rowManager.RowCount; }
        }

        public IDictionary<string, IReactiveColumn> Columns
        {
            get { return _targetTable.Columns; }
        }

        public IReactiveColumn GetColumnByIndex(int index)
        {
            return _targetTable.GetColumnByIndex(index);
        }

        /// <summary>
        /// Not implemented as the table is write only
        /// </summary>
        public PropertyChangedNotifier ChangeNotifier
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Create batched pass through table
        /// </summary>
        /// <param name="targetTable">The table to write to</param>
        /// <param name="marshaller">The thread marshaller</param>
        /// <param name="onlyKeepLastValue">Whether to only keep the last value for a column/cell position</param>
        public ReactiveBatchedPassThroughTable(IWritableReactiveTable targetTable, IThreadMarshaller marshaller, bool onlyKeepLastValue = false)
        {
            _targetTable = targetTable;
            _marshaller = marshaller;
            _onlyKeepLastValue = onlyKeepLastValue;
        }

        /// <summary>
        /// Create batched pass through table - uses a timer
        /// </summary>
        /// <param name="targetTable">The table to write to</param>
        /// <param name="marshaller">The thread marshaller</param>
        /// <param name="delay">The frequency with which we should update the target table</param>
        /// <param name="onlyKeepLastValue">Whether to only keep the last value for a column/cell position</param>
        public ReactiveBatchedPassThroughTable(IWritableReactiveTable targetTable, IThreadMarshaller marshaller, TimeSpan delay, bool onlyKeepLastValue = false)
            :this(targetTable, marshaller, onlyKeepLastValue)
        {
            _timer = new System.Timers.Timer(delay.TotalMilliseconds);
            _timer.Elapsed += (sender, args) => SynchroniseChanges();
            _timer.AutoReset = false;
            _timer.Start();
        }

        /// <summary>
        /// For perf tests
        /// </summary>
        /// <returns></returns>
        public int GetRowUpdateCount()
        {
            lock (_shared)
            {
                return _rowUpdatesAdd.Count;
            }
        }

        /// <summary>
        /// Called when the timer ticks
        /// </summary>
        public void SynchroniseChanges()
        {
            // Make copies to control exactly when we lock
            List<TableUpdate> rowUpdatesAdd = null, rowUpdatesDelete = null;
            List<ITableColumnUpdater> colUpdaters;
            lock (_shared)
            {
                if (_rowUpdatesAdd.Count > 0) rowUpdatesAdd = _rowUpdatesAdd.DequeueAllToList();
                if (_rowUpdatesDelete.Count > 0) rowUpdatesDelete = _rowUpdatesDelete.DequeueAllToList();

                // Create a cloned list so we don't modify the main has list from multiple threads
                // TODO: need to figure out a way to avoid clone as we're adding GC pressure.
                colUpdaters = (from u in _columnUpdaters.Values
                              where u.UpdateCount > 0
                              select u.Clone()).ToList();

                // Clear all the updates in the original version to indicate that there are no updates pending.
                foreach (var tableColumnUpdater in _columnUpdaters.Values)
                {
                    tableColumnUpdater.Clear();
                }
            }

            if (rowUpdatesAdd == null && rowUpdatesDelete == null && colUpdaters.Count == 0)
            {
                if (_timer != null) _timer.Enabled = true;
                return;
            }

            // Don't make dispatch granular so that we don't generate as many messages on the pump
            _marshaller.Dispatch(() => CopyChanges(rowUpdatesAdd, colUpdaters, rowUpdatesDelete));
        }

        /// <summary>
        /// Called on the target table thread - copies adds/updates/deletes
        /// </summary>
        /// <param name="rowUpdatesAdd"></param>
        /// <param name="colUpdaters"></param>
        /// <param name="rowUpdatesDelete"></param>
        private void CopyChanges(List<TableUpdate> rowUpdatesAdd, List<ITableColumnUpdater> colUpdaters, List<TableUpdate> rowUpdatesDelete)
        {
            try
            {
                // Copy the adds
                if (rowUpdatesAdd != null)
                {
                    for (int i = 0; i < rowUpdatesAdd.Count; i++)
                    {
                        _targetTable.AddRow();
                    }
                }

                // Copy the updates
                foreach (var updater in colUpdaters)
                {
                    updater.SetValues(_targetTable);
                }

                // Copy the deletes
                if (rowUpdatesDelete != null)
                {
                    for (int i = 0; i < rowUpdatesDelete.Count; i++)
                    {
                        _targetTable.DeleteRow(rowUpdatesDelete[i].RowIndex);
                    }
                }
            }
            finally
            {
                if (_timer != null) _timer.Enabled = true;
            }
        }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            throw new NotImplementedException();
        }

        public IReactiveColumn AddColumn(IReactiveColumn column)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not implemented as the table is write only
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnId"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        public T GetValue<T>(string columnId, int rowIndex)
        {
            throw new NotImplementedException();
        }

        public IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner)
        {
            throw new NotImplementedException();
        }

        public IReactiveTable Filter(IReactivePredicate predicate)
        {
            throw new NotImplementedException();
        }

        public void ReplayRows(IObserver<TableUpdate> observer)
        {
            throw new NotImplementedException();
        }

        public int GetRowAt(int position)
        {
            return _rowManager.GetRowAt(position);
        }

        public int GetPositionOfRow(int rowIndex)
        {
            return _rowManager.GetPositionOfRow(rowIndex);
        }

        public void SetValue<T>(string columnId, int rowIndex, T value)
        {
            BatchedColumnUpdate<T> update = new BatchedColumnUpdate<T> {ColumnId = columnId, Value = value, RowIndex = rowIndex};
            lock (_shared)
            {
                ITypedTableColumnUpdater<T> updater;
                Type type = typeof (T);
                if (!_columnUpdaters.ContainsKey(type))
                {
                    updater = _onlyKeepLastValue ? CreateNormalColumnUpdaterLastValue<T>() : CreateNormalColumnUpdater<T>();
                    _columnUpdaters.Add(type, updater);
                }
                else
                {
                    updater = (ITypedTableColumnUpdater<T>) _columnUpdaters[type];
                }
                updater.Add(update);
            }
        }

        private static ITypedTableColumnUpdater<T> CreateNormalColumnUpdater<T>()
        {
            return new TableColumnUpdater<T>();
        }

        private static ITypedTableColumnUpdater<T> CreateNormalColumnUpdaterLastValue<T>()
        {
            return new TableColumnUpdaterLastValue<T>();
        }

        public void SetValue(string columnId, int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex)
        {
            throw new NotImplementedException();
        }

        public int AddRow()
        {
            int rowIndex;
            lock (_shared)
            {
                rowIndex = _rowManager.AddRow();
                TableUpdate update = new TableUpdate(TableUpdateAction.Add, rowIndex);
                _rowUpdatesAdd.Enqueue(update);
            }
            //Debug.WriteLine("Added row {0} to batched passthrough", rowIndex);
            return rowIndex;
        }

        public void DeleteRow(int rowIndex)
        {
            lock (_shared)
            {
                _rowManager.DeleteRow(rowIndex);
                TableUpdate update = new TableUpdate(TableUpdateAction.Delete, rowIndex);
                _rowUpdatesDelete.Enqueue(update);
            }
        }

        public void Dispose()
        {
            if (_timer != null) _timer.Dispose();
        }
    }

    struct BatchedColumnUpdate<T>
    {
        public string ColumnId;
        public T Value;
        public int RowIndex;
    }

    interface ITableColumnUpdater
    {
        void SetValues(IWritableReactiveTable targetTable);
        ITableColumnUpdater Clone();
        int UpdateCount { get; }
        void Clear();
    }

    /// <summary>
    /// Keeps all events in order
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class TableColumnUpdater<T> : ITableColumnUpdater, ITypedTableColumnUpdater<T>
    {
        private readonly Queue<BatchedColumnUpdate<T>> _updates;

        public TableColumnUpdater()
        {
            _updates = new Queue<BatchedColumnUpdate<T>>();
        }

        private TableColumnUpdater(IEnumerable<BatchedColumnUpdate<T>> updates)
        {
            _updates = new Queue<BatchedColumnUpdate<T>>(updates);
        }

        public void Add(BatchedColumnUpdate<T> update)
        {
            _updates.Enqueue(update);
        }

        public void SetValues(IWritableReactiveTable targetTable)
        {
            while (_updates.Count > 0)
            {
                var update = _updates.Dequeue();
                targetTable.SetValue(update.ColumnId, update.RowIndex, update.Value);
            }
        }

        public void Clear()
        {
            _updates.Clear();
        }

        public ITableColumnUpdater Clone()
        {
            return new TableColumnUpdater<T>(_updates);
        }

        public int UpdateCount { get { return _updates.Count; } }
    }

    internal interface ITypedTableColumnUpdater<T> : ITableColumnUpdater
    {
        void Add(BatchedColumnUpdate<T> update);
    }

    /// <summary>
    /// Only keeps last event per row
    /// </summary>
    /// <typeparam name="T"></typeparam>
    class TableColumnUpdaterLastValue<T> : ITableColumnUpdater, ITypedTableColumnUpdater<T>
    {
        private readonly Dictionary<int, BatchedColumnUpdate<T>> _updatesByRow = new Dictionary<int, BatchedColumnUpdate<T>>();

        public TableColumnUpdaterLastValue(){}

        private TableColumnUpdaterLastValue(IDictionary<int, BatchedColumnUpdate<T>> updates)
        {
            _updatesByRow = new Dictionary<int, BatchedColumnUpdate<T>>(updates);
        }

        public void Add(BatchedColumnUpdate<T> update)
        {
            _updatesByRow[update.RowIndex] = update;
        }

        public void SetValues(IWritableReactiveTable targetTable)
        {
            foreach (var update in _updatesByRow.Values)
            {
                targetTable.SetValue(update.ColumnId, update.RowIndex, update.Value);
            }
            Clear();
        }

        public void Clear()
        {
            _updatesByRow.Clear();
        }

        public ITableColumnUpdater Clone()
        {
            return new TableColumnUpdaterLastValue<T>(_updatesByRow);
        }

        public int UpdateCount { get { return _updatesByRow.Count; } }
    }
}