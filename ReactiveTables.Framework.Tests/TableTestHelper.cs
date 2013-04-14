using NUnit.Framework;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Tests
{
    static internal class TableTestHelper
    {
        public static ReactiveTable CreateReactiveTable()
        {
            ReactiveTable table = new ReactiveTable();
            table.AddColumn(new ReactiveColumn<int>(TestTableColumns.IdColumn));
            table.AddColumn(new ReactiveColumn<string>(TestTableColumns.StringColumn));
            table.AddColumn(new ReactiveColumn<decimal>(TestTableColumns.DecimalColumn));
            return table;
        }

        public static void SetAndTestValue<T>(ReactiveTable table, int rowId, T value, string columnId)
        {
            table.SetValue(columnId, rowId, value);
            Assert.AreEqual(value, table.GetValue<T>(columnId, rowId));
        }

        public static void SetAndTestValue<T>(ReactiveTable setTable, ReactiveTable getTable, int setRowId, int getRowId, T value, string columnId)
        {
            setTable.SetValue(columnId, setRowId, value);
            TestValue(getTable, getRowId, value, columnId);
        }

        public static void TestValue<T>(ReactiveTable table, int rowId, T value, string columnId)
        {
            Assert.AreEqual(value, table.GetValue<T>(columnId, rowId));
        }

        public static void SetAndTestValueNotPresent<T>(ReactiveTable setTable, ReactiveTable getTable, int setRowId, int getRowId, T value, string columnId)
        {
            setTable.SetValue(columnId, setRowId, value);
            Assert.AreEqual(default(T), getTable.GetValue<T>(columnId, getRowId));
        }
    }
}