using System;

namespace ReactiveTables.Framework
{
    public interface IObservableColumn
    {
        IDisposable Subscribe(IColumnObserver observer);             
    }
}