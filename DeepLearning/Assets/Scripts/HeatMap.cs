using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class HeatMap
{

    Texture2D canvas;
    Color32[] canvasData;

    int width, height;

    const int NUM_SHADES = 30;

    Func<double, int> xScale;
    Func<double, int> yScale;

    Color32 c0 = new Color32(0x08, 0x77, 0xbd, 255);
    Color32 c1 = new Color32(0xf5, 0x93, 0x22, 255);

    public Texture2D texture
    {
        get { return canvas;  }
    }

    public HeatMap(int width, double[] xDomain, double[] yDomain)
    {
        this.width = width;
        this.height = width;

        xScale = (v) => { return (int)Dataset.Linear(xDomain[0], xDomain[1], 0, this.width, v); };
        yScale = (v) => { return (int)Dataset.Linear(yDomain[0], yDomain[1], 0, this.width, v); };

        this.canvas = new Texture2D(this.width, this.height, TextureFormat.ARGB32, false);
        
        this.canvasData = new Color32[width * height];
        for (int i = 0; i < width * height; i++)
            this.canvasData[i] = new Color32(255, 255, 255, 255);

        this.canvas.SetPixels32(this.canvasData);
        this.canvas.Apply();
    }

    void SetColor(int x, int y, Color32 c)
    {
        int pos = y * this.width + x;
        canvasData[pos] = c;
    }

    public void updateBackground(double[][] data, bool discretize)
    {

        int dx = data[0].Length;
        int dy = data.Length;

        

        Color32 center = new Color32(255, 255, 255, 255);

        for (int y = 0; y < dy; y++)
        {
            for (int x = 0; x < dx; x++)
            {
                double d = data[x][y];
                if (d >= 0)
                    SetColor(x, y, Color32.Lerp(center, this.c0, (float)d));
                else
                    SetColor(x, y, Color32.Lerp(center, this.c1, -(float)d));
            }
        }


        //Debug.Log(data[50][50] + " " + data[51][50]);

        this.canvas.SetPixels32(canvasData);

        this.canvas.Apply();
    }

    public void updatePoints(List<Dataset.Example2D> points)
    {
        updateCircles(points);
    }

    public void updateCircles(List<Dataset.Example2D> points, bool withBorder = true)
    {
        foreach (var point in points)
        {
            var x = xScale(point.x);
            var y = yScale(point.y);

            Color32 pointColor = new Color32(127, 127, 127, 255);
            if (!withBorder)
            {
                if (point.label == 1)
                    pointColor = c0;
                else
                    pointColor = c1;
            }
            var lx = x - 1;
            var rx = x + 1;

            if (lx < 0) lx = 0;
            if (rx > this.width) rx = this.width;

            var ty = y - 1;
            var by = y + 1;
            if (ty < 0) ty = 0;
            if (ty > this.height) ty = this.height;


            SetColor(lx, y, pointColor);
            SetColor(x, ty, pointColor);
            SetColor(rx, y, pointColor);
            SetColor(x, by, pointColor);
            

            if (point.label == 1)
                SetColor(x, y, c0);
            else
                SetColor(x, y, c1);
        }

        this.canvas.SetPixels32(canvasData);

        this.canvas.Apply();
    }

    Color32 RangeColor(double v)
    {
        return new Color32();
    }

}
