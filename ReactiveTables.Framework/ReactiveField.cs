/*This file is part of ReactiveTables.

ReactiveTables is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ReactiveTables is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace ReactiveTables.Framework
{
    public interface IReactiveField<out T>
    {
        T Value { get; }
    }

    public class ReactiveField<T>: IReactiveField<T>
    {
        public static readonly ReactiveField<T> Empty = new ReactiveField<T>(default(T));

        public ReactiveField()
        {
        }

        private ReactiveField(T value)
        {
            Value = value;
        }

        public T Value { get; private set; }

        internal void SetInternalFieldValue(T value)
        {
            Value = value;
        }
    }
}
