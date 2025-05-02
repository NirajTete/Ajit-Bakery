using System.Net.Sockets;
using System.Text;

namespace Ajit_Bakery.Services
{
    public class WeighingScaleService
    {
        private readonly string ip = "192.168.0.100"; // Replace with your converter's IP
        private readonly int port = 4001; // Replace with your converter's port

        public async Task<string> ReadWeightAsync()
        {
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ip, port);

                using var stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string weightData = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // TODO: Adjust parsing depending on your scale's output format
                return weightData.Trim();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }

}
