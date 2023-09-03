namespace SimpliestNeural;

public interface IActivation
{
    double Activation(double value);
    double ActivationDx(double value);
}