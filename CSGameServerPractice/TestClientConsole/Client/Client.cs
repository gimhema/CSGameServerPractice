using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CSTestClient
{
    class Client
    {
        private Socket clientSocket;
        private bool isConnect = false;
        private SocketAsyncEventArgs sendEventArgs;
//        private SocketAsyncEventArgs recvEventArgs;

        public void Run()
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress iPAddress = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(iPAddress, 80);

            clientSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += OnConnectCompleted;
            args.RemoteEndPoint = endPoint;

            clientSocket.ConnectAsync(args);
            isConnect = true;

            while (true)
            {
                string message = Console.ReadLine();
                if (message.ToLower() == "exit")
                {
                    clientSocket.Close();
                    isConnect = false;
                    break;
                }

                byte[] data = Encoding.UTF8.GetBytes(message);
                SocketAsyncEventArgs sendEventArg = new SocketAsyncEventArgs();
                sendEventArg.SetBuffer(data, 0, data.Length);
                sendEventArg.Completed += OnSendCompleted;

                if (!clientSocket.SendAsync(sendEventArg))
                {
                    SendMsg(message);
                }
            }
        }

        private void OnConnectCompleted(object obj, SocketAsyncEventArgs args)
        {
            if ( args.SocketError == SocketError.Success )
            {
                sendEventArgs = new SocketAsyncEventArgs();
                sendEventArgs.Completed += OnSendCompleted;

                SocketAsyncEventArgs recvEventArgs = new SocketAsyncEventArgs();
                recvEventArgs.SetBuffer(new byte[1024], 0, 1024);
                recvEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);

                bool pending = clientSocket.ReceiveAsync(recvEventArgs);
                if(pending == false)
                {
                    OnRecvCompleted(null, recvEventArgs);
                }
            }
        }

        public void OnRecvCompleted(object obj, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                string recvData = System.Text.Encoding.UTF8.GetString(args.Buffer, args.Offset, args.BytesTransferred);

                Console.WriteLine("RECV : ", recvData);


                bool pending = clientSocket.ReceiveAsync(args);
                if (pending == false)
                    OnRecvCompleted(null, args);
            }
        }

        public void SendMsg(string message)
        {
            if (message.Equals(string.Empty) == false)
            {
                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(message);
                sendEventArgs.SetBuffer(buffer, 0, buffer.Length);
                clientSocket.SendAsync(sendEventArgs);
            }
        }

        public void OnSendCompleted(object obj, SocketAsyncEventArgs args)
        {
            if (args.BytesTransferred > 0 && args.SocketError == SocketError.Success)
            {
                sendEventArgs.BufferList = null;
            }
        }


    }
}