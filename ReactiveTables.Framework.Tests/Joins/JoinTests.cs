using System.Collections.Generic;
using NUnit.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Columns.Calculated;
using ReactiveTables.Framework.Joins;
using ReactiveTables.Framework.UI;
using System.Linq;

namespace ReactiveTables.Framework.Tests.Joins
{
    public static class TestLeftColumns
    {
        public static readonly string IdColumn = "Test1.IdColumn";
        public static readonly string StringColumn = "Test1.String";
        public static readonly string DecimalColumn = "Test1.Decimal";
    }

    public static class TestRightColumns
    {
        public static readonly string IdColumn = "Test2.IdColumn";
        public static readonly string LeftIdColumn = "Test2.LeftId";
        public static readonly string DecimalColumn = "Test2.Decimal";
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
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated[0]++, null, null));

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                          {
                                              if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                                                  idCol1Updates++;
                                              if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                                                  idCol2Updates++;
                                          }, null, null));

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

            int [] rowsUpdated = new int[1];
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated[0]++, null, null));

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
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated[0]++, null, null));

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
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated[0]++, null, null));

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
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated[0]++, null, null));

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
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 2, 401, 889);
        }

        [Test]
        public void TestLeftOuterJoin()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable, JoinType.LeftOuter);

            RowUpdateHandler updateHandler = new RowUpdateHandler();
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(updateHandler.OnRowUpdate, null, null));

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
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(updateHandler.OnRowUpdate, null, null));

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
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated[0]++, null, null));

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
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(updateHandler.OnRowUpdate, null, null));

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
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(updateHandler.OnRowUpdate, null, null));

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                      {
                                          if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                                              idCol1Updates++;
                                          else if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                                              idCol2Updates++;
                                      }, null, null));

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
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated[0]++, null, null));

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                      {
                                          if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                                              idCol1Updates++;
                                          if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                                              idCol2Updates++;
                                      }, null, null));

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
                                      (IReactiveColumn<string>)leftTable.Columns[TestLeftColumns.StringColumn],
                                      (IReactiveColumn<decimal>)rightTable.Columns[TestRightColumns.DecimalColumn],
                                      (str, dec) => string.Format("{0} - {1}", str, dec)));

            int[] rowsUpdated = new int[1];
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated[0]++, null, null));

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                      {
                                          if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                                              idCol1Updates++;
                                          if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                                              idCol2Updates++;
                                      }, null, null));

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
            Assert.AreEqual("Decimal", testConsumer.PropertiesChanged[testConsumer.PropertiesChanged.Count-2]);
            Assert.AreEqual(columnId, testConsumer.LastPropertyChanged);
        }

        [Test]
        public void TestCalculatedColumnAddedAfter()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            int[] rowsUpdated = new int[1];
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated[0]++, null, null));

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                      {
                                          if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                                              idCol1Updates++;
                                          if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                                              idCol2Updates++;
                                      }, null, null));

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

            const string columnId = "CalcCol";
            joinedTable.AddColumn(new ReactiveCalculatedColumn2<string, string, decimal>(
                                      columnId,
                                      (IReactiveColumn<string>)leftTable.Columns[TestLeftColumns.StringColumn],
                                      (IReactiveColumn<decimal>)rightTable.Columns[TestRightColumns.DecimalColumn],
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
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated[0]++, null, null));

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                      {
                                          if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                                              idCol1Updates++;
                                          if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                                              idCol2Updates++;
                                      }, null, null));

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
                                      (IReactiveColumn<string>)leftTable.Columns[TestLeftColumns.StringColumn],
                                      (IReactiveColumn<decimal>)rightTable.Columns[TestRightColumns.DecimalColumn],
                                      (str, dec) => string.Format("{0} - {1}", str, dec)));

            int[] rowsUpdated = new int[1];
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated[0]++, null, null));

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(0, rowsUpdated[0]);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                      {
                                          if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                                              idCol1Updates++;
                                          if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                                              idCol2Updates++;
                                      }, null, null));

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

        private static ReactiveTable CreateJoinedTables(out ReactiveTable rightTable, out IReactiveTable joinedTable, JoinType joinType = JoinType.FullOuter)
        {
            var leftTable = CreateLeftTable();

            rightTable = CreateRightTable();

            joinedTable = leftTable.Join(rightTable, new Join<int>(leftTable, TestLeftColumns.IdColumn, rightTable, TestRightColumns.LeftIdColumn, joinType));
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

        private static ReactiveTable CreateLeftTable()
        {
            ReactiveTable leftTable = new ReactiveTable();
            leftTable.AddColumn(new ReactiveColumn<int>(TestLeftColumns.IdColumn));
            leftTable.AddColumn(new ReactiveColumn<string>(TestLeftColumns.StringColumn));
            leftTable.AddColumn(new ReactiveColumn<decimal>(TestLeftColumns.DecimalColumn));
            return leftTable;
        }

        private static void SetAndTestLeftRow(ReactiveTable leftTable, int leftRowId, int[] rowsUpdated, IReactiveTable joinedTable, int id, int expectedRowsUpdated, bool visibleInJoinTable = true)
        {
            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, id);
            Assert.AreEqual(expectedRowsUpdated, rowsUpdated[0]);
            if (visibleInJoinTable) Assert.AreEqual(id, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, leftRowId));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            if (visibleInJoinTable) Assert.AreEqual("hello", joinedTable.GetValue<string>(TestLeftColumns.StringColumn, leftRowId));
        }

        private static void SetAndTestRightRow(ReactiveTable rightTable, int rightRowId, int[] rowsUpdated, IReactiveTable joinedTable, int expectedRowsUpdated, int leftId, int id, bool visibleInJoinTable = true)
        {
            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, id);
            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, leftId);
            Assert.AreEqual(expectedRowsUpdated, rowsUpdated[0]);
            if (visibleInJoinTable) Assert.AreEqual(leftId, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, rightRowId));

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            if (visibleInJoinTable) Assert.AreEqual(9876m, joinedTable.GetValue<decimal>(TestRightColumns.DecimalColumn, rightRowId));
        }

        private void SetAndTestLeftRow(ReactiveTable leftTable, int leftRowId, RowUpdateHandler updateHandler, IReactiveTable joinedTable, int id, int expectedRowsUpdated, bool visibleInJoinTable = true)
        {
            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, id);
            TestRowCount(updateHandler, joinedTable, expectedRowsUpdated);
            if (visibleInJoinTable) Assert.AreEqual(id, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, updateHandler.LastRowUpdated));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            if (visibleInJoinTable) Assert.AreEqual("hello", joinedTable.GetValue<string>(TestLeftColumns.StringColumn, updateHandler.LastRowUpdated));
        }

        private void SetAndTestRightRow(ReactiveTable rightTable, int rightRowId, RowUpdateHandler updateHandler, IReactiveTable joinedTable, int leftId, int id, int expectedRowsUpdated, bool visibleInJoinTable = true)
        {
            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, id);
            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, leftId);
            TestRowCount(updateHandler, joinedTable, expectedRowsUpdated);
            if (visibleInJoinTable) Assert.AreEqual(leftId, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, updateHandler.LastRowUpdated));

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            if (visibleInJoinTable) Assert.AreEqual(9876m, joinedTable.GetValue<decimal>(TestRightColumns.DecimalColumn, updateHandler.LastRowUpdated));
        }


    }

    class RowUpdateHandler
    {
        public int CurrentRowCount { get; private set; }
        public int LastRowUpdated { get; private set; }
        public List<int> RowsUpdated { get; private set; }

        public RowUpdateHandler()
        {
            RowsUpdated = new List<int>();
        }

        public void OnRowUpdate(RowUpdate update)
        {
            RowsUpdated.Add(update.RowIndex);
            if (update.Action == RowUpdate.RowUpdateAction.Add)
            {
                CurrentRowCount++;
            }
            else
            {
                CurrentRowCount--;
            }
            LastRowUpdated = update.RowIndex;
        }
    }

    public class TestPropertyChangedConsumer : IReactivePropertyNotifiedConsumer
    {
        public string LastPropertyChanged { get; set; }
        public List<string> PropertiesChanged { get; set; }

        public TestPropertyChangedConsumer()
        {
            PropertiesChanged = new List<string>();
        }

        public void OnPropertyChanged(string propertyName)
        {
            PropertiesChanged.Add(propertyName);
            LastPropertyChanged = propertyName;
        }
    }
}
