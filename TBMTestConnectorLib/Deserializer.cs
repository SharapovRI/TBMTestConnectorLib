using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBMTestConnectorLib.Models;

namespace TBMTestConnectorLib
{
    public static class Deserializer
    {
        public static IEnumerable<Trade> DeserializeRestTrades(string pair, string response)
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

        public static IEnumerable<Candle> DeserializeRestCandles(string pair, string response)
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
                    });
                }
                catch { }
            }

            return candles;
        }
    }
}
