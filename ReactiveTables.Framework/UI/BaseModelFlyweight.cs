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
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace ReactiveTables.Framework.UI
{
    public interface IBaseModelFlyweight : INotifyPropertyChanged, IReactivePropertyNotifiedConsumer
    {
        event PropertyChangedEventHandler PropertyChanged;
        void SetRowIndex(int rowIndex);
        void SetTable(IReactiveTable table);
    }

    public abstract class BaseModelFlyweight<TFlyWeight> : IBaseModelFlyweight
    {
        private int _index;
        private IReactiveTable _table;
        private readonly Dictionary<string, string> _columnIdByPopertyNames = new Dictionary<string, string>();

        protected BaseModelFlyweight()
        {
            var type = typeof (TFlyWeight);
            foreach (var propertyInfo in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                _columnIdByPopertyNames[propertyInfo.Name] = $"{type.Name}.{propertyInfo.Name}";
            }
        }

        public void SetTable(IReactiveTable table)
        {
            _table = table;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetRowIndex(int rowIndex)
        {
            _index = rowIndex;
        }

        protected TColumn GetValue<TColumn>([CallerMemberName] string propertyName = "")
        {
            return _table.GetValue<TColumn>(_columnIdByPopertyNames[propertyName], _index);
        }

        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}