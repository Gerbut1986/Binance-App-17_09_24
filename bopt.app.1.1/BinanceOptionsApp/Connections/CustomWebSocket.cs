namespace BinanceOptionsApp.Connections
{
    using Helpers;
    using System;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Threading.Tasks;
    using WebSocketSharp;

    public class CustomWebSocket
    {
        string server1IP = "194.146.38.45";
        int server1Port = 4090;

        string server2IP = "194.146.38.45";
        int server2Port = 5678;
        public async Task<string> SendMessageAndReceiveResponseAsync(string message)
        {
            try
            {
                using (var ws = new ClientWebSocket())
                {
                    // Підключення до сервера WebSocket
                    await ws.ConnectAsync(new Uri($"ws://{server1IP}:{server1Port}/"), CancellationToken.None);

                    // Відправлення текстового повідомлення
                    await SendTextMessageAsync(ws, message);

                    // Отримання відповіді від сервера
                    string response = await ReceiveTextMessageAsync(ws);

                    return response;
                }
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

        private async Task SendTextMessageAsync(ClientWebSocket ws, string message)
        {
            // Кодуємо текстове повідомлення в байти
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);

            // Відправляємо текстове повідомлення
            await ws.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task<string> ReceiveTextMessageAsync(ClientWebSocket ws)
        {
            // Буфер для зберігання отриманого повідомлення
            byte[] buffer = new byte[1024];

            // Отримання повідомлення від сервера
            WebSocketReceiveResult result = await ws.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            // Декодуємо отримане повідомлення з байтів в рядок
            string response = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);

            return response;
        }

        public void func1()
        {
            // IP адреса і порт першого сервера
            string server1IP = "67.217.228.39";
            int server1Port = 1234; // Порт першого сервера

            // IP адреса і порт другого сервера
            string server2IP = "67.217.228.39";
            int server2Port = 5678; // Порт другого сервера

            // Створення об'єкту WebSocket для підключення до першого сервера
            using (var ws1 = new WebSocketSharp.WebSocket($"ws://{server1IP}:{server1Port}/"))
            {
                // Обробник події відкриття з'єднання з першим сервером
                ws1.OnOpen += (sender, e) =>
                {
                    Console.WriteLine("Підключено до першого сервера!");
                    // Тут можна відправляти дані до першого сервера, якщо потрібно
                };

                // Обробник події отримання повідомлення від першого сервера
                ws1.OnMessage += (sender, e) =>
                {
                    Console.WriteLine($"Повідомлення від першого сервера: {e.Data}");
                    // Обробка отриманого повідомлення
                };

                // Підключення до першого сервера
                ws1.Connect();

                // Створення об'єкту WebSocket для підключення до другого сервера
                using (var ws2 = new WebSocketSharp.WebSocket($"ws://{server2IP}:{server2Port}/"))
                {
                    // Обробник події відкриття з'єднання з другим сервером
                    ws2.OnOpen += (sender, e) =>
                    {
                        Console.WriteLine("Підключено до другого сервера!");
                        // Тут можна відправляти дані до другого сервера, якщо потрібно
                    };

                    // Обробник події отримання повідомлення від другого сервера
                    ws2.OnMessage += (sender, e) =>
                    {
                        Console.WriteLine($"Повідомлення від другого сервера: {e.Data}");
                        // Обробка отриманого повідомлення
                    };

                    // Підключення до другого сервера
                    ws2.Connect();

                    // Тут можна реалізувати логіку обміну даними між серверами через веб-сокети

                    Console.ReadLine(); // Зупинка програми, щоб підключення залишилися відкритими
                }
            }
        }
    }
}
