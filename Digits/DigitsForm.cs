using SimpliestNeural;

namespace Digits;

public partial class DigitsForm : Form
{
    public double[,] Digits = new double[28, 28];

    public int DWidth => Digits.GetLength(0);
    public int DHeight => Digits.GetLength(1);

    private Bitmap _bufferedImage;
    private Graphics _bufferedGraphic;

    private NeuronsEngine _nn;
    //private NeuralNetwork _nn;

    public DigitsForm()
    {
        InitializeComponent();
    }

    private void DigitsForm_Load(object sender, EventArgs e)
    {
        var activation = new SigmoidActivation();
        _nn = new NeuronsEngine(activation, 0.01);
        _nn.LoadState(File.ReadAllText("mnist_network.json"));

        _bufferedImage = new Bitmap(pbImg.Width, pbImg.Height);
        _bufferedGraphic = Graphics.FromImage(_bufferedImage);
        pbImg.Image = _bufferedImage;

        MainCycle();
    }

    public void MainCycle()
    {
        DrawGrid();

        var w = DWidth;
        var h = DHeight;
        var inputs = new double[w * h];
        for (var i = 0; i < w; i++)
        {
            for (var j = 0; j < h; j++)
            {
                inputs[i * w + j] = Digits[j, i];
            }
        }
        var outputs = _nn.FeedForward(inputs);

        pb0.Value = Convert.ToInt32(outputs[0] * 100);
        pb1.Value = Convert.ToInt32(outputs[1] * 100);
        pb2.Value = Convert.ToInt32(outputs[2] * 100);
        pb3.Value = Convert.ToInt32(outputs[3] * 100);
        pb4.Value = Convert.ToInt32(outputs[4] * 100);
        pb5.Value = Convert.ToInt32(outputs[5] * 100);
        pb6.Value = Convert.ToInt32(outputs[6] * 100);
        pb7.Value = Convert.ToInt32(outputs[7] * 100);
        pb8.Value = Convert.ToInt32(outputs[8] * 100);
        pb9.Value = Convert.ToInt32(outputs[9] * 100);

        pbImg.Refresh();
    }

    public void DrawGrid()
    {
        var w = DWidth;
        var h = DHeight;

        var cw = _bufferedImage.Width / w;
        var ch = _bufferedImage.Height / h;

        for (var i = 0; i < w; i++)
        {
            for (var j = 0; j < h; j++)
            {
                var l = Convert.ToByte(Digits[i, j] * 255);

                var cellColor = Color.FromArgb(l, l, l);
                using var cellBrush = new SolidBrush(cellColor);

                _bufferedGraphic.FillRectangle(cellBrush, i * cw, j * ch, cw, ch);
            }
        }
    }

    private void pbImg_DoubleClick(object sender, EventArgs e)
    {
        var w = DWidth;
        var h = DHeight;
        var inputs = new double[w * h];
        for (var i = 0; i < w; i++)
        {
            for (var j = 0; j < h; j++)
            {
                Digits[i, j] = 0;
            }
        }

        MainCycle();
    }

    private void pbImg_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.None)
        {
            return;
        }

        var x = e.X;
        var y = e.Y;

        var cx = Convert.ToInt32((double)x / pbImg.Width * DWidth);
        var cy = Convert.ToInt32((double)y / pbImg.Height * DHeight);

        var c = Digits[cx, cy];

        if (e.Button == MouseButtons.Left)
        {
            if (0 == c)
            {
                c = 0.44;
            }

            c += 0.1;
        }
        else
        {
            if (c >= 0.999)
            {
                c = 0.56;
            }
            c -= 0.1;
        }

        if (c < 0)
        {
            c = 0;
        }

        if (c > 1)
        {
            c = 1;
        }

        Digits[cx, cy] = c;

        MainCycle();
    }
}