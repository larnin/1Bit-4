using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class WorldGeneratorSettings
{
    [HideInInspector]
    public int seed = 0;

    public int size = 16;
    public int height = 4;

    public PerlinSettings waterZones;
    public float waterHeight = 0;

    public PerlinSettings HeightPerlin;
    public float heightMultiplier = 1;
    public float heightPower = 1;

    public PerlinSettings islandBorder;
    public float islandProportion = 0.9f;
}

[Serializable]
public class PerlinSettings
{
    public int baseFrequency = 1;
    public float baseAmplitude = 1;
    public int nbLayers = 1;
    public float layerAmplitudeScale = 0.5f;
}