using Dns;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core
{
    public class UdpListener
    {
        private readonly int listenPort;
        private Func<UdpMessage, DnsMessage> processMessage;
        private List<UdpMessage> messageQueue;

        public UdpListener(Func<UdpMessage, DnsMessage> processMessage, ushort port)
        {
            this.processMessage = processMessage ?? throw new ArgumentNullException(nameof(processMessage));
            this.listenPort = port;
            this.messageQueue = new List<UdpMessage>();
        }

        public async Task ListenAndProcessDatagrams(CancellationToken cancellationToken)
        {
            var listener = new UdpClient(listenPort);

            var taskCompletionSource = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(() => taskCompletionSource.TrySetResult(true)))
            {
                try
                {
                    while (true)
                    {
                        var receiveTask = listener.ReceiveAsync();
                        var cancelTask = taskCompletionSource.Task;

                        var completedTask = await Task.WhenAny(receiveTask, cancelTask);

                        if (completedTask == cancelTask)
                        {
                            // Console.WriteLine("Task cancellation requested.");
                            listener.Close();
                            cancellationToken.ThrowIfCancellationRequested();
                        }

                        var udpMessage = await receiveTask;
                        var bytes = udpMessage.Buffer;
                        var endpoint = udpMessage.RemoteEndPoint;

                        Console.WriteLine($"Received message from {endpoint} :");
                        Console.WriteLine($"{Encoding.ASCII.GetString(bytes, 0, bytes.Length)}");

                        var response = this.processMessage(new UdpMessage(bytes, endpoint));
                        if (response == null)
                        {
                            Console.WriteLine($"An error occurred while processing the UDP message.");
                        }

                        var responseBytes = response.ToByteArray();
                        await listener.SendAsync(responseBytes, responseBytes.Length, endpoint);
                    }
                }
                catch (OperationCanceledException)
                {
                    // close the listener and exit
                }
                catch (SocketException se)
                {
                    Console.WriteLine(se);
                }
                finally
                {
                    listener.Close();
                }
            }
        }

        private bool QueueMessage(byte[] message, IPEndPoint endpoint)
        {
            var item = new UdpMessage(message, endpoint);
            messageQueue.Add(item);
            return true;
        }
    }
}
