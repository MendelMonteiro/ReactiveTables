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
using ReactiveTables.Demo.Server;

namespace ReactiveTables.Demo.Client
{
    /// <summary>
    /// Interaction logic for BrokerFeed.xaml
    /// </summary>
    public partial class BrokerFeed
    {
        public BrokerFeed()
        {
            InitializeComponent();

            Grid.AddColumn<string>(BrokerTableDefinition.BrokerColumns.CcyPairColumn);
            Grid.AddColumn<double>(BrokerTableDefinition.BrokerColumns.BidColumn);
            Grid.AddColumn<double>(BrokerTableDefinition.BrokerColumns.AskColumn);
            Grid.AddColumn<string>(BrokerTableDefinition.BrokerColumns.BrokerNameColumn);
            Grid.AddColumn<string>(BrokerTableDefinition.BrokerColumns.MaturityColumn);

            CurrencyPairsGrid.AddColumn<string>(BrokerTableDefinition.BrokerClientColumns.ClientCcyPairColumn);
            CurrencyPairsGrid.AddColumn<string>(BrokerTableDefinition.BrokerClientColumns.ClientIpColumn);
        }
    }
}
