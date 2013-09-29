using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Joins;

namespace ReactiveTables.Framework.Filters
{
    /// <summary>
    /// A table that filters the underlying table using the supplied <see cref="IReactivePredicate"/>
    /// </summary>
    public class FilteredTable : IReactiveTable, IDisposable
    {
        private readonly IReactiveTable _sourceTable;
        private readonly IReactivePredicate _predicate;
        private readonly Dictionary<int, int> _filterRowToSourceRow = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _sourceRowToFilterRow = new Dictionary<int, int>();
        private readonly IDisposable _token;
        private readonly FieldRowManager _rowManager = new FieldRowManager();
        private readonly HashSet<IObserver<TableUpdate>> _observers = new HashSet<IObserver<TableUpdate>>();

        public FilteredTable(IReactiveTable sourceTable, IReactivePredicate predicate)
        {
            _sourceTable = sourceTable;
            _predicate = predicate;

            _token = _sourceTable.ReplayAndSubscribe(OnNext); 

            ChangeNotifier = new PropertyChangedNotifier(this);
        }

        private void OnNext(TableUpdate tableUpdate)
        {
            if (tableUpdate.Action == TableUpdate.TableUpdateAction.Delete)
            {
                TryRemoveMapping(tableUpdate.RowIndex);
                return;
            }

            bool shouldCheck = tableUpdate.Action == TableUpdate.TableUpdateAction.Add ||
                               _predicate.Columns.Contains(tableUpdate.Column.ColumnId);

            if (!shouldCheck) return;

            var rowIsVisible = _predicate.RowIsVisible(_sourceTable, tableUpdate.RowIndex);
            int filterRow;
            // We already have this row mapped
            if (_sourceRowToFilterRow.TryGetValue(tableUpdate.RowIndex, out filterRow))
            {
                // Already exists but is no longer visible
                if (!rowIsVisible)
                {
                    TryRemoveMapping(tableUpdate.RowIndex);
                    OnDelete(filterRow);
                }
                // otherwise do we need to propagate the column update
                else
                {
                    OnUpdate(filterRow, tableUpdate.Columns);
                }
            }
            // No mapping yet
            else
            {
                // Has just appeared
                if (rowIsVisible)
                {
                    int addRow = _rowManager.AddRow();
                    _sourceRowToFilterRow.Add(tableUpdate.RowIndex, addRow);
                    _filterRowToSourceRow.Add(addRow, tableUpdate.RowIndex);
                    OnAdd(addRow);
                }
                // otherwise do nothing
            }
        }

        private void OnAdd(int filterRow)
        {
            var update = new TableUpdate(TableUpdate.TableUpdateAction.Add, filterRow);
            foreach (var observer in _observers)
            {
                observer.OnNext(update);
            }
        }

        private void OnUpdate(int filterRow, IList<IReactiveColumn> column)
        {
            var update = new TableUpdate(TableUpdate.TableUpdateAction.Update, filterRow, column);
            foreach (var observer in _observers)
            {
                observer.OnNext(update);
            }
        }

        private void OnDelete(int filterRow)
        {
            var update = new TableUpdate(TableUpdate.TableUpdateAction.Delete, filterRow);
            foreach (var observer in _observers)
            {
                observer.OnNext(update);
            }
        }

        private void TryRemoveMapping(int rowIndex)
        {
            int filterRow;
            if (_sourceRowToFilterRow.TryGetValue(rowIndex, out filterRow))
            {
                _sourceRowToFilterRow.Remove(rowIndex);
                _filterRowToSourceRow.Remove(filterRow);
            }
        }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            _observers.Add(observer);
            return new SubscriptionToken<IReactiveTable, IObserver<TableUpdate>>(this, observer);
        }

        public void Unsubscribe(IObserver<TableUpdate> observer)
        {
            _observers.Remove(observer);
        }

        public IReactiveColumn AddColumn(IReactiveColumn column)
        {
            // TODO: Maybe we should delegate to the source table
            throw new NotImplementedException();
        }

        public T GetValue<T>(string columnId, int rowIndex)
        {
            int realRowIndex;
            if (_filterRowToSourceRow.TryGetValue(rowIndex, out realRowIndex))
            {
                return _sourceTable.GetValue<T>(columnId, realRowIndex);
            }
            
            return default(T);
        }

        public object GetValue(string columnId, int rowIndex)
        {
            int realRowIndex;
            if (_filterRowToSourceRow.TryGetValue(rowIndex, out realRowIndex))
            {
                return _sourceTable.GetValue(columnId, rowIndex);
            }

            return null;
        }

        public int RowCount
        {
            get { return _rowManager.RowCount; }
        }

        public IDictionary<string, IReactiveColumn> Columns { get { return _sourceTable.Columns; } }

        public IReactiveColumn GetColumnByIndex(int index)
        {
            return _sourceTable.GetColumnByIndex(index);
        }

        public PropertyChangedNotifier ChangeNotifier { get; private set; }

        public IReactiveTable Join(IReactiveTable otherTable, IReactiveTableJoiner joiner)
        {
            return new JoinedTable(this, otherTable, joiner);
        }

        public IReactiveTable Filter(IReactivePredicate predicate)
        {
            return new FilteredTable(this, predicate);
        }

        public void ReplayRows(IObserver<TableUpdate> observer)
        {
            var rowAdds = new List<TableUpdate>(_filterRowToSourceRow.Count);
            rowAdds.AddRange(_filterRowToSourceRow.Keys.Select(row => new TableUpdate(TableUpdate.TableUpdateAction.Add, row)));
            foreach (var rowAdd in rowAdds)
            {
                observer.OnNext(rowAdd);
            }
        }

        public int GetRowAt(int position)
        {
            return _rowManager.GetRowAt(position);
        }

        public int GetPositionOfRow(int rowIndex)
        {
            return _rowManager.GetPositionOfRow(rowIndex);
        }

        public void Dispose()
        {
            if (_token != null)
            {
                _token.Dispose();
            }
        }
    }
}