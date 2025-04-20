using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class WorldGeneratorSettings
{
    public int size = 16;
    public int height = 4;

    public bool loopX = false;
    public bool loopZ = false;

    public float waterHeight = 0;

    public float baseSurfaceHeight = 1;
    public float baseSurfaceNormalizedRadius = 0.7f;
    public float baseSurfaceNormalizedTopRadius = 0.6f;
    public Ease baseSurfaceDescreaseCurve = Ease.Linear;
    public PerlinSettings baseSurfaceRandomization;
    public PerlinSettings plainsHeight;
    public float montainsMinDistanceFromCenter = 10;
    public float montainsMinDistanceFromBorder = 10;
    public float montainsBlendDistance = 10;
    public PerlinSettings montainsHeight;
    public PerlinSettings montainsDistanceRandomization;
    public float montainsHeightOffset = 10;
    public float montainsHeightMultiplier = 1;
    public float montainsHeightPower = 1;

    public int lakeDensity = 0;
    public int lakeRetryCount = 2;
    public float lakeMinDistance = 30;
    public float lakeMinDistanceBetweenLakes = 20;
    public float lakeMinSize = 5;
    public float lakeMaxSize = 10;
    public float lakeDecreaseDistance = 5;
    public Ease lakeDescreaseCurve = Ease.Linear;
    public PerlinSettings lakeSurfaceRandomization;

    public float crystalPatchSizeBase = 5;
    public float crystalPatchIncrease = 10;
    public float crystalPatchDensity = 1;
    public float crystalRetryCount = 2;
    public float crystalInitialPatchDistance = 10;
    public float crystalMinDistance = 15;

    public float oilDensity = 1;
    public float oilRetryCount = 1;
    public float oilMinDistance = 20;

    public int titaniumPatchMin = 4;
    public int titaniumPatchMax = 10;
    public float titaniumPatchDensity = 1;
    public float titaniumElevationMinVariation = 5;
    public float titaniumRetryCount = 2;
    public float titaniumMinDistance = 20;
    public float titaniumMinHeight = 1;
    public float titaniumMaxHeight = 2;
    public float titaniumHeightNeighbour = 0.25f;
}

[Serializable]
public class PerlinSettings
{
    public int baseFrequency = 1;
    public float baseAmplitude = 1;
    public int nbLayers = 1;
    public float layerAmplitudeScale = 0.5f;
}