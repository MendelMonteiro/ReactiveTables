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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using ReactiveTables.Framework.Columns;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Framework
{
    public interface IReactiveTable<T> : IReactiveTable 
        where T : IBaseModelFlyweight, new()
    {
        ObservableCollection<T> Flyweights { get; }
    }

    public class ReactiveTable<T> : ReactiveTable, IReactiveTable<T> 
        where T : IBaseModelFlyweight, new()
    {
        public ReactiveTable()
        {
            var type = typeof(T);
            foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                             .Where(p => p.GetCustomAttribute<CalculatedColumnAttribute>() == null))
            {
                AddColumn(ReactiveColumn.Create($"{type.Name}.{propertyInfo.Name}", propertyInfo.PropertyType));
            }

            var token = Subscribe(update =>
                               {
                                   if (update.Action == TableUpdateAction.Add)
                                   {
                                       var item = new T();
                                       item.SetTable(this);
                                       item.SetRowIndex(update.RowIndex);
                                       ChangeNotifier.RegisterPropertyNotifiedConsumer(item, update.RowIndex);
                                       Flyweights.Add(item);
                                   }
                                   else if (update.Action == TableUpdateAction.Delete)
                                   {
                                       // TODO: Look at efficiency of this
                                       var item = Flyweights[update.RowIndex];
                                       ChangeNotifier.UnregisterPropertyNotifiedConsumer(item, update.RowIndex);
                                       Flyweights.RemoveAt(update.RowIndex);
                                   }
                               });
        }

        public ObservableCollection<T> Flyweights { get; } = new ObservableCollection<T>();
    }
}