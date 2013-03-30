using System;

namespace ReactiveTables
{
    public interface IObservableColumn<out T>
    {
        IDisposable Subscribe(IColumnObserver<T> observer);             
    }
}