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
using System.Linq;
using System.Reactive.Subjects;
using ReactiveTables.Framework;

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

        public object GetValue(int rowIndex, int columnIndex)
        {
            var row = _table.GetRowAt(rowIndex);
            if (row >= 0 && columnIndex >= 0)
            {
                var reactiveColumn = _table.Columns.Values.ElementAt(columnIndex);
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

        public void Dispose()
        {
            if (_token != null) _token.Dispose();
        }
    }

    public static class CollectionExtensions
    {
        public static int IndexOf<T>(this IEnumerable<T> collection, T item)
        {
            var i = 0;
            foreach (var foo in collection)
            {
                if (foo.Equals(item))
                {
                    return i;
                }
                i++;
            }
            return -1;
        }
    }
}