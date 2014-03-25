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
using ReactiveTables.Framework.Collections;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Filters;
using ReactiveTables.Framework.Joins;

namespace ReactiveTables.Framework
{
    /// <summary>
    /// The main writable/readable table.
    /// </summary>
    public interface IWritableReactiveTable : IReactiveTable
    {
        /// <summary>
        /// Set a value at a given column and row index
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnId"></param>
        /// <param name="rowIndex"></param>
        /// <param name="value"></param>
        void SetValue<T>(string columnId, int rowIndex, T value);

        /// <summary>
        /// Copy the value from the given column at the specified row index.
        /// Used for generic code which cannot specify the type at run time.
        /// </summary>
        /// <param name="columnId"></param>
        /// <param name="rowIndex"></param>
        /// <param name="sourceColumn"></param>
        /// <param name="sourceRowIndex"></param>
        void SetValue(string columnId, int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex);

        /// <summary>
        /// Add a row to the table
        /// </summary>
        /// <returns></returns>
        int AddRow();

        /// <summary>
        /// Delete a row from the table.
        /// </summary>
        /// <param name="rowIndex"></param>
        void DeleteRow(int rowIndex);
    }

    /// <summary>
    /// The main writable/readable table.
    /// </summary>
    public class ReactiveTable : IWritableReactiveTable, IDisposable
    {
        private readonly IndexedDictionary<string, IReactiveColumn> _columns = new IndexedDictionary<string, IReactiveColumn>();
        private readonly Subject<TableUpdate> _subject = new Subject<TableUpdate>();

        private readonly FieldRowManager _rowManager = new FieldRowManager();
        private readonly Lazy<PropertyChangedNotifier> _changeNotifier;

        public IReactiveColumn GetColumnByIndex(int index)
        {
            IList<IReactiveColumn> list = _columns;
            return list[index];
        }

        public PropertyChangedNotifier ChangeNotifier
        {
            get
            {
                return _changeNotifier.Value;
            }
        }

        /// <summary>
        /// Create a ReactiveTable
        /// </summary>
        public ReactiveTable()
        {
            _changeNotifier = new Lazy<PropertyChangedNotifier>(() => new PropertyChangedNotifier(this));
        }

        /// <summary>
        /// Create a ReactiveTable copying the columns from the given table
        /// </summary>
        /// <param name="reactiveTable"></param>
        public ReactiveTable(IReactiveTable reactiveTable)
        {
            CloneColumns(reactiveTable);
            _changeNotifier = new Lazy<PropertyChangedNotifier>(() => new PropertyChangedNotifier(this));
        }

        public IReactiveColumn AddColumn(IReactiveColumn column)
        {
            var columnId = column.ColumnId;
            Columns.Add(columnId, column);
            column.Subscribe(_subject);
            // TODO: fire events for existing rows
            return column;
        }

        private IReactiveColumn<T> GetColumn<T>(string columnId)
        {
            return (IReactiveColumn<T>) Columns[columnId];
        }

        public T GetValue<T>(string columnId, int rowIndex)
        {
            return GetColumn<T>(columnId).GetValue(rowIndex);
        }

        public object GetValue(string columnId, int rowIndex)
        {
            return Columns[columnId].GetValue(rowIndex);
        }

        public void SetValue<T>(string columnId, int rowIndex, T value)
        {
            GetColumn<T>(columnId).SetValue(rowIndex, value);
        }

        public void SetValue(string columnId, int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex)
        {
            _columns[columnId].CopyValue(rowIndex, sourceColumn, sourceRowIndex);
        }

        public int AddRow()
        {
            int rowIndex = _rowManager.AddRow();
            foreach (var column in Columns)
            {
                column.Value.AddField(rowIndex);
            }

            var rowUpdate = new TableUpdate(TableUpdateAction.Add, rowIndex);
            _subject.OnNext(rowUpdate);
            return rowIndex;
        }

        public void DeleteRow(int rowIndex)
        {
            _rowManager.DeleteRow(rowIndex);
            foreach (var column in Columns)
            {
                column.Value.RemoveField(rowIndex);
            }

            var rowUpdate = new TableUpdate(TableUpdateAction.Delete, rowIndex);
            _subject.OnNext(rowUpdate);
        }

        public IReactiveTable Filter(IReactivePredicate predicate)
        {
            return new FilteredTable(this, predicate);
        }

        public void ReplayRows(IObserver<TableUpdate> observer)
        {
            var rowAdds = new List<TableUpdate>(_rowManager.RowCount);
            rowAdds.AddRange(_rowManager.GetRows().Select(row => new TableUpdate(TableUpdateAction.Add, row)));

            foreach (var rowAdd in rowAdds)
            {
                observer.OnNext(rowAdd);
            }
        }

        public IEnumerable<int> GetRows()
        {
            return _rowManager.GetRows();
        }

        public int GetRowAt(int position)
        {
            return _rowManager.GetRowAt(position);
        }

        public int GetPositionOfRow(int rowIndex)
        {
            return _rowManager.GetPositionOfRow(rowIndex);
        }

        public int RowCount
        {
            get { return _rowManager.RowCount; }
        }

        public IDictionary<string, IReactiveColumn> Columns
        {
            get { return _columns; }
        }

        public IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner)
        {
            return new JoinedTable(this, otherTable, joiner);
        }

        public void CloneColumns(IReactiveTable reactiveTable)
        {
            foreach (var column in reactiveTable.Columns)
            {
                AddColumn(column.Value.Clone());
            }
        }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            return _subject.Subscribe(observer);
        }

        /// <summary>
        /// Finds a row for the given value when an index is defined for the column.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnId"></param>
        /// <param name="value"></param>
        /// <returns>The row id</returns>
        public int Find<T>(string columnId, T value)
        {
            var column = GetColumn<T>(columnId);
            return column.Find(value);
        }

        public void Dispose()
        {
            foreach (var reactiveColumn in Columns.Values)
            {
                var disposable = reactiveColumn as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                }
            }
        }
    }
}