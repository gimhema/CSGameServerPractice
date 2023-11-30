using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message;

namespace CSGameServerPractice
{
    public class GameMessageHandler
    {

        private static Queue<byte[]> sendQueue = new Queue<byte[]>();
        private static Queue<byte[]> recvQueue = new Queue<byte[]>();

        public GameMessageHandler() 
        { 

        }

        void WriteMessageToSendQueue(byte[] message)
        {
            sendQueue.Enqueue(message);
        }

        byte[] FetchMessageFromSendQueue()
        { 
            return sendQueue.Dequeue();
        }

        void RecvMessageFromClient(byte[] message)
        {
            recvQueue.Enqueue(message);
        }

        byte[] FetchMessageFromRecvQueue()
        {
            return recvQueue.Dequeue();
        }
    }
}
