using System.Collections.Generic;
using System.IO;
using System.Threading;
using NUnit.Framework;
using ReactiveTables.Framework.Protobuf;
using ReactiveTables.Framework.Utils;

namespace ReactiveTables.Framework.Tests.Comms.Protobuf
{
    [TestFixture]
    public class ProtobufTableEncoderTest
    {
        [Test]
        public void Test()
        {
            var tableEncoderTester = new TableEncoderTester();

            // Setup encoder
            var encoder = new ProtobufTableEncoder();
            var stream = new MemoryStream();
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
            encoder.Setup(stream, table, new ProtobufEncoderState(columnsToFieldIds));

            // Add data
            var row1 = TableEncoderTester.AddTestRow(table);
            var row2 = TableEncoderTester.AddTestRow(table);

            tableEncoderTester.UpdateTestRow(table, row1);

            encoder.Dispose();
            //stream.Flush();
            stream.Seek(0, SeekOrigin.Begin);

            // Decode
            var destTable = TestTableHelper.CreateReactiveTableFull();
            var tableDecoder = new ProtobufTableDecoder();
            tableDecoder.Setup(stream, destTable, columnsToFieldIds.InverseUniqueDictionary());
//            Task.Run(() => tableDecoder.Start());
            Thread.Sleep(100);
            tableDecoder.Stop();

            tableEncoderTester.CompareTables(table, destTable);
        }
    }
}
