using System.Collections.Generic;
using NUnit.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Columns.Calculated;
using ReactiveTables.Framework.Joins;
using ReactiveTables.Framework.UI;

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

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, 444);
            Assert.AreEqual(1, idCol1Updates);

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual(1, idCol1Updates);

            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 444);
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

            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 444, 1);

            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 1, 444, 888);
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

            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 1, 444, 888);

            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 444, 1);
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
            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 444, 1);

            // First row right side
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 1, 444, 888);

            // Second row right side
            rightRowId = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 2, 445, 889);

            // Second row left side
            leftRowId = leftTable.AddRow();
            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 445, 2);
        }

        private static void SetAndTestLeftRow(ReactiveTable leftTable, int leftRowId, int [] rowsUpdated, IReactiveTable joinedTable, int id, int expectedRowsUpdated, bool visibleInJoinTable = true)
        {
            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, id);
            Assert.AreEqual(expectedRowsUpdated, rowsUpdated[0]);
            if (visibleInJoinTable) Assert.AreEqual(id, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, leftRowId));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            if (visibleInJoinTable) Assert.AreEqual("hello", joinedTable.GetValue<string>(TestLeftColumns.StringColumn, leftRowId));
        }

        private static void SetAndTestRightRow(ReactiveTable rightTable, int rightRowId, int [] rowsUpdated, IReactiveTable joinedTable, int expectedRowsUpdated, int leftId, int id, bool visibleInJoinTable = true)
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
            Assert.AreEqual(expectedRowsUpdated, updateHandler.CurrentRowCount);
            if (visibleInJoinTable) Assert.AreEqual(id, joinedTable.GetValue<int>(TestLeftColumns.IdColumn, updateHandler.LastRowUpdated));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            if (visibleInJoinTable) Assert.AreEqual("hello", joinedTable.GetValue<string>(TestLeftColumns.StringColumn, updateHandler.LastRowUpdated));
        }

        private void SetAndTestRightRow(ReactiveTable rightTable, int rightRowId, RowUpdateHandler updateHandler, IReactiveTable joinedTable, int expectedRowsUpdated, int leftId, int id, bool visibleInJoinTable = true)
        {
            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, id);
            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, leftId);
            Assert.AreEqual(expectedRowsUpdated, updateHandler.CurrentRowCount);
            if (visibleInJoinTable) Assert.AreEqual(leftId, joinedTable.GetValue<int>(TestRightColumns.LeftIdColumn, updateHandler.LastRowUpdated));

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            if (visibleInJoinTable) Assert.AreEqual(9876m, joinedTable.GetValue<decimal>(TestRightColumns.DecimalColumn, updateHandler.LastRowUpdated));
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
            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 444, 1);

            // First row right side
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 1, 444, 888);

            // Second row right side
            rightRowId = rightTable.AddRow();
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 2, 444, 889);
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
            SetAndTestLeftRow(leftTable, leftRowId, rowsUpdated, joinedTable, 444, 0, false);
            
            // Right table first row
            SetAndTestRightRow(rightTable, rightRowId, rowsUpdated, joinedTable, 1, 444, 888);
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
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 444, 0, false);

            // Right table first row
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 1, 444, 888);

            // Right table second row
            rightRowId = rightTable.AddRow();
            Assert.AreEqual(1, updateHandler.CurrentRowCount);
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 2, 444, 889);

            // Right table third row
            rightRowId = rightTable.AddRow();
            Assert.AreEqual(2, updateHandler.CurrentRowCount);
            SetAndTestRightRow(rightTable, rightRowId, updateHandler, joinedTable, 3, 444, 890);

            // Left table second row
            leftRowId = leftTable.AddRow();
            Assert.AreEqual(3, updateHandler.CurrentRowCount);
            SetAndTestLeftRow(leftTable, leftRowId, updateHandler, joinedTable, 444, 6);
        }

        [Test]
        public void TestDelete()
        {
            ReactiveTable rightTable;
            IReactiveTable joinedTable;
            var leftTable = CreateJoinedTables(out rightTable, out joinedTable);

            int[] rowsUpdated = new int[1];
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(
                update => { if (update.Action == RowUpdate.RowUpdateAction.Add) rowsUpdated[0]++; else rowsUpdated[0]--; }, 
                null, null));

            var leftRowId = leftTable.AddRow();
            Assert.AreEqual(1, rowsUpdated[0]);

            var rightRowId = rightTable.AddRow();
            Assert.AreEqual(1, rowsUpdated[0]);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                      {
                                          if (update.Column.ColumnId == TestLeftColumns.IdColumn)
                                              idCol1Updates++;
                                          if (update.Column.ColumnId == TestRightColumns.LeftIdColumn)
                                              idCol2Updates++;
                                      }, null, null));

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, 444);
            Assert.AreEqual(1, idCol1Updates);

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual(1, idCol1Updates);

            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 444);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual(1, rowsUpdated[0]);
            leftTable.DeleteRow(leftRowId);
            Assert.AreEqual(0, rowsUpdated[0]);
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

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, 444);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);

            Assert.AreEqual(" - 0", joinedTable.GetValue<string>(columnId, 0));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual(1, idCol1Updates);

            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 444);
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

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, 444);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);

            Assert.AreEqual(" - 0", joinedTable.GetValue<string>(columnId, 0));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual(1, idCol1Updates);

            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 444);
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

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, 444);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual(1, idCol1Updates);

            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 444);
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

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, 444);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual(1, idCol1Updates);

            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 444);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            rightTable.SetValue(TestRightColumns.DecimalColumn, rightRowId, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            rightRowId = rightTable.AddRow();
            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 889);
            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 444);
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

            leftTable.SetValue(TestLeftColumns.IdColumn, leftRowId, 444);
            Assert.AreEqual(1, rowsUpdated[0]);
            Assert.AreEqual(1, idCol1Updates);

            Assert.AreEqual(" - 0", joinedTable.GetValue<string>(columnId, 0));

            leftTable.SetValue(TestLeftColumns.StringColumn, leftRowId, "hello");
            Assert.AreEqual(1, idCol1Updates);

            rightTable.SetValue(TestRightColumns.IdColumn, rightRowId, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 444);
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
            rightTable.SetValue(TestRightColumns.LeftIdColumn, rightRowId, 444);
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
            var leftTable = CreateleftTable();

            rightTable = CreaterightTable();

            joinedTable = leftTable.Join(rightTable, new Join<int>(leftTable, TestLeftColumns.IdColumn, rightTable, TestRightColumns.LeftIdColumn, joinType));
            return leftTable;
        }

        private static ReactiveTable CreaterightTable()
        {
            ReactiveTable rightTable = new ReactiveTable();
            rightTable.AddColumn(new ReactiveColumn<int>(TestRightColumns.IdColumn));
            rightTable.AddColumn(new ReactiveColumn<int>(TestRightColumns.LeftIdColumn));
            rightTable.AddColumn(new ReactiveColumn<decimal>(TestRightColumns.DecimalColumn));
            return rightTable;
        }

        private static ReactiveTable CreateleftTable()
        {
            ReactiveTable leftTable = new ReactiveTable();
            leftTable.AddColumn(new ReactiveColumn<int>(TestLeftColumns.IdColumn));
            leftTable.AddColumn(new ReactiveColumn<string>(TestLeftColumns.StringColumn));
            leftTable.AddColumn(new ReactiveColumn<decimal>(TestLeftColumns.DecimalColumn));
            return leftTable;
        }
    }

    class RowUpdateHandler
    {
        public int CurrentRowCount { get; set; }
        public int LastRowUpdated { get; set; }
        public void OnRowUpdate(RowUpdate update)
        {
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
