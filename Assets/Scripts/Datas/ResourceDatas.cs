using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum ResourceType
{
    Energy,
    Crystal,
    Titanium,
    Oil,
    EnemyPart,
    Water,
}

[Serializable]
public class OneResourceData
{
    public ResourceType type;
    public Sprite sprite;
}

[Serializable]
public class ResourceDatas
{
    [SerializeField] List<OneResourceData> m_resources;

    public OneResourceData GetResource(ResourceType type)
    {
        foreach (var r in m_resources)
        {
            if (r.type == type)
                return r;
        }

        return null;
    }
}
