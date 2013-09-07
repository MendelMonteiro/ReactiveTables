using System;
using System.Collections.ObjectModel;
using System.Windows;
using ReactiveTables.Framework;
using ReactiveTables.Framework.UI;

namespace ReactiveTables.Demo.Client
{
    class FxClientViewModel : ReactiveViewModelBase, IDisposable
    {
        private readonly FxDataService _dataService = new FxDataService();
        private readonly IDisposable _currenciesSubscription;
        private readonly IDisposable _fxRatesSubscription;

        public FxClientViewModel()
        {
            CurrencyPairs = new ObservableCollection<FxCurrencyPairViewModel>();
            _currenciesSubscription = _dataService.Currencies.ReplayAndSubscribe(update =>
            {
                if (update.Action == TableUpdate.TableUpdateAction.Add)
                {
                    CurrencyPairs.Add(new FxCurrencyPairViewModel(_dataService.Currencies, update.RowIndex));
                }
            });

            FxRates = new ObservableCollection<FxRateViewModel>();
            _fxRatesSubscription = _dataService.FxRates.ReplayAndSubscribe(update =>
            {
                if (update.Action == TableUpdate.TableUpdateAction.Add)
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
