using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

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
            Thread dataThread = new Thread(StreamData);
            dataThread.Start(Humans);
        }

        private static void StreamData(object o)
        {
            IWritableReactiveTable humans = (IWritableReactiveTable) o;
            Thread.Sleep(1000);
            int id = CreateTestData(humans);
            while (true)
            {
                Thread.Sleep(1000);
                AddHuman(humans, id, "Human #" + id);
            }
        }

        private static int CreateTestData(IWritableReactiveTable humans)
        {
            var idCol = humans.AddColumn(new ReactiveColumn<int>(HumanColumns.IdColumn));
            var nameCol = humans.AddColumn(new ReactiveColumn<string>(HumanColumns.NameColumn));
            var idNameCol = humans.AddColumn(new ReactiveCalculatedColumn2<string, int, string>(
                                                  HumanColumns.IdNameColumn,
                                                  idCol,
                                                  nameCol,
                                                  (idVal, nameVal) => idVal + nameVal));
            var id = 1;
            var _rowIndex = AddHuman(humans, id++, "Mendel");
            _rowIndex = AddHuman(humans, id++, "Marie");
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

    class DataCache
    {
         
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
