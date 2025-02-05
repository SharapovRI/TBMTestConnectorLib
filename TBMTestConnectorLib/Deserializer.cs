using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBMTestConnectorLib.Models;

namespace TBMTestConnectorLib
{
    internal static class Deserializer
    {
        internal static IEnumerable<Trade> DeserializeRestTrades(string pair, string response)
        {
            List<List<decimal>> listOfTrades = [];

            try
            {
                listOfTrades = JsonConvert.DeserializeObject<List<List<decimal>>>(response) ?? [];
            }
            catch (JsonException ex)
            {
                throw new FormatException("Invalid JSON format.", ex);
            }

            List<Trade> trades = new List<Trade>();
            foreach (var trade in listOfTrades)
            {
                try
                {
                    trades.Add(new Trade
                    {
                        Id = trade[0].ToString(),
                        Time = new DateTimeOffset(DateTime.UnixEpoch, TimeSpan.Zero).Add(TimeSpan.FromMilliseconds(Decimal.ToDouble(trade[1]))),
                        Side = trade[2] >= 0 ? "buy" : "sell",
                        Amount = Math.Abs(trade[2]),
                        Price = trade[3],
                        Pair = pair,
                    });
                }
                catch { }
            }

            return trades;
        }

        internal static IEnumerable<Candle> DeserializeRestCandles(string pair, string response, string timeFrame)
        {
            List<List<decimal>> listOfCandles = [];

            try
            {
                listOfCandles = JsonConvert.DeserializeObject<List<List<decimal>>>(response) ?? [];
            }
            catch (JsonException ex)
            {
                throw new FormatException("Invalid JSON format.", ex);
            }

            List<Candle> candles = new List<Candle>();
            foreach (var candle in listOfCandles)
            {
                try
                {
                    candles.Add(new Candle
                    {
                        OpenTime = new DateTimeOffset(DateTime.UnixEpoch, TimeSpan.Zero).Add(TimeSpan.FromMilliseconds(Decimal.ToDouble(candle[0]))),
                        OpenPrice = candle[1],
                        ClosePrice = candle[2],
                        HighPrice = candle[3],
                        LowPrice = candle[4],
                        TotalVolume = candle[5],
                        Pair = pair,
                        Period = Helper.TimeFrameToSeconds(timeFrame),
                    });
                }
                catch { }
            }

            return candles;
        }

        internal static Ticker DeserializeRestTicker(string pair, string response)
        {
            List<decimal> tickerInfo = [];

            try
            {
                tickerInfo = JsonConvert.DeserializeObject<List<decimal>>(response) ?? [];
            }
            catch (JsonException ex)
            {
                throw new FormatException("Invalid JSON format.", ex);
            }

            Ticker ticker = new Ticker()
            {
                Bid = tickerInfo[0],
                BidSize = tickerInfo[1],
                Ask = tickerInfo[2],
                AskSize = tickerInfo[3],
                DailyChange = tickerInfo[4],
                DailyChangeRelative = tickerInfo[5],
                LastPrice = tickerInfo[6],
                Volume = tickerInfo[7],
                High = tickerInfo[8],
                Low = tickerInfo[9],
                Pair = pair,
            };

            return ticker;
        }
    }
}
