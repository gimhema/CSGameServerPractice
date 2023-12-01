using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Message;

namespace MessageHandler
{
    public class GameMessageHandler
    {

        public Queue<byte[]> sendQueue = new Queue<byte[]>();
        public Queue<byte[]> recvQueue = new Queue<byte[]>();

        public GameMessageHandler() 
        { 

        }

        public int GetSendQueueCapacity() { return sendQueue.Count; }

        public int GetRecvQueueCapacity() { return recvQueue.Count; }

        public void PushMessageToSendQueue(byte[] message)
        {
            sendQueue.Enqueue(message);
        }

        public byte[] DequeueMessageFromSendQueue()
        { 
            return sendQueue.Dequeue();
        }

        public void PushMessageToRecvQueue(byte[] message)
        {
            recvQueue.Enqueue(message);
        }

        public byte[] DeququeMessageFromRecvQueue()
        {
            return recvQueue.Dequeue();
        }
    }
}
