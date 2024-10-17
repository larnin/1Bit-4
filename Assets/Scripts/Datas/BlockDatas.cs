using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

enum BlockFace
{
    Top,
    Bottom,
    Front,
    Back,
    Left,
    Right
}

[Serializable]
public class CustomBlock
{
    public BlockType type;
    public GameObject prefab;
}

[Serializable]
public class BlockDatas
{
    public Material defaultMaterial;

    public Material waterMaterial;

    public List<CustomBlock> customBlocks;

    public bool IsCustomBlock(BlockType type)
    {
        foreach (var b in customBlocks)
        {
            if (b.type == type)
                return true;
        }
        return false;
    }

    public CustomBlock GetCustomBlock(BlockType type)
    {
        foreach (var b in customBlocks)
        {
            if (b.type == type)
                return b;
        }
        return null;
    }
}
