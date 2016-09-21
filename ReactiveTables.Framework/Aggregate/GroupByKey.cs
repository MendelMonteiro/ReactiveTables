using System;
using System.Collections.Generic;
using System.Linq;

namespace ReactiveTables.Framework.Aggregate
{
    class GroupByKey : IEquatable<GroupByKey>
    {
        private readonly int _hashCode;
        private readonly List<object> _values;

        public GroupByKey(List<IHashcodeAccessor> columns, int rowId)
        {
            _values = new List<object>(columns.Count);
            unchecked
            {
                _hashCode = 17;
                foreach (var column in columns)
                {
                    _hashCode = _hashCode*31 + column.GetColumnHashCode(rowId);
                    _values.Add(column.GetValue(rowId));
                }
            }
        }

        public bool Equals(GroupByKey other)
        {
            for (var i = 0; i < _values.Count; i++)
            {
                if (!Equals(_values[i], other._values[i]))
                    return false;
            }
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is GroupByKey && Equals((GroupByKey) obj);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }
    }
}