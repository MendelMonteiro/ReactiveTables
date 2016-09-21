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

using Ninject;
using ReactiveTables.Demo.Client;
using ReactiveTables.Demo.GroupedData;
using ReactiveTables.Demo.Syncfusion;

namespace ReactiveTables.Demo.Services
{
    internal class ViewModelLocator
    {
        public IKernel Kernel { get; set; }

        public MainViewModel MainViewModel => Kernel.Get<MainViewModel>();

        public SyncfusionTestViewModel SyncfusionTestViewModel => Kernel.Get<SyncfusionTestViewModel>();

        public XceedTestViewModel XceedTestViewModel => Kernel.Get<XceedTestViewModel>();

        public FxClientViewModel FxClientViewModel => Kernel.Get<FxClientViewModel>();

        public FxClientSyncfusionViewModel FxSyncfusionClientViewModel => Kernel.Get<FxClientSyncfusionViewModel>();

        public BrokerFeedViewModel BrokerFeedViewModel => Kernel.Get<BrokerFeedViewModel>();

        public GroupTestViewModel GroupTestViewModel => Kernel.Get<GroupTestViewModel>();

        public GroupTestViewModelSyncfusion GroupTestSyncfusionViewModel => Kernel.Get<GroupTestViewModelSyncfusion>();
    }
}