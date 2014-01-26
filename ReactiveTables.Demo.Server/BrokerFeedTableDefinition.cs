using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveTables.Framework;
using ReactiveTables.Framework.Columns;

namespace ReactiveTables.Demo.Server
{
    static class BrokerFeedTableDefinition
    {
        public static class BrokerColumns
        {
            public const string CcyPairColumn = "Broker.CcyPairColumn";
            public const string BidColumn = "Broker.BidColumn";
            public const string AskColumn = "Broker.AskColumn";
            public const string MaturityColumn = "Broker.MaturityColumn";
            public const string BrokerNameColumn = "Broker.BrokerNameColumn";
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

        public static void SetupFeedTable(IReactiveTable feeds)
        {
            feeds.AddColumn(new ReactiveColumn<string>(BrokerColumns.CcyPairColumn));
            feeds.AddColumn(new ReactiveColumn<double>(BrokerColumns.BidColumn));
            feeds.AddColumn(new ReactiveColumn<double>(BrokerColumns.AskColumn));
            feeds.AddColumn(new ReactiveColumn<string>(BrokerColumns.MaturityColumn));
            feeds.AddColumn(new ReactiveColumn<string>(BrokerColumns.BrokerNameColumn));
        }
    }
}
