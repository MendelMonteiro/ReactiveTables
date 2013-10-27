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

using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveTables.Demo.Services;
using ReactiveTables.Framework.Filters;
using ReactiveTables.Framework.Sorting;

namespace ReactiveTables.Demo.Syncfusion
{
    class SyncfusionTestViewModel : SyncfusionViewModelBase
    {
        private decimal _balanceBelowFilter;
        private readonly FilteredTable _accountFilter;
        private readonly SortedTable<decimal> _balanceSort;
        private string _sortByColumn;

        public SyncfusionTestViewModel(IAccountBalanceDataService dataService)
        {
            var table = dataService.AccountPeople;
            _accountFilter = (FilteredTable)table.Filter(
                new DelegatePredicate1<decimal>(AccountColumns.AccountBalance, b => b > BalanceBelowFilter));
            _balanceSort = new SortedTable<decimal>(_accountFilter, AccountColumns.AccountBalance, Comparer<decimal>.Default);
            SetTable(_balanceSort);

            Columns = new ObservableCollection<string>(new[] {
                                                               AccountColumns.AccountBalance,
                                                               PersonColumns.IdNameColumn,
                                                               PersonColumns.NameColumn,
                                                               PersonAccountColumns.AccountDetails
                                                           });
        }

        public ObservableCollection<string> Columns { get; private set; }

        public decimal BalanceBelowFilter
        {
            get { return _balanceBelowFilter; }
            set { 
                SetProperty(ref _balanceBelowFilter, value);
                _accountFilter.PredicateChanged();
            }
        }

        public string SortByColumn
        {
            get { return _sortByColumn; }
            set
            {
                if (_sortByColumn != value)
                {
                    _sortByColumn = value;
//                    _balanceSort.SetSort(_sortByColumn, Comparer<>);
                }
            }
        }
    }
}