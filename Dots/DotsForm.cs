using SimpliestNeural;
using System.Diagnostics;
using SampleWithArrays;

namespace Dots;

public partial class DotsForm : Form
{
    private const double ImageScale = 0.1;
    private const int PointSize = 30;
    private const int InnerPointShift = 3;

    private ulong _drawCount;
    private Stopwatch _stopwatch;

    private readonly List<PointWithType> _points = new();

    private Brush _mainBrush;
    private Pen _penType1, _penType2;

    private Bitmap _neuralBitmap;
    private Bitmap _bufferedImage;
    private Graphics _bufferedGraphic;

    private NeuronsEngine _nn;
    //private NeuralNetwork _nn;

    private readonly Random _random = new();

    public DotsForm()
    {
        InitializeComponent();
    }

    private void DotsForm_Load(object sender, EventArgs e)
    {
        var activation = new SigmoidActivation();
        _nn = new NeuronsEngine(activation, 0.01);
        _nn.CreateAllToAllLayers(true, 2, 5, 5, 2);
        //_nn = new NeuralNetwork(true, 0.01, activation.Activation, activation.ActivationDx, 2, 5, 5, 2);

        _neuralBitmap = new Bitmap(Convert.ToInt32(pbImg.Width * ImageScale), Convert.ToInt32(pbImg.Height * ImageScale));

        _bufferedImage = new Bitmap(pbImg.Width, pbImg.Height);
        _bufferedGraphic = Graphics.FromImage(_bufferedImage);

        _mainBrush = new SolidBrush(Color.White);
        _penType1 = new Pen(Color.Green, 5);
        _penType2 = new Pen(Color.Blue, 5);

        pbImg.Image = _bufferedImage;
    }

    public void MainCycle()
    {
        LearnNeural();

        DrawNeural();
        _bufferedGraphic.DrawImage(_neuralBitmap, 0, 0, _bufferedImage.Width, _bufferedImage.Height);

        DrawPoints();

        pbImg.Refresh();

        Interlocked.Increment(ref _drawCount);
    }

    private void LearnNeural()
    {
        if (0 == _points.Count)
        {
            return;
        }

        for (var k = 0; k < 10_000; k++)
        {
            var p = _points[_random.Next(_points.Count - 1)];
            var nx = (double)p.X / pbImg.Width - 0.5;
            var ny = (double)p.Y / pbImg.Height - 0.5;
            
            _nn.FeedForward(new[] { nx, ny });

            var targets = new[]
            {
                p.Type == 1 ? 1.0 : 0.0,
                p.Type == 1 ? 0.0 : 1.0
            };

            _nn.BackPropagation(targets);
        }
    }

    public void DrawNeural()
    {
        var bitmap = _neuralBitmap;
        var width = bitmap.Width;
        var height = bitmap.Height;
        for (var x = 0; x < width; x++)
        {
            for (var y = 0; y < height; y++)
            {
                var nx = (double)x / width - 0.5;
                var ny = (double)y / height - 0.5;

                var outputs = _nn.FeedForward(new[] { nx, ny });

                var green = Math.Max(0, Math.Min(1, outputs[0] - outputs[1] + 0.5));
                var blue = 1 - green;

                green = 0.3 + green * 0.5;
                blue = 0.5 + blue * 0.5;

                bitmap.SetPixel(x, y, Color.FromArgb(100, Convert.ToByte(green * 255), Convert.ToByte(blue * 255)));
            }
        }
    }

    public void DrawPoints()
    {
        var points = new List<PointWithType>(_points);
        foreach (var point in points)
        {
            _bufferedGraphic.FillEllipse(_mainBrush, point.X, point.Y, PointSize, PointSize);
            _bufferedGraphic.DrawEllipse(point.Type == 1 ? _penType1 : _penType2, point.X + InnerPointShift,
                point.Y + InnerPointShift, PointSize - InnerPointShift * 2, PointSize - InnerPointShift * 2);
        }
    }

    private void btnStartStop_Click(object sender, EventArgs e)
    {
        if (!tDraw.Enabled)
        {
            _points.Clear();
            _stopwatch = Stopwatch.StartNew();
            tCount.Enabled = true;
            tDraw.Enabled = true;
        }
        else
        {
            btnStartStop.Text = "Start";
            tCount.Enabled = false;
            tDraw.Enabled = false;
        }

        DotsForm_Load(sender, e);
    }

    private void tCount_Tick(object sender, EventArgs e)
    {
        var elapsed = _stopwatch.Elapsed.TotalMilliseconds;

        var count = Interlocked.Read(ref _drawCount);
        var fps = count / elapsed * 1000;

        Interlocked.Exchange(ref _drawCount, 0);

        btnStartStop.Text = fps.ToString("F1");

        _stopwatch.Restart();
    }

    private void DotsForm_SizeChanged(object sender, EventArgs e)
    {
        if (tDraw.Enabled)
        {
            btnStartStop_Click(sender, e);
        }
    }

    private void pbImg_Click(object sender, EventArgs e)
    {
        if (e is not MouseEventArgs me)
        {
            return;
        }

        var pointType = (me.Button & MouseButtons.Left) != 0 ? 1 : 2;

        _points.Add(new PointWithType(me.X - PointSize / 2, me.Y - PointSize / 2, pointType));
    }

    private void tDraw_Tick(object sender, EventArgs e)
    {
        MainCycle();
    }
}