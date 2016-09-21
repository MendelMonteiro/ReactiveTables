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
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Filters;
using ReactiveTables.Framework.Marshalling;

namespace ReactiveTables.Framework.Synchronisation
{
    /// <summary>
    /// A table that does not store any values but instead writes all updates received to another table.
    /// This table can be written to from multiple threads.
    /// </summary>
    public class ReactivePassThroughTable : IWritableReactiveTable
    {
        private readonly FieldRowManager _rowManager = new FieldRowManager();

        private readonly ReactiveTable _targetTargetTable;

        private readonly IThreadMarshaller _marshaller;

        public ReactivePassThroughTable(ReactiveTable targetTable, IThreadMarshaller marshaller)
        {
            _targetTargetTable = targetTable;
            _marshaller = marshaller;
        }

        public object GetValue(string columnId, int rowIndex)
        {
            throw new NotImplementedException();
        }

        public int RowCount => _rowManager.RowCount;

//        public IDictionary<string, IReactiveColumn> Columns { get { throw new NotImplementedException(); } }
        public IReadOnlyList<IReactiveColumn> Columns { get { throw new NotImplementedException(); } }

        public IReactiveColumn GetColumnByIndex(int index)
        {
            throw new NotImplementedException();
        }

        public PropertyChangedNotifier ChangeNotifier
        {
            get { throw new NotImplementedException(); }
        }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe(IObserver<TableUpdate> observer)
        {
            throw new NotImplementedException();
        }

        public IReactiveColumn AddColumn(IReactiveColumn column, bool shouldSubscribe = true)
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
            lock (_rowManager)
            {
                return _rowManager.GetRowAt(position);
            }
        }

        public int GetPositionOfRow(int rowIndex)
        {
            lock (_rowManager)
            {
                return _rowManager.GetPositionOfRow(rowIndex);
            }
        }

        public IReactiveColumn GetColumnByName(string columnId)
        {
            return ((IReactiveTable) _targetTargetTable).GetColumnByName(columnId);
        }

        public bool GetColumnByName(string columnId, out IReactiveColumn column)
        {
            return ((IReactiveTable) _targetTargetTable).GetColumnByName(columnId, out column);
        }

        public void SetValue<T>(string columnId, int rowIndex, T value)
        {
            _marshaller.Dispatch(() => _targetTargetTable.SetValue(columnId, rowIndex, value));
        }

        public void SetValue(string columnId, int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex)
        {
            throw new NotImplementedException();
        }

        public int AddRow()
        {
            lock (_rowManager)
            {
                var rowIndex = _rowManager.AddRow();
                _marshaller.Dispatch(() => _targetTargetTable.AddRow());
                return rowIndex;
            }
        }

        public void DeleteRow(int rowIndex)
        {
            lock (_rowManager)
            {
                _rowManager.DeleteRow(rowIndex);
                _marshaller.Dispatch(() => _targetTargetTable.DeleteRow(rowIndex));
            }
        }
    }
}