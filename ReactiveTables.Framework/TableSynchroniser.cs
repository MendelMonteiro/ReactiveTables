using System;
using System.Diagnostics;
using ReactiveTables.Framework.Marshalling;

namespace ReactiveTables.Framework
{
    public class TableSynchroniser : IObserver<RowUpdate>, IObserver<ColumnUpdate>, IDisposable
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

            _rowSubscription = _sourceTable.Subscribe((IObserver<RowUpdate>)this);
            _columnSubscription = _sourceTable.Subscribe((IObserver<ColumnUpdate>) this);
        }

        public void OnNext(RowUpdate rowUpdate)
        {
            _threadMarshaller.Dispatch(
                () =>
                    {
                        if (rowUpdate.Action == RowUpdate.RowUpdateAction.Add)
                        {
//                            if (rowUpdate.RowIndex >= _targetTable.RowCount)
//                            {
                                var newRowIndex = _targetTable.AddRow();
                                Debug.Assert(rowUpdate.RowIndex == newRowIndex);
//                            }
                        }
                        else if (rowUpdate.Action == RowUpdate.RowUpdateAction.Delete)
                        {
                            _targetTable.DeleteRow(rowUpdate.RowIndex);
                        }
                    });
        }

        public void OnNext(ColumnUpdate value)
        {
            _threadMarshaller.Dispatch(
                () => _targetTable.SetValue(value.Column.ColumnId, value.RowIndex, value.Column, value.RowIndex));
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
        }
    }
}