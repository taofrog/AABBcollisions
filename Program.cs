using System;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using SFML;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace AABBcollisions
{
    static class Program
    {
        static void drawcollider(Collider body, RenderWindow screen, Color colour)
        {
            RectangleShape r = new RectangleShape(new Vector2f(body.size.x, body.size.y));
            r.Origin = new Vector2f(body.half_size.x, body.half_size.y);
            r.Position = new Vector2f(body.position.x, body.position.y);

            r.FillColor = colour;
            screen.Draw(r);
        }
        static void collisions(ref Rigidbody[] bodies, ref Collider[] colliders, ref Plane[] planes)
        {
            for (int i = 0; i < bodies.Length; i++)
            {
                bodies[i].edges = [false, false, false, false];
            }
            for (int k = 0; k < planes.Length; k++)
            {
                bodies[0].resolveplanecollision(ref planes[k]);
            }
            for (int k = 0; k < colliders.Length; k++)
            {
                bodies[0].resolverectcollision(ref colliders[k]);
            }
            for (int i = 0; i < bodies.Length - 1; i++)
            {
                for (int k = 0; k < planes.Length; k++)
                {
                    bodies[i + 1].resolveplanecollision(ref planes[k]);
                }
                for (int k = 0; k < colliders.Length; k++)
                {
                    bodies[i + 1].resolverectcollision(ref colliders[k]);
                }
                for (int j = i + 1; j < bodies.Length; j++)
                {
                    bodies[i].resolverbcollision(ref bodies[j]);
                }
            }
        }

        static void OnClose(object sender, EventArgs e)
        {
            // Close the window when OnClose event is received
            RenderWindow window = (RenderWindow)sender;
            window.Close();
        }

        static void OnKeyPress(object sender, EventArgs e)
        {

        }

        static void Main()
        {
            Clock clock;
            // Create the main window
            RenderWindow app = new RenderWindow(new VideoMode(800, 800), "Game");
            app.Closed += new EventHandler(OnClose);

            Color windowColor = new Color(0, 192, 255);

            app.SetVerticalSyncEnabled(true);

            Rigidbody[] bodies = { new Rigidbody(new vec2(-10, 130), new vec2(50, 50)),
                                   new Rigidbody(new vec2(700, 230), new vec2(50, 50))
                                 };

            Collider[] rects = {new Rigidbody(new vec2(425, 400), new vec2(50, 75)),
                                new Rigidbody(new vec2(400, 450), new vec2(50, 75)),
                                new Rigidbody(new vec2(375, 500), new vec2(50, 75))};

            Plane[] walls = { new Plane(new vec2(1, 0), 10), new Plane(new vec2(-1, 0), 800),
                              new Plane(new vec2(0, 1), 40), new Plane(new vec2(0, -1), 700)};

            // Start the game loop
            while (app.IsOpen)
            {
                // Process events
                app.DispatchEvents();

                if (Keyboard.IsKeyPressed(Keyboard.Key.W))
                {
                    bodies[0].accelerate(new vec2(0, -0.1));
                }
                if (Keyboard.IsKeyPressed(Keyboard.Key.S))
                {
                    bodies[0].accelerate(new vec2(0, 0.1));
                }
                if (Keyboard.IsKeyPressed(Keyboard.Key.A))
                {
                    bodies[0].accelerate(new vec2(-0.1, 0));
                }
                if (Keyboard.IsKeyPressed(Keyboard.Key.D))
                {
                    bodies[0].accelerate(new vec2(0.1, 0));
                }

                foreach (Rigidbody body in bodies)
                {
                    body.update();
                }
                collisions(ref bodies, ref rects, ref walls);

                // Clear screen
                app.Clear(windowColor);

                //draw things here
                foreach (Rigidbody body in bodies)
                {
                    drawcollider(body, app, new Color(255, 255, 255));
                }

                foreach (Rigidbody body in rects)
                {
                    drawcollider(body, app, new Color(50, 50, 150));
                }

                foreach (Rigidbody body in bodies)
                {
                    Console.WriteLine(body.edges[1]);
                    for (int i = 0; i < 4; i++)
                    {
                        bool b = body.edges[i];
                        if (b)
                        {
                            RectangleShape r = new RectangleShape(new Vector2f(body.size.x, body.size.y));
                            r.Origin = new Vector2f(body.half_size.x, body.half_size.y);
                            r.Position = new Vector2f(body.position.x, body.position.y);

                            Color colour = new Color(255, 0, 0);

                            r.FillColor = colour;

                            if (i == 0)
                            {
                                r.Size = new Vector2f(r.Size.X, 2);
                                r.Position = new Vector2f(r.Position.X, r.Position.Y);
                            }
                            else if (i == 1)
                            {
                                r.Size = new Vector2f(2, r.Size.Y);
                                r.Position = new Vector2f(r.Position.X + body.size.x, r.Position.Y);
                            }
                            else if (i == 2)
                            {
                                r.Size = new Vector2f(r.Size.X, 2);
                                r.Position = new Vector2f(r.Position.X, r.Position.Y + body.size.y);
                            }
                            else if (i == 3)
                            {
                                r.Size = new Vector2f(2, r.Size.Y);
                                r.Position = new Vector2f(r.Position.X, r.Position.Y);
                            }

                            app.Draw(r);
                        }
                    }
                }

                // Update the window
                app.Display();
            }
        }
    }
}