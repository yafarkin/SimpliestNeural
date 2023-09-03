/*
 * All code here and above was copied and adapted to C# from this repository: https://github.com/ArtemOnigiri/SimpleNN
 */

using Newtonsoft.Json;

namespace SampleWithArrays;

public class NeuralNetwork
{
    public readonly double learningRate;
    public readonly Layer[] layers;

    private readonly Func<double, double> activation;
    private readonly Func<double, double> derivative;

    public string SaveState()
    {
        return JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    public static NeuralNetwork? LoadState(string json)
    {
        return JsonConvert.DeserializeObject<NeuralNetwork>(json);
    }

    public NeuralNetwork(bool randomize, double learningRate, Func<double, double> activation, Func<double, double> derivative, params int[] sizes)
    {
        this.learningRate = learningRate;
        this.activation = activation;
        this.derivative = derivative;

        layers = new Layer[sizes.Length];

        var random = new Random();

        for (var i = 0; i < sizes.Length; i++)
        {
            var nextSize = 0;
            if (i < sizes.Length - 1)
            {
                nextSize = sizes[i + 1];
            }

            layers[i] = new Layer(sizes[i], nextSize);
            for (var j = 0; j < sizes[i]; j++)
            {
                layers[i].biases[j] = randomize ? random.NextDouble() * 2.0 - 1.0 : 0;
                for (var k = 0; k < nextSize; k++)
                {
                    layers[i].weights[j, k] = randomize ? random.NextDouble() * 2.0 - 1.0 : 1;
                }
            }
        }
    }

    public double[] FeedForward(double[] inputs)
    {
        Array.Copy(inputs, layers[0].neurons, inputs.Length);
        for (var i = 1; i < layers.Length; i++)
        {
            var l = layers[i - 1];
            var l1 = layers[i];
            for (var j = 0; j < l1.size; j++)
            {
                l1.neurons[j] = 0;
                for (var k = 0; k < l.size; k++)
                {
                    l1.neurons[j] += l.neurons[k] * l.weights[k, j];
                }

                l1.neurons[j] += l1.biases[j];
                l1.neurons[j] = activation(l1.neurons[j]);
            }
        }

        return layers[layers.Length - 1].neurons;
    }

    public void BackPropagation(double[] targets)
    {
        var errors = new double[layers[layers.Length - 1].size];
        for (var i = 0; i < layers[layers.Length - 1].size; i++)
        {
            errors[i] = targets[i] - layers[layers.Length - 1].neurons[i];
        }

        for (var k = layers.Length - 2; k >= 0; k--)
        {
            var currentLayer = layers[k];
            var nextLayer = layers[k + 1];
            var errorsNext = new double[currentLayer.size];
            var gradients = new double[nextLayer.size];

            for (var i = 0; i < nextLayer.size; i++)
            {
                gradients[i] = errors[i] * derivative(nextLayer.neurons[i]);
                gradients[i] *= learningRate;
            }

            var deltas = new double[nextLayer.size, currentLayer.size];
            for (var i = 0; i < nextLayer.size; i++)
            {
                for (var j = 0; j < currentLayer.size; j++)
                {
                    deltas[i, j] = gradients[i] * currentLayer.neurons[j];
                }
            }

            for (var i = 0; i < currentLayer.size; i++)
            {
                errorsNext[i] = 0;
                for (var j = 0; j < nextLayer.size; j++)
                {
                    errorsNext[i] += currentLayer.weights[i, j] * errors[j];
                }
            }

            errors = new double[currentLayer.size];
            Array.Copy(errorsNext, errors, errorsNext.Length);

            var weightsNew = new double[currentLayer.weights.GetLength(0), currentLayer.weights.GetLength(1)];
            for (var i = 0; i < nextLayer.size; i++)
            {
                for (var j = 0; j < currentLayer.size; j++)
                {
                    weightsNew[j, i] = currentLayer.weights[j, i] + deltas[i, j];
                }
            }

            currentLayer.weights = weightsNew;
            for (var i = 0; i < nextLayer.size; i++)
            {
                nextLayer.biases[i] += gradients[i];
            }
        }
    }
}