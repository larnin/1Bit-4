using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class GameEntityData
{
    public GameEntityType type;
    public GameObject prefab;
}

[Serializable]
public class QuestElementData
{
    public QuestElementType type;
    public GameObject prefab;
}

[Serializable]
public class ProjectileData
{
    public ProjectileType type;
    public GameObject prefab;
}

[Serializable]
public class EditorDatas
{
    public string editorLayer;
    public Material cursorMaterial;
    public LayerMask toolHoverLayer;

    public LayerMask groundLayer;

    public Material questElementMaterial;

    public List<GameEntityData> entities;
    public List<ProjectileData> projectiles;
    public List<QuestElementData> questElements;

    public GameObject GetQuestElementPrefab(QuestElementType type)
    {
        foreach(var e in questElements)
        {
            if (e.type == type)
                return e.prefab;
        }

        return null;
    }

    public GameObject GetEntityPrefab(GameEntityType type)
    {
        foreach(var e in entities)
        {
            if (e.type == type)
                return e.prefab;
        }

        return null;
    }

    public GameObject GetProjectilePrefab(ProjectileType type)
    {
        foreach(var e in projectiles)
        {
            if (e.type == type)
                return e.prefab;
        }

        return null;
    }
}
