using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using TBMTestConnectorLib;
using TBMTestConnectorLib.Interfaces;
using TBMTestConnectorLib.Models;

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

            LoadTradesCommand = new RelayCommand(async () => await LoadTrades());
            LoadCandlesCommand = new RelayCommand(async () => await LoadCandles());
            SubscribeTradesCommand = new RelayCommand(() => SubscribeTrades());
            SubscribeCandlesCommand = new RelayCommand(() => SubscribeCandles());
            
            UnsubscribeTradesCommand = new RelayCommand(() => UnsubscribeTrades());
            UnsubscribeCandlesCommand = new RelayCommand(() => UnsubscribeCandles());

            _connector.NewBuyTrade += trade => _dispatcher.Invoke(() => Trades.Add(trade));
            _connector.NewSellTrade += trade => _dispatcher.Invoke(() => Trades.Add(trade));
            _connector.CandleSeriesProcessing += candle => _dispatcher.Invoke(() => Candles.Add(candle));
        }

        public ObservableCollection<Trade> Trades { get; }
        public ObservableCollection<Candle> Candles { get; }

        public ObservableCollection<Trade> TradesRest { get; }
        public ObservableCollection<Candle> CandlesRest { get; }

        public ICommand LoadTradesCommand { get; }
        public ICommand LoadCandlesCommand { get; }
        public ICommand SubscribeTradesCommand { get; }
        public ICommand SubscribeCandlesCommand { get; }
        public ICommand UnsubscribeTradesCommand { get; }
        public ICommand UnsubscribeCandlesCommand { get; }

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

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}