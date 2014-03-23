/* Generated SBE (Simple Binary Encoding) message codec */

#pragma warning disable 1591 // disable warning on missing comments
using System;
using Adaptive.SimpleBinaryEncoding;
using Adaptive.SimpleBinaryEncoding.Ir.Generated;

namespace ReactiveTables.Framework.SimpleBinaryEncoding
{
    public class SbeTableUpdate
    {
        public const ushort BlockLength = (ushort)9;
        public const ushort TemplateId = (ushort)2;
        public const ushort SchemaId = (ushort)1;
        public const ushort Schema_Version = (ushort)0;
        public const string SematicType = "";

        private readonly SbeTableUpdate _parentMessage;
        private DirectBuffer _buffer;
        private int _offset;
        private int _limit;
        private int _actingBlockLength;
        private int _actingVersion;

        public int Offset { get { return _offset; } }

        public SbeTableUpdate()
        {
            _parentMessage = this;
        }

        public void WrapForEncode(DirectBuffer buffer, int offset)
        {
            _buffer = buffer;
            _offset = offset;
            _actingBlockLength = BlockLength;
            _actingVersion = Schema_Version;
            Limit = offset + _actingBlockLength;
        }

        public void WrapForDecode(DirectBuffer buffer, int offset, int actingBlockLength, int actingVersion)
        {
            _buffer = buffer;
            _offset = offset;
            _actingBlockLength = actingBlockLength;
            _actingVersion = actingVersion;
            Limit = offset + _actingBlockLength;
        }

        public int Size
        {
            get
            {
                return _limit - _offset;
            }
        }

        public int Limit
        {
            get
            {
                return _limit;
            }
            set
            {
                _buffer.CheckLimit(_limit);
                _limit = value;
            }
        }


        public const int TypeId = 1;

        public static string TypeMetaAttribute(MetaAttribute metaAttribute)
        {
            switch (metaAttribute)
            {
                case MetaAttribute.Epoch: return "unix";
                case MetaAttribute.TimeUnit: return "nanosecond";
                case MetaAttribute.SemanticType: return "";
            }

            return "";
        }

        public OperationType Type
        {
            get
            {
                return (OperationType)_buffer.Uint8Get(_offset + 0);
            }
            set
            {
                _buffer.Uint8Put(_offset + 0, (byte)value);
            }
        }


        public const int RowIdId = 2;

        public static string RowIdMetaAttribute(MetaAttribute metaAttribute)
        {
            switch (metaAttribute)
            {
                case MetaAttribute.Epoch: return "unix";
                case MetaAttribute.TimeUnit: return "nanosecond";
                case MetaAttribute.SemanticType: return "";
            }

            return "";
        }

        public const int RowIdNullValue = -2147483648;

        public const int RowIdMinValue = -2147483647;

        public const int RowIdMaxValue = 2147483647;

        public int RowId
        {
            get
            {
                return _buffer.Int32GetLittleEndian(_offset + 1);
            }
            set
            {
                _buffer.Int32PutLittleEndian(_offset + 1, value);
            }
        }


        public const int FieldIdId = 3;

        public static string FieldIdMetaAttribute(MetaAttribute metaAttribute)
        {
            switch (metaAttribute)
            {
                case MetaAttribute.Epoch: return "unix";
                case MetaAttribute.TimeUnit: return "nanosecond";
                case MetaAttribute.SemanticType: return "";
            }

            return "";
        }

        public const int FieldIdNullValue = -2147483648;

        public const int FieldIdMinValue = -2147483647;

        public const int FieldIdMaxValue = 2147483647;

        public int FieldId
        {
            get
            {
                return _buffer.Int32GetLittleEndian(_offset + 5);
            }
            set
            {
                _buffer.Int32PutLittleEndian(_offset + 5, value);
            }
        }

    }
}
