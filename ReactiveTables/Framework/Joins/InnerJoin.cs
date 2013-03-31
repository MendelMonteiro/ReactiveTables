using System;
using System.Collections.Generic;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Joins
{
    internal class InnerJoin<TKey> : IReactiveTableJoiner, IObserver<ColumnUpdate>
    {
        private readonly IReactiveTable _leftTable;
        private readonly IReactiveTable _rightTable;
        private int _lastRowIndex = -1;

        private readonly Dictionary<TKey, JoinEntry<TKey>> _keys = new Dictionary<TKey, JoinEntry<TKey>>();
        private readonly Dictionary<int, TKey> _leftKeysReverse = new Dictionary<int, TKey>();
        private readonly Dictionary<int, TKey> _rightKeysReverse = new Dictionary<int, TKey>();
        private readonly IReactiveColumn _leftColumn;
        private readonly IReactiveColumn _rightColumn;

        public InnerJoin(IReactiveTable leftTable, string leftIdColumn, IReactiveTable rightTable, string rightIdColumn)
        {
            _leftTable = leftTable;
            _rightTable = rightTable;

            _leftColumn = leftTable.Columns[leftIdColumn];
            _rightColumn = rightTable.Columns[rightIdColumn];

            leftTable.Subscribe(this);
            rightTable.Subscribe(this);

            var leftKeyManager = new KeyManager<TKey>(_keys, _leftKeysReverse, KeyManager<TKey>.Side.Left);
            var rightKeyManager = new KeyManager<TKey>(_keys, _rightKeysReverse, KeyManager<TKey>.Side.Right);
            leftTable.Subscribe(leftKeyManager);
            rightTable.Subscribe(rightKeyManager);
        }

        public int RowCount { get; private set; }
        public int GetRowIndex(IReactiveColumn column, int rowIndex)
        {
            if (_leftTable.Columns.ContainsKey(column.ColumnId) 
                && _leftTable.Columns[column.ColumnId] == column
                && rowIndex < _leftKeysReverse.Count)
            {
                var key = _leftKeysReverse[rowIndex];
                var joinEntry = _keys[key];
                return joinEntry.LeftRowIndex.GetValueOrDefault(-1);
            }
            if (_rightTable.Columns.ContainsKey(column.ColumnId) 
                && _rightTable.Columns[column.ColumnId] == column 
                && rowIndex < _rightKeysReverse.Count)
            {
                var key = _rightKeysReverse[rowIndex];
                var joinEntry = _keys[key];
                return joinEntry.RightRowIndex.GetValueOrDefault(-1);
            }

            return -1;
        }

        /// <summary>
        /// Keep the key dictionaries up to date when the key columns are updated.
        /// </summary>
        /// <param name="value"></param>
        public void OnNext(ColumnUpdate value)
        {
            // Key update
            int rowIndex = value.RowIndex;
            if (value.Column == _leftColumn)
            {
                SetJoinEntry(value, rowIndex, _leftTable, _leftKeysReverse,
                             (lastRowIndex, key, leftRowIndex) => new JoinEntry<TKey>(key, lastRowIndex, leftRowIndex, null),
                             (leftRowIndex, entry) => {
                                                          entry.LeftRowIndex = leftRowIndex;
                                                          return entry;
                             });
            }
            if (value.Column == _rightColumn)
            {
                SetJoinEntry(value, rowIndex, _rightTable, _rightKeysReverse,
                             (lastRowIndex, key, rightRowIndex) => new JoinEntry<TKey>(key, lastRowIndex, null, rightRowIndex),
                             (rightRowIndex, entry) =>
                                 {
                                     entry.RightRowIndex = rightRowIndex;
                                     return entry;
                                 });
            }
        }

        private void SetJoinEntry(ColumnUpdate value, int rowIndex, IReactiveTable table, Dictionary<int, TKey> keysReverse,
                                  Func<int, TKey, int, JoinEntry<TKey>> createJoinEntry, Func<int, JoinEntry<TKey>, JoinEntry<TKey>> setJoinEntryRowIndex)
        {
            var key = table.GetValue<TKey>(value.Column.ColumnId, rowIndex);
            JoinEntry<TKey> joinEntry;
            if (!_keys.TryGetValue(key, out joinEntry))
            {
                _lastRowIndex++;
                _keys.Add(key, createJoinEntry(rowIndex, key, _lastRowIndex));
            }
            else
            {
                joinEntry = setJoinEntryRowIndex(rowIndex, joinEntry);
                _keys[key] = joinEntry; // Overwrite as it's a struct.
            }
            keysReverse[rowIndex] = key;
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }



    struct JoinEntry<TKey>
    {
        public int RowIndex;
        public TKey Key;
        public int? LeftRowIndex;
        public int? RightRowIndex;

        public JoinEntry(TKey key, int rowIndex, int? leftRowIndex, int? rightRowIndex)
        {
            Key = key;
            RowIndex = rowIndex;
            LeftRowIndex = leftRowIndex;
            RightRowIndex = rightRowIndex;
        }
    }

    /// <summary>
    /// Used for keeping the key dictionaries up to date when rows are removed
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    internal class KeyManager<TKey> : IObserver<RowUpdate>
    {
        private readonly Dictionary<TKey, JoinEntry<TKey>> _keys;
        private readonly Dictionary<int, TKey> _keysReverse;
        private readonly Side _side;

        public enum Side
        {
            Left,
            Right
        }

        public KeyManager(Dictionary<TKey, JoinEntry<TKey>> keys, Dictionary<int, TKey> keysReverse, Side side)
        {
            _keys = keys;
            _keysReverse = keysReverse;
            _side = side;
        }

        public void OnNext(RowUpdate value)
        {
            if (value.Action == RowUpdate.RowUpdateAction.Delete)
            {
                if (!_keysReverse.ContainsKey(value.RowIndex))
                    return;

                var key = _keysReverse[value.RowIndex];
                var joinEntry = _keys[key];

                if (_side == Side.Left)
                    joinEntry.LeftRowIndex = null;
                else
                    joinEntry.RightRowIndex = null;

                // Now the rows in both tables don't exist so clear our key store.
                if (joinEntry.RightRowIndex == null && joinEntry.LeftRowIndex == null)
                {
                    _keysReverse.Remove(value.RowIndex);
                    _keys.Remove(key);
                }
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