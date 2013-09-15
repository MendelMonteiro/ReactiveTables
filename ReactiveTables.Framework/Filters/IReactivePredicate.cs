using System.Collections.Generic;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Filters
{
    /// <summary>
    /// A predicate used for determining if a current row is visible or not.
    /// </summary>
    public interface IReactivePredicate
    {
        /// <summary>
        /// The columns used to apply this predicate
        /// </summary>
        IList<IReactiveColumn> Columns { get; }

        /// <summary>
        /// Whether the row at <see cref="rowIndex"/> is visible or not.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        bool RowIsVisible(int rowIndex);
    }
}