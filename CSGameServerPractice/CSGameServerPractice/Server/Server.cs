using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;
using System.Threading;

namespace CSGameServerPractice.Server
{
    internal class Server
    {
        static Dictionary<Socket, byte[]> clients = new Dictionary<Socket, byte[]>();

        static void Main(string[] args)
        {
            // 서버 소켓 생성 및 바인딩
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Any, 12345);
            Socket serverSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            serverSocket.Bind(endPoint);
            serverSocket.Listen(10);

            Console.WriteLine("Start Server . . . Wait for Client . . .");

            // 클라이언트 접속 대기 스레드 시작
            Thread listenThread = new Thread(ListenForClients);
            listenThread.Start(serverSocket);

            Console.ReadLine();
        }

        static void ListenForClients(object serverSocket)
        {
            Socket listener = (Socket)serverSocket;

            while (true)
            {
                // 클라이언트 연결 대기
                Socket clientSocket = listener.Accept();
                Console.WriteLine("Connect Client . . . " + clientSocket.RemoteEndPoint);

                // 클라이언트 관리
                clients.Add(clientSocket, new byte[1024]);
                Thread clientThread = new Thread(HandleClient);
                clientThread.Start(clientSocket);
            }
        }

        static void HandleClient(object clientSocket)
        {
            Socket client = (Socket)clientSocket;
            byte[] buffer = clients[client];
            while (true)
            {
                try
                {
                    // 클라이언트로부터 메시지 수신
                    int bytesRead = client.Receive(buffer);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine("Client (" + client.RemoteEndPoint + "): " + message);

                        // 모든 클라이언트에게 메시지 전송
                        byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
                        foreach (var otherClient in clients.Keys)
                        {
                            if (otherClient != client)
                            {
                                otherClient.Send(messageBuffer);
                            }
                        }
                    }
                }
                catch
                {
                    // 클라이언트 연결 끊김
                    Console.WriteLine("Disconnect from (" + client.RemoteEndPoint + ").");
                    clients.Remove(client);
                    break;
                }
            }
        }
    }
}
