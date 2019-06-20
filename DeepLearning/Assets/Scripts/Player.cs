using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    private bool isPlaying = false;

    public delegate void CallBack(bool isPlaying);
    CallBack callback;

    public Playground playground;

    
    public Image imgHeatMap;
    public Image imgLineChart;

    public Text txtTestLoss;
    public Text txtTrainLoss;
    public Text txtEpoch;

    public Canvas mainCanvas;
    public GameObject hiddenLayers;
    public GameObject featureLayer;

    public Button startButton;
    public GameObject dataGroup;

    public Dropdown dropRate;
    public Dropdown dropActivation;
    public Dropdown dropRegularization;
    public Dropdown dropRegularizationRate;

    public Text txtBatchSize;
    public Text txtNoise;
    public Text txtRatio;

    public Sprite imgStart;
    public Sprite imgPause;

    public GameObject[] features;
    
    public void playOrPause()
    {
        if (this.isPlaying)
        {
            this.isPlaying = false;

        }
    }

    public void onPlayPause(CallBack callback)
    {
        this.callback = callback;
    }

    public void play()
    {
        if (this.isPlaying == false)
        {
            this.isPlaying = true;
            if (this.callback != null)
                this.callback(this.isPlaying);
            startButton.GetComponent<Image>().sprite = imgPause;
        }
        else
        {
            this.isPlaying = false;
            startButton.GetComponent<Image>().sprite = imgStart;
        }
    }

    public void reset()
    {
        if (isPlaying)
            play();
        this.playground.reset();
        resetUI();
    }


    private void Update()
    {
        if (this.isPlaying == true)
        {
            playground.oneStep();
            updateUI();
        }
    }

    void updateUI()
    {
        txtTestLoss.text = string.Format("Test Loss {0:0.000} ", this.playground.lossTest);
        txtTrainLoss.text = string.Format("Train Loss {0:0.000} ", this.playground.lossTrain);
        txtEpoch.text = string.Format("{0:000,000}", this.playground.iter);

        ImageHeatmap[] heatMaps = mainCanvas.GetComponentsInChildren<ImageHeatmap>();
        foreach (var heatMap in heatMaps)
        {
            heatMap.updateHeatmap(this.playground.boundary, false);
        }
    }

    void createHiddenLayers()
    {
        int layerNumber = this.playground.network.Count - 2;
        if (layerNumber > 0)
        {
            const int layerWidth = 300;


            GameObject obj = Resources.Load("Heatmap") as GameObject;
            Vector2 pos = new Vector2(0, -90);
            float offX = 0;
            if (layerNumber == 1)
                pos.x = 0;
            else
            {
                pos.x = -150;
                offX = layerWidth / (layerNumber - 1);
            }

            GameObject btns = Resources.Load("buttons") as GameObject;

            for (int i = 1; i < this.playground.network.Count - 1; i++)
            {
                GameObject btn = GameObject.Instantiate(btns, hiddenLayers.transform, true);
                btn.GetComponent<RectTransform>().anchoredPosition = new Vector2(pos.x, -50);
                Button[] new_buttons = btn.GetComponentsInChildren<Button>();

                int shapeId = i - 1;
                new_buttons[0].onClick.AddListener(() =>
                {
                    
                    if (this.playground.state.networkShape[shapeId] > 1)
                        this.playground.state.networkShape[shapeId]--;
                    reset();
                });
                new_buttons[1].onClick.AddListener(() =>
                {
                    if (this.playground.state.networkShape[shapeId] < 8)
                        this.playground.state.networkShape[shapeId]++;
                    reset();
                });



                var layer = this.playground.network[i];

                for (int j = 0; j < layer.Count; j++)
                {
                    var node = layer[j];

                    GameObject heatMap = GameObject.Instantiate(obj, hiddenLayers.transform, true);
                    RectTransform t = heatMap.GetComponent<RectTransform>();
                    t.anchoredPosition = pos;
                    ImageHeatmap imageHeatmap = heatMap.GetComponent<ImageHeatmap>();
                    imageHeatmap.id = node.id;
                    
                    pos.y -= 55;
                }

                pos.y = -90;
                pos.x += offX;
            }
        }
        
    }

    public void resetUI()
    {
        if (isPlaying)
        {
            isPlaying = false;
        }
        destroyHiddenLayer();
        createHiddenLayers();
        updateUI();
    }

    void destroyHiddenLayer()
    {
        int child = hiddenLayers.transform.childCount;
        for (int i = 0; i < child; i++)
        {
            GameObject.Destroy(hiddenLayers.transform.GetChild(i).gameObject);
        }
    }


    void createUI()
    {
        // create neural image 
        GameObject obj = Resources.Load("Heatmap") as GameObject;
        Vector2 pos = new Vector2(-20, -90);

        int idx = 0;
        foreach (var input in this.playground.INPUTS)
        {
            GameObject heatMap = GameObject.Instantiate(obj, featureLayer.transform, true);
            RectTransform t = heatMap.GetComponent<RectTransform>();
            t.anchoredPosition = pos;

            ImageHeatmap imageHeatmap = heatMap.GetComponent<ImageHeatmap>();
            imageHeatmap.id = input.Key;

            Toggle toggle = heatMap.GetComponent<Toggle>();
            if (this.playground.state.inputs.Contains(input.Key) == false)
                toggle.isOn = false;

            imageHeatmap.onClick = (id) =>
            {
                if (this.playground.state.inputs.Contains(id))
                    this.playground.state.inputs.Remove(id);
                else
                    this.playground.state.inputs.Add(id);
                reset();
            };

            GameObject feature = GameObject.Instantiate(features[idx++], featureLayer.transform, false);
            t = feature.GetComponent<RectTransform>();
            Vector2 tPos = t.anchoredPosition;
            tPos.y = pos.y;
            t.anchoredPosition = tPos;

            pos.y -= 55;

        }

        createHiddenLayers();
        createThumbnails();
    }

    private void Start()
    {
        this.playground = new Playground();
        createUI();
        updateUI();
        Texture2D texture = this.playground.heatMap.texture;
        imgHeatMap.sprite = UnityEngine.Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

        Texture2D lineChartTexture = this.playground.lineChart.texture;
        imgLineChart.sprite = UnityEngine.Sprite.Create(lineChartTexture, new Rect(0, 0, 256, 128), new Vector2(0.5f, 0.5f), 1);
    }

    void renderBezier(Vector2 v0, Vector2 v1)
    {
        //Handles.DrawBezier()
    }

    void renderThumbnail(HeatMap heatMap, Dataset.DataGenerator dataGenerator)
    {
        var data = dataGenerator(200, 0);
        heatMap.updateCircles(data, false);
    }

    void createThumbnails()
    {
        ImageHeatmap[] heatmaps = dataGroup.GetComponentsInChildren<ImageHeatmap>();

        foreach (var heatmap in heatmaps)
        {
            Dataset.DataGenerator generator;
            if (this.playground.datasets.TryGetValue(heatmap.id, out generator))
            {
                heatmap.updatePoints(generator(200, 0));
            }


            heatmap.onClick = (id)=>
            {
                this.playground.changeDataset(id);
                this.reset();                
            };
        }
    }

    public void addHiddenLayer(bool add)
    {
        if (add)
        {
            if (this.playground.state.networkShape.Count < 6)
                this.playground.state.networkShape.Add(2);
        }
        else
        {
            if (this.playground.state.networkShape.Count > 1)
                this.playground.state.networkShape.RemoveAt(this.playground.state.networkShape.Count - 1);
        }
        reset();
    }

    public void changeRate(int idx)
    {
        var v = dropRate.options[idx];
        this.playground.state.learningRate = double.Parse(v.text);
    }

    public void changeRatio(float v)
    {
        this.playground.state.percTrainData = v * 10;

        txtRatio.text = string.Format("{0}%", v * 10);
        this.playground.generateData();

        reset();
    }

    public void changeNoise(float v)
    {
        this.playground.state.noise = v;

        txtNoise.text = string.Format("{0}", v);
        this.playground.generateData();
        reset();

    }
    public void changeBatch(float v)
    {
        this.playground.state.batchSize = v;

        txtBatchSize.text = string.Format("{0:00}", v);
        reset();
    }


    public void changeActivation(int idx)
    {
        var v = this.dropActivation.options[idx];
        if (v.text == "Tanh")
            this.playground.state.activation = new Activations.TANH();
        else if (v.text == "ReLU")
            this.playground.state.activation = new Activations.RELU();
        else if (v.text == "Sigmoid")
            this.playground.state.activation = new Activations.SIGMOID();
        else if (v.text == "Linear")
            this.playground.state.activation = new Activations.LINEAR();

        reset();
    }

    public void changeRegularization(int idx)
    {
        var v = this.dropRegularization.options[idx];

        if (v.text == "None")
            this.playground.state.regularization = null;
        else if (v.text == "L1")
            this.playground.state.regularization = new RegularizationFunctions.L1();
        else if (v.text == "L2")
            this.playground.state.regularization = new RegularizationFunctions.L2();

        reset();
    }

    public void changeRegularizationRate(int idx)
    {
        var v = this.dropRegularizationRate.options[idx];

        this.playground.state.regularizationRate = double.Parse(v.text);
        reset();
    }

    public void showTestData(bool show)
    {
        this.playground.state.showTestData = show;
        this.playground.updateUI(false);
    }
}