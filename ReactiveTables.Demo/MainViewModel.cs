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

        public MainViewModel(IAccountBalanceDataService dataService)
        {
            _dataService = dataService;

            Humans = new HumansViewModel(_dataService.Humans);
            Accounts = new AccountsViewModel(_dataService.Accounts);

            FilteredTable accountFilter = new FilteredTable(
                _dataService.AccountHumans,
                new DelegatePredicate1<decimal>(AccountColumns.AccountBalance, b => b > BalanceBelowFilter));
            HumanAccounts = new HumanAccountsViewModel(accountFilter, (IWritableReactiveTable)_dataService.Accounts);
            //            HumansBindingList = new ReactiveBindingList(App.Humans);

            StartData = new DelegateCommand(() => _dataService.Start());
            StopData = new DelegateCommand(() => _dataService.Stop());
        }

        public AccountsViewModel Accounts { get; private set; }
        public HumansViewModel Humans { get; private set; }
        public HumanAccountsViewModel HumanAccounts { get; private set; }
        public ReactiveBindingList HumansBindingList { get; set; }

        public ICommand StopData { get; private set; }
        public ICommand StartData { get; private set; }

        public decimal BalanceBelowFilter
        {
            get { return _balanceBelowFilter; }
            set { SetProperty(ref _balanceBelowFilter, value); }
        }

        public void Dispose()
        {
            Humans.Dispose();
            Accounts.Dispose();
            HumanAccounts.Dispose();
        }
    }
}