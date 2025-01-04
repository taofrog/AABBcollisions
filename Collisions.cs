using AABBcollisions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace AABBcollisions
{
    internal class Collisions
    {
        public static void collisions(ref Collider[] colliders, ref Plane[] planes, float dt)
        {
            for (int i = 0; i < colliders.Length; i++)
            {
                colliders[i].edges = [false, false, false, false];
            }

            for (int i = 0; i < colliders.Length; i++)
            {
                Rigidbody? body = colliders[i] as Rigidbody;
                if (body != null)
                {
                    foreach (Plane p in planes)
                    {
                        Plane plane = p;
                        body.resolveplanecollision(ref plane);
                    }

                    for (int j = i + 1; j < colliders.Length; j++)
                    {
                        Rigidbody? other = colliders[j] as Rigidbody;
                        if (other != null)
                        {
                            body.resolverbcollision(ref other, dt);
                        }
                        else
                        {
                            body.resolverectcollision(ref colliders[j], dt);
                        }
                    }
                }
                else
                {
                    for (int j = i + 1; j < colliders.Length; j++)
                    {
                        Rigidbody? other = colliders[j] as Rigidbody;
                        if (other != null)
                        {
                            other.resolverectcollision(ref colliders[i], dt);
                        }
                    }
                }
            }
        }
    }
}
