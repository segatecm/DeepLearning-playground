using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ImageHeatmap : MonoBehaviour
{

    public string id;
    HeatMap heatMap;


    public System.Action<string> onClick;

    // Use this for initialization
    void Awake()
    {
        this.heatMap = new HeatMap(Playground.DENSITY, Playground.xDomain, Playground.xDomain);
        GetComponent<Image>().sprite = UnityEngine.Sprite.Create(heatMap.texture, new Rect(0, 0, heatMap.texture.width, heatMap.texture.height), new Vector2(0.5f, 0.5f));
    }

    private void Start()
    {
        Toggle toggle = GetComponent<Toggle>();
        if (toggle != null)
        {
            setImageColor(toggle.isOn, false);
            toggle.onValueChanged.AddListener((check) =>
            {
                setImageColor(check, true);
            });
        }
    }


    public void updateHeatmap(Dictionary<string, double[][]> boundary, bool discretize)
    {
        if (this.heatMap != null)
        {
            double[][] data;
            if (boundary.TryGetValue(id, out data))
            {
                heatMap.updateBackground(data, discretize);
            }
        }

    }

    public void updatePoints(List<Dataset.Example2D> points)
    {
        if (this.heatMap != null)
        {
            heatMap.updateCircles(points, false);
        }
    }

    public void ToggleChanged(bool check)
    {
        setImageColor(check, true);
    }

    public void setImageColor(bool check, bool rebuild)
    {
        Image[] images = GetComponentsInChildren<Image>();
        foreach(var image in images)
        {
            if (check)
            {
                image.color = new Color(1, 1, 1);
                
            }
            else
                image.color = new Color(1, 1, 1, 0.2f);
        }

        if (rebuild)
        {
            if (onClick != null)
                onClick(this.id);
        }
    }

}
