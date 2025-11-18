using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace server
{
    internal class Program
    {
        public static readonly List<TcpClient> clients = new List<TcpClient>();
        private static readonly object locker = new object();

        static async Task Main(string[] args)
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, 8080);
            tcpListener.Start();

            Console.WriteLine("Сервер запущен и ожидает подключения...");

            while (true)
            {
                int i = 1;
                TcpClient tcpClient = await tcpListener.AcceptTcpClientAsync();
                
                lock (locker)
                {
                    clients.Add(tcpClient);
                    Console.WriteLine($"Клиент {i} подключен.");
                    i++;
                }

                _ = ReceiveMessages(tcpClient);
                
            }
        }


        private static async Task ReceiveMessages(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int reader = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (reader == 0)
                    {
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, reader);
                    Console.WriteLine("Сообщение от клиента: " + message);
                    await SendMessageAsync(client, message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("!! Возникла ошибка при обработке клиента: " + ex.Message);
            }
            finally
            {
                lock (locker)
                {
                    clients.Remove(client);
                }
                client.Close();
            }
        }


        private static async Task SendMessageAsync(TcpClient sender, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);

            lock (locker)
            {
                foreach (var clientt in clients)
                {
                    if (clientt != sender)
                    {
                        try
                        {
                            NetworkStream stream = clientt.GetStream();
                            stream.WriteAsync(buffer, 0, buffer.Length);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка при отправке сообщения клиенту: {ex.Message}");
                        }
                    }
                }
            }

        }
    }
}
