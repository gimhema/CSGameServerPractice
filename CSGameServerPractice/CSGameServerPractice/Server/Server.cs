using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace CSGameServerPractice
{
    class Server
    {
        private static Socket serverSocket;
        private static List<Socket> clientList = new List<Socket>();
        private static Queue<byte[]> sendQueue = new Queue<byte[]>();
        public void Run()
        {
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            IPAddress ipAddress = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, 80);

            serverSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            serverSocket.Bind(endPoint);
            serverSocket.Listen(1);

            while (true)
            {
                SocketAsyncEventArgs eventArgs = new SocketAsyncEventArgs();
                eventArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted);

                if ( !serverSocket.AcceptAsync(eventArgs) )
                {
                    RegisterAccept(eventArgs);
                }
            }

        }

        private void RegisterAccept(SocketAsyncEventArgs eventArgs)
        {
            eventArgs.AcceptSocket = null;

            bool pending = serverSocket.AcceptAsync(eventArgs);
            if (pending == false) {
                OnAcceptCompleted(null, eventArgs);
            }
        }

        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if( args.SocketError == SocketError.Success )
            {
                clientList.Add(args.AcceptSocket);
                
                if (args.AcceptSocket != null )
                {
                    Console.WriteLine("New Cliet Accpeted ");
                    if ( args.AcceptSocket.RemoteEndPoint != null ) 
                    {
                        Console.WriteLine("RemoteEndPoint : " + args.AcceptSocket.RemoteEndPoint.ToString());
                    }
                    if (args.AcceptSocket.LocalEndPoint != null )
                    {
                        Console.WriteLine("LocalEndPoint : " + args.AcceptSocket.LocalEndPoint.ToString());
                    }
                }

                SocketAsyncEventArgs recvArgs = new SocketAsyncEventArgs();
                recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnRecvCompleted);
                recvArgs.SetBuffer(new byte[1024], 0, 1024);
                recvArgs.UserToken = args.AcceptSocket;

                RegisterRecv(recvArgs);
                RegisterAccept(args);

            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }
        }

        private void RegisterRecv(SocketAsyncEventArgs eventArgs)
        {
            Socket client = eventArgs.UserToken as Socket;
            bool pending = client.ReceiveAsync(eventArgs);
            if (pending == false)
            {
                OnRecvCompleted(null, eventArgs);
            }
        }

        private void OnRecvCompleted(object sender, SocketAsyncEventArgs eventArgs)
        {
            if (eventArgs.BytesTransferred > 0 && eventArgs.SocketError == SocketError.Success )
            {
                string recvData = System.Text.Encoding.UTF8.GetString(eventArgs.Buffer,
                    eventArgs.Offset, eventArgs.BytesTransferred);

                

                Console.WriteLine("RECEV : " + recvData);

                byte[] sendArray = Encoding.UTF8.GetBytes(recvData);
                sendQueue.Enqueue(sendArray);

                RegisterSend();
            }
            RegisterRecv(eventArgs);
        }

        private void RegisterSend()
        {
            byte[] buff = sendQueue.Dequeue();

            for (int i = 0; i < clientList.Count; i++)
            {
                SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
                sendArgs.SetBuffer(buff, 0, buff.Length);
                sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);                

//                Console.WriteLine("Send Buffer : " + buff.ToString());

                bool pending = clientList[i].SendAsync(sendArgs);

                if(pending == false)
                {
                    OnSendCompleted(null, sendArgs);
                }
            }
        }

        private void OnSendCompleted( object send, SocketAsyncEventArgs eventargs )
        {
            if(eventargs.BytesTransferred > 0 && eventargs.SocketError == SocketError.Success )
            {
                if(sendQueue.Count > 0)
                {

                }
            }
        }

        private void OnApplicationQuit()
        {
            for (int i = 0; i < clientList.Count; i++)
            {
                clientList[i].Shutdown(SocketShutdown.Both);
                clientList[i].Close();
            }
            if (serverSocket != null)
            {
                serverSocket.Close();
            }
        }

    }
}