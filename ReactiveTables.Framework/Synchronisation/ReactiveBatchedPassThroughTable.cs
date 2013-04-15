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
// along with Foobar.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Threading;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Marshalling;

namespace ReactiveTables.Framework.Synchronisation
{
    public class ReactiveBatchedPassThroughTable: IWritableReactiveTable
    {
        struct BatchedColumnUpdate<T>
        {
            public string ColumnId;
            public T Value;
            public int RowIndex;
        }

        private readonly Queue<RowUpdate> _rowUpdatesAdd = new Queue<RowUpdate>();
        private readonly Queue<RowUpdate> _rowUpdatesDelete = new Queue<RowUpdate>();
        private readonly FieldRowManager _rowManager = new FieldRowManager();
        private readonly ReactiveTable _targetTable;
        private readonly IThreadMarshaller _marshaller;
        private readonly Timer _timer;
        private readonly object _shared = new object();

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

        public ReactiveBatchedPassThroughTable(ReactiveTable table, IThreadMarshaller marshaller, TimeSpan delay)
        {
            _targetTable = table;
            _marshaller = marshaller;
            _timer = new Timer(SynchroniseChanges, null, delay, delay);
        }

        private void SynchroniseChanges(object state)
        {
            lock (_shared)
            {
                while (_rowUpdatesAdd.Count > 0)
                {
                    var add = _rowUpdatesAdd.Dequeue();
                    _targetTable.AddRow();
                }

                foreach (var updater in _updaters)
                {
                    updater.Value.SetValues(_targetTable);
                }

                while (_rowUpdatesDelete.Count > 0)
                {
                    var delete = _rowUpdatesDelete.Dequeue();
                    _targetTable.DeleteRow(delete.RowIndex);
                }
            }
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

        private IReactiveColumn<T> GetColumn<T>(string columnId)
        {
            throw new NotImplementedException();
        }

        public IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner)
        {
            throw new NotImplementedException();
        }

        interface IUpdater
        {
            void SetValues(ReactiveTable targetTable);
        }

        class Updater<T> : IUpdater
        {
            readonly Queue<BatchedColumnUpdate<T>> _updates = new Queue<BatchedColumnUpdate<T>>();
             public void Add(BatchedColumnUpdate<T> update)
             {
                 _updates.Enqueue(update);
             }

            public void SetValues(ReactiveTable targetTable)
            {
                while (_updates.Count > 0)
                {
                    var update = _updates.Dequeue();
                    targetTable.SetValue(update.ColumnId, update.RowIndex, update.Value);
                }
            }
        }

        readonly Dictionary<Type, IUpdater> _updaters = new Dictionary<Type, IUpdater>();

        public void SetValue<T>(string columnId, int rowIndex, T value)
        {
            BatchedColumnUpdate<T> update = new BatchedColumnUpdate<T> {ColumnId = columnId, Value = value, RowIndex = rowIndex};
            lock (_shared)
            {
                Updater<T> updater;
                Type type = typeof (T);
                if (!_updaters.ContainsKey(type))
                {
                    updater = new Updater<T>();
                    _updaters.Add(type, updater);
                }
                else
                {
                    updater = (Updater<T>) _updaters[type];
                }
                updater.Add(update);
//                _columnUpdates.Enqueue(update);
            }
//            _marshaller.Dispatch(() => _targetTable.SetValue(columnId, rowIndex, value));
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
                _rowUpdatesAdd.Enqueue(update);
            }
//            _marshaller.Dispatch(() => _targetTable.AddRow());
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
//            _marshaller.Dispatch(() => _targetTable.DeleteRow(rowIndex));
        }
    }
}