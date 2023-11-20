using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLogic
{
    public class Vector
    {
        public int x;
        public int y;
        public int z;

        public Vector(int x, int y, int z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    // 쿼터니언은 다루지않음
    public class Rotation
    {
        public int roll;
        public int pitch;
        public int yaw;

        public Rotation(int roll, int pitch, int yaw)
        {
            this.roll = roll;
            this.pitch = pitch;
            this.yaw = yaw;
        }
    }

    public class Location
    {
        public Vector position;
        public Rotation rotation;

        public Location(Vector position, Rotation rotation)
        {
            this.position = position;
            this.rotation = rotation;
        }

        public Vector GetLocation()
        {
            return this.position;
        }

        public Rotation GetRotation()
        {
            return this.rotation;
        }
    }

}
