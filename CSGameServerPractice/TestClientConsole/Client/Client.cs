using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace TestClientConsole.Client
{
    internal class Client
    {
        static void Main(string[] args)
        {
            Console.Write("Write Your Name: ");
            string nickname = Console.ReadLine();

            TcpClient client = new TcpClient();
            client.Connect(IPAddress.Loopback, 12345);

            Console.WriteLine("Connect Server.");

            // 서버로부터 메시지를 수신하는 스레드 시작
            Thread receiveThread = new Thread(() => ReceiveMessages(client));
            receiveThread.Start();

            while (true)
            {
                string message = Console.ReadLine();
                SendMessage(client, nickname + ": " + message);
            }
        }

        static void ReceiveMessages(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            byte[] buffer = new byte[1024];

            while (true)
            {
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                Console.WriteLine(message);
            }
        }

        static void SendMessage(TcpClient client, string message)
        {
            NetworkStream stream = client.GetStream();
            byte[] messageBuffer = Encoding.UTF8.GetBytes(message);
            stream.Write(messageBuffer, 0, messageBuffer.Length);
        }
    }
}
