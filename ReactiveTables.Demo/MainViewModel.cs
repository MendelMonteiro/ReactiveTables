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
using ReactiveTables.Framework;

namespace ReactiveTables.Demo
{
    internal class MainViewModel : IDisposable
    {
        public MainViewModel()
        {
            Humans = new HumansViewModel(App.Humans);
            Accounts = new AccountsViewModel(App.Accounts);
            HumanAccounts = new HumanAccountsViewModel(App.AccountHumans, (IWritableReactiveTable) App.Accounts);
//            HumansBindingList = new ReactiveBindingList(App.Humans);
        }

        public AccountsViewModel Accounts { get; private set; }
        public HumansViewModel Humans { get; private set; }
        public HumanAccountsViewModel HumanAccounts { get; private set; }
        public ReactiveBindingList HumansBindingList { get; set; }
        public void Dispose()
        {
            Humans.Dispose();
            Accounts.Dispose();
            HumanAccounts.Dispose();
        }
    }
}