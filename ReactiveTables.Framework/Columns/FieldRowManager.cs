using System.Collections.Generic;

namespace ReactiveTables.Framework.Columns
{
    public class FieldRowManager
    {
        private readonly Queue<int> _deletedRows = new Queue<int>();

        public FieldRowManager()
        {
            RowCount = 0;
        }

        public int RowCount { get; private set; }

        public int AddRow()
        {
            RowCount++;
            var newRow = _deletedRows.Count <= 0;
            if (!newRow)
            {
                return _deletedRows.Dequeue();
            }
            return RowCount - 1;
        }

        public void DeleteRow(int rowIndex)
        {
            RowCount--;
            _deletedRows.Enqueue(rowIndex);
        }
    }
}