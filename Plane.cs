using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AABBcollisions
{
    class Plane
    {
        // facing, must be axis aligned
        private vec2 face;
        public vec2 facing
        {
            get { return face;  }
            set { face = value; }
        }

        // distance from origin
        private float dis;
        public float distance
        {
            get { return dis;  }
            set { dis = value; }
        }

        // facing must be either (1, 0), (-1, 0), (0, 1) or (0, -1)
        //distance is always along the active axis
        public Plane(vec2 facing, float distance)
        {
            dis = distance;
            if (facing.x == 0 ^ facing.y == 0){ // ^ = xor
                face = facing;
            }
            else
            {
                throw new ApplicationException("Plane vector must be axis aligned");
            }
        }
    }
}
