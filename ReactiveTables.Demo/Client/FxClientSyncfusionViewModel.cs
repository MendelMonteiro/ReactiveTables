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

using System.Windows;
using ReactiveTables.Demo.Syncfusion;

namespace ReactiveTables.Demo.Client
{
    internal class FxClientSyncfusionViewModel : SyncfusionViewModelBase
    {
        private readonly FxDataService _dataService = new FxDataService();

        public FxClientSyncfusionViewModel()
        {
            var table = _dataService.FxRates;
            SetTable(table);
            _dataService.Start(Application.Current.Dispatcher);
        }
    }
}