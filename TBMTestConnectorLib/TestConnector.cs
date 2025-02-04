using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBMTestConnectorLib.Interfaces;
using TBMTestConnectorLib.Models;
using RestSharp;
using Newtonsoft.Json;
using System.Web;
using System.Collections.Specialized;
using System.Collections;

namespace TBMTestConnectorLib
{
    public class TestConnector : ITestConnector
    {
        public event Action<Trade> NewBuyTrade;
        public event Action<Trade> NewSellTrade;
        public event Action<Candle> CandleSeriesProcessing;


        public async Task<int> GetPlatformStatusAsync()
        {
            var options = new RestClientOptions("https://api-pub.bitfinex.com/v2/platform/status");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            var response = await client.GetAsync(request);

            if (string.IsNullOrEmpty(response.Content))
            {
                throw new InvalidOperationException("Response content is null or empty.");
            }

            try
            {
                int result = JsonConvert.DeserializeObject<int[]>(response.Content)[0];
                return result;
            }
            catch (JsonException ex)
            {
                throw new FormatException("Invalid JSON format.", ex);
            }
        }

        public async Task<IEnumerable<string>> GetTradingPairsAsync()
        {
            var options = new RestClientOptions("https://api-pub.bitfinex.com/v2/conf/pub:list:pair:exchange");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            var response = await client.GetAsync(request);

            if (string.IsNullOrEmpty(response.Content))
            {
                throw new InvalidOperationException("Response content is null or empty.");
            }

            try
            {
                IEnumerable<string> listOfPairs = JsonConvert.DeserializeObject<IEnumerable<IEnumerable<string>>>(response.Content).First();
                return listOfPairs;
            }
            catch (JsonException ex)
            {
                throw new FormatException("Invalid JSON format.", ex);
            }
        }

        #region REST


        public async Task<IEnumerable<Trade>> GetNewTradesAsync(string pair, int maxCount)
        {
            var options = new RestClientOptions($"https://api-pub.bitfinex.com/v2/trades/t{pair}/hist?limit={maxCount}&sort=-1");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            var response = await client.GetAsync(request);

            if (string.IsNullOrEmpty(response.Content))
            {
                throw new InvalidOperationException("Response content is null or empty.");
            }

            return Deserializer.DeserializeRestTrades(pair, response.Content);
        }

        public async Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
        {
            var timeFrame = Helper.SecondsToTimeFrame(periodInSec);
            string baseUrl = $"https://api-pub.bitfinex.com/v2/candles/trade:{timeFrame}:t{pair}/hist";
            var queryParams = new NameValueCollection();

            if (from.HasValue)
            {
                queryParams["start"] = from.Value.ToUnixTimeMilliseconds().ToString();
            }

            if (to.HasValue)
            {
                queryParams["end"] = to.Value.ToUnixTimeMilliseconds().ToString();
            }

            if (count.HasValue && count > 0)
            {
                queryParams["limit"] = count.ToString();
            }

            var uriBuilder = new UriBuilder(baseUrl) { Query = string.Join("&", queryParams.AllKeys.Select(key => $"{key}={queryParams[key]}")) };

            var options = new RestClientOptions(uriBuilder.ToString());
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            var response = await client.GetAsync(request);

            if (string.IsNullOrEmpty(response.Content))
            {
                throw new InvalidOperationException("Response content is null or empty.");
            }

            return Deserializer.DeserializeRestCandles(pair, response.Content);
        }

        public async Task<Ticker> GetTickerInfoAsync(string pair)
        {
            var options = new RestClientOptions($"https://api-pub.bitfinex.com/v2/ticker/t{pair}");
            var client = new RestClient(options);
            var request = new RestRequest("");
            request.AddHeader("accept", "application/json");
            var response = await client.GetAsync(request);

            if (string.IsNullOrEmpty(response.Content))
            {
                throw new InvalidOperationException("Response content is null or empty.");
            }

            return Deserializer.DeserializeRestTicker(pair, response.Content);
        }

        #endregion

        public void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
        {
            throw new NotImplementedException();
        }

        public void SubscribeTrades(string pair, int maxCount = 100)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeCandles(string pair)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeTrades(string pair)
        {
            throw new NotImplementedException();
        }

    }
}
