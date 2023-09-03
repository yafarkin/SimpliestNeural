using Microsoft.VisualStudio.TestTools.UnitTesting;
using SampleWithArrays;
using SimpliestNeural;

namespace Tests;

[TestClass]
public class Tests
{
    public NeuronsEngine Engine;
    public IActivation Activation;

    public Tests()
    {
        Activation = new LeakyReLUActivation();
        Engine = new NeuronsEngine(Activation, 0.01);
    }

    [TestMethod]
    public void Smoke_Test()
    {
        // A -> B -> C
        var neuronA = Engine.CreateInputNeuron();
        var neuronB = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronA, 1.5 } });
        var neuronC = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronB, 2 } });

        Assert.AreEqual(NeuronStateType.NotReady, neuronA.State);
        Assert.AreEqual(NeuronStateType.NotReady, neuronB.State);
        Assert.AreEqual(NeuronStateType.NotReady, neuronC.State);
        Assert.IsTrue(neuronA.Id > 0);
        Assert.IsTrue(neuronB.Id > neuronA.Id);
        Assert.IsTrue(neuronC.Id > neuronB.Id);

        Engine.FeedForward(10);

        var outputNeuron = Engine.OutputNeurons.Single();
        Assert.IsTrue(outputNeuron.IsOutputNeuron);
        Assert.AreSame(outputNeuron, neuronC);
        Assert.AreEqual(NeuronStateType.Ready, outputNeuron.State);
        Assert.AreEqual(30, outputNeuron.Value);

        Assert.AreEqual(NeuronStateType.Ready, neuronA.State);
        Assert.AreEqual(NeuronStateType.Ready, neuronB.State);
        Assert.AreEqual(NeuronStateType.Ready, neuronC.State);

        var json = Engine.SaveState();
        Engine.LoadState(json);
        var json2 = Engine.SaveState();
        Assert.AreEqual(json, json2);

        Engine.FeedForward(1);
        Assert.AreEqual(30, outputNeuron.Value);
    }

    [TestMethod]
    public void SimpleNetwork_Test()
    {
        // A1+A2 -> B1
        // A1+A2 -> B2
        // B1+B2 -> C

        var neuronA1 = Engine.CreateInputNeuron();
        var neuronA2 = Engine.CreateInputNeuron();
        var neuronB1 = Engine.CreateNeuron(new Dictionary<Neuron, double>
        {
            { neuronA1, 1.5 },
            { neuronA2, 2 }
        });
        var neuronB2 = Engine.CreateNeuron(new Dictionary<Neuron, double>
        {
            { neuronA1, 1 },
            { neuronA2, 1 }
        });
        var neuronC = Engine.CreateNeuron(new Dictionary<Neuron, double>
        {
            { neuronB1, 1 },
            { neuronB2, 1 }
        });

        Engine.FeedForward(1, 1);

        Assert.AreEqual(5.5, neuronC.Value);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void CircularLink_Test()
    {
        // A -> B -> C -> D -> E
        // D -> B
        var neuronA = Engine.CreateInputNeuron();
        var neuronB = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronA, 1 } });
        var neuronC = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronB, 1 } });
        var neuronD = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronC, 1 } });
        var neuronE = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronD, 1 } });
        neuronB.InputNeurons.Add(neuronD, 1);

        Engine.FeedForward(1);
    }

    [TestMethod]
    public void DiffPaths_Test()
    {
        // A1 -> B -> C1 -> D -> E
        // A2 -> C2 -> E
        // D+C2 -> E
        var neuronA1 = Engine.CreateInputNeuron();
        var neuronA2 = Engine.CreateInputNeuron();
        var neuronB = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronA1, 2 } });
        var neuronC1 = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronB, 1 } });
        var neuronC2 = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronA2, 1 } });
        var neuronD = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronC1, 1 } });
        var neuronE = Engine.CreateNeuron(new Dictionary<Neuron, double>
        {
            { neuronD, 1 },
            { neuronC2, 1 }
        });

        Engine.FeedForward(1, 1);

        Assert.AreEqual(3, neuronE.Value);

        Engine.FeedForward(1, 1);

        Assert.AreEqual(3, neuronE.Value);
    }

    [TestMethod]
    public void TwoIndependedPaths_Test()
    {
        // A -> B
        // С -> D
        var neuronA = Engine.CreateInputNeuron();
        var neuronB = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronA, 2 } });
        var neuronC = Engine.CreateInputNeuron();
        var neuronD = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronC, -0.75 } });

        Engine.FeedForward(1, 2);

        Assert.AreEqual(2, neuronB.Value);
        Assert.AreEqual(-0.0149, neuronD.Value, 0.0001);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void NoNeurons_Test()
    {
        Engine.FeedForward();
    }

    [TestMethod]
    public void BackPropagation_Test()
    {
        // A -> B -> C
        var initialValue = 5;
        var expectedValue = 10;

        var neuronA = Engine.CreateInputNeuron();
        var neuronB = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronA, 1 } });
        var neuronC = Engine.CreateNeuron(new Dictionary<Neuron, double> { { neuronB, 1 } });

        for (var i = 0; i < 10; i++)
        {
            Engine.FeedForward(initialValue);
            Engine.BackPropagation(expectedValue);
        }

        Assert.AreEqual(expectedValue, neuronC.Value, 0.00001);
    }

    [TestMethod]
    public void BackPropagation_MoreComplex_Test()
    {
        // 3, 10, 2
        var initialValues = new[] { 0.1, 0.2, 0.3 };
        var expectedValues = new[] { 100.0, 200.0 };

        Engine.CreateAllToAllLayers(true, 3, 10, 2);

        for (var i = 0; i < 1000; i++)
        {
            Engine.FeedForward(initialValues);
            Engine.BackPropagation(expectedValues);
        }

        var outputs = Engine.Outputs;
        Console.WriteLine($"{outputs[0]}; {outputs[1]}");
    }

    [TestMethod]
    public void SampleNeuron_Test()
    {
        var initialValues = new[] { 1.0, 2.0, 3.0 };
        var expectedValues = new[] { 5.0, 10.0 };

        var nn = new NeuralNetwork(false, 0.01, Activation.Activation, Activation.ActivationDx, 3, 10, 2);

        var outputs = new double[2];

        for (var i = 0; i < 1000; i++)
        {
            outputs = nn.FeedForward(initialValues);
            nn.BackPropagation(expectedValues);
        }

        Console.WriteLine($"{outputs[0]}; {outputs[1]}");
    }
}