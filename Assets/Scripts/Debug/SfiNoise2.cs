using NRand;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SfiNoise2 : MonoBehaviour
{
    [SerializeField] int m_size = 1024;
    [SerializeField] int m_bridgeWidth = 4;
    [SerializeField] float m_TopOffset = 0.5f;
    [SerializeField] int m_minVariationDistance = 5;
    [SerializeField] int m_maxVariationDistance = 10;
    [SerializeField] uint m_seed = 1234;
    [SerializeField] string filename = "Noise";

    class Point
    {
        public float value = -1;
        public Vector2Int previous = -5 *Vector2Int.one;
        public Vector2Int next = -5 *Vector2Int.one;
    }

    [Button("Generate")]
    void Exec()
    {
        Texture2D tex = new Texture2D(m_size, m_size);
        for (int i = 0; i < m_size; i++)
        {
            for (int j = 0; j < m_size; j++)
                tex.SetPixel(i, j, new Color(1, 0, 0));
        }

        MT19937 rand = new MT19937(m_seed);

        int bridgeCount = m_size / m_bridgeWidth;

        List<Vector2Int> points = new List<Vector2Int>();
        for (int i = 0; i < bridgeCount; i++)
        {
            for (int j = 0; j < bridgeCount; j++)
                points.Add(new Vector2Int(i, j));
        }

        points.Shuffle(rand);

        Matrix<Point> mat = new Matrix<Point>(bridgeCount, bridgeCount);
        for(int i = 0; i < bridgeCount; i++)
        {
            for (int j = 0; j < bridgeCount; j++)
                mat.Set(i, j, new Point());
        }

        foreach (var p in points)
        {
            var elem = mat.Get(p.x, p.y);

            if (elem.previous.x < -2)
                SetFreeRotationFrom(p, true, mat, rand);
            if (elem.next.x < -2)
                SetFreeRotationFrom(p, false, mat, rand);
        }

        for (int i = 0; i < bridgeCount; i++)
        {
            for (int j = 0; j < bridgeCount; j++)
            {
                var elem = mat.Get(i, j);
                if (elem.value >= 0)
                    continue;

                var pos = GetFirstPos(new Vector2Int(i, j), mat);
                int nextVariationDistance = Rand.UniformIntDistribution(m_minVariationDistance, m_maxVariationDistance + 1, rand);
                int currentVariationDistance = 0;
                float startValue = Rand.UniformFloatDistribution(0.5f - m_TopOffset, 0.5f + m_TopOffset, rand);
                float endValue = Rand.UniformFloatDistribution(0.5f - m_TopOffset, 0.5f + m_TopOffset, rand);
                while (true)
                {
                    elem = mat.Get(pos.x, pos.y);

                    float distancePercent = (float)(currentVariationDistance) / nextVariationDistance;
                    float value = endValue * distancePercent + startValue * (1 - distancePercent);
                    elem.value = value;

                    currentVariationDistance++;
                    if (currentVariationDistance >= nextVariationDistance)
                    {
                        currentVariationDistance = 0;
                        nextVariationDistance = Rand.UniformIntDistribution(m_minVariationDistance, m_maxVariationDistance + 1, rand);
                        startValue = endValue;
                        endValue = Rand.UniformFloatDistribution(0.5f - m_TopOffset, 0.5f + m_TopOffset, rand);
                    }

                    if (elem.next.x < -2)
                        break;

                    pos = elem.next;
                    pos.x = GridEx.LoopPos(pos.x, mat.size.x);
                    pos.y = GridEx.LoopPos(pos.y, mat.size.z);
                }
            }
        }

        for (int i = 0; i < bridgeCount; i++)
        {
            for (int j = 0; j < bridgeCount; j++)
            {
                var currentPos = new Vector2Int(i, j);
                Vector2Int minPoint = currentPos * m_bridgeWidth;
                Vector2Int maxPoint = minPoint + new Vector2Int(m_bridgeWidth, m_bridgeWidth);

                var elem = mat.Get(i, j);
                var previousPos = elem.previous;
                var nextPos = elem.next;
                var previousLoopPos = previousPos;
                var nextLoopPos = nextPos;
                previousLoopPos.x = GridEx.LoopPos(previousPos.x, mat.size.x);
                previousLoopPos.y = GridEx.LoopPos(previousPos.y, mat.size.z);
                nextLoopPos.x = GridEx.LoopPos(nextPos.x, mat.size.x);
                nextLoopPos.y = GridEx.LoopPos(nextPos.y, mat.size.z);
                
                if(previousPos.x < -2 && nextPos.x < -2)
                {
                    for(int k = minPoint.x; k < maxPoint.x; k++)
                    {
                        for (int l = minPoint.y; l < maxPoint.y; l++)
                        {
                            int x = GridEx.LoopPos(k, m_size);
                            int y = GridEx.LoopPos(l, m_size);
                            tex.SetPixel(x, y, new Color(elem.value, elem.value, elem.value));
                        }
                    }
                    continue;
                }

                var previousElem = mat.Get(previousLoopPos.x, previousLoopPos.y);
                var nextElem = mat.Get(nextLoopPos.x, nextLoopPos.y);
                float previousValue = previousElem.value;
                float currentValue = elem.value;
                float nextValue = nextElem.value;

                if(previousPos.x < -2)
                {
                    previousValue = elem.value;
                    previousPos = 2 * currentPos - nextPos;
                }
                else if(nextPos.x < -2)
                {
                    nextValue = elem.value;
                    nextPos = 2 * currentPos - previousPos;
                }

                previousValue = (currentValue + previousValue) / 2;
                nextValue = (currentValue + nextValue) / 2;

                Vector2 previousBorder = new Vector2((previousPos.x + currentPos.x + 1) / 2.0f, (previousPos.y + currentPos.y + 1) / 2.0f) * m_bridgeWidth;
                Vector2 nextBorder = new Vector2((nextPos.x + currentPos.x + 1) / 2.0f, (nextPos.y + currentPos.y + 1) / 2.0f) * m_bridgeWidth;

                Vector2Int dirToPrevious = previousPos - currentPos;
                Vector2Int dirToNext = nextPos - currentPos;

                for (int k = minPoint.x; k < maxPoint.x; k++)
                {
                    for (int l = minPoint.y; l < maxPoint.y; l++)
                    {
                        int x = GridEx.LoopPos(k, m_size);
                        int y = GridEx.LoopPos(l, m_size);

                        float distToPrevious = Mathf.Max(Mathf.Abs((k - previousBorder.x) * dirToPrevious.x), Math.Abs((l - previousBorder.y) * dirToPrevious.y));
                        float distToNext = Mathf.Max(Mathf.Abs((k - nextBorder.x) * dirToNext.x), Mathf.Abs((l - nextBorder.y) * dirToNext.y));

                        float percent = 0;
                        if (distToPrevious + distToNext < 0.1f)
                            percent = 0.5f;
                        else if (distToPrevious < distToNext)
                            percent = distToPrevious / (distToPrevious + distToNext);
                        else percent = 1 - (distToNext / (distToPrevious + distToNext));

                        float value = currentValue;
                        if (percent < 0.5f)
                            value = percent * 2 * currentValue + (1 - 2 * percent) * previousValue;
                        else
                        {
                            float p = (1 - percent) * 2;
                            value = p * currentValue + (1 - p) * nextValue;
                        }

                        tex.SetPixel(x, y, new Color(value, value, value));
                    }
                }
            }
        }

        TextureEx.SaveTexture(tex, Application.dataPath + "\\..\\Gen\\" + filename, TextureEx.SaveTextureFileFormat.PNG);
        Debug.Log(Application.dataPath + "\\..\\Gen\\" + filename);
    }

    void SetFreeRotationFrom(Vector2Int pos, bool previous, Matrix<Point> mat, IRandomGenerator gen)
    {
        var elem = mat.Get(pos.x, pos.y);
        if (previous && elem.previous.x > -2)
            return;
        if (!previous && elem.next.x > -2)
            return;

        Rotation baseRot = RotationEx.RandomRotation(gen);
        for(int i = 0; i < 4; i++)
        {
            var rot = RotationEx.Add(baseRot, (Rotation)i);
            var dir = RotationEx.ToVectorInt(rot);
            var nextPos = pos + dir;
            if (elem.next == nextPos || elem.previous == nextPos)
                continue;

            var nextPosLoop = new Vector2Int(GridEx.LoopPos(nextPos.x, mat.size.x), GridEx.LoopPos(nextPos.y, mat.size.z));
            var nextElem = mat.Get(nextPosLoop.x, nextPosLoop.y);
            if (previous && nextElem.next.x >= -2)
                continue;
            if (!previous && nextElem.previous.x >= -2)
                continue;

            var posLoop = pos;
            if(nextPosLoop != nextPos)
                posLoop = nextPosLoop + (pos - nextPos);

            if (nextElem.next == posLoop || nextElem.previous == posLoop)
                continue;

            if (previous)
            {
                elem.previous = nextPos;
                nextElem.next = posLoop;
            }
            else
            {
                elem.next = nextPos;
                nextElem.previous = posLoop;
            }

            break;
        }
    }

    Vector2Int GetFirstPos(Vector2Int pos, Matrix<Point> mat)
    {
        List<Vector2Int> checkedPoints = new List<Vector2Int>();

        var currentPos = pos;
        while(true)
        {
            var elem = mat.Get(currentPos.x, currentPos.y);
            if (elem.previous.x < -2)
                return currentPos;
            currentPos = elem.previous;
            currentPos.x = GridEx.LoopPos(currentPos.x, mat.size.x);
            currentPos.y = GridEx.LoopPos(currentPos.y, mat.size.z);
            if (currentPos == pos)
            {
                var previousElem = mat.Get(currentPos.x, currentPos.y);
                previousElem.next = -5*Vector2Int.one;
                elem.previous = -5*Vector2Int.one;
                return currentPos;
            }
            checkedPoints.Add(currentPos);
        }
    }
}
