﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace AABBcollisions
{
    internal class Collider
    {
        protected vec2 pos;
        protected vec2 halfsize; // halfsize is used instead of size because it is the distance from the origin in all calculations

        // what faces are colliding, from top clockwise (top, right, bottom, left)
        public bool[] edges = [false, false, false, false];

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
        public vec2 half_size
        {
            get { return halfsize; }
        }
        public vec2 size
        {
            get { return halfsize * 2; }
        }

        public Collider(float x, float y, float xsize, float ysize)
        {
            pos = new vec2(x, y);
            halfsize = new vec2(xsize / 2, ysize / 2);
        }
        public Collider(vec2 _pos, vec2 _size)
        {
            pos = _pos;
            halfsize = _size / 2;
        }

        // can be used with values that arent tied to a specific object
        public static vec2 findoverlap(vec2 p1, vec2 p2, vec2 s1, vec2 s2)
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
        public vec2 findoverlap(ref Collider other)
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

        protected int vectoedges(vec2 facing)
        {
            // vector (0, 1), (1, 0), (0, -1), (-1, 0)
            // converted to int 0, 1, 2, 3

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

        protected vec2 edgetovec(int edge)
        {
            // vector (0, 1), (1, 0), (0, -1), (-1, 0)
            // converted to int 0, 1, 2, 3

            vec2 v = new vec2();

            if (edge == 0)
            {
                v = new vec2(0, 1);
            }
            else if (edge == 1)
            {
                v = new vec2(1, 0);
            }
            else if (edge == 2)
            {
                v = new vec2(0, -1);
            }
            else if (edge == 3)
            {
                v = new vec2(-1, 0);
            }

            return v;
        }
    }
}
