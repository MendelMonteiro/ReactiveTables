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

using System.Reflection;
using System.Windows;
using Ninject;
using ReactiveTables.Demo.GroupedData;
using ReactiveTables.Demo.Services;
using ReactiveTables.Framework.Utils;
using log4net;

namespace ReactiveTables.Demo
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private IKernel _kernel;

        protected override void OnStartup(StartupEventArgs e)
        {
//            Xceed.Wpf.DataGrid.Licenser.LicenseKey = "DGP45-L7AAA-RUWWA-5BBA";
            log4net.Config.XmlConfigurator.Configure();
            Exit += (sender, args) => _log.Debug(ProcessInfoDumper.GetProcessInfo());

            _kernel = new StandardKernel();
            _kernel.Bind<IAccountBalanceDataService>()
                  .To<AccountBalanceDataService>()
                  .WithConstructorArgument("dispatcher", Dispatcher)
                  .WithConstructorArgument("maxEntries", 1024);

            _kernel.Bind<MainViewModel>().ToSelf();
            _kernel.Bind<IFxDataService>().To<FxDataService>();
            _kernel.Bind<IBrokerFeedDataService>().To<BrokerFeedDataService>();
            _kernel.Bind<GroupTestViewModel>().ToSelf();
            _kernel.Bind<GroupTestViewModelSyncfusion>().ToSelf();
            
            var locator = (ViewModelLocator) Resources["ViewModelLocator"];
            locator.Kernel = _kernel;
        }

        public App()
        {           
        }

        public IKernel Kernel => _kernel;
    }
}