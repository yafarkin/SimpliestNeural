namespace SimpliestNeural;

public record NeuronSlim
{
    public int Id { get; init; }

    public double Value { get; init; }

    public double Bias { get; init; }

    public Tuple<int, double>[] Inputs { get; init; } = Array.Empty<Tuple<int, double>>();

    public NeuronStateType State { get; init; }

    public NeuronSlim()
    {
    }

    public NeuronSlim(Neuron neuron)
    {
        Id = neuron.Id;
        Value = neuron.Value;
        Bias = neuron.Bias;
        State = neuron.State;
        Inputs = neuron.InputNeurons.Select(_ => new Tuple<int, double>(_.Key.Id, _.Value)).ToArray();
    }
}