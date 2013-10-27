using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Filters;
using ReactiveTables.Framework.Joins;
using System.Linq;

namespace ReactiveTables.Framework.Sorting
{
    public interface IReactiveSortedTable
    {
        IObservable<bool> RowPositionsUpdated { get; }
    }

    /// <summary>
    /// 1. New row arrives
    ///     - Need to propagate event at the righ row position
    ///     - Update the positions of other rows (invalidate the view)
    /// 2. Row is updated
    ///     - If the key column changes we need to re-sort and then do a BinarySearch to find the new position
    ///     - If another column changes we need to find the row position using the key and BinarySearch to propagate
    ///     - Update the positions of other rows (invalidate the view)
    /// 3. Row at position X scrolls into view
    ///     - Go get the row id at the given position
    /// </summary>
    public class SortedTable<T> : IReactiveTable, IReactiveSortedTable, IDisposable where T : IComparable<T>
    {
        /// <summary>
        /// Used to keep the values present before an update to the key
        /// </summary>
        private readonly Dictionary<int, T> _rowIdsToValues; 
        /// <summary>
        /// List of all the keys which is kept sorted
        /// </summary>
        private readonly List<KeyValuePair<T, int>> _keysToRows;

        /// <summary>
        /// Used to notify outside observers
        /// </summary>
        private readonly Subject<TableUpdate> _subject;

        private readonly IReactiveTable _sourceTable;
        private readonly string _sortColumnId;
        private readonly IComparer<T> _comparer;
        private readonly IDisposable _token;
        private readonly KeyComparer<T> _keyComparer;
        private readonly Subject<bool> _rowPosUpdatedSubject;

        public SortedTable(IReactiveTable sourceTable, string sortColumnId, IComparer<T> comparer)
        {
            _sourceTable = sourceTable;
            _sortColumnId = sortColumnId;
            _comparer = comparer;
            _keyComparer = new KeyComparer<T>(_comparer);
            _keysToRows = new List<KeyValuePair<T, int>>(_sourceTable.RowCount);
            _rowIdsToValues = new Dictionary<int, T>(_sourceTable.RowCount);

            _subject = new Subject<TableUpdate>();
            _rowPosUpdatedSubject = new Subject<bool>();
            RowPositionsUpdated = _rowPosUpdatedSubject;

            _token = _sourceTable.ReplayAndSubscribe(OnNext);
        }

        private void OnNext(TableUpdate update)
        {
            var sortColValue = _sourceTable.GetValue<T>(_sortColumnId, update.RowIndex);
            var keyValuePair = new KeyValuePair<T, int>(sortColValue, update.RowIndex);

            bool needToResort = false;
            int sortedRowId = -1;
            switch (update.Action)
            {
                case TableUpdate.TableUpdateAction.Add:
                    _keysToRows.Add(keyValuePair);
                    _rowIdsToValues.Add(update.RowIndex, sortColValue);
                    needToResort = true;
                    break;
                case TableUpdate.TableUpdateAction.Delete:
                    sortedRowId = _keysToRows.BinarySearch(keyValuePair, _keyComparer);
                    _keysToRows.RemoveAt(sortedRowId);
                    _rowIdsToValues.Remove(update.RowIndex);
                    needToResort = true;
                    break;
                case TableUpdate.TableUpdateAction.Update:
                    {
                        // Sort column updating
                        if (update.Columns.Any(column => column.ColumnId == _sortColumnId))
                        {
                            var oldValue = _rowIdsToValues[update.RowIndex];
                            var oldSortColValue = new KeyValuePair<T, int>(oldValue, update.RowIndex);
                            _keysToRows[_keysToRows.BinarySearch(oldSortColValue, _keyComparer)] = keyValuePair;
                            _rowIdsToValues[update.RowIndex] = sortColValue;
                            needToResort = true;
                        }
                        // Other column - row can't change position
                        else
                        {
                            sortedRowId = _keysToRows.BinarySearch(keyValuePair, _keyComparer);
                        }
                    }
                    break;
            }

            // Keep the table sorted
            if (needToResort)
            {
                _keysToRows.Sort((pair1, pair2) => _comparer.Compare(pair1.Key, pair2.Key));
                sortedRowId = _keysToRows.BinarySearch(keyValuePair, _keyComparer);
            }
            // Find the now row id
            else if (sortedRowId < 0)
            {
                sortedRowId = _keysToRows.BinarySearch(keyValuePair, _keyComparer);
            }

            // Propagate the update
            _subject.OnNext(new TableUpdate(update.Action, sortedRowId, update.Columns));

            if (needToResort)
            {
                _rowPosUpdatedSubject.OnNext(true);
            }
        }

        private class KeyComparer<T1> : IComparer<KeyValuePair<T, int>>
        {
            private readonly IComparer<T> _comparer;

            public KeyComparer(IComparer<T> comparer)
            {
                _comparer = comparer;
            }

            public int Compare(KeyValuePair<T, int> x, KeyValuePair<T, int> y)
            {
                return _comparer.Compare(x.Key, y.Key);
            }
        }

        public IDisposable Subscribe(IObserver<TableUpdate> observer)
        {
            return _subject.Subscribe(observer);
        }

        public IReactiveColumn AddColumn(IReactiveColumn column)
        {
            throw new NotImplementedException();
        }

        public T GetValue<T>(string columnId, int rowIndex)
        {
            var sourceRowId = _keysToRows[rowIndex].Value;
            return _sourceTable.GetValue<T>(columnId, sourceRowId);
        }

        public object GetValue(string columnId, int rowIndex)
        {
            var sourceRowId = _keysToRows[rowIndex].Value;
            return _sourceTable.GetValue(columnId, sourceRowId);
        }

        public int RowCount { get { return _keysToRows.Count; } }

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
            var rowAdds = new List<TableUpdate>(_keysToRows.Count);
            rowAdds.AddRange(_keysToRows.Select(row => new TableUpdate(TableUpdate.TableUpdateAction.Add, row.Value)));
            foreach (var rowAdd in rowAdds)
            {
                observer.OnNext(rowAdd);
            }
        }

        public int GetRowAt(int position)
        {
            if (position > 0 && position < _keysToRows.Count)
            {
                return position;
            }
            return -1;
        }

        public int GetPositionOfRow(int rowIndex)
        {
            return rowIndex;
        }

        public void Dispose()
        {
            if (_token != null) _token.Dispose();
            if (_subject != null) _subject.Dispose();
            if (_rowPosUpdatedSubject != null) _rowPosUpdatedSubject.Dispose();
        }

        public IObservable<bool> RowPositionsUpdated { get; private set; }
    }
}
