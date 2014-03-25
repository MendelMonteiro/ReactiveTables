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
using System.Collections.ObjectModel;
using System.Windows;
using ReactiveTables.Demo.Services;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Demo.Client
{
    internal class FxClientViewModel : ReactiveViewModelBase, IDisposable
    {
        private readonly IFxDataService _dataService;
        private readonly IDisposable _currenciesSubscription;
        private readonly IDisposable _fxRatesSubscription;

        public FxClientViewModel(IFxDataService dataService)
        {
            _dataService = dataService;
            CurrencyPairs = new ObservableCollection<FxCurrencyPairViewModel>();
            _currenciesSubscription = _dataService.Currencies.ReplayAndSubscribe(
                update =>
                    {
                        if (update.Action == TableUpdateAction.Add)
                        {
                            CurrencyPairs.Add(new FxCurrencyPairViewModel(_dataService.Currencies, update.RowIndex));
                        }
                    });

            FxRates = new ObservableCollection<FxRateViewModel>();
            _fxRatesSubscription = _dataService.FxRates.ReplayAndSubscribe(
                update =>
                    {
                        if (update.Action == TableUpdateAction.Add)
                        {
                            FxRates.Add(new FxRateViewModel(_dataService.FxRates, update.RowIndex));
                        }
                    });

            _dataService.Start(Application.Current.Dispatcher);
        }

        public ObservableCollection<FxRateViewModel> FxRates { get; set; }
        public ObservableCollection<FxCurrencyPairViewModel> CurrencyPairs { get; set; }

        public void Dispose()
        {
            _dataService.Stop();
            if (_currenciesSubscription != null) _currenciesSubscription.Dispose();
            if (_fxRatesSubscription != null) _fxRatesSubscription.Dispose();
            foreach (var fxRate in FxRates)
            {
                fxRate.Dispose();
            }
        }
    }
}