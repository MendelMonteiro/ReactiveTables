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
            var table = TestTableHelper.CreateReactiveTable();
            var columnsToFieldIds = new Dictionary<string, int>
                                        {
                                            {TestTableColumns.IdColumn, 101},
                                            {TestTableColumns.StringColumn, 102}, 
                                            {TestTableColumns.DecimalColumn, 103},
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
            var destTable = TestTableHelper.CreateReactiveTable();
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
        }

        private static int AddTestRow(ReactiveTable table)
        {
            var rowId = table.AddRow();
            table.SetValue(TestTableColumns.IdColumn, rowId, 1);
            table.SetValue(TestTableColumns.StringColumn, rowId, "Foo");
            table.SetValue(TestTableColumns.DecimalColumn, rowId, 2132.232m);
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
