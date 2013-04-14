﻿using ReactiveTables.Framework;

namespace ReactiveTables
{
    class MainViewModel
    {
        public MainViewModel()
        {
//            Humans = new HumansViewModel(App.Humans);
//            Accounts = new AccountsViewModel(App.Accounts);
            HumanAccounts = new HumanAccountsViewModel(App.AccountHumans, (IWritableReactiveTable) App.Accounts);
        }

        public AccountsViewModel Accounts { get; private set; }
        public HumansViewModel Humans { get; private set; }
        public HumanAccountsViewModel HumanAccounts { get; private set; }
    }
}