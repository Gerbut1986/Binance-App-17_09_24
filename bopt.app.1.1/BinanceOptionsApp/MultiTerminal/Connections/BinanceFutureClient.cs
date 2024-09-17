namespace MultiTerminal.Connections
{
    using BinanceOptionsApp.MultiTerminal.Connections.API.Future;
    using MultiTerminal.Connections.Details.Binance;
    using MultiTerminal.Connections.API.Future;
    using MultiTerminal.Connections.Models;
    using Environment = System.Environment;
    using System.Collections.Concurrent;
    using System.Security.Cryptography;
    using System.Collections.Generic;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using System.Globalization;
    using System.Diagnostics;
    using System.Windows.Threading;
    using Newtonsoft.Json.Linq;
    using global::Models.Algo;
    using Helpers.Extensions;
    using BinanceOptionsApp;
    using BinanceOptionApp;
    using System.Threading;
    using System.Net.Http;
    using Newtonsoft.Json;
    using WebSocket4Net;
    using System.Text;
    using System.Linq;
    using System.Net;
    using System;
    using System.IO;
    using WebSocketSharp.Server;
    using WebSocketSharp;


    public class BinanceFutureClient : BaseCryptoClient
    {
        private List<long> OrderTickets = new List<long>();
        public List<BinanceNewOrderF> PlacedOrdersUsd_M { get; set; } = new List<BinanceNewOrderF>();
        public List<COIN_M_NewOrder> PlacedOrdersCoin_M { get; set; } = new List<COIN_M_NewOrder>();
        public List<Asset> FutureBalances = default;
        public AccountInfoFuture AccountInfo = null, AccInfo_UsdUsd_M = null;
        public AccountInfo_Coin_M_Future Coin_MAccInfo = null;
        DateTime lastBalanceRequestTime = DateTime.MinValue;
        private decimal lastBalance = 0;
        private int ReconectCount = 0;
        private Timer timerResetReconectCount;
        private HttpClient _httpClient;
        private readonly long recvWindow = 5000;
        private long dTime = 0; // разница между временем сервера и клиента
        private BinancePingPongManager pingPongManager;
        public bool is1stLeg = false;
        private string symbol;
        bool Step_St =false, Step_Lt=false;
        //int rememberFonStepUP = 0, rememberFonStepDN = 0;
        private readonly Dictionary<string, WebSocketData> SubscribeList = new Dictionary<string, WebSocketData>();

        private DispatcherTimer timerEvent; // TIMER ***
                                            // object lockObject = new object(); // створення об'єкта блокування


        #region [onTick() TIMER FOR LEVEL_2 STREAM SYNCHRONIZATION]:
        long period = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        public bool uvaga = false, relax = false;

        private async void OnTick(object sender, EventArgs e)
        {

            if (uvaga)
            {
                if (symbol != null) SubscribeBookTickerStream(symbol);

            }

            if (relax)
            {
                isCloseBTCustom = true;
            }
            //isCloseBTCustom = relax ? true : false;

            await Task.Run(() =>
            {

                lock (lockObject)
                {
                  if (TasRawF != null && TasRawF.Count != 0)
                  {
                            TasCopyF = TasRawF;//var ggh = TasCopyF.Last();//var btDict = BookTickerFutureDictionary;
                            //--------------------------------------------------------
                            //  ask_f = AskPrice; bid_f = BidPrice;

                            try
                            {
                                if (TasCopyF != null && TasCopyF.Any())
                                {
                                    var tasUpdate1 = TasCopyF.LastOrDefault();

                                    if (tasUpdate1 != null && tasUpdate1.data.Symbol=="BTCUSDT")
                                    {
                                        if (ask_f != PrevAsk || bid_f != PrevBid || tasUpdate1 != PrevTAS) //  
                                        {
                                            decimal p = 0; decimal v = 0; decimal ask = ask_f, bid = bid_f; bool isMMbuyer = false;

                                            if (tasUpdate1 == PrevTAS)//e.Message 
                                            {
                                                tasUpdate1.data.Volume = 0.0m;
                                                tasUpdate1.data.Price = 0.0m;
                                                //tasUpdate.data.EventTime = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                                                tasUpdate1.data.LastTradeId = 0;
                                                tasUpdate1.data.FirstTradeId = 0;
                                                //tasUpdate.data.TradeTime = tasUpdate.data.TradeTime; // Pre time
                                                tasUpdate1.data.AggTradeId = 0;

                                                long timeSynchronization = (long)tasUpdate1.data.EventTime;

                                                tasUpdate1.data.Ask = QuoteAsk;
                                                tasUpdate1.data.Bid = QuoteBid;
                                                var dt = long.Parse(tasUpdate1.data.EventTime.ToString()).GetFullTime();
                                                tasUpdate1.data.EventDate = $"{dt.Day}/{dt.Month} {dt.ToString("HH:mm:ss.ffffff")}";
                                                tasUpdate1.data.Id = (int)(++counter);
                                                tasUpdate1.data.MarketBuy = 0;
                                                tasUpdate1.data.MarketSell = 0;

                                                TasF.Add(tasUpdate1);

                                            }
                                            else
                                            {
                                                v = tasUpdate1.data.Volume;
                                                p = tasUpdate1.data.Price;
                                                isMMbuyer = tasUpdate1.data.IsMarketMaker;

                                                lock (lockObject)
                                                {
                                                    try
                                                    {
                                                        if (isMMbuyer) tasUpdate1.data.MarketSell = v;
                                                        else tasUpdate1.data.MarketBuy = v;

                                                        tasUpdate1.data.Ask = ask;
                                                        tasUpdate1.data.Bid = bid;
                                                        var dt = long.Parse(tasUpdate1.data.EventTime.ToString()).GetFullTime();
                                                        tasUpdate1.data.EventDate = $"{dt.Day}/{dt.Month} {dt.ToString("HH:mm:ss.ffffff")}";
                                                        tasUpdate1.data.Id = (int)(++counter);

                                                        TasF.Add(tasUpdate1);
                                                        zz.BuildZigZag(tasUpdate1.data.Ask, tasUpdate1.data.Bid,
                                                                               tasUpdate1.data.EventTime, (ulong)tasUpdate1.data.Id, TasF, TradeZigZag.GAPbuy_q, TradeZigZag.GAPsell_q,
                                                                                TradeZigZag.GAPbuy_r, TradeZigZag.GAPsell_r, marketDepthList, BookTickerFuture, AsksP);

                                                    }
                                                    catch (NullReferenceException)
                                                    {

                                                    }
                                                }
                                            }
                                            PrevAsk = ask_f;
                                            PrevBid = bid_f;
                                            PrevTAS = tasUpdate1;
                                        }
                                    }
                                }
                            }
                            catch (Exception ex) { }

                            //-------------------------------------
                        }
                }

                
            });

            string symb = TradeZigZag.Symbol; //
            string FileName = "TradeLevelPlot" + symb + ".txt";
            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&
            if (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - period >= 600) //evry 10 min
            {
                ClusterTradePlot.Clear(); // nulling

                if (TradeLevelPlot.Any())
                {
                    // Знаходження ключа для найбільшого об'єму MarketBuy
                    decimal maxBuyKey = TradeLevelPlot.OrderByDescending(kv => kv.Value.MarketBuy).First().Key;
                    // Знаходження ключа для найбільшого об'єму MarketSell
                    decimal maxSellKey = TradeLevelPlot.OrderByDescending(kv => kv.Value.MarketSell).First().Key;
                    // Отримання значення об'єму для maxBuyKey та maxSellKey
                    decimal maxBuyVolume = TradeLevelPlot[maxBuyKey].MarketBuy;
                    decimal maxSellVolume = TradeLevelPlot[maxSellKey].MarketSell;

                    period = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    string formattedTime = DateTimeOffset.UtcNow.ToString("HH:mm:ss");

                    TradeZigZag.PriceMaxBuyGross = maxBuyKey;
                    TradeZigZag.VolMaxBuyGross = maxBuyVolume;
                    TradeZigZag.PriceMaxSellGross = maxSellKey;
                    TradeZigZag.VolMaxSellGross = maxSellVolume;

                    WriteMessageToDesktopFile($"Time: { formattedTime} |  | Price max buy: {maxBuyKey }, vol = {maxBuyVolume}|  | " +
                    $"Price max sell: {maxSellKey}, vol = {maxSellVolume}", FileName);
                    //-----------------------------------------------------------------+
                    // -------------------  CLUSTER -----------------------------------+

                    decimal sensitiveZone = TradeZigZag.ClusterTS; //   ширина кластера  
                    decimal startCluster = 0, endCluster = 0;

                    decimal minPrice = TradeLevelPlot.Keys.Min();
                    decimal maxPrice = TradeLevelPlot.Keys.Max();
                    // Розділ всі ціни на кластери

                    if (startCluster == 0) startCluster = minPrice;
                    endCluster = startCluster + sensitiveZone;

                    try
                    {
                        var tradeLevelPlotCopy = TradeLevelPlot.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                        while (startCluster > 0 && startCluster < maxPrice)
                        {
                            var ClusterPlot = tradeLevelPlotCopy
                              .Where(kvp => kvp.Key > startCluster && kvp.Key <= endCluster)
                              .GroupBy(kvp => kvp.Key)
                              .Select(group => group.First())
                              .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

                            if (ClusterPlot.Any())
                            {
                                // Знаходження елемента з найбільшим значенням MarketBuy
                                // BUY
                                var itemWithMaxMarketBuy = ClusterPlot
                                 .OrderByDescending(kvp => kvp.Value.MarketBuy)
                                 .FirstOrDefault();

                                decimal maxMarketBuyValue = itemWithMaxMarketBuy.Value.MarketBuy;
                                decimal PricemaxMarketBuy = itemWithMaxMarketBuy.Value.Price;

                                Plot newPlotBuy = new Plot
                                {
                                    MarketBuy = maxMarketBuyValue,
                                    Price = PricemaxMarketBuy
                                };
                                // ---- наповнюю словник найвагомішими рівнями кластеру ----------

                                if (!ClusterTradePlot.ContainsKey(PricemaxMarketBuy))
                                {
                                    ClusterTradePlot.Add(PricemaxMarketBuy, newPlotBuy);
                                }
                                else
                                {
                                    ClusterTradePlot[PricemaxMarketBuy] = newPlotBuy; // для випадку, коли ключ вже існує
                                }

                                // SELL
                                // Знаходження елемента з найбільшим значенням MarketSell
                                var itemWithMaxMarketSell = ClusterPlot
                                 .OrderByDescending(kvp => kvp.Value.MarketSell)
                                 .FirstOrDefault();

                                decimal maxMarketSellValue = itemWithMaxMarketSell.Value.MarketSell;
                                decimal PricemaxMarketSell = itemWithMaxMarketSell.Value.Price;

                                Plot newPlotSell = new Plot
                                {
                                    MarketSell = maxMarketSellValue,
                                    Price = PricemaxMarketSell
                                };
                                // ---- наповнюю словник найвагомішими рівнями кластеру ----------

                                if (!ClusterTradePlot.ContainsKey(PricemaxMarketSell))
                                {
                                    ClusterTradePlot.Add(PricemaxMarketSell, newPlotSell);
                                }
                                else
                                {
                                    ClusterTradePlot[PricemaxMarketSell] = newPlotSell; // Виконати логіку для випадку, коли ключ вже існує
                                }

                                // ---------------- Print -----------------------------------------
                                WriteMessageToDesktopFile($"Time: {formattedTime} | startCluster: {startCluster} - endCluster: {endCluster} | PriceMaxBuy: {PricemaxMarketBuy }, vol = {maxMarketBuyValue}|  | " +
                                $"PriceMaxSell: {PricemaxMarketSell}, vol = {maxMarketSellValue}", "ClusterPlot" + symbol + ".txt");


                            }

                            ClusterPlot.Clear();
                            startCluster = endCluster;
                            endCluster = startCluster + sensitiveZone;

                            if (startCluster < maxPrice && endCluster > maxPrice) endCluster = maxPrice;
                            else endCluster = startCluster + sensitiveZone;


                        }
                    }
                    catch (Exception ex) { }
                }
            }
            //&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&&

        }
        #endregion

        void WriteMessageToDesktopFile2(string message, string fileName)
        {
            try
            {
                // Отримуємо шлях до робочого столу користувача
                string desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

                // Формуємо повний шлях до файлу на робочому столі
                string filePath = Path.Combine(desktopPath, fileName);

                // Записуємо повідомлення у файл, додаючи його на новий рядок
                File.AppendAllText(filePath, $"{message}{Environment.NewLine}");

                // Console.WriteLine($"Повідомлення було успішно записано до файлу {fileName} на робочому столі.");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Виникла помилка: {ex.Message}");
            }
        }
        private void WriteMessageToDesktopFile(string message, string fileName)
        {
            try
            {
                // Отримуємо шлях до робочого столу користувача
                string desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop));

                // Формуємо повний шлях до файлу на робочому столі
                string filePath = Path.Combine(desktopPath, fileName);

                // Записуємо повідомлення у файл, додаючи його на новий рядок
                File.AppendAllText(filePath, $"{message}{Environment.NewLine}");

                // Console.WriteLine($"Повідомлення було успішно записано до файлу {fileName} на робочому столі.");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Виникла помилка: {ex.Message}");
            }
        }

        public BinanceFutureClient(IConnectorLogger logger, ManualResetEvent cancelToken, BinanceFutureConnectionModel model, bool is1stLeg = false) : base(logger, cancelToken, model)
        {
            pingPongManager = new BinancePingPongManager(logger);
            // Створення таймера
            timerEvent = new DispatcherTimer();
            timerEvent.Interval = TimeSpan.FromMilliseconds(10); // 10 мілісекунд

            timerEvent.Tick += OnTick; // Додавання події Tick

            timerEvent.Start(); // Запуск таймера


            Type obj = logger.GetType();
            switch (obj.Name)
            {
                case nameof(TwoLegArbFutureUSD_M):
                    (logger as TwoLegArbFutureUSD_M).fc = this;
                    break;
                case nameof(TradeScalper):
                    (logger as TradeScalper).bfc = this;
                    break;
                case nameof(TradeZigZag):
                    if (is1stLeg)
                    {
                        (logger as TradeZigZag).BfcLeg1 = this;
                        (logger as TradeZigZag).BfcLeg1.is1stLeg = is1stLeg;
                    }
                    else
                    {
                        (logger as TradeZigZag).BfcLeg2 = this;
                        (logger as TradeZigZag).BfcLeg2.is1stLeg = is1stLeg;
                    }
                    zz = new ZigZagFuture();
                    //(logger as TradeZigZag).zigZagFuture = new ZigZagFuture(this);
                    break;
            }
        }

        internal override decimal GetBalance()
        {
            lastBalance = GetBalance(BinanceOptionsApp.Models.TradeModel.fullSymbolFuture).Result;
            if ((DateTime.UtcNow - lastBalanceRequestTime).TotalSeconds >= 25)
            {
                //  lastBalance = GetBalance(BinanceOptionsApp.Models.TradeModel.fullSymbolFuture).Result;
                lastBalanceRequestTime = DateTime.UtcNow;
            }
            return lastBalance;
        }

        public async Task<decimal> GetBalance(string Asset)
        {
            decimal balance = 0;

            try
            {
                if (AccountTradeType == AccountTradeType.USD_M)
                {
                    AccountInfoFuture AIF = await CallAsync<AccountInfoFuture>(EndPoints.AccountInformationFuture, $"recvWindow={recvWindow}");
                    TradeZigZag.AccountInfoFuture = AIF;
                    AccountInfo = AIF;
                    AccInfo_UsdUsd_M = AIF;
                    FutureBalances = AIF.Assets;
                    //var ddd = AIF.
                    switch (Asset)
                    {
                        case "BTC":
                            balance = decimal.Parse(AIF.Assets.Single(b => b.asset == Asset).AvailableBalance, CultureInfo.InvariantCulture);
                            break;
                        default: //  if we pssed Full symbol
                            balance = decimal.Parse(AIF.AvailableBalance, CultureInfo.InvariantCulture);
                            //decimal.Parse(AIF.Positions.Single(b => b.Symbol == Asset).PositionAmt, CultureInfo.InvariantCulture);
                            break;
                    }
                }
                else if (AccountTradeType == AccountTradeType.COIN_M)
                {
                    AccountInfo_Coin_M_Future details = await CallAsync<AccountInfo_Coin_M_Future>(EndPoints.COIN_MAccountInformationF, $"recvWindow={recvWindow}");
                    Coin_MAccInfo = details;
                    //if (Asset == SubscribeList.First().Key) Asset = "BTC";
                    //var basset = details.Assets.FirstOrDefault(ua => ua.AssetName == "BTC").AvailableBalance;
                    //var bposAmt = details.Positions.FirstOrDefault(ua => ua.Symbol == "BTCUSD_240329").PositionAmt;
                    //var btcasset = details.Assets.FirstOrDefault(ua => ua.AssetName == Asset);
                    switch (Asset)
                    {
                        case "BTC":
                            balance = details.Assets.FirstOrDefault(ua => ua.AssetName == "BTC").AvailableBalance;
                            break;
                        default:
                            var found = details.Positions.FirstOrDefault(ua => ua.Symbol == "BTCUSD_240329");
                            balance = found.PositionAmt;
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                //logger.LogError(ViewId + $" Balance Error: {e.Message}");
            }

            return balance;
        }

        #region Order's actions:
        public override OrderCloseResult Close(string symbol, string orderId, decimal price, decimal volume, OrderSide side, int slippage, OrderType type, int lifetimeMs)
        {
            OrderSide closeSide = side == OrderSide.Buy ? OrderSide.Sell : OrderSide.Buy;
            var openResult = Open(symbol, price, volume, FillPolicy.FOK, closeSide, 0, slippage, 0, type, lifetimeMs);

            return new OrderCloseResult()
            {
                ClosePrice = openResult.OpenPrice,
                ExecutionTime = openResult.ExecutionTime,
                Error = openResult.Error
            };
        }
        public override bool OrderDelete(string symbol, string orderId, string origClientOrderId)
        {
            try
            {
                var args = $"symbol={symbol}&orderId={orderId}&origClientOrderId={origClientOrderId}&recvWindow={recvWindow}";

                if (AccountTradeType == AccountTradeType.USD_M)
                {
                    var DeleteResult = CallAsync<BinanceNewOrder>(EndPoints.DeleteOrderFuture, args).Result;

                    if (DeleteResult.status == "CANCELED")
                    {
                        return true;
                    }
                    else return false;
                }
                if (AccountTradeType == AccountTradeType.COIN_M)
                {

                }

                return false;
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return false;
            }
        }
        public override OrderModifyResult Modify(string symbol, string origClientOrderId, string orderId, OrderSide side, decimal newPrice, decimal lot)
        {
            try
            {
                bool isSuccess = false;
                string sPrice = newPrice.ToString(CultureInfo.InvariantCulture);
                string sAmount = lot.ToString(CultureInfo.InvariantCulture);
                string sSide = (side == OrderSide.Buy) ? "BUY" : "SELL";

                BinanceNewOrderF ModifyResult = null;
                var args = $"orderId={orderId}&origClientOrderId={origClientOrderId}&symbol={symbol.ToUpper()}" +
                       $"&side={sSide}&quantity={sAmount}&price={sPrice}&recvWindow={recvWindow}";

                DateTime start = DateTime.MinValue, end = DateTime.MinValue;

                if (AccountTradeType == AccountTradeType.COIN_M)
                {
                    BinanceNewOrder o = CallAsync<BinanceNewOrder>(EndPoints.MarginAccountOrderDelete, args).Result;

                    if (o.status == "FILLED")
                    {
                        string Id = o.orderId.ToString();
                        decimal OpenPrice = decimal.Parse(o.price, CultureInfo.InvariantCulture);
                        decimal Volume = decimal.Parse(o.executedQty, CultureInfo.InvariantCulture);
                    }
                    else
                    {
                        return new OrderModifyResult() { Error = $"Order status = {o.status}" };
                    }

                }
                if (AccountTradeType == AccountTradeType.USD_M)
                {
                    start = DateTime.UtcNow;
                    ModifyResult = CallAsync<BinanceNewOrderF>(EndPoints.OrderModifyFutureUsd_M, args).Result;
                    end = DateTime.UtcNow;

                    if (ModifyResult.Status == "NEW") isSuccess = true;
                }

                if (isSuccess)
                {
                    return new OrderModifyResult()
                    {
                        Lot = ModifyResult.OrigQty,
                        Side = ModifyResult.Side,
                        OpenPrice = ModifyResult.Price,
                        ExecutionTime = end - start
                    };
                }

                return new OrderModifyResult { Error = "Timeout error." };
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new OrderModifyResult { Error = e.Message };
            }
            //return new OrderModifyResult() { Error = "Not implemented" };
        }

        public OrderOpenResult BatchOrder(string symbol, OrderSide side, OrderType otype, decimal lot, decimal price)
        {
            try
            {
                DateTime begin = DateTime.UtcNow;
                OrderInformation order = null;

                string sPrice = price.ToString(CultureInfo.InvariantCulture);
                string sAmount = lot.ToString(CultureInfo.InvariantCulture);
                string sSide = (side == OrderSide.Buy) ? "BUY" : "SELL";

                //var args = $"symbol={symbol}&side={sSide}&type=LIMIT&timeInForce=FOK&quantity={sAmount}&price={sPrice}&recvWindow={recvWindow}";
                var bo = $"'symbol':'{symbol},'side':'{side},'type':'{otype}','timeInForce':'GTC','quantity':'{sAmount}','price':'{sPrice}'";
                bo = $"{{{bo}}}";
                //var args = $"batchOrders=[{bo}]";//&timestamp=1673295895556&signature= 
                var a = "batchOrders =[{ \"symbol\":\"1INCHUSDT\",\"side\":\"BUY\",\"type\":\"STOP\",\"quantity\":30,\"price\":9000,\"timeInForce\":\"GTC\"," +
                                  "\"stopPrice\":9100},{ \"symbol\":\"1INCHUSDT\",\"side\":\"BUY\",\"type\":\"LIMIT\",\"quantity\":30,\"price\":15,\"timeInForce\":\"GTC\"}]}";
                if (AccountTradeType == AccountTradeType.USD_M)
                {
                    BatchOrderFuture o = CallAsync<BatchOrderFuture>(EndPoints.BatchOrderFuture, a).Result;

                    if (o.Status == "FILLED")
                    {
                        //    string Id = o.OrderId.ToString();
                        //    decimal OpenPrice = o.Price;
                        //    decimal Volume = decimal.Parse(o.ExecutedQty, CultureInfo.InvariantCulture);

                        //    order = GetPosition(symbol);
                        //    if (order == null)
                        //    {
                        //        order = new OrderInformation
                        //        {
                        //            Id = symbol,
                        //            Symbol = symbol,
                        //            Side = side,
                        //            OpenTime = DateTime.UtcNow
                        //        };
                        //        AddPosition(order);
                        //    }
                        //    if (order.Side != side)
                        //    {
                        //        RemovePosition(order);
                        //    }
                        //    order.OpenPrice = OpenPrice;
                        //    order.Volume = Volume;
                    }
                    else
                    {
                        return new OrderOpenResult() { Error = $"Order status = {o.Status}" };
                    }
                }

                return new OrderOpenResult() { Error = "Timeout error." };
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new OrderOpenResult() { Error = e.Message };
            }
        }

        public override OrderOpenResult Open(string symbol, decimal price, decimal lot, FillPolicy policy, OrderSide side, int magic, int slippage, int track, OrderType type, int lifetimeMs)
        {
            try
            {
                DateTime begin = DateTime.UtcNow;
                OrderInformation order = null;

                string sPrice = price.ToString(CultureInfo.InvariantCulture);
                string sAmount = lot.ToString(CultureInfo.InvariantCulture);
                string sSide = (side == OrderSide.Buy) ? "BUY" : "SELL";

                var args = "";
                if (type == OrderType.Limit)
                {
                    args = $"symbol={symbol.ToUpper()}&side={sSide}&type=LIMIT&timeInForce={policy.ToString()}&quantity={sAmount}&price={sPrice}&recvWindow={recvWindow}";
                }
                else
                    args = $"symbol={symbol.ToUpper()}&side={sSide}&type=MARKET&quantity={sAmount}&recvWindow={recvWindow}";

                if (AccountTradeType == AccountTradeType.USD_M)
                {
                    BinanceNewOrderF o = CallAsync<BinanceNewOrderF>(EndPoints.NewOrderFuture, args).Result;

                    if (o.Status == "FILLED")
                    {
                        OrderTickets.Add(o.OrderId);
                        PlacedOrdersUsd_M.Add(o);
                        string Id = o.OrderId.ToString();
                        decimal OpenPrice = o.Price;
                        decimal Volume = decimal.Parse(o.ExecutedQty, CultureInfo.InvariantCulture);

                        order = GetPosition(symbol);
                        if (order == null)
                        {
                            order = new OrderInformation
                            {
                                Id = symbol,
                                Symbol = symbol,
                                Side = side,
                                OpenTime = DateTime.UtcNow
                            };
                            AddPosition(order);
                        }
                        if (order.Side != side)
                        {
                            RemovePosition(order);
                        }
                        order.OpenPrice = OpenPrice;
                        order.Volume = Volume;
                    }
                    if (o.Status == "NEW")
                    {
                        PlacedOrdersUsd_M.Add(o);
                        string Id = o.OrderId.ToString();
                        decimal OpenPrice = o.Price;
                        decimal Volume = decimal.Parse(o.ExecutedQty, CultureInfo.InvariantCulture);

                        order = GetPosition(symbol);
                        if (order == null)
                        {
                            order = new OrderInformation
                            {
                                Id = o.OrderId.ToString(),//symbol,
                                Symbol = symbol,
                                Side = side,
                                OpenTime = DateTime.UtcNow
                            };
                            AddPosition(order);
                        }
                        if (order.Side != side)
                        {
                            RemovePosition(order);
                        }
                        order.OpenPrice = OpenPrice;
                        order.Volume = Volume;
                    }
                    else
                    {
                        return new OrderOpenResult() { Error = $"Order status = {o.Status}" };
                    }

                }
                if (AccountTradeType == AccountTradeType.COIN_M)
                {
                    COIN_M_NewOrder o = CallAsync<COIN_M_NewOrder>(EndPoints.Coin_M_NewOrder, args).Result;

                    if (o.status == "NEW")
                    {
                        PlacedOrdersCoin_M.Add(o);
                        string Id = o.orderId.ToString();
                        decimal OpenPrice = o.price;
                        decimal Volume = decimal.Parse(o.executedQty, CultureInfo.InvariantCulture);

                        order = GetPosition(symbol);
                        if (order == null)
                        {
                            order = new OrderInformation
                            {
                                Id = symbol,
                                Symbol = symbol,
                                Side = side,
                                OpenTime = DateTime.UtcNow
                            };
                            AddPosition(order);
                        }
                        if (order.Side != side)
                        {
                            RemovePosition(order);
                        }
                        order.OpenPrice = OpenPrice;
                        order.Volume = Volume;
                    }
                }

                if (order != null)
                {
                    return new OrderOpenResult()
                    {
                        Id = order.Id,
                        ExecutionTime = DateTime.UtcNow - begin,
                        OpenPrice = order.OpenPrice
                    };
                }

                return new OrderOpenResult() { Error = "Timeout error." };
            }
            catch (Exception e)
            {
                while (e.InnerException != null) e = e.InnerException;
                return new OrderOpenResult() { Error = e.Message };
            }
        }
        #endregion

        public override void Subscribe(string symbol, string id, string algo = "")
        {
            this.symbol = symbol;
            var quote = new TickEventArgs
            {
                Symbol = symbol,
                SymbolId = id
            };

            WebSocketData data = new WebSocketData
            {
                Active = true,
                quote = quote
            };

            if (SubscribeList.ContainsKey(quote.Symbol)) SubscribeList[quote.Symbol] = data;
            else SubscribeList.Add(quote.Symbol, data);
            // Check Subscribe Websoket's Events:
            switch (algo)
            {
                case "ZigZag":
                    //  SubscribeBookTickerStream(symbol);
                    //temporarily 

                    SubscribePartialDepthStream(quote.Symbol);
                    if (symbol == "BTCUSDT")
                    {
                        ConnectToDepthStream(symbol);
                        SubscribeToAggTradeStream(quote.Symbol);
                    }
                    break;
                case "Scalper":
                    SubscribePartialDepthStream(quote.Symbol);
                    SubscribeToAggTradeStream(quote.Symbol);
                    break;
                case "TwoLegArbitrage":
                    SubscribePartialDepthStream(quote.Symbol);
                    break;
                case "LatencyArbitrage":
                    SubscribePartialDepthStream(quote.Symbol);
                    break;
                case "COIN_M":
                    PartialDepth_COIN_M(quote.Symbol);
                    break;
                case "USD_M":
                    SubscribePartialDepthStream(quote.Symbol);
                    break;
                default:
                    SubscribePartialDepthStream(quote.Symbol);
                    break;
            }
            //OrderStatus();
        }

        public override void Unsubscribe(string symbol, string id)
        {
            if (SubscribeList.ContainsKey(symbol))
            {
                SubscribeList[symbol].Active = false;
                SubscribeList[symbol].webSocket.Close();
                SubscribeList.Remove(symbol);
            }
        }

        public override void Start()
        {
            //var respos = new BinanceOptionsApp.Connections.CustomWebSocket().SendMessageAndReceiveResponseAsync("Hello!");
            //  new BinanceOptionsApp.Connections.CustomWebSocket().func1();
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", Key);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _IsLoggedIn = true;

            StartUserDataStream();


        }

        private async void StartUserDataStream()
        {
            try
            {
                BinanceEndPoint ep = (AccountTradeType == AccountTradeType.USD_M)
                    ? EndPoints.SpotCreateListenKeyFuture : EndPoints.MarginCreateListenKey;
                BinanceListenKey o = await CallAsync<BinanceListenKey>(ep);
                if (!string.IsNullOrEmpty(o.listenKey))
                {
                    OnLoggedIn();
                }
                else
                {
                    // logger.LogError(ViewId + $" listenKey is null");
                }

                try
                {
                    decimal balance = GetBalance();
                    logger.LogInfo(ViewId + $" {AccountTradeType} Balance: {balance} USDT");
                }
                catch (Exception e)
                {
                    logger.LogError(ViewId + $" Margin Balance Error: {e.Message}");
                }
            }
            catch (Exception e)
            {
                logger.LogError(ViewId + $" {e.Message}");
            }
        }

        public override void Stop(bool wait)
        {
            _IsLoggedIn = false;
            OnLoggedOut();
        }

        #region API
        public async Task<T> CallAsync<T>(BinanceEndPoint endpoint, string parameters = null, bool firstCall = true)
        {
            string finalEndpoint = endpoint.Value + (string.IsNullOrWhiteSpace(parameters) ? "" : $"?{parameters}");

            if (endpoint.isSigned)
            {
                string p = parameters + (!string.IsNullOrWhiteSpace(parameters) ? "&timestamp=" : "timestamp=") + GenerateTimeStamp(DateTime.Now.ToUniversalTime());
                var signature = GenerateSignature(Secret, p);
                finalEndpoint = $"{endpoint.Value}?{p}&signature={signature}";
            }

            string urlPath = AccountTradeType == AccountTradeType.USD_M ? "https://fapi.binance.com" + finalEndpoint : urlPath = "https://dapi.binance.com" + finalEndpoint;
            var path = new Uri(urlPath);

            HttpResponseMessage response;
            if (endpoint.Method == "POST")
                response = await _httpClient.PostAsync(path, null);
            else if (endpoint.Method == "GET")
                response = await _httpClient.GetAsync(path);
            else if (endpoint.Method == "PUT")
                response = await _httpClient.PutAsync(path, null);
            else if (endpoint.Method == "DELETE")
                response = await _httpClient.DeleteAsync(path);
            else
                throw new Exception($"Unknown type of request: endpoint.Method = \"{endpoint.Method}\"");

            if (response.IsSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                return JsonConvert.DeserializeObject<T>(result);
            }

            if (response.StatusCode == HttpStatusCode.GatewayTimeout)
            {
                throw new Exception("Api Request Timeout.");
            }

            var e = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //"{\"code\":-1021,\"msg\":\"Timestamp for this request was 1000ms ahead of the server's time.\"}"
            var eCode = 0;
            string eMsg = "";
            if (e.IsValidJson())
            {
                try
                {
                    var i = JObject.Parse(e);

                    eCode = i["code"]?.Value<int>() ?? 0;
                    eMsg = i["msg"]?.Value<string>();
                }
                catch { }
            }

            if (firstCall && eCode == -1021)
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                long clientTime = (long)(DateTime.Now.ToUniversalTime() - epoch).TotalMilliseconds;
                BinanceServerTime sTime = await CallAsync<BinanceServerTime>(EndPoints.CheckServerTimeFuture);
                dTime = clientTime - sTime.serverTime;

                long clientTime2 = (long)(DateTime.Now.ToUniversalTime() - epoch).TotalMilliseconds;
                if ((clientTime2 - clientTime) < 1000)
                    return await CallAsync<T>(endpoint, parameters, false);
                else
                    throw new Exception(string.Format("Api Error Code: {0} Message: {1}", eCode, eMsg));
            }
            else
                throw new Exception(string.Format("Api Error Code: {0} Message: {1}", eCode, eMsg));
        }

        public string GenerateTimeStamp(DateTime baseDateTime)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            return ((long)(baseDateTime.ToUniversalTime() - epoch).TotalMilliseconds - dTime).ToString();
        }
        public string GenerateSignature(string apiSecret, string message)
        {
            var key = Encoding.UTF8.GetBytes(apiSecret);
            string stringHash;
            using (var hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                stringHash = BitConverter.ToString(hash).Replace("-", "").ToLower();
            }

            return stringHash;
        }
        #endregion

        #region Binance future's events [API streams][WebSocket]:
        #region Fields:
        public ConcurrentDictionary<decimal, MarketDepthUpdateFuture> marketDepthList { get; set; }
            = new ConcurrentDictionary<decimal, MarketDepthUpdateFuture>();

        private ConcurrentDictionary<long, BookTickerF> BookTickerFutureDictionary { get; set; }
            = new ConcurrentDictionary<long, BookTickerF>();

        private List<BookTickerF> BookTickerFuture { get; set; } = new List<BookTickerF>();
        private BookTickerF Pre_BookTickerFuture { get; set; }
        private BookTickerF Pre2_BookTickerFuture { get; set; }
        private BookTickerF Pre3_BookTickerFuture { get; set; }

        public List<AggTradeFuture> TasF = new List<AggTradeFuture>();
        private static List<AggTradeFuture> TasRawF = new List<AggTradeFuture>();//??
        private static List<AggTradeFuture> TasCopyF = new List<AggTradeFuture>();// &&??
        private AggTradeFuture tasF;
        private ZigZagFuture zz;
        private ulong counter = 0;
        private double standardDeviationBuy = 0;
        private double standardDeviationSell = 0;
        private decimal meanBuy = 0, meanSell = 0;
        private AggTradeFuture PrevTAS { get; set; }
        private decimal PrevAsk { get; set; }
        private decimal PrevBid { get; set; }
        private decimal LastAskF { get; set; }
        private decimal LastBidF { get; set; }
        private QuoteAskBidFuture qAB = new QuoteAskBidFuture();

        private object lockObject = new object();
        private ConcurrentDictionary<decimal, MarketDepthUpdateFuture> marketDepthUpdate_Cpy { get;set;}
            = new ConcurrentDictionary<decimal, MarketDepthUpdateFuture>();

        private Dictionary<decimal, Plot> TradeLevelPlot = new Dictionary<decimal, Plot>();
        private Dictionary<decimal, Plot> ClusterTradePlot = new Dictionary<decimal, Plot>();

        private decimal QuoteAsk { get; set; }
        private decimal QuoteBid { get; set; }
        private static decimal ask_f { get; set; }
        private static decimal bid_f { get; set; }
        private decimal BestAskFuture { get; set; }
        private decimal BestBidFuture { get; set; }
        // object lockObject = new object(); // створення об'єкта блокування
        // Define an object for locking
        #endregion

        #region Get Market Depth [Http request] [https://fapi.binance.com/fapi/v1/depth?symbol=btcusdt&limit=1000]:
        public DateTime lastTimeUpdDepth5000 = DateTime.MinValue;
        public async void CallMarketDepth(string symbol = "BTCUSDT")
        {
            string depthEndpoint = $"https://fapi.binance.com/fapi/v1/depth?symbol={symbol}&limit=1000";
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    HttpResponseMessage response = await client.GetAsync(depthEndpoint);
                    if (response.IsSuccessStatusCode)
                    {
                        //lastTimeUpdDepth5000 = DateTime.Now;
                        string responseContent = await response.Content.ReadAsStringAsync();

                        var depthUpdate = JsonConvert.DeserializeObject<DepthHttpReqFuture>(responseContent);
                        marketDepthList = new ConcurrentDictionary<decimal, MarketDepthUpdateFuture>();
                        foreach (var ask in depthUpdate.asks)
                        {
                            try
                            {
                                var key = decimal.Parse(ask[0], CultureInfo.InvariantCulture);
                                marketDepthList[key] =
                                    new MarketDepthUpdateFuture { Type = 1, Volume = decimal.Parse(ask[1], CultureInfo.InvariantCulture), LastTimeUpd = DateTime.Now.ToString("mm:ss.fff") };
                            }
                            catch (Exception ex)
                            {
                                logger.LogError($"[CallMarketDepth](asks): Exception message:\n{ex.Message}\nStack Trace:\n{ex.StackTrace}");
                            }
                        }
                        foreach (var bid in depthUpdate.bids)
                        {
                            try
                            {
                                var key = decimal.Parse(bid[0], CultureInfo.InvariantCulture);
                                marketDepthList[key] =
                                     new MarketDepthUpdateFuture { Type = 2, Volume = decimal.Parse(bid[1], CultureInfo.InvariantCulture), LastTimeUpd = DateTime.Now.ToString("mm:ss.fff") };
                            }
                            catch (Exception ex)
                            {
                                logger.LogError($"[CallMarketDepth](bids): Exception message:\n{ex.Message}\nStack Trace:\n{ex.StackTrace}");
                            }
                        }
                    }
                    else logger.LogError($"[CallMarketDepth)]: Failed to fetch data. Status code: {response.StatusCode}");
                }
                catch (HttpRequestException ex)
                {
                    logger.LogError($"Request exception: {ex.Message}");
                }
            }
            await Task.Delay(0);
        }
        #endregion

        #region Get Market Depth [WebSoket] [Up to date Depth] [wss://stream.binance.com:9443/ws/btcusdt@depth]:
        /// <summary>
        /// MARKET DEPTH [UPDATE GENERAL DCTIONARY] [via WebSocket]:
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public void ConnectToDepthStream(string symbol = "BTCUSDT")
        {
            depthWS = new WebSocket4Net.WebSocket($"wss://fstream.binance.com/stream?streams={symbol.ToLower()}@depth@500ms");

            depthWS.Opened += (sender, e) => logger.LogInfo("WebSocket connected! Start @depth [Future] process...");

            depthWS.DataReceived += (sender, e) =>
            {
                var msg = e.Data;
            };

            depthWS.EnableAutoSendPing = true;
            depthWS.AutoSendPingInterval = 180;
            // ++++++++++++++++++++++++++++++++++++++++++++++
            

            //+++++++++++++++++++++++++++++++++++++++++++++++
            depthWS.MessageReceived += (sender, e) =>
            {
                var depth = JsonConvert.DeserializeObject<DepthFuture>(e.Message);
                foreach (var ask in depth.data.Asks)
                    UpdDepthData(ask[0], ask[1], 1);
                foreach (var bid in depth.data.Bids)
                    UpdDepthData(bid[0], bid[1], 2);
                // Отримуємо копію стакану:
                marketDepthList = GetSafeCopyOfMarketDepthList(symbol);
                var time = DateTime.Now.GetTotalMilliseconds() - startDepth;
                ///if (pingPongManager == null && depthWS.State == WebSocketState.Open) 
                //pingPongManager.StartPingPong(depthWS);
                //depthWS.Send("ping");
            };

            depthWS.Error += (sender, e) =>
            {
                logger.LogError($"WebSocket [ConnectToDepthStream()] [future] error: {e.Exception.Message}");
            };

            depthWS.Closed += (s, e) =>
            {
                if (isCloseMDCustom)
                {
                    logger.LogInfo("MarketDepth was closed by Custom!");
                    depthWS.Dispose();
                }
                else
                {
                    Task.Delay(5000).ContinueWith(_ =>
                    {
                        ReconectCount++;
                        if (ReconectCount > 5)
                        {
                            // Can not open WebSocket;
                            return;
                        }
                        logger.LogInfo(ViewId + " Reconect ConnectToDepthStream() to WebSocket #");

                        // если в течении минуты нет реконектов, то сбрасываем счётчик реконектов
                        if (timerResetReconectCount != null) { timerResetReconectCount.Dispose(); }
                        timerResetReconectCount = new Timer(__ => { ReconectCount = 0; timerResetReconectCount.Dispose(); }, null, 60 * 1000, 5000);
                        logger.LogWarning($"WebSocket [ConnectToDepthStream()] [Closed] [future] and reconnect again [during Task.Delay]..");
                        ConnectToDepthStream();
                    });
                }
                //logger.LogWarning($"WebSocket [ConnectToDepthStream()] [Closed] [future] and reconnect again..");
                //ConnectToDepthStream();
            };
            depthWS.Open();
        }

        /// <summary>
        /// Copy general array marketDepthUpdate:
        /// </summary>
        /// <param name="symbol">symbol</param>
        /// <returns>completed copy</returns>
        public ConcurrentDictionary<decimal, MarketDepthUpdateFuture> GetSafeCopyOfMarketDepthList(string symbol)
        {
            ConcurrentDictionary<decimal, MarketDepthUpdateFuture> copyOfMarketDepthList
                = new ConcurrentDictionary<decimal, MarketDepthUpdateFuture>();

            lock (lockObject)
            {
                foreach (var kvp in marketDepthList)
                {
                    copyOfMarketDepthList.TryAdd(kvp.Key, kvp.Value.DeepCopy());
                }

            }

            return copyOfMarketDepthList;
        }
        #endregion

        #region Partial Depth:
        private List<List<string>> BidsP { get; set; }
        private List<List<string>> AsksP { get; set; }
        public List<QuoteFuture> Quotes = new List<QuoteFuture>();
        int dominSell = 0, dominBuy = 0, dominatorSell=0, dominatorBuy=0; decimal AskPrice = 0, BidPrice = 0, AskVolume=0, BidVolume=0,
            PriceRemAsk=0,PriceRemBid=0, resistenceAsk=0, supportBid=0; decimal MaxImpuleUP = 0, MinImpuleUP = 0, MaxImpuleDN = 0, MinImpuleDN = 0;
        decimal MaxImpuleUPLenta = 0, MinImpuleUPLenta = 0, MaxImpuleDNLenta = 0, MinImpuleDNLenta = 0;
        int Zona = 0; decimal startZona = 0, endZona = 0;

        public void SubscribePartialDepthStream(string symbol = "BTCUSDT")
        {
            var webSocket = new WebSocket4Net.WebSocket($"wss://fstream.binance.com/stream?streams={symbol.ToLower()}@depth20@500ms");

            webSocket.Opened += (sender, e) => logger.LogInfo("WebSocket connected! Start Partial @depth20 [Future] process...");

            webSocket.DataReceived += (sender, e) =>
            {
                var msg = e.Data;
            };

            webSocket.EnableAutoSendPing = true;
            webSocket.AutoSendPingInterval = 180;

            webSocket.MessageReceived += (sender, e) =>
            {
                var depth = JsonConvert.DeserializeObject<PartialDepthFuture>(e.Message);
                //var asks = depth.Data.Asks.OrderBy(p => p[0]).Select(o => o[0]).ToList();
                //var bids = depth.Data.Bids.OrderByDescending(p => p[0]).Select(o => o[0]).ToList();
                //BestAskFuture = decimal.Parse(depth.Data.Asks.Select(o => o[0]).First(), CultureInfo.InvariantCulture);
                //BestBidFuture = decimal.Parse(depth.Data.Bids.Select(o => o[0]).First(), CultureInfo.InvariantCulture);
                OnTick(new TickEventArgs
                {
                    Asks = AsksP = depth.Data.Asks,
                    Bids = BidsP = depth.Data.Bids,
                    Ask = QuoteAsk = decimal.Parse(depth.Data.Asks.Select(o => o[0]).First(), CultureInfo.InvariantCulture),//BestAskFuture,
                    Bid = QuoteBid = decimal.Parse(depth.Data.Bids.Select(o => o[0]).First(), CultureInfo.InvariantCulture),//BestBidFuture,
                    Symbol = symbol

                });
                //--------------
                decimal AskV = 0, BidV = 0;
                decimal SumA = 0, SumB = 0;


                AskPrice = decimal.Parse(depth.Data.Asks.Select(o => o[0]).First(), CultureInfo.InvariantCulture);//BestAskFuture,
                BidPrice = decimal.Parse(depth.Data.Bids.Select(o => o[0]).First(), CultureInfo.InvariantCulture);//BestBidFuture,

                AskVolume = decimal.Parse(depth.Data.Asks.Select(o => o[1]).First(), CultureInfo.InvariantCulture);//BestAskFuture,
                BidVolume = decimal.Parse(depth.Data.Bids.Select(o => o[1]).First(), CultureInfo.InvariantCulture);//BestBidFuture,
                
                if (symbol == "BTCUSDT") { ask_f = AskPrice; bid_f = BidPrice; }
                   
                lock (lockObject)
                {
                    var AAA = depth.Data.Asks.ToArray();
                    var BBB = depth.Data.Bids.ToArray();
                    for (int i = 0; i < AsksP.Count(); i++)
                    {
                        var Av = AAA[i];
                        var Bv = BBB[i];

                        string vA = Av[1];
                        string vB = Bv[1];
                        BidV = decimal.Parse(vB);
                        AskV = decimal.Parse(vA);
                        SumA += AskV;
                        SumB += BidV;
                    }
                    DateTimeOffset currentTime = DateTimeOffset.Now;
                    string formattedTime = currentTime.ToString("HH:mm:ss.fff");

                    if (symbol == "BTCUSDT")
                    {
                        if (SumA > 8 * SumB && AskPrice != PriceRemAsk)
                        {
                            
                            if (AskPrice > MaxImpuleDN) MaxImpuleDN = AskPrice;
                            if (AskPrice < MinImpuleDN || MinImpuleDN == 0) MinImpuleDN = AskPrice;

                            MaxImpuleUP = 0; MinImpuleUP = 0;
                            dominSell++; dominBuy = 0;
                            PriceRemAsk = AskPrice;
                            resistenceAsk = AskPrice;

                            if (dominSell >= 3)
                            {
                                dominatorSell++; dominatorBuy = 0; 

                                // string symb = TradeZigZag.Symbol; 
                                var Len = MaxImpuleDN - MinImpuleDN;

                                if (Len <= 10 && dominatorSell >= 15)
                                {
                                    // TradeZigZag.StakanImpulse = 11;
                                    string FileName4 = "PrusginaStakan" + symbol + ".txt";
                                    WriteMessageToDesktopFile($"Time: {formattedTime} | dominatorSell = {dominatorSell},  Price={PriceRemAsk} | Len = {Len}", FileName4);
                                }
                                //if (Len > 10) { TradeZigZag.StakanImpulse = 0; }

                                if (Len <= 100 && dominatorSell >= 15)
                                {
                                    stakanDominatorDN++; stakanDominatorUP = 0;

                                    string FileName4 = "L100D15Stakan" + symbol + ".txt";
                                    WriteMessageToDesktopFile($"Time: {formattedTime} | dominatorSell = {dominatorSell},  Price={PriceRemAsk} | Len = {Len}", FileName4);
                                    if (stakanDominatorDN >= 1)
                                    {
                                        TradeZigZag.StakanImpulse = 1;
                                        TradeZigZag.StakanImpulseTime = currentTime;
                                        TradeZigZag.StakanImpulseWeight = dominatorSell;
                                        TradeZigZag.StakanImpulsePrice = MinImpuleDN;
                                    }

                                }

                                string FileName = "StakanFuture" + symbol + ".txt";

                                WriteMessageToDesktopFile($"Time: {formattedTime} | dominSell = {dominSell},  Price={PriceRemAsk}  | SumBuyLimit = {SumB} | SumSellLimit = {SumA}", FileName);

                                if (dominatorSell > 0) { TradeZigZag.Stakan = 10; TradeZigZag.StakanTime = DateTimeOffset.Now; TradeZigZag.StakanWeight = dominatorSell; }
                                if (dominatorSell >= 10) { TradeZigZag.Stakan = 1; TradeZigZag.StakanTime = DateTimeOffset.Now;  Zona = 0; TradeZigZag.StakanWeight = dominatorSell; }
                                if (dominatorSell >= 30) { TradeZigZag.Stakan = 111; TradeZigZag.StakanTime = DateTimeOffset.Now; TradeZigZag.StakanWeight = dominatorSell; }
                                //if (dominatorSell == 0) { TradeZigZag.Stakan = 0; TradeZigZag.StakanTime = DateTimeOffset.Now; }

                                //----%%%%%%%%%%------ S ------
                                Step_St = true;

                                if (Step_Lt)//!Step_St && // && TradeZigZag.StakanTime > TradeZigZag.StepByStepTime
                                {
                                    if (TradeZigZag.Lentochka == 1 && TradeZigZag.StakanTime > TradeZigZag.LentochkaTime)
                                    { TradeZigZag.StepByStepDN++; TradeZigZag.StepByStepPrice = BidPrice; TradeZigZag.StepByStepTime = currentTime;  Step_Lt = false; }
                                   
                                }
                                
                                TradeZigZag.StepByStepUP = 0;

                                if (TradeZigZag.StepByStepDN >= 3)
                                {
                                    if (TradeZigZag.FonStepByStepUP > 0)
                                    {
                                        TradeZigZag.rememberFonStepUP = TradeZigZag.FonStepByStepUP;
                                        if (TradeZigZag.rememberFonStepUP > TradeZigZag.rememberFonStepDN)
                                        {
                                            if (TradeZigZag.OrderStep == 2)
                                            {
                                                TradeZigZag.OrderStep = 101; TradeZigZag.OrderStepTime = currentTime;
                                                string FileNameR = "RememberByStep" + symbol + ".txt";
                                               // WriteMessageToDesktopFile($"Time: {formattedTime} | close UP | rememberFonStepUP={TradeZigZag.rememberFonStepUP} |rememberFonStepDN={TradeZigZag.rememberFonStepDN} | Price={AskPrice}", FileNameR);

                                            }
                                        }

                                        if (TradeZigZag.rememberFonStepDN >= 8 && TradeZigZag.rememberFonStepUP < TradeZigZag.rememberFonStepDN)
                                        {
                                         //   TradeZigZag.OrderStep = 10;
                                           // TradeZigZag.OrderStepTime = currentTime;
                                            string FileNameR = "RememberByStep_00_" + symbol + ".txt";
                                           // WriteMessageToDesktopFile($"Time: {formattedTime} | DN | rememberFonStepUP={TradeZigZag.rememberFonStepUP} |rememberFonStepDN={TradeZigZag.rememberFonStepDN} | Price={AskPrice}", FileNameR);
                                        }
                                  
                                    }

                                    TradeZigZag.FonStepByStepDN++; TradeZigZag.FonStepByStepUP = 0;
                                    string FileNameS = "StepByStep" + symbol + ".txt";
                                    WriteMessageToDesktopFile($"Time: {formattedTime} | DN | Fon={TradeZigZag.FonStepByStepDN}  |StepByStepDN = {TradeZigZag.StepByStepDN} | Price={AskPrice}", FileNameS);
                                }


                            }
                        }
                        else if (SumA > 2 * SumB && AskPrice != PriceRemAsk)
                        {
                            PriceRemAsk = AskPrice;
                            string FileT = "TideStakan.txt";
                            WriteMessageToDesktopFile($"Time: {formattedTime} | 1 | Price={AskPrice}  | SumBuyLimit = {SumB} | SumSellLimit = {SumA}", FileT);
                            TradeZigZag.Tide = 10;
                            TradeZigZag.TideTime = DateTimeOffset.Now;
                            TradeZigZag.TidePrice = AskPrice;
                        }

                        if (SumB > 8 * SumA && BidPrice != PriceRemBid)
                        {
                            if (BidPrice > MaxImpuleUP)MaxImpuleUP = BidPrice;
                    
                            if (BidPrice < MinImpuleUP || MinImpuleUP == 0)MinImpuleUP = BidPrice;

                            MinImpuleDN = 0; MaxImpuleDN = 0;
                            dominBuy++; dominSell = 0;
                            PriceRemBid = BidPrice;
                            supportBid = BidPrice;

                            if (dominBuy >= 3)
                            {
                                dominatorBuy++; dominatorSell = 0; 

                                decimal Len = 0;
                                if (MaxImpuleUP > 0 && MinImpuleUP > 0) Len = MaxImpuleUP - MinImpuleUP;

                                if (Len <= 10 && dominatorBuy >= 15)
                                {
                                    //  TradeZigZag.StakanImpulse = 22;
                                    string FileName5 = "PrusginaStakan" + symbol + ".txt";
                                    WriteMessageToDesktopFile($"Time: {formattedTime} | dominatorBuy = {dominatorBuy},  Price={PriceRemBid} | Len = {Len}", FileName5);
                                }
                                //if (Len > 10) { TradeZigZag.StakanImpulse = 0; }

                                if (Len <= 100 && dominatorBuy >= 15)
                                {
                                    stakanDominatorDN = 0; stakanDominatorUP++;
                                    string FileName4 = "L100D15Stakan" + symbol + ".txt";
                                    WriteMessageToDesktopFile($"Time: {formattedTime} | dominatorBuy = {dominatorBuy},  Price={PriceRemBid} | Len = {Len}", FileName4);
                                    if (stakanDominatorUP >= 1)
                                    {
                                        TradeZigZag.StakanImpulse = 2;
                                        TradeZigZag.StakanImpulseTime = currentTime;
                                        TradeZigZag.StakanImpulseWeight = dominatorBuy;
                                        TradeZigZag.StakanImpulsePrice = MaxImpuleUP;
                                    }

                                }

                                string FileName = "StakanFuture" + symbol + ".txt";

                                    WriteMessageToDesktopFile($"Time: {formattedTime} | dominBuy = {dominBuy},  Price={PriceRemBid}  | SumBuyLimit = {SumB} | SumSellLimit = {SumA}", FileName);
                                if (dominatorBuy > 0) { TradeZigZag.Stakan = 20; TradeZigZag.StakanTime = DateTimeOffset.Now; TradeZigZag.StakanWeight = dominatorBuy; }
                                if (dominatorBuy >= 10) {TradeZigZag.Stakan = 2; Zona = 0; TradeZigZag.StakanTime = DateTimeOffset.Now; TradeZigZag.StakanWeight = dominatorBuy; }
                                if (dominatorBuy >= 30) { TradeZigZag.Stakan = 222; TradeZigZag.StakanTime = DateTimeOffset.Now; TradeZigZag.StakanWeight = dominatorBuy; }
                                //if (dominatorBuy == 0) { TradeZigZag.Stakan = 0; TradeZigZag.StakanTime = DateTimeOffset.Now; }
                          
                               //-----%%%%%%---- S ---
                                Step_St = true;

                                if (Step_Lt)//!Step_St && // && TradeZigZag.StakanTime > TradeZigZag.StepByStepTime
                                {
                                    if (TradeZigZag.Lentochka == 2 && TradeZigZag.StakanTime > TradeZigZag.LentochkaTime)
                                    { TradeZigZag.StepByStepUP++; TradeZigZag.StepByStepPrice = AskPrice; TradeZigZag.StepByStepTime = currentTime;  Step_Lt = false; }
                                }

                                TradeZigZag.StepByStepDN = 0;

                                if (TradeZigZag.StepByStepUP >= 3)
                                {
                                    if (TradeZigZag.FonStepByStepDN > 0)
                                    {   
                                        TradeZigZag.rememberFonStepDN = TradeZigZag.FonStepByStepDN;

                                        if (TradeZigZag.rememberFonStepUP > TradeZigZag.rememberFonStepDN)
                                        {
                                            if (TradeZigZag.OrderStep == 1)
                                            {
                                                TradeZigZag.OrderStep = 202; TradeZigZag.OrderStepTime = currentTime;
                                                string FileNameR = "RememberByStep" + symbol + ".txt";
                                              //  WriteMessageToDesktopFile($"Time: {formattedTime} | close DN | rememberFonStepUP={TradeZigZag.rememberFonStepUP} |rememberFonStepDN={TradeZigZag.rememberFonStepDN} | Price={AskPrice}", FileNameR);

                                            }
                                        }
                                        if (TradeZigZag.rememberFonStepUP >= 8 && TradeZigZag.rememberFonStepUP > TradeZigZag.rememberFonStepDN)
                                        {
                                          //  TradeZigZag.OrderStep = 20;
                                            //TradeZigZag.OrderStepTime = currentTime;
                                            string FileNameR = "RememberByStep_00_" + symbol + ".txt";
                                          //  WriteMessageToDesktopFile($"Time: {formattedTime} | UP | rememberFonStepUP={TradeZigZag.rememberFonStepUP} |rememberFonStepDN={TradeZigZag.rememberFonStepDN} | Price={AskPrice}", FileNameR);
                                        }

                                 
                                    }

                                    TradeZigZag.FonStepByStepDN = 0; TradeZigZag.FonStepByStepUP++;
                                    string FileNameS = "StepByStep" + symbol + ".txt";
                                    WriteMessageToDesktopFile($"Time: {formattedTime} | UP | Fon={TradeZigZag.FonStepByStepUP} |StepByStepUP = {TradeZigZag.StepByStepUP} | Price={BidPrice}", FileNameS);
                                }


                            }
                        }
                        else if (SumB > 2 * SumA && BidPrice != PriceRemBid)
                        {
                            PriceRemBid = BidPrice;
 
                            string FileT = "TideStakan.txt";
                            WriteMessageToDesktopFile($"Time: {formattedTime} | 2 | Price={BidPrice}  | SumBuyLimit = {SumB} | SumSellLimit = {SumA}", FileT);
                            TradeZigZag.Tide = 20;
                            TradeZigZag.TideTime = DateTimeOffset.Now;
                            TradeZigZag.TidePrice = BidPrice;
                        }
                    }
                }

            };

            webSocket.Error += (sender, e) => logger.LogError($"WebSocket [SubscribePartialDepthStream()] [future] error: {e.Exception.Message}");
            webSocket.Closed += (s, e) =>
            {
                Task.Delay(5000).ContinueWith(_ =>
                    {
                        ReconectCount++;
                        if (ReconectCount > 5)
                        {
                            // Can not open WebSocket;
                            return;
                        }
                        logger.LogInfo(ViewId + " Reconect to WebSocket #");

                        // если в течении минуты нет реконектов, то сбрасываем счётчик реконектов
                        if (timerResetReconectCount != null) { timerResetReconectCount.Dispose(); }
                        timerResetReconectCount = new Timer(__ => { ReconectCount = 0; timerResetReconectCount.Dispose(); }, null, 60 * 1000, 5000);
                        logger.LogWarning($"WebSocket [SubscribePartialDepthStream()] [Closed] [future] and reconnect again [during Task.Delay]..");
                        ConnectToDepthStream();
                    });
                //logger.LogWarning($"WebSocket [SubscribePartialDepthStream()] [Closed] [future] and reconnect again..");
                //ConnectToDepthStream();
            };
            webSocket.Open();
        }
        #endregion

        #region Get BookTicker for Quotes [wss://fstream.binance.com/stream?streams=BTCUSDT@bookTicker]:
        long startTime2 = 0;
        public BookTickerF bt;
        public void SubscribeBookTickerStream(string symbol = "BTCUSDT")
        {

            bookTikerWS = new WebSocket4Net.WebSocket($"wss://fstream.binance.com/stream?streams={symbol.ToLower()}@bookTicker");
            // bookTikerWS.
            bookTikerWS.Opened += (sender, e) => logger.LogInfo("WebSocket connected! Start @bookTicker [Future] process...");

            bookTikerWS.DataReceived += (sender, e) =>
            {
                var msg = e.Data;
            };

            bookTikerWS.NoDelay = true;
            bookTikerWS.EnableAutoSendPing = true;
            bookTikerWS.AutoSendPingInterval = 180;

            bookTikerWS.MessageReceived += (sender, e) =>
            {
                if (Socket_Wnd.IsClient)
                {
                   
                }
                else // if Server(Listener):
                {

                }
                bt = JsonConvert.DeserializeObject<BookTickerF>(e.Message);
                bt.Data.SynchronizationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                AddToDictionary(bt);
                BookTickerFuture.Add(bt);
                //-----------------------------------------------
            };

            bookTikerWS.Error += (sender, e) =>
            {
                if (e.Exception.Message.Contains("429"))
                {
                    logger.LogError("!!!!!!!!!!!!!!!!!! We have reached the limit. WARNING! 429 !!!!!!!!!!!!!!!!!!!!");
                    // call all closed websocket ...
                }
                logger.LogError($"WebSocket [SubscribeBookTickerStream()] [future] error: {e.Exception.Message}");
            };

            bookTikerWS.Closed += (s, e) =>
            {
                if (isCloseBTCustom)
                {
                    logger.LogInfo("BookTicker was closed by Custom!");
                    bookTikerWS.Dispose();
                }
                else
                {
                    Task.Delay(5000).ContinueWith(_ =>
                    {
                        ReconectCount++;
                        if (ReconectCount > 5)
                        {
                            // Can not open WebSocket;
                            return;
                        }
                        logger.LogInfo(ViewId + " Reconect SubscribeBookTickerStream WebSocket #");

                        // если в течении минуты нет реконектов, то сбрасываем счётчик реконектов
                        if (timerResetReconectCount != null) { timerResetReconectCount.Dispose(); }
                        timerResetReconectCount = new Timer(__ => { ReconectCount = 0; timerResetReconectCount.Dispose(); }, null, 60 * 1000, 5000);
                        logger.LogWarning($"WebSocket [SubscribeBookTickerStream()] [Closed] [future] and reconnect again [during Task.Delay]..");
                        SubscribeBookTickerStream(symbol);
                    });
                }
            };
            bookTikerWS.Open();
            //Task.Delay(10);
        }

        public void AddToDictionary(BookTickerF model)
        {
            if (BookTickerFutureDictionary.TryAdd(model.Data.EventTime, model))
            {
                //  var sorted = BookTickerFutureDictionary.OrderBy(t => t.Key);  // Відсортувати словник за зростанням ключа (EventTime)
                //   var sortedDictionary = new ConcurrentDictionary<long, BookTickerF>(sorted);  // Створити новий відсортований словник
                //  BookTickerFutureDictionary = sortedDictionary; // Замінити оригінальний словник відсортованим
            }
        }

        long lastSearchTime = 0; int lastIndexF = 0;
        public BookTickerF FindModelByTime(long targetTime)
        {
            var CopyBook = BookTickerFuture;

            lock (lockObject)
            {
                BookTickerF mod;
                // var foundModel = CopyBook.FirstOrDefault(model => model.SynchronizationTime >= targetTime && model.SynchronizationTime >= lastSearchTime);
                var foundModelInd = CopyBook.FindIndex(lastIndexF, model => model.Data.SynchronizationTime >= targetTime);
                if (foundModelInd > 0)
                {
                    mod = CopyBook[foundModelInd];
                    //  lastSearchTime = foundModel.SynchronizationTime;
                    lastIndexF = foundModelInd;
                    return mod;
                }
                else return null;
                // return foundModel;
            }
        }

        public BookTickerF FindModelByTime2(long targetTime)
        {
            var foundModel = BookTickerFutureDictionary
                .Where(pair => pair.Value.Data.EventTime >= targetTime)
                .OrderBy(pair => pair.Value.Data.EventTime)
                .Select(pair => pair.Value)
                .FirstOrDefault();

            return foundModel;
        }

        public void RemoveOlderThanSynchronizationTime(long synchronizationTime)
        {
            var keysToRemove = BookTickerFutureDictionary// Отримати список ключів, які потрібно видалити
                .Where(pair => pair.Value.Data.EventTime < synchronizationTime - 3000)
                .Select(pair => pair.Key)
                .ToList();

            foreach (var key in keysToRemove)// Видалити об'єкти за визначеними ключами
            {
                BookTickerFutureDictionary.TryRemove(key, out _);
            }
        }
        #endregion

        #region Time and Sales [WebSoket] [wss://fstream.binance.com:9443/ws/btcusdt@aggTrade]:
        // Створення словника Plot
      //  public static Dictionary<decimal, Plot> TradeLevelPlot = new Dictionary<decimal, Plot>();
        private BookTickerF PreMod { get; set; }
        // Створення нового таймера Stopwatch
        Stopwatch stopwatch = new Stopwatch();
        AggTradeFuture PreUpdate;

        decimal PreAskPrice=0, PreBidPrice=0, PreAskVolume=0, PreBidVolume = 0, PremarketBuy=0, PremarketSell=0, Pre_p=0;
        int LentaBuy = 0, LentaSell = 0, KusKusUP = 0, KusKusDN = 0, FonDN=0, FonUP = 0, MoveBuy=0, MoveSell = 0;
        int stakanDominatorUP = 0, stakanDominatorDN = 0;
        decimal KusUPprice = 0, KusDNprice = 0;
        decimal SumBuyLenta = 0, SumSellLenta = 0, SumLong=0, SumShort=0;
        int SumLentaUP=0, SumLentaDN=0;
        long LastTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string PreMes = "", PreMes2 = "";
        ulong PreID = 0;
        public async void SubscribeToAggTradeStream(string symbol = "btcusdt")
        {
            aggTrade = new WebSocket4Net.WebSocket($"wss://fstream.binance.com/stream?streams={symbol.ToLower()}@aggTrade");

            aggTrade.Opened += (sender, e) =>
            {
                logger.LogInfo("Lenta has started...Start SubscribeToAggTradeStream @aggTrade [future] process...");
            };

            aggTrade.DataReceived += (sender, e) =>
            {
                var msg = e.Data;
            };

            aggTrade.EnableAutoSendPing = true;
            aggTrade.AutoSendPingInterval = 180;

            long TimeTrade;
            //var mod = TasRawF.Count > 0 && TasRawF != null ? TasRawF.Last() : null; //tut problema
            //System.IndexOutOfRangeException: 'Index was outside the bounds of the array.'
            //This exception was originally thrown at this call stack:
            //[External Code]
            //MultiTerminal.Connections.BinanceFutureClient.SubscribeToAggTradeStream(string) in BinanceFutureClient.cs

            //------------------------------------------
            aggTrade.Error += (sender, e) =>
            {
                logger.LogError(e.Exception.Message);
            };

            int lastIndexF = 0; long startTime = 0; 

            // webSocket.MessageReceived += (sender, e) => //// TAS
            aggTrade.MessageReceived += async (sender, e) =>
            {
                var tasUpdateR = JsonConvert.DeserializeObject<AggTradeFuture>(e.Message);
                if (startTime == 0) startTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                //==================================
                try
                {
                    if (symbol == "BTCUSDT")
                    {
                        TasRawF.Add(tasUpdateR);
                        TasCopyF = TasRawF;
                    }
                }
                catch (Exception ex)
                {

                }
                //var ff = symbol;
                //==========================================
                if (tasUpdateR != null && tasUpdateR != PreUpdate)
                {
                    decimal p = tasUpdateR.data.Price;
                    decimal v = tasUpdateR.data.Volume;
                    bool um = tasUpdateR.data.IsMarketMaker;
                    var id = tasUpdateR.data.AggTradeId;
                    decimal marketBuy = um ? 0 : v;
                    decimal marketSell = um ? v : 0;
                    decimal key = p;
                    if (id != PreID)
                    {
                       lock (lockObject)
                       {
                          if (TradeLevelPlot.TryGetValue(key, out var plot))
                          {
                            plot.MarketBuy += marketBuy;
                            plot.MarketSell += marketSell;
                          }
                          else
                          {
                             try
                             {
                                TradeLevelPlot.Add(key, new Plot { MarketBuy = marketBuy, MarketSell = marketSell, Price = key });
                             }
                             catch (Exception ex) { }
                          }
                       }
                        //++++++++++++++++++++++++++++++++++++++++++
                        SumLong += marketBuy; SumShort += marketSell;

                        if ((DateTimeOffset.UtcNow.ToUnixTimeSeconds() - LastTime) >= 10)
                        {
                            
                            string FileNameX = "StreamLenta" + symbol + ".txt";
                            DateTimeOffset currentTime = DateTimeOffset.Now;
                            string formattedTime = currentTime.ToString("HH:mm:ss.fff");
                            if (SumLong > 1.6m * SumShort)
                            {
                                SumLentaUP++; SumLentaDN=0;
                                TradeZigZag.SumLenta = 2; TradeZigZag.SumLentaWeight = SumLentaUP; TradeZigZag.SumLentaTime = currentTime;
                                
                                string MesX = $"Time: {formattedTime} | 2 | WeightUP ={SumLentaUP} | SumLong = {SumLong} | SumShort = {SumShort} | Price={p} ";
                                WriteMessageToDesktopFile(MesX, FileNameX);
                            }
                            if (SumLong*1.6m < SumShort)
                            {
                                SumLentaUP =0; SumLentaDN ++;
                                TradeZigZag.SumLenta = 1; TradeZigZag.SumLentaWeight = SumLentaUP; TradeZigZag.SumLentaTime = currentTime;
                                
                                string MesX = $"Time: {formattedTime} | 1 | WeightDN ={SumLentaDN} | SumLong = {SumLong} | SumShort = {SumShort} | Price={p} ";
                                WriteMessageToDesktopFile(MesX, FileNameX);
                            }

                            SumLong = 0; SumShort = 0;
                            LastTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                        }

                        if (marketBuy >= 1)
                        {
                            LentaBuy++; LentaSell = 0; MaxImpuleDNLenta = 0; MinImpuleDNLenta = 0; SumSellLenta = 0;
                            SumBuyLenta += marketBuy;
 
                            DateTimeOffset currentTime = DateTimeOffset.Now;
                            string formattedTime = currentTime.ToString("HH:mm:ss.fff");

                            if (p > MaxImpuleUPLenta)
                            {
                                MaxImpuleUPLenta = p;
                            }
                            if (p < MinImpuleUPLenta || MinImpuleUPLenta == 0)
                            {
                                MinImpuleUPLenta = p;
                            }
                            //-------------%%%%%%%%--------- L ---------------------------------------------------
                            TradeZigZag.Lentochka = 2; TradeZigZag.LentochkaPrice = p; TradeZigZag.LentochkaTime = currentTime;
                            Step_Lt = true; 
                            //-------------------------------------------------------------------------
                            if (Step_St)// && !Step_Lt
                            {
                                if ((TradeZigZag.Stakan == 20 || TradeZigZag.Stakan == 2) && TradeZigZag.StakanWeight >= 3 && TradeZigZag.StakanTime < TradeZigZag.LentochkaTime && TradeZigZag.LentochkaTime > TradeZigZag.StepByStepTime)
                                { TradeZigZag.StepByStepUP++; TradeZigZag.StepByStepPrice = p; TradeZigZag.StepByStepTime = currentTime; Step_St = false; }
                            }

                            TradeZigZag.StepByStepDN = 0;

                            if (TradeZigZag.StepByStepUP >= 3)
                            {
                                if (TradeZigZag.FonStepByStepDN > 0)
                                {
                                    TradeZigZag.rememberFonStepDN = TradeZigZag.FonStepByStepDN;

                                    if (TradeZigZag.rememberFonStepUP > TradeZigZag.rememberFonStepDN)
                                    {
                                        if(TradeZigZag.OrderStep == 1)
                                        {
                                            TradeZigZag.OrderStep = 202; TradeZigZag.OrderStepTime = currentTime;
                                            string FileNameR = "RememberByStep" + symbol + ".txt";
                                           // WriteMessageToDesktopFile($"Time: {formattedTime} | close DN | rememberFonStepUP={TradeZigZag.rememberFonStepUP} |rememberFonStepDN={TradeZigZag.rememberFonStepDN} | Price={p}", FileNameR);

                                        }
                                    }

                                    if (TradeZigZag.rememberFonStepUP >= 8 && TradeZigZag.rememberFonStepUP > TradeZigZag.rememberFonStepDN)
                                    {
                                      //  TradeZigZag.OrderStep = 20;
                                        //TradeZigZag.OrderStepTime = currentTime;
                                        string FileNameR = "RememberByStep_00_" + symbol + ".txt";
                                        //WriteMessageToDesktopFile($"Time: {formattedTime} | UP | rememberFonStepUP={TradeZigZag.rememberFonStepUP} |rememberFonStepDN={TradeZigZag.rememberFonStepDN} | Price={AskPrice}", FileNameR);
                                    }

                             
                                }

                                TradeZigZag.FonStepByStepDN = 0; TradeZigZag.FonStepByStepUP ++;
                                string FileNameS = "StepByStep" + symbol + ".txt";
                                WriteMessageToDesktopFile($"Time: {formattedTime} | UP | Fon={TradeZigZag.FonStepByStepUP}  |StepByStepUP = {TradeZigZag.StepByStepUP} | Price={p}", FileNameS);
                            }
                            //------------%%%%%%%%-------------------------------------------------------------

                            string FileName3 = "Lenta" + symbol + ".txt";
 
                            string Mes = $"Time: {formattedTime} | EatingAskResistence = {resistenceAsk} | MarketBuy = {marketBuy} | Price={p} ";
                            // if (resistenceAsk == p) WriteMessageToDesktopFile(Mes, FileName3);
                            var Len = MaxImpuleUPLenta - MinImpuleUPLenta;

                            if (Len <= 20 && LentaBuy >= 10 && symbol == "BTCUSDT")
                            {
                           

                                FonDN =0; FonUP++;
                                
                                TradeZigZag.LentaPruzgina = 2;
                                TradeZigZag.LentaPruzginaWeight = FonUP;
                                TradeZigZag.LentaPruzginaTime = currentTime;

                                string FileName4 = "PrusginaLenta" + symbol + ".txt";
                                WriteMessageToDesktopFile($"Time: {formattedTime} | FonUP={FonUP} |LentaBuy = {LentaBuy}| SumBuyLenta = {SumBuyLenta}   Price={p} | Len = {Len}", FileName4);

                                if (TradeZigZag.Stakan == 1)
                                {
                                    TradeZigZag.fight = 2;
                                    TradeZigZag.fightTime = currentTime;
                                    string FileNameF = "fightLenta" + symbol + ".txt";
                                    WriteMessageToDesktopFile($"Time: {formattedTime} | fight = 2 | FoonUP = {FonUP}|   Price={p}", FileNameF);
                                }

                                if (TradeZigZag.Stakan == 2)
                                {
                                    TradeZigZag.Fon = 2;
                                    TradeZigZag.FonTime = currentTime;
                                    string FileNameF = "FonLenta" + symbol + ".txt";
                                    WriteMessageToDesktopFile($"Time: {formattedTime} | fon = 2 | FoonUP = {FonUP}|   Price={p}", FileNameF);
                                }

                                if (p > Pre_p)
                                {
                                    KusKusDN = 0; KusKusUP++;
                                    
                                    if (KusKusUP >= 3)
                                    {
                                        TradeZigZag.Lenta = 2; KusUPprice = p;
                                        TradeZigZag.LentaTime = currentTime;
                                        TradeZigZag.LentaWeight = KusKusUP;
                                        TradeZigZag.LentaPrice = KusUPprice;

                                        WriteMessageToDesktopFile($"Time: {formattedTime} | KusBuy = {KusKusUP}| p={p} ", "KusLenta" + symbol + ".txt");
                                        if (TradeZigZag.Stakan == 2)
                                        {
                                            MoveBuy++; MoveSell = 0;
                                          //  WriteMessageToDesktopFile($"Time: {formattedTime} | MoveBuy | p={p} ", "Move" + symbol + ".txt");
                                            if (MoveBuy >= 1) TradeZigZag.StakanLentaMove = 2;
                                            else TradeZigZag.StakanLentaMove = 0;
                                        }
                                    }
                                }
 
                            }

                            string Mes2 = $"Price={p} MarkeBuy = {marketBuy} | Ask={AskPrice} Volume={AskVolume} | Bid={BidPrice} Volume={BidVolume}";
                            // if(PreMes2 != Mes2) 
                            WriteMessageToDesktopFile($"Time: {formattedTime} |  " + Mes2, FileName3);

                            PreMes2 = Mes2;  PreAskPrice = AskPrice; PreBidPrice = BidPrice; PreAskVolume = AskVolume; PreBidVolume = BidVolume; PremarketBuy = marketBuy;
                        }

                        if (marketSell >= 1)
                        {
                            LentaBuy = 0; LentaSell++; MaxImpuleUPLenta = 0; MinImpuleUPLenta = 0; SumBuyLenta = 0;
                            SumSellLenta += marketSell;

                            DateTimeOffset currentTime = DateTimeOffset.Now;
                            string formattedTime = currentTime.ToString("HH:mm:ss.fff");

                            if (p > MaxImpuleDNLenta)
                            {
                                MaxImpuleDNLenta = p;
                            }
                            if (p < MinImpuleDNLenta || MinImpuleDNLenta == 0)
                            {
                                MinImpuleDNLenta = p;
                            }

                            var Len = MaxImpuleDNLenta - MinImpuleDNLenta;

                            //----------------%%%%%%%%%%--------- L ------------------------------------------------
                            TradeZigZag.Lentochka = 1; TradeZigZag.LentochkaPrice = p; TradeZigZag.LentochkaTime = currentTime;
                            Step_Lt = true; 
                            //-------------------------------------------------------------------------
                            if (Step_St)//&& !Step_Lt
                            {
                                if ((TradeZigZag.Stakan == 1 || TradeZigZag.Stakan == 10) && TradeZigZag.StakanWeight >= 3 && TradeZigZag.StakanTime < TradeZigZag.LentochkaTime && TradeZigZag.LentochkaTime > TradeZigZag.StepByStepTime)
                                { TradeZigZag.StepByStepDN++; TradeZigZag.StepByStepPrice = p; TradeZigZag.StepByStepTime = currentTime; Step_St = false;}
                            }
                            
                            TradeZigZag.StepByStepUP=0;

                            if(TradeZigZag.StepByStepDN >= 3) 
                            {
                                if (TradeZigZag.FonStepByStepUP > 0)
                                {
                                    TradeZigZag.rememberFonStepUP = TradeZigZag.FonStepByStepUP;
                                    if (TradeZigZag.rememberFonStepUP > TradeZigZag.rememberFonStepDN)
                                    {
                                        if (TradeZigZag.OrderStep == 2)
                                        {
                                            TradeZigZag.OrderStep = 101; TradeZigZag.OrderStepTime = currentTime;
                                            string FileNameR = "RememberByStep" + symbol + ".txt";
                                          //  WriteMessageToDesktopFile($"Time: {formattedTime} | close UP | rememberFonStepUP={TradeZigZag.rememberFonStepUP} |rememberFonStepDN={TradeZigZag.rememberFonStepDN} | Price={p}", FileNameR);

                                        }
                                    }
                                    if (TradeZigZag.rememberFonStepDN >= 8 && TradeZigZag.rememberFonStepUP < TradeZigZag.rememberFonStepDN)
                                    {
                                      //  TradeZigZag.OrderStep = 10;
                                       // TradeZigZag.OrderStepTime = currentTime;
                                        string FileNameR = "RememberByStep_00_" + symbol + ".txt";
                                       // WriteMessageToDesktopFile($"Time: {formattedTime} | DN | rememberFonStepUP={TradeZigZag.rememberFonStepUP} |rememberFonStepDN={TradeZigZag.rememberFonStepDN} | Price={AskPrice}", FileNameR);
                                    }

                                }

                                TradeZigZag.FonStepByStepDN++; TradeZigZag.FonStepByStepUP = 0;
                                string FileNameS = "StepByStep" + symbol + ".txt";
                                WriteMessageToDesktopFile($"Time: {formattedTime} | DN | Fon={TradeZigZag.FonStepByStepDN}  |StepByStepDN = {TradeZigZag.StepByStepDN} | Price={p}", FileNameS);
                            }
                            //----------------%%%%%%%%%%---------------------------------------------------------

                            if (Len <= 20 && LentaSell >= 10 && symbol == "BTCUSDT")
                            {
                       
                                FonDN++; FonUP = 0; 
                                TradeZigZag.LentaPruzgina = 1;
                                TradeZigZag.LentaPruzginaWeight = FonDN;
                                TradeZigZag.LentaPruzginaTime = currentTime;

                                string FileName4 = "PrusginaLenta" + symbol + ".txt";
                                WriteMessageToDesktopFile($"Time: {formattedTime} | FonDN={FonDN} | LentaSell = {LentaSell} | SumSellLenta = {SumSellLenta} | Price={p} | Len = {Len}", FileName4);

                                if (TradeZigZag.Stakan == 2)
                                {
                                    TradeZigZag.fight = 1;
                                    TradeZigZag.fightTime = currentTime;
                                    string FileNameF = "fightLenta" + symbol + ".txt";
                                    WriteMessageToDesktopFile($"Time: {formattedTime} | fight = 1 | FoonDN = {FonDN}|   Price={p}", FileNameF);
                                }
                                if (TradeZigZag.Stakan == 1)
                                {
                                    TradeZigZag.Fon = 1;
                                    TradeZigZag.FonTime = currentTime;
                                    string FileNameF = "FonLenta" + symbol + ".txt";
                                    WriteMessageToDesktopFile($"Time: {formattedTime} | fon = 1 | FoonDN = {FonDN}|   Price={p}", FileNameF);
                                }

                                if (p < Pre_p)
                                { KusKusDN++; KusKusUP = 0;
                                    if (KusKusDN >= 3) 
                                    {   TradeZigZag.Lenta = 1; KusDNprice = p;
                                        TradeZigZag.LentaTime = currentTime;
                                        TradeZigZag.LentaWeight = KusKusDN;
                                        TradeZigZag.LentaPrice = KusDNprice;

                                        WriteMessageToDesktopFile($"Time: {formattedTime} | KusSell = {KusKusDN}| p={p} ", "KusLenta" + symbol + ".txt");
                                        if(TradeZigZag.Stakan == 1) 
                                        {
                                            MoveSell++; MoveBuy = 0;
                                           // WriteMessageToDesktopFile($"Time: {formattedTime} | MoveSell | p={p} ", "Move" + symbol + ".txt");
                                            if (MoveSell >= 1) TradeZigZag.StakanLentaMove = 1;
                                            else TradeZigZag.StakanLentaMove = 0;
                                        }
                                    }
                                }
                                 
                                if (TradeZigZag.Lenta != 1)
                                {
                             
                                }
                          
                            }

                           //   if (supportBid == p)  WriteMessageToDesktopFile($"Time: {formattedTime} | EatingBidSupport = {supportBid} | MarketSell = {marketSell} | Price={p} ", FileName3);
                            string Mes = $"Price={p} MarkeSell = {marketSell}| Ask={AskPrice} Volume={AskVolume} | Bid={BidPrice} Volume={BidVolume}";
                            // if(Mes != PreMes)
                            string FileName3 = "Lenta" + symbol + ".txt";
                            WriteMessageToDesktopFile($"Time: { formattedTime} | " + Mes, FileName3);
                                 
                            PreMes = Mes; PreAskPrice = AskPrice; PreBidPrice = BidPrice; PreAskVolume = AskVolume; PreBidVolume = BidVolume; PremarketSell = marketSell;
                            Pre_p = p;
                        }

                        PreID = id;
                    }

                    //++++++++++++++++++++++++++++++++++++++++++
                    PreUpdate = tasUpdateR;
                }
                //==========================================

            };

            aggTrade.Closed += (s, e) =>
            {
                if (isCloseAggTrdCustom)
                {
                    logger.LogInfo("AggTrade was closed by Custom!");
                    aggTrade.Dispose();
                }
                else
                {
                    Task.Delay(5000).ContinueWith(_ =>
                    {
                            ReconectCount++;
                            if (ReconectCount > 5)
                            {
                                // Can not open WebSocket;
                                return;
                            }
                            logger.LogInfo(ViewId + " Reconect to WebSocket #");

                            // если в течении минуты нет реконектов, то сбрасываем счётчик реконектов
                            if (timerResetReconectCount != null) { timerResetReconectCount.Dispose(); }
                            timerResetReconectCount = new Timer(__ => { ReconectCount = 0; timerResetReconectCount.Dispose(); }, null, 60 * 1000, 5000);
                            logger.LogError($"WebSocket [SubscribeToAggTradeStream()] [Closed] [future] and reconnect again [with Task.Delay]..");
                            SubscribeToAggTradeStream();
                    });
                }
            };
            aggTrade.Open();
        }
        #endregion

        #region Order Status [Websocket] [TEST]:
        public async void OrderStatus(string symbol = "BTCUSDT")
        {
           ///string depthEndpoint = $"https://fapi.binance.com/fapi/v1/openOrders";

            // Construct the request URL
            string endpoint = "/fapi/v1/openOrders";
            string timestamp = GenerateTimeStamp(DateTime.Now.ToUniversalTime());
            string queryString = $"symbol={symbol}&timestamp={timestamp}"; 
            string signature = GenerateSignature(queryString, BinanceOptionsApp.Models.Credential.Sec);
            string requestUrl = $"https://fapi.binance.com{endpoint}?{queryString}&signature={signature}";
            
            var path = new Uri(requestUrl);

            HttpResponseMessage response = await _httpClient.GetAsync(path);

            if (response.IsSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                //return JsonConvert.DeserializeObject<T>(result);
            }

            var e = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //"{\"code\":-1021,\"msg\":\"Timestamp for this request was 1000ms ahead of the server's time.\"}"
            var eCode = 0;
            string eMsg = "";
            if (e.IsValidJson())
            {
                try
                {
                    var i = JObject.Parse(e);

                    eCode = i["code"]?.Value<int>() ?? 0;
                    eMsg = i["msg"]?.Value<string>();
                }
                catch { }
            }

            if (eCode == -1021)
            {
                var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                long clientTime = (long)(DateTime.Now.ToUniversalTime() - epoch).TotalMilliseconds;
                BinanceServerTime sTime = await CallAsync<BinanceServerTime>(EndPoints.CheckServerTimeFuture);
                dTime = clientTime - sTime.serverTime;

                long clientTime2 = (long)(DateTime.Now.ToUniversalTime() - epoch).TotalMilliseconds;
                if ((clientTime2 - clientTime) < 1000)
                {
                    if (response.IsSuccessStatusCode)
                    {
                        response.EnsureSuccessStatusCode();
                        var result = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    }
                }
            }
        }
        #endregion

        #region Auxiliary methods:        
        private void UpdDepthData(string price, string volume, int type)
        {
            var key = decimal.Parse(price, CultureInfo.InvariantCulture);
            if (marketDepthList.ContainsKey(key))
            {
                if (volume == "0.000")
                    marketDepthList.TryRemove(key, out MarketDepthUpdateFuture val);
                else
                {
                    try
                    {
                        var found = marketDepthList[key];
                        if (found != null)
                        {
                            marketDepthList[key] = new MarketDepthUpdateFuture
                            {
                                Volume = decimal.Parse(volume, CultureInfo.InvariantCulture),
                                Type = type,
                                LastTimeUpd = DateTime.Now.ToString("mm:ss.fff")
                            };
                        }
                    }
                    catch
                    {
                    }
                }
            }
            else if (!marketDepthList.ContainsKey(key))
            {
                try
                {
                    if (volume == "0.000")
                    {
                    }
                    else
                    {
                        marketDepthList.TryAdd(key, new MarketDepthUpdateFuture
                        { Volume = decimal.Parse(volume, CultureInfo.InvariantCulture), Type = type, LastTimeUpd = DateTime.Now.ToString("mm:ss.fff") });
                    }
                }
                catch (Exception ex)
                {
                }
            }

        }

        public void FunStD(decimal SellPrice, decimal BuyPrice, decimal Ask, decimal Bid)
        {
            //int i = (int)(SellPrice * 100);
            //int ii = (int)(BuyPrice * 100);
            //int iAsk = (int)(Ask * 100);
            //int iBid = (int)(Bid * 100);

            //if (iAsk >= 0 && iAsk < marketDepthList.Count && i >= 0 && i < marketDepthList.Count && i > iAsk)
            //{
            //    var volumeRange = marketDepthList
            //        .Where(entry => int.Parse(entry.Key) >= iAsk && int.Parse(entry.Key) <= i && entry.Value.Type == 1 && entry.Value.Volume > 0)
            //        .Select(entry => entry.Value.Volume);

            //    if (volumeRange.Any())
            //    {
            //        meanSell = volumeRange.Average();
            //        double squaredDifferencesSum = volumeRange.Sum(volume => Math.Pow((double)(volume - meanSell), 2));
            //        double variance = squaredDifferencesSum / volumeRange.Count();
            //        standardDeviationSell = Math.Sqrt(variance);
            //    }
            //    else
            //    {
            //        // В діапазоні немає елементів
            //    }
            //}
            //else
            //{
            //    // Індекси виходять за межі масиву
            //}

            //if (iBid >= 0 && iBid < marketDepthList.Count && ii >= 0 && ii < marketDepthList.Count && iBid > ii)
            //{
            //    var volumeRange = marketDepthList
            //        .Where(entry => int.Parse(entry.Key) >= ii && int.Parse(entry.Key) <= iBid && entry.Value.Type == 2 && entry.Value.Volume > 0)
            //        .Select(entry => entry.Value.Volume);

            //    if (volumeRange.Any())
            //    {
            //        meanBuy = volumeRange.Average();
            //        double squaredDifferencesSum = volumeRange.Sum(volume => Math.Pow((double)(volume - meanBuy), 2));
            //        double variance = squaredDifferencesSum / volumeRange.Count();
            //        standardDeviationBuy = Math.Sqrt(variance);
            //    }
            //    else
            //    {
            //        // В діапазоні немає елементів
            //    }
            //}
            //else
            //{
            //    // Індекси виходять за межі масиву
            //}
        }

        public void AssignRankToRange(decimal SellPrice, decimal BuyPrice, decimal Ask, decimal Bid)
        {
            int iSellPrice = (int)(SellPrice * 100);
            int iBuyPrice = (int)(BuyPrice * 100);
            int iAsk = (int)(Ask * 100);
            int iBid = (int)(Bid * 100);

            // if (iAsk >= 0 && iAsk < marketDepthList.Count && iSellPrice >= 0 && iSellPrice < marketDepthList.Count && iSellPrice > iAsk)
            if (iAsk >= 0 && iSellPrice >= 0 && iSellPrice > iAsk)
            {
                var askVolumeRange = marketDepthList
                    .Skip(iSellPrice)//iAsk
                    .Take(1000) //iSellPrice - iAsk
                                // .Where(entry => entry.Value != null && int.TryParse(entry.Key, out int entryType) && entryType == 1 && entry.Value.Volume > 0)
                    .Where(entry => entry.Value != null)
                    .Select(entry => entry.Value.Volume)
                    .ToList();

                var sortedAskVolume = askVolumeRange.ToList();
                sortedAskVolume.Sort((a, b) => b.CompareTo(a));

                var askVolumeRangeList = askVolumeRange.ToList();
                int Sz = askVolumeRangeList.Count;
                for (int i = 0; i < askVolumeRangeList.Count; i++)
                {
                    int rank = sortedAskVolume.IndexOf(askVolumeRangeList[i]) + 1;
                    int index = iSellPrice + i;//(iSellPrice + i).ToString(); //

                    if (index >= 0 && index < marketDepthList.Count)
                    {
                        //marketDepthList[index.ToString()].RankAsk = rank;
                    }
                }
            }

            // if (iBid >= 0 && iBid < marketDepthList.Count && iBuyPrice >= 0 && iBuyPrice < marketDepthList.Count && iBid > iBuyPrice)
            if (iBid >= 0 && iBuyPrice >= 0 && iBid > iBuyPrice)
            {
                //var bidVolumeRange = marketDepthList
                //    .Skip(iBuyPrice)
                //    .Take(iBid - iBuyPrice)
                //    .Where(entry => entry.Value != null && int.TryParse(entry.Key, out int entryType) && entryType == 2 && entry.Value.Volume > 0)
                //    .Select(entry => entry.Value.Volume)
                //    .ToList();

                //var sortedBidVolume = bidVolumeRange.ToList();
                //sortedBidVolume.Sort((a, b) => b.CompareTo(a));

                //var bidVolumeRangeList = bidVolumeRange.ToList();
                //for (int i = 0; i < bidVolumeRangeList.Count; i++)
                //{
                //    int rank = sortedBidVolume.IndexOf(bidVolumeRangeList[i]) + 1;
                //    int index = iBuyPrice + i;
                //    if (index >= 0 && index < marketDepthList.Count)
                //    {
                //        marketDepthList[index.ToString()].RankBid = rank;
                //    }
                //}
            }
        }
        #endregion

        #region My func:
        public async Task MyFunc()
        {
            await Task.Delay(0);
            if (LastAskF != 0 && LastBidF != 0)
            {
                decimal SumA = 0, SumB = 0;

                var a = LastAskF;
                var b = LastBidF;
                var start = LastAskF + 1.2m;
                for (decimal i = start; i > LastBidF - 1.2m; i -= 0.01m)
                {
                    if (marketDepthList.ContainsKey(i))
                    {
                        var elem = marketDepthList[i];
                        if (elem != null)
                        {
                            if (elem.Type == 1 && elem.Volume > 0)
                            {
                                SumA += elem.Volume;

                                //  ForegroundColor = ConsoleColor.Red;
                                //WriteLine(" Ask: {0:F2} | Vol: {1}", i, marketDepthList[((int)price).ToString()].Volume);
                                //ResetColor();
                            }
                            else if (elem.Type == 2 && elem.Volume > 0)
                            {
                                SumB += elem.Volume;
                                // ForegroundColor = ConsoleColor.Green;
                                //WriteLine(" Bid: {0:F2} | Vol: {1}", i, marketDepthList[((int)price).ToString()].Volume);
                                //ResetColor();
                            }
                        }
                    }
                }
                //zz.SumAsk = SumA; zz.SumBid = SumB;
            }
        }
        #endregion

        #region Synchronization_Level_2()[]:

        void Synchronization_Level_2()
        {




        }








        #endregion

        #region FunDepthProba1 & FunDepthProba2:
        public int FunDepthProba1(int Tip)
        {
            int res = 0;

            if (LastAskF != 0 && LastBidF != 0)
            {
          
                int R_A = 0, R_B = 0;
                decimal SuppP = 0, SuppV = 0, ResisP = 0, ResisV = 0;

                var a = LastAskF;
                var b = LastBidF;
                var start = LastAskF + 100.01m;
                for (decimal i = start; i > LastBidF - 100.01m; i -= 0.01m)
                {
                    if (marketDepthList.ContainsKey(i))
                    {
                        var elem = marketDepthList[i];
                        if (elem != null)
                        {
                            if (Tip == 1 && elem.Type == 1 && elem.Volume > 0)
                            {
                                //SumA += elem.Volume;
                                if (elem.RankAsk < R_A || R_A == 0)
                                {
                                    R_A = elem.RankAsk;
                                    ResisP = i;
                                    ResisV = elem.Volume;
                                }


                            }
                            else if (Tip == 2 && elem.Type == 2 && elem.Volume > 0)
                            {
                                
                                if (elem.RankBid < R_B || R_B == 0)
                                {
                                    R_B = elem.RankBid;
                                    SuppP = i;
                                    SuppV = elem.Volume;
                                }

                            }
                        }
                    }
                }
                //if (Tip == 1) { zz.ResistancePrice = ResisP; zz.ResistanceVol = ResisV; res = R_A; }
                //if (Tip == 2) { zz.SupportPrice = SuppP; zz.SupportVol = SuppV; res = R_B; }

            }
            return (res);
        }

        public int FunDepthProba2(int Tip)
        {
            int res = 0;

            //if (LastAsk != 0 && LastBid != 0)
            //{
            //    int R_A = 0, R_B = 0;
            //    decimal SuppP = 0, SuppV = 0, ResisP = 0, ResisV = 0;

            //    var a = LastAsk;
            //    var b = LastBid;
            //    var start = LastAsk + 100.01m;

            //    foreach (var kvp in marketDepthList.Reverse())
            //    {
            //        decimal i = kvp.Key; // конвертація стрінгу в decimal
            //        var elem = kvp.Value;

            //        if (Tip == 1 && elem.Type == 1 && elem.Volume > 0)
            //        {
            //            if (elem.RankAsk < R_A || R_A == 0)
            //            {
            //                R_A = elem.RankAsk;
            //                ResisP = i;
            //                ResisV = elem.Volume;
            //            }
            //        }
            //        else if (Tip == 2 && elem.Type == 2 && elem.Volume > 0)
            //        {
            //            if (elem.RankBid < R_B || R_B == 0)
            //            {
            //                R_B = elem.RankBid;
            //                SuppP = i;
            //                SuppV = elem.Volume;
            //            }
            //        }
            //    }

            //    if (Tip == 1) { zz.ResistancePrice = ResisP; zz.ResistanceVol = ResisV; res = R_A; }
            //    if (Tip == 2) { zz.SupportPrice = SuppP; zz.SupportVol = SuppV; res = R_B; }
            //}

            return res;
        }
        #endregion
        #endregion

        #region Binance future's events [COIN-M] [API streams][WebSocket]:
        #region Partial Depth:
        public void PartialDepth_COIN_M(string symbol = "BTCUSD")
        {
            var webSocket = new WebSocket4Net.WebSocket($"wss://dstream.binance.com/stream?streams={symbol.ToLower()}@depth10@100ms");

            webSocket.Opened += (sender, e) => logger.LogInfo("WebSocket connected! Start Partial @depth10 [FUTURE Coin-M] process...");

            webSocket.DataReceived += (sender, e) =>
            {
                var msg = e.Data;
            };

            webSocket.EnableAutoSendPing = true;
            webSocket.AutoSendPingInterval = 180;

            webSocket.MessageReceived += (sender, e) =>
            {
                var depth = JsonConvert.DeserializeObject<PartialDepthFuture>(e.Message);
                var asks = depth.Data.Asks.OrderBy(p => p[0]).Select(o => o[0]).ToList();
                var bids = depth.Data.Bids.OrderByDescending(p => p[0]).Select(o => o[0]).ToList();
                var a = decimal.Parse(asks.First(), CultureInfo.InvariantCulture);
                var b = decimal.Parse(bids.First(), CultureInfo.InvariantCulture);
                OnTickFuture(new TickEventArgsFutures
                {
                    Ask = a,
                    Bid = b,
                    Symbol = symbol
                });
            };

            webSocket.Error += (sender, e) => logger.LogError($"WebSocket [PartialDepth_COIN_M()] [FUTURE Coin-M] error: {e.Exception.Message}");
            webSocket.Closed += (s, e) =>
            {
                Task.Delay(5000).ContinueWith(_ =>
                {
                    ReconectCount++;
                    if (ReconectCount > 5)
                    {
                        // Can not open WebSocket;
                        return;
                    }
                    logger.LogInfo(ViewId + " Reconect to WebSocket #");

                    // если в течении минуты нет реконектов, то сбрасываем счётчик реконектов
                    if (timerResetReconectCount != null) { timerResetReconectCount.Dispose(); }
                    timerResetReconectCount = new Timer(__ => { ReconectCount = 0; timerResetReconectCount.Dispose(); }, null, 60 * 1000, 5000);
                    logger.LogWarning($"WebSocket [PartialDepth_COIN_M()] [Closed] [FUTURE Coin-M] and reconnect again [during Task.Delay]..");
                    ConnectToDepthStream();
                });
                logger.LogWarning($"WebSocket [PartialDepth_COIN_M()] [Closed] [FUTURE Coin-M] and reconnect again..");
                ConnectToDepthStream();
            };
            webSocket.Open();
        }
        #endregion
        #endregion

        #region Closing Websocket Custom:
        private long startDepth = 0L, startBT = 0L, startAggTrd = 0L;
        private bool isCloseMDCustom = false, isCloseBTCustom = false, isCloseAggTrdCustom = false;
        WebSocket4Net.WebSocket depthWS = null, bookTikerWS = null, aggTrade = null;

        private void CloseWebsocket(string name)
        {
            switch (name)
            {
                case nameof(SubscribeBookTickerStream):
                    isCloseBTCustom = true;
                    bookTikerWS.Close();
                    break;
                case nameof(ConnectToDepthStream):
                    isCloseMDCustom = true;
                    depthWS.Close();
                    break;
                case nameof(SubscribeToAggTradeStream):
                    isCloseAggTrdCustom = true;
                    aggTrade.Close();
                    break;
            }
        }

        #endregion

        #region [CUSTOM MAMAGER SOCKET]:
        public class BinancePingPongManagerX
        {
            private WebSocket4Net.WebSocket _webSocket;
            private int _pingInterval = 3;
            private int _expectedPongDelay = 2;
            private readonly IConnectorLogger logger;

            public BinancePingPongManagerX(IConnectorLogger logger)
            {
                this.logger = logger;
            }

            public async void StartPingPong(WebSocket4Net.WebSocket webSocket, int pingInterval = 180, int expectedPongDelay = 600)
            {
                _webSocket = webSocket;
                _pingInterval = pingInterval;
                _expectedPongDelay = expectedPongDelay;

                while (_webSocket.State == WebSocket4Net.WebSocketState.Open)
                {
                    await Task.Delay(_pingInterval * 1000);

                    try
                    {
                        var pongReceivedTask = SendPingAndGetPong();

                        if (await Task.WhenAny(pongReceivedTask, Task.Delay(_expectedPongDelay * 1000)) != pongReceivedTask)
                        {
                            logger.LogInfo("(BinancePingPongManager.StartPingPong())=>Pong not received on time. Handle the disconnection or other logic here");
                            // Pong not received on time
                            // Handle the disconnection or other logic here
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions, e.g., WebSocket closed unexpectedly
                        //Console.WriteLine($"Exception: {ex.Message}");
                    }
                }
            }

            private async Task SendPingAndGetPong()
            {
                var pongReceivedTaskSource = new TaskCompletionSource<bool>();

                _webSocket.Send("ping");

                _webSocket.MessageReceived += (sender, args) =>
                {
                    if (args.Message.Contains("pong"))
                        pongReceivedTaskSource.TrySetResult(true);
                };

                await pongReceivedTaskSource.Task;
            }
        }


        #endregion

        #region [!!!!!!!!!!!!!!!!!!!!!!!!]:
        public class BinancePingPongManager
        {
            private WebSocket4Net.WebSocket _webSocket;
            private int _pingInterval = 3;
            private int _expectedPongDelay = 2;
            private readonly IConnectorLogger _logger;

            public BinancePingPongManager(IConnectorLogger logger)
            {
                _logger = logger;
            }

            public async Task StartPingPong(WebSocket4Net.WebSocket webSocket, int pingInterval = 180, int expectedPongDelay = 600)
            {
                _webSocket = webSocket;
                _pingInterval = pingInterval;
                _expectedPongDelay = expectedPongDelay;

                while (_webSocket.State == WebSocket4Net.WebSocketState.Open)
                {
                    await Task.Delay(_pingInterval * 1000);

                    try
                    {
                        var pingPayload = Guid.NewGuid().ToString(); // Генеруємо унікальний пінг-пейлод
                        var pongReceivedTask = SendPingAndGetPong(pingPayload);

                        if (await Task.WhenAny(pongReceivedTask, Task.Delay(_expectedPongDelay * 1000)) != pongReceivedTask)
                        {
                            _logger.LogInfo("(BinancePingPongManager.StartPingPong())=>Pong not received on time. Handle the disconnection or other logic here");
                            // Pong not received on time
                            // Handle the disconnection or other logic here
                        }
                    }
                    catch (Exception ex)
                    {
                        // Handle exceptions, e.g., WebSocket closed unexpectedly
                        _logger.LogError($"Exception: {ex.Message}");
                    }
                }
            }

            private async Task SendPingAndGetPong(string pingPayload)
            {
                var pongReceivedTaskSource = new TaskCompletionSource<bool>();

                _webSocket.Send($"{{\"ping\":{pingPayload}}}"); // Відправляємо пінг із унікальним пінг-пейлодом

                _webSocket.MessageReceived += (sender, args) =>
                {
                    if (args.Message.Contains("pong") && args.Message.Contains(pingPayload))
                        pongReceivedTaskSource.TrySetResult(true);
                };

                await pongReceivedTaskSource.Task;
            }
        }

        #endregion

        #region Websocket test [NOT USAGE YET]:
        private void WebsocketClientSender(string message)
        {
            string serverAddress = "ws://67.217.228.39:8080/echo"; // Потрібно замінити на реальну адресу сервера

            using (var ws = new WebSocketSharp.WebSocket(serverAddress))
            {
                // Запускаємо підключення до сервера
                ws.Connect();
                Console.WriteLine("Connected to server");

                // Введення повідомлень з консолі та їх відправлення серверу

                // Відправляємо повідомлення серверу
                ws.Send(message);
            }
        }

        private void WebsocketServerListener()
        {
            var server = new WebSocketServer("ws://0.0.0.0:8080");
            server.AddWebSocketService<Echo>("/echo");
            server.Start();
            logger.LogInfo("Server started. Press any key to exit...");
            Console.ReadKey(true);
            server.Stop();
        }

        public class Echo : WebSocketBehavior
        {
            protected override void OnMessage(MessageEventArgs e)
            {
                //bt = JsonConvert.DeserializeObject<BookTickerF>(e.Data);
               // Send(e.Data);
            }
        }

        private void GetOrderInfo()
        {
            //var client = new BinanceClient(new Binance.Net.Objects.Models.Futures.BinanceFuturesAccountAsset()//BinanceClientOptions()
            //{
            //    ApiCredentials = new ApiCredentials(apiKey, apiSecret),
            //});

            //// Fetch all open orders
            //var result = client.Spot.Order.GetOpenOrders("BTCUSDT");

            //if (result.Success)
            //{
            //    // Print information about each order
            //    foreach (var order in result.Data)
            //    {
            //        Console.WriteLine($"Symbol: {order.Symbol}, OrderId: {order.OrderId}, Type: {order.OrderType}, Status: {order.Status}, Quantity: {order.Quantity}, Price: {order.Price}");
            //    }
            //}
            //else
            //{
            //    Console.WriteLine($"Failed to get open orders: {result.Error}");
            //}
        }
        #endregion
    }
}


