using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.EventSystems;

public enum AliveType
{
    Alive,
    Dead,
    NoLive,
    NotSet,
}

public static class Utility
{
    public static float Angle(Vector2 a, Vector2 b)
    {
        float angle = Mathf.Atan2(b.y, b.x) - Mathf.Atan2(a.y, a.x);

        if (angle < -Mathf.PI)
            angle += 2 * Mathf.PI;
        if (angle > Mathf.PI)
            angle -= 2 * Mathf.PI;

        return angle;
    }

    public static float Angle(Vector2 vect)
    {
        return Mathf.Atan2(vect.y, vect.x);
    }

    public static Vector2 Project(Vector2 vect, Vector2 dir)
    {
        float a = Angle(vect, dir);

        return dir.normalized * Mathf.Cos(a) * vect.magnitude;
    }

    static float DistanceToPoint(Vector3 pos, Vector3 point)
    {
        return (point - pos).magnitude;
    }

    static float DistanceToPoint(Vector2 pos, Vector2 point)
    {
        return (point - pos).magnitude;
    }

    public static float DistanceToSegment(Vector2 line1, Vector2 line2, Vector2 point)
    {
        Vector2 p = Project(line1, line2, point);

        float d = (line1 - line2).sqrMagnitude;
        float d1 = (p - line1).sqrMagnitude;
        float d2 = (p - line2).sqrMagnitude;

        if (d1 > d)
            p = line2;
        else if (d2 > d)
            p = line1;

        return (p - point).magnitude;
    }

    public static Vector2 Project(Vector2 line1, Vector2 line2, Vector2 point)
    {
        Vector2 dir1 = line2 - line1;
        Vector2 dir2 = point - line1;

        return line1 + Vector2.Dot(dir2, dir1) / Vector2.Dot(dir1, dir1) * dir1;
    }

    public static Vector3 Project(Vector3 line1, Vector3 line2, Vector3 point)
    {
        Vector3 dir1 = line2 - line1;
        Vector3 dir2 = point - line1;

        return line1 + Vector3.Dot(dir2, dir1) / Vector3.Dot(dir1, dir1) * dir1;
    }

    public static bool IsLeft(Vector2 line1, Vector2 line2, Vector2 pos)
    {
        return ((line2.x - line1.x) * (pos.y - line1.y) - (line2.y - line1.y) * (pos.x - line1.x)) >= 0;
    }

    public static bool IsRight(Vector2 line1, Vector2 line2, Vector2 pos)
    {
        return ((line2.x - line1.x) * (pos.y - line1.y) - (line2.y - line1.y) * (pos.x - line1.x)) <= 0;
    }

    public static bool IsOnSameLine(Vector2 p1, Vector2 p2, Vector2 p3, float epsilon = 0.0001f)
    {
        float d = Mathf.Abs(Vector2.Dot((p1 - p2), (p1 - p3)));
        if (Mathf.Abs(1 - d) < epsilon)
            return true;
        return false;
    }

    public static T DeepClone<T>(this T obj)
    {
        using (MemoryStream memory_stream = new MemoryStream())
        {
            // Serialize the object into the memory stream.
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Serialize(memory_stream, obj);

            // Rewind the stream and use it to create a new object.
            memory_stream.Position = 0;
            return (T)formatter.Deserialize(memory_stream);
        }
    }
    public static bool MouseOverUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    public static bool Intersects(this BoundsInt a, BoundsInt b)
    {
        return (a.xMin < b.xMax) && (a.xMax > b.xMin) &&
            (a.yMin < b.yMax) && (a.yMax > b.yMin) &&
            (a.zMin < b.zMax) && (a.zMax > b.zMin);
    }

    public static float ReduceAngle(float current)
    {
        while (current > 180.0f)
            current -= 360.0f;
        while (current < -180.0f)
            current += 180.0f;
        return current;
    }

    public static ulong PosToID(Vector3Int pos)
    {
        const int powLimit = 20;
        const int absLimit = 1 << powLimit;

        for (int i = 0; i < 3; i++)
        {
            if (Mathf.Abs(pos[i]) > absLimit - 1)
                pos[i] = (absLimit - 1) * ((pos[i] > 0) ? 1 : -1);
            pos[i] += absLimit - 1;
        }

        ulong ID = (ulong)pos.x;
        ID <<= powLimit;
        ID += (ulong)pos.y;
        ID <<= powLimit;
        ID += (ulong)pos.z;

        return ID;
    }

    public static ulong PosToID(Vector2Int pos)
    {
        const int powLimit = 30;
        const int absLimit = 1 << powLimit;

        for (int i = 0; i < 2; i++)
        {
            if (Mathf.Abs(pos[i]) > absLimit - 1)
                pos[i] = (absLimit - 1) * ((pos[i] > 0) ? 1 : -1);
            pos[i] += absLimit - 1;
        }

        ulong ID = (ulong)pos.x;
        ID <<= powLimit;
        ID += (ulong)pos.y;

        return ID;
    }

    public static Vector2 IntersectLines(Vector2 A, Vector2 B, Vector2 O, Vector2 P)
    {
        Vector2 AB = B - A;
        Vector2 OP = P - O;

        float det = AB.x * OP.y - AB.y * OP.x;
        if(Mathf.Abs(det) < Mathf.Epsilon)
            return new Vector2(float.MinValue, float.MinValue);

        float k = -(A.x * OP.y - O.x * OP.y - OP.x * A.y + OP.x * O.y) / det;
        float l = -(-AB.x * A.y + AB.x * O.y + AB.y * A.x - AB.y * O.x) / det;

        return l * OP + O;
    }

    public static string FormateTime(float time, bool forceDisplayMin = false)
    {
        int secs = Mathf.RoundToInt(time);
        int mins = Mathf.FloorToInt(secs / 60);
        secs -= mins * 60;
        int hours = Mathf.FloorToInt(mins / 60);
        mins -= hours * 60;

        string str = "";
        if (hours > 0)
            str += hours + ":";
        if(hours > 0 || mins > 0 || forceDisplayMin)
        {
            if (mins <= 9 && (hours > 0 || mins > 0))
                str += '0';
            str += mins + ":";
        }
        if ((hours > 0 || mins > 0 || forceDisplayMin) && secs <= 9)
            str += '0';
        str += secs;

        return str;
    }

    public static bool IsFrozen(GameObject obj)
    {
        var frozen = new IsFrozenEvent();
        Event<IsFrozenEvent>.Broadcast(frozen, obj);

        return frozen.frozen;
    }

    public static bool IsDead(GameObject obj)
    {
        IsDeadEvent dead = new IsDeadEvent();
        Event<IsDeadEvent>.Broadcast(dead, obj);

        return dead.isDead;
    }

    static readonly ProfilerMarker ms_aliveProfilerMarker = new ProfilerMarker(ProfilerCategory.Scripts, "Utility.IsAliveFilter");

    public static bool IsAliveFilter(GameObject obj, AliveType aliveFilter)
    {
        if (aliveFilter == AliveType.NotSet)
            return true;

        GetLifeEvent life = new GetLifeEvent();
        Event<GetLifeEvent>.Broadcast(life, obj);

        if (!life.haveLife)
            return aliveFilter == AliveType.NoLive;
        else if (aliveFilter == AliveType.NoLive)
            return false;

        float lifePercent = life.lifePercent;

        if (lifePercent > 0 && aliveFilter == AliveType.Alive)
            return true;

        if (lifePercent <= 0 && aliveFilter == AliveType.Dead)
            return true;

        return false;
    }
}
