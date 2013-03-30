using System;

namespace ReactiveTables
{
    public interface IColumnObserver<in T>
    {
        void OnNext(T value, int index);
        void OnError(Exception error, int index);
        void OnCompleted(int index);
    }
}