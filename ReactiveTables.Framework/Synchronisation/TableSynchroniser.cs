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
along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Diagnostics;
using ReactiveTables.Framework.Marshalling;

namespace ReactiveTables.Framework.Synchronisation
{
    public class TableSynchroniser : IObserver<TableUpdate>, IDisposable
    {
        private readonly IReactiveTable _sourceTable;
        private readonly IWritableReactiveTable _targetTable;
        private readonly IThreadMarshaller _threadMarshaller;
        private readonly IDisposable _subscription;
        private readonly IDisposable _columnSubscription;

        public TableSynchroniser(IReactiveTable sourceTable, IWritableReactiveTable targetTable, IThreadMarshaller threadMarshaller)
        {
            _sourceTable = sourceTable;
            _targetTable = targetTable;
            _threadMarshaller = threadMarshaller;

            _subscription = _sourceTable.Subscribe((IObserver<TableUpdate>)this);
        }

        public void OnNext(TableUpdate update)
        {
            _threadMarshaller.Dispatch(
                () =>
                    {
                        if (update.Action == TableUpdate.TableUpdateAction.Add)
                        {
                            var newRowIndex = _targetTable.AddRow();
                            Debug.Assert(update.RowIndex == newRowIndex);
                        }
                        else if (update.Action == TableUpdate.TableUpdateAction.Delete)
                        {
                            _targetTable.DeleteRow(update.RowIndex);
                        }
                        else if (update.Action == TableUpdate.TableUpdateAction.Update)
                        {
                            // BUG: When this line is called the original update.Column may not contain the same state as when the outside method is called.
                            _targetTable.SetValue(update.Column.ColumnId, update.RowIndex, update.Column, update.RowIndex);
                        }
                    });
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
            _columnSubscription.Dispose();
            _subscription.Dispose();
        }
    }
}
