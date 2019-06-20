using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Dataset
{
    public struct Example2D
    {
        public double x;
        public double y;
        public double label;

        public Example2D(double x, double y, double label)
        {
            this.x = x;
            this.y = y;
            this.label = label;
        }
    }

    public struct Point
    {
        public double x;
        public double y;

        public Point(double x, double y)
        {
            this.x = x;
            this.y = y;
        }
    }


    public static void shuffle<T>(List<T> array)
    {
        var counter = array.Count;
        T temp;

        int index = 0;

        while(counter > 0)
        {
            index = UnityEngine.Random.Range(0, counter);
            counter--;

            temp = array[counter];
            array[counter] = array[index];
            array[index] = temp;
        }
    }

    public delegate List<Example2D> DataGenerator(int numSamples, double noise);

    public static List<Example2D> classifyTwoGaussData(int numSamples, double noise)
    {
        var points = new List<Example2D>();
        var variance = Linear(0, .5, .5, 4, noise);

        Action<double, double, double> genGauss = (cx, cy, label) =>
          {
              for (int i = 0; i < numSamples / 2; i++)
              {
                  var x = normalRandom(cx, variance);
                  var y = normalRandom(cy, variance);
                  points.Add(new Example2D(x, y, label));
              }
          };
        genGauss(2, 2, 1);
        genGauss(-2, -2, -1);
        return points;
    }

    public static double randUniform(double a, double b)
    {
        return UnityEngine.Random.Range((float)a, (float)b);
        /*
        System.Random r = new System.Random();
        double d = r.NextDouble();
        return a + (b - a) * d;
        */
    }

    /**
     * Samples from a normal distribution. Uses the seedrandom library as the
     * random generator.
     *
     * @param mean The mean. Default is 0.
     * @param variance The variance. Default is 1.
     */
    public static double normalRandom(double mean, double variance)
    {
        double v1, v2, s;

        do
        {
            v1 = 2 * UnityEngine.Random.Range(0.0f, 1.0f) - 1;
            v2 = 2 * UnityEngine.Random.Range(0.0f, 1.0f) - 1;
            s = v1 * v1 + v2 * v2;
        } while (s > 1);

        var result = Math.Sqrt(-2 * Math.Log(s) / s) * v1;
        return mean + Math.Sqrt(variance) * result;
    }

    public static double dist(Point a, Point b)
    {
        var dx = a.x - b.x;
        var dy = a.y - b.y;

        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static double Linear(double domainMin, double domainMax, double rangeMin, double rangeMax, double value)
    {
        double dv = value - domainMin;
        double df = domainMax - domainMin;
        dv = dv / df;
        return rangeMin + (rangeMax - rangeMin) * dv;

    }

    public static List<Example2D> classifyCircleData(int numSamples, double noise)
    {
        List<Example2D> points = new List<Example2D>();
        double radius = 5;

        Func<Point, Point, double> getCircleLabel = (p, center) =>
        {
            return dist(p, center) < (radius * 0.5) ? 1 : -1;
        };

        for (int i = 0; i < numSamples / 2; i++)
        {
            var r = randUniform(0, radius * 0.5);
            var angle = randUniform(0, 2 * Math.PI);
            var x = r * Math.Sin(angle);
            var y = r * Math.Cos(angle);

            var noiseX = randUniform(-radius, radius) * noise;
            var noiseY = randUniform(-radius, radius) * noise;
            var label = getCircleLabel(new Point(x + noiseX, y + noiseY), new Point(0, 0));
            points.Add(new Example2D(x, y, label));
        }

        for (int i = 0; i < numSamples / 2; i++)
        {
            var r = randUniform(radius * 0.7, radius);
            var angle = randUniform(0, 2 * Math.PI);
            var x = r * Math.Sin(angle);
            var y = r * Math.Cos(angle);

            var noiseX = randUniform(-radius, radius) * noise;
            var noiseY = randUniform(-radius, radius) * noise;
            var label = getCircleLabel(new Point(x + noiseX, y + noiseY), new Point(0, 0));
            points.Add(new Example2D(x, y, label));
        }

        return points;
    }

    public static List<Example2D> classifyXORData(int numSamples, double noise)
    {
        List<Example2D> points = new List<Example2D>();
        
        for (int i = 0; i < numSamples; i++)
        {
            var x = randUniform(-5, 5);
            double padding = 0.3;

            x += x > 0 ? padding : -padding;

            var y = randUniform(-5, 5);
            y += y > 0 ? padding : -padding;

            var noiseX = randUniform(-5, 5) * noise;
            var noiseY = randUniform(-5, 5) * noise;

            var label = (x + noiseX) * (y + noiseY) >= 0 ? 1 : -1;

            points.Add(new Example2D(x, y, label));
        }



        return points;
    }

    public static List<Example2D> classifySpiralData(int numSample, double noise)
    {
        List<Example2D> points = new List<Example2D>();

        var n = numSample / 2;

        Action<double, double> genSpiral = (deltaT, label) =>
        {
            for (int i = 0; i < n; i++)
            {
                var r = (double)i / (double)n * 5.0;
                var t = 1.75 * i / n * 2 * Math.PI + deltaT;
                var x = r * Math.Sin(t) + randUniform(-1, 1) * noise;
                var y = r * Math.Cos(t) + randUniform(-1, 1) * noise;
                points.Add(new Example2D(x, y, label));
            }
        };

        genSpiral(0, 1);
        genSpiral(Math.PI, -1);

        return points;
    }

    public static List<Example2D> regressPlane(int numSamples, double noise)
    {
        double radius = 6;

        Func<double, double, double> getLabel = (x, y) => Linear(-10, 10, -1, 1, x + y);
        List<Example2D> points = new List<Example2D>();

        for (int i = 0; i < numSamples; i++)
        {
            var x = randUniform(-radius, radius);
            var y = randUniform(-radius, radius);

            var noiseX = randUniform(-radius, radius) * noise;
            var noiseY = randUniform(-radius, radius) * noise;

            var label = getLabel(x + noiseX, y + noiseY);
            points.Add(new Example2D(x, y, label));
        }

        return points;
    }
}





