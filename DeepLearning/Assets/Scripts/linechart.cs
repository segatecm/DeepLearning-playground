using System.Collections;
using System.Collections.Generic;
using UnityEngine;


struct DataPoint
{
    public int x;
    public double l0;
    public double l1;

    public DataPoint(int x, double l0, double l1)
    {
        this.x = x;
        this.l0 = l0;
        this.l1 = l1;
    }
}


public class Linechart
{
    Texture2D canvas;
    Color32[] canvasData;

    double minY = double.MaxValue;
    double maxY = double.MinValue;

    int width;
    int height;

    List<DataPoint> data = new List<DataPoint>();

    public Texture2D texture
    {
        get { return canvas; }
    }

    public void addData(double lossTrain, double lossTest)
    {
        

        data.Add(new DataPoint(data.Count, lossTrain, lossTest));
        data.ForEach((p) =>
        {
            this.minY = Mathf.Min((float)this.minY, (float)p.l0, (float)p.l1);
            this.maxY = Mathf.Max((float)this.maxY, (float)p.l0, (float)p.l1);
        });

        redraw();
    }

    public Linechart(int width, int height)
    {
        this.width = width;
        this.height = height;

        this.canvas = new Texture2D(this.width, this.height, TextureFormat.ARGB32, false);
        this.canvasData = new Color32[width * height];

        canvas.SetPixels32(canvasData);
        canvas.Apply();


        reset();
    }


    void redraw()
    {
        for (int i = 0; i < canvasData.Length; i++)
            canvasData[i] = new Color32(0, 0, 0, 0);

        if (data.Count > 1)
        {

            

            for (int i = 0; i < data.Count - 1; i++)
            {
                int x0 = (int)(Dataset.Linear(0, data.Count, 0, this.width - 1, i));
                int x1 = (int)(Dataset.Linear(0, data.Count, 0, this.width - 1, i + 1));
                int y0 = (int)(Dataset.Linear(this.minY, this.maxY, 0, this.height - 1, data[i].l0));
                int y1 = (int)(Dataset.Linear(this.minY, this.maxY, 0, this.height - 1, data[i + 1].l0));

                drawLine(x0, y0, x1, y1, new Color32(100, 100, 100, 255));
                Debug.Log(x0 + ":" + y0 + "-" + x1 + ":" + y1);

                y0 = (int)(Dataset.Linear(this.minY, this.maxY, 0, this.height - 1, data[i].l1));
                y1 = (int)(Dataset.Linear(this.minY, this.maxY, 0, this.height - 1, data[i + 1].l1));

                drawLine(x0, y0, x1, y1, new Color32(150, 150, 150, 255));
            }

            

        }

        canvas.SetPixels32(canvasData);
        canvas.Apply();
    }

    void drawPoint(int x, int y, Color32 c)
    {
        canvasData[y * this.width + x] = c;
    }

    void drawLine(int x0, int y0, int x1, int y1, Color32 c)
    {
        //Debug.Log(x0 + "," + y0 + "," + x1 + "," + y1);
        int deltaX = Mathf.Abs(x1 - x0);
        int deltaY = Mathf.Abs(y1 - y0);

        if (deltaX == 0 && deltaY == 0)
            return;

        int acc = 0;
        if (deltaY > deltaX)
        {
            int startY, endY;
            if (y0 < y1)
            {
                startY = y0;
                endY = y1;
            }
            else
            {
                startY = y1;
                endY = y0;
            }

            int accX, o;
            if (y0 > y1)
            {
                accX = x1;
                o = -1;
            }
            else
            {
                accX = x0;
                o = 1;
            }

            for (int i = startY; i < endY; i++)
            {
                acc += deltaX;
                if (acc > deltaY)
                {
                    acc -= deltaY;
                    accX += o;
                }
                drawPoint(accX, i, c);
            }
        }
        else
        {
            int accY, o;

            if (y0 > y1)
            {
                accY = y0;
                o = -1;
            }
            else
            {
                accY = y0;
                o = 1;
            }
            for (int i = x0; i < x1; i++)
            {
                acc += deltaY;
                if (acc > deltaX)
                {
                    acc -= deltaX;
                    accY += o;
                }
                drawPoint(i, accY, c);
            }
        }
    }

    public void reset()
    {
        data = new List<DataPoint>();
        minY = double.MaxValue;
        maxY = double.MinValue;

        redraw();
    }
}
