using System;
using System.Collections.Generic;
using System.Threading;
using ReactiveTables.Framework.Marshalling;

namespace ReactiveTables.Framework
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
            lock (_rowUpdates)
            {
                if (_rowUpdates.Count == 0 && _rowUpdates.Count == 0) return;

                _threadMarshaller.Dispatch(
                    () =>
                        {
                            while (_rowUpdates.Count > 0)
                            {
                                var update = _rowUpdates.Dequeue();
                                if (update.Action == RowUpdate.RowUpdateAction.Add)
                                {
                                    _targetTable.AddRow();
                                }
                                else if (update.Action == RowUpdate.RowUpdateAction.Delete)
                                {
                                    _targetTable.DeleteRow(update.RowIndex);
                                }
                            }

                            while(_columnUpdates.Count > 0)
                            {
                                var update = _columnUpdates.Dequeue();
                                _targetTable.SetValue(update.Column.ColumnId, update.RowIndex, update.Column, update.RowIndex);
                            }
                        });
            }
        }

        public void OnNext(RowUpdate rowUpdate)
        {
            lock (_rowUpdates)
            {
                _rowUpdates.Enqueue(rowUpdate);
            }
        }

        public void OnNext(ColumnUpdate columnUpdate)
        {
            lock (_rowUpdates)
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