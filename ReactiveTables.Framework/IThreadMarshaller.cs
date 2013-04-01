using System;

namespace ReactiveTables.Framework
{
    public interface IThreadMarshaller
    {
        void Dispatch(Action action);
    }
}