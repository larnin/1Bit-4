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

    public float minBaseHeight = -10;
    public float maxBaseHeight = 10;
    public float plateformSurfaceScale = 0.1f;

    public PerlinSettings MontainsPlacement;
    public float montainsMinPlacementHeight;
    public float montainsMaxPlacementHeight;
    public PerlinSettings MontainsHeight;
    public float montainsHeightMultiplier = 1;
    public float montainsHeightPower = 1;
    public float montainsHeightOffset = 10;
    public float montainsMinPlacementDist = 0.2f;
    public float montainsMaxPacementDist = 0.8f;
    public float montainsPlacementBlendDist = 0.1f;

    public PerlinSettings islandBorder;
    public float islandProportion = 0.9f;
    public PerlinSettings waterZones;
    public float waterHeight = 0;

}

[Serializable]
public class PerlinSettings
{
    public int baseFrequency = 1;
    public float baseAmplitude = 1;
    public int nbLayers = 1;
    public float layerAmplitudeScale = 0.5f;
}