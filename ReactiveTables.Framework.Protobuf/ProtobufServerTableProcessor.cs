using System.Collections.Generic;
using System.IO;
using ReactiveTables.Framework.Comms;
using ReactiveTables.Utils;

namespace ReactiveTables.Framework.Protobuf
{
    public class ProtobufServerTableProcessor : IReactiveTableProcessor<IWritableReactiveTable>
    {
        private readonly Dictionary<int, string> _fieldsToColumnIds;
        private ProtobufTableDecoder _tableDecoder;

        public ProtobufServerTableProcessor(Dictionary<string, int> columnsToFieldIds)
        {
            _fieldsToColumnIds = columnsToFieldIds.InverseUniqueDictionary();
        }

        public void Setup(Stream outputStream, IWritableReactiveTable table, object state)
        {
            _tableDecoder = new ProtobufTableDecoder();
            _tableDecoder.Setup(outputStream, table, _fieldsToColumnIds);
            _tableDecoder.Start();
        }

        public void Dispose()
        {
            if (_tableDecoder != null) _tableDecoder.Stop();
        }
    }
}