using System.Collections.Generic;

namespace ReactiveTables.Framework.Joins
{
    struct JoinEntry<TKey>
    {
        public TKey Key;
        public List<int> LeftRowIndexes;
        public List<int> RightRowIndexes;
        public int RowIndex;

        public JoinEntry(TKey key, int? leftRowIndexes, int? rightRowIndexes)
        {
            Key = key;
            RowIndex = -1;
            LeftRowIndexes = new List<int>();
            RightRowIndexes = new List<int>();
        }
    }
}