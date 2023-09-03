using CsvHelper;
using IrisLearn;
using SimpliestNeural;
using System.Diagnostics;
using System.Globalization;

const string dataSetFile = "iris_dataset.csv";
const string neuronNetworkFile = "iris_network.json";

const int epochs = 1000;
const int batchSize = 100;

using var reader = new StreamReader(dataSetFile);
using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
var items = csv.GetRecords<IrisDto>().ToList();

Shuffle(items);

var activation = new SigmoidActivation();
//var nn1 = new SampleWithArrays.NeuralNetwork(true, 0.01, activation.Activation, activation.ActivationDx, 4, 10, 3);
var nn = new NeuronsEngine(activation, 0.01);
nn.CreateAllToAllLayers(true, 4, 10, 3);

var trainSet = items.Take(Convert.ToInt32(items.Count * 0.7)).ToList();
var testSet = items.Skip(Convert.ToInt32(items.Count * 0.7)).ToList();

if (File.Exists(neuronNetworkFile))
{
    Console.WriteLine($"Neuron network file '{neuronNetworkFile}' exists, no learning will be proceed");
    var json = File.ReadAllText(neuronNetworkFile);
    nn.LoadState(json);
    testSet = items;
}
else
{
    var sw = Stopwatch.StartNew();
    var r = new Random();
    var targets = new double[3];
    for (var epoch = 0; epoch < epochs; epoch++)
    {
        var right = 0;
        var errorSum = 0.0;

        for (var batch = 0; batch < batchSize; batch++)
        {
            var iris = trainSet[r.Next(trainSet.Count - 1)];
            var inputs = new[] { iris.PetalLength, iris.PetalWidth, iris.SepalLength, iris.SepalWidth };
            var outputs = nn.FeedForward(inputs);

            Array.Clear(targets);
            targets[iris.Value - 1] = 1;

            var outputIdx = 0;
            var outputMaxValue = double.MinValue;
            for (var idx = 0; idx < outputs.Length; idx++)
            {
                var outputValue = outputs[idx];
                errorSum += (targets[idx] - outputValue) * (targets[idx] - outputValue);

                if (outputValue > outputMaxValue)
                {
                    outputMaxValue = outputValue;
                    outputIdx = idx;
                }
            }

            if (outputIdx == iris.Value - 1)
            {
                right++;
            }

            nn.BackPropagation(targets);
        }

        var str = $"epoch: {epoch}; correct: {right}; error: {errorSum}; elapsed: {sw.Elapsed.TotalSeconds};\n";
        Console.Write(str);
        sw.Restart();
    }

    var json = nn.SaveState();
    File.WriteAllText(neuronNetworkFile, json);
}

var errors = 0;
foreach (var iris in testSet)
{
    var inputs = new[] { iris.PetalLength, iris.PetalWidth, iris.SepalLength, iris.SepalWidth };
    var outputs = nn.FeedForward(inputs);

    var max = FindMax(outputs);

    if (iris.Value - 1 != max.Item1)
    {
        errors++;
    }
}

var total = testSet.Count;
var valid = total - errors;
var percent = (double)valid / total;
Console.WriteLine($"Total: {total}; Valid = {valid}; Errors = {errors}; Percent = {percent:P}");

static (int, double) FindMax(double[] values)
{
    var outputIdx = 0;
    var outputMaxValue = double.MinValue;
    for (var i = 0; i < values.Length; i++)
    {
        var outputValue = values[i];

        if (outputValue > outputMaxValue)
        {
            outputMaxValue = outputValue;
            outputIdx = i;
        }
    }

    return (outputIdx, outputMaxValue);
}

static void Shuffle<T>(IList<T> list)
{
    var rnd = new Random();
    var n = list.Count;
    while (n > 1)
    {
        n--;
        var k = rnd.Next(n + 1);
        (list[k], list[n]) = (list[n], list[k]);
    }
}