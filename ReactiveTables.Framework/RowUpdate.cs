namespace ReactiveTables.Framework
{
    public struct RowUpdate
    {
        public enum RowUpdateAction
        {
            Add,
            Delete
        }

        private int _rowIndex;

        private RowUpdateAction _action;

        public RowUpdate(int rowIndex, RowUpdateAction action)
        {
            _rowIndex = rowIndex;
            _action = action;
        }

        public int RowIndex
        {
            get { return _rowIndex; }
            set { _rowIndex = value; }
        }

        public RowUpdateAction Action
        {
            get { return _action; }
            set { _action = value; }
        }
    }
}