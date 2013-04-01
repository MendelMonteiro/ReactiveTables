using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Framework
{
    public struct ColumnUpdate
    {
        private readonly IReactiveColumn _column;
        private readonly int _rowIndex;

        public ColumnUpdate(IReactiveColumn column, int rowIndex) 
        {
            _column = column;
            _rowIndex = rowIndex;
        }

        public IReactiveColumn Column
        {
            get { return _column; }
        }

        public int RowIndex
        {
            get { return _rowIndex; }
        }
    }
}