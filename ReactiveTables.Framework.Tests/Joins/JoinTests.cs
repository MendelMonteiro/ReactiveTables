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
using System.Reactive;
using NUnit.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Columns.Calculated;
using ReactiveTables.Framework.Joins;
using System.Reactive.Linq;

namespace ReactiveTables.Framework.Tests.Joins
{
    public static class TestLeftColumns
    {
        public const string IdColumn = "Test1.IdColumn";
        public const string StringColumn = "Test1.String";
        public const string DecimalColumn = "Test1.Decimal";
    }

    public static class TestRightColumns
    {
        public const string IdColumn = "Test2.IdColumn";
        public const string LeftIdColumn = "Test2.LeftId";
        public const string DecimalColumn = "Test2.Decimal";
    }

    [TestFixture]
    public class JoinTests
    {
        [Test]
        public void TestOuterJoin()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            int[] rowsUpdated = new int[1];
            joinedTable.RowUpdates().Subscribe(update => rowsUpdated[0]++);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.ColumnUpdates().Subscribe(
                update =>
                    {
                        if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                            idCol1Updates++;
                        if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                            idCol2Updates++;
                    });

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, 401);
            Assert.AreEqual(1, idCol1Updates);

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual(1, idCol1Updates);

            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 801);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 401);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);
        }

        [Test]
        public void TestFullOuterJoinLeftFirst()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            int[] rowsUpdated = new int[1];
            joinedTable.RowUpdates().Subscribe(update => rowsUpdated[0]++);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 401, 1);

            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 1, 401, 801);
        }

        [Test]
        public void TestFullOuterJoinRightFirst()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            int[] rowsUpdated = new int[1];
            joinedTable.RowUpdates().Subscribe(update => rowsUpdated[0]++);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 1, 401, 801);

            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 401, 1);
        }

        [Test]
        public void TestFullOuterJoinMultipleRows()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            int[] rowsUpdated = new int[1];
            joinedTable.RowUpdates().Subscribe(update => rowsUpdated[0]++);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            // First row left side
            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 401, 1);

            // First row right side
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 1, 401, 801);

            // Second row right side
            rightRowId = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 2, 445, 889);

            // Second row left side
            leftRowId = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 445, 2);
        }

        [Test]
        public void TestOuterJoin1ToN()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            int[] rowsUpdated = new int[1];
            joinedTable.RowUpdates().Subscribe(update => rowsUpdated[0]++);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            // First row left side
            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 401, 1);

            // First row right side
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 1, 401, 801);

            // Second row left side
            leftRowId = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 402, 2);

            // Second row right side
            rightRowId = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 2, 402, 889);

            // Third row right side
            rightRowId = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 3, 401, 890);
        }

        [Test]
        public void TestOuterJoin1ToNAllLeftFirst()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            RowUpdateHandler rowsUpdated = new RowUpdateHandler();
            joinedTable.RowUpdates().Subscribe(rowsUpdated.OnRowUpdate);

            var leftRowId = leftTable.AddRow();
            var leftRowId1 = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated.CurrentRowCount);
            
            // First row left side
            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 401, 1);

            // Second row left side
            SetAndTestLeftRow(leftTable, leftRowId1, rowsUpdated, joinedTable, 402, 2);

            var rightRowId = rightTable.AddRow();
            var rightRowId2 = rightTable.AddRow();
            var rightRowId3 = rightTable.AddRow();

            // First row right side
            Assert.AreEqual(2, rowsUpdated.CurrentRowCount);
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 401, 801, 2, 0);
            
            // Second row right side
            SetAndTestRightRow(rightTable, rightRowId2, rowsUpdated, joinedTable, 401, 802, 3, 2);

            // Third row right side
            SetAndTestRightRow(rightTable, rightRowId3, rowsUpdated, joinedTable, 402, 803, 3, 1);
        }

        [Test]
        public void TestOuterJoin1ToNAllRightFirst()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            ColumnUpdateHandler colsUpdated = new ColumnUpdateHandler();
            joinedTable.ColumnUpdates().Subscribe(colsUpdated.OnColumnUpdate);
            RowUpdateHandler rowsUpdated = new RowUpdateHandler();
            joinedTable.RowUpdates().Subscribe(rowsUpdated.OnRowUpdate);
            
            var rightRowId = rightTable.AddRow();
            var rightRowId2 = rightTable.AddRow();
            var rightRowId3 = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated.CurrentRowCount);

            // First row right side
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 401, 801, 1, 0);
            // Bug happens when right elements appear first
//            Assert.AreEqual(,);

            // Second row right side
            SetAndTestRightRow(rightTable, rightRowId2, rowsUpdated, joinedTable, 401, 802, 2, 1);

            // Third row right side
            SetAndTestRightRow(rightTable, rightRowId3, rowsUpdated, joinedTable, 402, 803, 3, 2);

            Assert.AreEqual(3, rowsUpdated.CurrentRowCount);
            var leftRowId = leftTable.AddRow();
            var leftRowId1 = leftTable.AddRow();

            // First row left side
            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 401, 3, 0);
            Assert.AreEqual(401, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, 0)); // Check both rows created
            Assert.AreEqual(401, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, 1)); // Check both rows created

            // Second row left side
            SetAndTestLeftRow(leftTable, leftRowId1, rowsUpdated, joinedTable, 402, 3, 2);
            Assert.AreEqual(402, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, 2)); // Check both rows created
        }

        [Test]
        public void TestInnerJoin1ToNAllLeftFirst()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable, JoinType.Inner);

            RowUpdateHandler rowsUpdated = new RowUpdateHandler();
            joinedTable.RowUpdates().Subscribe(rowsUpdated.OnRowUpdate);

            var leftRowId = leftTable.AddRow();
            var leftRowId1 = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated.CurrentRowCount);

            // First row left side
            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 401, 0, false);

            // Second row left side
            SetAndTestLeftRow(leftTable, leftRowId1, rowsUpdated, joinedTable, 402, 0, false);

            var rightRowId = rightTable.AddRow();
            var rightRowId2 = rightTable.AddRow();
            var rightRowId3 = rightTable.AddRow();

            // First row right side
            Assert.AreEqual(0, rowsUpdated.CurrentRowCount);
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 401, 801, 1, 0);

            // Second row right side
            SetAndTestRightRow(rightTable, rightRowId2, rowsUpdated, joinedTable, 401, 802, 2, 1);

            // Third row right side
            SetAndTestRightRow(rightTable, rightRowId3, rowsUpdated, joinedTable, 402, 803, 3, 2);
        }

        [Test]
        public void TestInnerJoin1ToNAllRightFirst()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable, JoinType.Inner);

            RowUpdateHandler rowsUpdated = new RowUpdateHandler();
            joinedTable.RowUpdates().Subscribe(rowsUpdated.OnRowUpdate);

            var rightRowId = rightTable.AddRow();
            var rightRowId2 = rightTable.AddRow();
            var rightRowId3 = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated.CurrentRowCount);

            // First row right side
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 401, 801, 0, false);

            // Second row right side
            SetAndTestRightRow(rightTable, rightRowId2, rowsUpdated, joinedTable, 401, 802, 0, false);

            // Third row right side
            SetAndTestRightRow(rightTable, rightRowId3, rowsUpdated, joinedTable, 402, 803, 0, false);

            Assert.AreEqual(0, rowsUpdated.CurrentRowCount);
            var leftRowId = leftTable.AddRow();
            var leftRowId1 = leftTable.AddRow();

            // First row left side
            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 401, 2, 0);
            Assert.AreEqual(401, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, 1)); // Check both rows created
            Assert.AreEqual(401, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, 0)); // Check both rows created

            // Second row left side
            SetAndTestLeftRow(leftTable, leftRowId1, rowsUpdated, joinedTable, 402, 3, 2);
        }

        [Test]
        public void TestLeftOuterJoin()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable, JoinType.LeftOuter);

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            joinedTable.RowUpdates().Subscribe(updateHandler.OnRowUpdate);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, updateHandler.CurrentRowCount);
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 401, 1);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 401, 801, 1);

            rightRowId = rightTable.AddRow();
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 445, 889, 1, false);

            leftRowId = leftTable.AddRow();
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 445, 2);

            rightRowId = rightTable.AddRow();
            Assert.AreEqual(2, updateHandler.CurrentRowCount);
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 401, 890, 3);

            leftRowId = leftTable.AddRow();
            Assert.AreEqual(3, updateHandler.CurrentRowCount);
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 401, 5);
        }

        [Test]
        public void TestRightOuterJoin()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable, JoinType.RightOuter);

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            joinedTable.RowUpdates().Subscribe(updateHandler.OnRowUpdate);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, updateHandler.CurrentRowCount);
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 401, 0, false);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, updateHandler.CurrentRowCount);
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 401, 801, 1);

            rightRowId = rightTable.AddRow();
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 402, 802, 2);

            leftRowId = leftTable.AddRow();
            Assert.AreEqual(2, updateHandler.CurrentRowCount);
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 402, 2);

            leftRowId = leftTable.AddRow();
            Assert.AreEqual(2, updateHandler.CurrentRowCount);
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 401, 3);

            rightRowId = rightTable.AddRow();
            Assert.AreEqual(3, updateHandler.CurrentRowCount);
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 401, 803, 5);
        }

        [Test]
        public void TestInnerJoin()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable, JoinType.Inner);

            int[] rowsUpdated = new int[1];
            joinedTable.RowUpdates().Subscribe(update => rowsUpdated[0]++);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            // Left table first row
            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 401, 0, false);

            // Right table first row
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 1, 401, 801);
        }

        [Test]
        public void TestInnerJoin1ToN()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable, JoinType.Inner);

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            joinedTable.RowUpdates().Subscribe(updateHandler.OnRowUpdate);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, updateHandler.CurrentRowCount);

            // Left table first row
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 401, 0, false);

            // Right table first row
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 401, 801, 1);

            // Right table second row
            rightRowId = rightTable.AddRow();
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 401, 889, 2);

            // Right table third row
            rightRowId = rightTable.AddRow();
            Assert.AreEqual(2, updateHandler.CurrentRowCount);
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 401, 890, 3);

            // Left table second row
            leftRowId = leftTable.AddRow();
            Assert.AreEqual(3, updateHandler.CurrentRowCount);
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 401, 6);
        }

        [Test]
        public void TestDeleteFullOuterJoin()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            joinedTable.RowUpdates().Subscribe(updateHandler.OnRowUpdate);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.ColumnUpdates().Subscribe(
                update =>
                    {
                        if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                            idCol1Updates++;
                        else if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                            idCol2Updates++;
                    });

            // Add a coule of rows and then delete the primary side
            var leftRowId = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 401, 1);

            var rightRowId = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 401, 801, 1);

            leftTable.DeleteRow(leftRowId);
            TestRowCount(updateHandler, joinedTable, 1);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, updateHandler.LastRowUpdated));

            // Now delete the other side so that there are no rows left
            rightTable.DeleteRow(rightRowId);
            TestRowCount(updateHandler, joinedTable, 0);

            // Test multiple cardinality by adding two rows on each side that match the same key
            var leftRowId1 = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId1, updateHandler, joinedTable, 402, 1);

            var rightRowId1 = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId1, updateHandler, joinedTable, 402, 802, 1);

            var leftRowId2 = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId2, updateHandler, joinedTable, 402, 2);

            var rightRowId2 = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId2, updateHandler, joinedTable, 402, 803, 4);

            // Now delete the rows
            leftTable.DeleteRow(leftRowId1);
            TestRowCount(updateHandler, joinedTable, 2);
            int firstRowIndex = updateHandler.RowsUpdated.First();
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, firstRowIndex));

            leftTable.DeleteRow(leftRowId2);
            TestRowCount(updateHandler, joinedTable, 2);

            rightTable.DeleteRow(rightRowId1);
            TestRowCount(updateHandler, joinedTable, 1);

            rightTable.DeleteRow(rightRowId2);
            TestRowCount(updateHandler, joinedTable, 0);

            // Start adding again with the initial key - check we recycle the rowIds
            var leftRowIdNew = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowIdNew, updateHandler, joinedTable, 401, 1);
            Assert.AreEqual(leftRowId, leftRowIdNew);

            var rightRowIdNew = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowIdNew, updateHandler, joinedTable, 401, 801, 1);
            Assert.AreEqual(rightRowId, rightRowIdNew);
        }

        [Test]
        public void TestDeleteLeftOuterJoin()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable, JoinType.LeftOuter);

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            joinedTable.RowUpdates().Subscribe(updateHandler.OnRowUpdate);

            // Add a coule of rows and then delete the left side, make sure the right row dissapears
            var leftRowId = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 401, 1);

            var rightRowId = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 401, 801, 1);

            leftTable.DeleteRow(leftRowId);
            TestRowCount(updateHandler, joinedTable, 0);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, updateHandler.LastRowUpdated));
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, rightRowId));

            // Now delete the other side so that there are still no rows left
            rightTable.DeleteRow(rightRowId);
            TestRowCount(updateHandler, joinedTable, 0);

            var rightRowId1 = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId1, updateHandler, joinedTable, 401, 801, 0, false);

            var leftRowId1 = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId1, updateHandler, joinedTable, 401, 1);

            // Now delete the other side so that there is a row left
            rightTable.DeleteRow(rightRowId1);
            TestRowCount(updateHandler, joinedTable, 1);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, rightRowId1));

            // And then once the left row is gone there are no rows left
            leftTable.DeleteRow(leftRowId1);
            TestRowCount(updateHandler, joinedTable, 0);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, updateHandler.LastRowUpdated));
        }

        [Test]
        public void TestDeleteRightOuterJoin()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable, JoinType.RightOuter);

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            joinedTable.RowUpdates().Subscribe(updateHandler.OnRowUpdate);

            // Add a coule of rows and then delete the left side, make sure the right row stays
            var leftRowId = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 401, 0, false);

            var rightRowId = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 401, 801, 1);

            leftTable.DeleteRow(leftRowId);
            TestRowCount(updateHandler, joinedTable, 1);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, updateHandler.LastRowUpdated));

            // And then once the left row is gone there are no rows left
            rightTable.DeleteRow(rightRowId);
            TestRowCount(updateHandler, joinedTable, 0);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, rightRowId));

            // Add a coule of rows and then delete the right side, make sure the rows dissapear
            var rightRowId1 = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId1, updateHandler, joinedTable, 401, 801, 1);

            var leftRowId1 = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId1, updateHandler, joinedTable, 401, 1);

            rightTable.DeleteRow(rightRowId1);
            TestRowCount(updateHandler, joinedTable, 0);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, rightRowId1));

            // And now nothing changes after deleting the other side
            leftTable.DeleteRow(leftRowId1);
            TestRowCount(updateHandler, joinedTable, 0);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, updateHandler.LastRowUpdated));
        }

        [Test]
        public void TestDeleteInnerJoin()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable, JoinType.Inner);

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            joinedTable.RowUpdates().Subscribe(updateHandler.OnRowUpdate);

            // Add a coule of rows and then delete the left side, make sure the row goes
            var leftRowId = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 401, 0, false);

            var rightRowId = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 401, 801, 1);

            leftTable.DeleteRow(leftRowId);
            TestRowCount(updateHandler, joinedTable, 0);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, updateHandler.LastRowUpdated));

            // And now nothing changes after deleting the other side
            rightTable.DeleteRow(rightRowId);
            TestRowCount(updateHandler, joinedTable, 0);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, rightRowId));

            // Add a coule of rows and then delete the right side, make sure the row dissapears
            var rightRowId1 = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId1, updateHandler, joinedTable, 401, 801, 0, false);

            var leftRowId1 = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId1, updateHandler, joinedTable, 401, 1);

            rightTable.DeleteRow(rightRowId1);
            TestRowCount(updateHandler, joinedTable, 0);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, rightRowId1));

            // And now nothing changes after deleting the other side
            leftTable.DeleteRow(leftRowId1);
            TestRowCount(updateHandler, joinedTable, 0);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, updateHandler.LastRowUpdated));
        }

        [Test]
        public void TestDeleteFromOneSideInnerJoin()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable, JoinType.Inner);

            RowUpdateHandler rowUpdates = new RowUpdateHandler();
            joinedTable.RowUpdates().Subscribe(rowUpdates.OnRowUpdate);
            ColumnUpdateHandler columnUpdates = new ColumnUpdateHandler();
            joinedTable.ColumnUpdates().Subscribe(columnUpdates.OnColumnUpdate);

            // Add rows, first to the left and then to the right
            var leftRowId = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId, rowUpdates, joinedTable, 401, 0, false);

            var leftRowId1 = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId1, rowUpdates, joinedTable, 402, 0, false);
            
            var rightRowId = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId, rowUpdates, joinedTable, 401, 801, 1);

            var rightRowId1 = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId1, rowUpdates, joinedTable, 402, 802, 2);
            Assert.AreEqual(2, joinedTable.RowCount);

            // Then delete only the left side and check that the rows are deleted
            leftTable.DeleteRow(leftRowId);
            TestRowCount(rowUpdates, joinedTable, 1);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, rowUpdates.LastRowUpdated));
            Assert.AreEqual(1, joinedTable.RowCount);

            leftTable.DeleteRow(leftRowId1);
            TestRowCount(rowUpdates, joinedTable, 0);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, rowUpdates.LastRowUpdated));
            Assert.AreEqual(0, joinedTable.RowCount);

            // Modify something on the remaining table and check that the updates are not propagated
            var colsUpdated = columnUpdates.LastColumnsUpdated.Count;
            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 42.42m);
            Assert.AreEqual(colsUpdated, columnUpdates.LastColumnsUpdated.Count);

            // Re-add the rows to the left side and make sure that the rows reappear
            leftRowId = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId, rowUpdates, joinedTable, 401, 1);
            Assert.AreEqual(1, joinedTable.RowCount);
            Assert.AreEqual(801, joinedTable.GetValue<int>(TestRightColumns.IdColumn, rowUpdates.LastRowUpdated));

            leftRowId1 = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId1, rowUpdates, joinedTable, 402, 2);
            Assert.AreEqual(2, joinedTable.RowCount);
            Assert.AreEqual(802, joinedTable.GetValue<int>(TestRightColumns.IdColumn, rowUpdates.LastRowUpdated));

            // Modify something on the remaining table and check that the updates are propagated
            colsUpdated = columnUpdates.LastColumnsUpdated.Count;
            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 84.42m);
            Assert.AreEqual(colsUpdated + 1, columnUpdates.LastColumnsUpdated.Count);
            Assert.AreEqual(84.42m, joinedTable.GetValue<decimal>(TestRightColumns.DecimalColumn, columnUpdates.LastRowUpdated));

            // Then delete only the left side and check that the rows are deleted
            leftTable.DeleteRow(leftRowId);
            TestRowCount(rowUpdates, joinedTable, 1);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, rowUpdates.LastRowUpdated));
            Assert.AreEqual(1, joinedTable.RowCount);

            leftTable.DeleteRow(leftRowId1);
            TestRowCount(rowUpdates, joinedTable, 0);
            Assert.AreEqual(0, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, rowUpdates.LastRowUpdated));
            Assert.AreEqual(0, joinedTable.RowCount);
        }

        private static void TestRowCount(RowUpdateHandler updateHandler, IReactiveTable joinedTable, int expectedRows)
        {
            Assert.AreEqual(expectedRows, updateHandler.CurrentRowCount);
            Assert.AreEqual(expectedRows, joinedTable.RowCount);
        }

        [Test]
        public void TestCalculatedColumnAddedBefore()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            const string columnId = "CalcCol";
            joinedTable.AddColumn(new ReactiveCalculatedColumn2<string, string, decimal>(
                                      columnId,
                                      (IReactiveColumn<string>) leftTable.Columns[TestLeftColumns.StringColumn],
                                      (IReactiveColumn<decimal>) rightTable.Columns[TestRightColumns.DecimalColumn],
                                      (str, dec) => string.Format("{0} - {1}", str, dec)));

            int[] rowsUpdated = new int[1];
            joinedTable.RowUpdates().Subscribe(update => rowsUpdated[0]++);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.ColumnUpdates().Subscribe(
                update =>
                    {
                        if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                            idCol1Updates++;
                        if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                            idCol2Updates++;
                    });

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, 401);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);

            Assert.AreEqual(" - 0", joinedTable.GetValue<string>(columnId, 0));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual(1, idCol1Updates);

            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 801);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 401);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual("hello - 0", joinedTable.GetValue<string>(columnId, 0));

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual("hello - 9876", joinedTable.GetValue<string>(columnId, 0));
        }

        [Test]
        public void TestCalculatedColumnUpdateValues()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            const string columnId = "CalcCol";
            joinedTable.AddColumn(new ReactiveCalculatedColumn2<string, string, decimal>(
                                      columnId,
                                      (IReactiveColumn<string>) leftTable.Columns[TestLeftColumns.StringColumn],
                                      (IReactiveColumn<decimal>) rightTable.Columns[TestRightColumns.DecimalColumn],
                                      (str, dec) => string.Format("{0} - {1}", str, dec)));

            int[] rowsUpdated = new int[1];
            joinedTable.RowUpdates().Subscribe(update => rowsUpdated[0]++);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.ColumnUpdates().Subscribe(
                update =>
                    {
                        if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                            idCol1Updates++;
                        if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                            idCol2Updates++;
                    });

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, 401);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);

            Assert.AreEqual(" - 0", joinedTable.GetValue<string>(columnId, 0));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual(1, idCol1Updates);

            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 801);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 401);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual("hello - 0", joinedTable.GetValue<string>(columnId, 0));

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual("hello - 9876", joinedTable.GetValue<string>(columnId, 0));

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 6789m);
            Assert.AreEqual(6789m, joinedTable.GetValue<decimal>(TestRightColumns.DecimalColumn, rightRowId));
            Assert.AreEqual("hello - 6789", joinedTable.GetValue<string>(columnId, 0));

            PropertyChangedNotifier notifier = new PropertyChangedNotifier(joinedTable);
            var testConsumer = new TestPropertyChangedConsumer();
            notifier.RegisterPropertyNotifiedConsumer(testConsumer, 0);

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 7777m);
            Assert.AreEqual("Decimal", testConsumer.PropertiesChanged[testConsumer.PropertiesChanged.Count - 2]);
            Assert.AreEqual(columnId, testConsumer.LastPropertyChanged);
        }

        [Test]
        public void TestCalculatedColumnAddedAfter()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            int[] rowsUpdated = new int[1];
            joinedTable.RowUpdates().Subscribe(update => rowsUpdated[0]++);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, joinedTable.RowCount);
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, joinedTable.RowCount);
            Assert.AreEqual(0, rowsUpdated[0]);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.ColumnUpdates().Subscribe(
                update =>
                    {
                        if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                            idCol1Updates++;

                        if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                            idCol2Updates++;
                    });

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, 401);
            Assert.AreEqual(1, joinedTable.RowCount);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual(1, idCol1Updates);

            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 801);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 401);
            Assert.AreEqual(1, joinedTable.RowCount);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            const string columnId = "CalcCol";
            joinedTable.AddColumn(new ReactiveCalculatedColumn2<string, string, decimal>(
                                      columnId,
                                      (IReactiveColumn<string>) leftTable.Columns[TestLeftColumns.StringColumn],
                                      (IReactiveColumn<decimal>) rightTable.Columns[TestRightColumns.DecimalColumn],
                                      (str, dec) => string.Format("{0} - {1}", str, dec)));

            Assert.AreEqual("hello - 9876", joinedTable.GetValue<string>(columnId, 0));
        }

        [Test]
        public void TestJoin1ToN()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            int[] rowsUpdated = new int[1];
            joinedTable.RowUpdates().Subscribe(update => rowsUpdated[0]++);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.ColumnUpdates().Subscribe(
                update =>
                    {
                        if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                            idCol1Updates++;
                        if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                            idCol2Updates++;
                    });

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, 401);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual(1, idCol1Updates);

            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 801);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 401);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            rightRowId = rightTable.AddRow();
            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 889);
            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 401);
            Assert.AreEqual(2, idCol2Updates);
            Assert.AreEqual(2, rowsUpdated[0]);
        }

        [Test]
        public void TestCalculatedJoin1ToN()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            const string columnId = "CalcCol";
            joinedTable.AddColumn(new ReactiveCalculatedColumn2<string, string, decimal>(
                                      columnId,
                                      (IReactiveColumn<string>) leftTable.Columns[TestLeftColumns.StringColumn],
                                      (IReactiveColumn<decimal>) rightTable.Columns[TestRightColumns.DecimalColumn],
                                      (str, dec) => string.Format("{0} - {1}", str, dec)));

            int[] rowsUpdated = new int[1];
            joinedTable.RowUpdates().Subscribe(update => rowsUpdated[0]++);

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.ColumnUpdates().Subscribe(
                update =>
                    {
                        if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                            idCol1Updates++;
                        if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                            idCol2Updates++;
                    });

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, 401);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);

            Assert.AreEqual(" - 0", joinedTable.GetValue<string>(columnId, 0));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual(1, idCol1Updates);

            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 801);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 401);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual("hello - 0", joinedTable.GetValue<string>(columnId, 0));

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual("hello - 9876", joinedTable.GetValue<string>(columnId, 0));

            rightRowId = rightTable.AddRow();
            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 889);
            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 401);
            Assert.AreEqual(2, rowsUpdated[0]);
            Assert.AreEqual(2, idCol2Updates);
            Assert.AreEqual("hello - 0", joinedTable.GetValue<string>(columnId, 1));
        }

        [Test]
        public void TestRightSideFirst()
        {
            ReactiveTable right;
            IReactiveTable joinedTable;
            var left = CreateJoinedTables(out right, out joinedTable);

            int rightleftRowId = right.AddRow();
            Assert.AreEqual(0, joinedTable.RowCount);

            right.SetValue(TestRightColumns.IdColumn, rightleftRowId, 111);
            Assert.AreEqual(0, joinedTable.RowCount);

            right.SetValue(TestRightColumns.LeftIdColumn, rightleftRowId, 222);
            Assert.AreEqual(1, joinedTable.RowCount);
            Assert.AreEqual(111, joinedTable.GetValue<int>(TestRightColumns.IdColumn, 0));
            Assert.AreEqual(222, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, 0));

            int rightrightRowId = right.AddRow();
            Assert.AreEqual(1, joinedTable.RowCount);

            right.SetValue(TestRightColumns.IdColumn, rightrightRowId, 112);
            Assert.AreEqual(1, joinedTable.RowCount);

            right.SetValue(TestRightColumns.LeftIdColumn, rightrightRowId, 223);
            Assert.AreEqual(2, joinedTable.RowCount);
            Assert.AreEqual(112, joinedTable.GetValue<int>(TestRightColumns.IdColumn, 1));
            Assert.AreEqual(223, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, 1));

            int leftleftRowId = left.AddRow();
            left.SetValue(TestLeftColumns.IdColumn, leftleftRowId, 223);
            left.SetValue(TestLeftColumns.StringColumn, leftleftRowId, "hello");
            Assert.AreEqual(2, joinedTable.RowCount);
            Assert.AreEqual(223, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, 1));
            Assert.AreEqual("hello", joinedTable.GetValue<string>(TestLeftColumns.StringColumn, 1));
        }

        [Test]
        public void TestNotifyPropertyChangedOuter()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            TestPropertyChangedConsumer propertyChangedConsumer = new TestPropertyChangedConsumer();
            var leftRow = leftTable.AddRow();
            const int joinedRow = 0;
            joinedTable.ChangeNotifier.RegisterPropertyNotifiedConsumer(propertyChangedConsumer, joinedRow);

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRow, 1);
            Assert.Contains("IdColumn", propertyChangedConsumer.PropertiesChanged);
            Assert.Contains("String", propertyChangedConsumer.PropertiesChanged);
            Assert.Contains("Decimal", propertyChangedConsumer.PropertiesChanged);
            Assert.AreEqual(1, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, joinedRow));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRow, "Blah");
            Assert.AreEqual("String", propertyChangedConsumer.LastPropertyChanged);
            Assert.AreEqual("Blah", joinedTable.GetValue<string>(TestLeftColumns.StringColumn, joinedRow));

            var rightRow = rightTable.AddRow();
            rightTable.SetValue(TestRightColumns.IdColumn, rightRow, 1);
            // Right row updates should be suppressed until it's properly joined
            Assert.AreNotEqual("IdColumn", propertyChangedConsumer.LastPropertyChanged);
            Assert.AreNotEqual(1, joinedTable.GetValue<int>(TestRightColumns.IdColumn, joinedRow));

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRow, 1);
            // All other newly joined columns should have their notifications fired on join
            var rightChanges = propertyChangedConsumer.PropertiesChanged.Skip(propertyChangedConsumer.PropertiesChanged.Count - 3).Take(3);
            Assert.AreEqual("IdColumn", rightChanges.ElementAt(0));
            Assert.AreEqual("LeftId", rightChanges.ElementAt(1));
            Assert.AreEqual("Decimal", rightChanges.ElementAt(2));
            Assert.AreEqual(1, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, joinedRow));
            Assert.AreEqual(1, joinedTable.GetValue<int>(TestRightColumns.IdColumn, joinedRow));
        }

        [Test]
        public void TestNotifyPropertyChangedInner()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable, JoinType.Inner);

            TestPropertyChangedConsumer propertyChangedConsumer = new TestPropertyChangedConsumer();
            var leftRow = leftTable.AddRow();
            const int joinedRow = 0;
            joinedTable.ChangeNotifier.RegisterPropertyNotifiedConsumer(propertyChangedConsumer, joinedRow);

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRow, 1);
            // Updates and values should not be present until join
            CollectionAssert.DoesNotContain(propertyChangedConsumer.PropertiesChanged, "IdColumn");
            Assert.AreNotEqual(1, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, leftRow));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRow, "Blah");
            // Updates and values should not be present until join
            CollectionAssert.DoesNotContain(propertyChangedConsumer.PropertiesChanged, "String");
            Assert.AreNotEqual("Blah", joinedTable.GetValue<string>(TestLeftColumns.StringColumn, joinedRow));

            var rightRow = rightTable.AddRow();
            rightTable.SetValue(TestRightColumns.IdColumn, rightRow, 1);
            // Right row updates should be suppressed until it's properly joined
            CollectionAssert.DoesNotContain(propertyChangedConsumer.PropertiesChanged, "IdColumn");
            Assert.AreNotEqual(1, joinedTable.GetValue<int>(TestRightColumns.IdColumn, joinedRow));

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRow, 1);
            // All other newly joined columns should have their notifications fired on join
            Assert.Contains("IdColumn", propertyChangedConsumer.PropertiesChanged);
            Assert.AreEqual(1, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, joinedRow));

            Assert.Contains("String", propertyChangedConsumer.PropertiesChanged);
            Assert.AreEqual("Blah", joinedTable.GetValue<string>(TestLeftColumns.StringColumn, joinedRow));

            Assert.Contains("LeftId", propertyChangedConsumer.PropertiesChanged);
            Assert.AreEqual(1, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, joinedRow));

            Assert.Contains("IdColumn", propertyChangedConsumer.PropertiesChanged);
            Assert.AreEqual(1, joinedTable.GetValue<int>(TestRightColumns.IdColumn, joinedRow));
        }

        [Test]
        public void TestJoinFilledTables()
        {
            var leftTable = CreateLeftTable();
            var rightTable = CreateRightTable();

            var leftRow1 = leftTable.AddRow();
            leftTable.SetValue(TestLeftColumns.IdColumn, leftRow1, 1);
            leftTable.SetValue(TestLeftColumns.StringColumn, leftRow1, "Blah");

            var rightRow1 = rightTable.AddRow();
            rightTable.SetValue(TestRightColumns.IdColumn, rightRow1, 1);
            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRow1, 1);

            IReactiveTable joinedTable = leftTable.Join(rightTable, new Join<int>(leftTable, TestLeftColumns.IdColumn, rightTable, TestRightColumns.LeftIdColumn));

            Assert.AreEqual(1, joinedTable.RowCount);
        }

        private static ReactiveTable CreateJoinedTables(out ReactiveTable rightTable, out IReactiveTable joinedTable, JoinType joinType = JoinType.FullOuter)
        {
            var leftTable = CreateLeftTable();

            rightTable = CreateRightTable();

            joinedTable = leftTable.Join(rightTable, new Join<int>(leftTable, TestLeftColumns.IdColumn, rightTable, TestRightColumns.LeftIdColumn, joinType));
            return leftTable;
        }

        private static ReactiveTable CreateLeftTable()
        {
            ReactiveTable leftTable = new ReactiveTable();
            leftTable.AddColumn(new ReactiveColumn<int>(TestLeftColumns.IdColumn));
            leftTable.AddColumn(new ReactiveColumn<string>(TestLeftColumns.StringColumn));
            leftTable.AddColumn(new ReactiveColumn<decimal>(TestLeftColumns.DecimalColumn));
            return leftTable;
        }

        private static ReactiveTable CreateRightTable()
        {
            ReactiveTable rightTable = new ReactiveTable();
            rightTable.AddColumn(new ReactiveColumn<int>(TestRightColumns.IdColumn));
            rightTable.AddColumn(new ReactiveColumn<int>(TestRightColumns.LeftIdColumn));
            rightTable.AddColumn(new ReactiveColumn<decimal>(TestRightColumns.DecimalColumn));
            return rightTable;
        }

        private static void SetAndTestLeftRow(ReactiveTable leftTable,
                                              int leftRowId,
                                              int[] rowsUpdated,
                                              IReactiveTable joinedTable,
                                              int id,
                                              int expectedRowsUpdated,
                                              bool visibleInJoinTable = true)
        {
            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, id);
            Assert.AreEqual(expectedRowsUpdated, rowsUpdated[0]);
            if (visibleInJoinTable) Assert.AreEqual(id, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, leftRowId));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            if (visibleInJoinTable) Assert.AreEqual("hello", joinedTable.GetValue<string>(TestLeftColumns.StringColumn, leftRowId));
        }

        private static void SetAndTestRightRow(ReactiveTable rightTable,
                                               int rightRowId,
                                               int[] rowsUpdated,
                                               IReactiveTable joinedTable,
                                               int expectedRowsUpdated,
                                               int leftId,
                                               int id,
                                               bool visibleInJoinTable = true)
        {
            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, id);
            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, leftId);
            Assert.AreEqual(expectedRowsUpdated, rowsUpdated[0]);
            if (visibleInJoinTable) Assert.AreEqual(leftId, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, rightRowId));

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            if (visibleInJoinTable) Assert.AreEqual(9876m, joinedTable.GetValue<decimal>(TestRightColumns.DecimalColumn, rightRowId));
        }

        private static void SetAndTestLeftRow(ReactiveTable leftTable, int leftRowId, RowUpdateHandler updateHandler,
                                        IReactiveTable joinedTable, int id, int expectedRowsUpdated, bool visibleInJoinTable = true)
        {
            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, id);
            TestRowCount(updateHandler, joinedTable, expectedRowsUpdated);
            if (visibleInJoinTable) Assert.AreEqual(id, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, updateHandler.LastRowUpdated));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            if (visibleInJoinTable) Assert.AreEqual("hello", joinedTable.GetValue<string>(TestLeftColumns.StringColumn, updateHandler.LastRowUpdated));
        }

        private static void SetAndTestLeftRow(ReactiveTable leftTable, int leftRowId, RowUpdateHandler updateHandler,
                                        IReactiveTable joinedTable, int id, int expectedRowsUpdated, int lastRowUpdated)
        {
            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, id);
            TestRowCount(updateHandler, joinedTable, expectedRowsUpdated);
            Assert.AreEqual(id, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, lastRowUpdated));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual("hello", joinedTable.GetValue<string>(TestLeftColumns.StringColumn, lastRowUpdated));
        }

        private static void SetAndTestRightRow(ReactiveTable rightTable, int rightRowId, RowUpdateHandler updateHandler,
                                        IReactiveTable joinedTable, int leftId, int id, int expectedRowsUpdated, bool visibleInJoinTable = true)
        {
            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, id);
            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, leftId);
            TestRowCount(updateHandler, joinedTable, expectedRowsUpdated);
            if (visibleInJoinTable)
                Assert.AreEqual(leftId, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, updateHandler.LastRowUpdated));

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            if (visibleInJoinTable) 
                Assert.AreEqual(9876m, joinedTable.GetValue<decimal>(TestRightColumns.DecimalColumn, updateHandler.LastRowUpdated));
        }

        private static void SetAndTestRightRow(ReactiveTable rightTable, int rightRowId, RowUpdateHandler updateHandler,
                                        IReactiveTable joinedTable, int leftId, int id, int expectedRowsUpdated, int lastRowUpdated)
        {
            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, id);
            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, leftId);
            TestRowCount(updateHandler, joinedTable, expectedRowsUpdated);
            Assert.AreEqual(leftId, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, lastRowUpdated));

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            Assert.AreEqual(9876m, joinedTable.GetValue<decimal>(TestRightColumns.DecimalColumn, lastRowUpdated));
        }

        private class ColumnUpdateHandler
        {
            public List<string> LastColumnsUpdated { get; private set; }
            public List<int> LastRowsUpdated { get; private set; }
            public int LastRowUpdated { get { return LastRowsUpdated.LastOrDefault(); } }
            public string LastColumnUpdated { get { return LastColumnsUpdated.LastOrDefault(); } }

            public ColumnUpdateHandler()
            {
                LastColumnsUpdated = new List<string>();
                LastRowsUpdated = new List<int>();
            }

            public void OnColumnUpdate(TableUpdate update)
            {
                LastColumnsUpdated.Add(update.Column.ColumnId);
                LastRowsUpdated.Add(update.RowIndex);
            }
        }
    }
}