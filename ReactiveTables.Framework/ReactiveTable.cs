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
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Filters;
using ReactiveTables.Framework.Joins;

namespace ReactiveTables.Framework
{
    public interface IReactiveTable : IObservable<TableUpdate>, ISubscribable<IObserver<TableUpdate>>
    {
        void AddColumn(IReactiveColumn column);

        /// <summary>
        /// Typed version
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="columnId"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        T GetValue<T>(string columnId, int rowIndex);

        /// <summary>
        /// Untyped version
        /// </summary>
        /// <param name="columnId"></param>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        object GetValue(string columnId, int rowIndex);

        int RowCount { get; }
        Dictionary<string, IReactiveColumn> Columns { get; }
        PropertyChangedNotifier ChangeNotifier { get; }
        IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner);
        IReactiveTable Filter(IReactivePredicate predicate);
        void ReplayRows(IObserver<TableUpdate> observer);
        int GetRowAt(int position);
        int GetPositionOfRow(int rowIndex);
    }

    public interface IWritableReactiveTable : IReactiveTable
    {
        void SetValue<T>(string columnId, int rowIndex, T value);
        void SetValue(string columnId, int rowIndex, IReactiveColumn sourceColumn, int sourceRowIndex);
        int AddRow();
        void DeleteRow(int rowIndex);
    }

    public class ReactiveTable : IWritableReactiveTable
    {
        private readonly Dictionary<string, IReactiveColumn> _columns = new Dictionary<string, IReactiveColumn>();
        private readonly HashSet<IObserver<TableUpdate>> _observers = new HashSet<IObserver<TableUpdate>>();

        private readonly FieldRowManager _rowManager = new FieldRowManager();

        public PropertyChangedNotifier ChangeNotifier { get; private set; }

        public ReactiveTable()
        {
            ChangeNotifier = new PropertyChangedNotifier(this);
        }

        public ReactiveTable(IReactiveTable reactiveTable)
        {
            CloneColumns(reactiveTable);
        }

        public void AddColumn(IReactiveColumn column)
        {
            var columnId = column.ColumnId;
            Columns.Add(columnId, column);
            column.Subscribe(new ColumnChangePublisher(column, _observers));
            // TODO: fire events for existing rows
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

            var rowUpdate = new TableUpdate(TableUpdate.TableUpdateAction.Add, rowIndex);
            foreach (var observer in _observers)
            {
                observer.OnNext(rowUpdate);
            }
            return rowIndex;
        }

        public void DeleteRow(int rowIndex)
        {
            _rowManager.DeleteRow(rowIndex);
            foreach (var column in Columns)
            {
                column.Value.RemoveField(rowIndex);
            }

            var rowUpdate = new TableUpdate(TableUpdate.TableUpdateAction.Delete, rowIndex);
            foreach (var observer in _observers)
            {
                observer.OnNext(rowUpdate);
            }
        }

        public IReactiveTable Filter(IReactivePredicate predicate)
        {
            return new FilteredTable(this, predicate);
        }

        public void ReplayRows(IObserver<TableUpdate> observer)
        {
            var rowAdds = new List<TableUpdate>(_rowManager.RowCount);
            rowAdds.AddRange(_rowManager.GetRows().Select(row => new TableUpdate(TableUpdate.TableUpdateAction.Add, row)));

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

        public Dictionary<string, IReactiveColumn> Columns
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
            _observers.Add(observer);
            return new SubscriptionToken<ReactiveTable, IObserver<TableUpdate>>(this, observer);
        }

        public void Unsubscribe(IObserver<TableUpdate> observer)
        {
            _observers.Remove(observer);
        }

        public int Find<T>(string columnId, T value)
        {
            var column = GetColumn<T>(columnId);
            return column.Find(value);
        }
    }
}