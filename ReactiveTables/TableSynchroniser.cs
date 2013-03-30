using System;
using System.Diagnostics;
using ReactiveTables.Framework;

namespace ReactiveTables
{
    internal class TableSynchroniser : IObserver<int>, IObserver<ColumnUpdate>, IDisposable
    {
        private readonly IReactiveTable _sourceTable;
        private readonly IWritableReactiveTable _targetTable;
        private readonly IThreadMarshaller _threadMarshaller;
        private readonly IDisposable _rowSubscription;
        private readonly IDisposable _columnSubscription;

        public TableSynchroniser(ReactiveTable sourceTable, IWritableReactiveTable targetTable, IThreadMarshaller threadMarshaller)
        {
            _sourceTable = sourceTable;
            _targetTable = targetTable;
            _threadMarshaller = threadMarshaller;

            _rowSubscription = _sourceTable.Subscribe((IObserver<int>)this);
            _columnSubscription = _sourceTable.Subscribe((IObserver<ColumnUpdate>) this);
        }

        public void OnNext(int rowIndex)
        {
            _threadMarshaller.Dispatch(
                () =>
                    {
                        // TODO: Handle deletes!
                        if (rowIndex >= _targetTable.RowCount)
                        {
                            var newRowIndex = _targetTable.AddRow();
                            Debug.Assert(rowIndex == newRowIndex);
                        }

                        // Copy the values
                        foreach (var column in _sourceTable.Columns)
                        {
                            _targetTable.SetValue(column.Key, rowIndex, column.Value, rowIndex);
                        }
                    });
        }

        public void OnNext(ColumnUpdate value)
        {
            _threadMarshaller.Dispatch(
                () => _targetTable.SetValue(value.Column.ColumnId, value.RowIndex, value.Column, value.RowIndex));
        }

        void IObserver<int>.OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        void IObserver<int>.OnCompleted()
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
        }
    }
}