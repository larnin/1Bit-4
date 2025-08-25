using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum GameEntityType
{

}

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
public class EditorDatas
{
    public string editorLayer;
    public Material cursorMaterial;
    public LayerMask toolHoverLayer;

    public LayerMask groundLayer;

    public Material questElementMaterial;

    public List<GameEntityData> entities;
    public List<QuestElementData> questElements;
}
