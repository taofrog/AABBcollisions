using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AABBcollisions
{
    class Rigidbody
    {
        private vec2 pos;
        private vec2 vel;
        private vec2 halfsize; // halfsize is used instead of size because it is the distance from the origin in all calculations
        private vec2 locked;   // locked is which axis the object can move on

        // getters and setters
        public vec2 position
        {
            get { return pos; }
            set { pos = value; }
        }
        public float xpos
        {
            get { return pos.x; }
            set { pos.x = value; }
        }
        public float ypos
        {
            get { return pos.y; }
            set { pos.y = value; }
        }
        public vec2 velocity
        {
            get { return vel; }
            set { vel = value; }
        }
        public vec2 half_size
        {
            get { return halfsize; }
        }
        public vec2 size
        {
            get { return halfsize * 2; }
        }
        public vec2 islocked
        {
            get { return locked; }
            set { locked = value; }
        }

        public Rigidbody(float x, float y, float xsize, float ysize, float xvel=0, float yvel=0, int xlocked=0, int ylocked=0)
        {
            pos = new vec2(x, y);
            vel = new vec2(xvel, yvel);
            halfsize = new vec2(xsize / 2, ysize / 2);
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
        public Rigidbody(vec2 _pos, vec2 _size, vec2? _vel = null, vec2? _locked=null)
        {
            pos = _pos;
            vel = _vel ?? new vec2();
            halfsize = _size / 2;
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

        // the same as the other findoverlap except it can be used with values that arent tied to a specific object
        public vec2 findoverlap(vec2 p1, vec2 p2, vec2 s1, vec2 s2)
        {
            vec2 overlap = new vec2(0, 0);

            vec2 dif = p2 - p1;
            if (!dif.iszero())
            {
                vec2 size = s1 + s2;

                if (Math.Abs(dif.x) < size.x)
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
            // only run plane calculations if the 
            if (locked.x == 1 && plane.facing.x != 0)
            {
                return;
            }
            if (locked.y == 1 && plane.facing.y != 0)
            {
                return;
            }
            vec2 overlap = planeoverlap(ref plane);

            if (!overlap.iszero())
            {
                if (!vel.iszero())
                {
                    vec2 scalefac = overlap / vel.abs();

                    if (!scalefac.iszero() && scalefac.x < 1 && scalefac.y < 1)
                    {
                        vec2 norm = scalefac.normalise();
                        vec2 swapped = new vec2(norm.y, norm.x);

                        pos -= vel * scalefac;
                        vel *= swapped;

                        return;
                    }
                }
                vec2 axis = plane.facing.abs();
                vec2 swap = new vec2(axis.y, axis.x);

                pos += overlap * plane.facing;
                vel *= swap;
            }
        }

        // moves objects outside eachother, and sets velocity on the collision axis to 0
        public void resolverectcollision(ref Rigidbody other)
        {
            vec2 nextoverlap = findoverlap(pos + vel, other.position + other.velocity, halfsize, other.halfsize);

            if (nextoverlap.x != 0 && nextoverlap.y != 0) {
                vec2 overlap = findoverlap(ref other);

                if (overlap.x != 0 && overlap.y != 0)
                {
                    if (!(vel - other.vel).iszero())
                    {
                        vec2 scalefac = overlap / (other.velocity - vel).abs();

                        if (scalefac.x < 1 || scalefac.y < 1)
                        {
                            if (scalefac.x < scalefac.y)
                            {
                                scalefac.y = 0;
                            }
                            else
                            {
                                scalefac.x = 0;
                            }
                            vec2 norm = scalefac.normalise();
                            vec2 swapped = new vec2(norm.y, norm.x);

                            pos -= vel * scalefac;
                            other.position -= other.velocity * scalefac;

                            vel *= swapped;
                            other.velocity *= swapped;

                            return;
                        }

                    }
                    vec2 tomove1 = new vec2();
                    vec2 tomove2 = new vec2();
                    tomove1 = (new vec2(1, 1) - locked) * overlap * (new vec2(0.5, 0.5) + 0.5f * other.islocked);
                    tomove2 = (new vec2(1, 1) - other.islocked) * overlap * (new vec2(0.5, 0.5) + 0.5f * locked);

                    if (overlap.x < overlap.y)
                    {
                        if (pos.x < other.position.x)
                        {
                            pos.x -= tomove1.x;
                            other.xpos += tomove2.x;
                        }
                        else
                        {
                            pos.x += tomove1.x;
                            other.xpos -= tomove2.x;
                        }
                        vel.x = 0;
                        other.velocity *= new vec2(0, 1);
                    }
                    else
                    {
                        if (pos.y < other.position.y)
                        {
                            pos.y -= tomove1.y;
                            other.ypos += tomove2.y;
                        }
                        else
                        {
                            pos.y += tomove1.y;
                            other.ypos -= tomove2.y;
                        }
                        vel.y = 0;
                        other.velocity *= new vec2(1, 0);
                    }
                }
            }
        }

        public void accelerate(vec2 dir)
        {
            dir *= new vec2(1, 1) - locked;
            vel += dir;
        }

        public void update()
        {
            vel *= new vec2(1, 1) - locked;
            pos += vel;
        }
    }
}