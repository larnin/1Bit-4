using Noise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class WorldGenerator
{
    public enum GenerationState
    {
        Idle,
        Working,
        Failed,
        Finished,
    }

    static readonly object m_stateLock = new object();
    static GenerationState m_state = GenerationState.Idle;

    static string m_stateTxt = "";

    static WorldGeneratorSettings m_settings = null;
    static Grid m_grid = null;


    public static void Generate(WorldGeneratorSettings settings)
    {
        lock (m_stateLock)
        {
            if (m_state == GenerationState.Working)
                return;

            Reset();

            m_state = GenerationState.Working;
            m_stateTxt = "Starting";

            m_settings = settings;
        }

        ThreadPool.StartJob(JobWorker, OnEndJob, 1000);
    }

    static void Reset()
    {
        m_state = GenerationState.Idle;
        m_stateTxt = "";

        m_settings = null;
        m_grid = null;
    }

    public static Grid GetGrid()
    {
        return m_grid;
    }

    public static GenerationState GetState()
    {
        return m_state;
    }

    public static string GetStateText()
    {
        return m_stateTxt;
    }

    static void OnEndJob()
    {
        if (m_state != GenerationState.Failed)
        {
            m_state = GenerationState.Finished;
            m_stateTxt = "Done";
        }
    }

    static void JobWorker()
    {
        m_grid = new Grid(m_settings.size, m_settings.height);

        m_stateTxt = "Generate Heights";
        var waterAreasHeight = GenerateMontains();
        m_stateTxt = "Generate Ground";
        SimpleApplyHeight(waterAreasHeight);
    }

    static Matrix<float> GenerateBaseSurface()
    {
        int size = GridEx.GetRealSize(m_grid);
        var mat = new Matrix<float>(size, size);

        Vector2 center = new Vector2(size / 2.0f, size / 2.0f);
        float maxDist = size / 2.0f;

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var pos = new Vector2(i, j) - center;

                float height = m_settings.maxBaseHeight;
                float dist = pos.magnitude / maxDist;
                if(dist > m_settings.plateformSurfaceScale)
                {
                    float norm = (dist - m_settings.plateformSurfaceScale) / (1 - m_settings.plateformSurfaceScale);
                    height = (1 - norm) * m_settings.maxBaseHeight + norm * m_settings.minBaseHeight;
                }

                mat.Set(i, j, height);
            }
        }

        return mat;
    }

    static Matrix<float> GenerateMontains()
    {
        int size = GridEx.GetRealSize(m_grid);
        var mat = new Matrix<float>(size, size);

        Vector2 center = new Vector2(size / 2.0f, size / 2.0f);
        float maxDist = size / 2.0f;

        Perlin[] placement = new Perlin[m_settings.MontainsPlacement.nbLayers];
        for (int i = 0; i < m_settings.MontainsPlacement.nbLayers; i++)
        {
            placement[i] = new Perlin(size, m_settings.MontainsPlacement.baseAmplitude * MathF.Pow(m_settings.MontainsPlacement.layerAmplitudeScale, i)
                , (int)(m_settings.MontainsPlacement.baseFrequency / MathF.Pow(2, i)), m_settings.seed + i);
        }

        Turbulence[] montains = new Turbulence[m_settings.MontainsHeight.nbLayers];
        for (int i = 0; i < m_settings.MontainsHeight.nbLayers; i++)
        {
            montains[i] = new Turbulence(size, m_settings.MontainsHeight.baseAmplitude * MathF.Pow(m_settings.MontainsHeight.layerAmplitudeScale, i)
                , (int)(m_settings.MontainsHeight.baseFrequency / MathF.Pow(2, i)), m_settings.seed + i);
        }

        float maxHeight = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float height = 0;
                foreach (var p in placement)
                    height += p.Get(i, j);
                if (height < m_settings.montainsMinPlacementHeight)
                    height = 0;
                else if (height > m_settings.montainsMaxPlacementHeight)
                    height = 1;
                else height = (height - m_settings.montainsMinPlacementHeight) / (m_settings.montainsMaxPlacementHeight - m_settings.montainsMinPlacementHeight);
                height *= GetMontainPlacement(i, j);

                float montainHeight = 0;
                if(height > 0)
                {
                    foreach (var m in montains)
                        montainHeight += m.Get(i, j);
                    montainHeight += m_settings.montainsHeightOffset;
                    if (montainHeight < 0)
                        montainHeight = 0;
                    height *= montainHeight;
                }

                if (height > maxHeight)
                    maxHeight = height;
                mat.Set(i, j, height);
            }
        }
        
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float currentHeight = mat.Get(i, j);
                currentHeight /= maxHeight;
                currentHeight = Mathf.Pow(currentHeight, m_settings.montainsHeightPower) * m_settings.montainsHeightMultiplier;
                mat.Set(i, j, currentHeight);
            }
        }

        return mat;
    }

    static float GetMontainPlacement(int x, int y)
    {
        int size = GridEx.GetRealSize(m_grid);

        Vector2 center = new Vector2(size / 2.0f, size / 2.0f);
        float maxDist = size / 2.0f;

        var pos = new Vector2(x, y) - center;
        
        float dist = pos.magnitude / maxDist;
        if (dist > m_settings.montainsMinPlacementDist && dist < m_settings.montainsMinPlacementDist + m_settings.montainsPlacementBlendDist)
            return (dist - m_settings.montainsMinPlacementDist) / m_settings.montainsPlacementBlendDist;
        if (dist > m_settings.montainsMaxPacementDist - m_settings.montainsPlacementBlendDist && dist < m_settings.montainsMaxPacementDist)
            return 1 - (dist - (m_settings.montainsMaxPacementDist - m_settings.montainsPlacementBlendDist)) / m_settings.montainsPlacementBlendDist;
        if (dist >= m_settings.montainsMinPlacementDist + m_settings.montainsPlacementBlendDist && dist <= m_settings.montainsMaxPacementDist - m_settings.montainsPlacementBlendDist)
            return 1;

        return 0;
    }

    static Matrix<float> GenerateWaterAreas()
    {
        int size = GridEx.GetRealSize(m_grid);

        var mat = new Matrix<float>(size, size);

        Turbulence[] perlins = new Turbulence[m_settings.waterZones.nbLayers];
        for(int i = 0; i < m_settings.waterZones.nbLayers; i++)
        {
            perlins[i] = new Turbulence(size, m_settings.waterZones.baseAmplitude * MathF.Pow(m_settings.waterZones.layerAmplitudeScale, i)
                , (int)(m_settings.waterZones.baseFrequency / MathF.Pow(2, i)), m_settings.seed + i);
        }

        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                float height = 0;
                foreach (var p in perlins)
                    height += p.Get(i, j);
                mat.Set(i, j, height);
            }
        }

        return mat;
    }

    static void SimpleApplyHeight(Matrix<float> heights)
    {
        int size = GridEx.GetRealSize(m_grid);
        int height = GridEx.GetRealHeight(m_grid);

        int midHeight = height / 2;

        for(int i = 0; i < size; i++)
        {
            for(int k = 0; k < size; k++)
            {
                int localHeight = (int)heights.Get(i, k) + midHeight;

                if (localHeight < 0)
                    localHeight = 0;
                if (localHeight >= height)
                    localHeight = height - 1;

                for(int j = localHeight; j >= 0; j--)
                    GridEx.SetBlock(m_grid, new Vector3Int(i, j, k), BlockType.ground);
            }
        }
    }
}

