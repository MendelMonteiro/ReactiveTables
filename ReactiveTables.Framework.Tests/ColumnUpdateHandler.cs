using System.Collections.Generic;
using System.Linq;

namespace ReactiveTables.Framework.Tests
{
    internal class ColumnUpdateHandler
    {
        public List<string> LastColumnsUpdated { get; }
        public List<int> LastRowsUpdated { get; }
        public int LastRowUpdated => LastRowsUpdated.LastOrDefault();
        public string LastColumnUpdated => LastColumnsUpdated.LastOrDefault();

        public ColumnUpdateHandler()
        {
            LastColumnsUpdated = new List<string>();
            LastRowsUpdated = new List<int>();
        }

        public void OnColumnUpdate(TableUpdate update)
        {
            if (update.IsColumnUpdate())
            {
                LastColumnsUpdated.Add(update.Column.ColumnId);
                LastRowsUpdated.Add(update.RowIndex);
            }
        }
    }
}