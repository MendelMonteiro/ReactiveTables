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
using System.Reactive.Subjects;
using ReactiveTables.Demo.Utils;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Sorting;
using ReactiveTables.Framework.Utils;

namespace ReactiveTables.Demo.Syncfusion
{
    class SyncfusionViewModelBase : BaseViewModel, ISyncfusionViewModel, IDisposable
    {
        protected IReactiveTable Table;
        private readonly Subject<TableUpdate> _subject = new Subject<TableUpdate>();
        protected IDisposable Token;

        protected void SetTable(IReactiveTable table)
        {
            Table = table;
            Token = table.Subscribe(OnNext);
            var sortedTable = table as IReactiveSortedTable;
            if (sortedTable != null)
            {
                RowPositionsUpdated = sortedTable.RowPositionsUpdated;
            }
        }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            return _subject.Subscribe(observer);
        }

        private void OnNext(TableUpdate tableUpdate)
        {
            _subject.OnNext(tableUpdate);
        }

        public virtual T GetValue<T>(int rowIndex, int columnIndex)
        {
            var row = Table.GetRowAt(rowIndex);
            if (row >= 0 && columnIndex >= 0)
            {
                var reactiveColumn = Table.GetColumnByIndex(columnIndex);
                return Table.GetValue<T>(reactiveColumn.ColumnId, row);
            }
            return default(T);
        }

        public virtual int GetRowPosition(int rowIndex)
        {
            return Table.GetPositionOfRow(rowIndex);
        }

        public virtual int GetColPosition(string columnId)
        {
            // TODO: Nasty - should keep a list of columns that are actually used by the grid and a map of their indeces
            return Table.Columns.Keys.IndexOf(columnId);
        }

        public virtual string GetColumnId(int columnIndex)
        {
            var reactiveColumn = Table.GetColumnByIndex(columnIndex);
            return reactiveColumn.ColumnId;
        }

        public IObservable<bool> RowPositionsUpdated { get; private set; }

        public void Dispose()
        {
            if (Token != null) Token.Dispose();
            if (_subject != null) _subject.Dispose();
        }

        // TODO: Handle columns added after the ViewModel is created.
        public int ColumnCount { get { return Table.Columns.Count; } }
    }
}