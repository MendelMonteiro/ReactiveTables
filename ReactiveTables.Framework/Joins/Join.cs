// This file is part of ReactiveTables.
// 
// ReactiveTables is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReactiveTables is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Utils;
using System.Reactive.Linq;

namespace ReactiveTables.Framework.Joins
{
    public enum JoinType
    {
        FullOuter,
        Inner,
        LeftOuter,
        RightOuter
    }

    /// <summary>
    /// Represents a DB like join which can handle, Inner, Left, Right and Full Outer joins.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    public class Join<TKey> : IReactiveTableJoiner
    {
        #region Internal structures
        internal class ColumnRowMapping
        {
            public List<Row> ColRowMappings;

            public override string ToString()
            {
                return string.Format("ColRowMappings: {0}", ColRowMappings);
            }
        }

        /// <summary>
        /// The row can have three states:
        /// 1. It exists with only one side and the row id - when an outer join is used
        /// 2. It exists with only one side and _no_ row id - when an inner join is used
        /// 3. IT exists with both sides and the row id - when the row is fully joined for both inner and outer joins
        /// </summary>
        internal struct Row
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

        private struct RowToUpdate
        {
            public enum RowUpdateType
            {
                Add,
                Link
            }

            public int RowIndex { get; set; }
            public RowUpdateType Type { get; set; }
        }
        #endregion

        private readonly IReactiveTable _leftTable;
        private readonly IReactiveTable _rightTable;
        private readonly IReactiveColumn _leftColumn;
        private readonly IReactiveColumn _rightColumn;

        private readonly Dictionary<int, HashSet<int>> _leftColumnRowsToJoinRows = new Dictionary<int, HashSet<int>>();
        private readonly Dictionary<int, HashSet<int>> _rightColumnRowsToJoinRows = new Dictionary<int, HashSet<int>>();

        private readonly Dictionary<TKey, ColumnRowMapping> _rowsByKey = new Dictionary<TKey, ColumnRowMapping>();
        private readonly List<Row?> _rows = new List<Row?>();

        private IObserver<TableUpdate> _updateObservers;

        private readonly JoinType _joinType;
        private readonly JoinRowDeleteHandler<TKey> _leftRowDeleteHandler;
        private readonly JoinRowDeleteHandler<TKey> _rightRowDeleteHandler;
        private readonly FieldRowManager _rowManager = new FieldRowManager();

        private readonly IDisposable _leftColToken;
        private readonly IDisposable _rightColToken;
        private readonly IDisposable _rightRowToken;
        private readonly IDisposable _leftRowToken;

        private readonly bool _replaying = true;

        public int RowCount
        {
            get { return _rowManager.RowCount; }
        }

        public IEnumerable<int> GetRows()
        {
            return _rowManager.GetRows();
        }

        /// <summary>
        /// Create a new join.
        /// </summary>
        /// <param name="leftTable"></param>
        /// <param name="leftIdColumn">The column from the left table to join on</param>
        /// <param name="rightTable"></param>
        /// <param name="rightIdColumn">The column from the right table to join on</param>
        /// <param name="joinType">The type of the join</param>
        public Join(IReactiveTable leftTable, string leftIdColumn, IReactiveTable rightTable, string rightIdColumn, 
                    JoinType joinType = JoinType.FullOuter)
        {
            _leftTable = leftTable;
            _rightTable = rightTable;
            _joinType = joinType;

            _leftColumn = leftTable.Columns[leftIdColumn];
            _rightColumn = rightTable.Columns[rightIdColumn];

            _leftColToken = leftTable.ReplayAndSubscribe(OnNextLeft);
            _rightColToken = rightTable.ReplayAndSubscribe(OnNextRight);

            _replaying = false;

            _leftRowDeleteHandler = new JoinRowDeleteHandler<TKey>(
                _rows, _rowsByKey, _leftColumnRowsToJoinRows, _rightColumnRowsToJoinRows, JoinSide.Left, _joinType, _rowManager);
            _rightRowDeleteHandler = new JoinRowDeleteHandler<TKey>(
                _rows, _rowsByKey, _rightColumnRowsToJoinRows, _leftColumnRowsToJoinRows, JoinSide.Right, _joinType, _rowManager);

            _leftRowToken = leftTable.RowUpdates().Subscribe(_leftRowDeleteHandler);
            _rightRowToken = rightTable.RowUpdates().Subscribe(_rightRowDeleteHandler);
        }

        public int GetRowIndex(IReactiveColumn column, int joinRowIndex)
        {
            if (joinRowIndex >= _rows.Count) return -1;

            Row? row = _rows[joinRowIndex];
            if (_leftTable.Columns.ContainsKey(column.ColumnId)
                && _leftTable.Columns[column.ColumnId] == column
                && row.HasValue)
            {
                return row.Value.LeftRowId.GetValueOrDefault(-1);
            }

            if (_rightTable.Columns.ContainsKey(column.ColumnId)
                && _rightTable.Columns[column.ColumnId] == column
                && row.HasValue)
            {
                return row.Value.RightRowId.GetValueOrDefault(-1);
            }

            return -1;
        }

        public void SetObserver(IObserver<TableUpdate> observer)
        {
            _updateObservers = observer;
            _leftRowDeleteHandler.AddRowObserver(observer);
            _rightRowDeleteHandler.AddRowObserver(observer);
        }

        public int GetRowAt(int position)
        {
            return _rowManager.GetRowAt(position);
        }

        public int GetPositionOfRow(int rowIndex)
        {
            return _rowManager.GetPositionOfRow(rowIndex);
        }

        private void OnNextLeft(TableUpdate update)
        {
            OnNext(update, JoinSide.Left);
        }

        private void OnNextRight(TableUpdate update)
        {
            OnNext(update, JoinSide.Right);
        }
        
        /// <summary>
        /// Keep the key dictionaries up to date when the key columns are updated.
        /// </summary>
        /// <param name="update"></param>
        private void OnNext(TableUpdate update, JoinSide side)
        {
            // Filter out add/deletes
            if (update.Action == TableUpdate.TableUpdateAction.Delete) return;

            // If we have an add and which is a replay of the underlying tables then we need to
            // simulate column updates for all columns
            if (update.Action == TableUpdate.TableUpdateAction.Add)
            {
                if (!_replaying) return;

                var columns = GetTableColumns(side);
                foreach (var column in columns)
                {
                    update = new TableUpdate(TableUpdate.TableUpdateAction.Update, update.RowIndex, column);
                    ProcessColumnUpdate(update, column);
                }
            }
            else
            {
                // Key update
                ProcessColumnUpdate(update, update.Column);
            }
        }

        private IList<IReactiveColumn> GetTableColumns(JoinSide side)
        {
            var table = GetTableForSide(side);
            return table.Columns.Values.ToList();
        }

        private void ProcessColumnUpdate(TableUpdate update, IReactiveColumn column)
        {
            JoinSide side;
            int columnRowIndex = update.RowIndex;
            if (column == _leftColumn)
            {
                side = JoinSide.Left;
            }
            else if (column == _rightColumn)
            {
                side = JoinSide.Right;
            }
            else
            {
                // Column upates after tables are joined - if this column is joined propagate the update, otherwise ignore
                var propagated = PropagateColumnUpdates(update, columnRowIndex, _leftTable, _leftColumnRowsToJoinRows, JoinSide.Left);
                propagated = propagated || PropagateColumnUpdates(update, columnRowIndex, _rightTable, _rightColumnRowsToJoinRows, JoinSide.Right);

                return;
            }

            // Processing a join column
            var updatedRows = OnColumnUpdate(columnRowIndex, side);
            if (updatedRows != null && updatedRows.Count > 0)
            {
                UpdateRowObserversAdd(updatedRows, column, side);
            }
        }

        private bool PropagateColumnUpdates(TableUpdate update,
                                            int columnRowIndex,
                                            IReactiveTable table,
                                            Dictionary<int, HashSet<int>> columnRowsToJoinRows,
                                            JoinSide side)
        {
            HashSet<int> joinRows;
            if (table.Columns.ContainsKey(update.Column.ColumnId)
                && columnRowsToJoinRows.TryGetValue(columnRowIndex, out joinRows))
            {
                foreach (var joinRow in joinRows)
                {
                    var row = _rows[joinRow];
                    if (row.HasValue && row.Value.RowId.HasValue && !IsRowUnlinked(row.Value, side))
                    {
                        var colUpdate = new TableUpdate(TableUpdate.TableUpdateAction.Update, joinRow, update.Column);
                        if (_updateObservers != null) _updateObservers.OnNext(colUpdate);
                        return true;
                    }
                }
            }
            return false;
        }

        private void UpdateRowObserversAdd(IEnumerable<RowToUpdate> updatedRows, IReactiveColumn column, JoinSide side)
        {
            foreach (var updatedRow in updatedRows)
            {
                // Update that the new row exists
                if (updatedRow.Type == RowToUpdate.RowUpdateType.Add)
                {
                    var rowUpdate = new TableUpdate(TableUpdate.TableUpdateAction.Add, updatedRow.RowIndex);
                    if (_updateObservers != null) _updateObservers.OnNext(rowUpdate);

                    // Update all columns for newly added row on the sides that are present.
                    var row = _rows[updatedRow.RowIndex];
                    if (row.HasValue && row.Value.LeftRowId.HasValue) SendColumnUpdates(JoinSide.Left, updatedRow.RowIndex);
                    if (row.HasValue && row.Value.RightRowId.HasValue) SendColumnUpdates(JoinSide.Right, updatedRow.RowIndex);
                }
                else
                {
                    // Update all rows on the side that has been added and also on the other side if also joined
                    SendColumnUpdates(side, updatedRow.RowIndex);
                }
            }
        }

        private void SendColumnUpdates(JoinSide side, int rowIndex)
        {
            var table = GetTableForSide(side);
            foreach (var tableColumn in table.Columns)
            {
                var columnUpdate = new TableUpdate(TableUpdate.TableUpdateAction.Update, rowIndex, tableColumn.Value);
                if (_updateObservers != null) _updateObservers.OnNext(columnUpdate);
            }
        }

        private IReactiveTable GetTableForSide(JoinSide side)
        {
            return side == JoinSide.Left ? _leftTable : _rightTable;
        }

        private List<RowToUpdate> OnColumnUpdate(int columnRowIndex, JoinSide side)
        {
            List<RowToUpdate> updateRows = new List<RowToUpdate>();
            IReactiveTable table = side == JoinSide.Left ? _leftTable : _rightTable;
            IReactiveColumn column = side == JoinSide.Left ? _leftColumn : _rightColumn;
            var key = table.GetValue<TKey>(column.ColumnId, columnRowIndex);

            ColumnRowMapping keyRowsMapping;
            // Has the key been used before?
            Dictionary<int, HashSet<int>> otherColumnRowsToJoinRows = side == JoinSide.Left ? _rightColumnRowsToJoinRows : _leftColumnRowsToJoinRows;
            Dictionary<int, HashSet<int>> columnRowsToJoinRows = side == JoinSide.Left ? _leftColumnRowsToJoinRows : _rightColumnRowsToJoinRows;
            if (_rowsByKey.TryGetValue(key, out keyRowsMapping))
            {
                HashSet<int> existingRowIndeces;
                // Existing row
                if (columnRowsToJoinRows.TryGetValue(columnRowIndex, out existingRowIndeces))
                {
                    UpdateExistingRow(columnRowIndex, side, keyRowsMapping, existingRowIndeces, key, columnRowsToJoinRows);
                }
                    // New row
                else
                {
                    UpdateNewRow(columnRowIndex, side, keyRowsMapping, updateRows, key, otherColumnRowsToJoinRows, columnRowsToJoinRows);
                }
            }
                // New key
            else
            {
                keyRowsMapping = UpdateNewKey(columnRowIndex, side, key, updateRows, columnRowsToJoinRows);
            }

            _rowsByKey[key] = keyRowsMapping;

            return updateRows;
        }

        private void UpdateExistingRow(int columnRowIndex,
                                       JoinSide side,
                                       ColumnRowMapping keyRowsMapping,
                                       HashSet<int> existingRowIndeces,
                                       TKey key,
                                       Dictionary<int, HashSet<int>> columnRowsToJoinRows)
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
                    // TODO: Need to possibly alter other keyRowMappings - not possible with current method signature.
                    keyRowsMapping = MoveKeyEntry(_rows, rowId, oldKey, key,
                                                  columnRowsToJoinRows, keyRowsMapping, _rowsByKey);
                }
            }
        }

        private void UpdateNewRow(int columnRowIndex,
                                  JoinSide side,
                                  ColumnRowMapping keyRowsMapping,
                                  List<RowToUpdate> updateRows,
                                  TKey key,
                                  Dictionary<int, HashSet<int>> otherColumnRowsToJoinRows,
                                  Dictionary<int, HashSet<int>> columnRowsToJoinRows)
        {
            // Other side rows exist with no mapping - try to join to an unlinked one
            int unlinkedIndex = keyRowsMapping.ColRowMappings.FindIndex(IsRowUnlinked(side));
            if (unlinkedIndex >= 0)
            {
                for (int i = unlinkedIndex; i < keyRowsMapping.ColRowMappings.Count; i++)
                {
                    LinkNewItemToUnlinkedRows(columnRowIndex, side, keyRowsMapping, updateRows, otherColumnRowsToJoinRows, columnRowsToJoinRows, i);
                }
            }
                // No unlinked - add new mappings and rows for each row on the other side
            else
            {
                AddNewRowMapping(columnRowIndex, side, keyRowsMapping, updateRows, key, otherColumnRowsToJoinRows, columnRowsToJoinRows);
            }
        }

        private void LinkNewItemToUnlinkedRows(int columnRowIndex,
                                               JoinSide side,
                                               ColumnRowMapping keyRowsMapping,
                                               List<RowToUpdate> updateRows,
                                               Dictionary<int, HashSet<int>> otherColumnRowsToJoinRows,
                                               Dictionary<int, HashSet<int>> columnRowsToJoinRows,
                                               int i)
        {
            int? joinedRowId;
            // Link the row to the joined row.
            var rowToLink = keyRowsMapping.ColRowMappings[i];
            LinkRow(ref rowToLink, columnRowIndex, side);

            // We need to create the new row here (when we link)
            if (!rowToLink.RowId.HasValue)
            {
                joinedRowId = GetNewRowId();

                rowToLink.RowId = joinedRowId; // Can't forget to update the existing row object with the new id
                AddNewRow(rowToLink, updateRows, joinedRowId.Value);

                // Need to update the reverse mapping for the other side too as it wasn't done before
                var otherRowId = side == JoinSide.Left ? rowToLink.RightRowId : rowToLink.LeftRowId;
                otherColumnRowsToJoinRows.AddNewIfNotExists(otherRowId.Value).Add(joinedRowId.Value);
            }
            else
            {
                updateRows.Add(new RowToUpdate{RowIndex = rowToLink.RowId.Value, Type = RowToUpdate.RowUpdateType.Link});
                joinedRowId = rowToLink.RowId;
            }

            keyRowsMapping.ColRowMappings[i] = rowToLink;
            _rows[joinedRowId.Value] = rowToLink;

            // Update the reverse lookup
            columnRowsToJoinRows.AddNewIfNotExists(columnRowIndex).Add(joinedRowId.Value);
        }

        private void AddNewRowMapping(int columnRowIndex,
                                      JoinSide side,
                                      ColumnRowMapping keyRowsMapping,
                                      List<RowToUpdate> updateRows,
                                      TKey key,
                                      Dictionary<int, HashSet<int>> otherColumnRowsToJoinRows,
                                      Dictionary<int, HashSet<int>> columnRowsToJoinRows)
        {
            // For each distinct row on the other side we need to create a new row and link ourselves
            var otherRows = GetDistinctOtherRows(keyRowsMapping, side);
            int?[] otherRowIds = otherRows.ToArray();
            foreach (var otherRowId in otherRowIds)
            {
                int? joinedRowId = GetNewRowId();
                var joinRow = CreateNewLinkedJoinRow(columnRowIndex, joinedRowId.Value, key, otherRowId, side);
                keyRowsMapping.ColRowMappings.Add(joinRow);

                AddNewRow(joinRow, updateRows, joinedRowId.Value);

                // Update the reverse lookup
                columnRowsToJoinRows.AddNewIfNotExists(columnRowIndex).Add(joinedRowId.Value);
                otherColumnRowsToJoinRows.AddNewIfNotExists(otherRowId.Value).Add(joinedRowId.Value);
            }

            // If there are no matching entries on the other side we still need to add a row and mapping
            if (otherRowIds.Length == 0)
            {
                UpdateNewKeyInExistingMapping(columnRowIndex, side, key, updateRows, columnRowsToJoinRows, keyRowsMapping);
            }
        }

        private ColumnRowMapping UpdateNewKey(int columnRowIndex,
                                              JoinSide side,
                                              TKey key,
                                              List<RowToUpdate> updateRows,
                                              Dictionary<int, HashSet<int>> columnRowsToJoinRows)
        {
            // First time this key has been used so create a new (unlinked on the other side)
            ColumnRowMapping keyRowsMapping = new ColumnRowMapping {ColRowMappings = new List<Row>()};

            UpdateNewKeyInExistingMapping(columnRowIndex, side, key, updateRows, columnRowsToJoinRows, keyRowsMapping);

            return keyRowsMapping;
        }

        private void UpdateNewKeyInExistingMapping(int columnRowIndex,
                                                   JoinSide side,
                                                   TKey key,
                                                   List<RowToUpdate> updateRows,
                                                   Dictionary<int, HashSet<int>> columnRowsToJoinRows,
                                                   ColumnRowMapping keyRowsMapping)
        {
            bool shouldAddUnlinkedRow = ShouldAddUnlinkedRow(side);
            int? joinedRowId = shouldAddUnlinkedRow ? GetNewRowId() : (int?) null;
            var joinRow = CreateNewJoinRow(columnRowIndex, key, joinedRowId, side);
            keyRowsMapping.ColRowMappings.Add(joinRow);

            if (shouldAddUnlinkedRow)
            {
                AddNewRow(joinRow, updateRows, joinedRowId.Value);

                // Update the reverse lookup
                columnRowsToJoinRows.AddNewIfNotExists(columnRowIndex).Add(joinedRowId.Value);
            }
        }

        private int GetNewRowId()
        {
            int newRowId = _rowManager.AddRow();
//            Console.WriteLine("Adding row {0} to join {1} {2}", newRowId, _leftColumn.ColumnId, _rightColumn.ColumnId);
            return newRowId;
        }

        private bool ShouldAddUnlinkedRow(JoinSide side)
        {
            if (_joinType == JoinType.Inner) return false;
            if (_joinType == JoinType.LeftOuter) return side == JoinSide.Left;
            if (_joinType == JoinType.RightOuter) return side == JoinSide.Right;
            return true;
        }

        private void AddNewRow(Row joinRow, List<RowToUpdate> updateRows, int joinedRowId)
        {
            if (joinedRowId == _rows.Count)
            {
                _rows.Add(joinRow);
            }
            else
            {
                _rows[joinedRowId] = joinRow;
            }
            updateRows.Add(new RowToUpdate {RowIndex = joinedRowId, Type = RowToUpdate.RowUpdateType.Add});
        }

        private static Row CreateNewJoinRow(int columnRowIndex, TKey key, int? joinedRowId, JoinSide side)
        {
            if (side == JoinSide.Left)
            {
                return new Row {Key = key, LeftRowId = columnRowIndex, RowId = joinedRowId};
            }
            return new Row {Key = key, RightRowId = columnRowIndex, RowId = joinedRowId};
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
                return new Row {RowId = joinedRowId, Key = key, LeftRowId = columnRowIndex, RightRowId = otherRowId};
            }

            return new Row {RowId = joinedRowId, Key = key, RightRowId = columnRowIndex, LeftRowId = otherRowId};
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
            return r => IsRowUnlinked(r, side);
        }

        private static bool IsRowUnlinked(Row row, JoinSide side)
        {
            if (side == JoinSide.Left) return !row.LeftRowId.HasValue;

            return !row.RightRowId.HasValue;
        }

        private static Func<Row, bool> FindKeyByRow(int columnRowIndex, JoinSide side)
        {
            if (side == JoinSide.Left) return r => r.LeftRowId == columnRowIndex;

            return r => r.RightRowId == columnRowIndex;
        }

        private ColumnRowMapping MoveKeyEntry(List<Row?> rows,
                                              int existingRowIndex,
                                              TKey oldKey,
                                              TKey newKey,
                                              Dictionary<int, HashSet<int>> columnRowsToJoinRows,
                                              ColumnRowMapping keyRowsMapping,
                                              Dictionary<TKey, ColumnRowMapping> rowsByKey)
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

        public void Dispose()
        {
            _leftColToken.Dispose();
            _rightColToken.Dispose();
            _leftRowToken.Dispose();
            _rightRowToken.Dispose();
        }
    }
}