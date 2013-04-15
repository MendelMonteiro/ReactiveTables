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

        public void OnNext(RowUpdate update)
        {
            _threadMarshaller.Dispatch(
                () =>
                    {
                        if (update.Action == RowUpdate.RowUpdateAction.Add)
                        {
                            var newRowIndex = _targetTable.AddRow();
                            Debug.Assert(update.RowIndex == newRowIndex);
                        }
                        else if (update.Action == RowUpdate.RowUpdateAction.Delete)
                        {
                            _targetTable.DeleteRow(update.RowIndex);
                        }
                    });
        }

        public void OnNext(ColumnUpdate update)
        {
            _threadMarshaller.Dispatch(
                () => _targetTable.SetValue(update.Column.ColumnId, update.RowIndex, update.Column, update.RowIndex));
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
