using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum BlockType : byte
{
    air,
    ground,
    water,
    crystal,
    oil,
    Titanium
}

public struct Block
{
    public BlockType type;
    public byte data;

    public Block(BlockType _type, byte _data = 0)
    {
        type = _type;
        data = _data;
    }
}

public enum BlockShape
{
    Full,
    CornerL,
    HalfTriangle,
    CornerS,
}

public enum BlockRotation
{
    rot_0,
    rot_90,
    rot_180,
    rot_270,
    rot_vert_0,
    rot_vert_90,
    rot_vert_180,
    rot_vert_270,
    rot_flip_0,
    rot_flip_90,
    rot_flip_180,
    rot_flip_270,
}

public static class BlockEx
{
    public static byte MakeData(BlockShape shape, BlockRotation rot)
    {
        return (byte)((((int)shape) << 4) + (int)rot);
    }

    public static BlockShape GetShapeFromData(byte data)
    {
        int shape = data >> 4;
        return (BlockShape)shape;
    }

    public static BlockRotation GetRotationFromData(byte data)
    {
        int rot = data & 0b1111;
        return (BlockRotation)rot;
    }

    enum BlockTristate
    {
        NotSet = 0,
        Set = 1,
        Unknow = 2,
    }

    class BlockVariant
    {
        public BlockShape shape;
        public BlockRotation rot;

        public NearMatrix3<int> mat;

        public BlockVariant(BlockShape _shape, BlockRotation _rot, NearMatrix3<int> _mat)
        {
            shape = _shape;
            rot = _rot;
            mat = _mat;
        }
    }

    static List<BlockVariant> validTypes = ConstructValidTypes();

    static List<BlockVariant> ConstructValidTypes()
    {
        var d = new List<BlockVariant>();

        d.Add(new BlockVariant(BlockShape.CornerL, BlockRotation.rot_0, new NearMatrix3<int>(new int[] {2, 2, 2, 2, 1, 2, 2, 2, 2,
                                                                                                        2, 1, 2, 1, 1, 1, 2, 1, 0,
                                                                                                        2, 2, 2, 2, 2, 0, 2, 0, 2})));
        d.Add(new BlockVariant(BlockShape.HalfTriangle, BlockRotation.rot_0, new NearMatrix3<int>(new int[] {2, 1, 2, 2, 1, 2, 2, 2, 2,
                                                                                                             2, 1, 2, 1, 1, 1, 2, 0, 2,
                                                                                                             2, 2, 2, 2, 0, 2, 2, 2, 2})));
        d.Add(new BlockVariant(BlockShape.CornerS, BlockRotation.rot_0, new NearMatrix3<int>(new int[] {2, 1, 2, 1, 1, 2, 2, 2, 2,
                                                                                                        1, 1, 2, 1, 1, 0, 2, 0, 2,
                                                                                                        2, 2, 2, 2, 0, 2, 2, 2, 2})));

        int nbRot = Enum.GetValues(typeof(BlockRotation)).Length;
        int nbTypes = d.Count;

        for (int i = 1; i < 4; i++)
        {
            var rot = (BlockRotation)i;

            for (int j = 0; j < nbTypes; j++)
                d.Add(new BlockVariant(d[j].shape, rot, RotateMatrix(d[j].mat, rot)));
        }

        return d;
    }

    static NearMatrix3<T> RotateMatrix<T>(NearMatrix3<T> source, BlockRotation rot)
    {
        var target = source.Copy();
        Vector3Int axis = Vector3Int.zero;
        Vector3Int firstRot = Vector3Int.zero;
        Vector3Int secondRot = Vector3Int.zero;
        int count = 0;

        if(rot <= BlockRotation.rot_270)
        {
            count = (int)rot;
            axis.y = 1;
            firstRot.x = 1;
            secondRot.z = 1;
        }
        else if(rot <= BlockRotation.rot_vert_270)
        {
            count = (int)(rot - BlockRotation.rot_vert_0);
            axis.x = 1;
            firstRot.z = 1;
            secondRot.y = 1;
        }
        else
        {
            count = (int)(rot - BlockRotation.rot_flip_0);
            axis.z = 1;
            firstRot.y = 1;
            secondRot.x = 1;
        }

        for(int i = 0; i < count; i++)
        {
            for(int axisIndex = -1; axisIndex <= 1; axisIndex++)
            {
                var pos1 = axis * axisIndex - firstRot - secondRot;
                var pos2 = axis * axisIndex + firstRot - secondRot;
                var pos3 = axis * axisIndex + firstRot + secondRot;
                var pos4 = axis * axisIndex - firstRot + secondRot;

                var temp = target.Get(pos1.x, pos1.y, pos1.z);
                target.Set(target.Get(pos2.x, pos2.y, pos2.z), pos1.x, pos1.y, pos1.z);
                target.Set(target.Get(pos3.x, pos3.y, pos3.z), pos2.x, pos2.y, pos2.z);
                target.Set(target.Get(pos4.x, pos4.y, pos4.z), pos3.x, pos3.y, pos3.z);
                target.Set(temp, pos4.x, pos4.y, pos4.z);

                pos1 = axis * axisIndex - firstRot;
                pos2 = axis * axisIndex - secondRot;
                pos3 = axis * axisIndex + firstRot;
                pos4 = axis * axisIndex + secondRot;

                temp = target.Get(pos1.x, pos1.y, pos1.z);
                target.Set(target.Get(pos2.x, pos2.y, pos2.z), pos1.x, pos1.y, pos1.z);
                target.Set(target.Get(pos3.x, pos3.y, pos3.z), pos2.x, pos2.y, pos2.z);
                target.Set(target.Get(pos4.x, pos4.y, pos4.z), pos3.x, pos3.y, pos3.z);
                target.Set(temp, pos4.x, pos4.y, pos4.z);
            }
        }

        return target;
    }

    public static Block MakeBlock(NearMatrix3<Block> nearBlocks)
    {
        var current = nearBlocks.Get(0, 0, 0).type;
        if(BlockEx.IsComplexeBlock(current))
        {
            foreach(var v in validTypes)
            {
                if(IsEquivalent(nearBlocks, v.mat))
                {
                    var data = MakeData(v.shape, v.rot);
                    return new Block(current, data);
                }
            }
        }
        return new Block(current);
    }

    static bool IsEquivalent(NearMatrix3<Block> nearBlocks, NearMatrix3<int> type)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    if (i == 0 && j == 0 && k == 0)
                        continue;

                    int value = type.Get(i, j, k);
                    if (value == 2)
                        continue;

                    bool valueBool = value == 1;
                    bool blockSet = nearBlocks.Get(i, j, k).type != BlockType.air;

                    if (valueBool != blockSet)
                        return false;
                }
            }
        }

        return true;
    }

    public static bool IsComplexeBlock(BlockType type)
    {
        return type == BlockType.ground;
    }

    public static Vector3Int BlockFaceToDirection(BlockFace face)
    {
        switch(face)
        {
            case BlockFace.Top:
                return new Vector3Int(0, 1, 0);
            case BlockFace.Bottom:
                return new Vector3Int(0, -1, 0);
            case BlockFace.Front:
                return new Vector3Int(0, 0, 1);
            case BlockFace.Back:
                return new Vector3Int(0, 0, -1);
            case BlockFace.Left:
                return new Vector3Int(-1, 0, 0);
            case BlockFace.Right:
                return new Vector3Int(1, 0, 0);
            default:
                break;
        }

        return Vector3Int.zero;
    }

    public static BlockFace OppositeFace(BlockFace face)
    {
        switch (face)
        {
            case BlockFace.Top:
                return BlockFace.Bottom;
            case BlockFace.Bottom:
                return BlockFace.Top;
            case BlockFace.Left:
                return BlockFace.Right;
            case BlockFace.Right:
                return BlockFace.Left;
            case BlockFace.Front:
                return BlockFace.Back;
            case BlockFace.Back:
                return BlockFace.Front;
        }

        return face;
    }
}