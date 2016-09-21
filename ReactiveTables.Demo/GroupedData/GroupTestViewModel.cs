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
using System.Diagnostics;
using ReactiveTables.Demo.Services;
using ReactiveTables.Demo.Syncfusion;
using ReactiveTables.Demo.Utils;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Aggregate;
using ReactiveTables.Framework.Aggregate.Operations;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Demo.GroupedData
{
    class GroupTestViewModelSyncfusion : SyncfusionViewModelBase
    {
        public GroupTestViewModelSyncfusion(IAccountBalanceDataService service)
        {
            var accountsTable = service.Accounts;
            var groupedAccounts = new AggregatedTable(accountsTable);
            groupedAccounts.GroupBy<int>(AccountColumns.PersonId);
            var balanceColumn = (IReactiveColumn<decimal>)accountsTable.GetColumnByName(AccountColumns.AccountBalance);
            groupedAccounts.AddAggregate(balanceColumn, GroupTestViewModel.SumColumnId, () => new Sum<decimal>());
            groupedAccounts.AddAggregate(balanceColumn, GroupTestViewModel.CountColumnId, () => new Count<decimal>());
            groupedAccounts.FinishInitialisation();
            SetTable(groupedAccounts);
        }
    }

    internal class GroupTestViewModel : ReactiveViewModelBase, IDisposable
    {
        private readonly AggregatedTable _groupedAccounts;
        public const string SumColumnId = "Sum";
        public const string CountColumnId = "Count";

        public GroupTestViewModel(IAccountBalanceDataService service)
        {
            var accountsTable = service.Accounts;
            _groupedAccounts = new AggregatedTable(accountsTable);
            _groupedAccounts.GroupBy<int>(AccountColumns.PersonId);
            var balanceColumn = (IReactiveColumn<decimal>)accountsTable.GetColumnByName(AccountColumns.AccountBalance);
            _groupedAccounts.AddAggregate(balanceColumn, SumColumnId, () => new Sum<decimal>());
            _groupedAccounts.AddAggregate(balanceColumn, CountColumnId, () => new Count<decimal>());
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

            var accounts = new IndexedObservableCollection<AccountViewModel, int>(a => a.RowIndex);
            accountsTable.ReplayAndSubscribe(
                update =>
                {
                    if (update.Action == TableUpdateAction.Add)
                    {
                        var accountViewModel = new AccountViewModel(accountsTable, update.RowIndex);
                        accounts.Add(accountViewModel);
                        Debug.WriteLine("{1},Adding,,{0},", update.RowIndex, DateTime.Now.ToLongTimeString());
                    }
                    else if (update.Action == TableUpdateAction.Delete)
                    {
                        var indexForKey = accounts.GetIndexForKey(update.RowIndex);
                        var accountViewModel = accounts[indexForKey];
                        accounts.RemoveAt(indexForKey);
                        Debug.WriteLine("{1},Removing,{0}", accountViewModel.PersonId, DateTime.Now.ToLongTimeString());
                    }
                    else
                    {
                        var indexForKey = accounts.GetIndexForKey(update.RowIndex);
                        var accountViewModel = accounts[indexForKey];
                        Debug.WriteLine("{1},Modifying,{0},{2},{3}",
                                        accountViewModel.PersonId, DateTime.Now.ToLongTimeString(), update.RowIndex, update.Column.ColumnId);
                    }
                });
            Groups = groups;
            Accounts = accounts;
        }

        public ObservableCollection<AccountViewModel> Accounts { get; private set; }
        public ObservableCollection<BalanceGroup> Groups { get; }

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

            public int PersonId => _groups.GetValue<int>(AccountColumns.PersonId, _rowIndex);

            public decimal BalanceSum => _groups.GetValue<decimal>(SumColumnId, _rowIndex);

            public int Accounts => _groups.GetValue<int>(CountColumnId, _rowIndex);

            public int RowIndex => _rowIndex;

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
