using System;
using System.Collections.Generic;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Utils;
using System.Linq;

namespace ReactiveTables.Framework.Joins
{

    public struct JoinedRow
    {
        public int? LeftRowId;
        public List<JoinRows> Rows;
    }

    public struct JoinRows
    {
        public int RowId;
        public int? LeftRowId;
        public int? RightRowId;
    }

    public class InnerJoin<TKey> : IReactiveTableJoiner, IObserver<ColumnUpdate>
    {
        private readonly IReactiveTable _leftTable;
        private readonly IReactiveTable _rightTable;
        private int _lastRowIndex = -1;

//        private readonly List<JoinRows> _joinRowsToKeys = new List<JoinRows>(); 
        private readonly Dictionary<TKey, JoinEntry<TKey>> _keys = new Dictionary<TKey, JoinEntry<TKey>>();
        private readonly Dictionary<int, int> _leftColumnRowsToJoinRows = new Dictionary<int, int>();
        private readonly Dictionary<int, int> _rightColumnRowsToJoinRows = new Dictionary<int, int>();
        private readonly IReactiveColumn _leftColumn;
        private readonly IReactiveColumn _rightColumn;

        private readonly Dictionary<TKey, JoinedRow> _rowsByKey = new Dictionary<TKey, JoinedRow>();
        private readonly List<JoinRows> _rows = new List<JoinRows>();


        public int RowCount
        {
            get { return _rows.Count; }
        }

        public InnerJoin(IReactiveTable leftTable, string leftIdColumn, IReactiveTable rightTable, string rightIdColumn)
        {
            _leftTable = leftTable;
            _rightTable = rightTable;

            _leftColumn = leftTable.Columns[leftIdColumn];
            _rightColumn = rightTable.Columns[rightIdColumn];

            leftTable.Subscribe(this);
            rightTable.Subscribe(this);

            var leftKeyManager = new JoinKeyManager<TKey>(_rows, _rowsByKey, _leftColumnRowsToJoinRows, JoinKeyManager<TKey>.JoinSide.Left);
            var rightKeyManager = new JoinKeyManager<TKey>(_rows, _rowsByKey, _rightColumnRowsToJoinRows, JoinKeyManager<TKey>.JoinSide.Right);
            leftTable.Subscribe(leftKeyManager);
            rightTable.Subscribe(rightKeyManager);
        }

        public int GetRowIndex(IReactiveColumn column, int joinRowIndex)
        {
            if (_leftTable.Columns.ContainsKey(column.ColumnId) 
                && _leftTable.Columns[column.ColumnId] == column)
            {
                return _rows[joinRowIndex].LeftRowId.GetValueOrDefault(-1);
            }

            if (_rightTable.Columns.ContainsKey(column.ColumnId) 
                && _rightTable.Columns[column.ColumnId] == column)
            {
                return _rows[joinRowIndex].RightRowId.GetValueOrDefault(-1);
            }

            return -1;
        }

        /// <summary>
        /// Keep the key dictionaries up to date when the key columns are updated.
        /// </summary>
        /// <param name="update"></param>
        public void OnNext(ColumnUpdate update)
        {
            // Key update
            int columnRowIndex = update.RowIndex;
            if (update.Column == _leftColumn)
            {
                UpdateLeftSide(columnRowIndex);
            }

            if (update.Column == _rightColumn)
            {
                UpdateRightSide(columnRowIndex);
            }
        }

        private void UpdateLeftSide(int columnRowIndex)
        {
            var key = _leftTable.GetValue<TKey>(_leftColumn.ColumnId, columnRowIndex);
            AddNewJoinEntry(key, columnRowIndex);

            JoinedRow keyRows;
            int joinedRowId = -1;
            if (_rowsByKey.TryGetValue(key, out keyRows))
            {
                // Try to join to an empty one
                int unlinkedIndex = keyRows.Rows.FindIndex(r => !r.LeftRowId.HasValue);
                if (unlinkedIndex >= 0)
                {
                    for (int i = unlinkedIndex; i < keyRows.Rows.Count; i++)
                    {
                        var unlinked = keyRows.Rows[i];
                        unlinked.LeftRowId = columnRowIndex;
                        keyRows.Rows[i] = unlinked;
                        joinedRowId = unlinked.RowId;
                    }
                }
                    // No empties - add a new one
                else
                {
                    var joinRows = new JoinRows {RowId = _rows.Count};

                    // Join it to the left side
                    joinRows.LeftRowId = keyRows.LeftRowId;

                    _rows.Add(joinRows);
                    keyRows.Rows.Add(joinRows);
                    joinedRowId = joinRows.RowId;
                }
            }
            else
            {
                // First time this key has been used so create a new row
                keyRows = new JoinedRow {LeftRowId = columnRowIndex, Rows = new List<JoinRows>()};
                var joinRows = new JoinRows {LeftRowId = columnRowIndex, RowId = _rows.Count};
                _rows.Add(joinRows);
                keyRows.Rows.Add(joinRows);
                joinedRowId = joinRows.RowId;
            }

            _rowsByKey[key] = keyRows;

            // Update the reverse lookup
            _leftColumnRowsToJoinRows[columnRowIndex] = joinedRowId;
            /*

            // Create our virtual rows which are the max of the row counts of the two tables
            if (columnRowIndex >= _joinRowsToKeys.Count) _joinRowsToKeys.Add(new JoinRows {LeftRowId = columnRowIndex});
            else
            {
                JoinRows joinRows = _joinRowsToKeys[columnRowIndex];
                joinRows.LeftRowId = columnRowIndex;
                _joinRowsToKeys[columnRowIndex] = joinRows;
            }

            var joinEntry = _keys[key];
            if (!joinEntry.LeftRowIndexes.Contains(columnRowIndex))
            {
                joinEntry.LeftRowIndexes.Add(columnRowIndex);
                _leftColumnRowsToJoinRows.Add(columnRowIndex, joinEntry.RowIndex);
                _keys[key] = joinEntry;
            }*/
        }

        private void UpdateRightSide(int columnRowIndex)
        {
            // TODO: needs refactoring
            var key = _rightTable.GetValue<TKey>(_rightColumn.ColumnId, columnRowIndex);
            AddNewJoinEntry(key, columnRowIndex);

            JoinedRow keyRows;
            int joinedRowId = -1;
            // Key already exists
            if (_rowsByKey.TryGetValue(key, out keyRows))
            {
                // Try to join to an empty one
                int emptyIndex = keyRows.Rows.FindIndex(r => !r.RightRowId.HasValue);
                if (emptyIndex >= 0)
                {
                    var unlinked = keyRows.Rows[emptyIndex];
                    unlinked.RightRowId = columnRowIndex;
                    keyRows.Rows[emptyIndex] = unlinked;
                    _rows[unlinked.RowId] = unlinked;
                    joinedRowId = unlinked.RowId;
                }
                    // No empties - add a new one
                else
                {
                    var joinRows = new JoinRows { RowId = _rows.Count, RightRowId = columnRowIndex };

                    // Join it to the left side
                    joinRows.LeftRowId = keyRows.LeftRowId;

                    _rows.Add(joinRows);
                    keyRows.Rows.Add(joinRows);
                    joinedRowId = joinRows.RowId;
                }
            }
            else
            {
                // First time this key has been used so create a new row
                keyRows = new JoinedRow { Rows = new List<JoinRows>() };
                var joinRows = new JoinRows { RightRowId = columnRowIndex, RowId = _rows.Count };
                _rows.Add(joinRows);
                keyRows.Rows.Add(joinRows);
                joinedRowId = joinRows.RowId;
            }

            _rowsByKey[key] = keyRows;

            // Update the reverse lookup
            _rightColumnRowsToJoinRows[columnRowIndex] = joinedRowId;
        }

        private void AddNewJoinEntry(TKey key, int columnRowIndex)
        {
            if (!_keys.ContainsKey(key))
            {
                var entry = new JoinEntry<TKey> {Key = key, RowIndex = _lastRowIndex++};
                _keys.Add(key, entry);
            }
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }
}