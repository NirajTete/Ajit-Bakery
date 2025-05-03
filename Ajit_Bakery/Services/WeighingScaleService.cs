using System.Net.Sockets;
using System.Text;

namespace Ajit_Bakery.Services
{


    public class WeighingScaleService    {
        
        private readonly IConfiguration _config;
        public WeighingScaleService(IConfiguration config)
        {
            _config = config;
        }
        public async Task<string> ReadWeightAsync()
        {
            string ip = _config["AppSettings:weight"];
            int port = Convert.ToInt32(_config["AppSettings:port"]);

            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(ip, port);
                if (!client.Connected)
                {
                    return "Error: Unable to connect to the scale.";
                }

                using var stream = client.GetStream();
                byte[] buffer = new byte[1024];
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                string weightData = Encoding.ASCII.GetString(buffer, 0, bytesRead);

                // Remove all control characters (including \u0002)
                string cleanWeight = new string(weightData.Where(c => !char.IsControl(c)).ToArray());

                return cleanWeight.Trim();
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }

    }

}
