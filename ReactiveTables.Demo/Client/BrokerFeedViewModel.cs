using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using ReactiveTables.Demo.Server;
using ReactiveTables.Demo.Services;
using ReactiveTables.Demo.Syncfusion;
using ReactiveTables.Framework.Sorting;

namespace ReactiveTables.Demo.Client
{
    public class BrokerFeedViewModel : SyncfusionViewModelBase
    {
        private readonly IBrokerFeedDataService _dataService;
        private readonly SortedTable _sortedFeeds;
        private readonly Dictionary<string, Type> _columnTypes;

        public BrokerFeedViewModel(IBrokerFeedDataService dataService)
        {
            _dataService = dataService;
            var feedsTable = dataService.Feeds;
//            _sortedFeeds = new SortedTable(feedsTable);
//            _sortedFeeds.SortBy(BrokerFeedTableDefinition.BrokerColumns.BrokerNameColumn, Comparer<string>.Default);
//            _sortedFeeds = new SortedTable(_sortedFeeds);
//            _sortedFeeds.SortBy(BrokerFeedTableDefinition.BrokerColumns.MaturityColumn, Comparer<string>.Default);
            SetTable(feedsTable);

            _columnTypes = new Dictionary<string, Type>
                              {
                                  {BrokerFeedTableDefinition.BrokerColumns.CcyPairColumn, typeof(string)},
                                  {BrokerFeedTableDefinition.BrokerColumns.BidColumn, typeof(double)},
                                  {BrokerFeedTableDefinition.BrokerColumns.AskColumn, typeof(double)},
                                  {BrokerFeedTableDefinition.BrokerColumns.BrokerNameColumn, typeof(string)},
                                  {BrokerFeedTableDefinition.BrokerColumns.MaturityColumn, typeof(string)},
                              };
            Columns = new ObservableCollection<string>(_columnTypes.Keys);

            dataService.Start(Application.Current.Dispatcher);

        }

        public ObservableCollection<string> Columns { get; private set; }

        protected override void DisposeCore()
        {
            base.DisposeCore();
            _dataService.Stop();
        }
    }
}
