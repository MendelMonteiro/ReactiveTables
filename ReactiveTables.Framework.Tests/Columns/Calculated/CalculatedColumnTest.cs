using System;
using System.Collections.Generic;
using NUnit.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Columns.Calculated;
using System.Linq;

namespace ReactiveTables.Framework.Tests.Columns.Calculated
{
    [TestFixture]
    public class CalculatedColumnTest
    {
        [Test]
        public void Test1Column()
        {
            var table = TestTableHelper.CreateReactiveTable();
            const string calculatedColumnId = "CalculatedColumn";
            table.AddColumn(new ReactiveCalculatedColumn1<string, int>(
                                calculatedColumnId,
                                (IReactiveColumn<int>) table.Columns[TestTableColumns.IdColumn],
                                i => "The int value is " + i));

            var updates = new List<TableUpdate>();
            table.Subscribe(updates.Add);

            var row1 = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, row1, 42);
            
            Assert.AreEqual("The int value is 42", table.GetValue<string>(calculatedColumnId, row1));
            Assert.AreEqual(3, updates.Count);
            Assert.AreEqual(1, updates.Count(u => u.RowIndex == row1 && u.Action == TableUpdate.TableUpdateAction.Add));
            Assert.AreEqual(1, updates.Count(u => u.RowIndex == row1 && u.Action == TableUpdate.TableUpdateAction.Update &&
                                                        u.Column.ColumnId == TestTableColumns.IdColumn));
            Assert.AreEqual(1, updates.Count(u => u.RowIndex == row1 && u.Action == TableUpdate.TableUpdateAction.Update &&
                                                        u.Column.ColumnId == calculatedColumnId));
        }

        [Test]
        public void Test2Column()
        {
            var table = TestTableHelper.CreateReactiveTable();
            const string calculatedColumnId = "CalculatedColumn";
            table.AddColumn(new ReactiveCalculatedColumn2<string, int, string>(
                                calculatedColumnId,
                                (IReactiveColumn<int>) table.Columns[TestTableColumns.IdColumn],
                                (IReactiveColumn<string>) table.Columns[TestTableColumns.StringColumn],
                                (i, s) => "The int " + s + " is " + i));

            var updates = new List<TableUpdate>();
            table.Subscribe(updates.Add);

            var row1 = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, row1, 42);
            table.SetValue(TestTableColumns.StringColumn, row1, "value");
            
            Assert.AreEqual("The int value is 42", table.GetValue<string>(calculatedColumnId, row1));
            Assert.AreEqual(5, updates.Count);
            Assert.AreEqual(1, updates.Count(u => u.RowIndex == row1 && u.Action == TableUpdate.TableUpdateAction.Add));
            Assert.AreEqual(1, updates.Count(u => u.RowIndex == row1 && u.Action == TableUpdate.TableUpdateAction.Update &&
                                                        u.Column.ColumnId == TestTableColumns.IdColumn));
            Assert.AreEqual(1, updates.Count(u => u.RowIndex == row1 && u.Action == TableUpdate.TableUpdateAction.Update &&
                                                        u.Column.ColumnId == TestTableColumns.StringColumn));
            // One after each update
            Assert.AreEqual(2, updates.Count(u => u.RowIndex == row1 && u.Action == TableUpdate.TableUpdateAction.Update &&
                                                        u.Column.ColumnId == calculatedColumnId));
        }
    }
}
