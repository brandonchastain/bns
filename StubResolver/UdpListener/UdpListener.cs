using Bns.StubResolver.Common;
using Bns.StubResolver.Udp.Contracts;
using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Bns.StubResolver.Udp
{
    public class UdpListener
    { 
        private readonly int listenPort;
        private ProcessMessageAsync processMessageCallback;
        private UdpClient listener;

        public delegate Task<IByteSerializable> ProcessMessageAsync(UdpMessage message);

        public UdpListener(ProcessMessageAsync processMessage, ushort port)
        {
            this.processMessageCallback = processMessage ?? throw new ArgumentNullException(nameof(processMessage));
            this.listenPort = port;
            this.listener = new UdpClient(listenPort);
        }

        public async Task Start(CancellationToken cancellationToken)
        {
            await TaskUtil.RunAndWaitForCancel(
                this.ListenAndProcessDatagrams(cancellationToken),
                cancellationToken,
                this.Stop);
        }

        public void Stop()
        {
            this.listener.Close();
        }

        private async Task ListenAndProcessDatagrams(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var receiveTask = this.listener.ReceiveAsync();
                var udpMessage = await receiveTask;
                var bytes = udpMessage.Buffer;
                var endpoint = udpMessage.RemoteEndPoint;

                try
                {
                    var response = await this.processMessageCallback(new UdpMessage(bytes, endpoint));
                    if (response == null)
                    {
                        Console.WriteLine($"An error occurred while processing the UDP message.");
                    }

                    var responseBytes = response.ToByteArray();
                    await listener.SendAsync(responseBytes, responseBytes.Length, endpoint).ConfigureAwait(false);
                }
                catch (ObjectDisposedException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
