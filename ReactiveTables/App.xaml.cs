using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using ReactiveTables.Framework;

namespace ReactiveTables
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static IReactiveTable Humans { get; private set; }

        public App()
        {
            Console.WriteLine("In constructor");

            Humans = new ReactiveTable();
            List<IReactiveColumn> baseColumns = new List<IReactiveColumn>
                                                    {
                                                        new ReactiveColumn<int>(HumanColumns.IdColumn), 
                                                        new ReactiveColumn<string>(HumanColumns.NameColumn)
                                                    };

            ReactiveTable humansWire;
            int id = CreateTestData((IWritableReactiveTable) Humans, baseColumns, out humansWire, Dispatcher);
            
            Thread dataThread = new Thread(StreamData);
            dataThread.IsBackground = true;
            dataThread.Start(humansWire);
        }

        private static void StreamData(object o)
        {
            IWritableReactiveTable humans = (IWritableReactiveTable) o;
            Thread.Sleep(1000);
            var id = 3;
            while (true)
            {
                Thread.Sleep(1000);
                AddHuman(humans, id, "Human #" + id);
            }
        }

        private static int CreateTestData(IWritableReactiveTable humans, List<IReactiveColumn> baseColumns, out ReactiveTable humansWire, Dispatcher dispatcher)
        {
            baseColumns.ForEach(c => humans.AddColumn(c));

            // Wire up the two tables before the dynamic columns
            humansWire = new ReactiveTable(Humans);
            new TableSynchroniser(humansWire, humans, new WpfThreadMarshaller(dispatcher));

            var idNameCol = humans.AddColumn(new ReactiveCalculatedColumn2<string, int, string>(
                                                  HumanColumns.IdNameColumn,
                                                  (IReactiveColumn<int>) baseColumns[0],
                                                  (IReactiveColumn<string>) baseColumns[1],
                                                  (idVal, nameVal) => idVal + nameVal));


            var id = 1;
            var _rowIndex = AddHuman(humansWire, id++, "Mendel");
            _rowIndex = AddHuman(humansWire, id++, "Marie");
            return id;
        }

        private static int AddHuman(IWritableReactiveTable humans, int id, string name)
        {
            int rowIndex = humans.AddRow();
            humans.SetValue(HumanColumns.IdColumn, rowIndex, id);
            humans.SetValue(HumanColumns.NameColumn, rowIndex, name);
            return rowIndex;
        }
    }


    /*public class EntryPoint
    {
        [STAThread]
        public static void Main(string[] args)
        {
            var app = new App();
            app.Run();
        }
    }*/
}
