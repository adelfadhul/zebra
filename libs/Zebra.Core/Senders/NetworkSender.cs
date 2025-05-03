using System.Net.Sockets;
using System.Text;

namespace Zebra.Core.Senders
{
    public class NetworkSender
    {


        public async Task SendToNetworkPrinterAsync(string ip, int port, string zpl)
        {
            try
            {
                using TcpClient client = new();
                await client.ConnectAsync(ip, port);
                using NetworkStream stream = client.GetStream();
                byte[] buffer = Encoding.UTF8.GetBytes(zpl);
                await stream.WriteAsync(buffer);
                Console.WriteLine("✅ Label sent to network printer.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Network error: {ex.Message}");
            }
        }
    }
}
