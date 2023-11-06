using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CSGameServerPractice
{
    class Server
    {
        private static Socket serverSocket;
        private static List<Socket> clients = new List<Socket>();

       

        public void StartServer()
        {
            serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(new IPEndPoint(IPAddress.Any, 12345));
            serverSocket.Listen(10);

            Thread acceptThread = new Thread(AcceptClients);
            acceptThread.Start();
        }

        private static void AcceptClients()
        {
            while (true)
            {
                SocketAsyncEventArgs acceptEventArg = new SocketAsyncEventArgs();
                acceptEventArg.Completed += AcceptCompleted;

                if (!serverSocket.AcceptAsync(acceptEventArg))
                {
                    ProcessAccept(acceptEventArg);
                }
            }
        }

        private static void AcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessAccept(e);
        }

        private static void ProcessAccept(SocketAsyncEventArgs e)
        {
            Socket client = e.AcceptSocket;
            clients.Add(client);
            Console.WriteLine("Client connected: " + client.RemoteEndPoint);

            e.AcceptSocket = null; // Reset for next accept
            AcceptClients();

            Thread receiveThread = new Thread(ReceiveFromClient);
            receiveThread.Start(client);
        }

        private static void ReceiveFromClient(object clientObj)
        {
            Socket client = (Socket)clientObj;
            byte[] buffer = new byte[1024];

            while (true)
            {
                try
                {
                    SocketAsyncEventArgs receiveEventArg = new SocketAsyncEventArgs();
                    receiveEventArg.SetBuffer(buffer, 0, buffer.Length);
                    receiveEventArg.UserToken = client;
                    receiveEventArg.Completed += ReceiveCompleted;

                    if (!client.ReceiveAsync(receiveEventArg))
                    {
                        ProcessReceive(receiveEventArg);
                    }
                }
                catch (SocketException)
                {
                    Console.WriteLine("Client disconnected: " + client.RemoteEndPoint);
                    clients.Remove(client);
                    client.Close();
                    break;
                }
            }
        }

        private static void ReceiveCompleted(object sender, SocketAsyncEventArgs e)
        {
            ProcessReceive(e);
        }

        private static void ProcessReceive(SocketAsyncEventArgs e)
        {
            Socket client = (Socket)e.UserToken;

            if (e.BytesTransferred > 0 && e.SocketError == SocketError.Success)
            {
                byte[] receivedData = new byte[e.BytesTransferred];
                Array.Copy(e.Buffer, e.Offset, receivedData, 0, e.BytesTransferred);
                string message = Encoding.UTF8.GetString(receivedData);

                Console.WriteLine("Received from " + client.RemoteEndPoint + ": " + message);

                foreach (var otherClient in clients)
                {
                    if (otherClient != client)
                    {
                        byte[] response = Encoding.UTF8.GetBytes(message);
                        SocketAsyncEventArgs sendEventArg = new SocketAsyncEventArgs();
                        sendEventArg.SetBuffer(response, 0, response.Length);
                        sendEventArg.UserToken = otherClient;
                        sendEventArg.Completed += SendCompleted;

                        if (!otherClient.SendAsync(sendEventArg))
                        {
                            ProcessSend(sendEventArg);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("Client disconnected: " + client.RemoteEndPoint);
                clients.Remove(client);
                client.Close();
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