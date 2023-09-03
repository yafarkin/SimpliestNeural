namespace SimpliestNeural;

public class SigmoidActivation : IActivation
{
    public double Activation(double value)
    {
        return 1.0 / (1.0 + Math.Pow(Math.E, -value));
    }

    public double ActivationDx(double value)
    {
        //var sigmoid = Activation(value);
        //return sigmoid / (1 - sigmoid);
        return value * (1 - value);
    }
}