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
using System.Linq;
using System.Windows;
using ReactiveTables.Demo.Server;
using ReactiveTables.Demo.Services;
using ReactiveTables.Demo.Syncfusion;
using ReactiveTables.Framework.Sorting;

namespace ReactiveTables.Demo.Client
{
    public class FxClientSyncfusionViewModel : SyncfusionViewModelBase
    {
        private readonly IFxDataService _dataService;
        private string _sortByColumn;
        private readonly SortedTable _sortedRates;
        private readonly Dictionary<string, Type> _columnTypes;

        public FxClientSyncfusionViewModel(IFxDataService dataService)
        {
            _dataService = dataService;
            var ratesTable = dataService.FxRates;
            _sortedRates = new SortedTable(ratesTable);
            _sortedRates.SortBy(FxTableDefinitions.FxRates.CcyPairId, Comparer<string>.Default);
            SetTable(_sortedRates);

            _columnTypes = new Dictionary<string, Type>
                              {
                                  {FxTableDefinitions.FxRates.CcyPairId, typeof(string)},
                                  {FxTableDefinitions.FxRates.Bid, typeof(double)},
                                  {FxTableDefinitions.FxRates.Ask, typeof(double)},
                                  {FxTableDefinitions.FxRates.Open, typeof(double)},
                                  {FxTableDefinitions.FxRates.Close, typeof(double)},
                                  {FxTableDefinitions.FxRates.Change, typeof(double)},
                                  {FxTableDefinitions.FxRates.YearRangeStart, typeof(double)},
                                  {FxTableDefinitions.FxRates.YearRangeEnd, typeof(double)},
                                  {FxTableDefinitions.FxRates.Time, typeof(DateTime)},
                                  {FxTableDefinitions.FxRates.Ticks, typeof(double)},
                              };
            Columns = new ObservableCollection<string>(_columnTypes.Keys);

            dataService.Start(Application.Current.Dispatcher);
        }

        protected override void DisposeCore()
        {
            base.DisposeCore();
            _dataService.Stop();
        }

        public ObservableCollection<string> Columns { get; private set; }

        public string SortByColumn
        {
            get { return _sortByColumn; }
            set
            {
                if (_sortByColumn != value)
                {
                    _sortByColumn = value;

                    try
                    {
                        var method = typeof(SortedTable).GetMethods().First(info => info.Name == "SortBy");
                        var generic = method.MakeGenericMethod(_columnTypes[_sortByColumn]);
                        generic.Invoke(_sortedRates, new object[] { _sortByColumn });
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }
        }
    }
}