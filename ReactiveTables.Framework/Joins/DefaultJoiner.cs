using System;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework.Joins
{
    class DefaultJoiner : IReactiveTableJoiner
    {
        public static readonly DefaultJoiner DefaultJoinerInstance = new DefaultJoiner();

        private DefaultJoiner(){}

        public int RowCount { get; private set; }
        public int GetRowIndex(IReactiveColumn column, int joinRowIndex)
        {
            return joinRowIndex;
        }

        public void AddRowObserver(IObserver<RowUpdate> observer)
        {
            throw new NotImplementedException();
        }
    }
}