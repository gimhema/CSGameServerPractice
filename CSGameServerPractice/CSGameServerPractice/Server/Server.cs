using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MessageHandler;

namespace CSGameServerPractice
{
    class Server
    {
        private static Socket serverSocket;
        private static List<Socket> clientList = new List<Socket>();

        public static GameMessageHandler gameMessageHandler = new GameMessageHandler();
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
                // 나중에 recv 처리하면서 pop 해야함
                WriteMessageToRecvQueue(sendArray);

                RegisterSend();
            }
            RegisterRecv(eventArgs);
        }

        private void RegisterSend()
        {
            //  byte[] buff = sendQueue.Dequeue();
            byte[] buff = FetchMessageFromRecvQueue(); // 일단 지금은 에코라..

            for (int i = 0; i < clientList.Count; i++)
            {
                SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
                sendArgs.SetBuffer(buff, 0, buff.Length);
                sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);                

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
                byte[] buff = eventargs.Buffer;
                string echoMsg = buff.ToString();
                Console.WriteLine("Echo Msg : " + echoMsg);

//                if ( gameMessageHandler.GetSendQueueCapacity() > 0 /*sendQueue.Count > 0*/)
//                {
//
//                }
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

        private void WriteMessageToSendQueue(byte[] message)
        {
            gameMessageHandler.PushMessageToSendQueue(message);
        }

        private byte[] FetchMessageFromSendQueue()
        {
            return gameMessageHandler.DequeueMessageFromSendQueue();
        }

        private void WriteMessageToRecvQueue(byte[] message)
        {
            gameMessageHandler.PushMessageToRecvQueue(message);
        }

        private byte[] FetchMessageFromRecvQueue()
        {
            return gameMessageHandler.DequeueMessageFromRecvQueue();
        }

    }
}