using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLogic;

namespace CSGameServerPractice
{
    public class Player
    {
        private int uid;
        private string name;
        private Location playerLocation;
        
        public Player(int uid, string name)
        {
            this.uid = uid;
            this.name = name;
        }

    }
}
