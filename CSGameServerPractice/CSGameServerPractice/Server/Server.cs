using System;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using MessageHandler;
using Message;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using ConnetionHandle;

namespace CSGameServerPractice
{
    class Server
    {
        private static Socket serverSocket;
        private static ConnectionHandler clientHandler = new ConnectionHandler();

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
                // clientList.Add(args.AcceptSocket);
                clientHandler.AddNewConnetion(args.AcceptSocket);
                
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

                RegisterRecv(recvArgs, SocketError.Success);
                RegisterAccept(args);

            }
            else
            {
                Console.WriteLine(args.SocketError.ToString());
            }
        }

        private void RegisterRecv(SocketAsyncEventArgs eventArgs, SocketError netState)
        {
            if(netState == SocketError.NetworkReset) { return; }
            Socket client = eventArgs.UserToken as Socket;
            bool pending = client.ReceiveAsync(eventArgs);


            if (pending == false)
            {
                if (eventArgs.BytesTransferred > 0 && eventArgs.SocketError == SocketError.Success)
                {
                    string recvData = System.Text.Encoding.UTF8.GetString(eventArgs.Buffer,
                        eventArgs.Offset, eventArgs.BytesTransferred);

                    Console.WriteLine("RegisterRecv RECEV : " + recvData);

                    byte[] sendArray = Encoding.UTF8.GetBytes(recvData);
                    WriteMessageToRecvQueue(sendArray);
                    RegisterSend();

                }

                OnRecvCompleted(null, eventArgs);
            }
        }

        private void OnRecvCompleted(object sender, SocketAsyncEventArgs eventArgs)
        {
            RegisterRecv(eventArgs, SocketError.ConnectionReset);
        }

        private bool SendAsyncToClient(int id, SocketAsyncEventArgs args)
        {
            return clientHandler.GetConnectionByID(id).SendAsync(args);
        }

        private void RegisterSend()
        {
            byte[] buff = FetchMessageFromRecvQueue(); // 일단 지금은 에코라..

            int numConnections = clientHandler.GetConnectionCount();

            for (int i = 0; i < numConnections; i++)
            {
                SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
                sendArgs.SetBuffer(buff, 0, buff.Length);
                sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendCompleted);

                bool pending = SendAsyncToClient(i, sendArgs);

                if (pending == false)
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
            }
        }

        private void OnApplicationQuit()
        {
            int numConnection = clientHandler.GetConnectionCount();
            for (int i = 0; i < numConnection; i++)
            {
                clientHandler.DisConnectByID(i);

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
            return gameMessageHandler.DeququeMessageFromRecvQueue();
        }

        public byte[] ConvertMessageToByte(GameMessage gameMessage)
        {
            byte[] serialized;

            using (MemoryStream ms = new MemoryStream())
            {
                BinaryFormatter bf = new BinaryFormatter();
                bf.Serialize(ms, gameMessage);
                serialized = ms.ToArray();
            }

            return serialized;
        }
        
        public GameMessage ConvertByteToMessage(byte[] message)
        {
            GameMessage gameMessage = new GameMessage();

            using (MemoryStream ms = new MemoryStream(message))
            {
                BinaryFormatter bf = new BinaryFormatter();
                gameMessage = (GameMessage)bf.Deserialize(ms);
            }

            return gameMessage;
        }
    }
}