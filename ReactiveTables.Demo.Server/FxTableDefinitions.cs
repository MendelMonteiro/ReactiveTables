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

            public static readonly Dictionary<string, int> ColumnsToFieldIds = new Dictionary<string, int>
                                                                          {
                                                                              {Id, 101},
                                                                              {CcyPair, 102},
                                                                              {Ccy1, 103},
                                                                              {Ccy2, 104},
                                                                          };
        }

        public static class FxRates
        {
            public static readonly string CcyPairId = "Fx.CcyPairId";
            public static readonly string Bid = "Fx.Bid";
            public static readonly string Ask = "Fx.Ask";
            public static readonly string Open = "Fx.Open";
            public static readonly string Close = "Fx.Close";
            public static readonly string Change = "Fx.Change";
            public static readonly string YearRangeStart = "Fx.YearRangeStart";
            public static readonly string YearRangeEnd = "Fx.YearRangeEnd";
            public static readonly string Time = "Fx.Time";
            public static readonly string Ticks = "Fx.Ticks";

            public static readonly Dictionary<string, int> ColumnsToFieldIds =
                new Dictionary<string, int>
                    {
                        {CcyPairId, 101},
                        {Bid, 102},
                        {Ask, 103},
                        {Open, 104},
                        {Close, 105},
                        {YearRangeStart, 106},
                        {YearRangeEnd, 107},
                        {Change, 108},
                        {Time, 109},
                        {Ticks, 110},
                    };
        }
    }
}