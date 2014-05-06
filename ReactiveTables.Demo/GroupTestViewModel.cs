// This file is part of ReactiveTables.
// 
// ReactiveTables is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// ReactiveTables is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using ReactiveTables.Demo.Services;
using ReactiveTables.Demo.Utils;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Aggregate;
using ReactiveTables.Framework.Aggregate.Operations;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.UI;
using ReactiveTables.Framework.Utils;

namespace ReactiveTables.Demo
{
    internal class GroupTestViewModel : ReactiveViewModelBase, IDisposable
    {
        private readonly AggregatedTable _groupedAccounts;
        private const string SumColumnId = "Sum";

        public GroupTestViewModel(IAccountBalanceDataService service)
        {
            _groupedAccounts = new AggregatedTable(service.Accounts);
            _groupedAccounts.GroupBy<int>(AccountColumns.PersonId);
            var balanceColumn = (IReactiveColumn<decimal>)service.Accounts.Columns[AccountColumns.AccountBalance];
            _groupedAccounts.AddAggregate(balanceColumn, SumColumnId, () => new Sum<decimal>());
            _groupedAccounts.FinishInitialisation();

            var groups = new IndexedObservableCollection<BalanceGroup, int>(g => g.RowIndex);
            _groupedAccounts.ReplayAndSubscribe(
                update =>
                {
                    if (update.Action == TableUpdateAction.Add)
                    {
                        groups.Add(new BalanceGroup(_groupedAccounts, update.RowIndex));
                    }
                    else if (update.Action == TableUpdateAction.Delete)
                    {
                        groups.RemoveAt(groups.GetIndexForKey(update.RowIndex));
                    }
                });
            Groups = groups;
        }

        public IReactiveTable GroupedAccounts { get { return _groupedAccounts; } }
        public ObservableCollection<BalanceGroup> Groups { get; private set; }

        internal class BalanceGroup : ReactiveViewModelBase, IDisposable
        {
            private readonly IReactiveTable _groups;
            private readonly int _rowIndex;

            public BalanceGroup(IReactiveTable groups, int rowIndex)
            {
                _groups = groups;
                _rowIndex = rowIndex;

                groups.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, _rowIndex);
            }

            public int PersonId { get { return _groups.GetValue<int>(AccountColumns.PersonId, _rowIndex); } }

            public decimal BalanceSum { get { return _groups.GetValue<decimal>(SumColumnId, _rowIndex); } }

            public int RowIndex { get { return _rowIndex; } }

            public void Dispose()
            {
                _groups.ChangeNotifier.UnregisterPropertyNotifiedConsumer(this, _rowIndex);
            }
        }

        public void Dispose()
        {
            _groupedAccounts.Dispose();
            foreach (var balanceGroup in Groups)
            {
                balanceGroup.Dispose();
            }
            Groups.Clear();
        }
    }
}
