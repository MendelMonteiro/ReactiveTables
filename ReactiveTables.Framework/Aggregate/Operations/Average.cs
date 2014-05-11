using System;
using System.Linq.Expressions;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Aggregate.Operations
{
    public class Average<TIn> : IAccumulator<TIn, double>
    {
        private readonly Count<TIn> _count;
        private readonly Sum<TIn> _sum;
        private static readonly Func<TIn, double> _sumToDouble;

        static Average()
        {
            var sum = Expression.Parameter(typeof(TIn));
            var sumToDoubleExp = Expression.Convert(sum, typeof(double));
            _sumToDouble = Expression.Lambda<Func<TIn, double>>(sumToDoubleExp, sum).Compile();            
        }

        public Average()
        {
            _count = new Count<TIn>();
            _sum = new Sum<TIn>();
        }

        public void SetSourceColumn(IReactiveColumn<TIn> sourceColumn)
        {
            _count.SetSourceColumn(sourceColumn);
            _sum.SetSourceColumn(sourceColumn);
        }

        public void AddValue(int sourceRowIndex)
        {
            _count.AddValue(sourceRowIndex);
            _sum.AddValue(sourceRowIndex);
        }

        public void RemoveValue(int sourceRowIndex)
        {
            _count.RemoveValue(sourceRowIndex);
            _sum.RemoveValue(sourceRowIndex);            
        }

        public double CurrentValue
        {
            get
            {
                if (_count.CurrentValue == 0)
                {
                    return 0;
                }

                return Divide();
            }
        }

        private double Divide()
        {
            var doubleSum = _sumToDouble(_sum.CurrentValue);
            return doubleSum/_count.CurrentValue;
        }
    }
}