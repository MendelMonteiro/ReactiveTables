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
using System.Reactive.Subjects;
using ReactiveTables.Demo.Utils;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Sorting;
using ReactiveTables.Framework.Utils;
using System.Linq;

namespace ReactiveTables.Demo.Syncfusion
{
    /// <summary>
    /// Extend this view model to bind a ReactiveTable to a Syncfusion grid.
    /// There is a 1 to 1 relation between a view model and a ReactiveTable.
    /// Simply call SetTable in the child class constructor to register the table and then set up the Grid columns in the window code behind.
    /// </summary>
    public abstract class SyncfusionViewModelBase : BaseViewModel, ISyncfusionViewModel, IDisposable
    {
        private IReactiveTable _table;
        private IWritableReactiveTable _writableTable;
        private readonly Subject<TableUpdate> _subject = new Subject<TableUpdate>();
        private IDisposable _token;
        private List<string> _columnIds; 

        protected void SetTable(IReactiveTable table)
        {
            _table = table;
            _writableTable = _table as IWritableReactiveTable;
            _token = table.ReplayAndSubscribe(OnNext);
            var sortedTable = table as ISortedTable;
            if (sortedTable != null)
            {
                RowPositionsUpdated = sortedTable.RowPositionsUpdated;
            }

            // TODO: This should look up a dictionary of column id's to friendly column names
            ColumnNames = table.Columns.Select(c => c.ColumnId.Substring(c.ColumnId.LastIndexOf('.') + 1)).ToList();

            _columnIds = table.Columns.Select(c => c.ColumnId).ToList();
        }

        public IList<string> ColumnNames { get; private set; }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            var disposable = _subject.Subscribe(observer);
            _table.ReplayRows(observer);
            return disposable;
        }

        private void OnNext(TableUpdate tableUpdate)
        {
            _subject.OnNext(tableUpdate);
        }

        public virtual T GetValue<T>(int rowIndex, int columnIndex)
        {
            IReactiveColumn column;
            var row = GetColAndRowFromGridCoordinates<T>(rowIndex, columnIndex, out column);
            if (column != null && row >= 0)
            {
                return _table.GetValue<T>(column.ColumnId, row);
            }
            return default(T);
        }

        private int GetColAndRowFromGridCoordinates<T>(int rowIndex, int columnIndex, out IReactiveColumn column)
        {
            var row = _table.GetRowAt(rowIndex);
            column = null;
            if (row >= 0 && columnIndex >= 0)
            {
                column = _table.GetColumnByIndex(columnIndex);
            }
            return row;
        }

        public void SetValue<T>(int rowIndex, int columnIndex, T value)
        {
            if (_writableTable == null) return;

            IReactiveColumn column;
            var row = GetColAndRowFromGridCoordinates<T>(rowIndex, columnIndex, out column);
            _writableTable.SetValue(column.ColumnId, row, value);
        }

        public virtual int GetRowPosition(int rowIndex)
        {
            return _table.GetPositionOfRow(rowIndex);
        }

        public virtual int GetColPosition(string columnId)
        {
            // TODO: Nasty - should keep a list of columns that are actually used by the grid and a map of their indeces
            return _columnIds.IndexOf(columnId);
        }

        public virtual string GetColumnId(int columnIndex)
        {
            var reactiveColumn = _table.GetColumnByIndex(columnIndex);
            return reactiveColumn.ColumnId;
        }

        public IObservable<bool> RowPositionsUpdated { get; private set; }

        public void Dispose()
        {
            DisposeCore();
        }

        protected virtual void DisposeCore()
        {
            if (_token != null) _token.Dispose();
            if (_subject != null) _subject.Dispose();            
        }

        // TODO: Handle columns added after the ViewModel is created.
        public int ColumnCount => _table.Columns.Count;
    }
}