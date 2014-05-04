// This file is part of ReactiveTables.
// 
// ReactiveTables is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReactiveTables is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Aggregate.Operations
{
    /// <summary>
    /// An aggregate operation accumulator
    /// </summary>
    /// <typeparam name="TIn">The type of the source column</typeparam>
    /// <typeparam name="TOut">The type produced by the operation</typeparam>
    public interface IAccumulator<TIn, out TOut>
    {
        /// <summary>
        /// Set the source column
        /// </summary>
        /// <param name="sourceColumn"></param>
        void SetSourceColumn(IReactiveColumn<TIn> sourceColumn);

        /// <summary>
        /// Add a new/modified source value to the accumulator
        /// </summary>
        /// <param name="sourceRowIndex"></param>
        void AddValue(int sourceRowIndex);

        /// <summary>
        /// A source value is no longer present
        /// </summary>
        /// <param name="sourceRowIndex"></param>
        void RemoveValue(int sourceRowIndex);

        /// <summary>
        /// The current value of the accumulator
        /// </summary>
        TOut CurrentValue { get; }
    }
}