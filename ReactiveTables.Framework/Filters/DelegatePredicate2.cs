using System;
using System.Collections.Generic;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Filters
{
    public class DelegatePredicate2<T1, T2> : IReactivePredicate
    {
        private readonly string _column1;
        private readonly string _column2;
        private readonly Func<T1, T2, bool> _predicate;

        public DelegatePredicate2(string column1, string column2, Func<T1, T2, bool> predicate)
        {
            _column1 = column1;
            _column2 = column2;
            _predicate = predicate;
            Columns = new List<string> { column1, column2 };
        }

        public IList<string> Columns { get; }

        public bool RowIsVisible(IReactiveTable sourceTable, int rowIndex)
        {
            return _predicate(sourceTable.GetValue<T1>(_column1, rowIndex), sourceTable.GetValue<T2>(_column2, rowIndex));
        }
    }
}