namespace BinanceOptionsApp
{
    using global::MultiTerminal.Connections;
    using System.Collections.Generic;
    using System.Windows;
    using WebSocketSharp;

    public partial class Socket_Wnd : Window
    {
        public static bool IsClient = false;
        public static WebSocket WebsocketClient1 { get; set; }
        public static WebSocket WebsocketClient2 { get; set; }
        public static WebSocket WebsocketClient3 { get; set; }
        private readonly BinanceFutureClient futureClient;
        private string _symbol;
        private bool _isRun = false;

        public Socket_Wnd(BinanceFutureClient bfc, string symbol)
        {
            InitializeComponent();
            if (bfc != null) _isRun = true;
            futureClient = bfc;
            _symbol = symbol;
        }

        private void Submit_Btn_Click(object sender, RoutedEventArgs e)
        {
            if (_isRun)
            {
                if (Ip1_Tb.Text != null && Port1_Tb.Text != null)
                {
                    futureClient.SubscribeBookTickerStream(_symbol);
                }
                //...ip2,ip3...
            }
            else this.Close();
        }

        private void IsClient_Chb_Checked(object sender, RoutedEventArgs e)
        {
            if (IsClient_Chb.IsChecked == true)
                IsClient = true;
            else IsClient = false;
        }
    }
}
