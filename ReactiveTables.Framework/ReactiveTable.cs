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
using System.Collections.Generic;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework
{
    public interface IReactiveTable : IObservable<RowUpdate>, ISubscribable<IObserver<RowUpdate>>, IObservable<ColumnUpdate>, ISubscribable<IObserver<ColumnUpdate>>
    {
        IReactiveColumn AddColumn(IReactiveColumn column);
        T GetValue<T>(string columnId, int rowIndex);
        int RowCount { get; }
        Dictionary<string, IReactiveColumn> Columns { get; }
        PropertyChangedNotifier ChangeNotifier { get; }
        IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner);
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
        private readonly HashSet<IObserver<RowUpdate>> _rowObservers = new HashSet<IObserver<RowUpdate>>();
        private readonly HashSet<IObserver<ColumnUpdate>> _columnObservers = new HashSet<IObserver<ColumnUpdate>>();
        private readonly FieldRowManager _rowManager = new FieldRowManager();

        public PropertyChangedNotifier ChangeNotifier { get; private set; }

        public ReactiveTable()
        {
            ChangeNotifier = new PropertyChangedNotifier(this);
        }

        public IReactiveColumn AddColumn(IReactiveColumn column)
        {
            var columnId = column.ColumnId;
            Columns.Add(columnId, column);
            column.Subscribe(new ColumnChangePublisher(column, _rowObservers, _columnObservers));
            // TODO: fire events for existing rows
            return column;
        }

        private IReactiveColumn<T> GetColumn<T>(string columnId)
        {
            return (IReactiveColumn<T>) Columns[columnId];
        }

        private IReactiveField<T> GetField<T>(string columnId, int index)
        {
            return GetColumn<T>(columnId).GetValue(index);
        }

        public T GetValue<T>(string columnId, int rowIndex)
        {
            return GetColumn<T>(columnId).GetValue(rowIndex).Value;
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

            var rowUpdate = new RowUpdate(rowIndex, RowUpdate.RowUpdateAction.Add);
            foreach (var observer in _rowObservers)
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

            var rowUpdate = new RowUpdate(rowIndex, RowUpdate.RowUpdateAction.Delete);
            foreach (var observer in _rowObservers)
            {
                observer.OnNext(rowUpdate);
            }
        }

        public int RowCount { get { return _rowManager.RowCount; } }

        public Dictionary<string, IReactiveColumn> Columns
        {
            get { return _columns; }
        }

        public IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner)
        {
            return new JoinedTable(this, otherTable, joiner);
        }

        public ReactiveTable(IReactiveTable reactiveTable)
        {
            CloneColumns(reactiveTable);
        }

        public void CloneColumns(IReactiveTable reactiveTable)
        {
            foreach (var column in reactiveTable.Columns)
            {
                AddColumn(column.Value.Clone());
            }
        }

        public IDisposable Subscribe(IObserver<RowUpdate> observer)
        {
            _rowObservers.Add(observer);
            return new SubscriptionToken<ReactiveTable, IObserver<RowUpdate>>(this, observer);
        }

        public void Unsubscribe(IObserver<RowUpdate> observer)
        {
            _rowObservers.Remove(observer);
        }

        public IDisposable Subscribe(IObserver<ColumnUpdate> observer)
        {
            _columnObservers.Add(observer);
            return new SubscriptionToken<ReactiveTable, IObserver<ColumnUpdate>>(this, observer);
        }

        public void Unsubscribe(IObserver<ColumnUpdate> observer)
        {
            _columnObservers.Remove(observer);
        }
    }
}
