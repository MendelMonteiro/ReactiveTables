/* Generated SBE (Simple Binary Encoding) message codec */

#pragma warning disable 1591 // disable warning on missing comments
using System;
using Adaptive.SimpleBinaryEncoding;

namespace ReactiveTables.Framework.SimpleBinaryEncoding
{
    public enum OperationType : byte
    {
        Add = 0,
        Update = 1,
        Delete = 2,
        NULL_VALUE = 255
    }
}
