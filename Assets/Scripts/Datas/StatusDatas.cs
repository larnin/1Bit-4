using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class StatusDatas
{
    public BurningStatusDatas burning;
    public FrozenStatusDatas frozen;
}

[Serializable]
public class BurningStatusDatas
{
    public GameObject effectPrefab;
    public string icon;
    public float powerToDuration;
    public float powerToDot;
}

[Serializable]
public class FrozenStatusDatas
{
    public float powerToDuration;
    public string icon;
}
