using NUnit.Framework;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Tests
{
    public static class TestTableColumns
    {
        public const string IdColumn = "TestTable.IdColumn";
        public const string StringColumn = "TestTable.StringColumn";
        public const string DecimalColumn = "TestTable.DecimalColumn";
    }

    [TestFixture]
    public class ReactiveTableTests
    {
        [Test]
        public void TestAdd()
        {
            var table = CreateReactiveTable();

            var rowId1 = table.AddRow();
            Assert.AreEqual(1, table.RowCount);
            SetAndTestValue(table, rowId1, 123, TestTableColumns.IdColumn);
            SetAndTestValue(table, rowId1, "Hello", TestTableColumns.StringColumn);
            SetAndTestValue(table, rowId1, 321m, TestTableColumns.DecimalColumn);

            var rowId2 = table.AddRow();
            Assert.AreEqual(2, table.RowCount);
            SetAndTestValue(table, rowId2, 1234, TestTableColumns.IdColumn);
            SetAndTestValue(table, rowId2, "Hello 12", TestTableColumns.StringColumn);
            SetAndTestValue(table, rowId2, 3214m, TestTableColumns.DecimalColumn);
        }

        [Test]
        public void TestUpdate()
        {
            var table = CreateReactiveTable();

            var rowId1 = table.AddRow();
            Assert.AreEqual(1, table.RowCount);
            SetAndTestValue(table, rowId1, 123, TestTableColumns.IdColumn);
            SetAndTestValue(table, rowId1, "Hello", TestTableColumns.StringColumn);
            SetAndTestValue(table, rowId1, 321m, TestTableColumns.DecimalColumn);

            var rowId2 = table.AddRow();
            Assert.AreEqual(2, table.RowCount);
            SetAndTestValue(table, rowId2, 1234, TestTableColumns.IdColumn);
            SetAndTestValue(table, rowId2, "Hello 12", TestTableColumns.StringColumn);
            SetAndTestValue(table, rowId2, 3214m, TestTableColumns.DecimalColumn);

            SetAndTestValue(table, rowId2, 12341, TestTableColumns.IdColumn);
            SetAndTestValue(table, rowId2, "Hello", TestTableColumns.StringColumn);
            SetAndTestValue(table, rowId2, 321m, TestTableColumns.DecimalColumn);

            SetAndTestValue(table, rowId1, 321, TestTableColumns.IdColumn);
            SetAndTestValue(table, rowId1, "Hello 334", TestTableColumns.StringColumn);
            SetAndTestValue(table, rowId1, 32132m, TestTableColumns.DecimalColumn);
        }

        [Test]
        public void TestDelete()
        {
            var table = CreateReactiveTable();

            var rowId1 = table.AddRow();
            Assert.AreEqual(1, table.RowCount);
            SetAndTestValue(table, rowId1, 123, TestTableColumns.IdColumn);
            SetAndTestValue(table, rowId1, "Hello", TestTableColumns.StringColumn);
            SetAndTestValue(table, rowId1, 321m, TestTableColumns.DecimalColumn);

            var rowId2 = table.AddRow();
            Assert.AreEqual(2, table.RowCount);
            SetAndTestValue(table, rowId2, 1234, TestTableColumns.IdColumn);
            SetAndTestValue(table, rowId2, "Hello 12", TestTableColumns.StringColumn);
            SetAndTestValue(table, rowId2, 3214m, TestTableColumns.DecimalColumn);

            table.DeleteRow(rowId2);
            Assert.AreEqual(1, table.RowCount);

            table.DeleteRow(rowId1);
            Assert.AreEqual(0, table.RowCount);
        }

        private static ReactiveTable CreateReactiveTable()
        {
            ReactiveTable table = new ReactiveTable();
            table.AddColumn(new ReactiveColumn<int>(TestTableColumns.IdColumn));
            table.AddColumn(new ReactiveColumn<string>(TestTableColumns.StringColumn));
            table.AddColumn(new ReactiveColumn<decimal>(TestTableColumns.DecimalColumn));
            return table;
        }

        private static void SetAndTestValue<T>(ReactiveTable table, int rowId, T value, string columnId)
        {
            table.SetValue(columnId, rowId, value);
            Assert.AreEqual(value, table.GetValue<T>(columnId, rowId));
        }
    }
}
