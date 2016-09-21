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
using ReactiveTables.Demo.Services;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;
using Xceed.Wpf.DataGrid;

namespace ReactiveTables.Demo
{
    public struct AccountPeopleSelector : IReactivePropertyNotifiedConsumer, INotifyPropertyChanged
    {
        private readonly IReactiveTable _table;
        private readonly int _rowId;

        public AccountPeopleSelector(IReactiveTable table, int rowId) : this()
        {
            _table = table;
            _rowId = rowId;
            table.ChangeNotifier.RegisterPropertyNotifiedConsumer(this, rowId);
        }

        public int Id => _table.GetValue<int>(PersonColumns.IdColumn, _rowId);

        public decimal AccountBalance => _table.GetValue<decimal>(AccountColumns.AccountBalance, _rowId);

        public string Name => _table.GetValue<string>(PersonColumns.NameColumn, _rowId);

        public void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class TestViewModel 
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal AccountBalance { get; set; }
    }

    class XceedTestViewModel
    {
        private readonly List<TestViewModel> _objects;

        public XceedTestViewModel(IAccountBalanceDataService dataService)
        {
            Accounts = dataService.AccountPeople;

            _objects = new List<TestViewModel>();
            for (var i = 0; i < 200; i++)
            {
                _objects.Add(new TestViewModel {Id = i, Name = "test", AccountBalance = 1000*i});
            }

            ViewSource = new DataGridVirtualizingCollectionView(typeof(AccountPeopleSelector), false, 50, 1000);
            ViewSource.QueryItems += ViewSourceOnQueryItems;
            ViewSource.QueryItemCount += ViewSourceOnQueryItemCount;

            Accounts.Subscribe(update =>
                                   {
                                       if (update.IsRowUpdate()) ViewSource.Refresh();
                                   });
        }

        private IReactiveTable Accounts { get; }

        private void ViewSourceOnQueryItemCount(object sender, QueryItemCountEventArgs queryItemCountEventArgs)
        {
            queryItemCountEventArgs.Count = Accounts.RowCount;
        }

        private void ViewSourceOnQueryItems(object sender, QueryItemsEventArgs queryItemsEventArgs)
        {
            var startIndex = queryItemsEventArgs.AsyncQueryInfo.StartIndex;
            var requestedItemCount = queryItemsEventArgs.AsyncQueryInfo.RequestedItemCount;
            var updateObjects = new object[requestedItemCount];
            var index = 0;
            for (var i = startIndex; i < startIndex + requestedItemCount; i++)
            {
                var rowId = Accounts.GetRowAt(i);
                var selector = new AccountPeopleSelector(Accounts, rowId);
                updateObjects[index++] = selector;
            }
            queryItemsEventArgs.AsyncQueryInfo.EndQuery(updateObjects);
        }

        public DataGridVirtualizingCollectionView ViewSource { get; set; }
    }
}