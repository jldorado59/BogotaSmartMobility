using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class Centroid
{
    public double x { get; set; }
    public double z { get; set; }
}

public class Point
{
    public double x { get; set; }
    public double z { get; set; }
}

public class Feature
{
    public int id { get; set; }
    public int pisos { get; set; }
    public Centroid centroid { get; set; }
    public List<Point> points { get; set; }
}

public class RootObject
{
    public List<Feature> features { get; set; }
}