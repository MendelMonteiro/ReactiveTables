using System;

namespace ReactiveTables.Framework
{
    internal interface IThreadMarshaller
    {
        void Dispatch(Action action);
    }
}