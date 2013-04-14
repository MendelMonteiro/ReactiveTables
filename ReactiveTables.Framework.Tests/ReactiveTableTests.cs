using NUnit.Framework;

namespace ReactiveTables.Framework.Tests
{
    [TestFixture]
    public class ReactiveTableTests
    {
        [Test]
        public void TestAdd()
        {
            var table = TableTestHelper.CreateReactiveTable();

            var rowId1 = table.AddRow();
            Assert.AreEqual(1, table.RowCount);
            TableTestHelper.SetAndTestValue(table, rowId1, 123, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, "Hello", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, 321m, TestTableColumns.DecimalColumn);

            var rowId2 = table.AddRow();
            Assert.AreEqual(2, table.RowCount);
            TableTestHelper.SetAndTestValue(table, rowId2, 1234, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, "Hello 12", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, 3214m, TestTableColumns.DecimalColumn);
        }

        [Test]
        public void TestUpdate()
        {
            var table = TableTestHelper.CreateReactiveTable();

            var rowId1 = table.AddRow();
            Assert.AreEqual(1, table.RowCount);
            TableTestHelper.SetAndTestValue(table, rowId1, 123, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, "Hello", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, 321m, TestTableColumns.DecimalColumn);

            var rowId2 = table.AddRow();
            Assert.AreEqual(2, table.RowCount);
            TableTestHelper.SetAndTestValue(table, rowId2, 1234, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, "Hello 12", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, 3214m, TestTableColumns.DecimalColumn);

            TableTestHelper.SetAndTestValue(table, rowId2, 12341, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, "Hello", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, 321m, TestTableColumns.DecimalColumn);

            TableTestHelper.SetAndTestValue(table, rowId1, 321, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, "Hello 334", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, 32132m, TestTableColumns.DecimalColumn);
        }

        [Test]
        public void TestDelete()
        {
            var table = TableTestHelper.CreateReactiveTable();

            var rowId1 = table.AddRow();
            Assert.AreEqual(1, table.RowCount);
            TableTestHelper.SetAndTestValue(table, rowId1, 123, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, "Hello", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId1, 321m, TestTableColumns.DecimalColumn);

            var rowId2 = table.AddRow();
            Assert.AreEqual(2, table.RowCount);
            TableTestHelper.SetAndTestValue(table, rowId2, 1234, TestTableColumns.IdColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, "Hello 12", TestTableColumns.StringColumn);
            TableTestHelper.SetAndTestValue(table, rowId2, 3214m, TestTableColumns.DecimalColumn);

            table.DeleteRow(rowId2);
            Assert.AreEqual(1, table.RowCount);

            table.DeleteRow(rowId1);
            Assert.AreEqual(0, table.RowCount);
        }
    }
}
