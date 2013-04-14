using System;
using System.Collections.Generic;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework
{
    internal class ColumnChangePublisher : IColumnObserver
    {
        private readonly IReactiveColumn _column;
        private readonly HashSet<IObserver<RowUpdate>> _rowObservers;
        private readonly HashSet<IObserver<ColumnUpdate>> _columnObservers;

        public ColumnChangePublisher(IReactiveColumn column, HashSet<IObserver<RowUpdate>> rowObservers, HashSet<IObserver<ColumnUpdate>> columnObservers)
        {
            _column = column;
            _rowObservers = rowObservers;
            _columnObservers = columnObservers;
        }

        public void OnNext(int rowIndex)
        {
            var columnUpdate = new ColumnUpdate(_column, rowIndex);
            foreach (var observer in _columnObservers)
            {
                observer.OnNext(columnUpdate);
            }
        }

        public void OnError(Exception error, int rowIndex)
        {
            foreach (var observer in _rowObservers)
            {
                observer.OnError(error);
            }
        }

        public void OnCompleted(int rowIndex)
        {
            foreach (var observer in _rowObservers)
            {
                observer.OnCompleted();
            }
        }
    }
}