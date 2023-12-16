using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Message
{
    public enum MessageType
    {
        DEFAULT = 0
    }

    [Serializable]
    public class GameMessage
    {
        public const int paramSize = 32;
        public int pID; // 송신자의 식별자
        public int moduleID; // 처리해야할 모듈의 식별자
        public int fid; // 모듈이 기능을 요청받은 메소드의 식별자
        public float[] param = new float[paramSize]; // 함수의 파라미터

        public GameMessage() 
        { 
            pID = 0;
            moduleID = 0;
            fid = 0;
        }

        public GameMessage(int pID, int moduleID, int fid, string paramStr)
        {
            this.pID = pID;
            this.moduleID = moduleID;
            this.fid = fid;
            
            // paramStr 슬라이싱 후 param을 세팅
            string[] _temp = paramStr.Split(',');
            for (int i = 0; i < paramSize; i++)
            {
                this.param[i] = float.Parse(_temp[i]);
            }
            
        }
    }
}
