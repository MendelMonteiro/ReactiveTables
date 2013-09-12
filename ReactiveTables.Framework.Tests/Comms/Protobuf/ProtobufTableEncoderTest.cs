using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using ReactiveTables.Framework.Comms.Protobuf;
using ReactiveTables.Utils;

namespace ReactiveTables.Framework.Tests.Comms.Protobuf
{
    [TestFixture]
    public class ProtobufTableEncoderTest
    {
        [Test]
        public void Test()
        {
            // Setup encoder
            ProtobufTableEncoder encoder = new ProtobufTableEncoder();
            MemoryStream stream = new MemoryStream();
            var table = TestTableHelper.CreateReactiveTableFull();
            var columnsToFieldIds = new Dictionary<string, int>
                                        {
                                            {TestTableColumns.IdColumn, 101},
                                            {TestTableColumns.StringColumn, 102}, 
                                            {TestTableColumns.DecimalColumn, 103},
                                            {TestTableColumns.BoolColumn, 104},
                                            {TestTableColumns.DoubleColumn, 105},
                                            {TestTableColumns.ShortColumn, 106},
                                            {TestTableColumns.LongColumn, 107},
                                            {TestTableColumns.DateTimeColumn, 108},
                                            {TestTableColumns.TimespanColumn, 109},
                                            {TestTableColumns.GuidColumn, 110},
                                            {TestTableColumns.FloatColumn, 111},
                                            {TestTableColumns.ByteColumn, 112},
                                            {TestTableColumns.CharColumn, 113},
                                        };
            encoder.Setup(stream, table, new ProtobufEncoderState
                                             {
                                                 ColumnsToFieldIds = columnsToFieldIds
                                             });
            // Add data
            var row1 = AddTestRow(table);
            var row2 = AddTestRow(table);

            UpdateTestRow(table, row1);

            encoder.Close();
            //stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            // Decode
            var destTable = TestTableHelper.CreateReactiveTableFull();
            ProtobufTableDecoder tableDecoder = new ProtobufTableDecoder(destTable, columnsToFieldIds.InverseUniqueDictionary(), stream);
            Task.Run(() => tableDecoder.Start());
            Thread.Sleep(50);
            tableDecoder.Stop();

            CompareTables(table, destTable);
        }

        private void UpdateTestRow(ReactiveTable table, int rowId)
        {
            table.SetValue(TestTableColumns.IdColumn, rowId, 2);
            table.SetValue(TestTableColumns.StringColumn, rowId, "Bar");
            table.SetValue(TestTableColumns.DecimalColumn, rowId, 7678.232m);
            table.SetValue(TestTableColumns.BoolColumn, rowId, true);
            table.SetValue(TestTableColumns.DoubleColumn, rowId, 2222.23);
            table.SetValue(TestTableColumns.ShortColumn, rowId, (short)3131);
            table.SetValue(TestTableColumns.LongColumn, rowId, 11111111111111);
            table.SetValue(TestTableColumns.DateTimeColumn, rowId, new DateTime(1983, 07, 03));
            table.SetValue(TestTableColumns.TimespanColumn, rowId, TimeSpan.FromHours(1));
            table.SetValue(TestTableColumns.GuidColumn, rowId, Guid.NewGuid());
            table.SetValue(TestTableColumns.FloatColumn, rowId, 1.2343f);
            table.SetValue(TestTableColumns.ByteColumn, rowId, (byte)234);
            table.SetValue(TestTableColumns.CharColumn, rowId, 'a');
        }

        private static int AddTestRow(ReactiveTable table)
        {
            var rowId = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, rowId, 1);
            table.SetValue(TestTableColumns.StringColumn, rowId, "Foo");
            table.SetValue(TestTableColumns.DecimalColumn, rowId, 2132.233m);
            table.SetValue(TestTableColumns.BoolColumn, rowId, false);
            table.SetValue(TestTableColumns.DoubleColumn, rowId, 1111.232);
            table.SetValue(TestTableColumns.ShortColumn, rowId, (short)4242);
            table.SetValue(TestTableColumns.LongColumn, rowId, 22222222222222);
            table.SetValue(TestTableColumns.DateTimeColumn, rowId, new DateTime(1982, 04, 10));
            table.SetValue(TestTableColumns.TimespanColumn, rowId, TimeSpan.FromDays(1));
            table.SetValue(TestTableColumns.GuidColumn, rowId, Guid.NewGuid());
            table.SetValue(TestTableColumns.FloatColumn, rowId, 6.6542f);
            table.SetValue(TestTableColumns.ByteColumn, rowId, (byte)188);
            table.SetValue(TestTableColumns.CharColumn, rowId, 'D');
            return rowId;
        }

        private void CompareTables(ReactiveTable expectedTable, ReactiveTable table)
        {
            Assert.AreEqual(expectedTable.RowCount, table.RowCount);
            Assert.AreEqual(expectedTable.Columns.Count, table.Columns.Count);
            foreach (var column in expectedTable.Columns)
            {
                Assert.IsTrue(table.Columns.ContainsKey(column.Key));
                for (int i = 0; i < expectedTable.RowCount; i++)
                {
                    // Cheating a little by relying on the fact that i know the row id starts at 0.
                    Assert.AreEqual(expectedTable.GetValue(column.Key, i), table.GetValue(column.Key, i));
                }
            }
        }
    }
}
