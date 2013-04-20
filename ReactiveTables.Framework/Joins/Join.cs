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
using System.Diagnostics;
using ReactiveTables.Framework.Columns;
using System.Linq;
using ReactiveTables.Utils;

namespace ReactiveTables.Framework.Joins
{
    public enum JoinType
    {
        FullOuter,
        Inner,
        LeftOuter,
        RightOuter
    }

    public class Join<TKey> : IReactiveTableJoiner, IObserver<ColumnUpdate>
    {
        public class ColumnRowMapping
        {
            public List<Row> ColRowMappings;

            public override string ToString()
            {
                return string.Format("ColRowMappings: {0}", ColRowMappings);
            }
        }

        public struct Row
        {
            public int? RowId;
            public int? LeftRowId;
            public int? RightRowId;
            public TKey Key;

            public override string ToString()
            {
                return string.Format("RowId: {0}, LeftRowId: {1}, RightRowId: {2}, Key: {3}", RowId, LeftRowId, RightRowId, Key);
            }
        }
        
        private readonly IReactiveTable _leftTable;
        private readonly IReactiveTable _rightTable;
        private readonly IReactiveColumn _leftColumn;
        private readonly IReactiveColumn _rightColumn;

        private readonly Dictionary<int, HashSet<int>> _leftColumnRowsToJoinRows = new Dictionary<int, HashSet<int>>();
        private readonly Dictionary<int, HashSet<int>> _rightColumnRowsToJoinRows = new Dictionary<int, HashSet<int>>();
        
        private readonly Dictionary<TKey, ColumnRowMapping> _rowsByKey = new Dictionary<TKey, ColumnRowMapping>();
        private readonly List<Row?> _rows = new List<Row?>();

        private readonly List<IObserver<RowUpdate>> _rowUpdateObservers = new List<IObserver<RowUpdate>>();
        
        private readonly JoinType _joinType;
        private readonly JoinRowDeleteHandler<TKey> _leftRowDeleteHandler;
        private readonly JoinRowDeleteHandler<TKey> _rightRowDeleteHandler;
        private readonly Queue<int> _deletedRowIds = new Queue<int>();  

        public int RowCount
        {
            get { return _rows.Count - _deletedRowIds.Count; }
        }

        public Join(IReactiveTable leftTable, string leftIdColumn, IReactiveTable rightTable, string rightIdColumn, JoinType joinType = JoinType.FullOuter)
        {
            _leftTable = leftTable;
            _rightTable = rightTable;
            _joinType = joinType;

            _leftColumn = leftTable.Columns[leftIdColumn];
            _rightColumn = rightTable.Columns[rightIdColumn];

            leftTable.Subscribe(this);
            rightTable.Subscribe(this);

            _leftRowDeleteHandler = new JoinRowDeleteHandler<TKey>(_rows, _rowsByKey, _leftColumnRowsToJoinRows, _rightColumnRowsToJoinRows, JoinSide.Left, _joinType, _deletedRowIds);
            _rightRowDeleteHandler = new JoinRowDeleteHandler<TKey>(_rows, _rowsByKey, _rightColumnRowsToJoinRows, _leftColumnRowsToJoinRows, JoinSide.Right, _joinType, _deletedRowIds);
            leftTable.Subscribe(_leftRowDeleteHandler);
            rightTable.Subscribe(_rightRowDeleteHandler);
        }

        public int GetRowIndex(IReactiveColumn column, int joinRowIndex)
        {
            if (joinRowIndex >= _rows.Count) return -1;

            Row? row = _rows[joinRowIndex];
            if (_leftTable.Columns.ContainsKey(column.ColumnId) 
                && _leftTable.Columns[column.ColumnId] == column
                && row.HasValue)
            {
                int valueOrDefault = row.Value.LeftRowId.GetValueOrDefault(-1);
                return valueOrDefault;
            }

            if (_rightTable.Columns.ContainsKey(column.ColumnId) 
                && _rightTable.Columns[column.ColumnId] == column
                && row.HasValue)
            {
                return row.Value.RightRowId.GetValueOrDefault(-1);
            }

            return -1;
        }

        public void AddRowObserver(IObserver<RowUpdate> observer)
        {
            _rowUpdateObservers.Add(observer);
            _leftRowDeleteHandler.AddRowObserver(observer);
            _rightRowDeleteHandler.AddRowObserver(observer);
        }

        /// <summary>
        /// Keep the key dictionaries up to date when the key columns are updated.
        /// </summary>
        /// <param name="update"></param>
        public void OnNext(ColumnUpdate update)
        {
            // Key update
            int columnRowIndex = update.RowIndex;
            List<int> updatedRows = null;
            if (update.Column == _leftColumn)
            {
                updatedRows = OnColumnUpdate(columnRowIndex, JoinSide.Left);
            }
            else if (update.Column == _rightColumn)
            {
                updatedRows = OnColumnUpdate(columnRowIndex, JoinSide.Right);
            }
            if (updatedRows != null && updatedRows.Count > 0)
            {
                UpdateRowObserversAdd(updatedRows);
            }
        }

        private void UpdateRowObserversAdd(List<int> updatedRows)
        {
            foreach (var updatedRow in updatedRows)
            {
                var rowUpdate = new RowUpdate(updatedRow, RowUpdate.RowUpdateAction.Add);
                foreach (var observer in _rowUpdateObservers)
                {
                    observer.OnNext(rowUpdate);
                }
            }
        }

        private List<int> OnColumnUpdate(int columnRowIndex, JoinSide side)
        {
            List<int> updateRows = new List<int>();
            IReactiveTable table = side == JoinSide.Left ? _leftTable : _rightTable;
            IReactiveColumn column = side == JoinSide.Left ? _leftColumn : _rightColumn;
            var key = table.GetValue<TKey>(column.ColumnId, columnRowIndex);

            ColumnRowMapping keyRowsMapping;
            int? joinedRowId;
            // Has the key been used before?
            Dictionary<int, HashSet<int>> otherColumnRowsToJoinRows = side == JoinSide.Left ? _rightColumnRowsToJoinRows : _leftColumnRowsToJoinRows;
            Dictionary<int, HashSet<int>> columnRowsToJoinRows = side == JoinSide.Left ? _leftColumnRowsToJoinRows : _rightColumnRowsToJoinRows;
            if (_rowsByKey.TryGetValue(key, out keyRowsMapping))
            {
                HashSet<int> existingRowIndeces;
                // Existing left row
                if (columnRowsToJoinRows.TryGetValue(columnRowIndex, out existingRowIndeces))
                {
                    // Same key as before
                    if (keyRowsMapping.ColRowMappings.Any(FindKeyByRow(columnRowIndex, side)))
                    {
                        // Do nothing
                    }
                    // Key has changed
                    else
                    {
                        foreach (var rowId in existingRowIndeces)
                        {
                            var oldKey = _rows[rowId].Value.Key;
                            keyRowsMapping = MoveKeyEntry(_rows, rowId, oldKey, key,
                                                          columnRowsToJoinRows, keyRowsMapping, _rowsByKey);
                        }
                    }
                }
                // New left row
                else
                {
                    // Right rows exist with no mapping - try to join to an unlinked one
                    int unlinkedIndex = keyRowsMapping.ColRowMappings.FindIndex(IsRowUnlinked(side));
                    if (unlinkedIndex >= 0)
                    {
                        for (int i = unlinkedIndex; i < keyRowsMapping.ColRowMappings.Count; i++)
                        {
                            // Link the left to the joined row.
                            var unlinked = keyRowsMapping.ColRowMappings[i];
                            LinkRow(ref unlinked, columnRowIndex, side);

                            // If it's an inner join we need to create the new row here (when we link)
                            if (!unlinked.RowId.HasValue)
                            {
                                joinedRowId = GetNewRowId();

                                unlinked.RowId = joinedRowId; // Can't forget to update the existing row object with the new id
                                AddNewRow(unlinked, updateRows, joinedRowId.Value);

                                // Need to update the reverse mapping for the other side too as it wasn't done before
                                var otherRowId = side == JoinSide.Left ? unlinked.RightRowId : unlinked.LeftRowId;
                                otherColumnRowsToJoinRows.AddNewIfNotExists(otherRowId.Value).Add(joinedRowId.Value);
                            }
                            else
                            {
                                joinedRowId = unlinked.RowId;                                
                            }

                            keyRowsMapping.ColRowMappings[i] = unlinked;
                            _rows[joinedRowId.Value] = unlinked;

                            // Update the reverse lookup
                            columnRowsToJoinRows.AddNewIfNotExists(columnRowIndex).Add(joinedRowId.Value);
                        }
                    }
                    // No unlinked - add new mappings and rows for each right row
                    else
                    {
                        var otherRows = GetDistinctOtherRows(keyRowsMapping, side);
                        foreach (var otherRowId in otherRows.ToArray())
                        {
                            joinedRowId = GetNewRowId();
                            var joinRow = CreateNewLinkedJoinRow(columnRowIndex, joinedRowId.Value, key, otherRowId, side);
                            keyRowsMapping.ColRowMappings.Add(joinRow);

                            AddNewRow(joinRow, updateRows, joinedRowId.Value);

                            // Update the reverse lookup
                            columnRowsToJoinRows.AddNewIfNotExists(columnRowIndex).Add(joinedRowId.Value);
                            otherColumnRowsToJoinRows.AddNewIfNotExists(otherRowId.Value).Add(joinedRowId.Value);
                        }
                    }
                }
            }
            else
            {
                // First time this key has been used so create a new (unlinked on right side)
                keyRowsMapping = new ColumnRowMapping {ColRowMappings = new List<Row>()};
                bool shouldAddUnlinkedRow = ShouldAddUnlinkedRow(side);

                joinedRowId = shouldAddUnlinkedRow ? GetNewRowId() : (int?)null;
                var joinRow = CreateNewJoinRow(columnRowIndex, key, joinedRowId, side);
                keyRowsMapping.ColRowMappings.Add(joinRow);

                if (shouldAddUnlinkedRow)
                {
                    AddNewRow(joinRow, updateRows, joinedRowId.Value);

                    // Update the reverse lookup
                    columnRowsToJoinRows.AddNewIfNotExists(columnRowIndex).Add(joinedRowId.Value);
                }
            }

            _rowsByKey[key] = keyRowsMapping;

            return updateRows;
        }

        private int GetNewRowId()
        {
            if (_deletedRowIds.Count > 0)
            {
                return _deletedRowIds.Dequeue();
            }
            return _rows.Count();
        }

        private bool ShouldAddUnlinkedRow(JoinSide side)
        {
            if (_joinType == JoinType.Inner) return false;
            if (_joinType == JoinType.LeftOuter) return side == JoinSide.Left;
            if (_joinType == JoinType.RightOuter) return side == JoinSide.Right;
            return true;
        }

        private void AddNewRow(Row joinRow, List<int> updateRows, int joinedRowId)
        {
            if (joinedRowId == _rows.Count)
            {
                _rows.Add(joinRow);
            }
            else
            {
                _rows[joinedRowId] = joinRow;
            }
            updateRows.Add(joinedRowId);
        }

        private static Row CreateNewJoinRow(int columnRowIndex, TKey key, int? joinedRowId, JoinSide side)
        {
            if (side == JoinSide.Left)
            {
                return new Row { Key = key, LeftRowId = columnRowIndex, RowId = joinedRowId };
            }
            return new Row { Key = key, RightRowId = columnRowIndex, RowId = joinedRowId };
        }

        private static IEnumerable<int?> GetDistinctOtherRows(ColumnRowMapping keyRowsMapping, JoinSide side)
        {
            if (side == JoinSide.Left)
            {
                return keyRowsMapping.ColRowMappings.Where(r => r.RightRowId.HasValue).Select(r => r.RightRowId).Distinct();
            }

            return keyRowsMapping.ColRowMappings.Where(r => r.LeftRowId.HasValue).Select(r => r.LeftRowId).Distinct();
        }

        private static Row CreateNewLinkedJoinRow(int columnRowIndex, int joinedRowId, TKey key, int? otherRowId, JoinSide side)
        {
            if (side == JoinSide.Left)
            {
                return new Row { RowId = joinedRowId, Key = key, LeftRowId = columnRowIndex, RightRowId = otherRowId };
            }

            return new Row { RowId = joinedRowId, Key = key, RightRowId = columnRowIndex, LeftRowId = otherRowId };            
        }

        private static void LinkRow(ref Row unlinked, int columnRowIndex, JoinSide side)
        {
            if (side == JoinSide.Left)
            {
                unlinked.LeftRowId = columnRowIndex;
            }
            else
            {
                unlinked.RightRowId = columnRowIndex;
            }
        }

        private static Predicate<Row> IsRowUnlinked(JoinSide side)
        {
            if (side == JoinSide.Left) return r => !r.LeftRowId.HasValue;

            return r => !r.RightRowId.HasValue;
        }

        private static Func<Row, bool> FindKeyByRow(int columnRowIndex, JoinSide side)
        {
            if (side == JoinSide.Left) return r => r.LeftRowId == columnRowIndex;
            
            return r => r.RightRowId == columnRowIndex;
        }
        
        private ColumnRowMapping MoveKeyEntry(List<Row?> rows, int existingRowIndex, TKey oldKey, 
            TKey newKey, Dictionary<int, HashSet<int>> columnRowsToJoinRows, 
            ColumnRowMapping keyRowsMapping, Dictionary<TKey, ColumnRowMapping> rowsByKey)
        {
            throw new NotImplementedException();
            // Clear all matching left key entries
            // If inner mode optionally remove the rows too
            // Run the usual process for creating a mapping with the new key (including new rows being added)
            // Note: What will happen with non contiguous row ids!?  Should I set to null for the time being?
            //      The index will no longer match the RowId.  Set to null and keep a list(queue) of old keys to reuse?


            // Undo the mapping on the existing column mapping
            rowsByKey[oldKey].ColRowMappings.FirstOrDefault();

            // Redo the mapping for the new column mapping


            // Change the key on the Row entry
            var row = rows[existingRowIndex].Value;
            row.Key = newKey;
            rows[existingRowIndex] = row;
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }
    }
}
