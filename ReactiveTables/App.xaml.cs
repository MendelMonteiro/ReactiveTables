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
along with ReactiveTables.  If not, see <http://www.gnu.org/licenses/>.
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

        private readonly TimeSpan _synchroniseTablesDelay = TimeSpan.FromMilliseconds(500);
        private const int BatchSize = 50;
        private const int DataDelay = 0;
        private const int GuiDisplayDelay = 0;
        public static IReactiveTable Humans { get; set; }
        public static IReactiveTable Accounts { get; private set; }
        public static IReactiveTable AccountHumans { get; private set; }

        private const int HumanIdOffset = 1000;
        private const int AccountIdOffset = 10000;
        private const int HumanDataPauseDelay = 2500;
        private const int AccountDataPauseDelay = 5000;

        protected override void OnStartup(StartupEventArgs e)
        {
//            Xceed.Wpf.DataGrid.Licenser.LicenseKey = "DGP45-L7AAA-RUWWA-5BBA";
        }

        public App()
        {
            log4net.Config.XmlConfigurator.Configure();
            Exit += (sender, args) => _log.Debug(ProcessInfoDumper.GetProcessInfo());

            Humans = new ReactiveTable();
            List<IReactiveColumn> baseHumanColumns = new List<IReactiveColumn>
                                                    {
                                                        new ReactiveColumn<int>(HumanColumns.IdColumn), 
                                                        new ReactiveColumn<string>(HumanColumns.NameColumn)
                                                    };

            IWritableReactiveTable humansWire = SetupHumanTable((IWritableReactiveTable) Humans, baseHumanColumns, Dispatcher);

            List<IReactiveColumn> baseAccountColumns = new List<IReactiveColumn>
                                                           {
                                                               new ReactiveColumn<int>(AccountColumns.IdColumn),
                                                               new ReactiveColumn<int>(AccountColumns.HumanId),
                                                               new ReactiveColumn<decimal>(AccountColumns.AccountBalance)
                                                           };
            Accounts = new ReactiveTable();
            IWritableReactiveTable accountsWire = SetupAccountTable((IWritableReactiveTable) Accounts, baseAccountColumns, Dispatcher);

            AccountHumans = SetupAccountHumansTable(Accounts, Humans);

            AddTestHumans(humansWire);
            AddTestAccounts(accountsWire);

            Thread humanDataThread = new Thread(StreamHumanData);
            humanDataThread.IsBackground = true;
            humanDataThread.Start(humansWire);

            Thread accountDataThread = new Thread(StreamAccountData);
            accountDataThread.IsBackground = true;
            accountDataThread.Start(accountsWire);

            Thread.Sleep(GuiDisplayDelay);
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

        private IWritableReactiveTable SetupAccountTable(IWritableReactiveTable accounts, List<IReactiveColumn> baseAccountColumns, Dispatcher dispatcher)
        {
            baseAccountColumns.ForEach(accounts.AddColumn);

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

        private static void AddAccount(IWritableReactiveTable accountsWire, int accountId, int humanId, decimal balance)
        {
            int rowId = accountsWire.AddRow();
            accountsWire.SetValue(AccountColumns.IdColumn, rowId, AccountIdOffset + accountId);
            accountsWire.SetValue(AccountColumns.HumanId, rowId, HumanIdOffset + humanId);
            accountsWire.SetValue(AccountColumns.AccountBalance, rowId, balance);
        }

        private IWritableReactiveTable SetupHumanTable(IWritableReactiveTable humans, List<IReactiveColumn> baseColumns, Dispatcher dispatcher)
        {
            baseColumns.ForEach(humans.AddColumn);

            // Wire up the two tables before the dynamic columns
            var humansWire = new ReactiveBatchedPassThroughTable(humans, new WpfThreadMarshaller(dispatcher), _synchroniseTablesDelay);
//            var humansWire = new ReactiveTable(humans);
//            new BatchedTableSynchroniser(humansWire, humans, new WpfThreadMarshaller(dispatcher), _synchroniseTablesDelay);

            humans.AddColumn(new ReactiveCalculatedColumn2<string, int, string>(
                                 HumanColumns.IdNameColumn,
                                 (IReactiveColumn<int>) baseColumns[0],
                                 (IReactiveColumn<string>) baseColumns[1],
                                 (idVal, nameVal) => idVal + nameVal));

            return humansWire;
        }

        private static void AddTestHumans(IWritableReactiveTable humans)
        {
            AddHuman(humans, 1, "Mendel");
            AddHuman(humans, 2, "Marie");
        }

        private static void AddHuman(IWritableReactiveTable humans, int id, string name)
        {
            int rowIndex = humans.AddRow();
            humans.SetValue(HumanColumns.IdColumn, rowIndex, HumanIdOffset + id);
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
            Thread.Sleep(DataDelay);
            var id = 3;
            while (true)
            {
                Thread.Sleep(HumanDataPauseDelay);
                for (int i = 0; i < BatchSize; i++)
                {
                    AddHuman(humans, id, "Human #" + id);

//                    UpdateRandomHuman(humans, id, random);
                    id++;
                }
            }
        }

        private static void StreamAccountData(object o)
        {
            var random = new Random();
            IWritableReactiveTable accounts = (IWritableReactiveTable)o;
            Thread.Sleep(DataDelay);
            var id = 3;
            while (true)
            {
                Thread.Sleep(AccountDataPauseDelay);

                for (int i = 0; i < BatchSize; i++)
                {
                    AddAccount(accounts, id, id, 66666m);

//                    UpdateRandomAccount(accounts, id, random);
                    id++;
                }
            }            
        }

        private static void UpdateRandomHuman(IWritableReactiveTable humans, int maxId, Random random)
        {
            int id = random.Next(1, maxId);
            int rowIndex = id - 1;
//            var currentValue = humans.GetValue<string>(HumanColumns.NameColumn, rowIndex);
            humans.SetValue(HumanColumns.NameColumn, rowIndex, new string('*', random.Next(0, BatchSize)));
//            humans.SetValue(HumanColumns.NameColumn, rowIndex, "Modified at " + DateTime.Now);
        }

        private static void UpdateRandomAccount(IWritableReactiveTable accounts, int maxId, Random random)
        {
            int id = random.Next(1, maxId);
            int rowIndex = id - 1;
            var currentBalance = random.Next(0, 100000);// accounts.GetValue<decimal>(AccountColumns.AccountBalance, rowIndex);
            decimal offset = id%2 == 0 ? 3242 : -7658;
            accounts.SetValue(AccountColumns.AccountBalance, rowIndex, currentBalance + offset);
        }
    }
}
