using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Threading;
using TBMTestConnectorLib.Interfaces;
using TBMTestConnectorLib.Models;
using TestConnectorGUI.Models;

namespace TestConnectorGUI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ITestConnector _connector;
        private readonly Dispatcher _dispatcher;

        public MainViewModel(ITestConnector connector)
        {
            _connector = connector;
            _connector.ConnectAsync().Wait();
            _dispatcher = Dispatcher.CurrentDispatcher;

            Trades = new ObservableCollection<Trade>();
            Candles = new ObservableCollection<Candle>();

            TradesRest = new ObservableCollection<Trade>();
            CandlesRest = new ObservableCollection<Candle>();

            _balances = new ObservableCollection<PortfolioCurrencyBalance>();

            LoadTradesCommand = new RelayCommand(async () => await LoadTrades());
            LoadCandlesCommand = new RelayCommand(async () => await LoadCandles());
            SubscribeTradesCommand = new RelayCommand(() => SubscribeTrades());
            SubscribeCandlesCommand = new RelayCommand(() => SubscribeCandles());

            UnsubscribeTradesCommand = new RelayCommand(() => UnsubscribeTrades());
            UnsubscribeCandlesCommand = new RelayCommand(() => UnsubscribeCandles());

            LoadBalancesCommand = new RelayCommand(async () => await LoadBalancesAsync());

            _connector.NewBuyTrade += trade => _dispatcher.Invoke(() => Trades.Add(trade));
            _connector.NewSellTrade += trade => _dispatcher.Invoke(() => Trades.Add(trade));
            _connector.CandleSeriesProcessing += candle => _dispatcher.Invoke(() => Candles.Add(candle));
        }

        public ObservableCollection<Trade> Trades { get; }
        public ObservableCollection<Candle> Candles { get; }

        public ObservableCollection<Trade> TradesRest { get; }
        public ObservableCollection<Candle> CandlesRest { get; }

        private ObservableCollection<PortfolioCurrencyBalance> _balances;
        public ObservableCollection<PortfolioCurrencyBalance> Balances
        {
            get => _balances;
            set
            {
                _balances = value;
                OnPropertyChanged(nameof(Balances));
            }
        }

        public ICommand LoadTradesCommand { get; }
        public ICommand LoadCandlesCommand { get; }
        public ICommand SubscribeTradesCommand { get; }
        public ICommand SubscribeCandlesCommand { get; }
        public ICommand UnsubscribeTradesCommand { get; }
        public ICommand UnsubscribeCandlesCommand { get; }
        public ICommand LoadBalancesCommand { get; }

        private async Task LoadTrades()
        {
            var trades = await _connector.GetNewTradesAsync("BTCUSD", 10);
            TradesRest.Clear();
            foreach (var trade in trades)
            {
                TradesRest.Add(trade);
            }
        }

        private async Task LoadCandles()
        {
            var candles = await _connector.GetCandleSeriesAsync("BTCUSD", 60, DateTimeOffset.Now.AddHours(-1));
            CandlesRest.Clear();
            foreach (var candle in candles)
            {
                CandlesRest.Add(candle);
            }
        }

        private void SubscribeTrades()
        {
            _connector.SubscribeTrades("BTCUSD");
        }

        private void UnsubscribeTrades()
        {
            _connector.UnsubscribeTrades("BTCUSD");
        }

        private void SubscribeCandles()
        {
            _connector.SubscribeCandles("BTCUSD", 60);
        }

        private void UnsubscribeCandles()
        {
            _connector.UnsubscribeCandles("BTCUSD");
        }        

        private async Task LoadBalancesAsync()
        {
            var balances = await CalculateBalancesAsync();
            Balances = new ObservableCollection<PortfolioCurrencyBalance>(balances);
        }

        private async Task<List<PortfolioCurrencyBalance>> CalculateBalancesAsync()
        {
            var balances = new Dictionary<string, decimal>
            {
                { "BTC", 1 },
                { "XRP", 15000 },
                { "XMR", 50 },
                { "DASH", 30 }
            };

            var targetCurrencies = new List<string> { "USDT", "BTC", "XRP", "XMR", "DASH" };

            var pricesInUsd = new Dictionary<string, decimal>();

            foreach (var currency in balances.Keys)
            {
                var pair = GetCurrencyPair(currency, "USD");
                var ticker = await _connector.GetTickerInfoAsync(pair);
                pricesInUsd[currency] = ticker.LastPrice;
            }

            decimal totalBalanceInUsd = balances.Sum(b => b.Value * pricesInUsd[b.Key]);

            var portfolioBalances = new List<PortfolioCurrencyBalance>();

            foreach (var targetCurrency in targetCurrencies)
            {
                decimal balanceInTargetCurrency;

                if (pricesInUsd.ContainsKey(targetCurrency))
                {
                    balanceInTargetCurrency = totalBalanceInUsd / pricesInUsd[targetCurrency];
                }
                else 
                {
                    var pair = GetCurrencyPair(targetCurrency, "USD");
                    var ticker = await _connector.GetTickerInfoAsync(pair);
                    balanceInTargetCurrency = totalBalanceInUsd / ticker.LastPrice;
                }


                portfolioBalances.Add(new PortfolioCurrencyBalance
                {
                    Currency = targetCurrency,
                    Balance = balanceInTargetCurrency
                });
            }

            return portfolioBalances;
        }

        // Метод для получения правильного названия пары, потому что на Bitfinex названия некоторых монет и токенов не совпадают с тем, как они представлены в списке валют https://api-pub.bitfinex.com/v2/conf/pub:list:currency
        private string GetCurrencyPair(string currency, string targetCurrency)
        {
            var currencyMappings = new Dictionary<string, string>
            {
                { "USDT", "UST" },
                { "DASH", "DSH" }
            };

            var fromCurrency = currencyMappings.ContainsKey(currency) ? currencyMappings[currency] : currency;
            var toCurrency = currencyMappings.ContainsKey(targetCurrency) ? currencyMappings[targetCurrency] : targetCurrency;

            return $"{fromCurrency}{toCurrency}";
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}