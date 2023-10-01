using System.Diagnostics;
using SimpliestNeural;

namespace Dots;

internal static class Program
{
    const int Width = 80;
    const int Height = 60;

    private static NeuronsEngine _nn;
    //private static NeuralNetwork _nn;

    private static readonly Random _random = new();
    private static readonly byte[] _rgba = new byte[Width * Height * 4];
    private static Stopwatch _sw;
    private static int _cnt;
    private static bool _backPropagation = true;

    static void Main()
    {
        var activation = new SigmoidActivation();
        _nn = new NeuronsEngine(activation, 0.01);
        _nn.CreateAllToAllLayers(true, 2, 5, 5, 2);

        Console.WriteLine("Press ESC to stop. F2 - clear points; F3 - start/stop back propogation");
        using var dots = new DotsWindow(Width, Height);
        _sw = Stopwatch.StartNew();
        dots.Run(CycleFunc);
    }

    static (bool, byte[]) CycleFunc(List<PointWithType> points)
    {
        _cnt++;

        if (_sw.ElapsedMilliseconds > 1000)
        {
            var fps = (float)_cnt / _sw.ElapsedMilliseconds * 1000;
            _cnt = 0;
            _sw.Restart();

            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(fps.ToString("N"));
        }

        if (_backPropagation)
        {
            LearnNeural(points);
            CalcNeural();
        }

        if (Console.KeyAvailable)
        {
            var key = Console.ReadKey().Key;
            if (key == ConsoleKey.Escape)
            {
                return (false, _rgba);
            }

            if (key == ConsoleKey.F2)
            {
                points.Clear();
            }

            if (key == ConsoleKey.F3)
            {
                _backPropagation = !_backPropagation;
            }
        }

        return (true, _rgba);
    }

    static void CalcNeural()
    {
        for (var h = 0; h < Height; h++)
        {
            for (var w = 0; w < Width; w++)
            {
                var nx = (double)w / Width - 0.5;
                var ny = (double)h / Height - 0.5;

                var outputs = _nn.FeedForward(new[] { nx, ny });

                var green = Math.Max(0, Math.Min(1, outputs[0] - outputs[1] + 0.5));
                var blue = 1 - green;

                green = 0.3 + green * 0.5;
                blue = 0.5 + blue * 0.5;

                var color = Color.FromArgb(100, Convert.ToByte(green * 255), Convert.ToByte(blue * 255));
                var shift = h * Width * 4 + w * 4;

                _rgba[shift] = color.R;
                _rgba[shift + 1] = color.G;
                _rgba[shift + 2] = color.B;
                _rgba[shift + 3] = color.A;
            }
        }
    }

    private static void LearnNeural(List<PointWithType> points)
    {
        if (0 == points.Count)
        {
            return;
        }

        for (var k = 0; k < 10_000; k++)
        {
            var p = points[_random.Next(points.Count - 1)];
            var nx = p.Left - 0.5;
            var ny = p.Top - 0.5;

            _nn.FeedForward(new[] { nx, ny });

            var targets = new[]
            {
                p.Type ? 1.0 : 0.0,
                p.Type ? 0.0 : 1.0
            };

            _nn.BackPropagation(targets);
        }
    }
}