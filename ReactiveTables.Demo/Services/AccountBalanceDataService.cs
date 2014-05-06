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
using System.Reflection;
using System.Threading;
using System.Windows.Threading;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Columns.Calculated;
using ReactiveTables.Framework.Joins;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Synchronisation;
using log4net;

namespace ReactiveTables.Demo.Services
{
    internal interface IAccountBalanceDataService
    {
        IReactiveTable People { get; set; }
        IReactiveTable Accounts { get; }
        IReactiveTable AccountPeople { get; }
        void Start();
        void Stop();
    }

    internal class AccountBalanceDataService : IAccountBalanceDataService
    {
        private readonly int _maxEntries;
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly TimeSpan _synchroniseTablesDelay = TimeSpan.FromMilliseconds(200);
        private const int BatchSize = 5;
        private const int DataDelay = 0;
        private const int GuiDisplayDelay = 0;
        private const int PersonDataPauseDelay = 200;
        private const int AccountDataPauseDelay = 200;
        private const int PersonIdOffset = 1000;
        private const int AccountIdOffset = 10000;

        private readonly ManualResetEventSlim _running = new ManualResetEventSlim(true);

        public IReactiveTable People { get; set; }
        public IReactiveTable Accounts { get; private set; }
        public IReactiveTable AccountPeople { get; private set; }

        public void Start()
        {
            _running.Set();
        }

        public void Stop()
        {
            _running.Reset();
        }

        public AccountBalanceDataService(Dispatcher dispatcher, int maxEntries = int.MaxValue)
        {
            _maxEntries = maxEntries;
            People = new ReactiveTable();
            List<IReactiveColumn> basePersonColumns = new List<IReactiveColumn>
                                                      {
                                                          new ReactiveColumn<int>(PersonColumns.IdColumn),
                                                          new ReactiveColumn<string>(PersonColumns.NameColumn)
                                                      };

            IWritableReactiveTable peopleWire = SetupPersonTable((IWritableReactiveTable) People, basePersonColumns, dispatcher);

            List<IReactiveColumn> baseAccountColumns = new List<IReactiveColumn>
                                                       {
                                                           new ReactiveColumn<int>(AccountColumns.IdColumn),
                                                           new ReactiveColumn<int>(AccountColumns.PersonId),
                                                           new ReactiveColumn<decimal>(AccountColumns.AccountBalance)
                                                       };
            Accounts = new ReactiveTable();
            IWritableReactiveTable accountsWire = SetupAccountTable((IWritableReactiveTable) Accounts, baseAccountColumns, dispatcher);

            AccountPeople = SetupAccountPeopleTable(Accounts, People);

            AddTestPeople(peopleWire);
            AddTestAccounts(accountsWire);

            Thread personDataThread = new Thread(StreamPersonData);
            personDataThread.IsBackground = true;
            personDataThread.Start(new StreamingState {Table = peopleWire, Running = _running, MaxEntries = _maxEntries});

            Thread accountDataThread = new Thread(StreamAccountData);
            accountDataThread.IsBackground = true;
            accountDataThread.Start(new StreamingState {Table = accountsWire, Running = _running, MaxEntries = _maxEntries});

            Thread.Sleep(GuiDisplayDelay);
        }

        private IReactiveTable SetupAccountPeopleTable(IReactiveTable accounts, IReactiveTable people)
        {
            IReactiveTable joinedTable = people.Join(accounts, new Join<int>(
                                                                   people, PersonColumns.IdColumn,
                                                                   accounts, AccountColumns.PersonId));

            joinedTable.AddColumn(new ReactiveCalculatedColumn2<string, string, decimal>(
                                      PersonAccountColumns.AccountDetails,
                                      (IReactiveColumn<string>) people.Columns[PersonColumns.NameColumn],
                                      (IReactiveColumn<decimal>) accounts.Columns[AccountColumns.AccountBalance],
                                      (name, balance) => string.Format("{0} has {1} in their account.", name, balance)));

            return joinedTable;
        }

        private IWritableReactiveTable SetupAccountTable(IWritableReactiveTable accounts,
                                                         List<IReactiveColumn> baseAccountColumns,
                                                         Dispatcher dispatcher)
        {
            baseAccountColumns.ForEach(col => accounts.AddColumn(col));

            // Create the wire table
            var accountsWire = new ReactiveBatchedPassThroughTable(accounts, new WpfThreadMarshaller(dispatcher), _synchroniseTablesDelay);
            //            var accountsWire = new ReactiveTable(accounts);
            //            new BatchedTableSynchroniser(accountsWire, accounts, new WpfThreadMarshaller(dispatcher), _synchroniseTablesDelay);

            return accountsWire;
        }

        private static void AddTestAccounts(IWritableReactiveTable accounts)
        {
            AddAccount(accounts, 1, 1, 10m);
            AddAccount(accounts, 2, 1, 100m);
            AddAccount(accounts, 3, 2, 10000m);
        }

        private static void AddAccount(IWritableReactiveTable accountsWire, int accountId, int personId, decimal balance)
        {
            int rowId = accountsWire.AddRow();
            accountsWire.SetValue(AccountColumns.IdColumn, rowId, AccountIdOffset + accountId);
            accountsWire.SetValue(AccountColumns.PersonId, rowId, PersonIdOffset + personId);
            accountsWire.SetValue(AccountColumns.AccountBalance, rowId, balance);
        }

        private IWritableReactiveTable SetupPersonTable(IWritableReactiveTable people,
                                                        List<IReactiveColumn> baseColumns,
                                                        Dispatcher dispatcher)
        {
            baseColumns.ForEach(col => people.AddColumn(col));

            // Wire up the two tables before the dynamic columns
            var peopleWire = new ReactiveBatchedPassThroughTable(people, new WpfThreadMarshaller(dispatcher), _synchroniseTablesDelay);
            //            var PeopleWire = new ReactiveTable(People);
            //            new BatchedTableSynchroniser(PeopleWire, People, new WpfThreadMarshaller(dispatcher), _synchroniseTablesDelay);

            people.AddColumn(new ReactiveCalculatedColumn2<string, int, string>(
                                 PersonColumns.IdNameColumn,
                                 (IReactiveColumn<int>) baseColumns[0],
                                 (IReactiveColumn<string>) baseColumns[1],
                                 (idVal, nameVal) => idVal + nameVal));

            return peopleWire;
        }

        private static void AddTestPeople(IWritableReactiveTable people)
        {
            AddPerson(people, 1, "Mendel");
            AddPerson(people, 2, "Marie");
        }

        private static void AddPerson(IWritableReactiveTable people, int id, string name)
        {
            int rowIndex = people.AddRow();
            people.SetValue(PersonColumns.IdColumn, rowIndex, PersonIdOffset + id);
            people.SetValue(PersonColumns.NameColumn, rowIndex, name);
        }

        /// <summary>
        /// Mimic a data stream coming from an asynchronous source.
        /// </summary>
        /// <param name="o"></param>
        private static void StreamPersonData(object o)
        {
            var state = (StreamingState) o;
            var running = state.Running;
            IWritableReactiveTable people = state.Table;
            Thread.Sleep(DataDelay);
            var id = 3;
            while (id < state.MaxEntries)
            {
                running.Wait();
                Thread.Sleep(PersonDataPauseDelay);
                for (int i = 0; i < BatchSize; i++)
                {
                    AddPerson(people, id, "Person #" + id);

                    // UpdateRandomPerson(People, id, random);
                    id++;
                }
            }
        }

        private static void StreamAccountData(object o)
        {
            var state = (StreamingState) o;
            var running = state.Running;
            var random = new Random();
            IWritableReactiveTable accounts = state.Table;
            Thread.Sleep(DataDelay);
            var id = 3;
            while (id < state.MaxEntries)
            {
                running.Wait();
                Thread.Sleep(AccountDataPauseDelay);

                for (int i = 0; i < BatchSize; i++)
                {
                    AddAccount(accounts, id, id, random.Next(1000, 10000000));

                    // UpdateRandomAccount(accounts, id, random);
                    id++;
                }
            }
        }

        private static void UpdateRandomPerson(IWritableReactiveTable people, int maxId, Random random)
        {
            int id = random.Next(1, maxId);
            int rowIndex = id - 1;
            //            var currentValue = People.GetValue<string>(PersonColumns.NameColumn, rowIndex);
            people.SetValue(PersonColumns.NameColumn, rowIndex, new string('*', random.Next(0, BatchSize)));
            //            People.SetValue(PersonColumns.NameColumn, rowIndex, "Modified at " + DateTime.Now);
        }

        private static void UpdateRandomAccount(IWritableReactiveTable accounts, int maxId, Random random)
        {
            int id = random.Next(1, maxId);
            int rowIndex = id - 1;
            var currentBalance = random.Next(0, 100000); // accounts.GetValue<decimal>(AccountColumns.AccountBalance, rowIndex);
            decimal offset = id%2 == 0 ? 3242 : -7658;
            accounts.SetValue(AccountColumns.AccountBalance, rowIndex, currentBalance + offset);
        }


        private class StreamingState
        {
            public IWritableReactiveTable Table { get; set; }
            public ManualResetEventSlim Running { get; set; }
            public int MaxEntries { get; set; }
        }
    }
}