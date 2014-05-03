using System;
using NUnit.Framework;
using ReactiveTables.Framework.Aggregate;

namespace ReactiveTables.Framework.Tests.Aggregate
{
    [TestFixture]
    public class AggregateTests
    {
        /// <summary>
        /// Test a grouped column by using a standard table which has one groupable column
        /// and one value column upon which we can perform aggregate functions.
        /// </summary>
        [Test]
        public void TestGroupByOneColumn()
        {
            // Create group by
            var baseTable = TestTableHelper.CreateReactiveTable();
            var groupedTable = new AggregatedTable(baseTable);

            groupedTable.GroupByColumn<string>(TestTableColumns.StringColumn);

            RowUpdateHandler rowUpdates = new RowUpdateHandler();
            groupedTable.Subscribe(rowUpdates);

            ColumnUpdateHandler colUpdates = new ColumnUpdateHandler();
            groupedTable.Subscribe(colUpdates.OnColumnUpdate);

            // Add values
            var row1 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row1, "Value1");
            Assert.AreEqual(1, groupedTable.RowCount);
            Assert.AreEqual(1, rowUpdates.CurrentRowCount);
            Assert.AreEqual("Value1", groupedTable.GetValue<string>(TestTableColumns.StringColumn, colUpdates.LastRowUpdated));

            var row2 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row2, "Value2");
            Assert.AreEqual(2, groupedTable.RowCount);
            Assert.AreEqual(2, rowUpdates.CurrentRowCount);
            Assert.AreEqual("Value2", groupedTable.GetValue<string>(TestTableColumns.StringColumn, colUpdates.LastRowUpdated));

            var row3 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row3, "Value1");
            Assert.AreEqual(2, groupedTable.RowCount);
            Assert.AreEqual(2, rowUpdates.CurrentRowCount);
            Assert.AreEqual("Value1", groupedTable.GetValue<string>(TestTableColumns.StringColumn, colUpdates.LastRowUpdated));

            // Modify grouped columns
            baseTable.SetValue(TestTableColumns.StringColumn, row2, "Value1");
            Assert.AreEqual(1, groupedTable.RowCount);
            Assert.AreEqual(1, rowUpdates.CurrentRowCount);
            Assert.AreEqual("Value1", groupedTable.GetValue<string>(TestTableColumns.StringColumn, colUpdates.LastRowUpdated));

            baseTable.SetValue(TestTableColumns.StringColumn, row3, "Value3");
            Assert.AreEqual(2, groupedTable.RowCount);
            Assert.AreEqual(2, rowUpdates.CurrentRowCount);
            Assert.AreEqual("Value3", groupedTable.GetValue<string>(TestTableColumns.StringColumn, colUpdates.LastRowUpdated));

            // Remove rows
            baseTable.DeleteRow(row1);
            Assert.AreEqual(2, groupedTable.RowCount);
            Assert.AreEqual(2, rowUpdates.CurrentRowCount);
            
            baseTable.DeleteRow(row2);
            Assert.AreEqual(1, groupedTable.RowCount);
            Assert.AreEqual(1, rowUpdates.CurrentRowCount);

            baseTable.DeleteRow(row3);
            Assert.AreEqual(0, groupedTable.RowCount);
            Assert.AreEqual(0, rowUpdates.CurrentRowCount);
        }

        [Test]
        public void TestWithCalculatedColumns()
        {

        }

        [Test]
        public void TestGroupByMultipleColumns()
        {

        }

        [Test]
        public void TestGroupByOnExistingValues()
        {

        }

        [Test]
        public void TestGroupByWithCount()
        {

        }

        [Test]
        public void TestGroupByWithSum()
        {

        }

        [Test]
        public void TestGroupByWithMin()
        {

        }

        [Test]
        public void TestGroupByWithMax()
        {

        }
    }
}