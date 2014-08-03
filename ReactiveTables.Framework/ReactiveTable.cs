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
    /// Contains functionality common to all tables
    /// </summary>
    public abstract class ReactiveTableBase : IReactiveTable
    {
        protected readonly Lazy<PropertyChangedNotifier> _changeNotifier;

        public ReactiveTableBase()
        {
            _changeNotifier = new Lazy<PropertyChangedNotifier>(() => new PropertyChangedNotifier(this));            
        }

        public abstract IReactiveColumn AddColumn(IReactiveColumn column, bool shouldSubscribe = true);
        public abstract T GetValue<T>(string columnId, int rowIndex);
        public abstract object GetValue(string columnId, int rowIndex);
        public abstract int RowCount { get; }
//        public abstract IDictionary<string, IReactiveColumn> Columns { get; }
        public abstract IReadOnlyList<IReactiveColumn> Columns { get; }

        public abstract IReactiveColumn GetColumnByIndex(int index);

        public PropertyChangedNotifier ChangeNotifier
        {
            get { return _changeNotifier.Value; }
        }

        public abstract void ReplayRows(IObserver<TableUpdate> observer);
        public abstract int GetRowAt(int position);
        public abstract int GetPositionOfRow(int rowIndex);
        public abstract IReactiveColumn GetColumnByName(string columnId);
        public abstract bool GetColumnByName(string columnId, out IReactiveColumn column);

        public abstract IDisposable Subscribe(IObserver<TableUpdate> observer);

        public virtual IReactiveTable Filter(IReactivePredicate predicate)
        {
            return new FilteredTable(this, predicate);
        }

        public virtual IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner)
        {
            return new JoinedTable(this, otherTable, joiner);
        }
    }

    /// <summary>
    /// The main writable/readable table.
    /// </summary>
    public class ReactiveTable : ReactiveTableBase, IWritableReactiveTable, IDisposable
    {
        private readonly Subject<TableUpdate> _subject = new Subject<TableUpdate>();
        private readonly ColumnList _columns = new ColumnList();
        private readonly FieldRowManager _rowManager = new FieldRowManager();

        /// <summary>
        /// Create a ReactiveTable
        /// </summary>
        public ReactiveTable()
        {
        }

        /// <summary>
        /// Create a ReactiveTable copying the columns from the given table
        /// </summary>
        /// <param name="reactiveTable"></param>
        public ReactiveTable(IReactiveTable reactiveTable)
        {
            CloneColumns(reactiveTable);
        }


        private IReactiveColumn<T> GetColumn<T>(string columnId)
        {
            return (IReactiveColumn<T>) GetColumnByName(columnId);
        }

        public override IReactiveColumn AddColumn(IReactiveColumn column, bool shouldSubscribe = true)
        {
            if (shouldSubscribe)
            {
                column.Subscribe(_subject);
            }
            return _columns.AddColumn(column);
        }

        public override T GetValue<T>(string columnId, int rowIndex)
        {
            return GetColumn<T>(columnId).GetValue(rowIndex);
        }

        public override object GetValue(string columnId, int rowIndex)
        {
            return GetColumnByName(columnId).GetValue(rowIndex);
        }

        public void SetValue<T>(string columnId, int rowIndex, T value)
        {
            GetColumn<T>(columnId).SetValue(rowIndex, value);
        }

        public void SetValue(string columnId, int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex)
        {
            GetColumnByName(columnId).CopyValue(rowIndex, sourceColumn, sourceRowIndex);
        }


        public int AddRow()
        {
            int rowIndex = _rowManager.AddRow();
            for (int index = 0; index < Columns.Count; index++)
            {
                var column = Columns[index];
                column.AddField(rowIndex);
            }

            var rowUpdate = new TableUpdate(TableUpdateAction.Add, rowIndex);
            _subject.OnNext(rowUpdate);
            return rowIndex;
        }

        /// <summary>
        /// Delete the given row from the underlying columns and notify consumers
        /// </summary>
        /// <param name="rowIndex"></param>
        public void DeleteRow(int rowIndex)
        {
            // First notify (don't delete first so that consumers can still use the value being deleted)
            var rowUpdate = new TableUpdate(TableUpdateAction.Delete, rowIndex);
            _subject.OnNext(rowUpdate);

            // And then delete from the underlying store
            _rowManager.DeleteRow(rowIndex);
            foreach (var column in Columns)
            {
                column.RemoveField(rowIndex);
            }
        }

        public override void ReplayRows(IObserver<TableUpdate> observer)
        {
            var rowAdds = new List<TableUpdate>(_rowManager.RowCount);
            rowAdds.AddRange(_rowManager.GetRows().Select(row => new TableUpdate(TableUpdateAction.Add, row)));

            foreach (var rowAdd in rowAdds)
            {
                observer.OnNext(rowAdd);
            }
        }

        /// <summary>
        /// Get all row IDs
        /// </summary>
        /// <returns></returns>
        public IEnumerable<int> GetRows()
        {
            return _rowManager.GetRows();
        }

        public override int GetRowAt(int position)
        {
            return _rowManager.GetRowAt(position);
        }

        public override int GetPositionOfRow(int rowIndex)
        {
            return _rowManager.GetPositionOfRow(rowIndex);
        }

        public override IReactiveColumn GetColumnByName(string columnId)
        {
            return _columns.GetColumnByName(columnId);
        }

        public override bool GetColumnByName(string columnId, out IReactiveColumn column)
        {
            return _columns.GetColumnByName(columnId, out column);
        }

        public override int RowCount
        {
            get { return _rowManager.RowCount; }
        }

        public override IReadOnlyList<IReactiveColumn> Columns { get { return _columns.Columns; } }

        public override IReactiveColumn GetColumnByIndex(int index)
        {
            return _columns.GetColumnByIndex(index);
        }

        public void CloneColumns(IReactiveTable reactiveTable)
        {
            foreach (var column in reactiveTable.Columns)
            {
                AddColumn(column.Clone());
            }
        }

        public override IDisposable Subscribe(IObserver<TableUpdate> observer)
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
            foreach (var reactiveColumn in Columns)
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