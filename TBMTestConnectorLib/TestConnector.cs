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
using Newtonsoft.Json.Linq;
using WebSocketSharp;

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
                
        private WebSocket _webSocket;
        private readonly Dictionary<string, int> _channelIds = new Dictionary<string, int>();
        private readonly Dictionary<int, string> _tradePairs = new Dictionary<int, string>();

        public async Task ConnectAsync()
        {
            _webSocket = new WebSocket("wss://api-pub.bitfinex.com/ws/2");
            _webSocket.OnMessage += (sender, e) =>
            {
                ProcessMessage(e.Data);
            };
            _webSocket.Connect();
        }        
 
        public void SubscribeTrades(string pair, int maxCount = 100)
        {
            var msg = new
            {
                @event = "subscribe",
                channel = "trades",
                symbol = $"t{pair}"
            };

            SendMessage(msg);
        }        

        public void UnsubscribeTrades(string pair)
        {
            var key = $"trades:{pair}";
            var channelId = _channelIds[key];
            var msg = new
            {
                @event = "unsubscribe",
                chanId = channelId
            };

            SendMessage(msg);
        }

        public void SubscribeCandles(string pair, int periodInSec, DateTimeOffset? from = null, DateTimeOffset? to = null, long? count = 0)
        {
            throw new NotImplementedException();
        }

        public void UnsubscribeCandles(string pair)
        {
            throw new NotImplementedException();
        }

        private void ProcessMessage(string message)
        {
            var json = JToken.Parse(message);

            if (json.Type == JTokenType.Array)
            {
                HandleDataMessage(json);
            }
            else if (json.Type == JTokenType.Object)
            {
                HandleEventMessage(json);
            }
        }

        private void HandleEventMessage(JToken json)
        {
            var eventType = json["event"].ToString();

            if (eventType == "subscribed")
            {
                var channelId = (int)json["chanId"];
                var channelKey = json["channel"].ToString();

                _channelIds[channelKey] = channelId;

                if (json["channel"]?.ToString() == "trades")
                {
                    _tradePairs[channelId] = json["pair"].ToString();
                }
            }
        }

        private void HandleDataMessage(JToken json)
        {
            var channelId = (int)json[0];

            foreach (var channelKey in _channelIds.Keys)
            {
                if (_channelIds[channelKey] == channelId && json[1].ToString() != "hb")
                {
                    if (channelKey.StartsWith("trade"))
                    {
                        if (json[1].Type == JTokenType.Array)
                        {
                            HandleTrades(channelId, json[1]);
                        }
                        else
                        {
                            HandleTradeMessage(channelId, json[2]);
                        }
                    }
                    break;
                }
            }
        }

        private void HandleTrades(int channelId, JToken trades)
        {
            foreach (var trade in trades)
            {
                HandleTradeMessage(channelId, trade);
            }
        }

        private void HandleTradeMessage(int channelId, JToken data)
        {
            var trade = new Trade
            {
                Id = data[0].ToString(),
                Time = DateTimeOffset.FromUnixTimeMilliseconds((long)data[1]),
                Side = (decimal)data[2] >= 0 ? "buy" : "sell",
                Amount = (decimal)data[2],
                Price = (decimal)data[3],
                Pair = _tradePairs[channelId],
            };

            if (trade.Side == "buy")
            {
                NewBuyTrade?.Invoke(trade);
            }
            else
            {
                NewSellTrade?.Invoke(trade);
            }
        }

        private void SendMessage(object message)
        {
            var json = JsonConvert.SerializeObject(message);
            _webSocket.Send(json);
        }
    }
}
