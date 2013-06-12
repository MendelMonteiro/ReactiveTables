using System.Collections.Generic;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Filters
{
    public interface IReactivePredicate
    {
        List<IReactiveColumn> Columns { get; }
        bool RowIsVisible(int rowIndex);
    }
}