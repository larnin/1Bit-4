using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

public static class BlockEx
{
    public static byte MakeData(BlockShape shape, Rotation rot)
    {
        return (byte)(((int)shape) << 2 + (int)rot);
    }

    public static BlockShape GetShapeFromData(byte data)
    {
        int shape = data >> 2;
        return (BlockShape)shape;
    }

    public static Rotation GetRotationFromData(byte data)
    {
        int rot = data | 0b11;
        return (Rotation)rot;
    }
}