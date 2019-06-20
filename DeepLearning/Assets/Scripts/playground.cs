using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class InputFeature
{
    public string label;
    public Func<double, double, double> f;

    public InputFeature(string label, Func<double, double, double> f)
    {
        this.label = label;
        this.f = f;
    }
}



public class Playground
{
    public Dictionary<string, InputFeature> INPUTS = new Dictionary<string, InputFeature>();
    public Dictionary<string, double[][]> boundary = new Dictionary<string, double[][]>();
    public State state = new State();

    public const int DENSITY = 100;
    const int NUM_SAMPLES_CLASSIFY = 500;
    const int NUM_SAMPLES_REGRESS = 1200;

    public static double[] xDomain = new double[2] { -6, 6 };

    public int iter;
    List<Dataset.Example2D> trainData = new List<Dataset.Example2D>();
    List<Dataset.Example2D> testData = new List<Dataset.Example2D>();
    public double lossTrain = 0;
    public double lossTest = 0;

    public List<List<Node>> network = new List<List<Node>>();

    public HeatMap heatMap;
    public Linechart lineChart;

    string selectedNodeId = null;

    public Dictionary<string, Dataset.DataGenerator> datasets = new Dictionary<string, Dataset.DataGenerator>();

    void initInputs()
    {
        INPUTS.Add("x", new InputFeature("X_1", (x, y) => { return x; }));
        INPUTS.Add("y", new InputFeature("X_2", (x, y) => { return y; }));
        INPUTS.Add("xSquared", new InputFeature("X_1^2", (x, y) => { return x * x; }));
        INPUTS.Add("ySquared", new InputFeature("X_2^2", (x, y) => { return y * y; }));
        INPUTS.Add("xTimesY", new InputFeature("X_1X_2", (x, y) => { return x * y; }));
        INPUTS.Add("sinX", new InputFeature("sin(X_1)", (x, y) => { return Math.Sin(x); }));
        INPUTS.Add("sinY", new InputFeature("sin(X_2)", (x, y) => { return Math.Sin(y); }));
    }

    void initDatasets()
    {
        datasets.Add("circle", Dataset.classifyCircleData);
        datasets.Add("xor", Dataset.classifyXORData);
        datasets.Add("gauss", Dataset.classifyTwoGaussData);
        datasets.Add("spiral", Dataset.classifySpiralData);
    }

    public Playground()
    {
        initInputs();
        initDatasets();

        this.heatMap = new HeatMap(DENSITY, xDomain, xDomain);
        this.lineChart = new Linechart(256, 128);

        generateData();
        reset();
    }


    public void oneStep()
    {
        iter++;
        int i = 0;
        trainData.ForEach((point) =>
        {
            var input = constructInput(point.x, point.y);
            nn.forwardProp(network, input);
            nn.backProp(this.network, point.label, Errors.SQUARE.der);
            if ((i + 1) % state.batchSize == 0)
                nn.updateWeights(this.network, state.learningRate, state.regularizationRate);
            i++;
        });

        lossTrain = getLoss(this.network, trainData);
        lossTest = getLoss(this.network, testData);
        updateUI(false);
    }

    public void updateUI(bool firstStep)
    {
        updateDecisionBoundary(this.network, firstStep);
        var selectedId = selectedNodeId != null ?
                selectedNodeId : nn.getOutputNode(network).id;

        this.heatMap.updateBackground(boundary[selectedId], this.state.discretize);
        this.heatMap.updatePoints(trainData);
        if (this.state.showTestData == true)
            this.heatMap.updatePoints(testData);
        this.lineChart.addData(lossTrain, lossTest);
    }

    void updateDecisionBoundary(List<List<Node>> network, bool firstTime)
    {
        if (firstTime)
        {
            
            nn.forEachNode(network, true, (node) =>
            {
                boundary[node.id] = new double[DENSITY][];
            });

            foreach (var nodeId in INPUTS.Keys)
            {
                boundary[nodeId] = new double[DENSITY][];
            }
        }
        Func<double, double> xScale = (v) => Dataset.Linear(0, DENSITY - 1, xDomain[0], xDomain[1], v);
        Func<double, double> yScale = (v) => Dataset.Linear(DENSITY - 1, 0, xDomain[0], xDomain[1], v);

        int i = 0, j = 0;

        for (i = 0; i < DENSITY; i++)
        {
            if (firstTime)
            {
                nn.forEachNode(network, true, node =>
                {
                    boundary[node.id][i] = new double[DENSITY];
                });

                foreach (var nodeId in INPUTS.Keys)
                {
                    ((object[])boundary[nodeId])[i] = new double[DENSITY];
                }
            }

            for (j = 0; j < DENSITY; j++)
            {
                var x = xScale(i);
                var y = yScale(j);

                var input = constructInput(x, y);

                nn.forwardProp(network, input);
                nn.forEachNode(network, true, node =>
                {
                    boundary[node.id][i][j] = node.output;
                });

                if (firstTime)
                {
                    foreach (var nodeId in INPUTS.Keys)
                    {
                        boundary[nodeId][i][j] = INPUTS[nodeId].f(x, y);
                    }
                }
            }
        }

        
    }

    List<double> constructInput(double x, double y)
    {
        var input = new List<double>();
 
        foreach (var i in state.inputs)
        {
            input.Add(INPUTS[i].f(x, y));
        }
        return input;
        
    }

    List<string> constructInputIds()
    {
        return state.inputs;
    }

    double getLoss(List<List<Node>> network, List<Dataset.Example2D> dataPoints)
    {
        double loss = 0;
        for (int i= 0; i < dataPoints.Count; i++)
        {
            var dataPoint = dataPoints[i];
            var input = constructInput(dataPoint.x, dataPoint.y);
            var output = nn.forwardProp(network, input);
            loss += Errors.SQUARE.error(output, dataPoint.label);
        }
        return loss / dataPoints.Count;
    }

    // (begin, end]
    List<Dataset.Example2D> slice(List<Dataset.Example2D> data, int begin, int end)
    {
        List<Dataset.Example2D> l = new List<global::Dataset.Example2D>();

        for (int i = begin; i < end; i++)
        {
            l.Add(data[i]);
        }

        return l;
    }

    public void generateData()
    {
        var numSamples = state.problem == Problem.REGRESSION ? NUM_SAMPLES_REGRESS : NUM_SAMPLES_CLASSIFY;
        var generator = state.problem == Problem.CLASSIFICATION ? state.dataset : state.regDataset;
        var data = generator(numSamples, state.noise / 100);

        Dataset.shuffle<Dataset.Example2D>(data);
        int splitIndex = (int)Math.Floor(data.Count * state.percTrainData / 100);

        trainData = slice(data, 0, splitIndex);
        testData = slice(data, splitIndex, data.Count);
        heatMap.updatePoints(trainData);
        if (this.state.showTestData)
            heatMap.updatePoints(testData);

    }

    public void reset()
    {

        iter = 0;
        var numInputs = constructInput(0, 0).Count;
        List<int> shape = new List<int>();
        shape.Add(numInputs);
        shape.AddRange(state.networkShape);
        shape.Add(1);


        ActivationFunction outputActivation = null;
        if (state.problem == Problem.REGRESSION)
            outputActivation = new Activations.LINEAR();
        else
            outputActivation = new Activations.TANH();

        network = nn.buildNetwork(shape, state.activation, outputActivation,
            state.regularization, constructInputIds(), state.initZero);

        lossTrain = getLoss(network, trainData);
        lossTest = getLoss(network, testData);
        updateUI(true);
        this.lineChart.reset();

    }

    public void changeDataset(string name)
    {
        Dataset.DataGenerator newDataset;
        
        if (datasets.TryGetValue(name, out newDataset) == false)
        {
            return; // No-op.
        }
        if (state.dataset == newDataset)
            return;

        state.dataset = newDataset;
        
        generateData();
        //parametersChanged = true;
        reset();
    }
    
}


