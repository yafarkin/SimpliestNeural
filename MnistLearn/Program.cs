using MnistLearn;
using SimpliestNeural;
using System.Diagnostics;
using System.IO.Compression;

const string dataSetFile = "mnist_train.csv.zip";
const string neuronNetworkFile = "mnist_network.json";
const int epochs = 1000;
const int batchSize = 100;

Console.WriteLine("Preparing");

var activation = new SigmoidActivation();

var nn = new NeuronsEngine(activation, 0.001);
nn.CreateAllToAllLayers(true, 784, 512, 128, 32, 10);
//var nn = new SampleWithArrays.NeuralNetwork(true, 0.001, activation.Activation, activation.ActivationDx, 784, 512, 128, 32, 10);

var items = ReadArchive(dataSetFile);
if (!items.Any())
{
    return;
}

Shuffle(items);

var trainSet = items.Take(Convert.ToInt32(items.Count * 0.7)).ToList();
var testSet = items.Skip(Convert.ToInt32(items.Count * 0.7)).ToList();

var sw = Stopwatch.StartNew();
if (File.Exists(neuronNetworkFile))
{
    Console.WriteLine($"Neuron network file '{neuronNetworkFile}' exists, no learning will be proceed");
    var json = File.ReadAllText(neuronNetworkFile);
    nn.LoadState(json);
    testSet = items;
}
else
{
    File.Delete("log.txt");
    Console.WriteLine("Learning");
    var r = new Random();
    var targets = new double[10];
    for (var i = 0; i < epochs; i++)
    {
        var right = 0;
        var errorSum = 0.0;
        for (var j = 0; j < batchSize; j++)
        {
            var digit = trainSet[r.Next(items.Count - 1)];

            Array.Clear(targets);
            targets[digit.Value] = 1;

            var outputs = nn.FeedForward(digit.Inputs);

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

            if (outputIdx == digit.Value)
            {
                right++;
            }

            nn.BackPropagation(targets);
        }

        var str = $"epoch: {i}; correct: {right}; error: {errorSum}; elapsed: {sw.Elapsed.TotalSeconds};\n";
        Console.Write(str);

        File.AppendAllText("log.txt", str);

        if (i % 100 == 0)
        {
            File.WriteAllText(neuronNetworkFile, nn.SaveState());
        }

        sw.Restart();
    }

#if !LEARN
    File.WriteAllText(neuronNetworkFile, nn.SaveState());
#endif
}

sw = Stopwatch.StartNew();
var errors = 0;
for(var i = 0; i < testSet.Count; i++)
{
    var digit = testSet[i];
    var outputs = nn.FeedForward(digit.Inputs);

    var max = FindMax(outputs);

    if (digit.Value != max.Item1)
    {
        errors++;
    }

    if (sw.Elapsed.TotalSeconds > 5)
    {
        sw.Restart();
        Console.WriteLine(((double)i/testSet.Count).ToString("P"));
    }
}

var total = testSet.Count;
var valid = total - errors;
var percent = (double)valid / total;
Console.WriteLine($"Total: {total}; Valid = {valid}; Errors = {errors}; Percent = {percent:P}");

List<DigitDto> ReadArchive(string filename)
{
    var result = new List<DigitDto>();

    if (!File.Exists(dataSetFile))
    {
        Console.WriteLine($"Can't find '{dataSetFile}'");
        return result;
    }

    using var archive = ZipFile.OpenRead(dataSetFile);
    foreach (var archiveEntry in archive.Entries)
    {
        using var stream = archiveEntry.Open();
        using TextReader reader = new StreamReader(stream);
        while (true)
        {
            var line = reader.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
            {
                break;
            }

            var numberValues = line.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var dto = new DigitDto
            {
                Value = Convert.ToByte(numberValues[0]),
                Inputs = numberValues.Skip(1).Select(_ => Convert.ToDouble(_) / 255.0).ToArray()
            };
            result.Add(dto);
        }
    }

    return result;
}

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