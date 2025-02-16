using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class OneColorData
{
    public string name;
    public Color lightColor;
    public Color darkColor;
}

[Serializable]
public class ColorsData
{
    public List<OneColorData> colors;

    public Color GetLightColor(string name)
    {
        foreach(var c in colors)
        {
            if (c.name == name)
                return c.lightColor;
        }

        return Color.white;
    }

    public Color GetDarkColor(string name)
    {
        foreach(var c in colors)
        {
            if (c.name == name)
                return c.darkColor;
        }

        return Color.black;
    }
}

