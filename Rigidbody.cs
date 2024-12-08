using SFML.Window;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace AABBcollisions
{
    class Rigidbody : Collider
    {
        protected vec2 vel;
        private vec2 locked;   // locked is which axis the object can move on

        public vec2 islocked
        {
            get { return locked; }
            set { locked = value; }
        }
        public vec2 velocity
        {
            get { return vel; }
            set { vel = value; }
        }

        public Rigidbody(float x, float y, float xsize, float ysize, float xvel=0, float yvel=0, int xlocked=0, int ylocked=0) : base(x, y, xsize, ysize)
        {
            vel = new vec2(xvel, yvel);
            locked = new vec2(xlocked, ylocked);

            if (locked.x != 0)
            {
                vel.x = 0;
            }
            if (locked.y != 0)
            {
                vel.y = 0;
            }
        }
        public Rigidbody(vec2 _pos, vec2 _size, vec2? _vel = null, vec2? _locked=null) : base(_pos, _size)
        {
            vel = _vel ?? new vec2();
            locked = _locked ?? new vec2();

            if (locked.x != 0)
            {
                vel.x = 0;
            }
            if (locked.y != 0)
            {
                vel.y = 0;
            }
        }

        private int vectoedges(vec2 facing)
        {
            // vector (0, 1), (1, 0), (0, -1), (-1, 0)
            // converted to int 1, 2, 3, 4

            if (facing.x == 0 ^ facing.y == 0) // ^ = xor
            {
                int i = 0;
                // find i here
                if (facing.y > 0)
                {
                    i = 0;
                }
                else if (facing.x < 0)
                {
                    i = 1;
                }
                else if (facing.y < 0)
                {
                    i = 2;
                }
                else if (facing.x > 0)
                {
                    i = 3;
                }

                return i;
            }
            else
            {
                throw new ApplicationException("facing vector must be axis aligned");
            }
        }

        // finds the distance that the objects are overlapping. This is always positive
        private vec2 findoverlap(ref Rigidbody other)
        {
            vec2 overlap = new vec2();

            vec2 dif = other.position - pos;             // current diference between the objects
            if (!dif.iszero())
            {
                vec2 size = halfsize + other.half_size;  // target distance

                if (Math.Abs(dif.x) < size.x)            // only return if they are actually overlapping
                {
                    overlap.x = size.x - Math.Abs(dif.x);
                }
                if (Math.Abs(dif.y) < size.y)
                {
                    overlap.y = size.y - Math.Abs(dif.y);
                }
            }
            return overlap;
        }

        // finds overlap with a plane
        private vec2 planeoverlap(ref Plane plane)
        {
            vec2 overlap = new vec2();

            vec2 axis = plane.facing.abs();         // which axis to check collisions because i dont want to use if statements

            vec2 dif = axis * plane.distance - pos; // diference in only the correct axis
            overlap = halfsize * axis - dif.abs();  // take it away from the size

            overlap.x = Math.Max(0, overlap.x);
            overlap.y = Math.Max(0, overlap.y);     // only return a value if it is actually touching

            return overlap;
        }

        // send object outside a plane
        public void resolveplanecollision(ref Plane plane)
        {
            // only run plane calculations if the axis of the plane is not locked
            if (locked.x == 1 && plane.facing.x != 0)
            {
                return;
            }
            if (locked.y == 1 && plane.facing.y != 0)
            {
                return;
            }

            vec2 overlap = planeoverlap(ref plane); // finds the overlap between plane and 

            if (!overlap.iszero())
            {
                //move to the outside and set vel to 0
                vec2 axis = plane.facing.abs();
                vec2 swap = new vec2(axis.y, axis.x);

                pos += overlap * plane.facing;
                vel *= swap;

                edges[vectoedges(plane.facing)] = true;
            }
        }

        // moves objects outside eachother, and sets velocity on the collision axis to 0
        public void resolverbcollision(ref Rigidbody other)
        {
            // only resolve collisions if the next frame is also overlapping. this fixes a bug where a rigidbody gets stuck on a corner
            vec2 nextoverlap = findoverlap(pos + vel, other.position + other.velocity, halfsize, other.halfsize);

            if (nextoverlap.x != 0 && nextoverlap.y != 0) {
                vec2 overlap = findoverlap(ref other);  // how much the objects are overlapping on each axis

                if (overlap.x != 0 && overlap.y != 0)
                {
                    vec2 tomove1;
                    vec2 tomove2;

                    vec2 velmultiplier = new vec2();

                    // funny algorithm to find how much to move each object
                    // considering that if an object is locked it cannot move and the other must move the full distance
                    // this was written at 3 am if anything breaks imma blame this algorithm
                    tomove1 = (new vec2(1, 1) - locked) * overlap * (new vec2(0.5, 0.5) + 0.5f * other.islocked);
                    tomove2 = (new vec2(1, 1) - other.islocked) * overlap * (new vec2(0.5, 0.5) + 0.5f * locked);

                    vec2 dir = (other.position - pos).normaliseaxis();

                    if (overlap.x < overlap.y)
                    {
                        dir.y = 0;
                        velmultiplier.y = 1;
                    }
                    else
                    {
                        dir.x = 0;
                        velmultiplier.x = 1;
                    }

                    tomove1 *= -dir;
                    tomove2 *= dir;

                    pos += tomove1;
                    other.position += tomove2;

                    vel *= velmultiplier;
                    other.velocity *= velmultiplier;

                    if (!tomove1.iszero())
                    {
                        edges[vectoedges(-dir)] = true;
                        other.edges[vectoedges(dir)] = true;
                    }
                }
            }
        }

        public void resolverectcollision(ref Collider other)
        {
            // only resolve collisions if the next frame is also overlapping. this fixes a bug where a rigidbody gets stuck on a corner
            vec2 nextoverlap = findoverlap(pos + vel, other.position, halfsize, other.half_size);

            if (nextoverlap.x != 0 && nextoverlap.y != 0)
            {
                vec2 overlap = findoverlap(ref other);  // how much the objects are overlapping on each axis

                if (overlap.x != 0 && overlap.y != 0)
                {
                    vec2 tomove;

                    vec2 velmultiplier = new vec2();

                    tomove = (new vec2(1, 1) - locked) * overlap;

                    vec2 dir = (other.position - pos).normaliseaxis();

                    if (overlap.x < overlap.y)
                    {
                        dir.y = 0;
                        velmultiplier.y = 1;
                    }
                    else
                    {
                        dir.x = 0;
                        velmultiplier.x = 1;
                    }

                    tomove *= -dir;

                    pos += tomove;

                    vel *= velmultiplier;

                    if (!tomove.iszero())
                    {
                        edges[vectoedges(-dir)] = true;
                    }
                }
            }
        }

        // add a value to velocity, accounting for locked axes
        public void accelerate(vec2 dir)
        {
            dir *= new vec2(1, 1) - locked;
            vel += dir;
        }

        // move object by velocity, accounting for locked axes
        public void update()
        {
            vel *= new vec2(1, 1) - locked;
            pos += vel;
        }
    }
}