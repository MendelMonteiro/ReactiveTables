using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Filters;
using ReactiveTables.Framework.Joins;
using System.Linq;

namespace ReactiveTables.Framework.Sorting
{
    /// <summary>
    /// A sorted table
    /// </summary>
    public interface ISortedTable
    {
        /// <summary>
        /// An observable stream of events which notify when the row positions are updated
        /// </summary>
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
    public class SortedTable : IReactiveTable, ISortedTable, IDisposable 
    {
        /// <summary>
        /// Used to notify outside observers
        /// </summary>
        private readonly Subject<TableUpdate> _subject;

        private readonly IReactiveTable _sourceTable;
        private ISorter _sorter;
        private IDisposable _token;
        private readonly Subject<bool> _rowPosUpdatedSubject;
        private readonly Lazy<PropertyChangedNotifier> _changeNotifier;

        /// <summary>
        /// Create a new sorted table
        /// </summary>
        /// <param name="sourceTable"></param>
        public SortedTable(IReactiveTable sourceTable)
        {
            _sourceTable = sourceTable;
            _sorter = new DefaultSorter(sourceTable);

            _subject = new Subject<TableUpdate>();
            _rowPosUpdatedSubject = new Subject<bool>();
            RowPositionsUpdated = _rowPosUpdatedSubject;

            _token = _sourceTable.ReplayAndSubscribe(OnNext);

            _changeNotifier = new Lazy<PropertyChangedNotifier>(() => new PropertyChangedNotifier(this));
        }

        public void SortBy<T>(string columnId) where T : IComparable<T>
        {
            SortBy(columnId, Comparer<T>.Default);
        }

        public void SortBy<T>(string columnId, IComparer<T> comparer) where T : IComparable<T>
        {
            if (_sorter != null)
            {
                DeleteAllRows();
            }

            _sorter = new Sorter<T>(_sourceTable, columnId, comparer);
            if (_token != null) _token.Dispose();
            _token = _sourceTable.ReplayAndSubscribe(OnNext);
        }

        private void DeleteAllRows()
        {
            for (int i = 0; i < _sorter.RowCount; i++)
            {
                var delete = new TableUpdate(TableUpdate.TableUpdateAction.Delete, i);
                _subject.OnNext(delete);
            }
        }

        private void OnNext(TableUpdate update)
        {
            bool needToResort;
            var sortedRowId = _sorter.OnNext(update, out needToResort);

            // Propagate the update
            _subject.OnNext(new TableUpdate(update.Action, sortedRowId, update.Columns));

            if (needToResort)
            {
                _rowPosUpdatedSubject.OnNext(true);
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
            var sourceRowId = _sorter.GetRowAt(rowIndex);
            return _sourceTable.GetValue<T>(columnId, sourceRowId);
        }

        public object GetValue(string columnId, int rowIndex)
        {
            var sourceRowId = _sorter.GetRowAt(rowIndex);
            return _sourceTable.GetValue(columnId, sourceRowId);
        }

        public int RowCount { get { return _sorter.RowCount; } }

        public IDictionary<string, IReactiveColumn> Columns { get { return _sourceTable.Columns; } }

        public IReactiveColumn GetColumnByIndex(int index)
        {
            return _sourceTable.GetColumnByIndex(index);
        }

        public PropertyChangedNotifier ChangeNotifier
        {
            get { return _changeNotifier.Value; }
        }

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
            var rowAdds = new List<TableUpdate>(_sorter.RowCount);
            rowAdds.AddRange(_sorter.GetAllRows().Select(row => new TableUpdate(TableUpdate.TableUpdateAction.Add, row)));
            foreach (var rowAdd in rowAdds)
            {
                observer.OnNext(rowAdd);
            }
        }

        public int GetRowAt(int position)
        {
            if (position > 0 && position < _sorter.RowCount)
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

    class DefaultSorter: ISorter
    {
        private readonly IReactiveTable _sourceTable;
        public DefaultSorter(IReactiveTable sourceTable)
        {
            _sourceTable = sourceTable;
        }

        public int OnNext(TableUpdate update, out bool needToResort)
        {
            needToResort = false;
            return 1;
        }

        public int GetRowAt(int position)
        {
            return _sourceTable.GetRowAt(position);
        }

        public int RowCount { get { return _sourceTable.RowCount; } }

        public IEnumerable<int> GetAllRows()
        {
            throw new NotImplementedException();
//                return _sourceTable.
        }
    }

    internal class KeyComparer<T> : IComparer<KeyValuePair<T, int>>
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

    internal interface ISorter
    {
        int OnNext(TableUpdate update, out bool needToResort);
        int GetRowAt(int position);
        int RowCount { get; }
        IEnumerable<int> GetAllRows();
    }

    internal class Sorter<T> : ISorter where T : IComparable<T>
    {
        /// <summary>
        /// Used to keep the values present before an update to the key
        /// </summary>
        private readonly Dictionary<int, T> _rowIdsToValues;
        /// <summary>
        /// List of all the keys which is kept sorted
        /// </summary>
        private readonly List<KeyValuePair<T, int>> _keysToRows;

        private readonly IReactiveTable _sourceTable;
        private readonly IComparer<T> _comparer;
        private readonly string _sortColumnId;
        private readonly KeyComparer<T> _keyComparer;

        public Sorter(IReactiveTable sourceTable, string sortColumnId, IComparer<T> comparer)
        {
            _sourceTable = sourceTable;
            _comparer = comparer;
            _sortColumnId = sortColumnId;
            _keyComparer = new KeyComparer<T>(_comparer);
            _keysToRows = new List<KeyValuePair<T, int>>(sourceTable.RowCount);
            _rowIdsToValues = new Dictionary<int, T>(sourceTable.RowCount);
        }

        public int OnNext(TableUpdate update, out bool needToResort)
        {
            var sortColValue = _sourceTable.GetValue<T>(_sortColumnId, update.RowIndex);
            var keyValuePair = new KeyValuePair<T, int>(sortColValue, update.RowIndex);

            needToResort = false;
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

            return sortedRowId;
        }

        public int GetRowAt(int position)
        {
            return _keysToRows[position].Value;
        }

        public int RowCount { get { return _keysToRows.Count; } }

        public IEnumerable<int> GetAllRows()
        {
            return _keysToRows.Select(pair => pair.Value);
        }
    }
}
