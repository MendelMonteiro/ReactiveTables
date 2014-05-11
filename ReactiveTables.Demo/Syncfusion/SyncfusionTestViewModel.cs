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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ReactiveTables.Demo.Services;
using ReactiveTables.Framework.Filters;
using ReactiveTables.Framework.Sorting;
using System.Linq;

namespace ReactiveTables.Demo.Syncfusion
{
    class SyncfusionTestViewModel : SyncfusionViewModelBase
    {
        private decimal _balanceBelowFilter;
        private readonly FilteredTable _accountFilter;
        private readonly SortedTable _balanceSort;
        private string _sortByColumn;
        private readonly Dictionary<string, Type> _columnTypes;

        public SyncfusionTestViewModel(IAccountBalanceDataService dataService)
        {
            var table = dataService.AccountPeople;
            _accountFilter = (FilteredTable) table.Filter(
                new DelegatePredicate1<decimal>(AccountColumns.AccountBalance, b => b > BalanceBelowFilter));
            _balanceSort = new SortedTable(_accountFilter);
            _balanceSort.SortBy(AccountColumns.AccountBalance, Comparer<decimal>.Default);
            SetTable(_balanceSort);

            _columnTypes = new Dictionary<string, Type>
                              {
                                  {AccountColumns.AccountBalance, typeof(decimal)},
                                  {PersonColumns.IdNameColumn, typeof(string)},
                                  {PersonColumns.NameColumn, typeof(string)},
                                  {PersonAccountColumns.AccountDetails, typeof(string)}
                              };
            Columns = new ObservableCollection<string>(_columnTypes.Keys);
        }

        public ObservableCollection<string> Columns { get; private set; }

        public decimal BalanceBelowFilter
        {
            get { return _balanceBelowFilter; }
            set
            {
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

                    try
                    {
                        var method = typeof(SortedTable).GetMethods().First(info => info.Name == "SortBy");
                        var generic = method.MakeGenericMethod(_columnTypes[_sortByColumn]);
                        generic.Invoke(_balanceSort, new object[] {_sortByColumn});
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }
        }
    }
}