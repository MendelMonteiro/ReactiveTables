﻿// This file is part of ReactiveTables.
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
using NUnit.Framework;
using ReactiveTables.Framework.Aggregate;
using ReactiveTables.Framework.Aggregate.Operations;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Columns.Calculated;

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

            groupedTable.GroupBy<string>(TestTableColumns.StringColumn);

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
            var baseTable = TestTableHelper.CreateReactiveTable();
            var groupedTable = new AggregatedTable(baseTable);

            var column = groupedTable.GroupBy<string>(TestTableColumns.StringColumn);

            RowUpdateHandler rowUpdates = new RowUpdateHandler();
            groupedTable.Subscribe(rowUpdates);

            ColumnUpdateHandler colUpdates = new ColumnUpdateHandler();
            groupedTable.Subscribe(colUpdates.OnColumnUpdate);

            var groupedColumn = (IReactiveColumn<string>)column;
            const string groupedCalc1 = "Grouped.Calc1";
            groupedTable.AddColumn(new ReactiveCalculatedColumn1<string, string>(groupedCalc1,
                                                                                 groupedColumn,
                                                                                 s => s + "Calc"));
            
            var row1 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row1, "Value1");
            Assert.AreEqual(1, groupedTable.RowCount);
            Assert.AreEqual(1, rowUpdates.CurrentRowCount);
            Assert.AreEqual("Value1", groupedTable.GetValue<string>(TestTableColumns.StringColumn, colUpdates.LastRowUpdated));
            Assert.AreEqual("Value1Calc", groupedTable.GetValue<string>(groupedCalc1, colUpdates.LastRowUpdated));

            var row2 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row2, "Value3");
            Assert.AreEqual(2, groupedTable.RowCount);
            Assert.AreEqual(2, rowUpdates.CurrentRowCount);
            Assert.AreEqual("Value3", groupedTable.GetValue<string>(TestTableColumns.StringColumn, colUpdates.LastRowUpdated));
            Assert.AreEqual("Value3Calc", groupedTable.GetValue<string>(groupedCalc1, colUpdates.LastRowUpdated));

            var row3 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row3, "Value3");
            Assert.AreEqual(2, groupedTable.RowCount);
            Assert.AreEqual(2, rowUpdates.CurrentRowCount);
            Assert.AreEqual("Value3", groupedTable.GetValue<string>(TestTableColumns.StringColumn, colUpdates.LastRowUpdated));
            Assert.AreEqual("Value3Calc", groupedTable.GetValue<string>(groupedCalc1, colUpdates.LastRowUpdated));

            baseTable.SetValue(TestTableColumns.StringColumn, row1, "Value2");
            Assert.AreEqual(2, groupedTable.RowCount);
            Assert.AreEqual(2, rowUpdates.CurrentRowCount);
            Assert.AreEqual("Value2", groupedTable.GetValue<string>(TestTableColumns.StringColumn, colUpdates.LastRowUpdated));
            Assert.AreEqual("Value2Calc", groupedTable.GetValue<string>(groupedCalc1, colUpdates.LastRowUpdated));
        }

        [Test]
        public void TestGroupByMultipleColumns()
        {
            // Create group by
            var baseTable = TestTableHelper.CreateReactiveTable();
            var groupedTable = new AggregatedTable(baseTable);

            groupedTable.GroupBy<string>(TestTableColumns.StringColumn);
            groupedTable.GroupBy<int>(TestTableColumns.IdColumn);
            
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

            baseTable.SetValue(TestTableColumns.IdColumn, row1, 42);
            Assert.AreEqual(1, groupedTable.RowCount);
            Assert.AreEqual(1, rowUpdates.CurrentRowCount);
            Assert.AreEqual(42L, groupedTable.GetValue<int>(TestTableColumns.IdColumn, colUpdates.LastRowUpdated));

            baseTable.SetValue(TestTableColumns.StringColumn, row1, "Value2");
            Assert.AreEqual("Value2", groupedTable.GetValue<string>(TestTableColumns.StringColumn, colUpdates.LastRowUpdated));

            baseTable.SetValue(TestTableColumns.IdColumn, row1, 43);
            Assert.AreEqual(43, groupedTable.GetValue<int>(TestTableColumns.IdColumn, colUpdates.LastRowUpdated));
        }

        [Test]
        public void TestGroupByOnExistingValues()
        {
            // Create group by
            var baseTable = TestTableHelper.CreateReactiveTable();

            // Add values
            var row1 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row1, "Value1");

            var row2 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row2, "Value2");

            var row3 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row3, "Value1");

            // Modify grouped columns
            baseTable.SetValue(TestTableColumns.StringColumn, row2, "Value1");
            baseTable.SetValue(TestTableColumns.StringColumn, row3, "Value3");

            // Now group it
            var groupedTable = new AggregatedTable(baseTable);
            groupedTable.GroupBy<string>(TestTableColumns.StringColumn);
            var countCol = "CountCol";
            groupedTable.AddAggregate((IReactiveColumn<string>) baseTable.Columns[TestTableColumns.StringColumn],
                                      countCol,
                                      () => new Count<string>());

            RowUpdateHandler rowUpdates = new RowUpdateHandler();
            groupedTable.Subscribe(rowUpdates);
            ColumnUpdateHandler colUpdates = new ColumnUpdateHandler();
            groupedTable.Subscribe(colUpdates.OnColumnUpdate);
            groupedTable.FinishInitialisation();

            Assert.AreEqual(2, groupedTable.RowCount);
            Assert.AreEqual(2, rowUpdates.CurrentRowCount);
            Assert.AreEqual("Value1", groupedTable.GetValue<string>(TestTableColumns.StringColumn, 0));
            Assert.AreEqual("Value3", groupedTable.GetValue<string>(TestTableColumns.StringColumn, 1));
            Assert.AreEqual(2, groupedTable.GetValue<int>(countCol, 0));
            Assert.AreEqual(1, groupedTable.GetValue<int>(countCol, 1));

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
        public void TestGroupByWithCount()
        {
            // Create group by
            var baseTable = TestTableHelper.CreateReactiveTable();
            var groupedTable = new AggregatedTable(baseTable);
            RowUpdateHandler rowUpdates = new RowUpdateHandler();
            groupedTable.Subscribe(rowUpdates);
            ColumnUpdateHandler colUpdates = new ColumnUpdateHandler();
            groupedTable.Subscribe(colUpdates.OnColumnUpdate);
            
            groupedTable.GroupBy<string>(TestTableColumns.StringColumn);
            groupedTable.GroupBy<int>(TestTableColumns.IdColumn);

            var countColumn = "Aggregate.Count";
            groupedTable.AddAggregate<string, int>((IReactiveColumn<string>) baseTable.Columns[TestTableColumns.StringColumn],
                                                   countColumn,
//                                                   (s, c, b) => b ? c + 1 : c - 1);
                                                   () => new Count<string>());

            var row1 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row1, "Value1");
            Assert.AreEqual(1, groupedTable.RowCount);
            Assert.AreEqual(1, groupedTable.GetValue<int>(countColumn, colUpdates.LastRowUpdated));

            var row2 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row2, "Value1");
            Assert.AreEqual(1, groupedTable.RowCount);
            Assert.AreEqual(2, groupedTable.GetValue<int>(countColumn, colUpdates.LastRowUpdated));

            var row3 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row3, "Value2");
            Assert.AreEqual(2, groupedTable.RowCount);
            Assert.AreEqual(2, groupedTable.GetValue<int>(countColumn, 0));
            Assert.AreEqual(1, groupedTable.GetValue<int>(countColumn, colUpdates.LastRowUpdated));

            baseTable.SetValue(TestTableColumns.StringColumn, row2, "Value2");
            Assert.AreEqual(2, groupedTable.RowCount);
            Assert.AreEqual(1, groupedTable.GetValue<int>(countColumn, 0));
            Assert.AreEqual(2, groupedTable.GetValue<int>(countColumn, 1));
        }

        [Test]
        public void TestGroupByWithSum()
        {
            // Create group by
            var baseTable = TestTableHelper.CreateReactiveTable();
            var groupedTable = new AggregatedTable(baseTable);
            RowUpdateHandler rowUpdates = new RowUpdateHandler();
            groupedTable.Subscribe(rowUpdates);
            ColumnUpdateHandler colUpdates = new ColumnUpdateHandler();
            groupedTable.Subscribe(colUpdates.OnColumnUpdate);
            
            groupedTable.GroupBy<string>(TestTableColumns.StringColumn);

            var sumColumn = "Aggregate.Sum";
            groupedTable.AddAggregate<int, int>((IReactiveColumn<int>) baseTable.Columns[TestTableColumns.IdColumn],
                                                sumColumn,
//                                                   (i, c, b) => b ? c += i : c -= i);
                                                () => new Sum<int>());

            var row1 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row1, "Value1");
            baseTable.SetValue(TestTableColumns.IdColumn, row1, 10);
            Assert.AreEqual(1, groupedTable.RowCount);
            Assert.AreEqual(10, groupedTable.GetValue<int>(sumColumn, 0));

            var row2 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row2, "Value1");
            baseTable.SetValue(TestTableColumns.IdColumn, row2, 20);
            Assert.AreEqual(1, groupedTable.RowCount);
            Assert.AreEqual(30, groupedTable.GetValue<int>(sumColumn, 0));

            var row3 = baseTable.AddRow();
            baseTable.SetValue(TestTableColumns.StringColumn, row3, "Value2");
            baseTable.SetValue(TestTableColumns.IdColumn, row3, 15);
            Assert.AreEqual(2, groupedTable.RowCount);
            Assert.AreEqual(30, groupedTable.GetValue<int>(sumColumn, 0));
            Assert.AreEqual(15, groupedTable.GetValue<int>(sumColumn, 1));

            baseTable.SetValue(TestTableColumns.IdColumn, row2, 25);
            Assert.AreEqual(2, groupedTable.RowCount);
            Assert.AreEqual(35, groupedTable.GetValue<int>(sumColumn, 0));

            // Now change the membership in the groups
            baseTable.SetValue(TestTableColumns.StringColumn, row2, "Value2");
            Assert.AreEqual(10, groupedTable.GetValue<int>(sumColumn, 0));
            Assert.AreEqual(40, groupedTable.GetValue<int>(sumColumn, 1));
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