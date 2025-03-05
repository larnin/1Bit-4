using Noise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;
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
    static int m_seed = 0;
    static Grid m_grid = null;
    static Matrix<Area> m_heights = null;


    public static void Generate(WorldGeneratorSettings settings, int seed)
    {
        lock (m_stateLock)
        {
            if (m_state == GenerationState.Working)
                return;

            Reset();

            m_state = GenerationState.Working;
            m_stateTxt = "Starting";

            m_settings = settings;
            m_seed = seed;
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
        m_stateTxt = "Generate Heights";

        m_grid = new Grid(m_settings.size, m_settings.height);

        m_heights = GenerateBaseSurface();
        CalculateMontainsDistance(m_heights);
        GenerateMontains(m_heights);

        Smooth(m_heights);

        m_stateTxt = "Apply Heights";
        SimpleApplyHeight(m_heights);

        m_stateTxt = "Generate Resources";
        GenerateCrystal();
        GenerateTitanium();
        GenerateOil();

        m_heights = null;
    }

    enum AreaType
    {
        Plain,
        Mountains,
        Water,
    }

    struct Area
    {
        public float height;
        public AreaType type;
        public float montainHeight;
        public float distanceFromBorder;
        public float distanceFromCenter;

        public Area(float _height, AreaType _type)
        {
            height = _height;
            type = _type;
            montainHeight = 0;
            distanceFromBorder = 0;
            distanceFromCenter = 0;
        }
    }

    static Matrix<Area> GenerateBaseSurface()
    {
        int size = GridEx.GetRealSize(m_grid);
        var mat = new Matrix<Area>(size, size);

        Vector2 center = new Vector2(size / 2.0f, size / 2.0f);
        float maxDist = size / 2.0f;

        int seedIndex = 0;

        Perlin[] perlinDistance = new Perlin[m_settings.baseSurfaceRandomization.nbLayers];
        for (int i = 0; i < m_settings.baseSurfaceRandomization.nbLayers; i++)
        {
            perlinDistance[i] = new Perlin(size, m_settings.baseSurfaceRandomization.baseAmplitude * Mathf.Pow(m_settings.baseSurfaceRandomization.layerAmplitudeScale, i)
                , (int)(m_settings.baseSurfaceRandomization.baseFrequency * Mathf.Pow(2, i)), m_seed + seedIndex);
            seedIndex++;
        }

        Perlin[] perlinPlain = new Perlin[m_settings.plainsHeight.nbLayers];
        for(int i = 0; i < m_settings.plainsHeight.nbLayers; i++)
        {
            perlinPlain[i] = new Perlin(size, m_settings.plainsHeight.baseAmplitude * Mathf.Pow(m_settings.plainsHeight.layerAmplitudeScale, i)
                , (int)(m_settings.plainsHeight.baseFrequency * Mathf.Pow(2, i)), m_seed + seedIndex);
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                float distance = (new Vector2(i, j) - center).magnitude;
                foreach (var p in perlinDistance)
                    distance += p.Get(i, j);

                distance /= size / 2.0f;

                AreaType area = AreaType.Plain;
                float plainMultiplier = 1;

                if (distance > m_settings.baseSurfaceNormalizedRadius)
                {
                    plainMultiplier = 0;
                    area = AreaType.Water;
                }
                else if(distance > m_settings.baseSurfaceNormalizedTopRadius)
                { 
                    float curveDistance = (distance - m_settings.baseSurfaceNormalizedTopRadius) / (m_settings.baseSurfaceNormalizedRadius - m_settings.baseSurfaceNormalizedTopRadius);

                    plainMultiplier = DOVirtual.EasedValue(1, 0, curveDistance, m_settings.baseSurfaceDescreaseCurve);
                }

                float height = 0;

                if (area != AreaType.Water)
                {
                    float plainHeight = 0;
                    foreach (var p in perlinPlain)
                        plainHeight += Mathf.Abs(p.Get(i, j));

                    height = plainHeight * plainMultiplier;
                }

                height +=(m_settings.baseSurfaceHeight * plainMultiplier) + (1 - plainMultiplier) * m_settings.waterHeight;

                mat.Set(i, j, new Area(height, area));
            }
        }

        return mat;
    }

    struct DistancePointInfo
    {
        public Vector2Int pos;
        public float height;

        public DistancePointInfo(Vector2Int _pos, float _height = 0)
        {
            pos = _pos;
            height = _height;
        }
    }

    static void CalculateMontainsDistance(Matrix<Area> heights)
    {
        int size = GridEx.GetRealSize(m_grid);

        Matrix<bool> visitedAreas = new Matrix<bool>(size, size);

        List<DistancePointInfo> nextPoints = new List<DistancePointInfo>();

        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                var a = heights.Get(i, j);
                if (a.type == AreaType.Water)
                    continue;

                for(int x = 0; x < 4; x++)
                {
                    var dir = RotationEx.ToVectorInt((Rotation)x);
                    var newPos = new Vector2Int(i, j) + dir;
                    if (newPos.x < 0 || newPos.y < 0 || newPos.x >= size || newPos.y >= size)
                        continue;

                    if(heights.Get(newPos.x, newPos.y).type == AreaType.Water)
                    {
                        nextPoints.Add(new DistancePointInfo(new Vector2Int(i, j)));
                        visitedAreas.Set(i, j, true);
                    }
                }
            }
        }

        int nbLoop = 0;
        while (nextPoints.Count > 0)
        {
            var p = nextPoints[0];
            nextPoints.RemoveAt(0);

            var h = heights.Get(p.pos.x, p.pos.y);
                h.distanceFromBorder = p.height;
            heights.Set(p.pos.x, p.pos.y, h);

            for (int x = 0; x < 4; x++)
            {
                var dir = RotationEx.ToVectorInt((Rotation)x);
                var newPos = new Vector2Int(p.pos.x, p.pos.y) + dir;
                if (newPos.x < 0 || newPos.y < 0 || newPos.x >= size || newPos.y >= size)
                    continue;

                if (heights.Get(newPos.x, newPos.y).type != AreaType.Water && !visitedAreas.Get(newPos.x, newPos.y))
                {
                    nextPoints.Add(new DistancePointInfo(newPos, p.height + 1));
                    visitedAreas.Set(newPos.x, newPos.y, true);
                }
            }

            nbLoop++;
            if (nbLoop > size * size)
                break;
        }
        
        int seedIndex = 2;

        Perlin[] perlinDistance = new Perlin[m_settings.montainsDistanceRandomization.nbLayers];
        for (int i = 0; i < m_settings.montainsDistanceRandomization.nbLayers; i++)
        {
            perlinDistance[i] = new Perlin(size, m_settings.montainsDistanceRandomization.baseAmplitude * Mathf.Pow(m_settings.montainsDistanceRandomization.layerAmplitudeScale, i)
                , (int)(m_settings.montainsDistanceRandomization.baseFrequency * Mathf.Pow(2, i)), m_seed + seedIndex);
            seedIndex++;
        }

        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < size; j++)
            {
                var h = heights.Get(i, j);
                if (h.type == AreaType.Water)
                    continue;

                float delta = 0;
                foreach (var p in perlinDistance)
                    delta += p.Get(i, j);

                float dist = (new Vector2Int(i, j) - new Vector2Int(size, size) / 2).magnitude;

                h.distanceFromBorder = h.distanceFromBorder * (1 + delta);
                h.distanceFromCenter = dist * (1 + delta);

                heights.Set(i, j, h);
            }
        }

    }

    static void GenerateMontains(Matrix<Area> heights)
    {
        int size = GridEx.GetRealSize(m_grid);

        int seedIndex = 4;

        Perlin[] perlinMontains = new Perlin[m_settings.montainsHeight.nbLayers];
        for (int i = 0; i < m_settings.montainsHeight.nbLayers; i++)
        {
            perlinMontains[i] = new Perlin(size, m_settings.montainsHeight.baseAmplitude * Mathf.Pow(m_settings.montainsHeight.layerAmplitudeScale, i)
                , (int)(m_settings.montainsHeight.baseFrequency * Mathf.Pow(2, i)), m_seed + seedIndex);
            seedIndex++;
        }

        Turbulence baseMontainPerlin = new Turbulence(size, m_settings.montainsHeight.baseAmplitude, m_settings.montainsHeight.baseFrequency, m_seed + seedIndex);

        Perlin[] montainsPerlin = null;
        if (m_settings.montainsHeight.nbLayers > 1)
        {
            montainsPerlin = new Perlin[m_settings.montainsHeight.nbLayers - 1];
            for (int i = 1; i < m_settings.montainsHeight.nbLayers; i++)
            {
                montainsPerlin[i - 1] = new Perlin(size, m_settings.montainsHeight.baseAmplitude * MathF.Pow(m_settings.montainsHeight.layerAmplitudeScale, i)
                    , (int)(m_settings.montainsHeight.baseFrequency * MathF.Pow(2, i)), m_seed + seedIndex);
                seedIndex++;
            }
        }

        float maxHeight = 0;
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var h = heights.Get(i, j);
                if (h.type == AreaType.Water)
                    continue;

                if (h.distanceFromBorder < m_settings.montainsMinDistanceFromBorder)
                    continue;
                if (h.distanceFromCenter < m_settings.montainsMinDistanceFromCenter)
                    continue;

                float dist = Mathf.Min(h.distanceFromBorder - m_settings.montainsMinDistanceFromBorder, h.distanceFromCenter - m_settings.montainsMinDistanceFromCenter);
                float multiplier = 1;
                if (dist < m_settings.montainsBlendDistance)
                    multiplier = dist / m_settings.montainsBlendDistance;

                float height = baseMontainPerlin.Get(i, j);
                foreach (var p in montainsPerlin)
                    height += p.Get(i, j);

                height -= m_settings.montainsHeightOffset;
                if (height < 0)
                    continue;

                h.type = AreaType.Mountains;
                h.montainHeight = height * multiplier;

                maxHeight = Mathf.Max(h.montainHeight, maxHeight);

                heights.Set(i, j, h);
            }
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var h = heights.Get(i, j);
                if (h.type != AreaType.Mountains)
                    continue;

                h.montainHeight /= maxHeight;
                h.height += Mathf.Pow(h.montainHeight, m_settings.montainsHeightPower) * m_settings.montainsHeightMultiplier;
                heights.Set(i, j, h);
            }
        }
    }

    static void Smooth(Matrix<Area> heights)
    {
        Matrix<Area> newHeights = new Matrix<Area>(heights.width, heights.depth);

        int size = GridEx.GetRealSize(m_grid);
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                var currentH = heights.Get(i, j);
                if (currentH.type == AreaType.Water)
                    continue;

                float totalHeight = currentH.height * 2;
                int weightCount = 2;

                for (int x = -1; x <= 1; x++)
                {
                    for (int y = -1; y <= 1; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        int tempI = i + x;
                        int tempJ = j + y;

                        if (tempI < 0 || tempJ < 0 || tempI >= size || tempJ >= size)
                            continue;

                        var h = heights.Get(tempI, tempJ);
                        if (h.type == AreaType.Water)
                            continue;

                        totalHeight += h.height;
                        weightCount++;
                    }
                }

                currentH.height = totalHeight / weightCount;
                newHeights.Set(i, j, currentH);
            }
        }

        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                if(heights.Get(i, j).type != AreaType.Water)
                    heights.Set(i, j, newHeights.Get(i, j));
            }
        }
    }

    static void SimpleApplyHeight(Matrix<Area> heights)
    {
        int size = GridEx.GetRealSize(m_grid);
        int height = GridEx.GetRealHeight(m_grid);

        int midHeight = height / 2;

        for (int i = 0; i < size; i++)
        {
            for (int k = 0; k < size; k++)
            {
                BlockType type = BlockType.ground;
                var a = heights.Get(i, k);
                if (a.type == AreaType.Water)
                    type = BlockType.water;

                int localHeight = Mathf.RoundToInt(a.height) + midHeight;

                if (localHeight < 0)
                    localHeight = 0;
                if (localHeight >= height)
                    localHeight = height - 1;

                for (int j = localHeight; j >= 0; j--)
                    GridEx.SetBlock(m_grid, new Vector3Int(i, j, k), type);
            }
        }
    }

    static void GenerateCrystal()
    {
        int size = GridEx.GetRealSize(m_grid);

        MT19937 rand = new MT19937((uint)m_seed + 1);

        //place initial patch
        var initialPoint = Rand2D.UniformVector2CircleSurfaceDistribution(rand);
        initialPoint *= m_settings.crystalInitialPatchDistance;
        initialPoint += new Vector2(size / 2, size / 2);
        PlaceCrystalAt(new Vector2Int(Mathf.RoundToInt(initialPoint.x), Mathf.RoundToInt(initialPoint.y)));

        // rain drom patchs
        float densitySize = (float)size / m_settings.crystalPatchDensity;
        for (int i = 0; i < m_settings.crystalPatchDensity; i++)
        {
            for (int j = 0; j < m_settings.crystalPatchDensity; j++)
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
        if (m_heights.Get(pos.x, pos.y).type != AreaType.Plain)
            return false;

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

        var rand = new MT19937((uint)(m_seed + pos.x + pos.y));

        while (openList.Count > 0 && addedCount < maxCount)
        {
            int currentIndex = 0;
            if (openList.Count > 1)
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

        for (int i = 0; i < 4; i++)
        {
            var dir = RotationEx.ToVector3Int((Rotation)i);

            Vector3Int testPos = pos + dir;

            if (m_heights.Get(testPos.x, testPos.z).type != AreaType.Plain)
                continue;

            var item = GridEx.GetBlock(m_grid, testPos);
            if (item == BlockType.ground)
            {
                testPos.y++;
                item = GridEx.GetBlock(m_grid, testPos);
            }

            if (item != BlockType.air)
                continue;

            testPos.y--;
            item = GridEx.GetBlock(m_grid, testPos);
            if (item == BlockType.air)
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

        MT19937 rand = new MT19937((uint)m_seed + 2);

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

                    int height = GridEx.GetHeight(m_grid, new Vector2Int(x, y));
                    GridEx.SetBlock(m_grid, new Vector3Int(x, height, y), BlockType.oil);
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

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
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

        MT19937 rand = new MT19937((uint)m_seed + 3);

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
        if (m_heights.Get(pos.x, pos.y).type != AreaType.Mountains)
            return false;

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

        List<Vector3Int> titaniums = new List<Vector3Int>();

        var rand = new MT19937((uint)(m_seed + 1 + pos.x + pos.y));

        int maxCount = Rand.UniformIntDistribution(m_settings.titaniumPatchMin, m_settings.titaniumPatchMax, rand);
        float radius = Mathf.Sqrt(maxCount) / 2 + 1;

        while (openList.Count > 0 && titaniums.Count < maxCount)
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
            titaniums.Add(points[pointIndex]);

            if (points.Count == 1)
                openList.RemoveAt(currentIndex);
        }

        foreach(var t in titaniums)
        {
            int nbNeightbourgs = 0;
            foreach(var otherT in titaniums)
            {
                int dist = Mathf.Abs(t.x - otherT.x) + Mathf.Abs(t.y - otherT.y);
                if (dist == 1)
                    nbNeightbourgs++;
            }

            if (nbNeightbourgs > 0)
                nbNeightbourgs--;

            int h = Mathf.RoundToInt(Rand.UniformFloatDistribution(m_settings.titaniumMinHeight, m_settings.titaniumMaxHeight, rand) + m_settings.titaniumHeightNeighbour);

            for(int i = 1; i < h; i++)
                GridEx.SetBlock(m_grid, t + new Vector3Int(0, i, 0), BlockType.Titanium);
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
                points.Add(newPos);
            }
        }

        return points;
    }
}

