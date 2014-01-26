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
using System.Threading;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Utils;
using System.Linq;

namespace ReactiveTables.Framework.Synchronisation
{
    [Obsolete("Use the ReactievBatchedPassThroughTable instead.")]
    public class BatchedTableSynchroniser : IObserver<TableUpdate>, IDisposable
    {
        private readonly IReactiveTable _sourceTable;
        private readonly IWritableReactiveTable _targetTable;
        private readonly IThreadMarshaller _threadMarshaller;
        private readonly IDisposable _subscription;
        private readonly Timer _timer;
        private readonly Queue<TableUpdate> _updates = new Queue<TableUpdate>();
        private readonly object _shared = new object();

        public BatchedTableSynchroniser(ReactiveTable sourceTable, IWritableReactiveTable targetTable,
                                        IThreadMarshaller threadMarshaller, TimeSpan delay)
        {
            _sourceTable = sourceTable;
            _targetTable = targetTable;
            _threadMarshaller = threadMarshaller;
            _timer = new Timer(SynchroniseChanges, null, delay, delay);

            _subscription = _sourceTable.Subscribe(this);
        }

        private void SynchroniseChanges(object state)
        {
            // Make copies to control exactly when we lock
            List<TableUpdate> updates;
            lock (_shared)
            {
                if (_updates.Count == 0) return;

                updates = _updates.DequeueAllToList();
            }

            _threadMarshaller.Dispatch(
                () =>
                    {
                        foreach (var update in updates.Where(TableUpdate.IsRowUpdate))
                        {
                            if (update.Action == TableUpdate.TableUpdateAction.Add)
                            {
                                _targetTable.AddRow();
                            }
                            else if (update.Action == TableUpdate.TableUpdateAction.Delete)
                            {
                                _targetTable.DeleteRow(update.RowIndex);
                            }
                        }

                        // BUG: When this line is called the original update.Column may not contain the same state as when the outside method is called.
                        foreach (var update in updates.Where(TableUpdate.IsColumnUpdate))
                        {
                            _targetTable.SetValue(update.Column.ColumnId, update.RowIndex, update.Column, update.RowIndex);
                        }
                    });
        }

        public void OnNext(TableUpdate rowUpdate)
        {
            lock (_shared)
            {
                _updates.Enqueue(rowUpdate);
            }
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _subscription.Dispose();
            _timer.Dispose();
        }
    }
}