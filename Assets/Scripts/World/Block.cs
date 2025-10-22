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

    public string SaveToString()
    {
        string str = "";

        switch(type)
        {
            case BlockType.air:
                str = "a";
                break;
            case BlockType.ground:
                str = "g";
                break;
            case BlockType.water:
                str = "w";
                break;
            case BlockType.crystal:
                str = "c";
                break;
            case BlockType.oil:
                str = "o";
                break;
            case BlockType.Titanium:
                str = "t";
                break;
            default:
                throw new Exception("not supported block type");
        }

        if (data != 0)
            str += data.ToString();

        return str;
    }

    public void LoadFromString(string value)
    {
        if (value[0] == 'a')
            type = BlockType.air;
        else if (value[0] == 'g')
            type = BlockType.ground;
        else if (value[0] == 'w')
            type = BlockType.water;
        else if (value[0] == 'c')
            type = BlockType.crystal;
        else if (value[0] == 'o')
            type = BlockType.oil;
        else if (value[0] == 't')
            type = BlockType.Titanium;
        else type = BlockType.air;

        int dataDecode;
        if (int.TryParse(value.Substring(1), out dataDecode))
            data = (byte)dataDecode;
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