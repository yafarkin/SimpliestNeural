namespace SimpliestNeural;

public class Neuron
{
    public int Id { get; internal set; }

    public double Value { get; set; }

    public double Bias { get; internal set; }

    public bool IsInputNeuron => 0 == InputNeurons.Count;

    public bool IsOutputNeuron => 0 == OutputNeurons.Count;

    public NeuronStateType State { get; protected set; }

    public Dictionary<Neuron, double> InputNeurons { get; set; } = new();
    public List<Neuron> OutputNeurons { get; protected set; } = new();

    internal double Error { get; set; }
    internal double Gradient { get; set; }

    protected IActivation Activation;

    public void ResetProcess()
    {
        State = IsInputNeuron ? NeuronStateType.Ready : NeuronStateType.NotReady;
    }

    public void Process(bool check)
    {
        if (check)
        {
            if (State != NeuronStateType.NotReady && State != NeuronStateType.Waiting)
            {
                return;
            }

            if (InputNeurons.Any(_ => _.Key.State != NeuronStateType.Ready))
            {
                State = NeuronStateType.Waiting;
                return;
            }
        }

        State = NeuronStateType.Calculating;

        Value = 0;
        double value = 0;
        foreach (var inputNeuron in InputNeurons)
        {
            value += inputNeuron.Key.Value * inputNeuron.Value;
        }

        value += Bias;

        Value = ActivationFunction(value);

        State = NeuronStateType.Ready;
    }

    internal Neuron(IActivation activation, int id, double value, double bias)
    {
        Activation = activation;

        Id = id;
        Value = value;
        Bias = bias;
    }

    internal Neuron(IActivation activation, NeuronSlim neuronSlim)
    {
        Activation = activation;
        Id = neuronSlim.Id;
        Value = neuronSlim.Value;
        Bias = neuronSlim.Bias;
        State = neuronSlim.State;
    }

    public virtual double ActivationFunction(double value)
    {
        return Activation.Activation(value);
    }

    public virtual double ActivationDxFunction(double value)
    {
        return Activation.ActivationDx(value);
    }

    public void Learn(double learnRate)
    {
        if (State != NeuronStateType.Ready)
        {
            throw new InvalidOperationException("Обучение возможно только после выполнения расчёта");
        }

        if (!IsOutputNeuron)
        {
            Error = 0;

            foreach (var outputNeuron in OutputNeurons)
            {
                if (outputNeuron.State != NeuronStateType.Learned)
                {
                    throw new InvalidOperationException("Не произведено обучение");
                }

                var weight = outputNeuron.InputNeurons[this];
                Error += weight * outputNeuron.Error;

                var delta = outputNeuron.Gradient * Value;
                var newWeight = weight + delta;
                outputNeuron.InputNeurons[this] = newWeight;
            }
        }

        if (IsInputNeuron)
        {
            Error = 0;
            Gradient = 0;
        }
        else
        {
            Gradient = Error * ActivationDxFunction(Value) * learnRate;
            Bias += Gradient;
        }

        if (!IsInputNeuron)
        {
            State = NeuronStateType.Learned;
        }
    }

    public override string ToString()
    {
        return $"{Id}; V = {Value} / {State}; B = {Bias}";
    }
}