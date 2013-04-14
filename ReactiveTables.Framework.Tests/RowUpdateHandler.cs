using System.Collections.Generic;

namespace ReactiveTables.Framework.Tests
{
    class RowUpdateHandler
    {
        public int CurrentRowCount { get; private set; }
        public int LastRowUpdated { get; private set; }
        public List<int> RowsUpdated { get; private set; }

        public RowUpdateHandler()
        {
            RowsUpdated = new List<int>();
            LastRowUpdated = -1;
        }

        public void OnRowUpdate(RowUpdate update)
        {
            RowsUpdated.Add(update.RowIndex);
            if (update.Action == RowUpdate.RowUpdateAction.Add)
            {
                CurrentRowCount++;
            }
            else
            {
                CurrentRowCount--;
            }
            LastRowUpdated = update.RowIndex;
        }
    }
}