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
using System.Threading;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Utils;

namespace ReactiveTables.Framework.Synchronisation
{
    public class ReactiveBatchedPassThroughTable: IWritableReactiveTable, IDisposable
    {
        private readonly Queue<RowUpdate> _rowUpdatesAdd = new Queue<RowUpdate>();
        private readonly Queue<RowUpdate> _rowUpdatesDelete = new Queue<RowUpdate>();
        private readonly Dictionary<Type, ITableColumnUpdater> _columnUpdaters = new Dictionary<Type, ITableColumnUpdater>();
        private readonly FieldRowManager _rowManager = new FieldRowManager();
        private readonly IWritableReactiveTable _targetTable;
        private readonly IThreadMarshaller _marshaller;
        private readonly Timer _timer;
        private readonly object _shared = new object();
        private readonly System.Timers.Timer _timer1;

        public int RowCount
        {
            get { return _rowManager.RowCount; }
        }

        public Dictionary<string, IReactiveColumn> Columns
        {
            get { throw new NotImplementedException(); }
        }

        public PropertyChangedNotifier ChangeNotifier
        {
            get { throw new NotImplementedException(); }
        }

        public Queue<RowUpdate> RowUpdatesAdd
        {
            get
            {
                lock (_shared)
                {
                    return _rowUpdatesAdd;
                }
            }
        }

        public ReactiveBatchedPassThroughTable(IWritableReactiveTable targetTable, IThreadMarshaller marshaller, TimeSpan delay)
        {
            _targetTable = targetTable;
            _marshaller = marshaller;
//            _timer = new Timer(SynchroniseChanges, null, delay, delay);
            _timer1 = new System.Timers.Timer(delay.TotalMilliseconds);
            _timer1.Elapsed += (sender, args) => SynchroniseChanges(null);
            _timer1.AutoReset = false;
            _timer1.Start();
        }

        private void SynchroniseChanges(object state)
        {
            // Make copies to control exactly when we lock
            List<RowUpdate> rowUpdatesAdd = null, rowUpdatesDelete = null;
            List<ITableColumnUpdater> colUpdaters;
            lock (_shared)
            {
                if (RowUpdatesAdd.Count > 0) rowUpdatesAdd = RowUpdatesAdd.DequeueAllToList();
                if (_rowUpdatesDelete.Count > 0) rowUpdatesDelete = _rowUpdatesDelete.DequeueAllToList();

                colUpdaters = new List<ITableColumnUpdater>(from u in _columnUpdaters.Values where u.UpdateCount > 0 select u.Clone());
                foreach (var tableColumnUpdater in _columnUpdaters.Values)
                {
                    tableColumnUpdater.Clear();
                }
            }

            if (rowUpdatesAdd == null && rowUpdatesDelete == null && colUpdaters.Count == 0)
            {
                _timer1.Enabled = true;
                return;
            }

            // Don't make dispatch granular so that we don't incur as many context switches.
            _marshaller.Dispatch(
                () =>
                    {
                        try
                        {
                            if (rowUpdatesAdd != null)
                            {
                                for (int i = 0; i < rowUpdatesAdd.Count; i++)
                                {
                                    _targetTable.AddRow();
                                }
                            }

                            foreach (var updater in colUpdaters)
                            {
                                updater.SetValues(_targetTable);
                            }

                            if (rowUpdatesDelete != null)
                            {
                                foreach (var delete in rowUpdatesDelete)
                                {
                                    _targetTable.DeleteRow(delete.RowIndex);
                                }
                            }
                        }
                        finally
                        {
                            _timer1.Enabled = true;
                        }
                    });
        }

        public IDisposable Subscribe(IObserver<RowUpdate> observer)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(IObserver<RowUpdate> observer)
        {
            throw new NotImplementedException();
        }

        public IDisposable Subscribe(IObserver<ColumnUpdate> observer)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(IObserver<ColumnUpdate> observer)
        {
            throw new NotImplementedException();
        }

        public void AddColumn(IReactiveColumn column)
        {
            throw new NotImplementedException();
        }

        public T GetValue<T>(string columnId, int rowIndex)
        {
            throw new NotImplementedException();
        }

        public IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner)
        {
            throw new NotImplementedException();
        }


        public void SetValue<T>(string columnId, int rowIndex, T value)
        {
            BatchedColumnUpdate<T> update = new BatchedColumnUpdate<T> {ColumnId = columnId, Value = value, RowIndex = rowIndex};
            lock (_shared)
            {
                TableColumnUpdater<T> updater;
                Type type = typeof (T);
                if (!_columnUpdaters.ContainsKey(type))
                {
                    updater = new TableColumnUpdater<T>();
                    _columnUpdaters.Add(type, updater);
                }
                else
                {
                    updater = (TableColumnUpdater<T>) _columnUpdaters[type];
                }
                updater.Add(update);
            }
        }

        public void SetValue(string columnId, int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex)
        {
            throw new NotImplementedException();
        }

        public int AddRow()
        {
            int rowIndex = _rowManager.AddRow();
            RowUpdate update = new RowUpdate(rowIndex, RowUpdate.RowUpdateAction.Add);
            lock (_shared)
            {
                RowUpdatesAdd.Enqueue(update);
            }
            return rowIndex;
        }

        public void DeleteRow(int rowIndex)
        {
            _rowManager.DeleteRow(rowIndex);
            RowUpdate update = new RowUpdate(rowIndex, RowUpdate.RowUpdateAction.Delete);
            lock (_shared)
            {
                _rowUpdatesDelete.Enqueue(update);
            }
        }

        public void Dispose()
        {
            if (_timer != null) _timer.Dispose();
            if (_timer1 != null) _timer1.Dispose();
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

    class TableColumnUpdater<T> : ITableColumnUpdater
    {
        readonly Queue<BatchedColumnUpdate<T>> _updates;

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
}