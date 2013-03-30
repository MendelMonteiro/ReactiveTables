using System;

namespace ReactiveTables
{
    internal interface IThreadMarshaller
    {
        void Dispatch(Action action);
    }
}