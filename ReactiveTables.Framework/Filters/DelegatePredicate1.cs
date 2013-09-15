using System;
using System.Collections.Generic;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Filters
{
    public class DelegatePredicate1<T> : IReactivePredicate
    {
        private readonly ReactiveColumn<T> _column;
        private readonly Predicate<T> _predicate;

        public DelegatePredicate1(ReactiveColumn<T> column, Predicate<T> predicate)
        {
            _column = column;
            _predicate = predicate;
            Columns = new List<IReactiveColumn>{column};
        }

        public IList<IReactiveColumn> Columns { get; private set; }
        public bool RowIsVisible(int rowIndex)
        {
            return _predicate(_column.GetValue(rowIndex));
        }
    }
}