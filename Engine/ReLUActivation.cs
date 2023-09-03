namespace SimpliestNeural;

public class ReLUActivation : IActivation
{
    public double Activation(double value)
    {
        return Math.Max(0, value);
    }

    public double ActivationDx(double value)
    {
        return value > 0 ? 1 : 0;
    }
}