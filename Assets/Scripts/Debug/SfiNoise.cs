using NRand;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SfiNoise : MonoBehaviour
{
    enum BlendMode
    {
        Override,
        Max,
        Add,
    }

    [SerializeField] int m_size = 1024;
    [SerializeField] int m_bridgeCount = 10;
    [SerializeField] int m_bridgeMinLength = 10;
    [SerializeField] int m_bridgeMaxLength = 20;
    [SerializeField] int m_bridgeWidth = 4;
    [SerializeField] float m_bridgeTurnChance = 10;
    [SerializeField] float m_bridgeMinHeight = 0.2f;
    [SerializeField] float m_bridgeMaxHeight = 0.3f;
    [SerializeField] float m_bridgeTopHeightLenght = 0.5f;
    [SerializeField] BlendMode m_blendMode = BlendMode.Max;
    [SerializeField] uint m_seed = 1234;
    [SerializeField] string filename = "Noise";

    [Button("Generate")]
    void Exec()
    {
        Texture2D tex = new Texture2D(m_size, m_size);
        for(int i = 0; i < m_size; i++)
        {
            for (int j = 0; j < m_size; j++)
                tex.SetPixel(i, j, new Color(0.5f, 0.5f, 0.5f));
        }

        MT19937 rand = new MT19937(m_seed);

        int gridSize = m_size / m_bridgeWidth;

        for (int i = 0; i < m_bridgeCount; i++)
        {
            int length = Rand.UniformIntDistribution(m_bridgeMinLength, m_bridgeMaxLength + 1, rand);
            if (length <= 1)
                continue;

            var points = new List<Vector2Int>();
            var start = Rand2D.UniformVector2SquareDistribution(rand);
            Rotation rot = RotationEx.RandomRotation(rand);
            points.Add(new Vector2Int((int)(start.x * gridSize), (int)(start.y * gridSize)));
            for (int j = 0; j < length - 1; j++)
            {
                var dir = RotationEx.ToVectorInt(rot);
                var current = points[points.Count - 1];
                points.Add(current + dir);

                if(Rand.BernoulliDistribution(m_bridgeTurnChance, rand))
                {
                    if (Rand.BernoulliDistribution(rand))
                        rot = RotationEx.Add(rot, Rotation.rot_90);
                    else rot = RotationEx.Sub(rot, Rotation.rot_90);
                }
            }

            float topHeight = Rand.UniformFloatDistribution(m_bridgeMinHeight, m_bridgeMaxHeight, rand);
            if (Rand.BernoulliDistribution(rand))
                topHeight *= -1;
            topHeight += 0.5f;
            topHeight = Mathf.Clamp01(topHeight);

            for(int j = 0; j < points.Count; j++)
            {
                Vector2Int currentPos = points[j];
                int previous = j - 1;
                int next = j + 1;
                Vector2Int previousPos = Vector2Int.zero;
                if (previous >= 0)
                    previousPos = points[previous];
                else previousPos = 2 * currentPos - points[next];
                Vector2Int nextPos = Vector2Int.zero;
                if (next < points.Count)
                    nextPos = points[next];
                else nextPos = 2 * currentPos - points[previous];

                float leftPercent = (j - 0.5f) / points.Count;
                float rightPercent = (j + 0.5f) / points.Count;

                Vector2 previousBorder = new Vector2((previousPos.x + currentPos.x + 1) / 2.0f, (previousPos.y + currentPos.y + 1) / 2.0f) * m_bridgeWidth;
                Vector2 nextBorder = new Vector2((nextPos.x + currentPos.x + 1) / 2.0f, (nextPos.y + currentPos.y + 1) / 2.0f) * m_bridgeWidth;

                Vector2Int minPoint = currentPos * m_bridgeWidth;
                Vector2Int maxPoint = minPoint + new Vector2Int(m_bridgeWidth, m_bridgeWidth);

                Vector2Int dirToPrevious = previousPos - currentPos;
                Vector2Int dirToNext = nextPos - currentPos;

                for(int k = minPoint.x; k < maxPoint.x; k++)
                {
                    for(int l = minPoint.y; l < maxPoint.y; l++)
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

                        percent = rightPercent * percent + leftPercent * (1 - percent);

                        float color = PercentToColor(percent, topHeight);
                        float current = tex.GetPixel(x, y).r;

                        color = blendColor(current, color, m_blendMode);

                        tex.SetPixel(x, y, new Color(color, color, color));
                    }
                }
            }

            
        }
        
        TextureEx.SaveTexture(tex, Application.dataPath + "\\..\\Gen\\" + filename, TextureEx.SaveTextureFileFormat.PNG);
        Debug.Log(Application.dataPath + "\\..\\Gen\\" + filename);
    }

    float PercentToColor(float percent, float topColor)
    {
        percent = Mathf.Clamp01(percent);

        if(percent < (1 - m_bridgeTopHeightLenght) / 2)
        {
            float part = percent / ((1 - m_bridgeTopHeightLenght) / 2);
            return part * topColor + (1 - part) * 0.5f;
        }
        else if(percent > 0.5f + m_bridgeTopHeightLenght / 2)
        {
            float part = percent - (0.5f + m_bridgeTopHeightLenght / 2);
            part /= (1 - m_bridgeTopHeightLenght) / 2;
            return part * 0.5f + (1 - part) * topColor;
        }
        else return topColor;
    }

    float blendColor(float currentColor, float newColor, BlendMode mode)
    {
        switch(mode)
        {
            case BlendMode.Add:
                return currentColor + newColor - 0.5f;
            case BlendMode.Max:
                if (currentColor < 0.501f)
                    return Mathf.Min(currentColor, newColor);
                if (currentColor > 0.501f)
                    return Mathf.Max(currentColor, newColor);
                return newColor;
            case BlendMode.Override:
                return newColor;
            default:
                return 0;
        }
    }
}
