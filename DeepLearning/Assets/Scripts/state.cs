using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Problem
{
    CLASSIFICATION,
    REGRESSION
}

public class State
{
    public double learningRate = 0.03;
    public double regularizationRate = 0;
    public bool showTestData = false;
    public double noise = 0;
    public double batchSize = 10;
    public bool discretize = false;
    public string tutorial;
    public double percTrainData = 50;
    public ActivationFunction activation = new Activations.TANH();
    public RegularizationFunction regularization = null;
    public Problem problem = Problem.CLASSIFICATION;
    public List<string> inputs = new List<string>(new string[] {"x", "y"});
    public bool initZero = false;
    public bool hideText = false;
    public bool collectStats = false;
    public int numHiddenLayers = 1;
    public List<Object> hiddenLayerControls = new List<Object>();
    public List<int> networkShape = new List<int>(new int[]{4, 2});
    public bool x = true;
    public bool y = true;
    public bool xTimesY = false;
    public bool xSquared = false;
    public bool ySquared = false;
    public bool cosX = false;
    public bool sinX = false;
    public bool cosY = false;
    public bool sinY = false;
    //dataset: dataset.DataGenerator = dataset.classifyCircleData;
    //regDataset: dataset.DataGenerator = dataset.regressPlane;

    public Dataset.DataGenerator dataset = Dataset.classifyCircleData;
    public Dataset.DataGenerator regDataset = Dataset.regressPlane;
    public string seed;

    
}
