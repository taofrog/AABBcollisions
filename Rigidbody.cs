using SFML.Graphics.Glsl;
using SFML.Window;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static SFML.Window.Joystick;

namespace AABBcollisions
{
    class Rigidbody : Collider
    {
        const float invsqrt2 = 1 / 1.41421356237f;

        public bool debug = false;

        protected vec2 vel;
        private vec2 locked;   // locked is which axis the object can move on

        private const float error = 0.0f;

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

        // finds the distance that the objects are overlapping. This is always positive
        protected vec2 findoverlap(ref Rigidbody other)
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
        protected vec2 planeoverlap(ref Plane plane)
        {
            vec2 overlap = new();

            vec2 axis = plane.facing.abs();         // which axis to check collisions because i dont want to use if statements

            vec2 dif = -plane.facing * (new vec2(plane.distance) - pos); // diference in only the correct axis

            overlap = (halfsize - dif) * axis;  // take it away from the size

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
        public void resolverbcollision(ref Rigidbody other, float dt)
        {
            // only resolve collisions if the next frame is also overlapping. this fixes a bug where a rigidbody gets stuck on a corner
            vec2 nextoverlap = findoverlap(pos + vel * dt, other.position + other.velocity * dt, halfsize, other.halfsize);

            if (nextoverlap.x != 0 && nextoverlap.y != 0) {
                vec2 overlap = findoverlap(ref other);  // how much the objects are overlapping on each axis

                
                float e1 = Math.Clamp(vel.length() * dt, 0, halfsize.length() * invsqrt2);
                float e2 = Math.Clamp(other.velocity.length() * dt, 0, other.halfsize.length() * invsqrt2);
                if ((overlap.x != 0 && overlap.y != 0) && (overlap.x >= e1 || overlap.y >= e1) && (overlap.x >= e2 || overlap.y >= e2))
                {
                    // how much of a frame have they been overlapping in each axis
                    vec2 scalefac = overlap / (dt * (vel - other.velocity).abs()); // overlap / relative velocity

                    vec2 tomove1 = new();
                    vec2 tomove2 = new();

                    vec2 dir = new();
                    vec2 velmultiplier = new();

                    if (scalefac.x <= 1 || scalefac.y <= 1)
                    {
                        dir = (vel - other.velocity).normaliseaxis();

                        if (scalefac.x < scalefac.y)
                        {
                            dir.y = 0;
                            velmultiplier.y = 1;
                        }
                        else
                        {
                            dir.x = 0;
                            velmultiplier.x = 1;
                        }

                        scalefac *= -dir;
                        if (Double.IsNaN(scalefac.x)) { scalefac.x = 0; }
                        if (Double.IsNaN(scalefac.y)) { scalefac.y = 0; }

                        tomove1 = -vel * scalefac * dt;
                        tomove2 = -other.velocity * scalefac * dt;
                    }
                    else
                    {
                        // funny algorithm to find how much to move each object
                        // considering that if an object is locked it cannot move and the other must move the full distance
                        // this was written at 3 am if anything breaks imma blame this algorithm
                        tomove1 = (new vec2(1, 1) - locked) * overlap * (new vec2(0.5, 0.5) + 0.5f * other.islocked);
                        tomove2 = (new vec2(1, 1) - other.islocked) * overlap * (new vec2(0.5, 0.5) + 0.5f * locked);

                        dir = (other.position - pos).normaliseaxis();

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
                    }

                    pos += tomove1;
                    other.position += tomove2;

                    vel *= velmultiplier;
                    other.velocity *= velmultiplier;

                    if (!tomove1.iszero() || !tomove2.iszero())
                    {
                        edges[vectoedges(-dir)] = true;
                        other.edges[vectoedges(dir)] = true;
                    }
                }
            }
        }

        public void resolverectcollision(ref Collider other, float dt)
        {
            // only resolve collisions if the next frame is also overlapping. this fixes a bug where a rigidbody gets stuck on a corner
            vec2 nextoverlap = findoverlap(pos + vel * dt, other.position, halfsize, other.half_size);

            if (nextoverlap.x != 0 && nextoverlap.y != 0)
            {
                vec2 overlap = findoverlap(ref other);  // how much the objects are overlapping on each axis

                float e = Math.Clamp(vel.length() * dt, 0, halfsize.length() * invsqrt2);

                if ((overlap.x != 0 && overlap.y != 0) && (overlap.x >= e || overlap.y >= e))
                {
                    // how much of a frame have they been overlapping in each axis
                    vec2 scalefac = overlap / vel.abs(); // overlap / velocity

                    vec2 tomove = new();

                    vec2 dir = new();
                    vec2 velmultiplier = new();

                    if (scalefac.x <= 1 || scalefac.y <= 1)
                    {
                        dir = vel.normaliseaxis();

                        if (scalefac.x < scalefac.y)
                        {
                            dir.y = 0;
                            velmultiplier.y = 1;
                        }
                        else
                        {
                            dir.x = 0;
                            velmultiplier.x = 1;
                        }

                        scalefac *= dir.abs();
                        if (Double.IsNaN(scalefac.x)) { scalefac.x = 0; }
                        if (Double.IsNaN(scalefac.y)) { scalefac.y = 0; }

                        tomove = -vel * scalefac;
                    }
                    else
                    {
                        // funny algorithm to find how much to move each object
                        // considering that if an object is locked it cannot move and the other must move the full distance
                        // this was written at 3 am if anything breaks imma blame this algorithm
                        tomove = (new vec2(1, 1) - locked) * overlap;

                        dir = (other.position - pos).normaliseaxis();

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
                    }

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
        public void accelerate(vec2 dir, float dt)
        {
            dir *= new vec2(1, 1) - locked;
            vel += dir;
        }

        // move object by velocity, accounting for locked axes
        public void update(vec2 gravity, float dt)
        {
            vel -= gravity * dt;

            vel *= new vec2(1, 1) - locked;
            pos += vel * dt;
        }
    }
}