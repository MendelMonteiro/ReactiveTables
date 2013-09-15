using System;
using System.Collections.Generic;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Filters
{
    public class DelegatePredicate2<T1, T2> : IReactivePredicate
    {
        private readonly ReactiveColumn<T1> _column1;
        private readonly ReactiveColumn<T2> _column2;
        private readonly Func<T1, T2, bool> _predicate;

        public DelegatePredicate2(ReactiveColumn<T1> column1, ReactiveColumn<T2> column2, Func<T1, T2, bool> predicate)
        {
            _column1 = column1;
            _column2 = column2;
            _predicate = predicate;
            Columns = new List<IReactiveColumn> { column1, column2 };
        }

        public IList<IReactiveColumn> Columns { get; private set; }
        public bool RowIsVisible(int rowIndex)
        {
            return _predicate(_column1.GetValue(rowIndex), _column2.GetValue(rowIndex));
        }
    }
}