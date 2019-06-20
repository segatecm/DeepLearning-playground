using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Link
{
    public string id;
    public Node source;
    public Node dest;
    public double weight = UnityEngine.Random.Range(-0.5f, 0.5f);
    public bool isDead = false;
    public double errorDer = 0;
    public double accErrorDer = 0;
    public double numAccumulatedDers = 0;
    public RegularizationFunction regularization = null;

    public Link(Node source, Node dest, RegularizationFunction regularization, bool initZero)
    {
        this.id = source.id + "_" + dest.id;
        this.source = source;
        this.dest = dest;
        this.regularization = regularization;
        if (initZero)
            this.weight = 0;
    }

}


public class Node
{
    public string id;
    public List<Link> inputLinks = new List<Link>();
    public double bias = 0.1;
    public List<Link> outputs = new List<Link>();
    public double totalInput;
    public double output;

    public double outputDer;
    public double inputDer;

    public double accInputDer;
    public double numAccumulatedDers;
    public ActivationFunction activation;

    public Node(string id, ActivationFunction activation, bool initZero)
    {
        this.id = id;
        this.activation = activation;
        if (initZero)
            this.bias = 0;
    }

    /** Recomputes the node's output and returns it. */
    public double updateOutput()
    {
        this.totalInput = this.bias;

        for (int j = 0; j < this.inputLinks.Count; j++)
        {
            var link = this.inputLinks[j];
            this.totalInput += link.weight * link.source.output;
        }

        this.output = this.activation.output(this.totalInput);

        return this.output;
    }
}


public interface ErrorFunction
{
    double error(double output, double target);

    double der(double output, double target);
}

public interface ActivationFunction
{
    double output(double input);

    double der(double input);
}

public interface RegularizationFunction
{
    double output(double input);

    double der(double input);
}

namespace Errors
{
    public class SQUARE
    {
        public static double der(double output, double target)
        {
            return output - target;
        }

        public static double error(double output, double target)
        {
            return 0.5 * Math.Pow(output - target, 2);
        }
    }

    public class CROSSENTROPY
    {
        public static double der(double output, double target)
        {
            return target / output;
        }

        public static double error(double output, double target)
        {
            return target * Math.Log(output);
        }
    }

}

public class MathUtil
{
    public static double tanh(double x)
    {
        if (Double.IsPositiveInfinity(x))
            return 1;
        else if (Double.IsNegativeInfinity(x))
            return -1;
        else
        {
            var e2x = Math.Exp(2 * x);
            return (e2x - 1) / (e2x + 1);
        }
    }
}

namespace Activations
{
    


    public class TANH : ActivationFunction
    {
        public double der(double input)
        {
            var op = output(input);
            return 1 - op * op;
        }

        public double output(double input)
        {
            return MathUtil.tanh(input);
        }
    }

    public class RELU : ActivationFunction
    {
        public double der(double input)
        {
            return input <= 0 ? 0 : 1;
        }

        public double output(double input)
        {
            return Math.Max(0, input);
        }
    }

    public class SIGMOID : ActivationFunction
    {
        public double der(double input)
        {
            var op = output(input);
            return op * (1 - op);
        }

        public double output(double input)
        {
            return 1 / (1 + Math.Exp(-input));
        }
    }

    public class LINEAR : ActivationFunction
    {
        public double der(double input)
        {
            return 1;
        }

        public double output(double input)
        {
            return input;
        }
    }
    public class SOFTMAX : ActivationFunction
    {
        public double der(double input)
        {
            return Math.Exp(input);
        }

        public double output(double input)
        {
            return Math.Exp(input);
        }
    }
}

namespace RegularizationFunctions
{

    class L1 : RegularizationFunction
    {
        public double der(double input)
        {
            return input < 0 ? -1 : (input > 0 ? 1 : 0);
        }

        public double output(double input)
        {
            return Math.Abs(input);
        }
    }

    class L2 : RegularizationFunction
    {
        public double der(double input)
        {
            return input;
        }

        public double output(double input)
        {
            return 0.5 * input * input;
        }
    }

}


public class nn
{

    public static List<List<Node>> buildNetwork(List<int> networkShape, ActivationFunction activation,
        ActivationFunction outputActivation, RegularizationFunction regularization,
        List<string> inputIds, bool initZero)
    {
        var numLayers = networkShape.Count;
        var id = 1;

        List<List<Node>> network = new List<List<Node>>();

        for (var layerIdx = 0; layerIdx < numLayers; layerIdx++)
        {
            var isOutputLayer = layerIdx == numLayers - 1;
            var isInputLayer = layerIdx == 0;
            List<Node> currentLayer = new List<Node>();

            network.Add(currentLayer);

            int numNodes = networkShape[layerIdx];
            for (int i = 0; i < numNodes; i++)
            {
                var nodeId = id.ToString();
                if (isInputLayer)
                    nodeId = inputIds[i];
                else
                    id++;
                var node = new Node(nodeId, isOutputLayer ? outputActivation : activation, initZero);
                currentLayer.Add(node);

                if (layerIdx >= 1)
                {
                    for (int j = 0; j < network[layerIdx - 1].Count; j++)
                    {
                        var prevNode = network[layerIdx - 1][j];
                        var link = new Link(prevNode, node, regularization, initZero);
                        prevNode.outputs.Add(link);
                        node.inputLinks.Add(link);
                    }
                }
            }

        }


        return network;

    }

    public static double forwardProp(List<List<Node>> network, List<double> inputs)
    {
        var inputLayer = network[0];
        if (inputs.Count != inputLayer.Count)
            throw new Exception("The number of inputs must match the number of nodes in" +
        " the input layer");

        for (int i = 0; i < inputLayer.Count; i++)
        {
            var node = inputLayer[i];
            node.output = inputs[i];
        }

        for (int layerIndx = 1; layerIndx < network.Count; layerIndx++)
        {
            var currentLayer = network[layerIndx];

            for (int i = 0; i < currentLayer.Count; i++)
            {
                var node = currentLayer[i];
                node.updateOutput();
            }
        }

        return network[network.Count - 1][0].output;
    }

    
    public static void backProp(List<List<Node>> network, double target, Func<double, double, double> errorFun)
    {
        var outputNode = network[network.Count - 1][0];

        outputNode.outputDer = errorFun(outputNode.output, target);

        for (int layerIdx = network.Count - 1; layerIdx >= 1; layerIdx--)
        {
            var currentLayer = network[layerIdx];

            // Compute the error derivative of each node with respect to:
            // 1) its total input
            // 2) each of its input weights.
            for (var i = 0; i < currentLayer.Count; i++)
            {
                var node = currentLayer[i];
                node.inputDer = node.outputDer * node.activation.der(node.totalInput);
                node.accInputDer += node.inputDer;
                node.numAccumulatedDers++;
            }

            // Error derivative with respect to each weight coming into the node.
            for (int i = 0; i < currentLayer.Count; i++)
            {
                var node = currentLayer[i];

                for (int j = 0; j < node.inputLinks.Count; j++)
                {
                    var link = node.inputLinks[j];
                    if (link.isDead)
                        continue;

                    link.errorDer = node.inputDer * link.source.output;
                    link.accErrorDer += link.errorDer;
                    link.numAccumulatedDers++;
                }
            }

            if (layerIdx == 1)
                continue;

            var preLayer = network[layerIdx - 1];
            for (int i = 0; i < preLayer.Count; i++)
            {
                var node = preLayer[i];

                // Compute the error derivative with respect to each node's output.
                node.outputDer = 0;

                for (int j = 0; j < node.outputs.Count; j++)
                {
                    var output = node.outputs[j];
                    node.outputDer += output.weight * output.dest.inputDer;
                }
            }
        }
    }

    public static void updateWeights(List<List<Node>> network, double learningRate, double regularizationRate)
    {
        for (int layerIdx = 1; layerIdx < network.Count; layerIdx++)
        {
            var currentLayer = network[layerIdx];
            for (int i = 0; i < currentLayer.Count; i++)
            {
                var node = currentLayer[i];

                if (node.numAccumulatedDers > 0)
                {
                    node.bias -= learningRate * node.accInputDer / node.numAccumulatedDers;
                    node.accInputDer = 0;
                    node.numAccumulatedDers = 0;
                }

                for (int j = 0; j < node.inputLinks.Count; j++)
                {
                    var link = node.inputLinks[j];
                    if (link.isDead)
                        continue;

                    var regulDer = link.regularization != null ? link.regularization.der(link.weight) : 0;
                    if (link.numAccumulatedDers > 0)
                    {
                        link.weight = link.weight - (learningRate / link.numAccumulatedDers) * link.accErrorDer;

                        var newLinkWeight = link.weight - (learningRate * regularizationRate) * regulDer;

                        if (link.regularization != null && link.regularization.GetType() == typeof(RegularizationFunctions.L1) &&
                            link.weight * newLinkWeight < 0)
                        {
                            link.weight = 0;
                            link.isDead = true;
                        }
                        else
                            link.weight = newLinkWeight;
                    }
                    link.accErrorDer = 0;
                    link.numAccumulatedDers = 0;
                }
            }
        }
    }

    public delegate void Accessor(Node node);

    public static void forEachNode(List<List<Node>> network, bool ignoreInputs, Accessor accessor)
    {
        for (int layerIdx = ignoreInputs ? 1 : 0;
            layerIdx < network.Count;
            layerIdx ++)
        {
            var currentLayer = network[layerIdx];
            for (int i = 0; i < currentLayer.Count; i++)
            {
                var node = currentLayer[i];
                accessor(node);
            }
        }
    }

    public static Node getOutputNode(List<List<Node>> network)
    {
        return network[network.Count - 1][0];
    }
}



