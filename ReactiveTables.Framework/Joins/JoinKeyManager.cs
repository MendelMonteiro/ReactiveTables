using System;
using System.Collections.Generic;

namespace ReactiveTables.Framework.Joins
{
    /// <summary>
    /// Used for keeping the key dictionaries up to date when rows are removed
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    internal class JoinKeyManager<TKey> : IObserver<RowUpdate>
    {
        private readonly List<JoinRows> _rows;
        private readonly Dictionary<TKey, JoinedRow> _rowsByKey;

        /// <summary>
        /// Table row id -> Joined row id
        /// </summary>
        private readonly Dictionary<int, int> _rowIdReverse;
        private readonly List<JoinEntry<TKey>> _joinEntries;
        private readonly JoinSide _joinSide;

        public enum JoinSide
        {
            Left,
            Right
        }

        public JoinKeyManager(List<JoinRows> rows, Dictionary<TKey, JoinedRow> rowsByKey, Dictionary<int, int> keysReverse, JoinSide joinSide)
        {
            _rows = rows;
            _rowsByKey = rowsByKey;
            _rowIdReverse = keysReverse;
            _joinSide = joinSide;
        }

        public void OnNext(RowUpdate update)
        {
            if (update.Action == RowUpdate.RowUpdateAction.Add)
            {
                OnAdd(update);
            }

            if (update.Action == RowUpdate.RowUpdateAction.Delete)
            {
                OnDelete(update);
            }
        }

        private void OnAdd(RowUpdate update)
        {
/*            int columnRow = update.RowIndex;
            // Add a new join entry if 
            if (columnRow >= _joinEntries.Count)
            {
                var joinEntry = new JoinEntry<TKey>();
                if (_joinSide == JoinSide.Left) joinEntry.LeftRowIndexes = columnRow;
                if (_joinSide == JoinSide.Right) joinEntry.RightRowIndexes = columnRow;
                _joinEntries.Add(joinEntry);
            }
            _rowIdReverse.Add(columnRow, _joinEntries.Count - 1);*/
        }

        private void OnDelete(RowUpdate update)
        {
            /*if (!_rowIdReverse.ContainsKey(update.RowIndex))
                return;

            var joinRowId = _rowIdReverse[update.RowIndex];
            var row = _rows[joinRowId];
            JoinEntry<TKey> joinEntry = _joinEntries[joinRowId];

            if (_joinSide == JoinSide.Left)
                row.LeftRowId = null;
            else
                row.RightRowId = null;

            // Now the rows in both tables don't exist so clear our key store.
            if (row.RightRowId == null && row.LeftRowId == null)
            {
                _rows.Remove(row);
                _rowsByKey[]

                _rowIdReverse.Remove(update.RowIndex);
                _joinEntries.RemoveAt(joinRowId);
                // TODO: perf issue as we do a scan to remove if the cardinality is 1 to huge N this will be slow
                _rows[joinEntry.Key].Remove(joinRowId);
            }*/
        }


        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }
    }
}