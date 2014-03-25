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
along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Joins
{
    /// <summary>
    /// Used for keeping the key dictionaries up to date when rows are removed
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    internal class JoinRowDeleteHandler<TKey> : IObserver<TableUpdate>
    {
        private readonly List<Join<TKey>.Row?> _rows;
        private readonly Dictionary<TKey, Join<TKey>.ColumnRowMapping> _rowsByKey;
        private readonly Dictionary<int, HashSet<int>> _columnRowsToJoinRows;
        private readonly Dictionary<int, HashSet<int>> _otherColumnRowsToJoinRows;
        private readonly JoinSide _joinSide;
        private readonly JoinType _joinType;
        private readonly FieldRowManager _rowManager;
        private IObserver<TableUpdate> _observers;

        public JoinRowDeleteHandler(List<Join<TKey>.Row?> rows, Dictionary<TKey, Join<TKey>.ColumnRowMapping> rowsByKey,
            Dictionary<int, HashSet<int>> columnRowsToJoinRows, Dictionary<int, HashSet<int>> otherColumnRowsToJoinRows, 
            JoinSide joinSide, JoinType joinType, FieldRowManager rowManager)
        {
            _rows = rows;
            _rowsByKey = rowsByKey;
            _columnRowsToJoinRows = columnRowsToJoinRows;
            _otherColumnRowsToJoinRows = otherColumnRowsToJoinRows;
            _joinSide = joinSide;
            _joinType = joinType;
            _rowManager = rowManager;
        }

        public void OnNext(TableUpdate update)
        {
            if (update.Action == TableUpdateAction.Delete)
            {
                OnDelete(update);
            }
        }

        private void OnDelete(TableUpdate update)
        {
            // If there is no mapping the joined row no longer exists and there's nothing to update.
            if (!_columnRowsToJoinRows.ContainsKey(update.RowIndex)) return;

            var joinRowIds = _columnRowsToJoinRows[update.RowIndex];
            if (joinRowIds.Count < 0) return;
            
            List<TableUpdate> rowUpdates = new List<TableUpdate>();

            // Get the key and row id to use from the first element
            var firstJoinRowId = joinRowIds.First();
            TKey key = _rows[firstJoinRowId].Value.Key;
            var colRowMappings = _rowsByKey[key].ColRowMappings;

            // Make a copy as we'll modify the original inside the loop
            List<int> joinRowIdsCopy = joinRowIds.ToList();

            /* 
             * 1. Blank our side
             * 2. If the other side has more than one entry for its row id then blank it
             * 3. For all rows where both sides are blank delete the row
             */
            foreach (var joinRowId in joinRowIdsCopy)
            {
                var row = _rows[joinRowId];
                if (!row.HasValue) throw new NotImplementedException();

                DeleteRowMapping(row, joinRowId, colRowMappings, update);
            }

            // 3. For all rows where both sides are blank delete the row
            DeleteClearedRows(colRowMappings, rowUpdates, key, joinRowIdsCopy);

            if (rowUpdates.Count > 0)
            {
                UpdateObservers(rowUpdates);
            }
        }

        private void DeleteRowMapping(Join<TKey>.Row? row, int joinRowId, List<Join<TKey>.Row> colRowMappings, TableUpdate update)
        {
            // Blank our side
            BlankSide(joinRowId, colRowMappings, _joinSide, update.RowIndex);

            // 2. If the other side has more than one entry for its row id then blank it
            var otherSideIndex = GetOtherSideIndex(row.Value, _joinSide);
            var isLastRowForColumnRowIndex = IsLastRowForColumnRowIndex(colRowMappings, _joinSide.GetOtherSide(), otherSideIndex, _joinType);

            if (!isLastRowForColumnRowIndex)
            {
                BlankOtherSide(joinRowId, colRowMappings, _joinSide.GetOtherSide(), GetOtherSideIndex(row.Value, _joinSide));
            }
        }

        private void DeleteClearedRows(List<Join<TKey>.Row> colRowMappings, List<TableUpdate> rowUpdates, TKey key,
            IEnumerable<int> joinRowIds)
        {
            // Remove the mappings where we don't have values on either side
            for (int i = colRowMappings.Count - 1; i >= 0; i--)
            {
                var mapping = colRowMappings[i];
                if (!mapping.LeftRowId.HasValue && !mapping.RightRowId.HasValue)
                {
                    colRowMappings.RemoveAt(i);
                }
            }

            // Where we don't have values for the left or right hand sides we need to decrement the row count
            foreach (var joinRowId in joinRowIds)
            {
                var joinRow = _rows[joinRowId].Value;
                if (_joinType == JoinType.Inner)
                {
                    // Inner joins should leave the row in place but as soon as one side dissapears the row will no longer be visible to consumers
                    if (!joinRow.LeftRowId.HasValue ^ !joinRow.RightRowId.HasValue)
                    {
                        // Clear the row ids from the Join state
                        joinRow.RowId = null;
                        _rows[joinRowId] = joinRow;
                        for (int i = 0; i < _rowsByKey[key].ColRowMappings.Count; i++)
                        {
                            var mapping = _rowsByKey[key].ColRowMappings[i];
                            mapping.RowId = null;
                            _rowsByKey[key].ColRowMappings[i] = mapping;
                        }
                        // Actually delete the row
                        rowUpdates.Add(new TableUpdate(TableUpdateAction.Delete, joinRowId));
                        _rowManager.DeleteRow(joinRowId);
                    }
                }
                else
                {
                    if (!joinRow.LeftRowId.HasValue && !joinRow.RightRowId.HasValue)
                    {
                        _rows[joinRowId] = null;
                        rowUpdates.Add(new TableUpdate(TableUpdateAction.Delete, joinRowId));
                        _rowManager.DeleteRow(joinRowId);
                    }
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

        
        private void BlankOtherSide(int joinRowId, List<Join<TKey>.Row> colRowMappings, JoinSide joinSide, int rowIndex)
        {
            for (int i = 0; i < colRowMappings.Count; i++)
            {
                Join<TKey>.Row mapping = colRowMappings[i];
                // When doing the other side we need to check the other side rowId and this side's rowId
                if (IsRowForOtherSide(mapping, joinSide, rowIndex))
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

        private static void RemoveReverseMapping(Join<TKey>.Row mapping, JoinSide joinSide, 
            Dictionary<int, HashSet<int>> columnRowsToJoinRows)
        {
            if (joinSide == JoinSide.Left)
            {
                var leftRowsToJoinRow = columnRowsToJoinRows[mapping.LeftRowId.Value];
                if (mapping.RowId.HasValue)
                {
                    leftRowsToJoinRow.Remove(mapping.RowId.Value);
                }
                else
                {
                    leftRowsToJoinRow.Clear();
                }
                if (leftRowsToJoinRow.Count == 0) columnRowsToJoinRows.Remove(mapping.LeftRowId.Value);
            }
            else
            {
                var rightRowsToJoinRow = columnRowsToJoinRows[mapping.RightRowId.Value];
                if (mapping.RowId.HasValue)
                {
                    rightRowsToJoinRow.Remove(mapping.RowId.Value);
                }
                else
                {
                    rightRowsToJoinRow.Clear();
                }

                if (rightRowsToJoinRow.Count == 0) columnRowsToJoinRows.Remove(mapping.RightRowId.Value);
            }
        }


        private int GetOtherSideIndex(Join<TKey>.Row row, JoinSide joinSide)
        {
            return joinSide == JoinSide.Left ? row.RightRowId.GetValueOrDefault(-1) : row.LeftRowId.GetValueOrDefault(-1);
        }

        private static bool IsLastRowForColumnRowIndex(IEnumerable<Join<TKey>.Row> colRowMappings,
            JoinSide joinSide, int rowIndex, JoinType joinType)
        {
            if (rowIndex < 0) return true;

            // When it's an inner join we never keep the last row
//            if (joinType == JoinType.Inner) return false;

            if (joinSide == JoinSide.Left)
            {
                // Don't keep the last row when if it's a sided outer join and we're on the other side
                if (joinType == JoinType.RightOuter) return false;

                return colRowMappings.Count(m => m.LeftRowId == rowIndex) <= 1;
            }

            // Don't keep the last row when if it's a sided outer join and we're on the other side
            if (joinType == JoinType.LeftOuter) return false;
            return colRowMappings.Count(m => m.RightRowId == rowIndex) <= 1;
        }

        private void UpdateObservers(IEnumerable<TableUpdate> rowUpdates)
        {
            foreach (var rowUpdate in rowUpdates)
            {
                _observers.OnNext(rowUpdate);
            }
        }

        private static bool IsRow(Join<TKey>.Row row, JoinSide joinSide, int rowIndex)
        {
            return joinSide == JoinSide.Left ? row.LeftRowId == rowIndex : row.RightRowId == rowIndex;
        }

        private static bool IsRowForOtherSide(Join<TKey>.Row row, JoinSide joinSide, int rowIndex)
        {
            return joinSide == JoinSide.Left
                       ? row.LeftRowId == rowIndex && row.RightRowId == null
                       : row.RightRowId == rowIndex && row.LeftRowId == null;
        }

        public void OnError(Exception error)
        {
        }

        public void OnCompleted()
        {
        }

        public void AddRowObserver(IObserver<TableUpdate> observer)
        {
            _observers = observer;
        }

        public void RemoveRowObserver(IObserver<TableUpdate> observer)
        {
            _observers = null;
        }
    }
}
