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
using ReactiveTables.Framework;
using ReactiveTables.Framework.Utils;

namespace ReactiveTables.Demo.Syncfusion
{
    public class SyncfusionViewModelBase : ISyncfusionViewModel, IDisposable
    {
        private IReactiveTable _table;
        private readonly Subject<TableUpdate> _subject = new Subject<TableUpdate>();
        private IDisposable _token;

        protected void SetTable(IReactiveTable table)
        {
            _table = table;
            _token = table.Subscribe(OnNext);
        }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            return _subject.Subscribe(observer);
        }

        private void OnNext(TableUpdate tableUpdate)
        {
            _subject.OnNext(tableUpdate);
        }

        public T GetValue<T>(int rowIndex, int columnIndex)
        {
            var row = _table.GetRowAt(rowIndex);
            if (row >= 0 && columnIndex >= 0)
            {
                var reactiveColumn = _table.GetColumnByIndex(columnIndex);
                return _table.GetValue<T>(reactiveColumn.ColumnId, row);
            }
            return default(T);
        }

        public object GetValue(int rowIndex, int columnIndex)
        {
            var row = _table.GetRowAt(rowIndex);
            if (row >= 0 && columnIndex >= 0)
            {
                var reactiveColumn = _table.GetColumnByIndex(columnIndex);
                // Nasty - boxing!!
                return _table.GetValue(reactiveColumn.ColumnId, row);
            }

            return null;
        }

        public int GetRowPosition(int rowIndex)
        {
            return _table.GetPositionOfRow(rowIndex);
        }

        public int GetColPosition(string columnId)
        {
            // TODO: Nasty - should keep a list of columns that are actually used by the grid and a map of their indeces
            return _table.Columns.Keys.IndexOf(columnId);
        }

        public string GetColumnId(int columnIndex)
        {
            var reactiveColumn = _table.GetColumnByIndex(columnIndex);
            return reactiveColumn.ColumnId;
        }

        public void Dispose()
        {
            if (_token != null) _token.Dispose();
        }

        // TODO: Handle columns added after the ViewModel is created.
        public int ColumnCount { get { return _table.Columns.Count; } }
    }
}