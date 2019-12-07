using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Dns;

namespace Core
{
    public class StubResolver
    {
        private const int listenPort = 10053;

        public async Task StartListener()
        {
            var listener = new UdpClient(listenPort);

            try
            {
                while (true)
                {
                    Console.WriteLine("Waiting for broadcast");

                    //synchronous
                    //IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, listenPort);
                    //var bytes = listener.Receive(ref endpoint);

                    var result = await listener.ReceiveAsync();
                    var bytes = result.Buffer;
                    var endpoint = result.RemoteEndPoint;

                    Console.WriteLine($"Received broadcast from {endpoint} :");
                    Console.WriteLine($"{Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");

                    //todo: queue query and process async/in another thread
                    var query = DnsMessage.Parse(bytes);
                    Console.WriteLine(query.ToString());
                }
            }
            catch (SocketException se)
            {
                Console.WriteLine("SocketException:");
                Console.WriteLine(se);
            }
            finally
            {
                listener.Close();
            }
        }


    }
}
