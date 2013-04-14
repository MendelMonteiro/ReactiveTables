using System;

namespace ReactiveTables.Framework.Marshalling
{
    public interface IThreadMarshaller
    {
        void Dispatch(Action action);
    }
}