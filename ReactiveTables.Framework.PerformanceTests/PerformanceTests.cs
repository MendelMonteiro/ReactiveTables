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
using System.Diagnostics;
using System.Threading;
using NUnit.Framework;
using ReactiveTables.Framework.Aggregate;
using ReactiveTables.Framework.Aggregate.Operations;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Synchronisation;
using ReactiveTables.Framework.Utils;

namespace ReactiveTables.Framework.PerformanceTests
{
    [TestFixture]
    public class PerformanceTests
    {
        const int RowCount = 100000;

        [Test]
        public void TestWriteToTable()
        {
            RunTest(TestWriteToTable, RowCount);
        }

        private static void TestWriteToTable(int rowCount)
        {
            var table = TableTestHelper.CreateReactiveTable();
            for (var i = 0; i < rowCount; i++)
            {
                var rowIndex = table.AddRow();
                table.SetValue(TestTableColumns.IdColumn, rowIndex, i);
                table.SetValue(TestTableColumns.StringColumn, rowIndex, $"Entry {i}");
                table.SetValue(TestTableColumns.DecimalColumn, rowIndex, (decimal) i);
            }
        }

        [Test]
        public void TestSyncrhonisedDuplicate()
        {
            RunTest(TestSyncrhonisedDuplicate, RowCount);
        }

        private void TestSyncrhonisedDuplicate(int rowCount)
        {
            var uiTable = TableTestHelper.CreateReactiveTable();
            var wireTable = new ReactiveTable(uiTable);
            new TableSynchroniser(wireTable, uiTable, new DefaultThreadMarshaller());

            for (var i = 0; i < rowCount; i++)
            {
                var rowIndex = wireTable.AddRow();
                wireTable.SetValue(TestTableColumns.IdColumn, rowIndex, i);
                wireTable.SetValue(TestTableColumns.StringColumn, rowIndex, $"Entry {i}");
                wireTable.SetValue(TestTableColumns.DecimalColumn, rowIndex, (decimal)i);
            }
        }

        [Test]
        public void TestSyncrhonisedPassThrough()
        {
            RunTest(TestSyncrhonisedPassThrough, RowCount);
        }

        private void TestSyncrhonisedPassThrough(int rowCount)
        {
            var uiTable = TableTestHelper.CreateReactiveTable();
            var wireTable = new ReactivePassThroughTable(uiTable, new DefaultThreadMarshaller());

            for (var i = 0; i < rowCount; i++)
            {
                var rowIndex = wireTable.AddRow();
                wireTable.SetValue(TestTableColumns.IdColumn, rowIndex, i);
                wireTable.SetValue(TestTableColumns.StringColumn, rowIndex, $"Entry {i}");
                wireTable.SetValue(TestTableColumns.DecimalColumn, rowIndex, (decimal)i);
            }

            Assert.AreEqual(uiTable.RowCount, wireTable.RowCount);
        }
        
        [Test]
        public void TestSyncrhonisedBatchPassThrough()
        {
            RunTest(TestSyncrhonisedBatchPassThrough, RowCount);
        }

        private void TestSyncrhonisedBatchPassThrough(int rowCount)
        {
            var uiTable = TableTestHelper.CreateReactiveTable();
            var delay = TimeSpan.FromMilliseconds(250);
            var wireTable = new ReactiveBatchedPassThroughTable(uiTable, new DefaultThreadMarshaller(), delay);

            for (var i = 0; i < rowCount; i++)
            {
                var rowIndex = wireTable.AddRow();
                wireTable.SetValue(TestTableColumns.IdColumn, rowIndex, i);
                wireTable.SetValue(TestTableColumns.StringColumn, rowIndex, $"Entry {i}");
                wireTable.SetValue(TestTableColumns.DecimalColumn, rowIndex, (decimal)i);
            }

            while (wireTable.GetRowUpdateCount() > 0)
            {
                Thread.Sleep(100);
            }
            Thread.Sleep(600);

            Assert.AreEqual(uiTable.RowCount, wireTable.RowCount);
        }


        private static void RunTest(Action<int> test, int rowCount)
        {
            var watch = new Stopwatch();
            watch.Start();

            test(rowCount);

            watch.Stop();
            Console.WriteLine("Test run for {0:N0} rows took {1:N0}ms", rowCount, watch.ElapsedMilliseconds);

            Console.WriteLine("Performance stats: ");
            ProcessInfoDumper.Dump();
        }

        [Test]
        public void TestGrouping()
        {
            var table1 = new ReactiveTable();
            var GroupColumnId = "GroupCol";
            var groupColumn = new ReactiveColumn<string>(GroupColumnId);
            table1.AddColumn(groupColumn);
            var ValueColumnId = "ValueCol";
            var reactiveColumn = new ReactiveColumn<int>(ValueColumnId);
            var valueColumn = reactiveColumn;
            table1.AddColumn(valueColumn);
            //            ReactiveTable table2 = new ReactiveTable();
            //            table2.AddColumn(new ReactiveColumn<string>("GroupColumn2"));
            //            table2.AddColumn(new ReactiveColumn<int>("ValueColumn2"));

            var groupedTable = new AggregatedTable(table1);
            groupedTable.GroupBy<string>(groupColumn.ColumnId);
            var CountColumnId = "Count";
            groupedTable.AddAggregate(groupColumn, CountColumnId, () => new Count<string>());
            var SumColumnId = "Sum";
            groupedTable.AddAggregate(valueColumn, SumColumnId, () => new Sum<int>());
            var AverageColumnId = "Average";
            groupedTable.AddAggregate(valueColumn, AverageColumnId, () => new Average<int>());
            var MinColumnId = "Min";
            groupedTable.AddAggregate(valueColumn, MinColumnId, () => new Min<int>());
            var MaxColumnId = "Max";
            groupedTable.AddAggregate(valueColumn, MaxColumnId, () => new Max<int>());

            RunTest(
                count =>
                {
                    for (var i = 0; i < count; i++)
                    {
                        var name1 = "Mendel" + i;
                        var name2 = "Marie" + i;
                        AddRow(name1, 42, table1, GroupColumnId, ValueColumnId);
                        AddRow(name2, 43, table1, GroupColumnId, ValueColumnId);
                        AddRow(name1, 44, table1, GroupColumnId, ValueColumnId);
                        AddRow(name2, 45, table1, GroupColumnId, ValueColumnId);
                        AddRow(name1, 46, table1, GroupColumnId, ValueColumnId);
                        AddRow(name2, 45, table1, GroupColumnId, ValueColumnId);
                    }
                }, 10000);
        }
        private void AddRow(string groupVal, int value, IWritableReactiveTable table1, string GroupColumnId, string ValueColumnId)
        {
            var row1 = table1.AddRow();
            table1.SetValue(GroupColumnId, row1, groupVal);
            table1.SetValue(ValueColumnId, row1, value);
        }
    }
}