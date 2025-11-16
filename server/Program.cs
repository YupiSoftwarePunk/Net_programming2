using System.Net;
using System.Net.Sockets;
using System.Text;

namespace server
{
    internal class Program
    {
        static void Main(string[] args)
        {
            TcpListener tcpListener = null;

            try
            {
                tcpListener = new TcpListener(IPAddress.Any, 8080);
                tcpListener.Start();

                Console.WriteLine("Сервер запущен и ожидает подключения...");

                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                Console.WriteLine("Клиент подключен");

                NetworkStream netStream = tcpClient.GetStream();

                Thread receiveThread = new Thread(() => ReceiveMessages(netStream));

                receiveThread.Start();

                SendMessage(netStream);

                receiveThread.Join();
                netStream.Close();
                tcpClient.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка на сервере: {ex.Message}");
            }
            finally
            {
                tcpListener?.Stop();
            }
        }



        static async Task ReceiveMessages(Stream netStream)
        {
            byte[] buffer = new byte[1024];

            try
            {
                while (true)
                {
                    int reader = netStream.Read(buffer, 0, buffer.Length);

                    if (reader == 0)
                    {
                        break;
                    }

                    string message = Encoding.UTF8.GetString(buffer, 0, reader);
                    Console.WriteLine($"[Сервер] Сообщение от клиента получено: {message}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!! Ошибка при приеме сообщения (сервер): {ex.Message}");
            }
        }


        static async Task SendMessage(Stream netStream)
        {
            try
            {
                while (true)
                {
                    string message = Console.ReadLine();

                    if (string.IsNullOrEmpty(message))
                        continue;

                    byte[] data = Encoding.UTF8.GetBytes(message);
                    netStream.Write(data, 0, data.Length);

                    if (message.ToLower() == "exit")
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!! Ошибка при отправке сообщения (сервер): {ex.Message}");
            }
        }
    }
}
