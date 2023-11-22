using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLogic
{
    public class Vector
    {
        public float x;
        public float y;
        public float z;

        public Vector(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
    }

    // 쿼터니언은 다루지않음
    public class Rotation
    {
        public float roll;
        public float pitch;
        public float yaw;

        public Rotation(float roll, float pitch, float yaw)
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

        public void Init()
        {
            this.position.x = 0;
            this.position.y = 0;
            this.position.z = 0;

            this.rotation.roll = 0;
            this.rotation.pitch = 0;
            this.rotation.yaw = 0;
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
