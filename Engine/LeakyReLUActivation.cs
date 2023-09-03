namespace SimpliestNeural;

public class LeakyReLUActivation : IActivation
{
    const double leaky_relu_alpha = 0.01;
    public double Activation(double value)
    {
        return Math.Max(leaky_relu_alpha * value, value);
    }

    public double ActivationDx(double value)
    {
        return value > 0 ? 1 : leaky_relu_alpha;
    }
}