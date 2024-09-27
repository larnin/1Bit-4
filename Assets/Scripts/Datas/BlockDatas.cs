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
public class BlockDatas
{
    public Material defaultMaterial;

    public Material waterMaterial;
}
