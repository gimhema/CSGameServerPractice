using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CSTestClient
{
    class Client
    {

        public void StartClient()
        {
            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect(new IPEndPoint(IPAddress.Loopback, 12345));

            Console.WriteLine("Connected to server. Type 'exit' to quit.");

            while (true)
            {
                string message = Console.ReadLine();
                if (message.ToLower() == "exit")
                {
                    client.Close();
                    break;
                }

                byte[] data = Encoding.UTF8.GetBytes(message);
                SocketAsyncEventArgs sendEventArg = new SocketAsyncEventArgs();
                sendEventArg.SetBuffer(data, 0, data.Length);
                sendEventArg.Completed += SendCompleted;

                if (!client.SendAsync(sendEventArg))
                {
                    ProcessSend(sendEventArg);
                }
            }
        }

        private static void SendCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessSend(e);
        }

        private static void ProcessSend(SocketAsyncEventArgs e)
        {
            // Handle send completion
        }
    }
}