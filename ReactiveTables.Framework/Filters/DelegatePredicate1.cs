using System;
using System.Collections.Generic;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Filters
{
    public class DelegatePredicate1<T> : IReactivePredicate
    {
        private readonly string _columnId;
        private readonly Predicate<T> _predicate;

        public DelegatePredicate1(string columnId, Predicate<T> predicate)
        {
            _columnId = columnId;
            _predicate = predicate;
            Columns = new List<string>{columnId};
        }

        public IList<string> Columns { get; private set; }
        public bool RowIsVisible(IReactiveTable sourceTable, int rowIndex)
        {
            return _predicate(sourceTable.GetValue<T>(_columnId, rowIndex));
        }
    }
}