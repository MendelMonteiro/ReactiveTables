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

using System.Collections.Generic;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Demo.Server
{
    static class BrokerTableDefinition
    {
        public static class BrokerColumns
        {
            public const string CcyPairColumn = "Broker.CcyPairColumn";
            public const string BidColumn = "Broker.BidColumn";
            public const string AskColumn = "Broker.AskColumn";
            public const string MaturityColumn = "Broker.MaturityColumn";
            public const string BrokerNameColumn = "Broker.BrokerNameColumn";
        }

        public static class BrokerClientColumns
        {
            public const string ClientIpColumn = "BrokerClient.IpColumn";
            public const string ClientCcyPairColumn = "BrokerClient.CcyColumn";
        }

        public static readonly Dictionary<string, int> ColumnsToFieldIds =
                new Dictionary<string, int>
                    {
                        {BrokerColumns.CcyPairColumn, 101},
                        {BrokerColumns.BidColumn, 102},
                        {BrokerColumns.AskColumn, 103},
                        {BrokerColumns.BrokerNameColumn, 104},
                        {BrokerColumns.MaturityColumn, 105},
                    };

        public static readonly Dictionary<string, int> ClientColumnsToFieldIds =
                new Dictionary<string, int>
                    {
                        {BrokerClientColumns.ClientIpColumn, 201},
                        {BrokerClientColumns.ClientCcyPairColumn, 202},
                    };

        public static void SetupFeedTable(IReactiveTable feeds)
        {
            feeds.AddColumn(new ReactiveColumn<string>(BrokerColumns.CcyPairColumn));
            feeds.AddColumn(new ReactiveColumn<double>(BrokerColumns.BidColumn));
            feeds.AddColumn(new ReactiveColumn<double>(BrokerColumns.AskColumn));
            feeds.AddColumn(new ReactiveColumn<string>(BrokerColumns.MaturityColumn));
            feeds.AddColumn(new ReactiveColumn<string>(BrokerColumns.BrokerNameColumn));
        }

        public static void SetupClientFeedTable(IReactiveTable clients)
        {
            clients.AddColumn(new ReactiveColumn<string>(BrokerClientColumns.ClientIpColumn));
            clients.AddColumn(new ReactiveColumn<string>(BrokerClientColumns.ClientCcyPairColumn));
        }
    }
}
