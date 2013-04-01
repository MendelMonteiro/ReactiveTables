using NUnit.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Columns.Calculated;
using ReactiveTables.Framework.Joins;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Framework.Tests.Joins
{
    public static class TestColumns1
    {
        public static readonly string IdColumn = "Test1.IdColumn";
        public static readonly string StringColumn = "Test1.String";
        public static readonly string DecimalColumn = "Test1.Decimal";
    }

    public static class TestColumns2
    {
        public static readonly string IdColumn = "Test2.IdColumn";
        public static readonly string Test1IdColumn = "Test2.Test1Id";
        public static readonly string DecimalColumn = "Test2.Decimal";
    }

    [TestFixture]
    public class InnerJoinTest
    {
        [Test]
        public void TestBasicJoin()
        {
            ReactiveTable table2;
            IReactiveTable joinedTable;
            var table1 = CreateJoinedTables(out table2, out joinedTable);

            int rowsUpdated = 0;
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated++, null, null));

            var rowId1 = table1.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            var rowId2 = table2.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                          {
                                              if (update.Column.ColumnId == TestColumns1.IdColumn)
                                                  idCol1Updates++;
                                              if (update.Column.ColumnId == TestColumns2.Test1IdColumn)
                                                  idCol2Updates++;
                                          }, null, null));

            table1.SetValue(TestColumns1.IdColumn, rowId1, 444);
            Assert.AreEqual(1, idCol1Updates);

            table1.SetValue(TestColumns1.StringColumn, rowId1, "hello");
            Assert.AreEqual(1, idCol1Updates);

            table2.SetValue(TestColumns2.IdColumn, rowId2, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            table2.SetValue(TestColumns2.Test1IdColumn, rowId2, 444);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            table2.SetValue(TestColumns2.DecimalColumn, rowId2, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);
        }

        [Test]
        public void TestDelete()
        {
            ReactiveTable table2;
            IReactiveTable joinedTable;
            var table1 = CreateJoinedTables(out table2, out joinedTable);

            int rowsUpdated = 0;
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(
                update => { if (update.Action == RowUpdate.RowUpdateAction.Add) rowsUpdated++; else rowsUpdated--; }, 
                null, null));

            var rowId1 = table1.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            var rowId2 = table2.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                      {
                                          if (update.Column.ColumnId == TestColumns1.IdColumn)
                                              idCol1Updates++;
                                          if (update.Column.ColumnId == TestColumns2.Test1IdColumn)
                                              idCol2Updates++;
                                      }, null, null));

            table1.SetValue(TestColumns1.IdColumn, rowId1, 444);
            Assert.AreEqual(1, idCol1Updates);

            table1.SetValue(TestColumns1.StringColumn, rowId1, "hello");
            Assert.AreEqual(1, idCol1Updates);

            table2.SetValue(TestColumns2.IdColumn, rowId2, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            table2.SetValue(TestColumns2.Test1IdColumn, rowId2, 444);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            table2.SetValue(TestColumns2.DecimalColumn, rowId2, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual(1, rowsUpdated);
            table1.DeleteRow(rowId1);
            Assert.AreEqual(0, rowsUpdated);
        }

        [Test]
        public void TestCalculatedColumnAddedBefore()
        {
            ReactiveTable table2;
            IReactiveTable joinedTable;
            var table1 = CreateJoinedTables(out table2, out joinedTable);

            const string columnId = "CalcCol";
            joinedTable.AddColumn(new ReactiveCalculatedColumn2<string, string, decimal>(
                                      columnId,
                                      (IReactiveColumn<string>) table1.Columns[TestColumns1.StringColumn],
                                      (IReactiveColumn<decimal>) table2.Columns[TestColumns2.DecimalColumn],
                                      (str, dec) => string.Format("{0} - {1}", str, dec)));

            int rowsUpdated = 0;
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated++, null, null));

            var rowId1 = table1.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            var rowId2 = table2.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                      {
                                          if (update.Column.ColumnId == TestColumns1.IdColumn)
                                              idCol1Updates++;
                                          if (update.Column.ColumnId == TestColumns2.Test1IdColumn)
                                              idCol2Updates++;
                                      }, null, null));

            table1.SetValue(TestColumns1.IdColumn, rowId1, 444);
            Assert.AreEqual(1, idCol1Updates);

            Assert.AreEqual(" - 0", joinedTable.GetValue<string>(columnId, 0));

            table1.SetValue(TestColumns1.StringColumn, rowId1, "hello");
            Assert.AreEqual(1, idCol1Updates);

            table2.SetValue(TestColumns2.IdColumn, rowId2, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            table2.SetValue(TestColumns2.Test1IdColumn, rowId2, 444);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual("hello - 0", joinedTable.GetValue<string>(columnId, 0));

            table2.SetValue(TestColumns2.DecimalColumn, rowId2, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual("hello - 9876", joinedTable.GetValue<string>(columnId, 0));
        }

        [Test]
        public void TestCalculatedColumnUpdateValues()
        {
            ReactiveTable table2;
            IReactiveTable joinedTable;
            var table1 = CreateJoinedTables(out table2, out joinedTable);

            const string columnId = "CalcCol";
            joinedTable.AddColumn(new ReactiveCalculatedColumn2<string, string, decimal>(
                                      columnId,
                                      (IReactiveColumn<string>)table1.Columns[TestColumns1.StringColumn],
                                      (IReactiveColumn<decimal>)table2.Columns[TestColumns2.DecimalColumn],
                                      (str, dec) => string.Format("{0} - {1}", str, dec)));

            int rowsUpdated = 0;
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated++, null, null));

            var rowId1 = table1.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            var rowId2 = table2.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                      {
                                          if (update.Column.ColumnId == TestColumns1.IdColumn)
                                              idCol1Updates++;
                                          if (update.Column.ColumnId == TestColumns2.Test1IdColumn)
                                              idCol2Updates++;
                                      }, null, null));

            table1.SetValue(TestColumns1.IdColumn, rowId1, 444);
            Assert.AreEqual(1, idCol1Updates);

            Assert.AreEqual(" - 0", joinedTable.GetValue<string>(columnId, 0));

            table1.SetValue(TestColumns1.StringColumn, rowId1, "hello");
            Assert.AreEqual(1, idCol1Updates);

            table2.SetValue(TestColumns2.IdColumn, rowId2, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            table2.SetValue(TestColumns2.Test1IdColumn, rowId2, 444);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual("hello - 0", joinedTable.GetValue<string>(columnId, 0));

            table2.SetValue(TestColumns2.DecimalColumn, rowId2, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual("hello - 9876", joinedTable.GetValue<string>(columnId, 0));

            table2.SetValue(TestColumns2.DecimalColumn, rowId2, 6789m);
            Assert.AreEqual(6789m, joinedTable.GetValue<decimal>(TestColumns2.DecimalColumn, rowId2));
            Assert.AreEqual("hello - 6789", joinedTable.GetValue<string>(columnId, 0));

            PropertyChangedNotifier notifier = new PropertyChangedNotifier(joinedTable);
            var testConsumer = new TestPropertyChangedConsumer();
            notifier.RegisterPropertyNotifiedConsumer(testConsumer, 0);

            table2.SetValue(TestColumns2.DecimalColumn, rowId2, 7777m);
            Assert.AreEqual("Decimal", testConsumer.LastPropertyChanged);
        }

        [Test]
        public void TestCalculatedColumnAddedAfter()
        {
            ReactiveTable table2;
            IReactiveTable joinedTable;
            var table1 = CreateJoinedTables(out table2, out joinedTable);

            int rowsUpdated = 0;
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated++, null, null));

            var rowId1 = table1.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            var rowId2 = table2.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                      {
                                          if (update.Column.ColumnId == TestColumns1.IdColumn)
                                              idCol1Updates++;
                                          if (update.Column.ColumnId == TestColumns2.Test1IdColumn)
                                              idCol2Updates++;
                                      }, null, null));

            table1.SetValue(TestColumns1.IdColumn, rowId1, 444);
            Assert.AreEqual(1, idCol1Updates);

            table1.SetValue(TestColumns1.StringColumn, rowId1, "hello");
            Assert.AreEqual(1, idCol1Updates);

            table2.SetValue(TestColumns2.IdColumn, rowId2, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            table2.SetValue(TestColumns2.Test1IdColumn, rowId2, 444);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            table2.SetValue(TestColumns2.DecimalColumn, rowId2, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            const string columnId = "CalcCol";
            joinedTable.AddColumn(new ReactiveCalculatedColumn2<string, string, decimal>(
                                      columnId,
                                      (IReactiveColumn<string>)table1.Columns[TestColumns1.StringColumn],
                                      (IReactiveColumn<decimal>)table2.Columns[TestColumns2.DecimalColumn],
                                      (str, dec) => string.Format("{0} - {1}", str, dec)));

            Assert.AreEqual("hello - 9876", joinedTable.GetValue<string>(columnId, 0));
        }

        [Test]
        public void TestJoin1ToN()
        {
            ReactiveTable table2;
            IReactiveTable joinedTable;
            var table1 = CreateJoinedTables(out table2, out joinedTable);

            int rowsUpdated = 0;
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated++, null, null));

            var rowId1 = table1.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            var rowId2 = table2.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                      {
                                          if (update.Column.ColumnId == TestColumns1.IdColumn)
                                              idCol1Updates++;
                                          if (update.Column.ColumnId == TestColumns2.Test1IdColumn)
                                              idCol2Updates++;
                                      }, null, null));

            table1.SetValue(TestColumns1.IdColumn, rowId1, 444);
            Assert.AreEqual(1, idCol1Updates);

            table1.SetValue(TestColumns1.StringColumn, rowId1, "hello");
            Assert.AreEqual(1, idCol1Updates);

            table2.SetValue(TestColumns2.IdColumn, rowId2, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            table2.SetValue(TestColumns2.Test1IdColumn, rowId2, 444);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            table2.SetValue(TestColumns2.DecimalColumn, rowId2, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            rowId2 = table2.AddRow();
            table2.SetValue(TestColumns2.IdColumn, rowId2, 889);
            table2.SetValue(TestColumns2.Test1IdColumn, rowId2, 444);
            Assert.AreEqual(2, idCol2Updates);
        }

        [Test]
        public void TestCalculatedJoin1ToN()
        {
            ReactiveTable table2;
            IReactiveTable joinedTable;
            var table1 = CreateJoinedTables(out table2, out joinedTable);

            const string columnId = "CalcCol";
            joinedTable.AddColumn(new ReactiveCalculatedColumn2<string, string, decimal>(
                                      columnId,
                                      (IReactiveColumn<string>)table1.Columns[TestColumns1.StringColumn],
                                      (IReactiveColumn<decimal>)table2.Columns[TestColumns2.DecimalColumn],
                                      (str, dec) => string.Format("{0} - {1}", str, dec)));

            int rowsUpdated = 0;
            joinedTable.Subscribe(new DelegateObserver<RowUpdate>(update => rowsUpdated++, null, null));

            var rowId1 = table1.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            var rowId2 = table2.AddRow();
            Assert.AreEqual(1, rowsUpdated);

            int idCol1Updates = 0, idCol2Updates = 0;
            joinedTable.Subscribe(new DelegateObserver<ColumnUpdate>(
                                      update =>
                                      {
                                          if (update.Column.ColumnId == TestColumns1.IdColumn)
                                              idCol1Updates++;
                                          if (update.Column.ColumnId == TestColumns2.Test1IdColumn)
                                              idCol2Updates++;
                                      }, null, null));

            table1.SetValue(TestColumns1.IdColumn, rowId1, 444);
            Assert.AreEqual(1, idCol1Updates);

            Assert.AreEqual(" - 0", joinedTable.GetValue<string>(columnId, 0));

            table1.SetValue(TestColumns1.StringColumn, rowId1, "hello");
            Assert.AreEqual(1, idCol1Updates);

            table2.SetValue(TestColumns2.IdColumn, rowId2, 888);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(0, idCol2Updates);

            table2.SetValue(TestColumns2.Test1IdColumn, rowId2, 444);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual("hello - 0", joinedTable.GetValue<string>(columnId, 0));

            table2.SetValue(TestColumns2.DecimalColumn, rowId2, 9876m);
            Assert.AreEqual(1, idCol1Updates);
            Assert.AreEqual(1, idCol2Updates);

            Assert.AreEqual("hello - 9876", joinedTable.GetValue<string>(columnId, 0));

            rowId2 = table2.AddRow();
            table2.SetValue(TestColumns2.IdColumn, rowId2, 889);
            table2.SetValue(TestColumns2.Test1IdColumn, rowId2, 444);
            Assert.AreEqual(2, idCol2Updates);
            Assert.AreEqual("hello - 0", joinedTable.GetValue<string>(columnId, 1));
        }

        private static ReactiveTable CreateJoinedTables(out ReactiveTable table2, out IReactiveTable joinedTable)
        {
            var table1 = CreateTable1();

            table2 = CreateTable2();

            joinedTable = table1.Join(table2, new InnerJoin<int>(table1, TestColumns1.IdColumn, table2, TestColumns2.Test1IdColumn));
            return table1;
        }

        private static ReactiveTable CreateTable2()
        {
            ReactiveTable table2 = new ReactiveTable();
            table2.AddColumn(new ReactiveColumn<int>(TestColumns2.IdColumn));
            table2.AddColumn(new ReactiveColumn<int>(TestColumns2.Test1IdColumn));
            table2.AddColumn(new ReactiveColumn<decimal>(TestColumns2.DecimalColumn));
            return table2;
        }

        private static ReactiveTable CreateTable1()
        {
            ReactiveTable table1 = new ReactiveTable();
            table1.AddColumn(new ReactiveColumn<int>(TestColumns1.IdColumn));
            table1.AddColumn(new ReactiveColumn<string>(TestColumns1.StringColumn));
            table1.AddColumn(new ReactiveColumn<decimal>(TestColumns1.DecimalColumn));
            return table1;
        }
    }

    public class TestPropertyChangedConsumer : IReactivePropertyNotifiedConsumer
    {
        public string LastPropertyChanged { get; set; }

        public void OnPropertyChanged(string propertyName)
        {
            LastPropertyChanged = propertyName;
        }
    }
}
