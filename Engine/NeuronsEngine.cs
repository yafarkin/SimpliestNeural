using Newtonsoft.Json;

namespace SimpliestNeural;

public class NeuronsEngine
{
    public readonly IActivation Activation;
    public readonly double LearnRate;

    public int NeuronCounter { get; protected set; }

    protected Dictionary<int, Neuron> Neurons = new();

    protected List<Neuron>? _outputNeurons;
    public IList<Neuron> OutputNeurons
    {
        get { return _outputNeurons ??= Neurons.Select(_ => _.Value).Where(_ => _.IsOutputNeuron).ToList(); }
    }

    protected List<Neuron>? _inputNeurons;
    public IList<Neuron> InputNeurons
    {
        get { return _inputNeurons ??= Neurons.Select(_ => _.Value).Where(_ => _.IsInputNeuron).ToList(); }
    }

    public double[] Outputs => OutputNeurons.Select(_ => _.Value).ToArray();

    protected const int _minNeuronsParallel = 100;
    protected List<List<Neuron>> _calcedLayers = new();
    protected ParallelOptions _parallelOptions;

    public NeuronsEngine(IActivation activation, double learnRate)
    {
        Activation = activation;
        LearnRate = learnRate;

        _parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = Math.Max(2, Convert.ToInt32(Environment.ProcessorCount * 0.75))
        };
    }

    protected void SetInputs(params double[] values)
    {
        if (values.Length != InputNeurons.Count)
        {
            throw new InvalidOperationException("Invalid values count");
        }

        for (var i = 0; i < values.Length; i++)
        {
            InputNeurons[i].Value = values[i];
        }
    }

    protected IList<Tuple<Neuron, double>> SetTargets(params double[] targets)
    {
        if (targets.Length != OutputNeurons.Count)
        {
            throw new InvalidOperationException("Invalid targets count");
        }

        var result = new List<Tuple<Neuron, double>>();
        for (var i = 0; i < targets.Length; i++)
        {
            result.Add(new Tuple<Neuron, double>(OutputNeurons[i], targets[i]));
        }

        return result;
    }

    protected void ResetSavedState()
    {
        _inputNeurons = null;
        _outputNeurons = null;
        _calcedLayers.Clear();
    }

    public void CreateAllToAllLayers(bool randomize, params int[] counts)
    {
        var random = new Random();
        var prevLayer = new List<Neuron>();
        var currentLayer = new List<Neuron>();
        foreach (var count in counts)
        {
            currentLayer.Clear();

            for (var i = 0; i < count; i++)
            {
                var inputNeurons = new Dictionary<Neuron, double>(prevLayer.Count);
                foreach (var prevNeuron in prevLayer)
                {
                    var weight = randomize ? random.NextDouble() * 2.0 - 1.0 : 1;
                    inputNeurons.Add(prevNeuron, weight);
                }

                var neuron = CreateNeuron(inputNeurons);
                currentLayer.Add(neuron);
            }

            prevLayer.Clear();
            prevLayer.AddRange(currentLayer);
        }
    }

    public Neuron CreateNeuron(IDictionary<Neuron, double> inputNeurons)
    {
        return CreateNeuron(0, inputNeurons);
    }

    public Neuron CreateInputNeuron()
    {
        return CreateNeuron(0, null);
    }

    protected Neuron CreateNeuron(double value, IDictionary<Neuron, double>? inputNeurons)
    {
        NeuronCounter++;
        var result = new Neuron(Activation, NeuronCounter, value, 0);
        Neurons.Add(result.Id, result);
        ResetSavedState();

        if (inputNeurons != null && inputNeurons.Count > 0)
        {
            foreach (var kv in inputNeurons)
            {
                result.InputNeurons.Add(kv.Key, kv.Value);
            }

            foreach (var inputNeuron in inputNeurons)
            {
                inputNeuron.Key.OutputNeurons.Add(result);
            }
        }

        return result;
    }

    public void LoadState(string json)
    {
        NeuronCounter = 0;
        Neurons.Clear();

        var data = JsonConvert.DeserializeObject<List<NeuronSlim>>(json);
        var dict = data!.ToDictionary(_ => _.Id);

        foreach (var kv in dict)
        {
            CreateNeuron(dict, kv.Key);
        }

        ResetSavedState();
    }

    protected Neuron CreateNeuron(IDictionary<int, NeuronSlim> dict, int id)
    {
        if (Neurons.TryGetValue(id, out var neuron))
        {
            return neuron;
        }

        if (id > NeuronCounter)
        {
            NeuronCounter = id;
        }

        var neuronSlim = dict[id];
        neuron = new Neuron(Activation, neuronSlim);
        Neurons.Add(neuron.Id, neuron);

        foreach (var input in neuronSlim.Inputs)
        {
            if (!Neurons.TryGetValue(input.Item1, out var inputNeuron))
            {
                inputNeuron = CreateNeuron(dict, input.Item1);
            }

            neuron.InputNeurons.Add(inputNeuron, input.Item2);
            inputNeuron.OutputNeurons.Add(neuron);
        }

        return neuron;
    }

    public string SaveState()
    {
        var x = InputNeurons;
        var y = OutputNeurons;

        var dict = new Dictionary<int, NeuronSlim>();

        foreach (var neuron in OutputNeurons)
        {
            CreateSlimNeuron(dict, neuron);
        }

        var data = dict.Select(_ => _.Value);
        var json = JsonConvert.SerializeObject(data, Formatting.Indented);
        return json;
    }

    protected void CreateSlimNeuron(IDictionary<int, NeuronSlim> slimNeurons, Neuron neuron)
    {
        var neuronSlim = new NeuronSlim(neuron);
        slimNeurons.Add(neuron.Id, neuronSlim);

        foreach (var inputNeuron in neuron.InputNeurons)
        {
            if (!slimNeurons.ContainsKey(inputNeuron.Key.Id))
            {
                CreateSlimNeuron(slimNeurons, inputNeuron.Key);
            }
        }
    }

    public void BackPropagation(params double[] targets)
    {
        if (0 == _calcedLayers.Count)
        {
            throw new InvalidOperationException($"Before run {nameof(BackPropagation)} you should run {nameof(FeedForward)}");
        }

        var expectedResults = SetTargets(targets);
        foreach (var tuple in expectedResults)
        {
            var outputNeuron = tuple.Item1;
            var targetValue = tuple.Item2;

            outputNeuron.Error = targetValue - outputNeuron.Value;
        }

        for (var i = _calcedLayers.Count - 1; i >= 0; i--)
        {
            var layer = _calcedLayers[i];
            if (layer.Count < _minNeuronsParallel)
            {
                foreach (var neuron in _calcedLayers[i])
                {
                    neuron.Learn(LearnRate);
                }
            }
            else
            {
                Parallel.ForEach(_calcedLayers[i], _parallelOptions, _ => _.Learn(LearnRate));
            }
        }

        if (InputNeurons.Count < _minNeuronsParallel)
        {
            foreach (var neuron in InputNeurons)
            {
                neuron.Learn(LearnRate);
            }
        }
        else
        {
            Parallel.ForEach(InputNeurons, _parallelOptions, _ => _.Learn(LearnRate));
        }
    }

    public double[] FeedForward(params double[] inputs)
    {
        if (0 == Neurons.Count)
        {
            throw new InvalidOperationException("No neurons configured");
        }

        var isEmpty = 0 == _calcedLayers.Count;

        SetInputs(inputs);

        if (!isEmpty)
        {
            foreach (var layer in _calcedLayers)
            {
                if (layer.Count < _minNeuronsParallel)
                {
                    foreach (var neuron in layer)
                    {
                        neuron.Process(false);
                    }
                }
                else
                {
                    Parallel.ForEach(layer, _parallelOptions, neuron =>
                    {
                        neuron.Process(false);
                    });
                }
            }
        }
        else
        {
            foreach (var neuron in Neurons)
            {
                neuron.Value.ResetProcess();
            }

            var currentNeurons = InputNeurons.SelectMany(_ => _.OutputNeurons).Distinct().ToList();
            var nextNeurons = new Dictionary<int, Neuron>();

            while (currentNeurons.Count > 0)
            {
                foreach (var neuron in currentNeurons)
                {
                    neuron.Process(true);
                    foreach (var outputNeuron in neuron.OutputNeurons)
                    {
                        nextNeurons.TryAdd(outputNeuron.Id, outputNeuron);
                    }
                }

                if (isEmpty)
                {
                    var layer = currentNeurons.Where(_ => _.State == NeuronStateType.Ready);
                    _calcedLayers.Add(new List<Neuron>(layer));
                }

                currentNeurons = nextNeurons.Select(_ => _.Value).ToList();
                nextNeurons.Clear();
            }
        }

        if (Neurons.Any(_ => _.Value.State == NeuronStateType.Waiting))
        {
            _calcedLayers.Clear();
            throw new InvalidOperationException("Circular link");
        }

        return Outputs;
    }
}