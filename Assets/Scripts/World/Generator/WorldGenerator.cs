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
        var waterAreasHeight = GenerateWaterAreas();
        m_stateTxt = "Generate Ground";
        SimpleApplyHeight(waterAreasHeight);
    }

    static Matrix<float> GenerateWaterAreas()
    {
        int size = GridEx.GetRealSize(m_grid);

        var mat = new Matrix<float>(size, size);

        Perlin[] perlins = new Perlin[m_settings.waterZones.nbLayers];
        for(int i = 0; i < m_settings.waterZones.nbLayers; i++)
        {
            perlins[i] = new Perlin(size, m_settings.waterZones.baseAmplitude * MathF.Pow(m_settings.waterZones.layerAmplitudeScale, i)
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

