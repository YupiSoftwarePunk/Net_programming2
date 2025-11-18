using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private TcpClient client;
        private NetworkStream stream;
        private CancellationToken cts;
        public MainWindow()
        {
            InitializeComponent();
            _ = ConnectToServer();
        }


        private async void SendMessageClick(object sender, RoutedEventArgs e)
        {
            string message = MessageInput.Text;
            if (string.IsNullOrWhiteSpace(message)) return;

            byte[] data = Encoding.UTF8.GetBytes(message);
            await stream.WriteAsync(data, 0, data.Length);

            MessagesList.Items.Add($"Мое сообщение: {message}");
            MessageInput.Clear();

            if (message.ToLower() == "exit")
            {
                Close();
            }
        }


        private async Task ConnectToServer()
        {
            try
            {
                client = new TcpClient();
                await client.ConnectAsync("192.168.31.146", 8080);

                stream = client.GetStream();
                MessagesList.Items.Add("Подключено к серверу.");
                MessageInput.Clear();

                cts = new CancellationToken();
                _ = Task.Run(() => ReceiveMessage(cts));
            }
            catch (Exception ex)
            {
                MessagesList.Items.Add($"Ошибка подключения: {ex.Message}");
            }
        }


        private async Task ReceiveMessage(CancellationToken token)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (!token.IsCancellationRequested)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    MessagesList.Items.Add($"[Сервер]: {message}");
                }
            }
            catch (Exception ex)
            {
                MessagesList.Items.Add($"Ошибка при получении сообщения: {ex.Message}");
            }
        }
    }
}