using System.Collections.Generic;
using System.Linq;

namespace ReactiveTables.Framework.Tests
{
    internal class ColumnUpdateHandler
    {
        public List<string> LastColumnsUpdated { get; private set; }
        public List<int> LastRowsUpdated { get; private set; }
        public int LastRowUpdated { get { return LastRowsUpdated.LastOrDefault(); } }
        public string LastColumnUpdated { get { return LastColumnsUpdated.LastOrDefault(); } }

        public ColumnUpdateHandler()
        {
            LastColumnsUpdated = new List<string>();
            LastRowsUpdated = new List<int>();
        }

        public void OnColumnUpdate(TableUpdate update)
        {
            LastColumnsUpdated.Add(update.Column.ColumnId);
            LastRowsUpdated.Add(update.RowIndex);
        }
    }
}