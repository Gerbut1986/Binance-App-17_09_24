namespace Models.Algo
{
    using System;
    using Helpers;
    using System.Linq;
    using Helpers.Extensions;
    using System.Collections.Generic;
    using MultiTerminal.Connections.API.Future;
    using MultiTerminal.Connections;
    using System.Collections.Concurrent; 
    using BinanceOptionsApp; 
    using System.Threading.Tasks;
    using BinanceOptionsApp.MultiTerminal.Connections.API.Future;

    public class Plot
    {
        public decimal MarketBuy { get; set; } = 0;
        public decimal MarketSell { get; set; } = 0;
        public decimal Price { get; set; } = 0;
    }
    public class Zig_Zag
    {
        public string Tip { get; set; } = "";
        public int WaweCount { get; set; } = 0;
        public decimal StartPrice { get; set; } = 0;
        public decimal FinishPrice { get; set; } = 0;
        public long StartTime { get; set; } = 0;
        public long FinishTime { get; set; } = 0;
        public ulong ID_Q_Start { get; set; } = 0;
        public ulong ID_Q_Finish { get; set; } = 0;
        public decimal MaxBuyVol { get; set; } = 0;
        public decimal PriceMaxBuy { get; set; } = 0;
        public decimal MaxSellVol { get; set; } = 0;
        public decimal PriceMaxSell { get; set; } = 0;
        public decimal AvgBuy { get; set; } = 0;
        public decimal AvgSell  { get; set; } = 0;
        public decimal Gap { get; set; } = 0;
        public decimal avGap { get; set; } = 0;
    }
 

    public class ZigZagFuture
    {
        #region Fields:
           
        //private static decimal Leg = 25; //100*100*10
        //private static readonly decimal SenDist = 200;
        private static readonly decimal ThresholdVol = 1;
        private decimal avGPB = 0, avGPS = 0, GPB = 0, GPS = 0, zGPB = 0, zGPS = 0;
        private static decimal MaxASK_Vol = 0;
        private static decimal MinBID_Vol = 0;
        private static decimal MaxASK = 0;
        private static decimal MinBID = 0;
        private static ulong ID_Q_MaxAsk = 0;
        private static ulong ID_Q_MinBid = 0;
        private static ulong TimeMaxASK;
        private static ulong TimeMinBID;
        public static int WaveCounter = 0;
        private static bool WaveDN = false;
        private static bool WaveUP = false;
        //--------------------------------------------+
        private decimal priceStep = 0.01m;
        private decimal StartPrice;
        private decimal FinishPrice;
        private ulong StartTime { get; set; }
        private ulong FinishTime;
        static ulong LastID_Q_MinBid = 0;
        static ulong LastID_Q_MaxAsk = 0;
        static ulong Pre_id = 0;
        string CL = "";
        int Kin = 0;
        static bool Triger = false;
        string CrLg = "0";
        public static decimal SumAsk = 0, SumBid = 0;
        public static decimal SupportPrice = 0, ResistancePrice = 0;
        public static decimal SupportVol = 0, ResistanceVol = 0;
        public static int MainRankSell = 0, MainRankBuy = 0;
        private static decimal RP = 0, SP = 0, RV = 0, SV = 0;
        private object lockObject = new object();
        private DateTime KinTime;
        //System.DateTime TimeNow;
        private DateTimeOffset currentTime;// = System.DateTime.Now;
        private ConcurrentDictionary<decimal, MarketDepthUpdateFuture> MaxAsklistMD = default, MinBidlistMD = default, MDZigDN = default, MDZigUP = default;
        private List<List<string>> MaxAsklistMD_P = default, MinBidlistMD_P = default, MDZigDN_P = default, MDZigUP_P = default;
        
        // Створюємо колекцію моделей MarketData
        List<MarketMaxLevel> MaxDataList = new List<MarketMaxLevel>();
        List<MarketMaxLevel> MaxCurrLegList = new List<MarketMaxLevel>();
        List<MarketMaxLevel> MaxKinList = new List<MarketMaxLevel>();
        decimal MaxLevelBuy = 0, MaxLevelSell = 0, PriceMaxLevelBuy = 0, PriceMaxLevelSell = 0;

        class MarketMaxLevel
        {
            public double MaxMarketBuy { get; set; }
            public double MaxMarketSell { get; set; }
        }
        static void AddMarketLevel(List<MarketMaxLevel> MaxDataList, double maxMarketBuy, double maxMarketSell)
        {
            var marketData = new MarketMaxLevel
            {
                MaxMarketBuy = maxMarketBuy,
                MaxMarketSell = maxMarketSell
            };

            MaxDataList.Add(marketData);
        }

        static double CalculateAverage(List<MarketMaxLevel> dataList, Func<MarketMaxLevel, double> selector)
        {
            int lastThreeCount = Math.Min(dataList.Count, 3);

            if (lastThreeCount == 0)
                return 0;

            double sum = 0;

            for (int i = dataList.Count - lastThreeCount; i < dataList.Count; i++)
            {
                double selectedValue = selector(dataList[i]);
        
                sum += selectedValue;
            }
            double average = sum / lastThreeCount;
            return average;
        }

        #endregion

        #region Arrays: ZigZagArr, CurrLeg, Kinchik[]:
        public readonly Plot[] ZigZagArr; //100*100*10*3 (for Leg 3000$; step=0.01)
        public static Plot[] CurrLeg = new Plot[300000];
        public static Plot[] Kinchik = new Plot[100000];
        #endregion

        public ZigZagFuture()
        {
            ZigZagArr = new Plot[300000];
            InitZigZagArr();
        }

        #region BuildZigZag func:
        string WW = "0";
        string uz = "";
        string uzG = ""; int uzz = 0;
        public void BuildZigZag(decimal ask, decimal bid, ulong Time, ulong ID_quark, List<AggTradeFuture> tasLst, decimal avGPbuy, decimal avGPsell, decimal GPbuy, decimal GPsell, ConcurrentDictionary<decimal, MarketDepthUpdateFuture> mdList = default, List<BookTickerF> bookTickerList = default, List<List<string>> asksP = default)
        {
            if (MaxASK == 0 || ask > MaxASK)  //  
            {
                MaxASK = ask;
                TimeMaxASK = Time;
                ID_Q_MaxAsk = ID_quark;
                MaxASK_Vol = SumAsk;
                GPB=GPbuy;
                avGPB = avGPbuy;
                //MainRankSell = BinanceExecution.FunDepthProba2(1);
                RP = ResistancePrice;
                RV = ResistanceVol;
                MaxAsklistMD = mdList;
                MaxAsklistMD_P = asksP;
                var bookTicker = bookTickerList;
                //bookTicker[0].Data.AskQuantity
            }  //new MAX

            if (MinBID == 0 || bid < MinBID)  //  
            {
                MinBID = bid;
                TimeMinBID = Time;
                ID_Q_MinBid = ID_quark;
                MinBID_Vol = SumBid;
                GPS = GPsell;
                avGPS = avGPsell;
                //MainRankBuy = BinanceExecution.FunDepthProba2(2);
                SP = SupportPrice;
                SV = SupportVol;
                MinBidlistMD = mdList;
            }  //new MIN

            if (TimeMaxASK > TimeMinBID) // UP zig zag
            {
                if (MaxASK - MinBID >= TradeZigZag.Leg && MaxASK - bid >= TradeZigZag.Leg) // UP zig zag
                {
                    WaveUP = true; WaveDN = false;
                    WaveCounter++;
                    StartPrice = MinBID;
                    StartTime = TimeMinBID;
                    FinishPrice = MaxASK;
                    FinishTime = TimeMaxASK;
                    zGPB = GPB;
                    MDZigUP = MaxAsklistMD;
                    ZigZagPlot.Clear();
                    CurrLegPlot.Clear();
                    //InitZigZagArr();
                    //InitCurrLeg();//nulling CurrLeg
                    preID = 0;
                    FillingSmallZZ(ID_Q_MinBid, ID_Q_MaxAsk, StartTime, FinishTime, StartPrice, FinishPrice, "UP", tasLst, avGPB, zGPB);
                    TradeZigZag.avDevUPzz = CheckGapZZ(8, "UP");
                    TradeZigZag.lastZZ = "UP"; TradeZigZag.wave = WaveCounter;
                    //uz = CheckUZ(3);
                    //uzG = CheckUZ(5);
                    //uzG = CheckVZ(5);
                    uzz = CheckZigOrd();
                    MinBID = bid;
                    TimeMinBID = Time;
                    ID_Q_MinBid = ID_quark;
                    MinBID_Vol = SumBid;
                    //MainRankBuy = BinanceExecution.FunDepthProba2(2);
                    SP = SupportPrice;
                    SV = SupportVol;
                }
            }   // UP Zig Zag

            if (WaveUP == true && TimeMaxASK < Time) //for DN cur leg
            {
                if (LastID_Q_MinBid == 0) LastID_Q_MinBid = ID_Q_MinBid;
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds((long)Time);
                string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");

                string symb = TradeZigZag.Symbol; bool ChPointZZ = false;

                if (ID_quark > Pre_id)
                {
                    if (ID_Q_MinBid > LastID_Q_MinBid)
                    {
                        KinchikPlot.Clear();

                        lock (lockObject)
                        {
                            FillingCurrentLeg(ID_Q_MaxAsk, ID_quark, MaxASK, "dn", tasLst);
                        }

                        CrLg = GazerCurrLeg2("dn", ask , bid, Time);
                        if (uzz == 2 && CrLg != "111") { TradeZigZag.OrderZig = 2; TradeZigZag.OrderZigTime = currentTime; }
                        else if (CrLg == "22") { TradeZigZag.OrderZig = 22; TradeZigZag.OrderZigTime = currentTime; }
                        else if (CrLg == "222") { TradeZigZag.OrderZig = 222; TradeZigZag.OrderZigTime = currentTime; }
                        else TradeZigZag.OrderZig = -10;

                        //string ser = "";
                        //if (uzG == "UP")ser = CheckLevel(ask, 2);
                        //if (uzG == "DN")ser = CheckLevel(ask, 1);

                        //if (ser == "DN" && CrLg != "22"){ TradeZigZag.OrderZig = 1;TradeZigZag.OrderZigTime = currentTime;  }
                        //else if (ser == "UP") { TradeZigZag.OrderZig = 2; TradeZigZag.OrderZigTime = currentTime; }
                        //else if (CrLg == "22") { TradeZigZag.OrderZig = 22; TradeZigZag.OrderZigTime = currentTime; }
                        //else TradeZigZag.OrderZig = -10;

                        //ChPointZZ = CheckPointZZ(bid, TradeZigZag.Leg * 0.15m);
                        //if(ChPointZZ)
                        //new _().WriteToTxtFile($"Time: {formattedTime} |cur dn | ChPoint={ChPointZZ} | Ask: {ask}, Bid: {bid}", "Point.txt");

                        //----------------------------------
                        // if (TradeZigZag.StepByStepDN > 0 && TradeZigZag.StepByStepDN < 4 && (currentTime - TradeZigZag.StepByStepTime).TotalSeconds < 3)
                        //if (uzG == "DN") //uz == "DN" && 
                        //{
                        //    TradeZigZag.OrderZig = 1;
                        //    TradeZigZag.OrderZigTime = currentTime;
                        //    string umu = "";
                        //    //if (currLEG.Tip == "dn" && (ask >= currLEG.PriceMaxSell)) umu = "Right";// || ask >= currLEG.PriceMaxBuy
                        //    //else umu = "0";
                        //    //new _().WriteToTxtFile($"Time: {formattedTime} | cur dn | sell | R:{umu} | Ask: {ask}, Bid: {bid}", "OrdZig.txt");
                        //}


                        //----------------------------------

                        Triger = true;
                        LastID_Q_MinBid = ID_Q_MinBid;
                    }
                    else
                    {
                        try
                        {
                            if (ID_quark > ID_Q_MinBid)
                            {
                                FillingKnut(ID_Q_MinBid, ID_quark, MinBID, "up", tasLst);
                                Kin = GazerKinchik("up", ask, bid,Time);
                            }
                            currentTime = DateTimeOffset.Now;

                            if (Triger)
                            {
                                string FileName2 = "ZigFinishFuture" + symb;
                                //---------------------------------------- && ZZF[ZZF.Count - 2].Gap <= TradeZigZag.GAPbuy_q
                                if (CrLg != "0") 
                                {
                                    string formattedTimeN = currentTime.ToString("HH:mm:ss.fff");//yyyy-MM-dd
                                    new _().WriteToTxtFile($"Time: {formattedTimeN} | ReversVol = {CrLg} | ChPoint={ChPointZZ} | Ask: {ask}, Bid: {bid}", FileName2);
                                    CrLg = "0";
                                    // WW = CrLg;//ZZF.Count;
                                }
                            }

                            string zzz = CheckZZ();

                            //if (TradeZigZag.StepByStepUP > 0 && TradeZigZag.StepByStepUP < 4 && (currentTime - TradeZigZag.StepByStepTime).TotalSeconds < 3)
                            //{
                            //    TradeZigZag.OrderZig = 2;
                            //    TradeZigZag.OrderZigTime = currentTime;
                            //    new _().WriteToTxtFile($"Time: {formattedTime} | kin up | buy | Ask: {ask}, Bid: {bid}", "OrdZig.txt");
                            //}

 
                            if ((Kin == 2 || CrLg == "22" || CrLg == "220" || Kin == 222 || Kin == 2222) && Triger) // && TradeZigZag.Stakan == 2
                            {
                                if(CrLg == "22" || CrLg == "220") TradeZigZag.Order = CrLg;
                                else TradeZigZag.Order = Kin.ToString();
                                TradeZigZag.OrderTime = currentTime;
                                long T = (long)Time;
                                KinTime = DateTimeOffset.FromUnixTimeMilliseconds(T).DateTime;//(long)
                                formattedTime = KinTime.ToString("HH:mm:ss.fff");
                                string formattedTimeN = currentTime.ToString("HH:mm:ss.fff");//yyyy-MM-dd
                                string KinMsg = $"Time= {formattedTimeN} Kin= {Kin}  Ask= {ask}  Bid= {bid} curr time: {formattedTime}";
                                new _().WriteToTxtFile(KinMsg, "Kinchik");
                                Triger = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            new _().WriteToTxtFile($"{ex.Message}\n{ex.StackTrace}", $"{nameof(BuildZigZag)}[ERROR]", true, true);
                        }
                    }

                    Pre_id = ID_quark;
                }
            }  // Curr Leg dn

            if (TimeMaxASK < TimeMinBID) // DN zig zag
            {
                if (MaxASK - MinBID >= TradeZigZag.Leg && ask - MinBID >= TradeZigZag.Leg) // DN zig zag
                {
                    WaveDN = true;
                    WaveUP = false;
                    WaveCounter++;
                    StartPrice = MaxASK;
                    StartTime = TimeMaxASK;
                    FinishPrice = MinBID;
                    FinishTime = TimeMinBID;
                    zGPS = GPS;
                    MDZigDN = MinBidlistMD;
                    ZigZagPlot.Clear();
                    CurrLegPlot.Clear();
                    //InitZigZagArr();
                    //InitCurrLeg();//nulling CurrLeg
                    preID = 0;
                    FillingSmallZZ(ID_Q_MaxAsk, ID_Q_MinBid, StartTime, FinishTime, StartPrice, FinishPrice, "DN", tasLst, avGPS, zGPS);
                    TradeZigZag.avDevDNzz = CheckGapZZ(8, "DN");
                    TradeZigZag.lastZZ = "DN"; TradeZigZag.wave = WaveCounter;
                    //uz = CheckUZ(3);
                    //uzG = CheckUZ(5);
                    //uzG = CheckVZ(5);
                    uzz = CheckZigOrd();
                    MaxASK = ask;
                    TimeMaxASK = Time;
                    ID_Q_MaxAsk = ID_quark;
                    MaxASK_Vol = SumAsk;
                    //MainRankSell = BinanceExecution.FunDepthProba2(1);
                    RP = ResistancePrice;
                    RV = ResistanceVol;
                }
            } // DN Zig Zag

            if (WaveDN == true && TimeMinBID < Time)//for UP cur leg
            {
                if (LastID_Q_MaxAsk == 0)  LastID_Q_MaxAsk = ID_Q_MaxAsk;
                DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeMilliseconds((long)Time);
                string formattedTime = dateTimeOffset.ToString("HH:mm:ss.fff");
                string symb = TradeZigZag.Symbol; bool ChPointZZ = false;
                string FileName2 = "ZigFinishFuture" + symb;
                currentTime = DateTimeOffset.Now;

                if (ID_quark > Pre_id)
                {
                    if (ID_Q_MaxAsk > LastID_Q_MaxAsk)
                    {
                        KinchikPlot.Clear();

                        lock (lockObject)
                        {
                            FillingCurrentLeg(ID_Q_MinBid, ID_quark, MinBID, "up", tasLst);
                        }
                        
                        CrLg = GazerCurrLeg2("up", ask, bid, Time);

                        if(uzz == 1 && CrLg != "222") { TradeZigZag.OrderZig = 1; TradeZigZag.OrderZigTime = currentTime; }
                        else if (CrLg == "11") { TradeZigZag.OrderZig = 11; TradeZigZag.OrderZigTime = currentTime; }
                        else if (CrLg == "111") { TradeZigZag.OrderZig = 111; TradeZigZag.OrderZigTime = currentTime; }
                        else TradeZigZag.OrderZig = -10;

                        //string ser = "";
                        //if (uzG == "UP") ser = CheckLevel(ask, 2);
                        //if (uzG == "DN") ser = CheckLevel(ask, 1);
                        //if (ser == "DN") { TradeZigZag.OrderZig = 1;TradeZigZag.OrderZigTime = currentTime; } 
                        //else if (ser == "UP" && CrLg != "11"){ TradeZigZag.OrderZig = 2; TradeZigZag.OrderZigTime = currentTime; }
                        //else if (CrLg == "11") { TradeZigZag.OrderZig = 11; TradeZigZag.OrderZigTime = currentTime; }
                        //else TradeZigZag.OrderZig = -10;

                        //ChPointZZ = CheckPointZZ(ask, TradeZigZag.Leg * 0.15m);
                        //if(ChPointZZ)
                        //    new _().WriteToTxtFile($"Time: {formattedTime} | cur up | ChPoint={ChPointZZ} | Ask: {ask}, Bid: {bid}", "Point.txt");

                        //----------------------------------------
                        //if (TradeZigZag.StepByStepUP > 0 && TradeZigZag.StepByStepUP < 4 && (currentTime - TradeZigZag.StepByStepTime).TotalSeconds < 3)
                        //if (uzG == "UP")//uz == "UP" && 
                        //{
                        //    TradeZigZag.OrderZig = 2;
                        //    TradeZigZag.OrderZigTime = currentTime;
                        //    string umu = "";
                        //    //if (currLEG.Tip == "up" && (bid <= currLEG.PriceMaxBuy)) umu = "Right";//bid <= currLEG.PriceMaxSell || 
                        //    //else umu = "0";
                        //    //new _().WriteToTxtFile($"Time: {formattedTime} | cur up | buy |R:{umu} |Ask: {ask}, Bid: {bid}", "OrdZig.txt");
                        //}


                        //----------------------------------------

                        Triger = true;
                        LastID_Q_MaxAsk = ID_Q_MaxAsk;
                    }
                    else
                    {
                        try
                        {
                            if (ID_quark > ID_Q_MaxAsk)
                            {
                                FillingKnut(ID_Q_MaxAsk, ID_quark, MaxASK, "dn", tasLst); // kinchik dn
                                Kin = GazerKinchik("dn",ask,bid, Time);
                            }
                            currentTime = DateTimeOffset.Now;

                            if (Triger)
                            {
                                if (CrLg != "0") 
                                {
                                    FileName2 = "ZigFinishFuture" + symb;
                                    string formattedTimeN = currentTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    new _().WriteToTxtFile($"Time: {formattedTimeN} | ReversVol = {CrLg} | ChPoint={ChPointZZ} | Ask: {ask}, Bid: {bid}", FileName2);
                                    CrLg = "0";
                                    
                                    // WW = CrLg;//ZZF.Count;
                                }
                            }
                            string zzz = CheckZZ();
                            //----------------------------
                            //if (TradeZigZag.StepByStepDN > 0 && TradeZigZag.StepByStepDN < 4 && (currentTime - TradeZigZag.StepByStepTime).TotalSeconds < 3)
                            //{
                            //    TradeZigZag.OrderZig = 1;
                            //    TradeZigZag.OrderZigTime = currentTime;
                            //    new _().WriteToTxtFile($"Time: {formattedTime} | kin dn | sell | Ask: {ask}, Bid: {bid}", "OrdZig.txt");

                            //}

                            //----------------------------------------
                            if ((Kin == 1 || CrLg == "11" || CrLg == "110" || Kin == 111 || Kin == 1111) && Triger) // && TradeZigZag.Stakan == 1
                            {
                                if(CrLg == "11" || CrLg == "110") TradeZigZag.Order = CrLg;
                                else TradeZigZag.Order = Kin.ToString();
                                TradeZigZag.OrderTime = currentTime;
                                long T = (long)Time;
                                KinTime = DateTimeOffset.FromUnixTimeMilliseconds(T).DateTime; 
                                string formattedTimeN = currentTime.ToString("yyyy-MM-dd HH:mm:ss.fff");
                                formattedTime = KinTime.ToString("HH:mm:ss.fff");
                                string KinMsg = $"Time= {formattedTimeN} Kin= {Kin}  Ask= {ask}  Bid= {bid} curr time: {formattedTime}";

                                //string KinMsg = $"Time= {formattedTime} Kin= {Kin}  Ask= {ask},  Bid= {bid} ";
                                new _().WriteToTxtFile(KinMsg, "Kinchik");
                                Triger = false;
 
                            }
                        }
                        catch (Exception ex)
                        {
                            new _().WriteToTxtFile($"{ex.Message}\n{ex.StackTrace}", $"{nameof(BuildZigZag)}[ERROR]", true, true);
                        }
                    }

                    Pre_id = ID_quark;
                }
            } // Curr Leg up
        }
        #endregion
        string CheckZZ()
        {
            string res = "0";
            if (ZZF.Count - 4 >= 0)
            {
                decimal eW5 = ZZF[ZZF.Count - 1].FinishPrice; // wave 5
                decimal eW4 = ZZF[ZZF.Count - 1].FinishPrice; //  wave 4
                decimal eW3 = ZZF[ZZF.Count - 2].FinishPrice; //  wave 3
                decimal eW2 = ZZF[ZZF.Count - 3].FinishPrice; //  wave 2
                decimal eW1 = ZZF[ZZF.Count - 4].FinishPrice; //  wave 1
                decimal eW0 = ZZF[ZZF.Count - 4].StartPrice;



                //--------------- OLD -------------------------------------------------------- 
                if (ZZF[ZZF.Count - 1].Tip == "DN" && ZZF[ZZF.Count - 2].Tip == "UP" && ZZF[ZZF.Count - 3].Tip == "DN"
                     && ZZF[ZZF.Count - 4].Tip == "UP")
                {
                    if (eW2 > eW0 && eW3 > eW1 && eW4 > eW1) res = "ImpulseUP";
                    else if (eW2 > eW0 && eW3 > eW1 && eW4 < eW1 && eW4 > eW2) res = "KlinUP";

                    //decimal LenZig = zzL.FinishPrice - zzL.StartPrice; // zzL.Tip == "UP"
                    decimal LenCurr = currLEG.FinishPrice - currLEG.StartPrice; //Up curLeg


                } // Wave UP

                else if (ZZF[ZZF.Count - 1].Tip == "UP" && ZZF[ZZF.Count - 2].Tip == "DN" && ZZF[ZZF.Count - 3].Tip == "UP" 
                             && ZZF[ZZF.Count - 4].Tip == "DN")
                {
                    if (eW2 < eW0 && eW3 < eW1 && eW4 < eW1) res = "ImpulseDN";
                    if (eW2 < eW0 && eW3 < eW1 && eW4 > eW1 && eW4 < eW2) res = "KlinDN";
       
                    decimal LenCurr = currLEG.StartPrice - currLEG.FinishPrice; //Dn curLeg

                } // Wave DN

                //-----------------------

            }

             return res;
        }

        int CheckZigOrd()//string tip
        {
            int res = 0;
            var zzL = ZZF.LastOrDefault();
            if (zzL != null)
            {
                if (zzL.Tip == "UP")// && tip == "dn"
                {
                    if(zzL.AvgBuy > zzL.AvgSell && zzL.MaxBuyVol > zzL.AvgBuy)
                    {
                        res = 2;//possible impulse up
                    }
                    else if (zzL.AvgBuy < zzL.AvgSell && zzL.MaxSellVol > zzL.AvgSell)
                    {
                        res = 11;//possible finish up impulse
                    }
                    else if (zzL.MaxBuyVol < zzL.AvgBuy && zzL.MaxSellVol < zzL.AvgSell)
                    {
                        res = 101;//possible correction
                    }

                }

                if (zzL.Tip == "DN")// && tip == "up"
                {
                    if (zzL.AvgBuy < zzL.AvgSell && zzL.MaxSellVol > zzL.AvgSell)
                    {
                        res = 1;//possible impulse dn
                    }
                    else if (zzL.AvgBuy > zzL.AvgSell && zzL.MaxBuyVol > zzL.AvgBuy)
                    {
                        res = 22;//possible finish dn impulse   q
                    }
                    else if (zzL.MaxBuyVol < zzL.AvgBuy && zzL.MaxSellVol < zzL.AvgSell)
                    {
                        res = 202;//possible correction
                    }
                }
            }

            return res;
        }

        string CheckUZ(int n)
        {
            string res = "0";
            decimal maxL = 0; decimal L = 0; string tip = ""; string tipMax = "";

            int count = ZZF.Count;
            int itemsToCheck = Math.Min(count, n);

 
                for (int i = 1; i <= itemsToCheck; i++)
                {
                   if (ZZF[ZZF.Count - i].FinishPrice > 0 && ZZF[ZZF.Count - i].StartPrice > 0)
                   {
                     L = Math.Abs(ZZF[ZZF.Count - i].FinishPrice - ZZF[ZZF.Count - i].StartPrice);
                     tip = ZZF[ZZF.Count - i].Tip;
                   }
           
                   if (L > maxL)
                   {
                     maxL = L;
                     tipMax = tip;
                   }

                }

            res = tipMax;

            return res;
        }

        string CheckVZ(int n)
        {
            string res = "0"; 
            decimal Sbuy = 0, Ssell = 0;
            
            int count = ZZF.Count;
            int itemsToCheck = Math.Min(count, n);

            for (int i = 1; i <= itemsToCheck; i++)
            {
                if (ZZF[ZZF.Count - i].FinishPrice > 0 && ZZF[ZZF.Count - i].StartPrice > 0)
                {
                    Sbuy += ZZF[ZZF.Count - i].MaxBuyVol;
                    Ssell += ZZF[ZZF.Count - i].MaxSellVol;
                }
            }

            if (Sbuy > Ssell && ZZF[ZZF.Count - 1].AvgBuy > ZZF[ZZF.Count - 1].AvgSell) res = "UP";
            else if (Sbuy < Ssell && ZZF[ZZF.Count - 1].AvgBuy < ZZF[ZZF.Count - 1].AvgSell) res = "DN";

            return res;
        }
        string CheckLevel(decimal price, int Lev)
        {
            string res = "";  
            if (price > PriceMaxLevelSell && price > PriceMaxLevelBuy)
            {
                if (Lev == 2) res = "UP";
                else if (Lev == 1) res = "N";
            }
            else if(price < PriceMaxLevelSell && price < PriceMaxLevelBuy)
            {
                if (Lev == 1) res = "DN";
                else if (Lev == 2) res = "N";
            }
            else if(price < PriceMaxLevelSell && price > PriceMaxLevelBuy)
            {
                if (Lev == 1 && price - TradeZigZag.TP > PriceMaxLevelBuy) res = "DN";
                else if (Lev == 2 && PriceMaxLevelSell - TradeZigZag.TP > price) res = "UP";
                else res = "N";
            }
            else if (price > PriceMaxLevelSell && price < PriceMaxLevelBuy)
            {
                if (Lev == 2 && price < PriceMaxLevelBuy - TradeZigZag.TP) res = "UP";
                else if (Lev == 1 && PriceMaxLevelSell + TradeZigZag.TP < price) res = "DN";
                else res = "N";
            }
            return res;
        }
        decimal CheckGapZZ(int n, string tip)
        {
            decimal res = 0; int cnt = 0;
            decimal Sum = 0; decimal AvDv = 0, dev=0;  

            int count = ZZF.Count;
            int itemsToCheck = Math.Min(count, n);


            for (int i = 1; i <= itemsToCheck; i++)
            {
                if (tip == "UP" && tip == ZZF[ZZF.Count - i].Tip)
                {
                    if (ZZF[ZZF.Count - i].avGap != 0 && ZZF[ZZF.Count - i].Gap != 0)
                    {
                        dev = ZZF[ZZF.Count - i].Gap - ZZF[ZZF.Count - i].avGap;
                        Sum += dev;
                        cnt++;
                    }

 
                }
                else  if (tip == "DN" && tip == ZZF[ZZF.Count - i].Tip)
                {
                  if (ZZF[ZZF.Count - i].avGap != 0 && ZZF[ZZF.Count - i].Gap != 0)
                  {
                        dev = ZZF[ZZF.Count - i].Gap - ZZF[ZZF.Count - i].avGap;
                        Sum += dev;
                        cnt++;
                  }

                }
                
            }
            if (cnt > 0) AvDv = Sum / cnt;
            else AvDv = 0;

            res = AvDv;

            return res;
        }

        bool CheckPointZZ(decimal price, decimal Sensetiv)
        {
            bool res = false;
            if (ZZF.Count > 2)
            {
                // for (int e = ZZF.Count - 2; e <= ZZF.Count - 1; e++)
                int e = ZZF.Count - 1;
                
                   // if (Math.Abs(ZZF[e].FinishPrice - price) <= Sensetiv) { res = true; }

                    if (ZZF[e].Tip == "UP" && ZZF[e-1].Tip == "DN")
                    {
                       if (ZZF[e].FinishPrice - ZZF[e].StartPrice >= ZZF[e - 1].StartPrice - ZZF[e - 1].FinishPrice)
                       { 
                         if (Math.Abs(ZZF[e].StartPrice - price) <= Sensetiv) { res = true; } 
                       }
                    }
                    if (ZZF[e].Tip == "DN" && ZZF[e-1].Tip == "UP")
                    {
                       if (ZZF[e].StartPrice - ZZF[e].FinishPrice >= ZZF[e-1].FinishPrice - ZZF[e-1].StartPrice)
                       {
                        if (Math.Abs(ZZF[e].StartPrice - price) <= Sensetiv) { res = true; }
                       }
                    }
                
            }

            return res;
        }

        #region FillingSmallZZ func:

        public static Dictionary<decimal, Plot> ZigZagPlot = new Dictionary<decimal, Plot>();
        public List<Zig_Zag> ZZF = new List<Zig_Zag>();
        public struct NaborLeg
        {
            public string Tip { get; set; }
            public decimal StartPrice { get; set; }
            public decimal FinishPrice { get; set; }
            public long StartTime { get; set; }
            public long FinishTime { get; set; }
            public decimal PriceMaxBuy { get; set; }
            public decimal MaxBuyVol { get; set; }
            public decimal PriceMaxSell { get; set; }
            public decimal MaxSellVol { get; set; }
        }

       public NaborLeg currLEG = new NaborLeg();
       public NaborLeg KINCH = new NaborLeg();
        //  Zig_Zag Z_Z = new Zig_Zag();

        private void FillingSmallZZ(ulong ID_Q_Start, ulong ID_Q_Finish, 
                                     ulong StartTime, ulong FinishTime, decimal StartPr, decimal FinishPr, string tip, List<AggTradeFuture> tasLst, decimal avGP, decimal GP)
        {
            if ((tip == "UP" && ID_Q_Finish > ID_Q_Start && ID_Q_Finish < (ulong)tasLst.Count && ID_Q_Start > 0) ||
               (tip == "DN" && ID_Q_Finish > ID_Q_Start && ID_Q_Finish < (ulong)tasLst.Count && ID_Q_Start > 0))
            {
                List<AggTradeFuture> subList = default; 
                
                try
                {
                    var length = (int)(ID_Q_Finish - ID_Q_Start);
                    subList = tasLst.GetRange((int)ID_Q_Start - 1, length);
                }
                catch (Exception ex) { new _().WriteToTxtFile($"{ex.Message}\n{ex.StackTrace}", "FillingSmallZZ{tasLst.GetRange()}[ERROR]", true, true); }

                if (subList.Count != 0)
                {
                    int i = 0;
                    foreach (var item in subList)
                    {
                        try
                        {
                            var LastPrice = item.data.Price;
                            var Vol = item.data.Volume;
                            decimal x;
                            //==========================================
                            if (item != null)
                            {
                                decimal p = item.data.Price;
                                decimal marketBuy = item.data.MarketBuy;
                                decimal marketSell = item.data.MarketSell;
                                decimal key = p;

                                if (ZigZagPlot.TryGetValue(key, out var plot))
                                {
                                    if (plot != null)
                                    {
                                        plot.MarketBuy += marketBuy;
                                        plot.MarketSell += marketSell;
                                    }
                                }
                                else
                                {
                                    ZigZagPlot.Add(key, new Plot { MarketBuy = marketBuy, MarketSell = marketSell, Price = key });
                                }
                            }
                            //==========================================
                         }
                        catch (Exception ex)
                        {
                            new _().WriteToTxtFile($"{ex.Message}\n{ex.StackTrace}", $"{nameof(FillingSmallZZ)}[ERROR]", true, true);
                        }
                    }

                    //----------------- шукаю мах level ZigZag -----------------------
                    decimal PriceMaxSell = 0, MaxMarketSell = 0, PriceMaxBuy = 0, MaxMarketBuy = 0, avrgBuy = 0, avrgSell = 0;

                    if (ZigZagPlot != null)
                    {
                        // Знаходження максимального значення MaxMarketBuy та відповідної ціни PriceMaxBuy
                         var maxBuyEntry = ZigZagPlot.OrderByDescending(kv => kv.Value.MarketBuy).FirstOrDefault();
                         MaxMarketBuy = maxBuyEntry.Value.MarketBuy;
                         PriceMaxBuy = maxBuyEntry.Key;

                        // Знаходження максимального значення MaxMarketSell та відповідної ціни PriceMaxSell
                        var maxSellEntry = ZigZagPlot.OrderByDescending(kv => kv.Value.MarketSell).FirstOrDefault();
                        MaxMarketSell = maxSellEntry.Value.MarketSell;
                        PriceMaxSell = maxSellEntry.Key;

                        AddMarketLevel(MaxDataList, (double)MaxMarketBuy, (double)MaxMarketSell);
                        var avrBuy = CalculateAverage(MaxDataList, data => data.MaxMarketBuy);
                        var avrSell = CalculateAverage(MaxDataList, data => data.MaxMarketSell);
                        avrgSell = (decimal)avrSell;
                        avrgBuy = (decimal)avrBuy;
                        if (MaxMarketBuy > MaxLevelBuy) { MaxLevelBuy = MaxMarketBuy; PriceMaxLevelBuy = PriceMaxBuy; }
                        if (MaxMarketSell > MaxLevelSell) { MaxLevelSell = MaxMarketSell; PriceMaxLevelSell = PriceMaxSell; }

                    }
                    // ---------------------$$$$$$$-------------------------------------
                    Zig_Zag Z_Z = new Zig_Zag();

                    Z_Z.Tip = tip;
                    Z_Z.WaweCount = WaveCounter;
                    Z_Z.ID_Q_Start = ID_Q_Start;
                    Z_Z.ID_Q_Finish = ID_Q_Finish;
                    Z_Z.StartPrice = StartPr;
                    Z_Z.FinishPrice = FinishPr;
                    Z_Z.StartTime = (long)StartTime;
                    Z_Z.FinishTime = (long)FinishTime;
                    Z_Z.PriceMaxBuy = PriceMaxBuy;
                    Z_Z.MaxBuyVol = MaxMarketBuy;
                    Z_Z.PriceMaxSell = PriceMaxSell;
                    Z_Z.MaxSellVol = MaxMarketSell;
                    Z_Z.AvgBuy = avrgBuy;
                    Z_Z.AvgSell = avrgSell;
                    Z_Z.Gap = GP;
                    Z_Z.avGap = avGP;

                    ZZF.Add(new Zig_Zag
                    {
                        Tip = Z_Z.Tip,
                        WaweCount = Z_Z.WaweCount,
                        ID_Q_Start = Z_Z.ID_Q_Start,
                        ID_Q_Finish = Z_Z.ID_Q_Finish,
                        StartPrice = Z_Z.StartPrice,
                        FinishPrice = Z_Z.FinishPrice,
                        StartTime = Z_Z.StartTime,
                        FinishTime = Z_Z.FinishTime,
                        PriceMaxBuy = Z_Z.PriceMaxBuy,
                        MaxBuyVol = Z_Z.MaxBuyVol,
                        PriceMaxSell = Z_Z.PriceMaxSell,
                        MaxSellVol = Z_Z.MaxSellVol,
                        AvgBuy = Z_Z.AvgBuy,
                        AvgSell = Z_Z.AvgSell,
                        Gap = Z_Z.Gap,
                        avGap = Z_Z.avGap
                    }) ;

                    //-----------------------$$$$$$$--------------------------------------
                    //--------------------------------------------------------------------
                    string symb = TradeZigZag.Symbol;
                    string FileName = "MDLevel2Future" + symb;
                    string logMsg = null;
                    if (tip == "UP") logMsg = $"** UP ****************************************************";
                    if (tip == "DN") logMsg = $"** DN ****************************************************";
                 //   new _().WriteToTxtFile(logMsg, FileName);
                 //   new _().WriteToTxtFile($"WaveCounter: {WaveCounter}".ToString(), FileName);
                    var st = long.Parse(StartTime.ToString()).GetFullTime();
                    var ft = long.Parse(FinishTime.ToString()).GetFullTime();
                    logMsg = $"StartTime = {st.Hour}:{st.Minute}:{st.Second}.{st.Millisecond} FinishTime = {ft.Hour}:{ft.Minute}:{ft.Second}.{ft.Millisecond}";
                  //  new _().WriteToTxtFile(logMsg, FileName);
                    logMsg = $"StartPrice = {StartPrice} FinishPrice = {FinishPrice}";
                   // new _().WriteToTxtFile(logMsg, FileName);
 
                    var startRange = FinishPrice + TradeZigZag.SenseDist;
                    var finishRange = FinishPrice - TradeZigZag.SenseDist;

                    //!!! Stakan print full data

                    //if (tip == "UP")
                    //{
                    //    FileName = "MDLevel2Future" + symb;
                    //    var filteredItems = MDZigUP
                    //     .Where(pair => pair.Key <= startRange && pair.Key >= finishRange)
                    //     .ToList();

                    //    var type1Items = filteredItems
                    //        .Where(item => item.Value.Type == 1)
                    //        .OrderByDescending(item => item.Key)
                    //        .ToList();

                    //    foreach (var item in type1Items)
                    //    {
                    //        new _().WriteToTxtFile($"Type: {item.Value.Type} | Price: {item.Key}, " +
                    //            $"Volume: {item.Value.Volume}", FileName);
                    //    }
                    //    new _().WriteToTxtFile($" - Ask UP Bid - @", FileName);


                    //    var type2Items = filteredItems
                    //          .Where(item => item.Value.Type == 2)
                    //          .OrderByDescending(item => item.Key)
                    //          .ToList();

                    //    foreach (var item in type2Items)
                    //    {
                    //        new _().WriteToTxtFile($"Type: {item.Value.Type} | Price: {item.Key}, " +
                    //            $"Volume: {item.Value.Volume}", FileName);
                    //    }
                    //}
                    //if (tip == "DN")
                    //{
                    //    FileName = "MDLevel2Future" + symb;
                    //    var filteredItems = MDZigDN
                    // .Where(pair => pair.Key <= startRange && pair.Key >= finishRange)
                    //     .ToList();

                    //    var type1Items = filteredItems
                    //        .Where(item => item.Value.Type == 1)
                    //        .OrderByDescending(item => item.Key)
                    //        .ToList();

                    //    foreach (var item in type1Items)
                    //    {
                    //        new _().WriteToTxtFile($"Type: {item.Value.Type} | Price: {item.Key}, " +
                    //            $"Volume: {item.Value.Volume}", FileName);
                    //    }
                    //    new _().WriteToTxtFile($" - Ask DN Bid - @", FileName);
                    //    var type2Items = filteredItems
                    //        .Where(item => item.Value.Type == 2)
                    //        .OrderByDescending(item => item.Key)
                    //        .ToList();
                    //    foreach (var item in type2Items)
                    //    {
                    //        new _().WriteToTxtFile($"Type: {item.Value.Type} | Price: {item.Key}, " +
                    //            $"Volume: {item.Value.Volume}", FileName);
                    //    }
                    //}

                    //!!!
                    //!!! Stakan print agregate data
                   // MakeAggMD("ZigZag", tip, startRange, finishRange);
                    //!!! Stakan print finish

                    FileName = "SmallZZFuture" + symb;
                    logMsg = "";
                    if (tip == "UP") logMsg = $"** UP *** {WaveCounter} *********************************************";
                    if (tip == "DN") logMsg = $"** DN *** {WaveCounter} *********************************************";
                    new _().WriteToTxtFile(logMsg, FileName);
                    new _().WriteToTxtFile($"WaveCounter: {WaveCounter}".ToString(), FileName);
                    st = long.Parse(StartTime.ToString()).GetFullTime();
                    ft = long.Parse(FinishTime.ToString()).GetFullTime();
                    logMsg = $"StartTime = {st.Hour}:{st.Minute}:{st.Second}.{st.Millisecond} FinishTime = {ft.Hour}:{ft.Minute}:{ft.Second}.{ft.Millisecond}";
                    new _().WriteToTxtFile(logMsg, FileName);
                    logMsg = $"StartPrice = {StartPrice} FinishPrice = {FinishPrice} L = {FinishPrice - StartPrice}";
                    new _().WriteToTxtFile(logMsg, FileName);
                    
                    logMsg = $"GAP = {GP} ";
                    new _().WriteToTxtFile(logMsg, FileName);

                    if(ZZF.Count() >= 2) logMsg = $"avGAP = {avGP}  delt = {ZZF[ZZF.Count() - 1].avGap - ZZF[ZZF.Count() - 2].avGap}"; 
                    else logMsg = $"avGAP = {avGP} ";
                    new _().WriteToTxtFile(logMsg, FileName);

                    // --- print Max Level ------

                    var logMsgBuilderM = new System.Text.StringBuilder();
                    logMsgBuilderM.AppendLine($"PriceMaxMarketBuy = {PriceMaxBuy} Vol = {MaxMarketBuy}");
                    logMsgBuilderM.AppendLine($"PriceMaxMarketSell = {PriceMaxSell} Vol = {MaxMarketSell}");
                    logMsgBuilderM.AppendLine($"avrgMaxBuy = {avrgBuy} ");
                    logMsgBuilderM.AppendLine($"avrgMaxSell = {avrgSell} ");
                    logMsgBuilderM.AppendLine($"PriceMaxLevelBuy = {PriceMaxLevelBuy} MaxLevelBuy = {MaxLevelBuy} ");
                    logMsgBuilderM.AppendLine($"PriceMaxLevelSell = {PriceMaxLevelSell} MaxLevelSell = {MaxLevelSell} ");

                    logMsg = logMsgBuilderM.ToString();
                    new _().WriteToTxtFile(logMsg, FileName);
                    // -----------------------------------------------------

                    var a = tasLst[(int)ID_Q_Start - 1].data.Ask;
                    var b = tasLst[(int)ID_Q_Start - 1].data.Bid;
                    var a2 = tasLst[(int)ID_Q_Finish - 1].data.Ask;
                    var b2 = tasLst[(int)ID_Q_Finish - 1].data.Bid;

                    logMsg = $"ID_Q_Start: {ID_Q_Start} | ID_Q_Finish: {ID_Q_Finish}, AskS = {a} BidS = {b} AskF = {a2} BidF = {b2}";
                    new _().WriteToTxtFile(logMsg, FileName);

                    //^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                    var selectedItems = ZigZagPlot.Where(kv => kv.Value.Price > 0).ToList();

                    if (tip == "UP")
                    {
                        var selectedItemsSorted = selectedItems.OrderBy(kv => kv.Value.Price).ToList();
                        selectedItems = selectedItemsSorted;
                    }
                    else //if (tip == "DN")
                    {
                        var selectedItemsSortedDescending = selectedItems.OrderByDescending(kv => kv.Value.Price).ToList();
                        selectedItems = selectedItemsSortedDescending;
                    }

                    foreach (var kvp in selectedItems)
                    {
                        var logMsgBuilder = new System.Text.StringBuilder();
                        logMsgBuilder.AppendLine($"Price= {kvp.Value.Price}");
                        logMsgBuilder.AppendLine($"MarketBuy = {kvp.Value.MarketBuy}");
                        logMsgBuilder.AppendLine($"MarketSell = {kvp.Value.MarketSell}");

                        logMsg = logMsgBuilder.ToString();
                        new _().WriteToTxtFile(logMsg, FileName);
                    }

                }
            }
            else
            {
                new _().WriteToTxtFile("ID_Q_MaxAsk > ID_Q_MinBid || ID_Q_MinBid > found.Length",
                     "FillingSmallZZ[errors]", true, true);
                string logMsgEr = $"Pomilka ID_Q_Start= {ID_Q_Start}   ID_Q_Finish= {ID_Q_Finish}  Bin= {tasLst.Count}";
                new _().WriteToTxtFile(logMsgEr, "Pomilka");
            }
        }
        #endregion

        #region [FillingCurrentLeg]:
        int preID = 0;
        public static Dictionary<decimal, Plot> CurrLegPlot = new Dictionary<decimal, Plot>();
        private void FillingCurrentLeg(ulong ID_Extremum, ulong id, decimal StartPr, string tip, List<AggTradeFuture> tasLst)
        {

            if ((tip == "up" && id >= ID_Extremum && id <= (ulong)tasLst.Count && ID_Extremum > 0) ||
               (tip == "dn" && id >= ID_Extremum && id <= (ulong)tasLst.Count && ID_Extremum > 0))
            {
                lock(lockObject)
                {

                    if(preID == 0) { preID = (int)ID_Extremum; }

                    var length = (int)id - preID;
                    var subList1 = (tasLst.GetRange(preID - 1, length));
                    //var subList1 = BinanceExecution.list.Skip((int)(ID_Extremum - 1)).Take((int)(id - ID_Extremum + 1)).ToArray();
                    if (subList1.Count != 0)
                    {
                        int i = 0; int qq = 0;
                        preID = (int)id;
                        try
                        {
                            foreach (var item in subList1)
                            {
                                qq++;
                                //==========================================
                                if (item != null)
                                {
                                    decimal p = item.data.Price;
                                    decimal marketBuy = item.data.MarketBuy;
                                    decimal marketSell = item.data.MarketSell;
                                    decimal key = p;

                                    if (CurrLegPlot.TryGetValue(key, out var plot))
                                    {
                                        plot.MarketBuy += marketBuy;
                                        plot.MarketSell += marketSell;
                                    }
                                    else
                                    {
                                        CurrLegPlot.Add(key, new Plot { MarketBuy = marketBuy, MarketSell = marketSell, Price = key });
                                    }
                                }
                                //==========================================
                            }
                            var zzz = 0;
                        }
                        catch (Exception ex)
                        {
                            new _().WriteToTxtFile($"{ex.Message}\n{ex.StackTrace}", $"{nameof(FillingCurrentLeg)}[ERROR]", true, true);
                        }
                    }
                }
            }
            else
            {
                string logMsgEr = $"Pomilka ID_Q_Start= {ID_Extremum}   ID_Q_Finish= {id}  Bin= {tasLst.Count}";
                new _().WriteToTxtFile(logMsgEr, "PomilkaCurrLeg");
            }

        }
        #endregion

        #region FillingKnut:
        public static Dictionary<decimal, Plot> KinchikPlot = new Dictionary<decimal, Plot>();
     
        private void FillingKnut(ulong ID_Extremum, ulong id, decimal StartPr, string tip, List<AggTradeFuture> tasLst)
        {
            bool subIf = id > ID_Extremum;
            if ((tip == "up" && subIf && id <= (ulong)tasLst.Count && ID_Extremum > 0) ||
                (tip == "dn" && id > ID_Extremum && id <= (ulong)tasLst.Count && ID_Extremum > 0))
            {
                lock (lockObject)
                {
                    if (preID == 0) { preID = (int)ID_Extremum; }

                    var length = (int)id - preID;
                    var subList2 = (tasLst.GetRange(preID - 1, length));
                    //-----------------------------------------
                    if (subList2.Count != 0)
                    {
                        int i = 0; 
                        preID = (int)id;
                        try
                        {
                            foreach (var item in subList2)
                            {
                                //==========================================
                                if (item != null)
                                {
                                    decimal p = item.data.Price;
                                    decimal marketBuy = item.data.MarketBuy;
                                    decimal marketSell = item.data.MarketSell;
                                    decimal key = p;

                                    if (KinchikPlot.TryGetValue(key, out var plot))
                                    {
                                        plot.MarketBuy += marketBuy;
                                        plot.MarketSell += marketSell;
                                    }
                                    else
                                    {
                                        KinchikPlot.Add(key, new Plot { MarketBuy = marketBuy, MarketSell = marketSell, Price = key });
                                    }
                                }
                                //==========================================
                            }

                        }
                        catch (Exception ex)
                        {
                            new _().WriteToTxtFile($"{ex.Message}\n{ex.StackTrace}", $"{nameof(FillingCurrentLeg)}[ERROR]", true, true);
                        }
                    }
                }

            }
            else
            {
                string logMsgEr = $"Pomilka ID_Q_Start= {ID_Extremum}   ID_Q_Finish= {id}  Bin= {tasLst.Count}";
                try
                {
                    new _().WriteToTxtFile(logMsgEr, "PomilkaKnut");
                }
                catch (Exception ex)
                {
                    var dt = DateTime.Now;
                }
            }

        }
        #endregion

        #region [Scalp]:
        string Scalp(string tipKin, decimal Ask, decimal Bid )
        {
            string res = "0";

            var zzL = ZZF.LastOrDefault();
            if (zzL != null)
            {    
                if (zzL.Tip == "UP" && tipKin == "up")
                {
                    decimal LenZig = zzL.FinishPrice - zzL.StartPrice; // zzL.Tip == "UP"
                    decimal LenCurr = currLEG.StartPrice - currLEG.FinishPrice; //Dn curLeg

                    if (currLEG.Tip == "dn" && KINCH.Tip == "up")
                    {
                        if (currLEG.MaxBuyVol > currLEG.MaxSellVol && (currLEG.MaxBuyVol > zzL.AvgBuy || currLEG.MaxSellVol > zzL.AvgSell))
                        {
                            if (KINCH.MaxBuyVol >= zzL.AvgBuy*0.382m) res = "20";
                        }
                        if (currLEG.MaxBuyVol < currLEG.MaxSellVol && (currLEG.PriceMaxSell - currLEG.PriceMaxBuy > TradeZigZag.Leg*0.38m) && (currLEG.MaxSellVol > zzL.AvgBuy || currLEG.MaxSellVol > zzL.AvgSell))
                        {
                            if (KINCH.MaxBuyVol >= zzL.AvgBuy*0.382m) res = "202";
                        }
                        if (KINCH.MaxBuyVol >= zzL.AvgBuy)
                        {
                            res = "2"; TradeZigZag.wantProfit = LenZig * 0.6m;
                        }
                        if ((currLEG.FinishPrice >= zzL.PriceMaxBuy || currLEG.FinishPrice >= zzL.PriceMaxSell) && currLEG.MaxBuyVol > currLEG.MaxSellVol)//  && KINCH.MaxBuyVol > KINCH.MaxSellVol
                        {
                            if (KINCH.MaxBuyVol >= zzL.AvgBuy || KINCH.MaxBuyVol >= zzL.MaxBuyVol || KINCH.MaxBuyVol >= zzL.MaxSellVol)
                            {
                                res = "2222";
                                TradeZigZag.wantProfit = LenZig * 0.8m;
                            }
                        }
                        if (zzL.MaxBuyVol == MaxLevelBuy && currLEG.FinishPrice >= zzL.PriceMaxSell) //  && KINCH.MaxBuyVol > KINCH.MaxSellVol
                        {
                            if (KINCH.MaxBuyVol >= zzL.AvgBuy || KINCH.MaxBuyVol >= zzL.MaxBuyVol || KINCH.MaxBuyVol >= zzL.MaxSellVol)
                            {
                                res = "222";
                                TradeZigZag.wantProfit = LenZig * 0.8m;
                            }
                        }
                    }
                }

                if (zzL.Tip == "DN" && tipKin == "dn")
                {
                    decimal LenZig = zzL.StartPrice - zzL.FinishPrice;
                    decimal LenCurr = currLEG.FinishPrice - currLEG.StartPrice; //Up curLeg

                    if (currLEG.Tip == "up" && KINCH.Tip == "dn")
                    {
                        if (currLEG.MaxBuyVol < currLEG.MaxSellVol && (currLEG.MaxBuyVol > zzL.AvgBuy || currLEG.MaxSellVol > zzL.AvgSell))
                        {
                            if (KINCH.MaxSellVol >= zzL.AvgSell * 0.382m) res = "10";
                        }
                        if (currLEG.MaxBuyVol > currLEG.MaxSellVol && (currLEG.PriceMaxSell - currLEG.PriceMaxBuy > TradeZigZag.Leg * 0.38m) && (currLEG.MaxBuyVol > zzL.AvgBuy || currLEG.MaxSellVol > zzL.AvgSell))
                        {
                            if (KINCH.MaxSellVol >= zzL.AvgSell*0.382m) res = "101";
                        }
                        if (KINCH.MaxSellVol >= zzL.AvgSell) 
                        {
                            res = "1"; TradeZigZag.wantProfit = LenZig * 0.6m;
                        }
                        if (zzL.MaxSellVol == MaxLevelSell && currLEG.FinishPrice <= zzL.PriceMaxBuy)// && KINCH.MaxBuyVol < KINCH.MaxSellVol
                        {
                            if (KINCH.MaxSellVol >= zzL.AvgSell || KINCH.MaxSellVol >= zzL.MaxBuyVol || KINCH.MaxSellVol >= zzL.MaxSellVol)
                            {
                                res = "111";
                                TradeZigZag.wantProfit = LenZig * 0.8m;
                            }
                        }
                        if ((currLEG.FinishPrice <= zzL.PriceMaxBuy || currLEG.FinishPrice <= zzL.PriceMaxSell) && currLEG.MaxBuyVol < currLEG.MaxSellVol)//  && KINCH.MaxBuyVol > KINCH.MaxSellVol
                        {
                            if (KINCH.MaxSellVol >= zzL.AvgSell || KINCH.MaxSellVol >= zzL.MaxBuyVol || KINCH.MaxSellVol >= zzL.MaxSellVol)
                            {
                                res = "1111";
                                TradeZigZag.wantProfit = LenZig * 0.8m;
                            }
                        }
                    }
                }
               // var f=ZZF[ZZF.Count() - 2].PriceMaxBuy;
            }

            return res;
        }
        #endregion

        #region [GazerCurrLeg]:

        private string GazerCurrLeg2(string tip, decimal ask, decimal bid, ulong Time )
        {
            string res = "0", rrr=""; 
           // long T = (long)Time;
           // KinTime = DateTimeOffset.FromUnixTimeMilliseconds(T).DateTime;
            string formattedTimeN = currentTime.ToString("HH:mm:ss.fff");
           // formattedTime = KinTime.ToString("HH:mm:ss.fff");
            string time = $"Time= {formattedTimeN} ";

            if (CurrLegPlot.Any() && CurrLegPlot.Count > 1)
            {
                try
                {
                    //----------------- шукаю мах level CurrLeg --------------------------------------
                    // Знаходження максимального значення MaxMarketBuy та відповідної ціни PriceMaxBuy
                    var maxBuyEntry = CurrLegPlot.OrderByDescending(kv => kv.Value.MarketBuy).FirstOrDefault();
                    decimal MaxCurrBuy = maxBuyEntry.Value.MarketBuy;
                    decimal PriceMaxCurrBuy = maxBuyEntry.Key;

                    // Знаходження максимального значення MaxMarketSell та відповідної ціни PriceMaxSell
                    var maxSellEntry = CurrLegPlot.OrderByDescending(kv => kv.Value.MarketSell).FirstOrDefault();
                    decimal MaxCurrSell = maxSellEntry.Value.MarketSell;
                    decimal PriceMaxCurrSell = maxSellEntry.Key;

                    // Знаходження максимального значення Price та відповідної ціни PriceMax
                    var maxPriceEntry = CurrLegPlot.OrderByDescending(kv => kv.Value.Price).FirstOrDefault();
                    decimal MaxPrice = maxPriceEntry.Value.Price;
                    decimal CurrMaxPrice = maxPriceEntry.Key;

                    // Знаходження мінімального значення Price, яке більше нуля, та відповідної ціни PriceMinPrice
                    var minPriceEntry = CurrLegPlot.Where(kv => kv.Value.Price > 0).OrderBy(kv => kv.Value.Price).FirstOrDefault();
                    decimal MinPrice = minPriceEntry.Value.Price;
                    decimal CurrMinPrice = minPriceEntry.Key;
                    //---------------------------------------------------------------
                    bool shuxerCloseBuy_OpenSell = false, shuxerCloseSell_OpenBuy = false;
                
                    //if (MaxLevelSell < MaxLevelBuy && MaxCurrBuy > MaxCurrSell && MaxCurrBuy > MaxLevelSell && MaxCurrBuy > MaxLevelBuy) rrr = "11";
                   // { shuxerCloseSell_OpenBuy = true; shuxerCloseBuy_OpenSell = false; rrr = "22"; }

                    //----------------------------------------------------------------
                    decimal LenCrlg = 0.0m, LenZig=0;
                   
                    var zzL = ZZF.LastOrDefault();
                    if (zzL != null)
                    {  
                       bool fine = false;
                       string Ser = "0";
                       if (CurrMaxPrice > 0 && PriceMaxCurrSell > 0 && CurrMinPrice > 0 && PriceMaxCurrBuy > 0)
                       {
                            LenCrlg = CurrMaxPrice - CurrMinPrice;

                            if (zzL.Tip == "UP" && tip == "dn")
                            {
                                LenZig = zzL.FinishPrice - zzL.StartPrice;
                                decimal bod = LenZig - LenCrlg;
                                if (LenCrlg / bod > 0.3m && LenCrlg / bod < 0.65m) fine = true;
                                else fine = false;
                                //zzL.AvgBuy
                                currLEG = new NaborLeg
                                {
                                    Tip = tip,
                                    StartPrice = CurrMaxPrice, // StartPr,
                                    FinishPrice = CurrMinPrice,//FinishPr,
                                    StartTime = 0,// (long)StartTime,
                                    FinishTime = (long)Time,// (long)FinishTime,
                                    PriceMaxBuy = PriceMaxCurrBuy,
                                    MaxBuyVol = MaxCurrBuy,
                                    PriceMaxSell = PriceMaxCurrSell,
                                    MaxSellVol = MaxCurrSell
                                };
                                
                                //if (MaxLevelSell > MaxLevelBuy && MaxLevelSell < MaxCurrBuy){ shuxerCloseSell_OpenBuy = true; rrr = "22"; }
                                //if (MaxLevelSell < MaxLevelBuy && MaxLevelBuy < MaxCurrBuy){ shuxerCloseSell_OpenBuy = true; rrr = "22"; }
                                //if (MaxLevelSell > MaxLevelBuy && MaxLevelSell * 2 < MaxCurrSell) { rrr = "22"; }
                                if(MaxCurrBuy > MaxCurrSell && MaxCurrBuy > zzL.AvgSell){rrr = "22"; }
                                else if(MaxCurrBuy < MaxCurrSell && MaxCurrSell > zzL.AvgSell) { rrr = "111"; }
                                else if (CurrMinPrice < zzL.StartPrice && MaxCurrSell < zzL.AvgSell && MaxCurrBuy > zzL.AvgSell) rrr = "222";


                                if (MaxCurrBuy > zzL.AvgBuy || MaxCurrSell > zzL.AvgSell)
                                {
                                    if (((PriceMaxCurrSell - CurrMinPrice) * 3 < CurrMaxPrice - PriceMaxCurrSell) && (CurrMaxPrice - PriceMaxCurrBuy) * 3 < (PriceMaxCurrBuy - CurrMinPrice))
                                    {
                                        Ser = "curr dn обєм по краях";
                                        new _().WriteToTxtFile(time + Ser + $" MaxCurrBuy:{MaxCurrBuy} MaxCurrSell:{MaxCurrSell}" + $" fine={fine}", "CurrLegSentiment.txt");
                                    }
                                    if ((PriceMaxCurrBuy - CurrMinPrice) * 3 < CurrMaxPrice - PriceMaxCurrBuy && (PriceMaxCurrSell - CurrMinPrice) * 3 < CurrMaxPrice - PriceMaxCurrSell)
                                    {
                                        Ser = "curr dn обєм в останній треті"; 
                                        new _().WriteToTxtFile(time + Ser + $" MaxCurrBuy:{MaxCurrBuy} MaxCurrSell:{MaxCurrSell}" + $" fine={fine}", "CurrLegSentiment.txt");
                                    }
                                    if ((CurrMaxPrice - PriceMaxCurrSell) * 3 < PriceMaxCurrSell - CurrMinPrice && (CurrMaxPrice - PriceMaxCurrBuy) * 3 < PriceMaxCurrBuy - CurrMinPrice)
                                    {
                                        Ser = "curr dn обєм в першій треті"; 
                                        new _().WriteToTxtFile(time + Ser + $" MaxCurrBuy:{MaxCurrBuy} MaxCurrSell:{MaxCurrSell}" + $" fine={fine}", "CurrLegSentiment.txt");
                                    }
                                    if ((PriceMaxCurrBuy - CurrMinPrice) * 3 < (CurrMaxPrice - PriceMaxCurrBuy))
                                    {
                                        if (fine) Ser = "220";
                                        else Ser = "22";
                                        TradeZigZag.wantProfit = LenZig * 0.8m;
                                        new _().WriteToTxtFile(time + Ser + $" MaxCurrBuy:{MaxCurrBuy} MaxCurrSell:{MaxCurrSell}" + $" fine={fine}", "CurrLegSentiment.txt");
                                    }
                                }
                            }
                            if (zzL.Tip == "DN" && tip == "up")
                            {
                                LenZig = zzL.StartPrice - zzL.FinishPrice;
                                decimal bod = LenZig - LenCrlg;
                                if (LenCrlg / bod > 0.3m && LenCrlg / bod < 0.65m) fine = true;
                                else fine = false;

                                currLEG = new NaborLeg
                                {
                                    Tip = tip,
                                    StartPrice = CurrMinPrice, // StartPr,
                                    FinishPrice = CurrMaxPrice,//FinishPr,
                                    StartTime = 0,// (long)StartTime,
                                    FinishTime = (long)Time,// (long)FinishTime,
                                    PriceMaxBuy = PriceMaxCurrBuy,
                                    MaxBuyVol = MaxCurrBuy,
                                    PriceMaxSell = PriceMaxCurrSell,
                                    MaxSellVol = MaxCurrSell
                                };

                                //if (MaxLevelSell < MaxLevelBuy && MaxLevelBuy < MaxCurrSell){ shuxerCloseBuy_OpenSell = true; rrr = "11"; }
                                //if (MaxLevelSell > MaxLevelBuy && MaxLevelSell < MaxCurrSell){shuxerCloseBuy_OpenSell = true; rrr = "11";}
                                //if (MaxLevelSell < MaxLevelBuy && MaxLevelBuy*2 < MaxCurrBuy) { rrr = "11"; }
                                if (MaxCurrBuy < MaxCurrSell && MaxCurrSell > zzL.AvgBuy) { rrr = "11"; }
                                else if (MaxCurrBuy > MaxCurrSell && MaxCurrBuy > zzL.AvgBuy) { rrr = "222"; }
                                else if (CurrMaxPrice > zzL.StartPrice && MaxCurrBuy < zzL.AvgBuy && MaxCurrSell > zzL.AvgBuy) rrr = "111";

                                if (MaxCurrBuy > zzL.AvgBuy || MaxCurrSell > zzL.AvgSell)
                                {
                                    if (((CurrMaxPrice - PriceMaxCurrBuy) * 3 < PriceMaxCurrBuy - CurrMinPrice) && ((PriceMaxCurrSell - CurrMinPrice) * 3 < CurrMaxPrice - PriceMaxCurrSell))
                                    {
                                        Ser = "curr up обєм по краях";
                                        new _().WriteToTxtFile(time + Ser + $" fine={fine}" + $" MaxCurrBuy:{MaxCurrBuy} MaxCurrSell:{MaxCurrSell}", "CurrLegSentiment.txt");
                                    }
                                    if ((CurrMaxPrice - PriceMaxCurrSell) * 3 < PriceMaxCurrSell - CurrMinPrice && (CurrMaxPrice - PriceMaxCurrBuy) * 3 < PriceMaxCurrBuy - CurrMinPrice)
                                    {
                                        Ser = "curr up обєм в останній треті";
                                        new _().WriteToTxtFile(time + Ser + $" fine={fine}" + $" MaxCurrBuy:{MaxCurrBuy} MaxCurrSell:{MaxCurrSell}", "CurrLegSentiment.txt");
                                    }
                                    if ((PriceMaxCurrBuy - CurrMinPrice) * 3 < CurrMaxPrice - PriceMaxCurrBuy && (PriceMaxCurrSell - CurrMinPrice) * 3 < CurrMaxPrice - PriceMaxCurrSell)
                                    {
                                        Ser = "curr up обєм в першій треті";
                                        new _().WriteToTxtFile(time + Ser + $" fine={fine}" + $" MaxCurrBuy:{MaxCurrBuy} MaxCurrSell:{MaxCurrSell}", "CurrLegSentiment.txt");
                                    }
                                    if ((CurrMaxPrice - PriceMaxCurrSell) * 3 < (PriceMaxCurrSell - CurrMinPrice))
                                    {
                                        if (fine) Ser = "110";
                                        else Ser = "11";
                                        TradeZigZag.wantProfit = LenZig * 0.8m;
                                        new _().WriteToTxtFile(time + Ser + $" MaxCurrBuy:{MaxCurrBuy} MaxCurrSell:{MaxCurrSell}", "CurrLegSentiment.txt");
                                    }
                                }

                            }
                       }
                       res = rrr;
                    }
                } 
                catch (Exception e) { }
            } 
           

            return res;
        }

        #endregion

        #region GazerKinchik:
        private int GazerKinchik(string tip, decimal ask, decimal bid, ulong Time)
        {
           // decimal SumBuy = 0; decimal SumSell = 0;
            int res = 0;
            try
            {
                if (KinchikPlot.Any() && KinchikPlot.Count > 1)
                {
                    // try { }
                    //----------------- шукаю мах level CurrLeg --------------------------------------
                    // Знаходження максимального значення MaxMarketBuy та відповідної ціни PriceMaxBuy
                    var maxBuyEntry = KinchikPlot.OrderByDescending(kv => kv.Value.MarketBuy).FirstOrDefault();
                    decimal MaxKinchikBuy = maxBuyEntry.Value.MarketBuy;
                    decimal PriceMaxKinchikBuy = maxBuyEntry.Key;

                    // Знаходження максимального значення MaxMarketSell та відповідної ціни PriceMaxSell
                    var maxSellEntry = KinchikPlot.OrderByDescending(kv => kv.Value.MarketSell).FirstOrDefault();
                    decimal MaxKinchikSell = maxSellEntry.Value.MarketSell;
                    decimal PriceMaxKinchikSell = maxSellEntry.Key;

                    // Знаходження максимального значення Price та відповідної ціни PriceMax
                    var maxPriceEntry = KinchikPlot.OrderByDescending(kv => kv.Value.Price).FirstOrDefault();
                    decimal MaxPrice = maxPriceEntry.Value.Price;
                    decimal KinchikMaxPrice = maxPriceEntry.Key;

                    // Знаходження мінімального значення Price, яке більше нуля, та відповідної ціни PriceMinPrice
                    var minPriceEntry = KinchikPlot.Where(kv => kv.Value.Price > 0).OrderBy(kv => kv.Value.Price).FirstOrDefault();
                    decimal MinPrice = minPriceEntry.Value.Price;
                    decimal KinchikMinPrice = minPriceEntry.Key;
                    //----------------------------------------------------------------
                   if (tip == "up")
                   {
                      KINCH = new NaborLeg
                            {
                                Tip = tip,
                                StartPrice = KinchikMinPrice, // StartPr,
                                FinishPrice = KinchikMaxPrice,//FinishPr,
                                StartTime = currLEG.FinishTime,// (long)StartTime,
                                FinishTime = (long)Time,// (long)FinishTime,
                                PriceMaxBuy = PriceMaxKinchikBuy,
                                MaxBuyVol = MaxKinchikBuy,
                                PriceMaxSell = PriceMaxKinchikSell,
                                MaxSellVol = MaxKinchikSell
                            };
                   }
                   if (tip == "dn")
                   {
                      KINCH = new NaborLeg
                            {
                                Tip = tip,
                                StartPrice = KinchikMaxPrice, // StartPr,
                                FinishPrice = KinchikMinPrice,//FinishPr,
                                StartTime = currLEG.FinishTime,// (long)StartTime,
                                FinishTime = (long)Time,// (long)FinishTime,
                                PriceMaxBuy = PriceMaxKinchikBuy,
                                MaxBuyVol = MaxKinchikBuy,
                                PriceMaxSell = PriceMaxKinchikSell,
                                MaxSellVol = MaxKinchikSell
                            };
                   }

                    var rres = Scalp(tip, ask, bid);
                    res = int.Parse(rres);
                } 
            }
            catch (Exception ex)
            {
                new _().WriteToTxtFile($"{ex.Message}\n{ex.StackTrace}", $"{nameof(GazerKinchik)}[ERROR]", true, true);
            }
 
            return (res);
        }
        #endregion

        #region Init arrays:
        private void InitZigZagArr()
        {
            //ZigZagArr = new Plot[300000]; // Змінюємо масив на null
            for (var i = 0; i < ZigZagArr.Length; i++)
                ZigZagArr[i] = new Plot();
        }
        private void InitCurrLeg()
        {
            CurrLeg = new Plot[300000]; // Змінюємо масив на null
            for (var i = 0; i < CurrLeg.Length; i++)
                CurrLeg[i] = new Plot();
        }
        private void InitKinchik()
        {
            Kinchik = new Plot[100000]; // Змінюємо масив на null
            for (var i = 0; i < Kinchik.Length; i++)
                Kinchik[i] = new Plot();
        }
        #endregion

        #region Aggregate of Market Depth:
        public class AggregatedBookItem
        {
            public decimal Price { get; set; }
            public decimal AggregatedVolume { get; set; }
        }

        private List<AggregatedBookItem> aggregatedBookType1 = new List<AggregatedBookItem>();
        private List<AggregatedBookItem> aggregatedBookType2 = new List<AggregatedBookItem>();

        private void MakeAggMD(string Event, string type, decimal startRange, decimal finishRange)
        {
            ConcurrentDictionary<decimal, MarketDepthUpdateFuture> MDZigRRR = default;


            if (Event == "ZigZag")
            {
                string logMsg = null;
                if (type == "UP") logMsg = $"** UP ****************************************************";
                if (type == "DN") logMsg = $"** DN ****************************************************";
                new _().WriteToTxtFile(logMsg, "MDLevel2FutureAg");
                new _().WriteToTxtFile($"WaveCounter: {WaveCounter}".ToString(), "MDLevel2FutureAg");
                var st = long.Parse(StartTime.ToString()).GetFullTime();
                var ft = long.Parse(FinishTime.ToString()).GetFullTime();
                logMsg = $"StartTime = {st.Hour}:{st.Minute}:{st.Second}.{st.Millisecond} FinishTime = {ft.Hour}:{ft.Minute}:{ft.Second}.{ft.Millisecond}";
                new _().WriteToTxtFile(logMsg, "MDLevel2FutureAg");
                logMsg = $"StartPrice = {StartPrice} FinishPrice = {FinishPrice}";
                new _().WriteToTxtFile(logMsg, "MDLevel2FutureAg");

                if (type == "UP") MDZigRRR = MDZigUP;
                else if (type == "DN") MDZigRRR = MDZigDN;
            }
            else
            {
                if (type == "UP") MDZigRRR = MinBidlistMD;
                else MDZigRRR = MaxAsklistMD;
            }


            if (MDZigRRR != null)
            {
                // Type 1 Aggregation
                var type1Items = MDZigRRR
                    .Where(item => item.Value.Type == 1 && item.Key <= startRange && item.Key >= finishRange && item.Value.Volume > 0)
                    .OrderByDescending(item => item.Key)
                    .ToList();

                decimal interval = TradeZigZag.Cluster;
                decimal currentRangeStart = type1Items.First().Key;
                decimal BestAsk = type1Items.Last().Key;
                decimal currentRangeEnd = currentRangeStart - interval;
                decimal nextRangeStart;

                while (currentRangeEnd >= BestAsk) //finishRange
                {
                    var sumVolume = type1Items
                        .Where(item => item.Key < currentRangeStart && item.Key >= currentRangeEnd)
                        .Sum(item => item.Value.Volume);

                    if (Event == "ZigZag")
                    {
                        new _().WriteToTxtFile($"Type: 1 Price: {currentRangeEnd}, " +
                            $"Aggregated Volume: {sumVolume}", "MDLevel2FutureAg");
                    }

                    aggregatedBookType1.Add(new AggregatedBookItem
                    {
                        Price = currentRangeEnd,
                        AggregatedVolume = sumVolume
                    });

                    nextRangeStart = currentRangeEnd;
                    currentRangeStart = nextRangeStart;
                    currentRangeEnd = nextRangeStart - interval;
                    if (currentRangeStart > BestAsk && currentRangeEnd < BestAsk) currentRangeEnd = BestAsk;

                }

                // Type 2 Aggregation
                var type2Items = MDZigRRR
                    .Where(item => item.Value.Type == 2 && item.Key <= startRange && item.Key >= finishRange && item.Value.Volume > 0)
                    .OrderByDescending(item => item.Key)
                    .ToList();

                currentRangeStart = type2Items.First().Key;
                currentRangeEnd = currentRangeStart - interval;

                while (currentRangeEnd >= finishRange)
                {
                    var sumVolume = type2Items
                        .Where(item => item.Key < currentRangeStart && item.Key >= currentRangeEnd)
                        .Sum(item => item.Value.Volume);

                    if (Event == "ZigZag")
                    {
                        new _().WriteToTxtFile($"Type: 2 Price: {currentRangeEnd}, " +
                        $"Aggregated Volume: {sumVolume}", "MDLevel2FutureAg");
                    }

                    aggregatedBookType2.Add(new AggregatedBookItem
                    {
                        Price = currentRangeEnd,
                        AggregatedVolume = sumVolume
                    });

                    nextRangeStart = currentRangeEnd;
                    currentRangeStart = nextRangeStart;
                    currentRangeEnd = nextRangeStart - interval;
                }

            }
        }

        private decimal Look4MaxVPrice(int tip)
        {
            decimal res = 0.0m;
            if (tip == 1)
            {
                decimal maxAggregatedVolumePrice = aggregatedBookType1
               .OrderByDescending(item => item.AggregatedVolume)
               .Select(item => item.Price)
               .FirstOrDefault();
                return maxAggregatedVolumePrice;
            }
            else
            {
                decimal maxAggregatedVolumePrice2 = aggregatedBookType2
                .OrderByDescending(item => item.AggregatedVolume)
                .Select(item => item.Price)
                .FirstOrDefault();
                return maxAggregatedVolumePrice2;
            }
            return res;

        }

        #endregion

    

        #region PasportDeal func:
        public void PasportDeal(decimal Ask, decimal Bid)
        {


        }
        #endregion
    }
}
