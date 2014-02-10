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
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using ReactiveTables.Demo.Server;
using ReactiveTables.Demo.Services;
using ReactiveTables.Demo.Syncfusion;
using ReactiveTables.Demo.Utils;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Sorting;

namespace ReactiveTables.Demo.Client
{
    public class BrokerFeedCurrencyPairsViewModel : SyncfusionViewModelBase
    {
        private readonly IBrokerFeedDataService _dataService;

        public BrokerFeedCurrencyPairsViewModel(IBrokerFeedDataService dataService)
        {
            _dataService = dataService;

            SetTable(_dataService.CurrencyPairs);
        }
    }

    public class BrokerFeedViewModel : SyncfusionViewModelBase
    {
        private readonly IBrokerFeedDataService _dataService;
        private readonly SortedTable _sortedFeeds;
        private readonly Dictionary<string, Type> _columnTypes;
        private readonly BrokerFeedCurrencyPairsViewModel _currencyPairsViewModel;

        public BrokerFeedViewModel(IBrokerFeedDataService dataService)
        {
            _dataService = dataService;

            var feedsTable = dataService.Feeds;
//            _sortedFeeds = new SortedTable(feedsTable);
//            _sortedFeeds.SortBy(BrokerTableDefinition.BrokerColumns.BrokerNameColumn, Comparer<string>.Default);
//            _sortedFeeds = new SortedTable(_sortedFeeds);
//            _sortedFeeds.SortBy(BrokerTableDefinition.BrokerColumns.MaturityColumn, Comparer<string>.Default);
            SetTable(feedsTable);

            _columnTypes = new Dictionary<string, Type>
                              {
                                  {BrokerTableDefinition.BrokerColumns.CcyPairColumn, typeof(string)},
                                  {BrokerTableDefinition.BrokerColumns.BidColumn, typeof(double)},
                                  {BrokerTableDefinition.BrokerColumns.AskColumn, typeof(double)},
                                  {BrokerTableDefinition.BrokerColumns.BrokerNameColumn, typeof(string)},
                                  {BrokerTableDefinition.BrokerColumns.MaturityColumn, typeof(string)},
                              };
            Columns = new ObservableCollection<string>(_columnTypes.Keys);

            _currencyPairsViewModel = new BrokerFeedCurrencyPairsViewModel(_dataService);

            AddCcyCommand = new DelegateCommand(
                () =>
                    {
                        var currencyPairsWire = (IWritableReactiveTable) _dataService.CurrencyPairs;
                        var row = currencyPairsWire.AddRow();
                        currencyPairsWire.SetValue(BrokerTableDefinition.BrokerClientColumns.ClientIpColumn, row, IPAddress.Loopback.ToString());
                        currencyPairsWire.SetValue(BrokerTableDefinition.BrokerClientColumns.ClientCcyPairColumn, row, "EUR/USD");
                    });

            dataService.Start(Application.Current.Dispatcher);
        }

        public BrokerFeedCurrencyPairsViewModel CurrencyPairsViewModel
        {
            get { return _currencyPairsViewModel; }
        }

        public DelegateCommand AddCcyCommand { get; private set; }

        public ObservableCollection<string> Columns { get; private set; }

        protected override void DisposeCore()
        {
            base.DisposeCore();
            _currencyPairsViewModel.Dispose();
            _dataService.Stop();
        }
    }
}
