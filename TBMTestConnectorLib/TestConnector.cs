using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TBMTestConnectorLib.Interfaces;
using TBMTestConnectorLib.Models;
using RestSharp;
using Newtonsoft.Json;

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

        public Task<IEnumerable<Candle>> GetCandleSeriesAsync(string pair, int periodInSec, DateTimeOffset? from, DateTimeOffset? to = null, long? count = 0)
        {
            throw new NotImplementedException();
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
