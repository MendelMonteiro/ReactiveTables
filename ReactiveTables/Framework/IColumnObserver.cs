using System;

namespace ReactiveTables.Framework
{
    public interface IColumnObserver
    {
        void OnNext(int rowIndex);
        void OnError(Exception error, int rowIndex);
        void OnCompleted(int rowIndex);
    }
}