using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public enum WorldSize
{
    Small,
    Medium,
    Large,
    Micro,
    Gargantuan,
}

[Serializable]
public class WorldGeneratorSettingByWorld
{
    public WorldSize size;
    public WorldGeneratorSettings settings;
}

public class Global : ScriptableObject
{
    [SerializeField] List<WorldGeneratorSettingByWorld> m_generatorSettings;
    public List<WorldGeneratorSettingByWorld> generatorSettings { get { return m_generatorSettings; } }

    public WorldGeneratorSettings GetWorldGeneratorSettings(WorldSize size)
    {
        foreach(var s in m_generatorSettings)
        {
            if (s.size == size)
                return s.settings;
        }

        return null;
    }

    [SerializeField] BlockDatas m_blockDatas;
    public BlockDatas blockDatas { get { return m_blockDatas; } }

    [SerializeField] BuildingsDatas m_buildingDatas;
    public BuildingsDatas buildingDatas { get { return m_buildingDatas; } }

    [SerializeField] ResourceDatas m_resourcesDatas;
    public ResourceDatas resourceDatas { get { return m_resourcesDatas; } }

    [SerializeField] DifficultyData m_difficultyDatas;
    public DifficultyData difficultyDatas { get { return m_difficultyDatas; } }

    [SerializeField] UIElementData m_UIElementDatas;
    public UIElementData UIElementDatas { get { return m_UIElementDatas; } }

    [SerializeField] StatusDatas m_statusDatas;
    public StatusDatas statusDatas { get { return m_statusDatas; } }

    [SerializeField] StatsData m_statsDatas;
    public StatsData statsDatas { get { return m_statsDatas; } }

    [SerializeField] TipsDatas m_tipsDatas;
    public TipsDatas tipsDatas { get { return m_tipsDatas; } }

    [SerializeField] SoundsDatas m_soundsDatas;
    public SoundsDatas soundsDatas { get { return m_soundsDatas; } }

    [SerializeField] ColorsData m_colorsDatas;
    public ColorsData colorsDatas { get { return m_colorsDatas; } }

    static Global m_instance;

    static string s_path = "World";
    static string s_globalName = "Global";

#if UNITY_EDITOR
    [MenuItem("Game/Create Global")]
    public static void MakeGlobal()
    {
        m_instance = Create<Global>(s_globalName) ?? m_instance;

        AssetDatabase.SaveAssets();
    }

    static T Create<T>(string name) where T : ScriptableObject
    {
        var elements = Resources.LoadAll<ScriptableObject>(s_path);
        foreach (var e in elements)
        {
            if (e.name == name)
            {
                Debug.LogError("An item with the name " + name + " already exist in Ressources/" + s_path);
                return null;
            }
        }

        T asset = ScriptableObjectEx.CreateAsset<T>(s_path, name);
        EditorUtility.SetDirty(asset);

        return asset;
    }
#endif

    public static Global instance
    {
        get
        {
            if (m_instance == null)
            {
                Load();
            }
            return m_instance;
        }
    }

    static void Load()
    {
        m_instance = LoadOneInstance<Global>(s_globalName);
    }

    static T LoadOneInstance<T>(string name) where T : ScriptableObject
    {
        T asset = null;

        var elements = Resources.LoadAll<ScriptableObject>(s_path);
        foreach (var e in elements)
        {
            if (e.name == name)
            {
                asset = e as T;
                break;
            }
        }

        if (asset == null)
            Debug.LogError("The " + name + " asset does not exist in the Ressources/" + s_path + " folder");

        return asset;
    }
}