using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CommandEngine.AI
{
    class Neuron
    {
        private ulong neuronId;
        private Dictionary<ulong, Axon> connections = new Dictionary<ulong, Axon>();
        private double value = 0.0;
        private double bias = 1.0;

        public Neuron(ulong id)
        {
            neuronId = id;
            value = Mathc.Random.NormalDouble * 2 - 1;
        }

        public void ReceiveValue(double value)
        {
            this.value += value;
        }
        public void ProcessValue()
        {
            value = Mathc.Sigmoid(value, bias);
            if(SendValue())
                value = 0.0;
        }
        private bool SendValue()
        {
            foreach (var axon in connections.Values)
                axon.TransmitValue(value);
            return connections.Count > 0;
        }
        public void Mutate()
        {
            bias += bias * Mathc.Random.Marsaglia(true);
            foreach (var axon in connections.Values)
                axon.Mutate();
        }

        public bool Connect(Neuron n)
        {
            if (connections.ContainsKey(n.neuronId))
                return false;

            connections.Add(n.neuronId, new Axon(n));
            return true;
        }
        public bool Disconnect(Neuron n)
        {
            if (!connections.ContainsKey(n.neuronId))
                return false;

            connections.Remove(n.neuronId);
            return false;
        }
        public double GetValue()
        {
            return value;
        }
    }
    class Axon
    {
        private Neuron connection;
        private double weight;

        public Axon(Neuron n)
        {
            connection = n;
            weight = Mathc.Random.NormalDouble * 2 - 1;
        }
        public void TransmitValue(double value)
        {
            connection.ReceiveValue(value * weight);
        }
        public void Mutate()
        {
            weight += weight * Mathc.Random.Marsaglia(true);
        }
    }

    class NeuronLayerWrapper
    {
        public List<Neuron> neurons;

        public int NeuronCount { get { return neurons.Count; } }

        public NeuronLayerWrapper(int numNeurons)
        {
            neurons = new List<Neuron>();
            for (int i = 0; i < numNeurons; ++i)
                neurons.Add(new Neuron((ulong)i));
        }
        public NeuronLayerWrapper(List<Neuron> neurons)
        {
            this.neurons = neurons;
        }

        public void ConnectToLayer(NeuronLayerWrapper layer)
        {
            foreach (var neuron in neurons)
                foreach (var otherNeuron in layer.neurons)
                    neuron.Connect(otherNeuron);
        }
    }

    public class NeuralNetwork
    {
        List<NeuronLayerWrapper> networkLayers = new List<NeuronLayerWrapper>();
        NeuronLayerWrapper inputLayer { get { return networkLayers[0]; } }
        NeuronLayerWrapper outputLayer { get { return networkLayers[networkLayers.Count - 1]; } }

        public NeuralNetwork(int inputHeight, int outputHeight, params int[] hiddenLayerHeights)
        {
            networkLayers.Add(new NeuronLayerWrapper(inputHeight));
            for (int i = 0; i < hiddenLayerHeights.Length; ++i)
                networkLayers.Add(new NeuronLayerWrapper(hiddenLayerHeights[i]));
            networkLayers.Add(new NeuronLayerWrapper(outputHeight));

            for (int i = 0; i < networkLayers.Count - 1; ++i)
            {
                networkLayers[i].ConnectToLayer(networkLayers[i + 1]);
            }
        }

        public double[] Process(params double[] inputs)
        {
            if (inputs.Length < outputLayer.NeuronCount)
                throw new Exception("ERROR: Not enough inputs to feed network");

            double[] retval = new double[outputLayer.NeuronCount];

            for(int i = 0; i < inputs.Length; ++i)
            {
                Neuron n = inputLayer.neurons[i];
                n.ReceiveValue(inputs[i]);
                n.ProcessValue();
            }
            for (int i = 1; i < networkLayers.Count; ++i)
                foreach (var n in networkLayers[i].neurons)
                    n.ProcessValue();

            for (int i = 0; i < outputLayer.NeuronCount; ++i)
            {
                retval[i] = outputLayer.neurons[i].GetValue();
            }

            return retval;
        }

        public void Mutate()
        {
            foreach (var layer in networkLayers)
                foreach (var neuron in layer.neurons)
                    neuron.Mutate();
        }
    }
}
