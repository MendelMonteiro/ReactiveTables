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
using System.Windows.Input;
using ReactiveTables.Demo.Services;
using ReactiveTables.Demo.Utils;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Filters;

namespace ReactiveTables.Demo
{
    internal class MainViewModel : BaseViewModel, IDisposable
    {
        private readonly IAccountBalanceDataService _dataService;
        private decimal _balanceBelowFilter;
        private readonly FilteredTable _accountFilter;

        public MainViewModel(IAccountBalanceDataService dataService)
        {
            _dataService = dataService;

            People = new PeopleViewModel(_dataService.People);
            Accounts = new AccountsViewModel(_dataService.Accounts);

            _accountFilter = (FilteredTable) _dataService.AccountPeople.Filter(
                new DelegatePredicate1<decimal>(AccountColumns.AccountBalance, b => b > BalanceBelowFilter));
            PersonAccounts = new PersonAccountsViewModel(_accountFilter, (IWritableReactiveTable)_dataService.Accounts);
            //            PeopleBindingList = new ReactiveBindingList(App.People);

            StartData = new DelegateCommand(() => _dataService.Start());
            StopData = new DelegateCommand(() => _dataService.Stop());
        }

        public AccountsViewModel Accounts { get; private set; }
        public PeopleViewModel People { get; private set; }
        public PersonAccountsViewModel PersonAccounts { get; private set; }
        public ReactiveBindingList PeopleBindingList { get; set; }

        public ICommand StopData { get; private set; }
        public ICommand StartData { get; private set; }

        public decimal BalanceBelowFilter
        {
            get { return _balanceBelowFilter; }
            set
            {
                SetProperty(ref _balanceBelowFilter, value);
                _accountFilter.PredicateChanged();
            }
        }

        public void Dispose()
        {
            People.Dispose();
            Accounts.Dispose();
            PersonAccounts.Dispose();
        }
    }
}