/*This file is part of ReactiveTables.

ReactiveTables is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ReactiveTables is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with Foobar.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Columns.Calculated;
using ReactiveTables.Framework.Joins;
using ReactiveTables.Framework.Marshalling;
using ReactiveTables.Framework.Synchronisation;
using ReactiveTables.Framework.Utils;
using log4net;

namespace ReactiveTables
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly TimeSpan _updateDelay = TimeSpan.FromMilliseconds(250);
        public static IReactiveTable Humans { get; private set; }
        public static IReactiveTable Accounts { get; private set; }
        public static IReactiveTable AccountHumans { get; private set; }

        private const int humanOffset = 1000;
        private const int accountOffset = 10000;

        public App()
        {
            Console.WriteLine("In constructor");

            log4net.Config.XmlConfigurator.Configure();
            Exit += (sender, args) => _log.Debug(ProcessInfoDumper.GetProcessInfo());

            Humans = new ReactiveTable();
            List<IReactiveColumn> baseHumanColumns = new List<IReactiveColumn>
                                                    {
                                                        new ReactiveColumn<int>(HumanColumns.IdColumn), 
                                                        new ReactiveColumn<string>(HumanColumns.NameColumn)
                                                    };

            ReactiveTable humansWire = new ReactiveTable();
            SetupHumanTable((IWritableReactiveTable) Humans, baseHumanColumns, humansWire, Dispatcher);

            List<IReactiveColumn> baseAccountColumns = new List<IReactiveColumn>
                                                           {
                                                               new ReactiveColumn<int>(AccountColumns.IdColumn),
                                                               new ReactiveColumn<int>(AccountColumns.HumanId),
                                                               new ReactiveColumn<decimal>(AccountColumns.AccountBalance)
                                                           };
            Accounts = new ReactiveTable();
            ReactiveTable accountsWire = new ReactiveTable();
            SetupAccountTable((IWritableReactiveTable) Accounts, baseAccountColumns, accountsWire, Dispatcher);

            AccountHumans = SetupAccountHumansTable(Accounts, Humans);

            Thread humanDataThread = new Thread(StreamHumanData);
            humanDataThread.IsBackground = true;
            humanDataThread.Start(humansWire);

            Thread accountDataThread = new Thread(StreamAccountData);
            accountDataThread.IsBackground = true;
            accountDataThread.Start(accountsWire);
        }

        private IReactiveTable SetupAccountHumansTable(IReactiveTable accounts, IReactiveTable humans)
        {
            IReactiveTable joinedTable = humans.Join(accounts, new Join<int>(
                humans, HumanColumns.IdColumn,
                accounts, AccountColumns.HumanId));

            joinedTable.AddColumn(new ReactiveCalculatedColumn2<string, string, decimal>(
                                      HumanAccountColumns.AccountDetails,
                                      (IReactiveColumn<string>) humans.Columns[HumanColumns.NameColumn],
                                      (IReactiveColumn<decimal>) accounts.Columns[AccountColumns.AccountBalance],
                                      (name, balance) => string.Format("{0} has {1} in their account.", name, balance)));

            return joinedTable;
        }

        private void SetupAccountTable(IWritableReactiveTable accounts, List<IReactiveColumn> baseAccountColumns,
            ReactiveTable accountsWire, Dispatcher dispatcher)
        {
            baseAccountColumns.ForEach(c => accounts.AddColumn(c));

            // Create the wire table
            accountsWire.CloneColumns(accounts);
            new BatchedTableSynchroniser(accountsWire, accounts, new WpfThreadMarshaller(dispatcher), _updateDelay);

            AddAccount(accountsWire, 1, 1, 10m);
            AddAccount(accountsWire, 2, 1, 100m);
            AddAccount(accountsWire, 3, 2, 10000m);
        }

        private static void AddAccount(IWritableReactiveTable accountsWire, int accountId, int humanId, decimal balance)
        {
            int rowId = accountsWire.AddRow();
            accountsWire.SetValue(AccountColumns.IdColumn, rowId, accountOffset + accountId);
            accountsWire.SetValue(AccountColumns.HumanId, rowId, humanOffset + humanId);
            accountsWire.SetValue(AccountColumns.AccountBalance, rowId, balance);
        }

        private void SetupHumanTable(IWritableReactiveTable humans, List<IReactiveColumn> baseColumns, ReactiveTable humansWire, Dispatcher dispatcher)
        {
            baseColumns.ForEach(c => humans.AddColumn(c));

            // Wire up the two tables before the dynamic columns
            humansWire.CloneColumns(Humans);
            new BatchedTableSynchroniser(humansWire, humans, new WpfThreadMarshaller(dispatcher), _updateDelay);

            humans.AddColumn(new ReactiveCalculatedColumn2<string, int, string>(
                                 HumanColumns.IdNameColumn,
                                 (IReactiveColumn<int>) baseColumns[0],
                                 (IReactiveColumn<string>) baseColumns[1],
                                 (idVal, nameVal) => idVal + nameVal));


            AddHuman(humansWire, 1, "Mendel");
            AddHuman(humansWire, 2, "Marie");
        }

        private static void AddHuman(IWritableReactiveTable humans, int id, string name)
        {
            int rowIndex = humans.AddRow();
            humans.SetValue(HumanColumns.IdColumn, rowIndex, humanOffset + id);
            humans.SetValue(HumanColumns.NameColumn, rowIndex, name);
        }

        /// <summary>
        /// Mimic a data stream coming from an asynchronous source.
        /// </summary>
        /// <param name="o"></param>
        private static void StreamHumanData(object o)
        {
            var random = new Random();
            IWritableReactiveTable humans = (IWritableReactiveTable)o;
            Thread.Sleep(6000);
            var id = 3;
            while (true)
            {
                Thread.Sleep(1000);
                AddHuman(humans, id, "Human #" + id);

                UpdateRandomHuman(humans, id, random);
                id++;
            }
        }

        private static void UpdateRandomHuman(IWritableReactiveTable humans, int maxId, Random random)
        {
            int id = random.Next(1, maxId);
            int rowIndex = id - 1;
            var currentValue = humans.GetValue<string>(HumanColumns.NameColumn, rowIndex);
            humans.SetValue(HumanColumns.NameColumn, rowIndex, "*" + currentValue);
//            humans.SetValue(HumanColumns.NameColumn, rowIndex, "Modified at " + DateTime.Now);
        }

        private static void StreamAccountData(object o)
        {
            var random = new Random();
            IWritableReactiveTable accounts = (IWritableReactiveTable)o;
            Thread.Sleep(3000);
            var id = 3;
            while (true)
            {
                Thread.Sleep(1000);
                AddAccount(accounts, id, id, 66666m);

                UpdateRandomAccount(accounts, id, random);
                id++;
            }            
        }

        private static void UpdateRandomAccount(IWritableReactiveTable accounts, int maxId, Random random)
        {
            int id = random.Next(1, maxId);
            int rowIndex = id - 1;
            var currentBalance = accounts.GetValue<decimal>(AccountColumns.AccountBalance, rowIndex);
            decimal offset = id%2 == 0 ? 3242 : -7658;
            accounts.SetValue(AccountColumns.AccountBalance, rowIndex, currentBalance + offset);
        }
    }
}
