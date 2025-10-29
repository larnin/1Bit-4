using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class UndoElementBlocks : UndoElementBase
{
    struct BlockInfo
    {
        public Vector3Int pos;
        public Block oldBlock;
        public Block newBlock;
    }

    List<BlockInfo> m_blocks = new List<BlockInfo>();

    public void AddBlock(Vector3Int pos, Block oldBlock, Block newBlock)
    {
        var info = new BlockInfo();
        info.pos = pos;
        info.oldBlock = oldBlock;
        info.newBlock = newBlock;
    }

    public override void Apply()
    {
        var editor = EditorGridBehaviour.instance;
        if (editor == null)
            return;

        if (m_blocks.Count == 0)
            return;

        Vector3Int min = m_blocks[0].pos;
        Vector3Int max = m_blocks[0].pos;

        foreach (var b in m_blocks)
        {
            editor.SetBlock(b.pos, b.oldBlock, false);

            min.x = Mathf.Min(min.x, b.pos.x);
            min.y = Mathf.Min(min.y, b.pos.y);
            min.z = Mathf.Min(min.z, b.pos.z);

            max.x = Mathf.Max(max.x, b.pos.x);
            max.y = Mathf.Max(max.y, b.pos.y);
            max.z = Mathf.Max(max.z, b.pos.z);
        }

        editor.SetRegionDirty(new BoundsInt(min, max - min));
    }

    public override UndoElementBase GetRevertElement()
    {
        var revert = new UndoElementBlocks();
        foreach(var b in m_blocks)
            revert.AddBlock(b.pos, b.newBlock, b.oldBlock);

        return revert;
    }
}
