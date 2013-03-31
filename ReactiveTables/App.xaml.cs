using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.Columns.Calculated;
using ReactiveTables.Framework.Joins;

namespace ReactiveTables
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IReactiveTable Humans { get; private set; }
        public static IReactiveTable Accounts { get; private set; }
        public static IReactiveTable AccountHumans { get; private set; }

        public App()
        {
            Console.WriteLine("In constructor");

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
            IReactiveTable joinedTable = accounts.Join(humans, new InnerJoin<int>(accounts, 
                AccountColumns.HumanId, humans, HumanColumns.IdColumn));
            return joinedTable;
        }

        private void SetupAccountTable(IWritableReactiveTable accounts, List<IReactiveColumn> baseAccountColumns,
            ReactiveTable accountsWire, Dispatcher dispatcher)
        {
            baseAccountColumns.ForEach(c => accounts.AddColumn(c));

            // Create the wire table
            accountsWire.CloneColumns(accounts);
            new TableSynchroniser(accountsWire, accounts, new WpfThreadMarshaller(dispatcher));

            AddAccount(accountsWire, 1, 10m);
            AddAccount(accountsWire, 2, 100m);
            AddAccount(accountsWire, 2, 10000m);
        }

        private static void AddAccount(IWritableReactiveTable accountsWire, int humanId, decimal balance)
        {
            int rowId = accountsWire.AddRow();
            const int accountIdOffset = 1000;
            accountsWire.SetValue(AccountColumns.IdColumn, rowId, accountIdOffset + humanId);
            accountsWire.SetValue(AccountColumns.HumanId, rowId, humanId);
            accountsWire.SetValue(AccountColumns.AccountBalance, rowId, balance);
        }

        private static void SetupHumanTable(IWritableReactiveTable humans, List<IReactiveColumn> baseColumns, ReactiveTable humansWire, Dispatcher dispatcher)
        {
            baseColumns.ForEach(c => humans.AddColumn(c));

            // Wire up the two tables before the dynamic columns
            humansWire.CloneColumns(Humans);
            new TableSynchroniser(humansWire, humans, new WpfThreadMarshaller(dispatcher));

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
            humans.SetValue(HumanColumns.IdColumn, rowIndex, id);
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
            Thread.Sleep(1000);
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
            humans.SetValue(HumanColumns.NameColumn, rowIndex, "Modified at " + DateTime.Now);
        }

        private static void StreamAccountData(object o)
        {
            var random = new Random();
            IWritableReactiveTable accounts = (IWritableReactiveTable)o;
            Thread.Sleep(1000);
            var id = 3;
            while (true)
            {
                Thread.Sleep(1000);
                AddAccount(accounts, id, 66666m);

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
