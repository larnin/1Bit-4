using Noise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

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
        var areasHeight = GenerateMontains();
        m_stateTxt = "Generate Ground";
        SimpleApplyHeight(areasHeight);

        m_stateTxt = "Generate Resources";
        GenerateCrystal();
        GenerateOil();
        GenerateTitanium();
    }

    static Matrix<float> GenerateMontains()
    {
        int size = GridEx.GetRealSize(m_grid);
        var mat = new Matrix<float>(size, size);

        Vector2 center = new Vector2(size / 2.0f, size / 2.0f);
        float maxDist = size / 2.0f;

        int seedIndex = 0;

        Perlin[] placement = new Perlin[m_settings.MontainsPlacement.nbLayers];
        for (int i = 0; i < m_settings.MontainsPlacement.nbLayers; i++)
        {
            placement[i] = new Perlin(size, m_settings.MontainsPlacement.baseAmplitude * MathF.Pow(m_settings.MontainsPlacement.layerAmplitudeScale, i)
                , (int)(m_settings.MontainsPlacement.baseFrequency / MathF.Pow(2, i)), m_settings.seed + seedIndex);
            seedIndex++;
        }

        Turbulence[] montains = new Turbulence[m_settings.MontainsHeight.nbLayers];
        for (int i = 0; i < m_settings.MontainsHeight.nbLayers; i++)
        {
            montains[i] = new Turbulence(size, m_settings.MontainsHeight.baseAmplitude * MathF.Pow(m_settings.MontainsHeight.layerAmplitudeScale, i)
                , (int)(m_settings.MontainsHeight.baseFrequency / MathF.Pow(2, i)), m_settings.seed + seedIndex);
            seedIndex++;
        }

        Perlin[] plains = new Perlin[m_settings.Plains.nbLayers];
        for (int i = 0; i < m_settings.Plains.nbLayers; i++)
        {
            plains[i] = new Perlin(size, m_settings.Plains.baseAmplitude * MathF.Pow(m_settings.Plains.layerAmplitudeScale, i)
                , (int)(m_settings.Plains.baseFrequency / MathF.Pow(2, i)), m_settings.seed + seedIndex);
            seedIndex++;
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

                foreach (var p in plains)
                    currentHeight += p.Get(i, j);

                currentHeight += GenerateBaseSurface(i, j);

                mat.Set(i, j, currentHeight);
            }
        }

        return mat;
    }

    static float GenerateBaseSurface(int x, int y)
    {
        int size = GridEx.GetRealSize(m_grid);

        Vector2 center = new Vector2(size / 2.0f, size / 2.0f);
        float maxDist = size / 2.0f;

        var pos = new Vector2(x, y) - center;

        float height = m_settings.maxBaseHeight;
        float dist = pos.magnitude / maxDist;
        if (dist > m_settings.plateformSurfaceScale)
        {
            float norm = (dist - m_settings.plateformSurfaceScale) / (1 - m_settings.plateformSurfaceScale);
            return (1 - norm) * m_settings.maxBaseHeight + norm * m_settings.minBaseHeight;
        }

        return m_settings.maxBaseHeight;
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

    static void SimpleApplyHeight(Matrix<float> heights)
    {
        int size = GridEx.GetRealSize(m_grid);
        int height = GridEx.GetRealHeight(m_grid);

        int midHeight = height / 2;

        for(int i = 0; i < size; i++)
        {
            for(int k = 0; k < size; k++)
            {
                float testHeight = heights.Get(i, k);
                BlockType type = BlockType.ground;
                if (testHeight < m_settings.waterHeight)
                {
                    testHeight = m_settings.waterHeight;
                    type = BlockType.water;
                }

                int localHeight = (int)testHeight + midHeight;

                if (localHeight < 0)
                    localHeight = 0;
                if (localHeight >= height)
                    localHeight = height - 1;

                for(int j = localHeight; j >= 0; j--)
                    GridEx.SetBlock(m_grid, new Vector3Int(i, j, k), type);
            }
        }
    }

    static void GenerateCrystal()
    {
        int size = GridEx.GetRealSize(m_grid);

        MT19937 rand = new MT19937((uint)m_settings.seed + 1);

        //place initial patch
        var initialPoint = Rand2D.UniformVector2CircleSurfaceDistribution(rand);
        initialPoint *= m_settings.crystalInitialPatchDistance;
        initialPoint += new Vector2(size / 2, size / 2);
        PlaceCrystalAt(new Vector2Int(Mathf.RoundToInt(initialPoint.x), Mathf.RoundToInt(initialPoint.y)));

        // rain drom patchs
        float densitySize = (float)size / m_settings.crystalPatchDensity;
        for (int i = 0; i < m_settings.crystalPatchDensity; i++)
        {
            for(int j = 0; j < m_settings.crystalPatchDensity; j++)
            {
                int minX = Mathf.FloorToInt(i * densitySize);
                int maxX = Mathf.CeilToInt((i + 1) * densitySize);
                int minY = Mathf.FloorToInt(j * densitySize);
                int maxY = Mathf.CeilToInt((j + 1) * densitySize);

                for (int k = 0; k < m_settings.crystalRetryCount + 1; k++)
                {
                    int x = Rand.UniformIntDistribution(minX, maxX, rand);
                    int y = Rand.UniformIntDistribution(minY, maxY, rand);

                    float d = new Vector2Int(x - size / 2, y - size / 2).sqrMagnitude;
                    if (d < m_settings.crystalMinDistance * m_settings.crystalMinDistance)
                        break;

                    if (!CanPlaceCrystalAt(new Vector2Int(x, y)))
                        continue;

                    PlaceCrystalAt(new Vector2Int(x, y));
                    break;
                }
            }
        }
    }

    static bool CanPlaceCrystalAt(Vector2Int pos)
    {
        int height = GridEx.GetHeight(m_grid, pos);
        if (height < 0)
            return false;

        float radius = 2.5f;
        for (int i = Mathf.FloorToInt(-radius); i <= Mathf.CeilToInt(radius); i++)
        {
            for (int j = Mathf.FloorToInt(-radius); j <= Mathf.CeilToInt(radius); j++)
            {
                Vector2Int localPos = new Vector2Int(pos.x + i, pos.y + j);
                float dist = (localPos - pos).sqrMagnitude;
                if (dist > radius)
                    continue;

                int localHeight = GridEx.GetHeight(m_grid, localPos);
                int deltaHeight = Mathf.Abs(height - localHeight);
                if (deltaHeight > m_settings.crystalElevationMaxVariation)
                    return false;

                var item = GridEx.GetBlock(m_grid, new Vector3Int(localPos.x, localHeight, localPos.y));
                if (item != BlockType.ground)
                    return false;
            }
        }

        return true;
    }

    static void PlaceCrystalAt(Vector2Int pos)
    {
        int height = GridEx.GetHeight(m_grid, pos);
        if (height < 0)
            return;
        int size = GridEx.GetRealSize(m_grid);

        height++;

        Vector3Int initialPos = new Vector3Int(pos.x, height, pos.y);
        GridEx.SetBlock(m_grid, initialPos, BlockType.crystal);

        List<Vector3Int> openList = new List<Vector3Int>();
        openList.Add(initialPos);

        int addedCount = 1;

        float dist = (pos - new Vector2Int(size / 2, size / 2)).magnitude / size * 2;
        int maxCount = (int)(m_settings.crystalPatchSizeBase + dist * m_settings.crystalPatchIncrease);

        float radius = Mathf.Sqrt(maxCount) / 2 + 1;

        var rand = new MT19937((uint)(m_settings.seed + pos.x + pos.y));

        while(openList.Count > 0 && addedCount < maxCount)
        {
            int currentIndex = 0;
            if(openList.Count > 1)
                currentIndex = Rand.UniformIntDistribution(0, openList.Count, rand);

            var points = GetValidCrystalPosAround(openList[currentIndex], initialPos, radius);
            if (points.Count == 0)
            {
                openList.RemoveAt(currentIndex);
                continue;
            }

            int pointIndex = 0;
            if (points.Count > 1)
                pointIndex = Rand.UniformIntDistribution(0, points.Count, rand);

            GridEx.SetBlock(m_grid, points[pointIndex], BlockType.crystal);
            openList.Add(points[pointIndex]);
            addedCount++;

            if (points.Count == 1)
                openList.RemoveAt(currentIndex);
        }
    }

    static List<Vector3Int> GetValidCrystalPosAround(Vector3Int pos, Vector3Int initialPos, float maxDist)
    {
        List<Vector3Int> points = new List<Vector3Int>();

        for(int i = 0; i < 4; i++)
        {
            var dir = RotationEx.ToVector3Int((Rotation)i);

            Vector3Int testPos = pos + dir;

            var item = GridEx.GetBlock(m_grid, testPos);
            if(item == BlockType.ground)
            {
                testPos.y++;
                item = GridEx.GetBlock(m_grid, testPos);
            }

            if (item != BlockType.air)
                continue;

            testPos.y--;
            item = GridEx.GetBlock(m_grid, testPos);
            if(item == BlockType.air)
            {
                testPos.y--;
                item = GridEx.GetBlock(m_grid, testPos);
            }

            if (item != BlockType.ground)
                continue;

            float dist = (testPos - initialPos).sqrMagnitude;
            if (dist > maxDist * maxDist)
                continue;

            testPos.y++;
            points.Add(testPos);
        }

        return points;
    }

    static void GenerateOil()
    {
        int size = GridEx.GetRealSize(m_grid);

        MT19937 rand = new MT19937((uint)m_settings.seed + 2);

        // rain drom patchs
        float densitySize = (float)size / m_settings.oilDensity;
        for (int i = 0; i < m_settings.oilDensity; i++)
        {
            for (int j = 0; j < m_settings.oilDensity; j++)
            {
                int minX = Mathf.FloorToInt(i * densitySize);
                int maxX = Mathf.CeilToInt((i + 1) * densitySize);
                int minY = Mathf.FloorToInt(j * densitySize);
                int maxY = Mathf.CeilToInt((j + 1) * densitySize);

                for (int k = 0; k < m_settings.oilRetryCount + 1; k++)
                {
                    int x = Rand.UniformIntDistribution(minX, maxX, rand);
                    int y = Rand.UniformIntDistribution(minY, maxY, rand);

                    float d = new Vector2Int(x - size / 2, y - size / 2).sqrMagnitude;
                    if (d < m_settings.oilMinDistance * m_settings.oilMinDistance)
                        break;

                    if (!CanPlaceOilAt(new Vector2Int(x, y)))
                        continue;

                    int height = GridEx.GetHeight(m_grid, new Vector2Int(i, j));
                    GridEx.SetBlock(m_grid, new Vector3Int(i, height, j), BlockType.oil);
                    break;
                }
            }
        }
    }

    static bool CanPlaceOilAt(Vector2Int pos)
    {
        int height = GridEx.GetHeight(m_grid, pos);
        if (height < 0)
            return false;

        for(int i = -1; i <= 1; i ++)
        {
            for(int j = -1; j <= 1; j++)
            {
                Vector2Int localPos = new Vector2Int(pos.x + i, pos.y + j);
                int localHeight = GridEx.GetHeight(m_grid, localPos);
                if (localHeight != height)
                    return false;

                var item = GridEx.GetBlock(m_grid, new Vector3Int(localPos.x, localHeight, localPos.y));
                if (item != BlockType.ground)
                    return false;
            }
        }

        return true;
    }

    static void GenerateTitanium()
    {
        int size = GridEx.GetRealSize(m_grid);

        MT19937 rand = new MT19937((uint)m_settings.seed + 3);

        // rain drom patchs
        float densitySize = (float)size / m_settings.titaniumPatchDensity;
        for (int i = 0; i < m_settings.titaniumPatchDensity; i++)
        {
            for (int j = 0; j < m_settings.titaniumPatchDensity; j++)
            {
                int minX = Mathf.FloorToInt(i * densitySize);
                int maxX = Mathf.CeilToInt((i + 1) * densitySize);
                int minY = Mathf.FloorToInt(j * densitySize);
                int maxY = Mathf.CeilToInt((j + 1) * densitySize);

                for (int k = 0; k < m_settings.titaniumRetryCount + 1; k++)
                {
                    int x = Rand.UniformIntDistribution(minX, maxX, rand);
                    int y = Rand.UniformIntDistribution(minY, maxY, rand);

                    float d = new Vector2Int(x - size / 2, y - size / 2).sqrMagnitude;
                    if (d < m_settings.titaniumMinDistance * m_settings.titaniumMinDistance)
                        break;

                    if (!CanPlaceTitaniumAt(new Vector2Int(x, y)))
                        continue;

                    PlaceTitaniumAt(new Vector2Int(x, y));
                    break;
                }
            }
        }
    }

    static bool CanPlaceTitaniumAt(Vector2Int pos)
    {
        int height = GridEx.GetHeight(m_grid, pos);
        if (height < 0)
            return false;

        float radius = 2.5f;
        int minHeight = -1;
        int maxHeight = -1;

        for (int i = Mathf.FloorToInt(-radius); i <= Mathf.CeilToInt(radius); i++)
        {
            for (int j = Mathf.FloorToInt(-radius); j <= Mathf.CeilToInt(radius); j++)
            {
                Vector2Int localPos = new Vector2Int(pos.x + i, pos.y + j);
                float dist = (localPos - pos).sqrMagnitude;
                if (dist > radius)
                    continue;

                int localHeight = GridEx.GetHeight(m_grid, localPos);
                var item = GridEx.GetBlock(m_grid, new Vector3Int(localPos.x, localHeight, localPos.y));
                if (item != BlockType.ground)
                    return false;

                if (minHeight < 0 || minHeight > localHeight)
                    minHeight = localHeight;
                if (maxHeight < 0 || maxHeight < localHeight)
                    maxHeight = localHeight;
            }
        }

        return maxHeight - minHeight > m_settings.titaniumElevationMinVariation;
    }

    static void PlaceTitaniumAt(Vector2Int pos)
    {
        int height = GridEx.GetHeight(m_grid, pos);
        if (height < 0)
            return;
        int size = GridEx.GetRealSize(m_grid);

        height++;

        Vector3Int initialPos = new Vector3Int(pos.x, height, pos.y);
        GridEx.SetBlock(m_grid, initialPos, BlockType.Titanium);

        List<Vector3Int> openList = new List<Vector3Int>();
        openList.Add(initialPos);

        int addedCount = 1;

        var rand = new MT19937((uint)(m_settings.seed + 1 + pos.x + pos.y));

        int maxCount = Rand.UniformIntDistribution(m_settings.titaniumPatchMin, m_settings.titaniumPatchMax, rand);
        float radius = Mathf.Sqrt(maxCount) / 2 + 1;

        while (openList.Count > 0 && addedCount < maxCount)
        {
            int currentIndex = 0;
            if (openList.Count > 1)
                currentIndex = Rand.UniformIntDistribution(0, openList.Count, rand);

            var points = GetValidTitaniumPosAround(openList[currentIndex], initialPos, radius);
            if (points.Count == 0)
            {
                openList.RemoveAt(currentIndex);
                continue;
            }

            int pointIndex = 0;
            if (points.Count > 1)
                pointIndex = Rand.UniformIntDistribution(0, points.Count, rand);

            GridEx.SetBlock(m_grid, points[pointIndex], BlockType.Titanium);
            openList.Add(points[pointIndex]);
            addedCount++;

            if (points.Count == 1)
                openList.RemoveAt(currentIndex);
        }
    }

    static List<Vector3Int> GetValidTitaniumPosAround(Vector3Int pos, Vector3Int initialPos, float maxDist)
    {
        List<Vector3Int> points = new List<Vector3Int>();

        for (int i = 0; i < 4; i++)
        {
            var dir = RotationEx.ToVector3Int((Rotation)i);

            Vector3Int testPos = pos + dir;

            int height = GridEx.GetHeight(m_grid, new Vector2Int(testPos.x, testPos.z));
            Vector3Int newPos = new Vector3Int(testPos.x, height, testPos.z);

            var item = GridEx.GetBlock(m_grid, newPos);
            if (item == BlockType.ground)
            {
                newPos.y++;
                points.Add(testPos);
            }
        }

        return points;
    }
}

