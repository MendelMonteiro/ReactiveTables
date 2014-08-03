using System;
using NUnit.Framework;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Tests.Comms.Protobuf
{
    public class TableEncoderTester
    {
        public void UpdateTestRow(ReactiveTable table, int rowId, bool includeComplexTypes = true)
        {
            table.SetValue(TestTableColumns.IdColumn, rowId, 2);
            table.SetValue(TestTableColumns.StringColumn, rowId, "Bar");
            table.SetValue(TestTableColumns.BoolColumn, rowId, true);
            table.SetValue(TestTableColumns.DoubleColumn, rowId, 2222.23);
            table.SetValue(TestTableColumns.ShortColumn, rowId, (short)3131);
            table.SetValue(TestTableColumns.LongColumn, rowId, 11111111111111);
            table.SetValue(TestTableColumns.DecimalColumn, rowId, 7678.232m);

            if (includeComplexTypes)
            {
                table.SetValue(TestTableColumns.DateTimeColumn, rowId, new DateTime(1983, 07, 03));
                table.SetValue(TestTableColumns.TimespanColumn, rowId, TimeSpan.FromHours(1));
                table.SetValue(TestTableColumns.GuidColumn, rowId, Guid.NewGuid());
            }
            
            table.SetValue(TestTableColumns.FloatColumn, rowId, 1.2343f);
            table.SetValue(TestTableColumns.ByteColumn, rowId, (byte)234);
            table.SetValue(TestTableColumns.CharColumn, rowId, 'a');
        }

        public static int AddTestRow(ReactiveTable table, bool includeComplexTypes = true)
        {
            var rowId = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, rowId, 1);
            table.SetValue(TestTableColumns.StringColumn, rowId, "Foo");
            table.SetValue(TestTableColumns.BoolColumn, rowId, false);
            table.SetValue(TestTableColumns.DoubleColumn, rowId, 1111.232);
            table.SetValue(TestTableColumns.ShortColumn, rowId, (short)4242);
            table.SetValue(TestTableColumns.LongColumn, rowId, 22222222222222);
            table.SetValue(TestTableColumns.DecimalColumn, rowId, 2132.233m);

            if (includeComplexTypes)
            {
                table.SetValue(TestTableColumns.DateTimeColumn, rowId, new DateTime(1982, 04, 10));
                table.SetValue(TestTableColumns.TimespanColumn, rowId, TimeSpan.FromDays(1));
                table.SetValue(TestTableColumns.GuidColumn, rowId, Guid.NewGuid());
            }
            
            table.SetValue(TestTableColumns.FloatColumn, rowId, 6.6542f);
            table.SetValue(TestTableColumns.ByteColumn, rowId, (byte)188);
            table.SetValue(TestTableColumns.CharColumn, rowId, 'D');
            return rowId;
        }

        public void CompareTables(ReactiveTable expectedTable, ReactiveTable table)
        {
            Assert.AreEqual(expectedTable.RowCount, table.RowCount);
            Assert.AreEqual(expectedTable.Columns.Count, table.Columns.Count);
            foreach (var column in expectedTable.Columns)
            {
                IReactiveColumn col;
                Assert.IsTrue(table.GetColumnByName(column.ColumnId, out col));
                for (int i = 0; i < expectedTable.RowCount; i++)
                {
                    // Cheating a little by relying on the fact that i know the row id starts at 0.
                    Assert.AreEqual(expectedTable.GetValue(column.ColumnId, i),
                                    table.GetValue(column.ColumnId, i),
                                    string.Format("For column {0}", column.ColumnId));
                }
            }
        }
    }
}