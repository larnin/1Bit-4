using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

public class EditorWorldGeneration : MonoBehaviour
{
    static EditorWorldGeneration m_instance;

    int m_seed;
    UIElementIntInput m_seedContainer;

    WorldGeneratorSettings m_settings = new WorldGeneratorSettings();

    bool m_generationStarted = false;

    public static EditorWorldGeneration instance { get { return m_instance; } }

    private void Awake()
    {
        if (m_instance == null)
            m_instance = this;

        RandomizeSeed();
    }

    public void DrawSettings(UIElementContainer container)
    {
        m_seedContainer = UIElementData.Create<UIElementIntInput>(container).SetLabel("Seed").SetValue(m_seed).SetValueChangeFunc((int value) => { m_seed = value; });
        UIElementData.Create<UIElementButton>(container).SetText("Randomize").SetClickFunc(RandomizeSeed);
        UIElementData.Create<UIElementSpace>(container).SetSpace(5);

        var foldGround = UIElementData.Create<UIElementFoldable>(container).SetHeaderText("Ground").GetContainer();
        DrawGroundSettings(foldGround);

        var foldLakes = UIElementData.Create<UIElementFoldable>(container).SetHeaderText("Lakes").GetContainer();
        DrawLakesSettings(foldLakes);

        var foldCrystal = UIElementData.Create<UIElementFoldable>(container).SetHeaderText("Crystal").GetContainer();
        DrawCrystalSettings(foldCrystal);

        var foldOil = UIElementData.Create<UIElementFoldable>(container).SetHeaderText("Oil").GetContainer();
        DrawOilSettings(foldOil);

        var foldTitanium = UIElementData.Create<UIElementFoldable>(container).SetHeaderText("Titanium").GetContainer();
        DrawTitaniumSettings(foldTitanium);

        UIElementData.Create<UIElementSpace>(container).SetSpace(15);
        UIElementData.Create<UIElementSimpleText>(container).SetText("WARNING - Generating a new surface will reset everything and can't be undone");
        UIElementData.Create<UIElementButton>(container).SetText("Generate").SetClickFunc(Generate);
    }

    void DrawGroundSettings(UIElementContainer container)
    {
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Base SurfaceHeight").SetValue(m_settings.baseSurfaceHeight).SetValueChangeFunc((float value) => { m_settings.baseSurfaceHeight = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Base Surface Normalized Radius").SetValue(m_settings.baseSurfaceNormalizedRadius).SetValueChangeFunc((float value) => { m_settings.baseSurfaceNormalizedRadius = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Base Surface Normalized Top Radius").SetValue(m_settings.baseSurfaceNormalizedTopRadius).SetValueChangeFunc((float value) => { m_settings.baseSurfaceNormalizedTopRadius = value; });

        DrawPerlinSettings(container, m_settings.baseSurfaceRandomization, "Base Surface Randomization");
        DrawPerlinSettings(container, m_settings.plainsHeight, "Plains Height");

        UIElementData.Create<UIElementSpace>(container).SetSpace(5);

        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Montains Min Distance From Center").SetValue(m_settings.montainsMinDistanceFromCenter).SetValueChangeFunc((float value) => { m_settings.montainsMinDistanceFromCenter = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Montains Min Distance From Border").SetValue(m_settings.montainsMinDistanceFromBorder).SetValueChangeFunc((float value) => { m_settings.montainsMinDistanceFromBorder = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Montains Blend Distance").SetValue(m_settings.montainsBlendDistance).SetValueChangeFunc((float value) => { m_settings.montainsBlendDistance = value; });

        DrawPerlinSettings(container, m_settings.montainsHeight, "Montains Height");
        DrawPerlinSettings(container, m_settings.montainsDistanceRandomization, "Montains Distance Randomization");

        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Montains Height Offset").SetValue(m_settings.montainsHeightOffset).SetValueChangeFunc((float value) => { m_settings.montainsHeightOffset = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Montains Height Multiplier").SetValue(m_settings.montainsHeightMultiplier).SetValueChangeFunc((float value) => { m_settings.montainsHeightMultiplier = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Montains Height Power").SetValue(m_settings.montainsHeightPower).SetValueChangeFunc((float value) => { m_settings.montainsHeightPower = value; });
    }

    void DrawLakesSettings(UIElementContainer container)
    {
        UIElementData.Create<UIElementIntInput>(container).SetLabel("Lake Density").SetValue(m_settings.lakeDensity).SetValueChangeFunc((int value) => { m_settings.lakeDensity = value; });
        UIElementData.Create<UIElementIntInput>(container).SetLabel("Lake Retry Count").SetValue(m_settings.lakeRetryCount).SetValueChangeFunc((int value) => { m_settings.lakeRetryCount = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Lake Min Distance").SetValue(m_settings.lakeMinDistance).SetValueChangeFunc((float value) => { m_settings.lakeMinDistance = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Lake Min Distance Between Lakes").SetValue(m_settings.lakeMinDistanceBetweenLakes).SetValueChangeFunc((float value) => { m_settings.lakeMinDistanceBetweenLakes = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Lake Min Size").SetValue(m_settings.lakeMinSize).SetValueChangeFunc((float value) => { m_settings.lakeMinSize = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Lake Max Size").SetValue(m_settings.lakeMaxSize).SetValueChangeFunc((float value) => { m_settings.lakeMaxSize = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Lake Decrease Distance").SetValue(m_settings.lakeDecreaseDistance).SetValueChangeFunc((float value) => { m_settings.lakeDecreaseDistance = value; });

        DrawPerlinSettings(container, m_settings.lakeSurfaceRandomization, "Lake Surface Randomization");
    }

    void DrawCrystalSettings(UIElementContainer container)
    {
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Crystal Patch Size Base").SetValue(m_settings.crystalPatchSizeBase).SetValueChangeFunc((float value) => { m_settings.crystalPatchSizeBase = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Crystal Patch Increase").SetValue(m_settings.crystalPatchIncrease).SetValueChangeFunc((float value) => { m_settings.crystalPatchIncrease = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Crystal Patch Density").SetValue(m_settings.crystalPatchDensity).SetValueChangeFunc((float value) => { m_settings.crystalPatchDensity = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Crystal Retry Count").SetValue(m_settings.crystalRetryCount).SetValueChangeFunc((float value) => { m_settings.crystalRetryCount = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Crystal Initial Patch Distance").SetValue(m_settings.crystalInitialPatchDistance).SetValueChangeFunc((float value) => { m_settings.crystalInitialPatchDistance = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Crystal Min Distance").SetValue(m_settings.crystalMinDistance).SetValueChangeFunc((float value) => { m_settings.crystalMinDistance = value; });
    }

    void DrawOilSettings(UIElementContainer container)
    {
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Oil Density").SetValue(m_settings.oilDensity).SetValueChangeFunc((float value) => { m_settings.oilDensity = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Oil RetryCount").SetValue(m_settings.oilRetryCount).SetValueChangeFunc((float value) => { m_settings.oilRetryCount = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Oil MinDistance").SetValue(m_settings.oilMinDistance).SetValueChangeFunc((float value) => { m_settings.oilMinDistance = value; });
    }

    void DrawTitaniumSettings(UIElementContainer container)
    {
        UIElementData.Create<UIElementIntInput>(container).SetLabel("Titanium Patch Min").SetValue(m_settings.titaniumPatchMin).SetValueChangeFunc((int value) => { m_settings.titaniumPatchMin = value; });
        UIElementData.Create<UIElementIntInput>(container).SetLabel("Titanium Patch Max").SetValue(m_settings.titaniumPatchMax).SetValueChangeFunc((int value) => { m_settings.titaniumPatchMax = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Titanium Patch Density").SetValue(m_settings.titaniumPatchDensity).SetValueChangeFunc((float value) => { m_settings.titaniumPatchDensity = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Titanium Elevation Min Variation").SetValue(m_settings.titaniumElevationMinVariation).SetValueChangeFunc((float value) => { m_settings.titaniumElevationMinVariation = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Titanium Retry Count").SetValue(m_settings.titaniumRetryCount).SetValueChangeFunc((float value) => { m_settings.titaniumRetryCount = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Titanium Min Distance").SetValue(m_settings.titaniumMinDistance).SetValueChangeFunc((float value) => { m_settings.titaniumMinDistance = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Titanium Min Height").SetValue(m_settings.titaniumMinHeight).SetValueChangeFunc((float value) => { m_settings.titaniumMinHeight = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Titanium Max Height").SetValue(m_settings.titaniumMaxHeight).SetValueChangeFunc((float value) => { m_settings.titaniumMaxHeight = value; });
        UIElementData.Create<UIElementFloatInput>(container).SetLabel("Titanium Height Neighbour").SetValue(m_settings.titaniumHeightNeighbour).SetValueChangeFunc((float value) => { m_settings.titaniumHeightNeighbour = value; });
    }

    void DrawPerlinSettings(UIElementContainer container, PerlinSettings settings, string label)
    {
        var folder = UIElementData.Create<UIElementFoldable>(container).SetHeaderText(label).GetContainer();

        UIElementData.Create<UIElementIntInput>(folder).SetLabel("Base Frequency").SetValue(settings.baseFrequency).SetValueChangeFunc((int value) => { settings.baseFrequency = value; });
        UIElementData.Create<UIElementFloatInput>(folder).SetLabel("Base Amplitude").SetValue(settings.baseFrequency).SetValueChangeFunc((float value) => { settings.baseAmplitude = value; });
        UIElementData.Create<UIElementIntInput>(folder).SetLabel("Nb Layers").SetValue(settings.baseFrequency).SetValueChangeFunc((int value) => { settings.nbLayers = value; });
        UIElementData.Create<UIElementFloatInput>(folder).SetLabel("Layer Amplitude Scale").SetValue(settings.baseFrequency).SetValueChangeFunc((float value) => { settings.layerAmplitudeScale = value; });
    }

    void RandomizeSeed()
    {
        m_seed = (int)StaticRandomGenerator<MT19937>.Get().Next();
        if (m_seedContainer != null)
            m_seedContainer.SetValue(m_seed);
    }

    void Generate()
    {
        if (m_generationStarted)
            return;

        if (WorldGenerator.GetState() == WorldGenerator.GenerationState.Working)
            return;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;

        m_settings.size = grid.grid.Size();
        m_settings.height = grid.grid.Height();
        m_settings.loopX = grid.grid.LoopX();
        m_settings.loopZ = grid.grid.LoopZ();

        int realHeight = GridEx.GetRealHeight(grid.grid);
        m_settings.waterHeight = -(realHeight / 2);

        m_generationStarted = true;
        WorldGenerator.Generate(m_settings, m_seed);
    }

    private void Update()
    {
        if(m_generationStarted)
        {
            if (EditorLogs.instance != null)
                EditorLogs.instance.AddLog("Generation", "Generating ...");

            if (WorldGenerator.GetState() == WorldGenerator.GenerationState.Finished)
            {
                OnGenerationEnd();
                if (EditorLogs.instance != null)
                    EditorLogs.instance.AddLog("Generation2", "Completed !");
            }
            else if (EditorLogs.instance != null)
                EditorLogs.instance.AddLog("Generation2", WorldGenerator.GetStateText());
        }
    }

    void OnGenerationEnd()
    {
        m_generationStarted = false;

        //todo remove entities

        var newGrid = WorldGenerator.GetGrid();

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;

        int size = GridEx.GetRealSize(grid.grid);
        int newSize = GridEx.GetRealSize(newGrid);
        int height = GridEx.GetRealHeight(grid.grid);
        int newHeight = GridEx.GetRealHeight(newGrid);

        if (size != newSize || height != newHeight)
            return; //must not happen

        for(int i = 0; i < size; i++)
        {
            for(int j = 0; j < height; j++)
            {
                for(int k = 0; k < size; k++)
                {
                    Vector3Int pos = new Vector3Int(i, j, k);
                    GridEx.SetBlock(grid.grid, pos, GridEx.GetBlock(newGrid, pos));
                }
            }
        }

        if(EditorGridBehaviour.instance != null)
        {
            EditorGridBehaviour.instance.SetRegionDirty(new BoundsInt(Vector3Int.zero, new Vector3Int(size, height, size)));
        }
    }
}
