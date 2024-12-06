using System;
using System.Runtime.Intrinsics.X86;
using SFML;
using SFML.Graphics;
using SFML.System;
using SFML.Window;

namespace AABBcollisions
{
    static class Program
    {
        static void collisions(ref Rigidbody[] bodies, ref Plane[] planes)
        {
            for (int k = 0; k < planes.Length; k++)
            {
                bodies[0].resolveplanecollision(ref planes[k]);
            }
            for (int i = 0; i < bodies.Length - 1; i++)
            {
                for (int k = 0; k < planes.Length; k++)
                {
                    bodies[i + 1].resolveplanecollision(ref planes[k]);
                }

                for(int j = i + 1; j < bodies.Length; j++)
                {
                    bodies[i].resolverectcollision(ref bodies[j]);
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

            Rigidbody[] bodies = { new Rigidbody(new vec2(-10, 130), new vec2(50, 50), _vel:new vec2(0, 0), _locked:new vec2(0, 1)),
                                   new Rigidbody(new vec2(425, 400), new vec2(50, 75), _locked:new vec2(1, 1)),
                                   new Rigidbody(new vec2(400, 450), new vec2(50, 75), _locked:new vec2(1, 1)),
                                   new Rigidbody(new vec2(375, 500), new vec2(50, 75), _locked:new vec2(1, 1))
                                 };

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
                    if (body.islocked.iszero())
                    {
                        Console.WriteLine(body.velocity);
                    }
                }
                collisions(ref bodies, ref walls);

                // Clear screen
                app.Clear(windowColor);

                //draw things here
                for (int i = 0; i < bodies.Length; i++)
                {
                    RectangleShape r = new RectangleShape(new Vector2f(bodies[i].size.x, bodies[i].size.y));
                    r.Origin = new Vector2f(bodies[i].half_size.x, bodies[i].half_size.y);
                    r.Position = new Vector2f(bodies[i].position.x, bodies[i].position.y);

                    Color colour = new Color(255, 255, 255);
                 
                    r.FillColor = colour;
                    app.Draw(r);
                }

                // Update the window
                app.Display();
            }
        }
    }
}