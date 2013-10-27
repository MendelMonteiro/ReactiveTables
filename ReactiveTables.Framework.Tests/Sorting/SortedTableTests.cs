using System.Collections.Generic;
using NUnit.Framework;
using ReactiveTables.Framework.Sorting;

namespace ReactiveTables.Framework.Tests.Sorting
{
    [TestFixture]
    public class SortedTableTests
    {
        [Test]
        public void TestFilledTable()
        {
            var sourceTable = TestTableHelper.CreateReactiveTable();
            const int count = 1000;
            AddValuesInReverseOrder(count, sourceTable);

            var table = new SortedTable<string>(sourceTable, TestTableColumns.StringColumn, Comparer<string>.Default);
            CheckValuesAreSorted(count, table);
        }

        [Test]
        public void TestFillingTable()
        {
            var sourceTable = TestTableHelper.CreateReactiveTable();
            const int count = 1000;

            var table = new SortedTable<string>(sourceTable, TestTableColumns.StringColumn, Comparer<string>.Default);

            AddValuesInReverseOrder(count, sourceTable);
            CheckValuesAreSorted(count, table);
        }

        [Test]
        public void TestAddingToFilledTable()
        {
            var sourceTable = TestTableHelper.CreateReactiveTable();
            const int count = 1000;

            AddValuesInReverseOrder(count, sourceTable);

            var table = new SortedTable<string>(sourceTable, TestTableColumns.StringColumn, Comparer<string>.Default);
            CheckValuesAreSorted(count, table);

            AddValuesInReverseOrder(count, sourceTable, count);
            CheckValuesAreSorted(count * 2, table);
        }

        [Test]
        public void TestUpdatingSortKey()
        {
            var sourceTable = TestTableHelper.CreateReactiveTable();
            const int count = 1000;
            AddValuesInReverseOrder(count, sourceTable);

            var table = new SortedTable<string>(sourceTable, TestTableColumns.StringColumn, Comparer<string>.Default);
            CheckValuesAreSorted(count, table);

            int rowId = 5;
            sourceTable.SetValue(TestTableColumns.StringColumn, rowId, "_");

            Assert.AreEqual("_", table.GetValue(TestTableColumns.StringColumn, 0));

            rowId = 6;
            sourceTable.SetValue(TestTableColumns.StringColumn, rowId, "X");

            Assert.AreEqual("X", table.GetValue(TestTableColumns.StringColumn, count-1));
        }

        private static void AddValuesInReverseOrder(int count, ReactiveTable sourceTable, int startIndex = 0)
        {
            int end = startIndex + count - 1;
            for (int i = end; i >= startIndex; i--)
            {
                var rowId = sourceTable.AddRow();
                int id = i;
                sourceTable.SetValue(TestTableColumns.IdColumn, rowId, id);
                sourceTable.SetValue(TestTableColumns.StringColumn, rowId, string.Format("Item #{0}", id.ToString("0000")));
            }
        }

        private static void CheckValuesAreSorted(int count, SortedTable<string> table)
        {
            Assert.AreEqual(count, table.RowCount);
            for (int i = 0; i < table.RowCount; i++)
            {
                var id = table.GetValue<int>(TestTableColumns.IdColumn, i);
                var s = table.GetValue<string>(TestTableColumns.StringColumn, i);

                Assert.AreEqual(i, id);
                Assert.AreEqual(string.Format("Item #{0}", i.ToString("0000")), s);
            }
        }
    }
}
