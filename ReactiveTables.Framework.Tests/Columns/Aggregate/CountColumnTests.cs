using System;
using System.Reactive.Linq;
using NUnit.Framework;

namespace ReactiveTables.Framework.Tests.Columns.Aggregate
{
    [TestFixture]
    public class CountColumnTests
    {
        [Test]
        public void TestSimpleCount()
        {
            var table = TableTestHelper.CreateReactiveTable();
            var stream = table;
            var countStream = stream.ColumnUpdates()
                .Where(update => update.Column.ColumnId == TestTableColumns.IdColumn)
                .Scan(0, (i, update) => i + 1);

//            var token = table.Subscribe<ColumnUpdate>(i => Console.WriteLine("Count is {0}", i));
            var token = countStream.Subscribe(i => Console.WriteLine("Count is {0}", i));
            
            var rowIndex = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, rowIndex, 1);

            var rowIndex2 = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, rowIndex2, 2);

            token.Dispose();
        }

        [Test]
        public void TestSimpleGroupBy()
        {
            var table = TableTestHelper.CreateReactiveTable();
            var stream = table;
            var groupBy = stream
//                .Where(update => update.Column.ColumnId == TestTableColumns.StringColumn)
                .GroupBy(update => table.GetValue<string>(TestTableColumns.StringColumn, update.RowIndex));

            groupBy.Subscribe(
                group =>
                    {
                        if (group.Key != default(string))
                        {
                            // Now we have a valid group and we can raise a new row event
                            Console.WriteLine("New key {0}", group.Key);

                            group.Subscribe(
                                // Here we have a col change and we can raise a col change event
                                update => Console.WriteLine("{0}\tId : {1}", group.Key, table.GetValue<int>(TestTableColumns.IdColumn, update.RowIndex)));

                            // We can do aggregates on the columns here
                            // Need to store the previous value in order to be able to update the sum correctly
                            // ohterwise we would need to start again from the beginning
                            group.Scan(0, (i, update) => i + table.GetValue<int>(TestTableColumns.IdColumn, update.RowIndex))
                                .Subscribe(i => Console.WriteLine("{2}\tSum of {0} : {1}", group.Key, i, group.Key));
                        }
                    });

            var rowIndex = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, rowIndex, 1);
            table.SetValue(TestTableColumns.StringColumn, rowIndex, "Name 1");

            var rowIndex2 = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, rowIndex2, 2);
            table.SetValue(TestTableColumns.StringColumn, rowIndex2, "Name 1");
            
            var rowIndex3 = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, rowIndex3, 3);
            table.SetValue(TestTableColumns.StringColumn, rowIndex3, "Name 2");

            table.SetValue(TestTableColumns.StringColumn, rowIndex, "Name 2");

            table.SetValue(TestTableColumns.IdColumn, rowIndex2, 4);
        }
    }
}
