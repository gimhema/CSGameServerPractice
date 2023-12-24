using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace ConnetionHandle
{
    public class ConnectionHandler
    {
        private static List<Socket> connections = new List<Socket>();
        public ConnectionHandler() { }  

        public void AddNewConnetion(Socket connetion)
        {
            connections.Add(connetion);
        }

        public int GetConnectionCount() { return connections.Count;}

        public Socket GetConnectionByID (int id) { return connections[id];}

        public void DisConnectByID(int id)
        {
            connections[id].Shutdown(SocketShutdown.Both);
            connections[id].Close();
        }

        public void RemoveConnectionByID(int id)
        {
            if(connections.Count == 0)
                return;
            
            if (id < 0)
                return;

            connections[id].Dispose();
            connections.RemoveAt(id);
        }

        public void ConnectionsReset()
        {
            foreach (Socket connetion in connections)
            {
                connetion.Shutdown(SocketShutdown.Both);
                connetion.Close();
            }
            connections.Clear(); 
        }

    }
}
