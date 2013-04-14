using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveTables.Framework.Joins
{
    /// <summary>
    /// Used for keeping the key dictionaries up to date when rows are removed
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    internal class JoinRowDeleteHandler<TKey> : IObserver<RowUpdate>
    {
        private readonly List<Join<TKey>.Row?> _rows;
        private readonly Dictionary<TKey, Join<TKey>.ColumnRowMapping> _rowsByKey;
        private readonly Dictionary<int, HashSet<int>> _columnRowsToJoinRows;
        private readonly Dictionary<int, HashSet<int>> _otherColumnRowsToJoinRows;
        private readonly JoinSide _joinSide;
        private readonly JoinType _joinType;
        private readonly Queue<int> _deletedRowIds;
        private readonly List<IObserver<RowUpdate>> _observers = new List<IObserver<RowUpdate>>();

        public JoinRowDeleteHandler(List<Join<TKey>.Row?> rows, Dictionary<TKey, Join<TKey>.ColumnRowMapping> rowsByKey,
            Dictionary<int, HashSet<int>> columnRowsToJoinRows, Dictionary<int, HashSet<int>> otherColumnRowsToJoinRows, 
            JoinSide joinSide, JoinType joinType, Queue<int> deletedRowIds)
        {
            _rows = rows;
            _rowsByKey = rowsByKey;
            _columnRowsToJoinRows = columnRowsToJoinRows;
            _otherColumnRowsToJoinRows = otherColumnRowsToJoinRows;
            _joinSide = joinSide;
            _joinType = joinType;
            _deletedRowIds = deletedRowIds;
        }

        public void OnNext(RowUpdate update)
        {
            if (update.Action == RowUpdate.RowUpdateAction.Delete)
            {
                OnDelete(update);
            }
        }

        private void OnDelete(RowUpdate update)
        {
            var joinRowIds = _columnRowsToJoinRows[update.RowIndex];
            if (joinRowIds.Count < 0) return;
            
            List<RowUpdate> rowUpdates = new List<RowUpdate>();

            var firstJoinRowId = joinRowIds.First();
            TKey key = _rows[firstJoinRowId].Value.Key;
            var colRowMappings = _rowsByKey[key].ColRowMappings;

            List<int> joinRowIdsCopy = joinRowIds.ToList();
            foreach (var joinRowId in joinRowIdsCopy)
            {
                var row = _rows[joinRowId];
                if (!row.HasValue) throw new NotImplementedException();

                /* 
                 * Delete our side (column to row and reverse) and then for the other side:
                 * 1. If the other side is unlinked then delete the row
                 * 2. If there are entries with the same col row id then delete the row
                 * 3. Otherwise just unlink
                 */

                // -- or -- 
                
                /* 
                 * 1. Blank our side
                 * 2. If the other side has more than one entry for its row id then blank it
                 * 3. For all rows where both sides are blank delete the row
                 */

                // Always delete when we're in an inner join
                if (_joinType == JoinType.Inner)
                {
                    DeleteRow(update, joinRowId, colRowMappings, _joinSide, rowUpdates, row.Value);
                }
                else /* Outer join */
                {
                    DeleteOuterJoin2(row, joinRowId, colRowMappings, update);
                }
            }

            // 3. For all rows where both sides are blank delete the row
            DeleteClearedRows(colRowMappings, rowUpdates, key, joinRowIdsCopy);

            if (rowUpdates.Count > 0)
            {
                UpdateObservers(rowUpdates);
            }
        }

        private void DeleteOuterJoin2(Join<TKey>.Row? row, int joinRowId, List<Join<TKey>.Row> colRowMappings, RowUpdate update)
        {
            // Blank our side
            BlankSide(joinRowId, colRowMappings, _joinSide, update.RowIndex);

            // 2. If the other side has more than one entry for its row id then blank it
            var otherSideIndex = GetOtherSideIndex(row.Value, _joinSide);
            var isLastRowForColumnRowIndex = IsLastRowForColumnRowIndex(colRowMappings, _joinSide.GetOtherSide(), otherSideIndex);

            if (!isLastRowForColumnRowIndex)
            {
                BlankOtherSide(joinRowId, colRowMappings, _joinSide.GetOtherSide(), GetOtherSideIndex(row.Value, _joinSide), update.RowIndex);
            }
        }

        private void DeleteClearedRows(List<Join<TKey>.Row> colRowMappings, List<RowUpdate> rowUpdates, TKey key,
            IEnumerable<int> joinRowIds)
        {
            for (int i = colRowMappings.Count - 1; i >= 0; i--)
            {
                var mapping = colRowMappings[i];
                if (!mapping.LeftRowId.HasValue && !mapping.RightRowId.HasValue)
                {
                    colRowMappings.RemoveAt(i);
                }
            }

            foreach (var joinRowId in joinRowIds)
            {
                var joinRow = _rows[joinRowId];
                if (!joinRow.Value.LeftRowId.HasValue && !joinRow.Value.RightRowId.HasValue)
                {
                    _rows[joinRowId] = null;
                    rowUpdates.Add(new RowUpdate(joinRowId, RowUpdate.RowUpdateAction.Delete));
                    _deletedRowIds.Enqueue(joinRowId);
                }
            }

            // If there are no mappings left than forget this key
            if (_rowsByKey[key].ColRowMappings.Count == 0)
            {
                _rowsByKey.Remove(key);
            }
        }

        private void BlankSide(int joinRowId, List<Join<TKey>.Row> colRowMappings, JoinSide joinSide, int rowIndex)
        {
            for (int i = 0; i < colRowMappings.Count; i++)
            {
                Join<TKey>.Row mapping = colRowMappings[i];
                // When doing the other side we need to check the other side rowId and this side's rowId
                if (IsRow(mapping, joinSide, rowIndex))
                {
                    RemoveReverseMapping(mapping, joinSide, joinSide == _joinSide ? _columnRowsToJoinRows : _otherColumnRowsToJoinRows);
                    if (joinSide == JoinSide.Left) mapping.LeftRowId = null;
                    else mapping.RightRowId = null;
                    colRowMappings[i] = mapping;
                }
            }

            Join<TKey>.Row rowValue = _rows[joinRowId].Value;
            if (joinSide == JoinSide.Left) rowValue.LeftRowId = null; else rowValue.RightRowId = null;
            _rows[joinRowId] = rowValue;
        }

        
        private void BlankOtherSide(int joinRowId, List<Join<TKey>.Row> colRowMappings, JoinSide joinSide, int rowIndex, int otherRowIndex)
        {
            for (int i = 0; i < colRowMappings.Count; i++)
            {
                Join<TKey>.Row mapping = colRowMappings[i];
                // When doing the other side we need to check the other side rowId and this side's rowId
                if (IsRow(mapping, joinSide, rowIndex, otherRowIndex))
                {
                    RemoveReverseMapping(mapping, joinSide, joinSide == _joinSide ? _columnRowsToJoinRows : _otherColumnRowsToJoinRows);
                    if (joinSide == JoinSide.Left) mapping.LeftRowId = null;
                    else mapping.RightRowId = null;
                    colRowMappings[i] = mapping;
                }
            }

            Join<TKey>.Row rowValue = _rows[joinRowId].Value;
            if (joinSide == JoinSide.Left) rowValue.LeftRowId = null; else rowValue.RightRowId = null;
            _rows[joinRowId] = rowValue;
        }

        private static void RemoveReverseMapping(Join<TKey>.Row mapping, JoinSide joinSide, Dictionary<int, HashSet<int>> columnRowsToJoinRows)
        {
            if (joinSide == JoinSide.Left)
            {
                columnRowsToJoinRows[mapping.LeftRowId.Value].Remove(mapping.RowId.Value);
                if (columnRowsToJoinRows[mapping.LeftRowId.Value].Count == 0) columnRowsToJoinRows.Remove(mapping.LeftRowId.Value);
            }
            else
            {
                columnRowsToJoinRows[mapping.RightRowId.Value].Remove(mapping.RowId.Value);
                if (columnRowsToJoinRows[mapping.RightRowId.Value].Count == 0) columnRowsToJoinRows.Remove(mapping.RightRowId.Value);
            }
        }

/*
        private void DeleteOuterJoin1(RowUpdate update, Join<TKey>.Row? row, int joinRowId, List<Join<TKey>.Row> colRowMappings,
            List<RowUpdate> rowUpdates, TKey key)
        {
//                    if (isLastRowForColumnRowIndex)
//                    {
            // Remove row
            if (OtherSideIsUnlinked(row.Value, _joinSide))
            {
                DeleteRow(update, joinRowId, colRowMappings, _joinSide, rowUpdates, row.Value);
                _rowsByKey.Remove(key);
            }
                // Unlink row
            else
            {
                var otherSideIndex = GetOtherSideIndex(row.Value, _joinSide);
                var isLastRowForColumnRowIndex = IsLastRowForColumnRowIndex(colRowMappings, _joinSide, otherSideIndex);
                if (isLastRowForColumnRowIndex)
                {
                    _rows[joinRowId] = UnlinkRow(row.Value, _joinSide);
                    ClearColRowMapping(colRowMappings, _joinSide, update.RowIndex);
                }
                else
                {
                    DeleteRow(update, joinRowId, colRowMappings, _joinSide, rowUpdates, row.Value);
                }
            }
//                    }
//                    else
//                    {
            // Remove row
//                        DeleteRow(update, joinRowId, colRowMappings, _joinSide, rowUpdates, row.Value);
//                    }
        }
*/

        private int GetOtherSideIndex(Join<TKey>.Row row, JoinSide joinSide)
        {
            return joinSide == JoinSide.Left ? row.RightRowId.GetValueOrDefault(-1) : row.LeftRowId.GetValueOrDefault(-1);
        }

        private static bool IsLastRowForColumnRowIndex(IEnumerable<Join<TKey>.Row> colRowMappings, JoinSide joinSide, int rowIndex)
        {
            if (rowIndex < 0) return true;
            if (joinSide == JoinSide.Left)
            {
                return colRowMappings.Count(m => m.LeftRowId == rowIndex) <= 1;
            }
            return colRowMappings.Count(m => m.RightRowId == rowIndex) <= 1;
        }

        private void UpdateObservers(List<RowUpdate> rowUpdates)
        {
            foreach (var observer in _observers)
            {
                foreach (var rowUpdate in rowUpdates)
                {
                    observer.OnNext(rowUpdate);
                }
            }
        }

/*
        private bool OtherSideIsUnlinked(Join<TKey>.Row row, JoinSide joinSide)
        {
            return joinSide == JoinSide.Left ? !row.RightRowId.HasValue : !row.LeftRowId.HasValue;
        }
*/

        private void DeleteRow(RowUpdate update, int joinRowId, List<Join<TKey>.Row> colRowMappings, JoinSide joinSide, 
            List<RowUpdate> rowUpdates, Join<TKey>.Row row)
        {
            // Remove the reverse mapping on the other side too.
            if ((joinSide == JoinSide.Left && row.RightRowId.HasValue) || row.LeftRowId.HasValue)
            {
                var otherRowId = joinSide == JoinSide.Left ? row.RightRowId.Value : row.LeftRowId.Value;
                _otherColumnRowsToJoinRows[otherRowId].RemoveWhere(r => r == row.RowId);
            }

            _rows[joinRowId] = null;
            _deletedRowIds.Enqueue(joinRowId);
            ClearColRowMapping(colRowMappings, joinSide, update.RowIndex);

            rowUpdates.Add(new RowUpdate(joinRowId, RowUpdate.RowUpdateAction.Delete));
        }

        private static void ClearColRowMapping(List<Join<TKey>.Row> colRowMappings, JoinSide joinSide, int rowIndex)
        {
            for (int i = 0; i < colRowMappings.Count; i++)
            {
                Join<TKey>.Row mapping = colRowMappings[i];
                if (IsRow(mapping, joinSide, rowIndex))
                {
                    if (joinSide == JoinSide.Left) mapping.LeftRowId = null;
                    else mapping.RightRowId = null;
                    colRowMappings[i] = mapping;
                }
            }
        }

        private static bool IsRow(Join<TKey>.Row row, JoinSide joinSide, int rowIndex)
        {
            return joinSide == JoinSide.Left ? row.LeftRowId == rowIndex : row.RightRowId == rowIndex;
        }

        private static bool IsRow(Join<TKey>.Row row, JoinSide joinSide, int rowIndex, int otherRowIndex)
        {
            return joinSide == JoinSide.Left
                       ? row.LeftRowId == rowIndex && row.RightRowId == null
                       : row.RightRowId == rowIndex && row.LeftRowId == null;
        }

        private static Join<TKey>.Row UnlinkRow(Join<TKey>.Row row, JoinSide joinSide)
        {
            if (joinSide == JoinSide.Left) 
                row.LeftRowId = null;
            else 
                row.RightRowId = null;

            return row;
        }


        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        public void AddRowObserver(IObserver<RowUpdate> observer)
        {
            _observers.Add(observer);
        }
    }
}