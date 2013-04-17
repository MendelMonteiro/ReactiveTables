/*This file is part of ReactiveTables.

ReactiveTables is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ReactiveTables is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Threading;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Utils;

namespace ReactiveTables.Framework.Synchronisation
{
    public class BatchedTableSynchroniser : IObserver<RowUpdate>, IObserver<ColumnUpdate>, IDisposable
    {
        private readonly IReactiveTable _sourceTable;
        private readonly IWritableReactiveTable _targetTable;
        private readonly IThreadMarshaller _threadMarshaller;
        private readonly IDisposable _rowSubscription;
        private readonly IDisposable _columnSubscription;
        private readonly Timer _timer;
        private readonly Queue<RowUpdate> _rowUpdates = new Queue<RowUpdate>();
        private readonly Queue<ColumnUpdate> _columnUpdates = new Queue<ColumnUpdate>();
        private readonly object _shared = new object();

        public BatchedTableSynchroniser(ReactiveTable sourceTable, IWritableReactiveTable targetTable,
                                        IThreadMarshaller threadMarshaller, TimeSpan delay)
        {
            _sourceTable = sourceTable;
            _targetTable = targetTable;
            _threadMarshaller = threadMarshaller;
            _timer = new Timer(SynchroniseChanges, null, delay, delay);

            _rowSubscription = _sourceTable.Subscribe((IObserver<RowUpdate>) this);
            _columnSubscription = _sourceTable.Subscribe((IObserver<ColumnUpdate>) this);
        }

        private void SynchroniseChanges(object state)
        {
            // Make copies to control exactly when we lock
            List<RowUpdate> rowUpdates;
            List<ColumnUpdate> colUpdates;
            lock (_shared)
            {
                if (_rowUpdates.Count == 0 && _columnUpdates.Count == 0) return;

                rowUpdates = _rowUpdates.DequeueAllToList();
                colUpdates = _columnUpdates.DequeueAllToList();
            }

            _threadMarshaller.Dispatch(
                () =>
                    {
                        foreach (var update in rowUpdates)
                        {
                            if (update.Action == RowUpdate.RowUpdateAction.Add)
                            {
                                _targetTable.AddRow();
                            }
                            else if (update.Action == RowUpdate.RowUpdateAction.Delete)
                            {
                                _targetTable.DeleteRow(update.RowIndex);
                            }
                        }

                        // BUG: When this line is called the original update.Column may not contain the same state as when the outside method is called.
                        foreach (var update in colUpdates)
                        {
                            _targetTable.SetValue(update.Column.ColumnId, update.RowIndex, update.Column, update.RowIndex);
                        }
                    });
        }

        public void OnNext(RowUpdate rowUpdate)
        {
            lock (_shared)
            {
                _rowUpdates.Enqueue(rowUpdate);
            }
        }

        public void OnNext(ColumnUpdate columnUpdate)
        {
            lock (_shared)
            {
                _columnUpdates.Enqueue(columnUpdate);
            }
        }

        void IObserver<RowUpdate>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        void IObserver<RowUpdate>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        void IObserver<ColumnUpdate>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        void IObserver<ColumnUpdate>.OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _columnSubscription.Dispose();
            _rowSubscription.Dispose();
            _timer.Dispose();
        }
    }
}
