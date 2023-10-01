using SFML.Graphics;
using SFML.System;
using SFML.Window;
using Color = SFML.Graphics.Color;

namespace Dots;

public class DotsWindow : IDisposable
{
    protected RenderWindow Window;
    protected Texture Texture;
    protected Sprite Sprite;

    public List<PointWithType>  Points = new();

    private const int HalfPointSize = 15;
    private const int InnerHalfPointSize = 12;

    public DotsWindow(uint width, uint height)
    {
        var videoMode = new VideoMode(1000, 1000);
        Window = new RenderWindow(videoMode, "Dots neural", Styles.Close);
        Window.SetVerticalSyncEnabled(true);
        Window.SetActive(true);

        Texture = new Texture(width, height) { Smooth = true };
        Sprite = new Sprite(Texture)
        {
            Scale = new Vector2f((float)Window.Size.X / width, (float)Window.Size.Y / height)
        };

        Window.Closed += (sender, e) =>
        {
            if (sender is RenderWindow w)
            {
                w.Close();
            }
        };
        Window.MouseButtonPressed += (sender, e) =>
        {
            var left = e.X - HalfPointSize;
            var top = e.Y - HalfPointSize;

            var pleft = (double)left / Window.Size.X;
            var ptop = (double)top / Window.Size.Y;

            var point = new PointWithType(pleft, ptop, e.Button == Mouse.Button.Left);
            Points.Add(point);
        };
        Window.KeyPressed += (sender, e) =>
        {
            if (e.Code == Keyboard.Key.Escape)
            {
                Window.Close();
            }
        };
    }

    public void Run(Func<List<PointWithType>, (bool, byte[])> cycleFunc)
    {
        while (Window.IsOpen)
        {
            var result = cycleFunc(Points);
            if (!result.Item1)
            {
                break;
            }

            Texture.Update(result.Item2);

            Window.DispatchEvents();
            Window.Clear(Color.Magenta);
            Window.Draw(Sprite);

            foreach (var point in Points)
            {
                var left = Convert.ToInt32(point.Left * Window.Size.X);
                var top = Convert.ToInt32(point.Top * Window.Size.Y);

                var cs = new CircleShape(HalfPointSize);
                cs.FillColor = Color.White;
                cs.Position = new Vector2f(left, top);
                Window.Draw(cs);

                var diff = HalfPointSize - InnerHalfPointSize;
                var innerCs = new CircleShape(InnerHalfPointSize);
                innerCs.FillColor = point.Type ? Color.Green : Color.Blue;
                innerCs.Position = new Vector2f(left + diff, top + diff);
                Window.Draw(innerCs);
            }

            Window.Display();
        }
    }

    public void Dispose()
    {
        Window.Dispose();
    }
}