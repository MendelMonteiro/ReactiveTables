namespace ReactiveTables.Demo.Server
{
    public static class FxTableDefinitions
    {
        public static class CurrencyPair
        {
            public static readonly string Id = "Ccy.Id";
            public static readonly string Ccy1 = "Ccy.Ccy1";
            public static readonly string Ccy2 = "Ccy.Ccy2";
            public static string CcyPair = "Ccy.CcyPairId";
        }

        public static class FxRates
        {
            public static readonly string CcyPairId = "Fx.CcyPairId";
            public static readonly string Bid = "Fx.Bid";
            public static readonly string Ask = "Fx.Ask";
            public static readonly string Time = "Fx.Time";
        }
    }
}