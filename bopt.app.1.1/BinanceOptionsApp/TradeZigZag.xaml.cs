using System;
using Models.Algo;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Timers;
using Helpers.Extensions;
using System.Windows.Media;
using System.Windows.Threading;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Documents;
using MultiTerminal.Connections;
using System.Collections.Generic;
using MultiTerminal.Connections.Models;
using MultiTerminal.Connections.API.Spot;
using MultiTerminal.Connections.API.Future;
using Binance.Net;

using System.IO;
using Environment = System.Environment;
using MultiTerminal.Connections.Details.Binance;

namespace BinanceOptionsApp
{

    public partial class TradeZigZag : UserControl, IConnectorLogger, ITradeTabInterface
    {
        public static decimal Leg { get; set; }
        public static decimal SenseDist { get; set; }
        public static decimal Cluster { get; set; }
        public static decimal ClusterTS { get; set; }
        public static decimal SpikeParametr { get; set; }
        public static decimal KrockPruzgina { get; set; }
        public static decimal VolPruzgina { get; set; }
        public static string Symbol { get; set; }
        public static decimal wantProfit { get; set; }
        public static int existOrder { get; set; } = 0;
        public static DateTimeOffset existOrderTime { get; set; } = DateTimeOffset.Now;
        public static string Order { get; set; } = "0";
        public static DateTimeOffset OrderTime { get; set; } = DateTimeOffset.Now;
        public static int fight { get; set; } = 0;
        public static DateTimeOffset fightTime { get; set; } = DateTimeOffset.Now;
        public static int Stakan { get; set; } = 0;
        public static int StakanWeight { get; set; } = 0;
        public static DateTimeOffset StakanTime { get; set; } = DateTimeOffset.Now;
        public static int StakanLentaMove { get; set; } = 0;
        public static int Tide { get; set; } = 0;
        public static decimal TidePrice { get; set; } = 0;
        public static int TideWeight { get; set; } = 0;
        public static DateTimeOffset TideTime { get; set; } = DateTimeOffset.Now;// DateTimeOffset.Now;
        public static int SumLenta { get; set; } = 0;
        public static DateTimeOffset SumLentaTime { get; set; } = DateTimeOffset.Now;
        public static int SumLentaWeight { get; set; } = 0;
        public static int Lenta { get; set; } = 0;
        public static decimal LentaPrice { get; set; } = 0;
        public static DateTimeOffset LentaTime { get; set; } = DateTimeOffset.Now;
        public static int LentaWeight { get; set; } = 0;
        public static int Lentochka { get; set; } = 0;
        public static decimal LentochkaPrice { get; set; } = 0;
        public static DateTimeOffset LentochkaTime { get; set; } = DateTimeOffset.Now;
        public static int LentochkaWeight { get; set; } = 0;
        public static int OrderStep { get; set; } = 0;
        public static DateTimeOffset OrderStepTime { get; set; } = DateTimeOffset.Now;
        public static int OrderZig { get; set; } = 0;
        public static DateTimeOffset OrderZigTime { get; set; } = DateTimeOffset.Now;
        public static int rememberFonStepUP { get; set; } = 0;
        public static int rememberFonStepDN { get; set; } = 0;
        public static int FonStepByStepUP { get; set; } = 0;
        public static int FonStepByStepDN { get; set; } = 0;
        public static int StepByStepUP { get; set; } = 0;
        public static int StepByStepDN { get; set; } = 0;
        public static decimal StepByStepPrice { get; set; } = 0;
        public static int StepByStepLast { get; set; } = 0;
        public static DateTimeOffset StepByStepTime { get; set; } = DateTimeOffset.Now;
        public static int Pruzgina { get; set; } = 0;
        public static int Fon { get; set; } = 0;
        public static DateTimeOffset FonTime { get; set; } = DateTimeOffset.Now;
        public static int Natiag { get; set; } = 0;
        public static DateTimeOffset NatiagTime { get; set; } = DateTimeOffset.Now;
        public static int StakanImpulse { get; set; } = 0;
        public static DateTimeOffset StakanImpulseTime { get; set; } = DateTimeOffset.Now;
        public static decimal StakanImpulsePrice { get; set; } = 0;
        public static int StakanImpulseWeight { get; set; } = 0;
        public static int LentaPruzgina { get; set; } = 0;
        public static DateTimeOffset LentaPruzginaTime { get; set; } = DateTimeOffset.Now;
        public static int LentaPruzginaWeight { get; set; } = 0;
        public static string lastZZ { get; set; } = "";
        public static int wave { get; set; } = 0;
        public static decimal avDevUPzz { get; set; } = 0;
        public static decimal avDevDNzz { get; set; } = 0;
        public Models.TradeModel model;
        private ManualResetEvent threadStop;
        private ManualResetEvent threadStopped;
        private readonly object loglock = new object();
        private bool Pos = false, PosBuy = false, PosSell = false, CloseLongOk=false, CloseShortOk=false;
        private bool LimitBuy = false, LimitSell = false, LockSell=false, LockBuy=false, NeedLockSell=false, NeedLockBuy=false;
        private bool deleteLockBuy = false, deleteLockSell = false, deleteMainBuy = false, deleteMainSell = false;
        private string ticketBuy = "0", ticketSell = "0", ticketBuyLock = "0", ticketSellLock = "0", ticketCustomBuyLock = "0", 
            ticketCustomSellLock = "0", ticketCustomBuy = "0", ticketCustomSell = "0";
        private decimal avgGapBuy = 0, maxAvgGapBuy = 0, maxDeltaAvg = 0, priceDeltaAvg = 0, priceMaxDeltaAvg = 0;
        private decimal avgGapSell = 0, minAvgGapSell = 0, minDeltaAvg = 0, priceMinDeltaAvg = 0;
        private decimal lastDivergencePrice = 0;
        private decimal avgAvg = 0, pre_avgAvg = 0;
        private int stepUpAvg = 0, stepDnAvg = 0;
        private int gapArgument = 0, DivergenceUP = 0, DivergenceDN = 0;
        private int trend = 0;
        private decimal deviationBuy = 0, MaxDevBuy = 0, maxDeltaA = 0, maxDeltaB = 0, maxDeltaAv = 0;
        private decimal deviationSell = 0, MinDevSell = 0, minDeltaA = 0, minDeltaB = 0, minDeltaAv = 0;
        private decimal PreDeviationBuy = 0, PreDeviationSell = 0;
        private decimal PreDeltaAvg = 0;
        private decimal PreDeltaAvgA = 0;
        private decimal PreDeltaAvgB = 0;
        private int deviationUp = 0, deviationDn = 0, lastDeviationUp = 0, lastDeviationDn = 0, deltaAvgUp = 0, deltaAvgDn = 0, deltaAvgAUp = 0, deltaAvgADn = 0, deltaAvgBUp = 0, deltaAvgBDn = 0;
        private bool argumentUP = false, argumentDN = false;


        private DateTimeOffset argumentUPTime { get; set; } = DateTimeOffset.Now;
        private DateTimeOffset argumentDNTime { get; set; } = DateTimeOffset.Now;

        private List<decimal> GapBuyArr = new List<decimal>();
        private List<decimal> GapSellArr = new List<decimal>();
        private List<decimal> AvgGapArr = new List<decimal>();
        private decimal PreGapBuy { get; set; }
        private decimal PreGapSell { get; set; }

        private decimal PreAskS = 0, PreBidS = 0, PreAskF = 0, PreBidF = 0;
        private decimal AssetBalS { get; set; }
        private decimal CurrBalS { get; set; }
        public static decimal GAPbuy_q {get;set;}
        public static decimal GAPsell_q {get;set;}
        public static decimal GAPbuy_r { get; set; }
        public static decimal GAPsell_r { get; set; }
        public static decimal TP, SL;
        private DateTime TimeAskF, TimeBidF, TimeAskS, TimeBidS, PreTimeAskF, PreTimeBidF, PreTimeAskS, PreTimeBidS;
        private DateTime StrtInterval, BookInterval_1, BookInterval_2, BookInterval_3;
        private int delayAsk, delayBid;
        decimal V_1, V_2, VV_1, VV_2;
        double averageVolumeA = 0, averageVolumeB = 0, priceResistence = 0, priceSupport = 0;
        double volResistence = 0, volSupport = 0;
        private List<List<string>> Bids { get; set; }
        private List<List<string>> Asks { get; set; }

        public ZigZagSpot zigZagSpot;
        public ZigZagFuture zigZagFuture;

        public BinanceCryptoClient bsc = null;
        public BinanceCryptoClient bmc { get; set; } = null;
        public BinanceFutureClient BfcLeg1 { get; set; } = null;
        public BinanceFutureClient BfcLeg2 { get; set; } = null;

        //  private decimal GapBuy = 0.0m;
        // private decimal GapSell = 0.0m;

        private decimal MaxGapBuyA = 0.0m;
        private decimal MinGapSellA = 0.0m;
        private decimal MaxGapBuyB = 0.0m;
        private decimal MinGapSellB = 0.0m;

        public static decimal PriceMaxBuyGross { get; set; }
        public static decimal VolMaxBuyGross { get; set; }
        public static decimal PriceMaxSellGross { get; set; }
        public static decimal VolMaxSellGross { get; set; }

        private decimal ThresholdVolume = 1.0m;
        private decimal ThresholdVolume2 = 50.0m;

        public static AccountInfoFuture AccountInfoFuture { get; set; }

        private List<List<string>> BidsLeg1 { get; set; }
        private List<List<string>> AsksLeg1 { get; set; }
        private List<List<string>> BidsLeg2 { get; set; }
        private List<List<string>> AsksLeg2 { get; set; }

        private TimeAndSale_BidAsk PreTASM { get; set; }
        private AggTradeFuture PreTASF { get; set; }

        private IConnector _1LegConnector;
        public IConnector OneLegConnector
        {
            get { return _1LegConnector; }
            set { _1LegConnector = value; }
        }
        private IConnector _2LegConnector;

        private string leg1Type, leg2Type;

        //public static BinanceFutureClient LatencyArb = null;

        string swLogPath;
        string swDebugPath;
        string swQuotesPath;
        System.IO.FileStream fsData;

       // private DispatcherTimer timerEvent;
        


        // Test flag for open:
        bool isOpen = false;

        public TradeZigZag()
        {
            InitializeComponent();

            //new BinanceFutureClient(this, threadStop, new BinanceFutureConnectionModel { Key = null, Secret = null, Name = "Futures", AccountTradeType = AccountTradeType.SPOT }).CallMarketDepth(model.Leg1.Symbol); //bbbbb .SPOT
            //new BinanceCryptoClient(this, threadStop, new BinanceConnectionModel { Key = null, Secret = null, Name = "Spot", AccountTradeType = AccountTradeType.SPOT }).CallMarketDepth(model.Leg2.Symbol);
        }

        public void InitializeTab()
        {
            model = DataContext as Models.TradeModel;
            fast.InitializeProviderControl(model.Leg1, true);
            slow.InitializeProviderControl(model.Leg2, true);

            var spt1 = model.Leg1.Name.Split(new char[] { '[', ']' });
            var spt2 = model.Leg2.Name.Split(new char[] { '[', ']' });
            leg1Type = spt1[1];
            leg2Type = spt2[1];

            model.LogError = LogError;
            model.LogInfo = LogInfo;
            model.LogWarning = LogWarning;
            model.LogClear = LogClear;
            model.LogOrderSuccess = LogOrderSuccess;
            HiddenLogs.LogHeader(model);
        }

        #region Log's methods:
        public void LogOrderSuccess(string message)
        {
            Log(message, Colors.Orange, Color.FromRgb(255, 165, 0));
        }
        public void LogInfo(string message)
        {
            Log(message, Colors.White, Color.FromRgb(0x00, 0x23, 0x44));
        }
        public void LogError(string message)
        {
            Log(message, Color.FromRgb(0xf3, 0x56, 0x51), Color.FromRgb(0xf3, 0x56, 0x51));
        }
        public void LogWarning(string message)
        {
            Log(message, Colors.LightBlue, Colors.Blue);
        }
        public void LogClear()
        {
            logBlock.Text = ""; 
        }
        public void Log(string _message, Color color, Color dashboardColor)
        {
            string message = DateTime.Now.ToString("HH:mm:ss.ffffff") + "> " + _message + "\r\n";
            lock (loglock)
            {
                if (swLogPath != null)
                {
                    System.IO.File.AppendAllText(swLogPath, message);
                    Model.CommonLogSave(message);
                }
            }
            SafeInvoke(() =>
            {
                model.LastLog = _message;
                model.LastLogBrush = new SolidColorBrush(dashboardColor);
                Run r = new Run(message)
                {
                    Tag = DateTime.Now,
                    Foreground = new SolidColorBrush(color)
                };
                try
                {
                    while (logBlock.Inlines.Count > 250)
                    {
                        logBlock.Inlines.Remove(logBlock.Inlines.LastInline);
                    }
                }
                catch
                {

                }
                int count = logBlock.Inlines.Count;
                if (count == 0) logBlock.Inlines.Add(r);
                else
                {
                    logBlock.Inlines.InsertBefore(logBlock.Inlines.FirstInline, r);
                }
            });
        }
        public void SafeInvoke(Action action)
        {
            Dispatcher.BeginInvoke((Action)(() =>
            {
                if (!Model.Closing)
                {
                    action();
                }
            }));
        }
        #endregion

        #region RestoreNullCombo & EscapePath methods:
        public void RestoreNullCombo(ConnectionModel cm)
        {
            fast.RestoreNullCombo(cm);
            slow.RestoreNullCombo(cm);
        }

        private string EscapePath(string path)
        {
            char[] invalid = System.IO.Path.GetInvalidPathChars();
            foreach (var c in invalid)
            {
                path = path.Replace(c, ' ');
            }
            return path;
        }

        private void fast_Loaded(object sender, RoutedEventArgs e)
        {

        }
        #endregion

        #region Window's Event Handlers
        private void BuStart_Click(object sender, RoutedEventArgs e)
        {
            Start();
        }

        private void BuStop_Click(object sender, RoutedEventArgs e)
        {
            Stop(true);
        }

        private void LogClear_Click(object sender, RoutedEventArgs e)
        {
            LogClear();
        }

        private void TbOpenOrderType_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
        #endregion

        #region Start & Stop methods
        public void Start()
        {
            if (model.Started) return;
            model.Started = true;
            model.FeederOk = false;
            LogClear();
            HiddenLogs.LogHeader(model);

            model.Leg1.Symbol = fast.AssetTb.Text + fast.CurrencyTb.Text;
            model.Leg2.Symbol = slow.AssetTb.Text + slow.CurrencyTb.Text;

            Models.TradeModel.currencySpot = model.Leg1.SymbolCurrency;
            Models.TradeModel.currencyFuture = model.Leg2.SymbolCurrency;
            Models.TradeModel.fullSymbolFuture = model.Leg2.Symbol;


            new BinanceFutureClient(this, threadStop, new BinanceFutureConnectionModel { Key = null, Secret = null, Name = "Futures", AccountTradeType = AccountTradeType.SPOT }).CallMarketDepth(model.Leg1.Symbol); //bbbbb .SPOT
            new BinanceCryptoClient(this, threadStop, new BinanceConnectionModel { Key = null, Secret = null, Name = "Spot", AccountTradeType = AccountTradeType.SPOT }).CallMarketDepth(model.Leg2.Symbol);


            threadStop = new ManualResetEvent(false);
            threadStopped = new ManualResetEvent(false);
            new Thread(ThreadProc).Start();
            ///timerEvent.Start();
            Model.OnUpdateDashboardStatus();
        }

        public void Stop(bool wait)
        {
            if (!model.Started) return;
            threadStop.Set();
            if (wait)
            {
                threadStopped.WaitOne();
                threadStop.Dispose();
                threadStopped.Dispose();
            }
            model.Started = false;
            model.FeederOk = false;
            Model.OnUpdateDashboardStatus();
        }
        #endregion

        #region THread Process Method
        decimal MaxBa = 0, MaxSa = 0,  MaxBb = 0, MaxSb = 0;

        private void Socket_Btn_Click(object sender, RoutedEventArgs e)
        {
            new Socket_Wnd(BfcLeg1, model.Leg1.Symbol).ShowDialog();
        }

        long LastTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        int calcOpenOrderBuy = 0, calcOpenOrderSell = 0, calcCloseSell = 0, calcCloseBuy = 0; 
        long LastTime2 = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long timeOpenPos = 0;
        long timeClosePos = 0;// DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        DateTimeOffset timeOpenPos2 = DateTimeOffset.Now;
        DateTimeOffset timeBuyLimitExecuted = DateTimeOffset.Now;
        DateTimeOffset timeSellLimitExecuted = DateTimeOffset.Now;

        int indexSymb=0; decimal actualPosition = 0; decimal LastBuyPrice = 0, 
            LastSellPrice = 0, EntryPrice=0, lastCloseLong = 0, lastCloseShort = 0, TakeProfitS=0, TakeProfitB = 0, stopLoss = 0;
        decimal LockBuyPrice = 0, LockSellPrice = 0, LockBuyVolume = 0, LockSellVolume = 0;
        int ClientOrderId = 0;

        void ThreadProc()
        {
            Leg = model.Open.Leg;
            KrockPruzgina = model.Open.Koef1;
            VolPruzgina = model.Open.Koef2;
            SpikeParametr = 10;

            model.Leg1.InitView();
            model.Leg2.InitView();
            _1LegConnector = model.Leg1.CreateConnector(this, threadStop, model.SleepMs, Dispatcher, true, true);
            _2LegConnector = model.Leg2.CreateConnector(this, threadStop, model.SleepMs, Dispatcher, false, false);
            _1LegConnector.Tick += _1LegConnector_Tick;
            _2LegConnector.Tick += _2LegConnector_Tick;
            _1LegConnector.LoggedIn += OneLegConnector_LoggedIn;
            _2LegConnector.LoggedIn += TwoLegConnector_LoggedIn;

            model.LogInfo(model.Title + " logging in...");
            while (!threadStop.WaitOne(100))
            {
                if (_1LegConnector.IsLoggedIn && _2LegConnector.IsLoggedIn)
                {
                    model.LogInfo(model.Title + " logged in OK.");
                    break;
                }
            }
            if (!threadStop.WaitOne(0))
            {
                if (_1LegConnector.IsLoggedIn)
                {
                    On_1LegLogin();
                }
                if (_2LegConnector.IsLoggedIn)
                {
                    On_2LegLogin();
                }
            }

            #region Log process:
            if (model.Log)
            {
                string stime = DateTime.Now.ToString("yyyyMMddHHmmss");
                string logfolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\.logs";
                logfolder = System.IO.Path.Combine(logfolder, EscapePath(model.Title));
                try { System.IO.Directory.CreateDirectory(logfolder); }
                catch { }
                swLogPath = System.IO.Path.Combine(logfolder, "lg_" + stime + ".log");
                swDebugPath = System.IO.Path.Combine(logfolder, "db_" + stime + ".log");
                swQuotesPath = System.IO.Path.Combine(logfolder, "qu_" + stime + ".log");
            }
            else
            {
                swLogPath = null;
                swDebugPath = null;
                swQuotesPath = null;
            }
            if (model.SaveTicks)
            {
                string stime = DateTime.Now.ToString("yyyyMMddHHmmss");
                string datafolder = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\.data";
                datafolder = System.IO.Path.Combine(datafolder, EscapePath(model.Title));
                try
                {
                    System.IO.Directory.CreateDirectory(datafolder);
                }
                catch
                {
                }
                fsData = new System.IO.FileStream(datafolder + "\\" + stime + ".ZigZag", System.IO.FileMode.Create);
            }
            #endregion

            TimeSpan startTime = model.Open.StartTimeSpan();
            TimeSpan endTime = model.Open.EndTimeSpan();
            
            //public int order=0;
            //int noor = 0;//****
            while (!threadStop.WaitOne(model.SleepMs))
            {
                //var _1stLeg = BfcLeg1;
                //var _2stLeg = BfcLeg2;
                SenseDist = model.Open.SenseDist;
                Cluster = model.Open.Cluster;
                ClusterTS = model.Open.ClusterTS;

                //  Here you can write arbitrage strategy and algo:

                decimal Ask1 = 0, VolAsk1 = 0, Bid1 = 0, VolBid1 = 0, Ask2 = 0, VolAsk2 = 0, Bid2 = 0, VolBid2 = 0;
                decimal GapBuy = 0; bool OpenShort = false, OpenLong = false;
                decimal GapSell = 0; bool CloseAllBuy = false, CloseAllSell = false;
                bool CloseLong = false, CloseShort = false, Lock = false, urgently=false;

                if (model.Leg2.Ask > 0 && model.Leg1.Bid > 0)//book ev
                {
                    var askByIndexl1 = GetAskByIndex(0, true).Split(new char[] { ',' });
                    Ask1 = decimal.Parse(askByIndexl1[0], CultureInfo.InvariantCulture);
                    VolAsk1 = decimal.Parse(askByIndexl1[1], CultureInfo.InvariantCulture);
                    var bidByIndexl1 = GetBidByIndex(0, true).Split(new char[] { ',' });
                    Bid1 = decimal.Parse(bidByIndexl1[0], CultureInfo.InvariantCulture);
                    VolBid1 = decimal.Parse(bidByIndexl1[1], CultureInfo.InvariantCulture);

                    var askByIndexl2 = GetAskByIndex(0, false).Split(new char[] { ',' });
                    Ask2 = decimal.Parse(askByIndexl2[0], CultureInfo.InvariantCulture);
                    VolAsk2 = decimal.Parse(askByIndexl2[1], CultureInfo.InvariantCulture);
                    var bidByIndexl2 = GetBidByIndex(0, false).Split(new char[] { ',' });
                    Bid2 = decimal.Parse(bidByIndexl2[0], CultureInfo.InvariantCulture);
                    VolBid2 = decimal.Parse(bidByIndexl2[1], CultureInfo.InvariantCulture);
                    if ((DateTime.Now - BookInterval_2).TotalMilliseconds > 1023)
                    {
                        V_1 = VolAsk1 / VolBid1;
                        V_2 = VolAsk2 / VolBid2;
                        BookInterval_2 = DateTime.Now;
                    }
                    if ((DateTime.Now - BookInterval_3).TotalMilliseconds > 3057)
                    {
                        VV_1 = VolAsk1 / VolBid1;
                        VV_2 = VolAsk2 / VolBid2;
                        BookInterval_3 = DateTime.Now;
                    }

                }

                DateTimeOffset CurrTime = DateTimeOffset.Now;
                string formattedTime = CurrTime.ToString("HH:mm:ss.fff");

                

                ThresholdVolume = model.Open.Threshold;
                // ThresholdVolume = model.Open.Threshold2;
                decimal ko1 = 0.618m;// model.Open.Koef1;
                decimal OpnCrit = model.Open.Leg * ko1;

                if (bmc != null && bmc.timeAndSale != null)
                {
                    List<TimeAndSale_BidAsk> lentaList = bmc.timeAndSale;  // margin
                    if (lentaList != null && lentaList.Count != 0)
                    {
                        TimeAndSale_BidAsk lastM = lentaList[lentaList.Count - 1];
                        if (lastM != null)
                        {
                            if (PreTASM != lastM)
                            {
                                var AskM = lastM.Ask;
                                var BidM = lastM.Bid;
                                var BuyerIDM = lastM.BuyerID;
                                var DealTimeM = lastM.DealTime;
                                var EventDateM = lastM.EventDate;
                                var EventTimeM = lastM.EventTime;
                                var EventTypeM = lastM.EventType;
                                var IsBuyLimitM = lastM.IsBuyLimit;
                                var IsSellLimitM = lastM.IsSellLimit;
                                var PriceM = lastM.Price;
                                var SellerIDM = lastM.SellerID;
                                var SymbolM = lastM.Symbol;
                                var TicketM = lastM.Ticket;
                                var VolumeM = lastM.Volume;

                                if (VolumeM >= ThresholdVolume)
                                {

                                }

                                PreTASM = lastM;
                            }
                            //  model.LogError($"[MARGIN] => Ask: {AskM} | Bid: {BidM} | Volume: {VolumeM} | EventType: {EventTypeM}");

                            //model.LogInfo($"Ask: {Ask} | Bid: {Bid} | BuyerID: {BuyerID} | SellerID: {SellerID} | Price: {Price}");
                        }
                    }
                }

                if (BfcLeg1 != null && BfcLeg1.TasF != null)//future
                {
                    decimal LongVol = 0, ShortVol = 0;

                    var lentaFList = BfcLeg1.TasF;        // futures
                    if (lentaFList != null && lentaFList.Count != 0)
                    {
                        AggTradeFuture lastF = lentaFList.Last();
                        if (lastF != null)
                        {
                            if (PreTASF != lastF)
                            {
                                var Ask = lastF.data.Ask;
                                var Bid = lastF.data.Bid;
                                var AggTradeId = lastF.data.AggTradeId;
                                var IsMarketMaker = lastF.data.IsMarketMaker;
                                var EventDate = lastF.data.EventDate;
                                var EventTime = lastF.data.EventTime;
                                var EventType = lastF.data.EventType;
                                var LastTradeId = lastF.data.LastTradeId;
                                var TradeTime = lastF.data.TradeTime;
                                var Price = lastF.data.Price;
                                var FirstTradeId = lastF.data.FirstTradeId;
                                var Symbol = lastF.data.Symbol;
                                var Volume = lastF.data.Volume;

                                PreTASF = lastF;
                            }
                            //   model.LogOrderSuccess($"[FUTURE] => Ask: {Ask} | Bid: {Bid} | Volume: {Volume} | EventType: {EventType}");
                        }

                    }
                }

                if (model.Leg2.Bid != 0 && model.Leg1.Ask != 0)
                {
                    if (PreBidF != model.Leg1.Bid) { TimeBidF = DateTime.Now; PreBidF = model.Leg1.Bid; }

                    if (PreAskF != model.Leg1.Ask) { TimeAskF = DateTime.Now; PreAskF = model.Leg1.Ask; }

                    if (PreAskS != model.Leg2.Ask) { TimeAskS = DateTime.Now; PreAskS = model.Leg2.Ask; }

                    if (PreBidS != model.Leg2.Bid) { TimeBidS = DateTime.Now; PreBidS = model.Leg2.Bid; }

                    if (model.Leg2.Bid != 0 && model.Leg1.Ask != 0 && model.Leg2.Ask != 0 && model.Leg1.Bid != 0)
                    {
                        if (model.Leg2.Ask > model.Leg2.Bid && model.Leg1.Ask > model.Leg1.Bid)
                        {
                            GapBuy = model.Leg1.Ask - model.Leg2.Ask;
                            if(TimeAskF > TimeAskS)delayAsk = 1;
                            else delayAsk = -1;

                            GapSell = model.Leg1.Bid - model.Leg2.Bid;
                            if (TimeBidF > TimeAskS) delayBid = 1;
                            else delayBid = -1;
                        }

                        if (GapBuy != PreGapBuy)
                        {
                            GapBuyArr.Add(GapBuy);
                            PreGapBuy = GapBuy;
                        }

                        if (GapSell != PreGapSell)
                        {
                            GapSellArr.Add(GapSell);
                            PreGapSell = GapSell;
                        }

                        var useAlignment = model.UseAlignment; //  if(useAlignment) 
                        var useStop = model.UseStop;
                        var buyCnt = GapBuyArr.Count;
                        var sellCnt = GapSellArr.Count;
                        var Period = model.Open.PeriodAlignment;
                        decimal avg21, deltaAvg, deltaAvgA, deltaAvgB;
                        

                        if ((DateTime.Now - StrtInterval).TotalSeconds > 10)
                        {
                            if (sellCnt > Period) avgGapSell = GetAvgGapSell(Period);
                            else avgGapSell = GetAvgGapSell(sellCnt);

                            if (buyCnt > Period) avgGapBuy = GetAvgGapBuy(Period);
                            else avgGapBuy = GetAvgGapBuy(buyCnt);
                            decimal avg = (avgGapBuy + avgGapSell) / 2;

                            AvgGapArr.Add(avg);
                            pre_avgAvg = avgAvg;
                            avgAvg = GetAvgAvgGap(true);
                            if (avgAvg > pre_avgAvg) { stepUpAvg++; stepDnAvg = 0; }//if (trend == 1) trend = 0; 
                            else if (avgAvg < pre_avgAvg) { stepDnAvg++; stepUpAvg = 0;  }//if (trend == 2) trend = 0;

                            avg21 =  GetAvgAvgGap(false);
                            deltaAvg = avg - avgAvg;
                            deltaAvgA = avg21 - avgAvg;
                            deltaAvgB = avg - avg21;

                            decimal avPrice=(model.Leg1.Ask + model.Leg1.Bid)/ 2;
                            priceDeltaAvg = avPrice;
                            //---------------------------

                            if (deltaAvg > 0) { deltaAvgUp++; deltaAvgDn = 0; }
                            else if (deltaAvg < 0) { deltaAvgAUp = 0; deltaAvgADn++; }

                            if (deltaAvgA > 0) { deltaAvgAUp++; deltaAvgADn = 0; }
                            else if (deltaAvg < 0) { deltaAvgAUp = 0; deltaAvgADn++; }

                            if (deltaAvgB > 0) { deltaAvgBUp++; deltaAvgBDn = 0; }
                            else if (deltaAvgB < 0) { deltaAvgBUp = 0; deltaAvgBDn++; }
                            //---------------------------
                           // if (deltaAvgUp >= 5 && deltaAvgAUp >= 5 && deltaAvgBUp >= 5) { argumentUP = true; }

                           // if (deltaAvgDn >= 5 && deltaAvgADn >= 5 && deltaAvgBDn >= 5) { argumentDN = true; }


                            PreDeltaAvg = deltaAvg; PreDeltaAvgA = deltaAvgA; PreDeltaAvgB = deltaAvgB;
                            //---------------------------
                            if (deltaAvg > maxDeltaAv || maxDeltaAv==0) maxDeltaAv = deltaAvg;
                            if (deltaAvgA > maxDeltaA || maxDeltaA == 0) maxDeltaA = deltaAvgA;
                            if (deltaAvgB > maxDeltaB || maxDeltaB == 0) maxDeltaB = deltaAvgB;

                            if (deltaAvg < minDeltaAv || minDeltaAv == 0) minDeltaAv = deltaAvg;
                            if (deltaAvgA < minDeltaA || minDeltaA == 0) minDeltaA = deltaAvgA;
                            if (deltaAvgB < minDeltaB || minDeltaB == 0) minDeltaB = deltaAvgB;

                            int trndA = 0, trndAv = 0, trndB = 0;
                            if(Math.Abs(maxDeltaA) > Math.Abs(minDeltaA)) { trndA = 2; }
                            else if (Math.Abs(maxDeltaA) < Math.Abs(minDeltaA)) { trndA = 1; }
                            if (Math.Abs(maxDeltaAv) > Math.Abs(minDeltaAv)) { trndAv = 2; }
                            else if (Math.Abs(maxDeltaAv) < Math.Abs(minDeltaAv)) { trndAv = 1; }

                            //-----
                            if (trndA == 2 && trndAv == 2) trend = 2;//stepUpAvg >= 5 && avg21 > avgAvg
                            else if (trndA == 1 && trndAv == 1) trend = 1;//stepDnAvg >= 5 && avg21 < avgAvg
                            //-----


                            if (avgGapBuy > maxAvgGapBuy || maxAvgGapBuy == 0)
                            {
                                maxAvgGapBuy = avgGapBuy;
                                gapArgument = 2;
                            }

                            if(avgGapSell < minAvgGapSell || minAvgGapSell == 0)
                            {
                                minAvgGapSell = avgGapSell; 
                                gapArgument = 1;
                            }

                            StrtInterval = DateTime.Now;
                            if (lastZZ == "DN")
                            {
                                if (minDeltaAvg != 0 || priceMinDeltaAvg != 0) { minDeltaAvg = 0; priceMinDeltaAvg = 0; lastDivergencePrice = 0; DivergenceUP = 0; }
                              
                                if (deltaAvg > maxDeltaAvg || maxDeltaAvg == 0)
                                {
                                    maxDeltaAvg = deltaAvg;
                                    priceMaxDeltaAvg = avPrice;
                                    DivergenceUP = 0; lastDivergencePrice = 0;
                                }
                                
                                if(avPrice > priceMaxDeltaAvg && deltaAvg < maxDeltaAvg && (avPrice > lastDivergencePrice || lastDivergencePrice==0))
                                {
                                    lastDivergencePrice = avPrice;
                                    DivergenceUP++;
                                    WriteMessageToDesktopFile
                                   ($"Time: {formattedTime}|ZZ DN:{wave} curr up|maxDeltaAvg:{Math.Round(maxDeltaAvg, 1)}" +
                                   $"|price:{Math.Round(priceMaxDeltaAvg, 1)}|DivergenceUP:{DivergenceUP}|lastDivergencePrice:{Math.Round(lastDivergencePrice, 2)}|" +
                                   $"avPrice: {avPrice} ", "divergenceGap.txt");
                                }
                                
                                WriteMessageToDesktopFile
                                ($"Time: {formattedTime}|ZZ DN:{wave} curr up|delayA:{delayAsk}|GapBuy:{Math.Round(GapBuy,1)}|deviationBuy:{Math.Round(deviationBuy, 1)}" +
                                $"|avgGap:{Math.Round(avg, 1)}|avg21:{Math.Round(avg21, 2)}|avgAvg:{Math.Round(avgAvg, 2)}|avPrice: {avPrice}", "potokGap.txt");
                              
                                //--------------------------------------
                                WriteMessageToDesktopFile
                                ($"Time: {formattedTime}|ZZ DN:{wave} curr up|delayA:{delayAsk}|deviationBuy:{Math.Round(deviationBuy, 1)}|deltaAvg:{Math.Round(deltaAvg, 1)}|" +
                                 $"|deltaAvgSlow:{Math.Round(deltaAvgA, 1)}|deltaAvgB:{Math.Round(deltaAvgB, 1)}|avPrice: {avPrice}", "DeltaAvg.txt");

                            }
                            else if (lastZZ == "UP")
                            {
                                if (maxDeltaAvg !=0 || priceMaxDeltaAvg !=0) { maxDeltaAvg = 0; priceMaxDeltaAvg = 0; lastDivergencePrice = 0; DivergenceDN = 0; }
                                if (deltaAvg < minDeltaAvg || minDeltaAvg == 0)
                                {
                                    minDeltaAvg = deltaAvg;
                                    priceMinDeltaAvg = avPrice;
                                    DivergenceDN = 0; lastDivergencePrice = 0;
                                }
                                
                                if (avPrice < priceMinDeltaAvg && deltaAvg > minDeltaAvg && (avPrice < lastDivergencePrice || lastDivergencePrice == 0))
                                {
                                    lastDivergencePrice = avPrice;
                                    DivergenceDN++;
                                    WriteMessageToDesktopFile
                                    ($"Time: {formattedTime}|ZZ UP:{wave} curr dn|minDeltaAvg:{Math.Round(minDeltaAvg, 1)}" +
                                    $"|price:{Math.Round(priceMinDeltaAvg, 1)}|DivergenceDN:{DivergenceDN}|lastDivergencePrice:{Math.Round(lastDivergencePrice, 2)}|" +
                                    $"avPrice: {avPrice} ", "divergenceGap.txt");
                                }
                                WriteMessageToDesktopFile
                                ($"Time: {formattedTime}|ZZ UP:{wave} curr dn|delayB:{delayBid}|GapSell:{Math.Round(GapSell,1)}" +
                                $"|deviationSell:{Math.Round(deviationSell, 1)}|avgGap:{Math.Round(avg,1)}|avg21:{Math.Round(avg21, 2)}|avgAvg:{Math.Round(avgAvg, 2)}|avPrice: {avPrice} ", "potokGap.txt");

                                //---------------------------
                                WriteMessageToDesktopFile
                                ($"Time: {formattedTime}|ZZ UP:{wave} curr dn|delayB:{delayBid}|deviationSell:{Math.Round(deviationSell, 1)}|deltaAvg:{Math.Round(deltaAvg, 1)}|" +
                                 $"|deltaAvgSlow:{Math.Round(deltaAvgA, 1)}|deltaAvgB:{Math.Round(deltaAvgB, 1)}|avPrice: {avPrice}", "DeltaAvg.txt");

                            }
                            //-----------------

                        }

                        //*********************************
                        if (GapBuy != 0 && avgGapBuy != 0)
                        {
                            deviationBuy = GapBuy - avgGapBuy;
                            GAPbuy_q = avgGapBuy;// 
                            GAPbuy_r = GapBuy;// 
                            if (lastZZ == "DN")
                            {
                                if (deviationBuy >= avDevUPzz)
                                {
                                    MinDevSell = 0;
                                    if (deviationBuy > MaxDevBuy)
                                    {
                                        MaxDevBuy = deviationBuy;
                                        WriteMessageToDesktopFile
                                        ($"Time: {formattedTime}|ZZ DN:{wave} curr up|delayA:{delayAsk}|GapBuy:{Math.Round(GapBuy, 1)}|avgGapBuy:{Math.Round(avgGapBuy, 2)}|deviationB:{Math.Round(deviationBuy, 1)} |avDevUPzz:{Math.Round(avDevUPzz, 1)}| Ask: { model.Leg1.Ask}, " +
                                        $"Bid: {model.Leg1.Bid} ", "DeviationSignal.txt");
                                    }
                                }
                            }

                            if (TimeAskF > TimeAskS)
                            {
                                if(deviationBuy > PreDeviationBuy) { deviationUp++; deviationDn = 0; }
                                else if(deviationBuy < PreDeviationBuy) { deviationUp = 0; deviationDn++; }

                                if (deviationBuy > MaxGapBuyA || MaxGapBuyA == 0)
                                {
                                    MaxGapBuyA = deviationBuy;
                                    // model.LogInfo($"New Max DevBuy (+): {MaxGapBuyA} GapBuy:{GapBuy} avgGapBuy:{avgGapBuy} deviationBuy:{deviationBuy}");
                                    //model.LogInfo($"Ask1: {model.Leg1.Ask} Bid1:{model.Leg1.Bid} Ask2:{model.Leg2.Ask} Bid2:{model.Leg2.Bid}");
                                    model.MaxGapBuyA = Math.Round(MaxGapBuyA, model.Open.Point);
                                }
                            }
                            else
                            {
                                if (deviationBuy > PreDeviationBuy) { deviationUp = 0; deviationDn++; }
                                else if (deviationBuy < PreDeviationBuy) { deviationUp++; deviationDn = 0; }

                                if (deviationBuy > MaxGapBuyB || MaxGapBuyB == 0)
                                {
                                    MaxGapBuyB = deviationBuy;
                                    //model.LogInfo($"New Max DevBuy (-): {MaxGapBuyB} GapBuy:{GapBuy} avgGapBuy:{avgGapBuy} deviationBuy:{deviationBuy}");
                                    //   model.LogInfo($"Ask1: {model.Leg1.Ask} Bid1:{model.Leg1.Bid} Ask2:{model.Leg2.Ask} Bid2:{model.Leg2.Bid}");
                                    model.MaxGapBuyB = Math.Round(MaxGapBuyB, model.Open.Point);
                                }

                            }

                            if (deviationUp >= 5 || deviationDn >= 5)
                            {
                                if (lastDeviationUp != deviationUp || lastDeviationDn != deviationDn)
                                {
                                    WriteMessageToDesktopFile
                                   ($"Time: {formattedTime}|ZZ wave:{wave}|deviationUp:{deviationUp}|deviationDn:{deviationDn}|delayA:{delayAsk}|GapBuy:{Math.Round(GapBuy, 1)}|deviationB:{Math.Round(deviationBuy, 1)} || Ask: { model.Leg1.Ask}, " +
                                   $"Bid: {model.Leg1.Bid} ", "DeviationPotok.txt");
                                }
                                lastDeviationUp = deviationUp; lastDeviationDn = deviationDn;
                            }

                            PreDeviationBuy = deviationBuy;
                        }
                        if (GapSell != 0 && avgGapSell != 0)
                        {
                            deviationSell = GapSell - avgGapSell;
                            GAPsell_q = avgGapSell; //GapSell
                            GAPsell_r = GapSell; //GapSell
                            if (lastZZ == "UP")
                            {
                                if (deviationSell <= avDevDNzz)
                                {
                                    MaxDevBuy = 0;
                                    if (deviationSell < MinDevSell || MinDevSell == 0)
                                    {
                                        MinDevSell = deviationSell;
                                        WriteMessageToDesktopFile($"Time: {formattedTime}|ZZ UP:{wave} curr dn|delayB:{delayBid}|GapSell:{Math.Round(GapSell, 1)}|avgGapSell:{Math.Round(avgGapSell, 1)}|deviationS:{Math.Round(deviationSell, 1)} |avDevDNzz:{Math.Round(avDevDNzz, 1)}| Ask: { model.Leg1.Ask}, " +
                                         $"Bid: {model.Leg1.Bid} ", "DeviationSignal.txt");
                                    }
                                }
                            }
                            if (TimeAskF > TimeAskS)
                            {
                                if (deviationSell < MinGapSellA || MinGapSellA == 0)
                                {
                                    MinGapSellA = deviationSell;
                                    // model.LogInfo($"New Min DevSell (+): {MinGapSellA} GapSell:{GapSell} avgGapSell:{avgGapSell} deviationSell:{deviationSell}");
                                    // model.LogInfo($"Ask1: {model.Leg1.Ask} Bid1:{model.Leg1.Bid} Ask2:{model.Leg2.Ask} Bid2:{model.Leg2.Bid}");
                                    model.MinGapSellA = Math.Round(MinGapSellA, model.Open.Point);

                                }
                                if (deviationSell < OpnCrit * -1)
                                {
                                    if (deviationSell < MaxSa || MaxSa == 0)
                                    {
                                        MaxSa = deviationSell;
                                        model.LogInfo($" DevSELL (+): {deviationSell} avgGapSell:{avgGapSell} ");
                                    }
                                }

                                if (deviationSell > 0) { MaxSa = 0; MaxSb = 0; }
                            }
                            else
                            {
                                if (deviationSell < MinGapSellB || MinGapSellB == 0)
                                {
                                    MinGapSellB = deviationSell;
                                    // model.LogInfo($"New Min DevSell (-): {MinGapSellA} GapSell:{GapSell} avgGapSell:{avgGapSell} deviationSell:{deviationSell}");
                                    // model.LogInfo($"Ask1: {model.Leg1.Ask} Bid1:{model.Leg1.Bid} Ask2:{model.Leg2.Ask} Bid2:{model.Leg2.Bid}");
                                    model.MinGapSellB = Math.Round(MinGapSellB, model.Open.Point);

                                }
                                if (deviationSell < OpnCrit * -1)
                                {
                                    if (deviationSell < MaxSb || MaxSb == 0)
                                    {
                                        MaxSb = deviationSell;
                                        model.LogInfo($" DevSELL (-): {deviationSell} avgGapSell:{avgGapSell} ");
                                    }
                                }

                                if (deviationSell > 0) { MaxSa = 0; MaxSb = 0; }
                            }
                        }
                        //*********************************
                    }

                    // model.MaxGapBuyA = MaxGapBuyA;
                    //model.MinGapSellA = MinGapSellA;

                }

                var Lot = model.Leg1.Lot;
                //var futureLot = model.Leg2.Lot;

                int grid = model.Leg1.LotStep;
                //var futureLotStep = model.Leg2.LotStep;

                #region OnView [for Spot/Margin] (NOT USAGE HERE):
                //if (bmc.MarginAccount != null)
                //{
                //    var assetFound = bmc.MarginAccount.userAssets.FirstOrDefault(a => a.asset == model.Leg1.SymbolAsset);
                //    model.Leg1.CollateralMarginLevel = decimal.Parse(bmc.MarginAccount.collateralMarginLevel, CultureInfo.InvariantCulture);
                //    model.Leg1.MarginLevel = decimal.Parse(bmc.MarginAccount.marginLevel, CultureInfo.InvariantCulture);
                //    model.Leg1.TotalCollateralValueInUSDT = decimal.Parse(bmc.MarginAccount.totalCollateralValueInUSDT, CultureInfo.InvariantCulture);
                //    if (assetFound != null)
                //    {
                //        model.Leg1.Borrowed = assetFound.borrowed;
                //        model.Leg1.Free = assetFound.free;
                //        model.Leg1.Interest = assetFound.interest;
                //        model.Leg1.Locked = assetFound.locked;
                //        model.Leg1.NetAsset = assetFound.netAsset;
                //    }
                //}
                #endregion

                AssetBalS = model.Leg1.NetAsset;
                CurrBalS = model.Leg1.TotalCollateralValueInUSDT;
                var Profit = model.Leg1.TotalCrossUnPnl;
                var minProfit = model.Open.minProfit;
                var gapForOpen = model.Open.GapForOpen;
                var gapForClose = model.Open.GapForClose;
                 TP = model.Open.TakeProfit;
                 SL = model.Open.StopLoss;
                decimal paramA = model.Open.ParamA;
                decimal paramN = model.Open.ParamN;
                var dfg = model.Open.Cluster;
                // ThresholdVolume2 = model.Open.Threshold2;
                //model.Open.Threshold
                model.GapSell = GapSell;
                model.GapBuy = GapBuy;
                Symbol = model.Leg1.Symbol.ToUpper();
                //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                model.PriceMaxBuy = PriceMaxBuyGross;
                model.VolMaxBuy = VolMaxBuyGross;
                model.PriceMaxSell = PriceMaxSellGross;
                model.VolMaxSell = VolMaxSellGross;
                model.DeviationBuy = deviationBuy;// Output DeviationBuy & DeviationSell on Monitor:
                model.DeviationSell = deviationSell;
 
                //******************************************************************

                decimal wantPriceBuy = 0, wantPriceSell = 0;
                if ((DateTimeOffset.UtcNow.ToUnixTimeSeconds() - LastTime2) >= 3)
                {
                    EntryPrice = CheckPosition(model.Leg2.Ask, model.Leg2.Bid);

                    if (actualPosition > 0 && EntryPrice > 0)//model.Leg1.Ask+25
                    {
                        //// wantPriceSell = HFT("up", EntryPrice, Ask1, Bid1, "Cls", TP);//CheckPosition(EntryPrice, model.Leg2.Ask, model.Leg2.Bid);
                        //wantPriceSell = EntryPrice + wantProfit;
                        //// if (wantPriceSell > 0) wantPriceSell = Math.Round(wantPriceSell + avgGapSell, 1);
                        //if (wantPriceSell > 0) wantPriceSell = Math.Round(wantPriceSell, 1);
                        //else wantPriceSell = EntryPrice + minProfit;

                        //if (wantPriceSell < EntryPrice+50) { wantPriceSell = EntryPrice + minProfit; }
                    }

                    if (actualPosition < 0 && EntryPrice > 0)//model.Leg1.Bid-25
                    {
                        //// wantPriceBuy = HFT("dn", EntryPrice, Ask1, Bid1, "Cls", TP);//CheckPosition(EntryPrice, model.Leg2.Ask, model.Leg2.Bid);
                        ////  if(wantPriceBuy > 0)wantPriceBuy = Math.Round(wantPriceBuy + avgGapBuy, 1);
                        //wantPriceBuy = EntryPrice - wantProfit;
                        //if (wantPriceBuy > 0) wantPriceBuy = Math.Round(wantPriceBuy, 1);
                        //else wantPriceBuy = EntryPrice - minProfit;

                        //if (wantPriceBuy > EntryPrice - minProfit) { wantPriceBuy = EntryPrice - minProfit; }
                    }
                    if (actualPosition == 0 && EntryPrice == 0)
                    {
                        LockBuy = false; LockSell = false;
                        //   Nulling();
                    }

                     LastTime2 = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                }
                //=================================================================
                if (deviationBuy >= paramA && delayAsk == 1 || deviationBuy*-1 >= paramA && delayAsk == -1)
                { argumentUP = true; argumentUPTime = DateTimeOffset.Now; }
                else argumentUP = false;

                if (deviationSell * -1 >= paramA && delayBid == 1 || deviationSell >= paramA && delayBid == -1) 
                { argumentDN = true; argumentDNTime = DateTimeOffset.Now; }
                else argumentDN = false;

                //******************************************************************
                decimal minStep = model.Open.Threshold2;
                //var asd = model.Open.MinGapForOpen
                //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                decimal ValueTotal = Math.Abs(actualPosition);
                decimal lot = model.Leg1.Lot;
                bool ClosePartlyLong = false, ClosePartlyShort = false, addingLong = false, addingShort = false;
                string reasone = " ";
                //DateTimeOffset CurrTime  = DateTimeOffset.Now;
                //string formattedTime = CurrTime.ToString("HH:mm:ss.fff");
                //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                if (CloseLongOk) {LastBuyPrice = 0;}
                if (CloseShortOk) {LastSellPrice = 0;}
                if(!Pos && DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timeOpenPos > 10 && timeOpenPos > 0 && (LastBuyPrice >0 || LastSellPrice>0))
                { LastBuyPrice = 0;  LastSellPrice = 0; LimitBuy = false; LimitSell = false; }


                if (LimitBuy && model.Leg1.Ask < LastBuyPrice)  
                {
                   LimitBuy = false;
                   model.LogInfo($" BuyLimit executed, LastBuyPrice:{LastBuyPrice}");
                   timeBuyLimitExecuted = DateTimeOffset.Now;
                }
                if (LimitSell && model.Leg1.Bid > LastSellPrice)
                {
                   LimitSell = false;
                   model.LogInfo($" SellLimit executed, LastSellPrice:{LastSellPrice}");
                   timeSellLimitExecuted = DateTimeOffset.Now;
                }

                if (ValueTotal < calcOpenOrderBuy * lot && LimitBuy && (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timeOpenPos > 25 || model.Leg1.Ask - LastBuyPrice > 50))
                { deleteMainBuy = true; model.LogInfo($" delete Main Buy Limit, LastBuyPrice:{LastBuyPrice}"); }
                if (ValueTotal < calcOpenOrderSell * lot && LimitSell && (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timeOpenPos > 25 || LastSellPrice - model.Leg1.Bid > 50))
                { deleteMainSell = true; model.LogInfo($" delete Main Sell Limit, LastSellPrice:{LastSellPrice}"); }

                // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$  
                bool devArgument = false;
                if ((argumentDN && (CurrTime - argumentDNTime).TotalSeconds < 25) || (argumentUP && (CurrTime - argumentUPTime).TotalSeconds < 25)) devArgument = true;

                //&& (CurrTime - OrderZigTime).TotalSeconds < 60
                if (devArgument && deviationUp == 5 && OrderZig == 2 && lastZZ != "")
                {
                    if (actualPosition == 0 && LastBuyPrice == 0)
                    {
                        OpenLong = true;
                        WriteMessageToDesktopFile($"Time: {formattedTime} | buy | Lnt:{Lenta} Fon:{Fon} fight:{fight} StepByStepDN:{StepByStepDN} Stk:{Stakan} StkImp:{StakanImpulse} Prg:{Pruzgina} Natiag:{Natiag} Order={Order} Ask: {model.Leg1.Ask}, Bid: {model.Leg1.Bid}", "FiltrSignal.txt");
                        reasone = $"Gap Signal open long, OrdZig={OrderZig} Order={Order} StepByStepUP:{StepByStepUP} Stk:{Stakan} StkImp:{StakanImpulse} Lnt:{Lenta} Prg:{Pruzgina} Tide:{Tide} Natiag:{Natiag} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}";
                    }

                    if (actualPosition > 0 && (model.Leg1.Bid < LastBuyPrice - minStep && LastBuyPrice > 0) && calcOpenOrderBuy < grid)
                    {
                        OpenLong = true;
                        addingLong = true;
                        reasone = $"Gap Signal open adding long, OrdZig={OrderZig} Order={Order} StepByStepUP:{StepByStepUP}, stoploss={stopLoss} TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} StkImp:{StakanImpulse} Lnt:{Lenta} Prg:{Pruzgina} Natiag:{Natiag}";
                        deleteLockSell = true;
                    }

                    if (actualPosition < 0 && !CloseShortOk)
                    {
                        CloseShort = true;
                        reasone = $"Gap Signal close short, OrdZig={OrderZig} Order={Order} stoploss={stopLoss} TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} StkImp:{StakanImpulse} Lnt:{Lenta} Prg:{Pruzgina} Natiag:{Natiag} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}";
                        deleteLockBuy = true; urgently = true;
                    }
                }

                //&& (CurrTime - OrderZigTime).TotalSeconds < 60 
                if (devArgument && deviationDn == 5 && OrderZig == 1 && lastZZ != "")
                {
                    if (actualPosition == 0 && LastSellPrice == 0)
                    {
                        OpenShort = true;
                        WriteMessageToDesktopFile($"Time: {formattedTime} | sell | Lnt:{Lenta} Fon:{Fon} fight:{fight} StepByStepDN:{StepByStepDN} Stk:{Stakan} StkImp:{StakanImpulse} Prg:{Pruzgina} Natiag:{Natiag} Order={Order} Ask: {model.Leg1.Ask}, Bid: {model.Leg1.Bid}", "FiltrSignal.txt");
                        reasone = $"Gap Signal open short, OrdZig={OrderZig} Order={Order} StepByStepDN:{StepByStepDN} Stk:{Stakan} StkImp:{StakanImpulse} Lnt:{Lenta} Prg:{Pruzgina} Natiag:{Natiag} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}";
                    }

                    if (actualPosition < 0 && (model.Leg1.Ask > LastSellPrice + minStep && LastSellPrice > 0) && calcOpenOrderSell < grid)
                    {
                        OpenShort = true;
                        reasone = $"Gap Signal open adding short, OrdZig={OrderZig} Order={Order}  StepByStepDN:{StepByStepDN} , stoploss={stopLoss} TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} StkImp:{StakanImpulse} Lnt:{Lenta} Prg:{Pruzgina} Natiag:{Natiag}";
                        addingShort = true;
                        deleteLockBuy = true;
                    }

                    if (actualPosition > 0 && !CloseLongOk)
                    {
                        CloseLong = true;
                        reasone = $"Gap Signal close long, OrdZig={OrderZig} Order={Order} stoploss={stopLoss} TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} StkImp:{StakanImpulse} Lnt:{Lenta} Prg:{Pruzgina} Natiag:{Natiag} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}";
                        deleteLockSell = true; urgently = true;
                    }

                }
                // $$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$$ 

                //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@
                // ------ addin --------
                if (actualPosition > 0 && (model.Leg1.Bid < LastBuyPrice - minStep && LastBuyPrice > 0) && calcOpenOrderBuy < grid)
                {
                    if (devArgument && deviationUp == 5 && OrderZig == 22 && (CurrTime - OrderZigTime).TotalSeconds < 30)
                    {
                        OpenLong = true;
                        addingLong = true;
                        reasone = $"Signal open adding long, OrdZig={OrderZig} Order={Order} StepByStepUP:{StepByStepUP}, stoploss={stopLoss} TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} StkImp:{StakanImpulse} Lnt:{Lenta} Prg:{Pruzgina} Natiag:{Natiag}";
                        deleteLockSell = true; 
                    }
                }
                if (actualPosition < 0 && (model.Leg1.Ask > LastSellPrice + minStep && LastSellPrice > 0) && calcOpenOrderSell < grid)
                {
                    if (devArgument && deviationDn == 5  && OrderZig == 11 && (CurrTime - OrderZigTime).TotalSeconds < 30)
                    {
                        OpenShort = true;
                        reasone = $"Signal open adding short, OrdZig={OrderZig} Order={Order}  StepByStepDN:{StepByStepDN} , stoploss={stopLoss} TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} StkImp:{StakanImpulse} Lnt:{Lenta} Prg:{Pruzgina} Natiag:{Natiag}";
                        addingShort = true;
                        deleteLockBuy = true;
                    }
                }


                //@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@

                //if (StepByStepDN > 0) StepByStepLast = 1;
                //if (StepByStepUP > 0) StepByStepLast = 2;

                //-------------------- calc Take Profit ----------------

                if (actualPosition > 0 && ValueTotal > 0)
                {
                    TakeProfitB = EntryPrice + TP;
                    if(ValueTotal == calcOpenOrderBuy* lot && !deleteLockSell 
                        && (CurrTime - timeOpenPos2).TotalSeconds > 10) NeedLockSell = true; //timeBuyLimitExecuted 
                    wantPriceSell = TakeProfitB;//model.Leg1.Ask + 5;
                    stopLoss = EntryPrice - SL;

                    // reasone = $"TakeProfit Signal close long, stoploss={stopLoss} TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} Lnt:{Lenta} Prg:{Pruzgina} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}";

                }//(decimal)Math.Round(TP, 2);
                else if (actualPosition < 0 && ValueTotal > 0) 
                {
                    TakeProfitS = EntryPrice - TP;
                    if (ValueTotal == calcOpenOrderSell * lot && !deleteLockBuy
                        && (CurrTime - timeOpenPos2).TotalSeconds > 10) NeedLockBuy = true; //timeSellLimitExecuted 
                    wantPriceBuy = TakeProfitS;//model.Leg1.Bid - 5;
                    stopLoss = EntryPrice + SL;

                    // reasone = $"TakeProfit Signal close short, stoploss={stopLoss} TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} Lnt:{Lenta} Prg:{Pruzgina} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}";

                }
                //------------ check stop loss -----------------------------------------------------
                if (actualPosition > 0 && model.Leg1.Bid < stopLoss && stopLoss > 0 && EntryPrice > 0)
                {
                    if (!CloseLongOk) { CloseLong = true; deleteLockSell = true; urgently = true; }
                   reasone = $"Stop Signal close long, stoploss={stopLoss} TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} Lnt:{Lenta} Prg:{Pruzgina} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}";
                }
                if (actualPosition < 0 && model.Leg1.Ask > stopLoss && stopLoss > 0 && EntryPrice > 0)
                {
                    if (!CloseShortOk) { CloseShort = true; deleteLockBuy = true; urgently = true; }
                    reasone = $"Stop Signal close short, stoploss={stopLoss} TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} Lnt:{Lenta} Prg:{Pruzgina} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}";
                }

                //------------ check min profit -----------------------------------------------------

                if (actualPosition > 0)
                {
                    if (model.Leg1.Bid > EntryPrice + minProfit && EntryPrice > 0)
                    {
                        if (deviationDn == 5 && devArgument)//
                        {
                            CloseLong = true; urgently = false;
                            reasone = $"close long v1, modify for take min profit, TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} Lnt:{Lenta} Prg:{Pruzgina} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}";
                        }
                    }
                    if(devArgument && deviationDn > 0 && (OrderZig == 11 || OrderZig == 111) && (CurrTime - OrderZigTime).TotalSeconds < 30)
                    {
                       CloseLong = true; urgently = false;
                       reasone = $"close long v2, modify for close, OrdZig:{OrderZig} TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} Lnt:{Lenta}  Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}";
                    }
                    
                }

                if (actualPosition < 0)
                {
                    if (model.Leg1.Ask < EntryPrice - minProfit && EntryPrice > 0)
                    {
                        if (deviationUp == 5 && devArgument) // 
                        {
                            CloseShort = true; urgently = false;
                            reasone = $"close short v1, modify for take min profit, TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} Lnt:{Lenta} Prg:{Pruzgina} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}";
                        }
                    }

                    if (deviationUp > 0 && (OrderZig == 22 || OrderZig == 222) && (CurrTime - OrderZigTime).TotalSeconds < 30)
                    {
                        CloseShort = true; urgently = false;
                        reasone = $"close short v2, modify for close, OrdZig:{OrderZig} TotalVal={ValueTotal} pos:{actualPosition} EntrPrice={EntryPrice} Stk:{Stakan} Lnt:{Lenta} Prg:{Pruzgina} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}";
                    }
                }

                //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

                //------------------------------------
                    long id2 = 0; 

                if (LockBuy || LockSell)//LimitBuy || LimitSell || 
                {
                    //var of = BfcLeg1.PlacedOrdersUsd_M;
                    //int indx = of.Count(); 
                    //id2 = of[indx-1].OrderId;
                    //string ID = id2.ToString();

                    //var PPP = of[indx - 1].Price;
                    //if(LockBuy && PPP > 0 && model.Leg1.Ask > PPP + minStep*0.5m) 
                    //{

                    //}
                }
                
                //model.Leg2.Bfc.

                /*
                FunMarketTrade(OpenShort,OpenLong,CloseShort,CloseLong, ClosePartlyLong, ClosePartlyShort, 
                               addingLong, addingShort, ValueTotal, lot,grid, reasone);
                */

                FunLimitTrade(Lock, wantPriceBuy, wantPriceSell, OpenShort, OpenLong, CloseShort, CloseLong, urgently, ClosePartlyLong, ClosePartlyShort,
               addingLong, addingShort, ValueTotal, lot, grid, reasone);

            }

            // bfc.uvaga = true;

            _2LegConnector.Tick -= _2LegConnector_Tick;
            _1LegConnector.Tick -= _1LegConnector_Tick;
            _2LegConnector.LoggedIn -= TwoLegConnector_LoggedIn;
            _1LegConnector.LoggedIn -= OneLegConnector_LoggedIn;
            ConnectorsFactory.Current.CloseConnector(model.Leg2.Name, true);
            ConnectorsFactory.Current.CloseConnector(model.Leg1.Name, true);

            swQuotesPath = null;
            swDebugPath = null;
            swLogPath = null;
            if (fsData != null)
            {
                fsData.Flush();
                fsData.Dispose();
                fsData = null;
            }
            threadStopped.Set();
        }

        private void _2LegConnector_Tick(object sender, TickEventArgs e)
        {
            if (e.Symbol.ToLower() == model.Leg2.Symbol.ToLower())
            {
                //Asks = e.Asks;
                //Bids = e.Bids;
                AsksLeg2 = e.Asks;
                BidsLeg2 = e.Bids;

                model.Leg2.Bid = e.Bid;
                model.Leg2.Ask = e.Ask;
                model.Leg2.Time = DateTime.Now;
                if (model.Leg2.Ask == 0) model.Leg2.Ask = BinanceCryptoClient.LastAsk;
                if (model.Leg2.Bid == 0) model.Leg2.Bid = BinanceCryptoClient.LastBid;
            }
        }

        private void _1LegConnector_Tick(object sender, TickEventArgs e)
        {
            if (e.Symbol.ToLower() == model.Leg1.Symbol.ToLower())
            {
                //Asks = e.Asks;
                //Bids = e.Bids;
                AsksLeg1 = e.Asks;
                BidsLeg1 = e.Bids;

                var maxVolumeRow = AsksLeg1
              .Where(row => row.Count > 1 && double.TryParse(row[1], out _))
              .OrderByDescending(row => double.Parse(row[1]))
              .FirstOrDefault();

                if (maxVolumeRow != null && double.TryParse(maxVolumeRow[1], out double maxVolume))
                {
                    priceResistence = double.Parse(maxVolumeRow[0]);
                    volResistence = double.Parse(maxVolumeRow[1]);
                }

                var maxVolumeRow2 = BidsLeg1
               .Where(row => row.Count > 1 && double.TryParse(row[1], out _))
               .OrderByDescending(row => double.Parse(row[1]))
               .FirstOrDefault();

                if (maxVolumeRow2 != null && double.TryParse(maxVolumeRow2[1], out double maxVolume2))
                {
                    priceSupport = double.Parse(maxVolumeRow2[0]);
                    volSupport = double.Parse(maxVolumeRow2[1]);
                }

                if ((DateTime.Now - BookInterval_1).TotalMilliseconds > 2000)
                {
                    double totalVolumeAsks = 0;
                    foreach (List<string> row in AsksLeg1)
                    {
                        if (row.Count > 1 && double.TryParse(row[1], out double volume))
                        {
                            totalVolumeAsks += volume;
                        }
                    }
                    double averageVolumeA = 0;
                    if (AsksLeg1.Count > 0) averageVolumeA = totalVolumeAsks / AsksLeg1.Count;

                    double totalVolumeBids = 0;
                    foreach (List<string> row in BidsLeg1)
                    {
                        if (row.Count > 1 && double.TryParse(row[1], out double volume))
                        {
                            totalVolumeBids += volume;
                        }
                    }
                    averageVolumeB = 0;
                    if (BidsLeg1.Count > 0) averageVolumeB = totalVolumeBids / BidsLeg1.Count;

                    BookInterval_1 = DateTime.Now;
                }

                model.Leg1.Bid = e.Bid;
                model.Leg1.Ask = e.Ask;
                model.Leg1.Time = DateTime.Now;
            }
        }

        private decimal HFT(string tip,decimal EntryPrice, decimal Ask1, decimal Bid1, string Action, decimal TPr)
        {
            decimal res = 0; decimal StartPrice = 0, FinishPrice = 0; decimal parm = 0;
            if (Action == "Opn" && tip=="dn") parm = 50;
            if (Action == "Cls") parm = 100;

            try
            {
                if (model.Leg2 != null)
                {
                    StartPrice = EntryPrice - avgGapBuy; FinishPrice = EntryPrice - avgGapBuy + TPr;

                    var StakanA = BfcLeg2.marketDepthList
                                      .Where(pair => pair.Key >= StartPrice && pair.Key <= FinishPrice)
                                      .OrderBy(pair => pair.Key) // Сортуємо за зростанням значення ключа
                                      .ToDictionary(pair => pair.Key, pair => pair.Value.DeepCopy());

                    // Знаходимо ключ з найбільшим значенням Volume
                    decimal keyMaxVolumeA = 0; decimal volumeA = 0;
                    if (StakanA.Any())
                    {
                        keyMaxVolumeA = StakanA.OrderByDescending(pair => pair.Value.Volume).First().Key;
                        volumeA = StakanA[keyMaxVolumeA].Volume;
                    }

                    if (Action == "Opn" && tip == "up") parm = 50;
                    if (Action == "Cls") parm = 80;

                    StartPrice = EntryPrice-avgGapSell; FinishPrice = EntryPrice - avgGapSell - TPr;
                    var StakanB = BfcLeg2.marketDepthList
                                      .Where(pair => pair.Key <= StartPrice && pair.Key >= FinishPrice)
                                      .OrderBy(pair => pair.Key) // Сортуємо за зростанням значення ключа
                                      .ToDictionary(pair => pair.Key, pair => pair.Value.DeepCopy());

                    // Знаходимо ключ з найбільшим значенням Volume
                    decimal keyMaxVolumeB = 0; decimal volumeB = 0;
                    if (StakanB.Any())
                    {
                        keyMaxVolumeB = StakanB.OrderByDescending(pair => pair.Value.Volume).First().Key;
                        volumeB = StakanB[keyMaxVolumeB].Volume;
                    }
                    DateTimeOffset currentTime = DateTimeOffset.Now;
                    string formattedTime = currentTime.ToString("HH:mm:ss.fff");
                    string symb = TradeZigZag.Symbol;

                    if (tip == "up")
                    {
                        string FileName4 = "HFTFuture" + Symbol + ".txt";
                        if (Stakan == 2)
                            WriteMessageToDesktopFile($"Time: {formattedTime} | place BuyLimit | BuyPrice = {keyMaxVolumeB},  Vol = {volumeB} | SellPrice = {keyMaxVolumeA}, vol={volumeA} ", FileName4);
                        if (Action == "Opn") res = keyMaxVolumeB; //2
                        else res = keyMaxVolumeA;
                    }
                    if (tip == "dn")
                    {
                        string FileName4 = "HFTFuture" + Symbol + ".txt";
                        if (Stakan == 1)
                            WriteMessageToDesktopFile($"Time: {formattedTime} | place SellLimit | SellPrice = {keyMaxVolumeA},  Vol = {volumeA} | BuyPrice = {keyMaxVolumeB}, vol={volumeB} ", FileName4);
                        if (Action == "Opn") res = keyMaxVolumeA; //1
                        else res = keyMaxVolumeB;
                    }


                }
            }
            catch (Exception) { }

            return res;
        }

        private decimal GetAvgGapBuy(int count)
        {
            int startIdx = Math.Max(0, GapBuyArr.Count - count);
            if (GapBuyArr.Count >= count)
            {
                decimal sum = 0;
                for (int i = startIdx; i < GapBuyArr.Count; i++)
                    sum += GapBuyArr[i];
                if (count != 0)
                    return sum / count;
            }
            else
            {
                if (GapBuyArr.Count > 0)
                {
                    decimal sum = 0;
                    for (int i = 0; i < GapBuyArr.Count; i++)
                        sum += GapBuyArr[i];
                    return sum / GapBuyArr.Count;
                }
            }
            return 0.0m;
        }

        private decimal GetAvgGapSell(int count)
        {
            int startIdx = Math.Max(0, GapSellArr.Count - count);
            if (count > 0 && GapSellArr.Count >= count)
            {
                decimal sum = 0;
                for (int i = startIdx; i < GapSellArr.Count; i++)
                    sum += GapSellArr[i];
                return sum / count;
            }
            else
            {
                if (GapSellArr.Count > 0)
                {
                    decimal sum = 0;
                    for (int i = 0; i < GapSellArr.Count; i++)
                        sum += GapSellArr[i];
                    return sum / GapSellArr.Count;
                }
            }
            return 0.0m;
        }

        private decimal GetAvgAvgGap(bool allarr)
        {
            if (allarr)
            {
                if (AvgGapArr.Count == 0)
                    return 0; // повертаємо 0, якщо список порожній

                return AvgGapArr.Average();
            }
            else
            {
                int count = AvgGapArr.Count;
                int elementsToTake = Math.Min(21, count);

                if (count == 0)
                    return 0; // повертаємо 0, якщо список порожній

                return AvgGapArr.Skip(count - elementsToTake).Average();
            }
        }

        private string GetAskByIndex(int index, bool is1stLeg)
        {
            var Asks_ = is1stLeg ? AsksLeg1 : AsksLeg2;
            if (Asks_ != null)
                if (Asks_.Count != 0 && index < 20)
                {
                    var foundAsk = Asks_[index];
                    return $"{foundAsk[0]},{foundAsk[1]}";
                }
            return "0";
        }

        private string GetBidByIndex(int index, bool is1stLeg)
        {
            var Bids_ = is1stLeg ? BidsLeg1 : BidsLeg2;
            if (Bids_ != null)
                if (Bids_.Count != 0 && index < 20)
                {
                    var foundBid = Bids_[index];
                    return $"{foundBid[0]},{foundBid[1]}";
                }
            return "0";
        }

        void Depo()
        {
          
            /*
            if (calcOpenOrderSell > 0) //PosSell
            {
                if (CloseSell)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Close Sell Pos, deviationBuy: {deviationBuy} Bid: {model.Leg1.Bid}");
                        model.LogInfo($"Support: {priceSupport} VolS:{volSupport} Resistence: {priceResistence} VolR:{volResistence}");
                        model.LogInfo($"Ask1: {Ask1} Vol:{VolAsk1} Bid1: {Bid1} Vol:{VolBid1}");
                        model.LogInfo($"Ask2: {Ask2} Vol:{VolAsk2} Bid2: {Bid2} Vol:{VolBid2}");

                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                        if (OpenPos(model.Leg1.Symbol, model.Leg1.Bid, lot, "1Leg",
                        FillPolicy.GTC, OrderSide.Buy, GapBuy, OrderType.Limit))
                        {
                            calcOpenOrderSell--;
                            model.LogInfo($"Close Sell Pos ");
                            CloseSell = false;
                            // Pos = false; 
                        }
                        else
                        {
                            // Pos = true; PosBuy = true;
                        }
                    }
                }
                else if (CloseAllSell)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Close All Sell Pos by market");
                        model.LogInfo($"Support: {priceSupport} VolS:{volSupport} Resistence: {priceResistence} VolR:{volResistence}");
                        model.LogInfo($"Ask1: {Ask1} Vol:{VolAsk1} Bid1: {Bid1} Vol:{VolBid1}");
                        model.LogInfo($"Ask2: {Ask2} Vol:{VolAsk2} Bid2: {Bid2} Vol:{VolBid2}");

                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                        if (OpenPos(model.Leg1.Symbol, model.Leg1.Bid, ValueTotal, "1Leg",
                        FillPolicy.GTC, OrderSide.Buy, GapBuy, OrderType.Market))
                        {
                            calcOpenOrderSell = 0;
                            model.LogInfo($"Shuxer! Close ALL Sell Pos by market");
                            CloseAllSell = false;
                            // Pos = false; 
                        }
                        else
                        {
                            // Pos = true; PosBuy = true;
                        }
                    }
                }
                else if (OpenShort && calcOpenOrderSell < grid)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Open Sell Pos, deviationSell: {deviationSell} Ask: {model.Leg1.Ask}");
                        model.LogInfo($"Support: {priceSupport} VolS:{volSupport} Resistence: {priceResistence} VolR:{volResistence}");

                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                        decimal prc = model.Leg1.Ask;

                        if (OpenPos(model.Leg1.Symbol, prc, lot, "1Leg",
                                FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Limit))
                        {
                            LastSellPrice = prc;
                            model.LogInfo($"Elapsted Open Sell (1Leg)");
                            OpenShort = false; //Pos = true; PosSell = true; 
                            WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 1, Open Sell  |  | Ask: { model.Leg1.Ask}, " +
                            $"Bid: {model.Leg1.Bid}", "SimulatorTrade.txt");
                            calcOpenOrderSell++;
                        }
                        else
                        {
                            // Pos = false; PosSell = false;
                        }
                    }
                }

            }
            else if (calcOpenOrderBuy > 0)
            {
                if (CloseBuy)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Close Buy Pos");
                        model.LogInfo($"Support: {priceSupport} VolS:{volSupport} Resistence: {priceResistence} VolR:{volResistence}");
                        model.LogInfo($"Ask1: {Ask1} Vol:{VolAsk1} Bid1: {Bid1} Vol:{VolBid1}");
                        model.LogInfo($"Ask2: {Ask2} Vol:{VolAsk2} Bid2: {Bid2} Vol:{VolBid2}");

                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                        if (OpenPos(model.Leg1.Symbol, model.Leg1.Ask, lot, "1Leg",
                            FillPolicy.GTC, OrderSide.Sell, GapSell, OrderType.Limit))
                        {
                            calcOpenOrderBuy--;
                            model.LogInfo($"Close Buy Pos ");
                            CloseBuy = false;//PosBuy = false; 
                        }
                        else
                        {
                            //Pos = true; PosBuy = true;
                        }
                    }
                }
                else if (CloseAllBuy)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Close ALL Buy Pos by market");
                        model.LogInfo($"Support: {priceSupport} VolS:{volSupport} Resistence: {priceResistence} VolR:{volResistence}");
                        model.LogInfo($"Ask1: {Ask1} Vol:{VolAsk1} Bid1: {Bid1} Vol:{VolBid1}");
                        model.LogInfo($"Ask2: {Ask2} Vol:{VolAsk2} Bid2: {Bid2} Vol:{VolBid2}");

                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                        if (OpenPos(model.Leg1.Symbol, model.Leg1.Ask, ValueTotal, "1Leg",
                            FillPolicy.GTC, OrderSide.Sell, GapSell, OrderType.Market))
                        {
                            calcOpenOrderBuy = 0;
                            model.LogInfo($"Shuxer! Close ALL Buy Pos by market");
                            CloseAllBuy = false;//PosBuy = false; 
                        }
                        else
                        {
                            //Pos = true; PosBuy = true;
                        }
                    }
                }
                else if (OpenLong && calcOpenOrderBuy < grid)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Open BuyLimit, Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}");
                        model.LogInfo($"Support: {priceSupport} VolS:{volSupport} Resistence: {priceResistence} VolR:{volResistence}");

                        long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                        decimal prc = model.Leg1.Bid;

                        if (OpenPos(model.Leg1.Symbol, prc, lot, "1Leg",
                            FillPolicy.GTC, OrderSide.Buy, deviationBuy, OrderType.Limit))
                        {
                            LastBuyPrice = prc;
                            model.LogInfo($"Elapsted Open BuyLimit ");
                            OpenLong = false; //Pos = true; PosBuy = true;

                            WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 2, Open Buy |  | Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid}", "SimulatorTrade.txt");
                            calcOpenOrderBuy++;
                        }
                        else
                        {
                            //  Pos = false; PosBuy = false;
                        }
                    }
                }

            }
            */
        }

        private decimal CheckPosition(decimal Ask1, decimal Bid1)//decimal EntryPrice, 
        {
            decimal res = 0.0m; decimal WantPriceSell = 0.0m, WantPriceBuy=0.0m;

            if (BfcLeg1 != null)
            {
                    _ = BfcLeg1.GetBalance(model.Leg1.Symbol);

                    // Виконати код, якщо AccountInfoFuture не є null
                    if (AccountInfoFuture != null)
                    {
                        var positions = AccountInfoFuture.Positions;

                        if (positions[indexSymb].Symbol != model.Leg1.Symbol.ToUpper())
                        {
                            for (int eu = 0; eu < positions.Count; eu++)
                            {
                                string ss = positions[eu].Symbol;

                                if (ss == model.Leg1.Symbol.ToUpper())
                                {
                                    if (decimal.TryParse(positions[eu].PositionAmt, out actualPosition))
                                    {
                                        if (actualPosition != 0) Pos = true;
                                        else 
                                        { Pos = false; CloseLongOk = false; CloseShortOk = false;
                                          
                                        }

                                        if (actualPosition > 0 && calcOpenOrderBuy <= 0) calcOpenOrderBuy = 1;
                                        else if (actualPosition < 0 && calcOpenOrderSell <= 0) calcOpenOrderSell = 1;
                                    }

                                    if (decimal.TryParse(positions[eu].EntryPrice, out EntryPrice))
                                    {
                                        if (EntryPrice == 0) { }
                                        else
                                        {
                                            res = EntryPrice;
                                            // if (actualPosition > 0) { TakeProfitB = EntryPrice + 300; stopLoss = EntryPrice - 300; }//(decimal)Math.Round(TP, 2);
                                            //else if (actualPosition < 0) { TakeProfitS = EntryPrice - 300; stopLoss = EntryPrice + 300; }
                                            // (decimal)Math.Round(TP, 2);
                                        }
                                    }

                                    indexSymb = eu;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (decimal.TryParse(positions[indexSymb].PositionAmt, out actualPosition))
                            {
                                if (actualPosition == 0)
                                { Pos = false; CloseLongOk = false; CloseShortOk = false; calcOpenOrderSell = 0; calcOpenOrderBuy = 0;} 
                                else
                                {
                                    Pos = true;
                                    if (actualPosition > 0 && calcOpenOrderBuy <= 0) calcOpenOrderBuy = 1;
                                    else if (actualPosition < 0 && calcOpenOrderSell <= 0) calcOpenOrderSell = 1;
                                }
                            }

                            if (decimal.TryParse(positions[indexSymb].EntryPrice, out EntryPrice))
                            {
                                if (EntryPrice == 0) { }
                                else
                                {
                                // if (actualPosition > 0) { TakeProfitB = EntryPrice + 300; stopLoss = EntryPrice - 300; }// (decimal)Math.Round(TP, 2);
                                //else if (actualPosition < 0) { TakeProfitS = EntryPrice - 300; stopLoss = EntryPrice + 300; }// (decimal)Math.Round(TP, 2);
                                 res = EntryPrice;
                                }
                            }

                        }
                    }
            }

            if (actualPosition > 0)
                {
                   // LimitBuy = false;
                    if (LockSellVolume > 0 && LockSellVolume != actualPosition)
                    {
                      //  deleteLockSell = true;
                    }
                    // Lock = true;

                    //res = Math.Round(WantPriceSell + avgGapSell, 1);
                }

            if (actualPosition < 0)
                {
                  //  LimitSell = false;
                    if (LockBuyVolume > 0 && LockBuyVolume != Math.Abs(actualPosition))
                    {
                      //  deleteLockBuy = true;
                    }
                    //Lock = true;
                    //WantPriceBuy= HFT("dn", EntryPrice, Ask1, Bid1, "Cls");
                    //res = Math.Round(WantPriceBuy + avgGapBuy, 1);

                }

            if (actualPosition == 0)// && (LastBuyPrice >0 || LastSellPrice>0)
                {
                    //////calcOpenOrderSell = 0; LastSellPrice = 0;TakeProfitB = 0; TakeProfitS = 0; 
                    //////calcOpenOrderBuy = 0; LastBuyPrice = 0; stopLoss = 0;
                    //////lastCloseShort = 0; lastCloseLong = 0; calcCloseBuy = 0; calcCloseSell = 0;
                    
                   //  var tempo = HFT("dn", Ask1, Bid1, "Cls");
                }

            return res;
        }

        private void Nulling()
        {
            calcOpenOrderSell = 0; LastSellPrice = 0; TakeProfitB = 0; TakeProfitS = 0;
            calcOpenOrderBuy = 0; LastBuyPrice = 0; stopLoss = 0; EntryPrice = 0;
            lastCloseShort = 0; lastCloseLong = 0; calcCloseBuy = 0; calcCloseSell = 0;
            LockBuy = false; LockSell = false; deleteLockBuy = false; deleteLockSell = false;
        }


     #region [Fun TRADE LIMIT:]

        private void FunLimitTrade(bool Lock, decimal wantPriceBuy, decimal wantPriceSell, bool OpenShort, bool OpenLong, bool CloseShort, bool CloseLong, bool urgently,
                                    bool ClosePartlyLong, bool ClosePartlyShort, bool addingLong, bool addingShort, decimal ValueTotal, decimal lot, int grid, string reasone)
        {
            //**************** TRADE *******************************************
            if (Pos)
            {
                if (NeedLockBuy && !CloseShortOk && !LockBuy && actualPosition < 0 && wantPriceBuy > 0 && EntryPrice > 0)// && calcCloseSell <= calcOpenOrderSell Lock && 
                { // && wantPriceBuy <= EntryPrice - 25
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try place lock BuyLimit, Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask} wantPrice:{wantPriceBuy} EntrPrice:{EntryPrice}");
                        long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                        decimal tickt = 0;
                        decimal prc = Math.Round(wantPriceBuy, 1);
                        decimal lt = calcOpenOrderSell * lot;//ValueTotal
                        var result = OpenPos(model.Leg1.Symbol, prc, lt, "1Leg", FillPolicy.GTC, OrderSide.Buy, deviationBuy, OrderType.Limit);
                        ticketBuyLock = result.Id;

                        if (decimal.TryParse(ticketBuyLock, out tickt))
                        {
                            if (tickt != 0)
                            {
                                Lock = false;
                                calcCloseSell++;
                                lastCloseShort = prc;
                                //calcOpenOrderSell--;
                                ClientOrderId++;
                                ticketCustomBuyLock = ClientOrderId.ToString();
                                LockBuyPrice = prc;
                                LockBuyVolume = ValueTotal;
                                model.LogInfo(reasone);
                                model.LogInfo($"Placed lock BuyLimit, volume:{ValueTotal} ticketCustom= {ticketCustomBuyLock}, ticket= {ticketBuyLock} ");
                                LockBuy = true;
                                NeedLockBuy = false;
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Lock Buy || Ask: { model.Leg1.Ask}, " +
                                  $"Bid: {model.Leg1.Bid} | placed price={prc}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                //  Pos = false; PosBuy = false;
                            }
                        }
                    }
                }
                if (NeedLockSell && !CloseLongOk && !LockSell && actualPosition > 0 && wantPriceSell > 0 && EntryPrice > 0)// && calcCloseBuy <= calcOpenOrderBuy
                { // && wantPriceSell >= EntryPrice + 25
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try place lock SellLimit, Bid: {model.Leg1.Bid} Ask:{model.Leg1.Ask} wantPrice:{wantPriceSell}  EntrPrice:{EntryPrice}");

                        long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                        decimal tickt = 0; 
                        decimal prc = Math.Round(wantPriceSell, 1);
                        decimal lt = calcOpenOrderBuy * lot;//ValueTotal
                        var result = OpenPos(model.Leg1.Symbol, prc, lt, "1Leg", FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Limit);
                        ticketSellLock = result.Id;

                        if (decimal.TryParse(ticketSellLock, out tickt))
                        {
                            if (tickt != 0)
                            {
                                Lock = false;
                                calcCloseBuy++;
                                lastCloseLong = prc;
                                //calcOpenOrderBuy--;
                                ClientOrderId++;
                                ticketCustomSellLock = ClientOrderId.ToString();
                                LockSellPrice = prc;
                                LockSellVolume = ValueTotal;
                                model.LogInfo($"Placed lock SellLimit, volume:{ValueTotal} ticketCustom= {ticketCustomSellLock}, ticket= {ticketSellLock} ");
                                model.LogInfo(reasone);
                                LockSell = true;
                                NeedLockSell = false;
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Lock Sell || Ask: { model.Leg1.Ask}, " +
                                    $"Bid: {model.Leg1.Bid} | placed price={prc}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                //  Pos = false; PosBuy = false;
                            }
                        }
                    }
                }

                if (deleteLockBuy && LockBuy)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Delete lock Buy order, ticket = {ticketBuyLock} ticketCustom = {ticketCustomBuyLock}");
                        model.LogInfo(reasone);
                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                        var of = BfcLeg1.PlacedOrdersUsd_M;
                        int indx = of.Count();
                        var id2 = of[indx - 1].OrderId;
                        string ID = id2.ToString();
                        var side = of[indx - 1].Side;
                        if (side == "BUY")
                        {
                            if (OrderDelete(model.Leg1.Symbol, OrderSide.Buy, OrderType.Limit, "0", ID, ticketCustomBuyLock))//(model.Leg1.Symbol, prc, lot, "1Leg", //FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Limit))
                            {
                                deleteLockBuy = false;
                                LockBuy = false;
                                model.LogInfo($"Delete lock BuyLimit (1Leg)");
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Delete lock BuyLimit || Ask: {model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                deleteLockBuy = false;
                                Thread.Sleep(5000);
                            }
                        }

                    }
                }
                if (deleteLockSell && LockSell)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Delete lock Sell order,  ticket = {ticketSellLock} ticketCustom = {ticketCustomSellLock}");
                        model.LogInfo(reasone);
                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                        var of = BfcLeg1.PlacedOrdersUsd_M;
                        int indx = of.Count();
                        var id2 = of[indx - 1].OrderId;
                        string ID = id2.ToString();
                        var side = of[indx - 1].Side;
                        if (side == "SELL")
                        {
                            if (OrderDelete(model.Leg1.Symbol, OrderSide.Sell, OrderType.Limit, "0", ID, ticketCustomSellLock))//(model.Leg1.Symbol, prc, lot, "1Leg", //FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Limit))
                            {
                                deleteLockSell = false;
                                LockSell = false;
                                model.LogInfo($"Delete lock SellLimit (1Leg)");
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Delete lock Sell || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                deleteLockSell = false;
                                Thread.Sleep(5000);
                            }
                        }

                    }
                }

                if (!CloseShort)
                {   //---- modify lock -----------
                    if (1>2 && LockBuy && actualPosition < 0 && wantPriceBuy > 0 && LockBuyPrice - wantPriceBuy > 10)// && Math.Abs(wantPriceBuy - LockBuyPrice) > 1000
                    {
                        if (model.AllowOpen)
                        {
                            model.LogInfo($"Try Modify Lock Buy order, ");
                            long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                            string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                            decimal prc = wantPriceBuy;///ticketBuyLock, ticketBuyLock
                            var of = BfcLeg1.PlacedOrdersUsd_M;
                            int indx = of.Count();
                            var id2 = of[indx - 1].OrderId;
                            string ID = id2.ToString();
                            var result = OrderModify(model.Leg1.Symbol, "1Leg", ClientOrderId.ToString(), ID, OrderSide.Buy, prc, lot);
                            if (string.IsNullOrEmpty(result.Error))
                            {
                                LockBuyPrice = prc;
                                model.LogInfo($"Modify Lock BuyLimit (1Leg)");
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Modify Lock Buy || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid} | placed price ={ prc}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                Thread.Sleep(5000); // Pos = false; PosSell = false;
                            }
                        }
                    }
                    //----- adding pos short -----------
                    if (!LockBuy && addingShort && OpenShort && calcOpenOrderSell < grid)// || (calcCloseSell > 0)
                    {
                        if (model.AllowOpen)
                        {
                            model.LogInfo($"Try adding SellLimit, Ask: {model.Leg1.Ask}");
                            model.LogInfo($"Support: {priceSupport} VolS:{volSupport} Resistence: {priceResistence} VolR:{volResistence}");

                            long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                            string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                            decimal prc = model.Leg1.Ask + 2; //wantPriceSell - 5;//model.Leg1.Ask+500;  
                            var result = OpenPos(model.Leg1.Symbol, prc, lot, "1Leg", FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Limit);
                            ticketSell = result.Id;

                            if (ticketSell != "0")
                            {
                                ClientOrderId++;
                                timeOpenPos = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                timeOpenPos2 = DateTimeOffset.Now;
                                LastSellPrice = prc;
                                model.LogInfo($"adding Sell (1Leg), ticket = {ticketSell} ticketCustom = {ClientOrderId}");
                                OpenShort = false; //Pos = true; PosSell = true; 
                                LimitSell = true;
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 1, Open Sell  || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid} | placed price ={ prc}", "SimulatorTrade.txt");
                                calcOpenOrderSell++;
                                if (calcCloseSell > 0) calcCloseSell--;
                            }
                            else
                            {
                                Thread.Sleep(2000); // Pos = false; PosSell = false;
                            }
                        }
                    }
                }
                else
                {
                    if (LockBuy && actualPosition < 0 && !urgently &&  model.Leg1.Bid - LockBuyPrice > 5)
                    {
                        if (model.AllowOpen)
                        {
                            model.LogInfo($"Try Modify Lock Buy order for close at now, ");
                            model.LogInfo(reasone);
                            long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                            string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                            decimal prc = model.Leg1.Bid - 5; ///ticketBuyLock, ticketBuyLock
                            var of = BfcLeg1.PlacedOrdersUsd_M;

                            int indx = of.Count();
                            var id2 = of[indx - 1].OrderId;
                            string ID = id2.ToString();
                            var result = OrderModify(model.Leg1.Symbol, "1Leg", ClientOrderId.ToString(), ID, OrderSide.Buy, prc, ValueTotal);
                            if (string.IsNullOrEmpty(result.Error))
                            {
                                LockBuyPrice = prc;
                                model.LogInfo($"Modify BuyLimit (1Leg) for Close at now");
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Modify Lock Buy || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid} | placed price ={ prc}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                if (result.Error.Contains("2013")) LockBuy = false;
                                Thread.Sleep(2000);// Pos = false; PosSell = false;
                            }
                        }
                    }

                    if (actualPosition < 0 && urgently)
                    {
                        if (model.AllowOpen)
                        {
                            model.LogInfo($"Try Close Sell urgently, Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}");
                            model.LogInfo(reasone);
                            long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                            string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                            decimal tickt = 0;
                            decimal prc = 0;
                            prc = model.Leg1.Ask;
                            var result = OpenPos(model.Leg1.Symbol, prc, ValueTotal, "1Leg", FillPolicy.GTC, OrderSide.Buy, deviationBuy, OrderType.Market);
                            ticketBuy = result.Id;

                            if (decimal.TryParse(ticketBuy, out tickt))
                            {
                                if (tickt != 0)
                                {
                                    ClientOrderId++;
                                    ticketCustomBuy = ClientOrderId.ToString();
                                    CloseShort = false; urgently = false;
                                    CloseShortOk = true;
                                    Lenta = 0;
                                    actualPosition = 0; Pos = false;
                                    model.LogInfo($"Close Main Sell ticketCustom = {ClientOrderId} ticket= {ticketBuy}");
                                    WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 2, Close Sell || Ask: { model.Leg1.Ask}, " +
                                        $"Bid: {model.Leg1.Bid} | placed price={prc}", "SimulatorTrade.txt");
                                }
                                else
                                {
                                    //  Pos = false; PosBuy = false;
                                }
                            }
                        }
                    }
                }

                if (!CloseLong)
                {
                    //---- modify lock ---------------------------
                    if (1>2 && LockSell && actualPosition > 0 && wantPriceSell > 0 && wantPriceSell - LockSellPrice > 10)// && Math.Abs(wantPriceSell - LockSellPrice > 1000
                    {
                        if (model.AllowOpen)
                        {
                            model.LogInfo($"Try Modify Lock Sell order, ");

                            long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                            string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                            decimal prc = wantPriceSell;// - 5;//model.Leg1.Ask+500;  
                                                        //ticketSellLock, ticketSellLock
                            var of = BfcLeg1.PlacedOrdersUsd_M;
                            int indx = of.Count();
                            var id2 = of[indx - 1].OrderId;
                            string ID = id2.ToString();
                            var result = OrderModify(model.Leg1.Symbol, "1Leg", ClientOrderId.ToString(), ID, OrderSide.Sell, prc, lot);
                            if (string.IsNullOrEmpty(result.Error))//(model.Leg1.Symbol, prc, lot, "1Leg",
                            {
                                LockSellPrice = prc;
                                model.LogInfo($"Modify Lock SellLimit (1Leg)");
                                OpenShort = false; //Pos = true; PosSell = true; 
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Modify Lock Sell || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid} | placed price ={ prc}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                Thread.Sleep(5000);
                                //LockSell = false;
                                // Pos = false; PosSell = false;
                            }

                        }
                    }
                    //----- adding pos long -----------
                    if (!LockSell && addingLong && OpenLong && calcOpenOrderBuy < grid)// || (calcCloseBuy > 0)
                    {
                        if (model.AllowOpen)
                        {
                            model.LogInfo($"Try adding BuyLimit, Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}");

                            long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                            string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                            decimal prc = model.Leg1.Bid - 2;//wantPriceBuy +5; //  model.Leg1.Bid;
                            var result = OpenPos(model.Leg1.Symbol, prc, lot, "1Leg", FillPolicy.GTC, OrderSide.Buy, deviationBuy, OrderType.Limit);
                            ticketBuy = result.Id;

                            if (ticketBuy != "0")
                            {
                                ClientOrderId++;
                                LastBuyPrice = prc;
                                timeOpenPos = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                timeOpenPos2 = DateTimeOffset.Now;
                                model.LogInfo($"adding BuyLimit, ticket = {ticketBuy} ticketCustom = {ClientOrderId} ");
                                OpenLong = false; //Pos = true; PosBuy = true;
                                LimitBuy = true;
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 2, Open Buy || Ask: { model.Leg1.Ask}, " +
                                    $"Bid: {model.Leg1.Bid} | placed price={prc}", "SimulatorTrade.txt");
                                calcOpenOrderBuy++;
                                if (calcCloseBuy > 0) calcCloseBuy--;
                            }
                            else
                            {
                                //  Pos = false; PosBuy = false;
                            }
                        }
                    }
                }
                else
                {
                    if (LockSell && actualPosition > 0 && !urgently && LockSellPrice - model.Leg1.Ask > 5)
                    {
                        if (model.AllowOpen)
                        {
                            model.LogInfo($"Try Modify Lock Sell order for Close at now, ");
                            model.LogInfo(reasone);
                            long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                            string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                            decimal prc = model.Leg1.Ask + 5;//model.Leg1.Ask+500;  
                                                         //ticketSellLock, ticketSellLock
                            var of = BfcLeg1.PlacedOrdersUsd_M;
                            int indx = of.Count();
                     
                            var id2 = of[indx - 1].OrderId;
                            string ID = id2.ToString();
                            var result = OrderModify(model.Leg1.Symbol, "1Leg", ClientOrderId.ToString(), ID, OrderSide.Sell, prc, ValueTotal);
                            if (string.IsNullOrEmpty(result.Error))//(model.Leg1.Symbol, prc, lot, "1Leg",
                            {
                                LockSellPrice = prc;
                                model.LogInfo($"Modify Lock SellLimit (1Leg) for Close at now");
                                OpenShort = false; //Pos = true; PosSell = true; 
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Modify Lock Sell || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid} | placed price ={ prc}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                if (result.Error.Contains("2013")) LockSell = false;
                                Thread.Sleep(2000); 
                            }

                        }
                    }

                    if (actualPosition > 0 && urgently)
                    {
                        if (model.AllowOpen)
                        {
                            model.LogInfo($"Try Close Buy urgently, Ask: {model.Leg1.Ask} Bid:{model.Leg1.Bid} ");
                            model.LogInfo(reasone);
                            long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                            string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                            decimal tickt = 0;
                            decimal prc = 0;

                            prc = model.Leg1.Bid;
                            var result = OpenPos(model.Leg1.Symbol, prc, ValueTotal, "1Leg", FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Market);
                            ticketSell = result.Id;

                            if (decimal.TryParse(ticketSell, out tickt))
                            {
                                if (tickt != 0)
                                {
                                    ClientOrderId++;
                                    ticketCustomSell = ClientOrderId.ToString();
                                    CloseLong = false; urgently = false;
                                    CloseLongOk = true;
                                    Lenta = 0;
                                    actualPosition = 0; Pos = false;
                                    model.LogInfo($"Close Main Buy, ticketCustom = {ClientOrderId} ticket = {ticketSell}");
                                    WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 1, Close Main Buy  || Ask: { model.Leg1.Ask}, " +
                                    $"Bid: {model.Leg1.Bid} | placed price ={ prc}", "SimulatorTrade.txt");

                                }
                                else
                                {
                                    // Pos = false; PosSell = false;
                                }
                            }
                        }
                    }

                }
                //---------- delete Main order ----------
                if (LimitSell && deleteMainSell)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Delete main Sell order, Ord={deleteMainSell}");
                        model.LogInfo(reasone);
                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                        var of = BfcLeg1.PlacedOrdersUsd_M;
                        int indx = of.Count();
                        var id2 = of[indx - 1].OrderId;
                        string ID = id2.ToString();
                        var side = of[indx - 1].Side;

                        if (side == "SELL")
                        {
                            if (OrderDelete(model.Leg1.Symbol, OrderSide.Sell, OrderType.Limit, "0", ID, ClientOrderId.ToString()))//(model.Leg1.Symbol, prc, lot, "1Leg", //FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Limit))
                            {
                                LimitSell = false; deleteMainSell = false;
                                calcOpenOrderSell--;
                                model.LogInfo($"Delete main SellLimit (1Leg)");
                                WriteMessageToDesktopFile($"Time: { formattedTime} | reasone: exp, Delete main Sell || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                LimitSell = false; deleteMainSell = false;
                                model.LogInfo($"Error Delete main SellLimit, ticketCustom={ClientOrderId.ToString()} ticket={ID}");
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
                if (LimitBuy && deleteMainBuy)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Delete main Buy order, Ord={deleteMainBuy}");
                        model.LogInfo(reasone);
                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                        var of = BfcLeg1.PlacedOrdersUsd_M;
                        int indx = of.Count();
                        var id2 = of[indx - 1].OrderId;
                        string ID = id2.ToString();
                        var side = of[indx - 1].Side;
                        if (side == "BUY")
                        {

                            if (OrderDelete(model.Leg1.Symbol, OrderSide.Buy, OrderType.Limit, "0", ID, ClientOrderId.ToString()))//(model.Leg1.Symbol, prc, lot, "1Leg", //FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Limit))
                            {
                                LimitBuy = false; deleteMainBuy = false;
                                calcOpenOrderBuy--;
                                model.LogInfo($"Delete main BuyLimit (1Leg)");
                                WriteMessageToDesktopFile($"Time: { formattedTime} | reasone: exp, Delete Buy || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                LimitBuy = false; deleteMainBuy = false;
                                model.LogInfo($"Error Delete main BuyLimit, ticketCustom={ClientOrderId.ToString()} ticket={ID}");
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
            }
            else if (!Pos)
            {
                if (!LimitBuy && OpenLong && calcOpenOrderBuy == 0)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Open BuyLimit, Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}");
                        model.LogInfo(reasone);

                        long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                        decimal tickt = 0;
                        decimal prc = 0;
                        if (calcOpenOrderBuy == 0) 
                        {
                            prc = model.Leg1.Ask;// Bid-1; 
                            var result = OpenPos(model.Leg1.Symbol, prc, lot, "1Leg", FillPolicy.GTC, OrderSide.Buy, deviationBuy, OrderType.Market);
                            ticketBuy = result.Id;
                        }
                        else
                        {
                            //prc = model.Leg1.Bid;
                            //var result = OpenPos(model.Leg1.Symbol, prc, lot, "1Leg", FillPolicy.GTC, OrderSide.Buy, deviationBuy, OrderType.Market);
                            //ticketBuy = result.Id;
                        }

                        if (decimal.TryParse(ticketBuy, out tickt))
                        {
                            if (tickt != 0)
                            {
                                ClientOrderId++;
                                ticketCustomBuy = ClientOrderId.ToString();
                                timeOpenPos = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                timeOpenPos2 = DateTimeOffset.Now;
                                LastBuyPrice = prc;
                                calcOpenOrderBuy++;
                                model.LogInfo($"Placed Main BuyLimit, calcOpenOrderBuy:{calcOpenOrderBuy} ticketCustom = {ClientOrderId} ticket= {ticketBuy} LastBuyPrice={LastBuyPrice}");
                                OpenLong = false; //Pos = true; PosBuy = true;
                                LimitBuy = true;//if(grid==1)
                              
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 2, Open Buy || Ask: { model.Leg1.Ask}, " +
                                    $"Bid: {model.Leg1.Bid} | placed price={prc}", "SimulatorTrade.txt");
                                

                                // Thread.Sleep(3000);
                            }
                            else
                            {
                                //  Pos = false; PosBuy = false;
                            }
                        }
                    }
                }
                if (!LimitSell && OpenShort && calcOpenOrderSell == 0)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Open SellLimit, Ask: {model.Leg1.Ask} Bid:{model.Leg1.Bid}");
                        model.LogInfo(reasone);

                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                        decimal tickt = 0;
                        decimal prc = 0;
                        if (calcOpenOrderSell == 0)
                        {
                            prc = model.Leg1.Bid;// Ask + 1;
                            var result = OpenPos(model.Leg1.Symbol, prc, lot, "1Leg", FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Market);
                            ticketSell = result.Id; 
                        }
                        else
                        {
                            //prc = model.Leg1.Ask;
                            //var result = OpenPos(model.Leg1.Symbol, prc, lot, "1Leg", FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Market);
                            //ticketSell = result.Id;
                        }

                        if (decimal.TryParse(ticketSell, out tickt))
                        {
                            if (tickt != 0)
                            {
                                ClientOrderId++;
                                timeOpenPos = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                timeOpenPos2 = DateTimeOffset.Now;
                                ticketCustomSell = ClientOrderId.ToString();
                                LastSellPrice = prc;
                                calcOpenOrderSell++;
                                model.LogInfo($"Placed Main SellLimit, calcOpenOrderSell:{calcOpenOrderSell} ticketCustom = {ClientOrderId} ticket = {ticketSell} LastSellPrice={LastSellPrice}");
                                OpenShort = false; //Pos = true; PosSell = true; 
                                LimitSell = true;
                               
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 1, Open Main Sell  || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid} | placed price ={ prc}", "SimulatorTrade.txt");
                                
                                //Thread.Sleep(3000);
                            }
                            else
                            {
                                // Pos = false; PosSell = false;
                            }
                        }
                    }
                }
                //---------- modify ---------
                if (1>2 && LimitSell && LastSellPrice > model.Leg1.Ask)//OpenShort && 
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Modify main Sell order, ");
                        // model.LogInfo($"Support: {priceSupport} VolS:{volSupport} Resistence: {priceResistence} VolR:{volResistence}");

                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                        decimal prc = model.Leg1.Ask;// wantPriceSell - 5;//model.Leg1.Ask+500;  
                        var result = OrderModify(model.Leg1.Symbol, "1Leg", ticketCustomSell, ticketSell, OrderSide.Sell, prc, lot);
                        if (string.IsNullOrEmpty(result.Error))//(model.Leg1.Symbol, prc, lot, "1Leg",
                        {
                            LastSellPrice = prc;
                            model.LogInfo($"Modify main SellLimit, ticketCustom={ClientOrderId.ToString()} ticket={ticketSell}");
                            OpenShort = false; //Pos = true; PosSell = true; 
                            WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 1, Modify Sell || Ask: { model.Leg1.Ask}, " +
                            $"Bid: {model.Leg1.Bid} | placed price ={prc}", "SimulatorTrade.txt");
                        }
                        else
                        {
                            model.LogInfo($"Error Modify main SellLimit, ticketCustom={ClientOrderId.ToString()} ticket={ticketSell}");
                            Thread.Sleep(5000); // Pos = false; PosSell = false;
                        }
                    }
                }
                if (1>2 && LimitBuy && LastBuyPrice < model.Leg1.Bid)// OpenLong &&
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Modify main Buy order, ");
                        model.LogInfo($"Support: {priceSupport} VolS:{volSupport} Resistence: {priceResistence} VolR:{volResistence}");

                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                        decimal prc = model.Leg1.Bid;// wantPriceBuy + 5;//model.Leg1.Ask+500;  
                        var result = OrderModify(model.Leg1.Symbol, "1Leg", ticketCustomBuy, ticketBuy, OrderSide.Buy, prc, lot);
                        if (string.IsNullOrEmpty(result.Error))//(model.Leg1.Symbol, prc, lot, "1Leg",//FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Limit))
                        {
                            LastBuyPrice = prc;
                            model.LogInfo($"Modify main BuyLimit (1Leg) ticketCustom={ClientOrderId.ToString()} ticket={ticketBuy}");
                            OpenLong = false; //Pos = true; PosSell = true; 
                            WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 2, Modify Buy || Ask: {model.Leg1.Ask}, " +
                            $"Bid: {model.Leg1.Bid} | placed price ={prc}", "SimulatorTrade.txt");
                        }
                        else
                        {
                            model.LogInfo($"Error Modify main BuyLimit, ticketCustom={ClientOrderId.ToString()} ticket={ticketBuy}");
                            Thread.Sleep(5000);// Pos = false; PosSell = false;
                        }
                    }
                }
                //---------- delete ----------
                if (1>2 && LimitSell && OpenLong)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Delete main Sell order, Ord=OpnLong");
                        model.LogInfo(reasone);
                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                        var of = BfcLeg1.PlacedOrdersUsd_M;
                        int indx = of.Count();
                        var id2 = of[indx - 1].OrderId;
                        string ID = id2.ToString();

                        if (OrderDelete(model.Leg1.Symbol, OrderSide.Sell, OrderType.Limit, "0", ID, ClientOrderId.ToString()))//(model.Leg1.Symbol, prc, lot, "1Leg", //FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Limit))
                        {
                            LimitSell = false;
                            model.LogInfo($"Delete main SellLimit (1Leg)");
                            WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 2, Delete main Sell || Ask: { model.Leg1.Ask}, " +
                            $"Bid: {model.Leg1.Bid}", "SimulatorTrade.txt");
                        }
                        else
                        {
                            //if (result.Error.Contains("2013")) 
                            model.LogInfo($"Error Delete main SellLimit, ticketCustom={ClientOrderId.ToString()} ticket={ID}");
                            LimitSell = false;
                            Thread.Sleep(1000);
                        }
                    }
                }
                if (1>2 && LimitBuy && OpenShort)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Delete main Buy order, Ord=OpnShort");
                        model.LogInfo(reasone);
                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                        var of = BfcLeg1.PlacedOrdersUsd_M;
                        int indx = of.Count();
                        var id2 = of[indx - 1].OrderId;
                        string ID = id2.ToString();

                        if (OrderDelete(model.Leg1.Symbol, OrderSide.Buy, OrderType.Limit, "0", ID, ClientOrderId.ToString()))//(model.Leg1.Symbol, prc, lot, "1Leg", //FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Limit))
                        {
                            LimitBuy = false;
                            model.LogInfo($"Delete main BuyLimit (1Leg)");
                            WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 1, Delete Buy || Ask: { model.Leg1.Ask}, " +
                            $"Bid: {model.Leg1.Bid}", "SimulatorTrade.txt");
                        }
                        else
                        {
                           // if (result.Error.Contains("2013"))
                            model.LogInfo($"Error Delete main BuyLimit, ticketCustom={ClientOrderId.ToString()} ticket={ID}");
                            LimitBuy = false;
                            Thread.Sleep(1000);
                        }
                    }
                }

                //---------- delete Main order ----------
                if (LimitSell && deleteMainSell)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Delete main Sell order, ");
                        model.LogInfo(reasone);
                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                        var of = BfcLeg1.PlacedOrdersUsd_M;
                        int indx = of.Count();
                        var id2 = of[indx - 1].OrderId;
                        string ID = id2.ToString();
                        var side = of[indx - 1].Side;
                        if (side == "SELL")
                        {
                            if (OrderDelete(model.Leg1.Symbol, OrderSide.Sell, OrderType.Limit, "0", ID, ClientOrderId.ToString()))//(model.Leg1.Symbol, prc, lot, "1Leg", //FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Limit))
                            {
                                LimitSell = false; deleteMainSell = false;
                                model.LogInfo($"Delete main SellLimit (1Leg)");
                                WriteMessageToDesktopFile($"Time: { formattedTime} | reasone: exp, Delete main Sell || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                LimitSell = false; deleteMainSell = false;
                                model.LogInfo($"Error Delete main SellLimit, ticketCustom={ClientOrderId.ToString()} ticket={ID}");
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }
                if (LimitBuy && deleteMainBuy)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Delete main Buy order, ");
                        model.LogInfo(reasone);
                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                        var of = BfcLeg1.PlacedOrdersUsd_M;
                        int indx = of.Count();
                        var id2 = of[indx - 1].OrderId;
                        string ID = id2.ToString();
                        var side = of[indx - 1].Side;
                        if (side == "BUY")
                        {
                            if (OrderDelete(model.Leg1.Symbol, OrderSide.Buy, OrderType.Limit, "0", ID, ClientOrderId.ToString()))//(model.Leg1.Symbol, prc, lot, "1Leg", //FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Limit))
                            {
                                LimitBuy = false; deleteMainBuy = false;
                                model.LogInfo($"Delete main BuyLimit (1Leg)");
                                WriteMessageToDesktopFile($"Time: { formattedTime} | reasone: exp, Delete Buy || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                LimitBuy = false; deleteMainBuy = false;
                                model.LogInfo($"Error Delete main BuyLimit, ticketCustom={ClientOrderId.ToString()} ticket={ID}");
                                Thread.Sleep(1000);
                            }
                        }
                    }
                }

            }
            //**************** TRADE *******************************************
        }

     #endregion

        private void FunMarketTrade(bool OpenShort, bool OpenLong, bool CloseShort, bool CloseLong, 
                                    bool ClosePartlyLong, bool ClosePartlyShort, bool addingLong, bool addingShort, decimal ValueTotal, decimal lot, int grid, string reasone)
        {
            //**************** TRADE *******************************************
            if (Pos)
            {
                if (ClosePartlyShort && actualPosition < 0 && calcCloseSell <= calcOpenOrderSell)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Close Partly Short, Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}");
                        model.LogInfo(reasone);

                        long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                        decimal tickt = 0;
                        decimal prc = model.Leg1.Bid - 1;//wantPriceBuy;//ValueTotal
                        var result = OpenPos(model.Leg1.Symbol, prc, lot, "1Leg", FillPolicy.GTC, OrderSide.Buy, deviationBuy, OrderType.Market);
                        ticketBuyLock = result.Id;

                        if (decimal.TryParse(ticketBuyLock, out tickt))
                        {
                            if (tickt != 0)
                            {
                                ClosePartlyShort = false;
                                timeOpenPos = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                calcCloseSell++;
                                lastCloseShort = prc;
                                calcOpenOrderSell--;
                                ClientOrderId++;
                                ticketCustomBuyLock = ClientOrderId.ToString();
                                actualPosition = actualPosition + lot;
                                if (ValueTotal == lot) { Nulling(); Pos = false; actualPosition = 0; }
                                model.LogInfo($"Close Partly Short, ticketCustom= {ticketCustomBuyLock}, ticket= {ticketBuyLock} ");
                                //LockBuy = true;
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Lock Buy || Ask: { model.Leg1.Ask}, " +
                                  $"Bid: {model.Leg1.Bid} | placed price={prc}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                //  Pos = false; PosBuy = false;
                            }
                        }


                    }
                }
                if (ClosePartlyLong && actualPosition > 0 && calcCloseBuy <= calcOpenOrderBuy)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Close Partly Long, Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}");
                        model.LogInfo(reasone);
                        long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                        decimal tickt = 0;
                        decimal prc = model.Leg1.Ask + 1;//wantPriceSell;
                        var result = OpenPos(model.Leg1.Symbol, prc, lot, "1Leg", FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Market);
                        ticketSellLock = result.Id;

                        if (decimal.TryParse(ticketSellLock, out tickt))
                        {
                            if (tickt != 0)
                            {
                                ClosePartlyLong = false;
                                timeOpenPos = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                calcCloseBuy++;
                                lastCloseLong = prc;
                                calcOpenOrderBuy--;
                                ClientOrderId++;
                                ticketCustomSellLock = ClientOrderId.ToString();
                                actualPosition = actualPosition - lot;
                                if (ValueTotal == lot) { Nulling(); Pos = false; actualPosition = 0; }
                                model.LogInfo($"Close Partly Long, ticketCustom= {ticketCustomSellLock}, ticket= {ticketSellLock} ");
                               // LockSell = true;
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Lock Sell || Ask: { model.Leg1.Ask}, " +
                                    $"Bid: {model.Leg1.Bid} | placed price={prc}", "SimulatorTrade.txt");
                            }
                            else
                            {
                                //  Pos = false; PosBuy = false;
                            }
                        }
                    }
                }

                if (!CloseShort)
                {   
                    //----- adding pos ---------
                    if (addingShort && OpenShort && calcOpenOrderSell < grid)// || (calcCloseSell > 0)
                    {
                        if (model.AllowOpen)
                        {
                            model.LogInfo($"Try adding SellLimit, Ask: {model.Leg1.Ask}");
                            model.LogInfo(reasone);

                            long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                            string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                            decimal prc = model.Leg1.Ask + 5; //wantPriceSell - 5;//model.Leg1.Ask+500;  
                            var result = OpenPos(model.Leg1.Symbol, prc, lot, "1Leg", FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Market);

                            ticketSell = result.Id;

                            if (ticketSell != "0")
                            {
                                addingShort = false;
                                timeOpenPos = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                ClientOrderId++;
                                LastSellPrice = prc;
                                model.LogInfo($"adding Sell (1Leg), ticket = {ticketSell} ticketCustom = {ClientOrderId}");
                                OpenShort = false; //Pos = true; PosSell = true; 
                                actualPosition = actualPosition - lot;
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 1, Open Sell  || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid} | placed price ={ prc}", "SimulatorTrade.txt");
                                calcOpenOrderSell++;
                                if (calcCloseSell > 0) calcCloseSell--;
                            }
                            else
                            {
                                Thread.Sleep(2000); // Pos = false; PosSell = false;
                            }
                        }
                    }
                }
                else
                {  //------- close -------------
                    if (actualPosition < 0)
                    {
                        if (model.AllowOpen)
                        {
                            model.LogInfo($"Try Close Sell, TotalVal={ValueTotal} pos{actualPosition} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}, stoploss={stopLoss}");
                            //model.LogInfo($"Support: {priceSupport} VolS:{volSupport} Resistence: {priceResistence} VolR:{volResistence}");
                            model.LogInfo(reasone);

                            long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                            string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                            decimal tickt = 0;
                            decimal prc = 0;
                            prc = model.Leg1.Ask;
                            var result = OpenPos(model.Leg1.Symbol, prc, ValueTotal, "1Leg", FillPolicy.GTC, OrderSide.Buy, deviationBuy, OrderType.Market);
                            ticketBuy = result.Id;
                            if (decimal.TryParse(ticketBuy, out tickt))
                            {
                                if (tickt != 0)
                                {
                                    ClientOrderId++;
                                    timeOpenPos = 0;
                                    timeClosePos = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                    model.LogInfo($"Move:{StakanLentaMove} Stk:{Stakan} Lnt:{Lenta} Prg:{Pruzgina} Fon:{Fon} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}");
                                    Nulling();
                                    ticketCustomBuy = ClientOrderId.ToString();
                                    CloseShort = false;
                                    Lenta = 0;
                                    Pos = false;
                                    actualPosition = 0; 
                                    //LastBuyPrice = prc;
                                    model.LogInfo($"Close Main Sell ticketCustom = {ClientOrderId} ticket= {ticketBuy}");
                                    //OpenLong = false; //Pos = true; PosBuy = true;
                                    //LimitBuy = true;//if(grid==1)
                                    //CheckPosition();
                                    WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 2, Close Sell || Ask: { model.Leg1.Ask}, " +
                                        $"Bid: {model.Leg1.Bid} | placed price={prc}", "SimulatorTrade.txt");

                                }
                                else
                                {
                                    //  Pos = false; PosBuy = false;
                                }
                            }
                        }
                    }
                }

                if (!CloseLong)
                {
                    //----- adding pos -----------
                    if (addingLong && OpenLong && calcOpenOrderBuy < grid)// || (calcCloseBuy > 0)
                    {
                        if (model.AllowOpen)
                        {
                            model.LogInfo($"Try adding BuyLimit, Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}");
                            model.LogInfo(reasone);

                            long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                            string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                            decimal prc = model.Leg1.Bid - 5;//wantPriceBuy +5; //  model.Leg1.Bid;
                            var result = OpenPos(model.Leg1.Symbol, prc, lot, "1Leg", FillPolicy.GTC, OrderSide.Buy, deviationBuy, OrderType.Market);
                            ticketBuy = result.Id;

                            if (ticketBuy != "0")
                            {
                                addingLong = false;
                                ClientOrderId++;
                                timeOpenPos = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                LastBuyPrice = prc;
                                model.LogInfo($"adding BuyLimit, ticket = {ticketBuy} ticketCustom = {ClientOrderId} ");
                                OpenLong = false; //Pos = true; PosBuy = true;
                                actualPosition = actualPosition + lot;
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 2, Open Buy || Ask: { model.Leg1.Ask}, " +
                                    $"Bid: {model.Leg1.Bid} | placed price={prc}", "SimulatorTrade.txt");
                                calcOpenOrderBuy++;
                                if (calcCloseBuy > 0) calcCloseBuy--;
                            }
                            else
                            {
                                //  Pos = false; PosBuy = false;
                            }
                        }
                    }
                }
                else
                {   //------- close --------------
                    if (actualPosition > 0)
                    {
                        if (model.AllowOpen)
                        {
                            model.LogInfo($"Try Close Buy, TotalVal={ValueTotal} pos{actualPosition} Bid: {model.Leg1.Bid} Ask: {model.Leg1.Ask}, stoploss={stopLoss}");
                            //model.LogInfo($"Support: {priceSupport} VolS:{volSupport} Resistence: {priceResistence} VolR:{volResistence}");
                            model.LogInfo(reasone);
                            long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                            string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                            decimal tickt = 0;
                            decimal prc = 0;

                            prc = model.Leg1.Bid;
                            
                            var result = OpenPos(model.Leg1.Symbol, prc, ValueTotal, "1Leg", FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Market);
                            ticketSell = result.Id;

                            if (decimal.TryParse(ticketSell, out tickt))
                            {
                                if (tickt != 0)
                                {
                                    ClientOrderId++;
                                    model.LogInfo($"Move:{StakanLentaMove} Stk:{Stakan} Lnt:{Lenta} Prg:{Pruzgina} Fon:{Fon} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}");
                                    Nulling();
                                    timeOpenPos = 0;
                                    timeClosePos = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                    ticketCustomSell = ClientOrderId.ToString();
                                    CloseLong = false;
                                    Lenta = 0;
                                    Pos = false;
                                    actualPosition = 0;
                                    model.LogInfo($"Close Main Buy, ticketCustom = {ClientOrderId} ticket = {ticketSell}");
                                    //OpenShort = false; //Pos = true; PosSell = true; 
                                    //CheckPosition();
                                    WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 1, Close Main Buy  || Ask: { model.Leg1.Ask}, " +
                                    $"Bid: {model.Leg1.Bid} | placed price ={ prc}", "SimulatorTrade.txt");

                                }
                                else
                                {
                                    // Pos = false; PosSell = false;
                                }
                            }
                        }
                    }
                }

            }
            else if (!Pos)
            {
                if (OpenLong && calcOpenOrderBuy < grid)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Open Buy, Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}");
                        model.LogInfo(reasone);
                        model.LogInfo($"Support: {priceSupport} VolS:{volSupport} Resistence: {priceResistence} VolR:{volResistence}");

                        long milliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                        decimal tickt = 0;
                        decimal prc  = model.Leg1.Ask; 
                         
                        var result = OpenPos(model.Leg1.Symbol, prc, lot, "1Leg", FillPolicy.GTC, OrderSide.Buy, deviationBuy, OrderType.Market);
                        ticketBuy = result.Id;

                        if (decimal.TryParse(ticketBuy, out tickt))
                        {
                            if (tickt != 0)
                            {
                                ClientOrderId++;
                                model.LogInfo($"Move:{StakanLentaMove} Stk:{Stakan} Lnt:{Lenta} Prg:{Pruzgina} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}");
                                timeOpenPos = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                timeClosePos = 0;
                                ticketCustomBuy = ClientOrderId.ToString();
                                Lenta = 0;
                                //CheckPosition();
                                //EntryPrice = result.OpenPrice;
                                Pos = true;
                                EntryPrice = LastBuyPrice = prc;
                                actualPosition = lot;
                                model.LogInfo($"Placed Main Buy, ticketCustom = {ClientOrderId} ticket= {ticketBuy} LastBuyPrice={LastBuyPrice}");
                                OpenLong = false; //Pos = true; PosBuy = true;
                                //LimitBuy = true;//if(grid==1)
                                
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 2, Open Buy || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid} | placed price={prc}", "SimulatorTrade.txt");
                                calcOpenOrderBuy++;

                                // Thread.Sleep(3000);
                            }
                            else
                            {
                                //  Pos = false; PosBuy = false;
                            }
                        }
                    }
                }
                if (OpenShort && calcOpenOrderSell < grid)
                {
                    if (model.AllowOpen)
                    {
                        model.LogInfo($"Try Open Sell, Ask: {model.Leg1.Ask} Bid: {model.Leg1.Bid}");
                        model.LogInfo(reasone);
                        //model.LogInfo($"Support: {priceSupport} VolS:{volSupport} Resistence: {priceResistence} VolR:{volResistence}");
                        
                        long milliseconds = (long)(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
                        DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds(milliseconds);
                        string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                        decimal tickt = 0;
                        decimal prc = model.Leg1.Bid;
                        
                        var result = OpenPos(model.Leg1.Symbol, prc, lot, "1Leg", FillPolicy.GTC, OrderSide.Sell, deviationSell, OrderType.Market);
                        ticketSell = result.Id;
                                      
                        if (decimal.TryParse(ticketSell, out tickt))
                        {
                            if (tickt != 0)
                            {
                                ClientOrderId++;
                                model.LogInfo($"Move:{StakanLentaMove} Stk:{Stakan} Lnt:{Lenta} Prg:{Pruzgina} Fon:{Fon} Bid: {model.Leg1.Bid} Ask:  {model.Leg1.Ask}");
                                timeOpenPos = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                                timeClosePos = 0;
                                ticketCustomSell = ClientOrderId.ToString();
                                EntryPrice = LastSellPrice = prc;
                                model.LogInfo($"Placed Main Sell, ticketCustom = {ClientOrderId} ticket = {ticketSell} LastSellPrice={LastSellPrice}");
                                OpenShort = false; //Pos = true; PosSell = true; 
                                Lenta = 0;
                                //CheckPosition();
                                //EntryPrice = result.OpenPrice;
                                actualPosition = lot*-1;
                                Pos = true;
                                //LimitSell = true;
                                WriteMessageToDesktopFile($"Time: { formattedTime} | Umova = 1, Open Main Sell  || Ask: { model.Leg1.Ask}, " +
                                $"Bid: {model.Leg1.Bid} | placed price ={ prc}", "SimulatorTrade.txt");
                                calcOpenOrderSell++;
                                //Thread.Sleep(3000);
                            }
                            else
                            {
                                // Pos = false; PosSell = false;
                            }
                        }
                    }
                }
 
            }
            //**************** TRADE *******************************************

        }
        public OrderOpenResult OpenPos(string symb, decimal price, decimal LOT, string type, FillPolicy policy, OrderSide bs, decimal gap, OrderType orderType)
        {
            string ticket = "0";
            bool isSuccess = false;   //model.Leg1.Lot            

            OrderOpenResult result = _1LegConnector.Open(symb, price, LOT, policy, bs, model.Magic, model.Slippage, 1,
                            orderType, model.Open.PendingLifeTimeMs);

            //var id=result.Id;
            //ticket = id;
            // Check if Order was Successfully send:
            if (string.IsNullOrEmpty(result.Error))
            {
                decimal slippage = -(result.OpenPrice - model.Leg1.Ask);
                model.LogOrderSuccess($"[{type}]: {bs.ToString()} OK " + model.Leg1.Symbol + " at " + model.FormatPrice(result.OpenPrice) + 
                    
                    ";Price=" + result.OpenPrice + ";Slippage=" + ToStr1(slippage) +
                    ";Execution=" + ToStrMs(result.ExecutionTime) + " ms;");
                isSuccess = true;
            }
            else
            {
                model.LogError(_1LegConnector.ViewId + " " + result.Error);
                //      var balanceActual = bcc.MarginAccount.totalCollateralValueInUSDT;
                model.LogInfo($"[{type}]: {bs.ToString()} FAILED " + model.Leg1.Symbol + ";Gap=" + gap + ";Price=" + model.FormatPrice(model.Leg1.Ask));
                isSuccess = false;
            }

            return result;//isSuccess;
        }

        public OrderModifyResult OrderModify(string symbol, string legType, string origClientOrderId, string orderId, OrderSide side, decimal newPrice, decimal lot)
        {
            bool isSuccess = false;

            var result = _1LegConnector.Modify(symbol, origClientOrderId, orderId, side, newPrice, lot);

            // check if order modify was successfully send:
            if (string.IsNullOrEmpty(result.Error))
            {
                model.LogOrderSuccess($"[{legType}]: {result.Side.ToString()} OK MODIFY " + model.Leg1.Symbol + $" at {result.Lot} lot" +
                    ";price=" + model.FormatPrice(result.OpenPrice) +
                    ";execution=" + ToStrMs(result.ExecutionTime) + " ms;");
                isSuccess = true;
            }
            else
            {
                model.LogError(_1LegConnector.ViewId + " " + result.Error);
                model.LogInfo($"[{legType}]: {side.ToString()} failed " + model.Leg1.Symbol + ";price=" +
                    model.FormatPrice(model.Leg1.Ask));

                if(result.Error.Contains("2013")) 
                {
                    model.LogInfo($"srabotalo");
                }

                isSuccess = false;
            }

            return result;
        }

       

        public bool OrderDelete(string symbol, OrderSide side, OrderType type, string price, string orderId, string origClientOrderId)
        {
            bool isSuccess = false;
            var start = DateTime.Now;
            var result = _1LegConnector.OrderDelete(symbol, orderId, origClientOrderId);
            var end = DateTime.Now;

            if (result)
            {
                model.LogOrderSuccess($"OrderId: [{orderId}]: {side.ToString()} {type.ToString()} OK DELETE  " + model.Leg1.Symbol +
                    ";price=" + price +
                    ";Execution=" + ToStrMs(end - start) + " ms;");
                isSuccess = true;
            }
            else
            {
                model.LogError(_1LegConnector.ViewId + " DELETE FAILURE.....");
                isSuccess = false;
            }

            // return isSuccess;
            return result;
        }

        void On_2LegLogin()
        {
            _2LegConnector.Fill = (FillPolicy)model.Open.Fill;
            _2LegConnector.Subscribe(model.Leg2.Symbol, model.Leg2.Symbol, Models.TradeAlgorithm.ZigZag.ToString());
        }
        void On_1LegLogin()
        {
            _1LegConnector.Fill = (FillPolicy)model.Open.Fill;
            _1LegConnector.Subscribe(model.Leg1.Symbol, model.Leg2.Symbol, Models.TradeAlgorithm.ZigZag.ToString());
        }

        string ToStr1(decimal value)
        {
            return value.ToString("F1", CultureInfo.InvariantCulture);
        }

        string ToStrMs(TimeSpan span)
        {
            return span.TotalMilliseconds.ToString("F3", CultureInfo.InvariantCulture);
        }

        private void TwoLegConnector_LoggedIn(object sender, EventArgs e)
        {
            On_2LegLogin();
        }

        private void OneLegConnector_LoggedIn(object sender, EventArgs e)
        {
            On_1LegLogin();
        }
        #endregion

        static void WriteMessageToDesktopFile(string message, string fileName)
        {
            try
            {
                // Отримуємо шлях до робочого столу користувача
                string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

                // Формуємо повний шлях до файлу на робочому столі
                string filePath = Path.Combine(desktopPath, fileName);

                // Додаємо повідомлення до файлу на новому рядку
                File.AppendAllText(filePath, $"{message}{Environment.NewLine}");

               // Console.WriteLine($"Повідомлення було успішно додано до файлу {fileName} на робочому столі.");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Виникла помилка: {ex.Message}");
            }
        }

    }
}

