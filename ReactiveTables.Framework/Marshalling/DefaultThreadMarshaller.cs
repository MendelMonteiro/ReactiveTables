using System;

namespace ReactiveTables.Framework.Marshalling
{
    public class DefaultThreadMarshaller : IThreadMarshaller
    {
        public void Dispatch(Action action)
        {
            action();
        }
    }
}