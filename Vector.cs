using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AABBcollisions
{
    struct vec2
    {
        // defining x and y variables
        public float x;
        public float y;

        // different constructors to turn any type of number input into floats
        public vec2()
        {
            x = 0;
            y = 0;
        }
        public vec2(float _x, float _y)
        {
            x = _x;
            y = _y;
        }
        public vec2(double _x, double _y)
        {
            x = (float)_x;
            y = (float)_y;
        }
        public vec2(int _x, int _y)
        {
            x = (float)_x;
            y = (float)_y;
        }

        // squared length of the vector - much faster to compute than actual length
        public float lengthsqr()
        {
            return x * x + y * y; // x^2 + y^2 = h^2
        }

        // total length of the vector
        public float length()
        {
            return (float)Math.Sqrt(lengthsqr()); 
        }

        // returns true if the total length of the vector is zero
        public bool iszero()
        {
            if (x == 0 && y == 0)
            {
                return true;
            }
            return false;
        }

        // returns the vector rotated by an angle, in radians
        public vec2 rotate(float angle)
        {
            return new vec2(Math.Cos(angle) * x - Math.Sin(angle) * y,  // x2 = x1 cos(0) - y1 sin(0)
                            Math.Sin(angle) * x + Math.Cos(angle) * y); // y2 = x1 sin(0) + y1 cos(0)
        }
        public vec2 rotate(double angle)
        {
            return new vec2(Math.Cos(angle) * x - Math.Sin(angle) * y,
                            Math.Sin(angle) * x + Math.Cos(angle) * y);
        }

        // returns the vector rotated by an angle, in degrees
        public vec2 rotatedeg(float angle)
        {
            angle = angle * 180 / (float)Math.PI;  // converted to radians
            return rotate(angle);
        }
        public vec2 rotatedeg(double angle)
        {
            angle = angle * 180 / Math.PI;
            return rotate(angle);
        }

        // returns the vector with only positive values
        public vec2 abs()
        {
            return new vec2(Math.Abs(x), Math.Abs(y));
        }

        // keeps the direction but returns a vector with length 1 
        public vec2 normalise()
        {
            float len = length();
            return new vec2(x / len, y / len);
        }

        
        public static vec2 operator +(vec2 a, vec2 b) => new vec2(a.x + b.x, a.y + b.y);//  v1 + v2 = [x1 + x2,  y1 + y2]
        public static vec2 operator -(vec2 a, vec2 b) => new vec2(a.x - b.x, a.y - b.y);//  v1 - v2 = [x1 - x2,  y1 - y2]
        public static vec2 operator *(vec2 a, vec2 b) => new vec2(a.x * b.x, a.y * b.y);//  v1 * v2 = [x1 * x2,  y1 * y2]
        public static vec2 operator *(vec2 a, int b) => new vec2(a.x * b, a.y * b);     //  v1 * a  = [x1 * a,   y1 * a ]
        public static vec2 operator *(vec2 a, float b) => new vec2(a.x * b, a.y * b);   //  v1 * a  = [x1 * a,   y1 * a ]
        public static vec2 operator *(int b, vec2 a) => new vec2(a.x * b, a.y * b);     //  v1 * a  = [x1 * a,   y1 * a ]
        public static vec2 operator *(float b, vec2 a) => new vec2(a.x * b, a.y * b);   //  v1 * a  = [x1 * a,   y1 * a ]
        public static vec2 operator /(vec2 a, vec2 b) => new vec2(a.x / b.x, a.y / b.y);//  v1 / v2 = [x1 / x2,  y1 / y2]
        public static vec2 operator /(vec2 a, int b) => new vec2(a.x / b, a.y / b);     //  v1 / a  = [x1 / a,   y1 / a ]
        public static vec2 operator /(vec2 a, float b) => new vec2(a.x / b, a.y / b);   //  v1 / a  = [x1 / a,   y1 / a ]
        public static vec2 operator /(int b, vec2 a) => new vec2(b / a.x, b / a.y);     //  v1 * a  = [x1 * a,   y1 * a ]
        public static vec2 operator /(float b, vec2 a) => new vec2(b / a.x, b / a.y);   //  v1 * a  = [x1 * a,   y1 * a ]

        // tostring override so printing returns the values in the form "(x, y) "
        public override string ToString() => $"({x}, {y}) ";
    }
}
