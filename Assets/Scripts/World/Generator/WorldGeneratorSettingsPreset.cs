using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class WorldGeneratorSettingsPreset : ScriptableObject
{
    const string s_name = "GenerationPresets";

    [Serializable]
    class OneGeneratorSetting
    {
        public string name;
        public WorldGeneratorSettings settings;
    }

    [SerializeField] List<OneGeneratorSetting> m_presets = new List<OneGeneratorSetting>();

    static WorldGeneratorSettingsPreset m_instance;

    public static WorldGeneratorSettingsPreset instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = Global.Create<WorldGeneratorSettingsPreset>(s_name, true);
            }
            return m_instance;
        }
    }

    public List<string> GetAllPresets()
    {
        List<string> presets = new List<string>();

        foreach (var p in m_presets)
            presets.Add(p.name);

        return presets;
    }

    public WorldGeneratorSettings GetPreset(string name)
    {
        foreach (var p in m_presets)
        {
            if (p.name == name)
                return p.settings;
        }

        return null;
    }

    public WorldGeneratorSettings GetPresetWithCopy(string name)
    {
        WorldGeneratorSettings preset = GetPreset(name);
        if (preset == null)
            return null;

        string json = JsonUtility.ToJson(preset);
        return JsonUtility.FromJson<WorldGeneratorSettings>(json);
    }

    public void SetPreset(string name, WorldGeneratorSettings settings)
    {
        string json = JsonUtility.ToJson(settings);
        WorldGeneratorSettings clonedSettings = JsonUtility.FromJson<WorldGeneratorSettings>(json);

        bool found = false;
        foreach (var p in m_presets)
        {
            if (p.name == name)
            {
                found = true;
                p.settings = clonedSettings;
                break;
            }
        }

        if(!found)
        {
            OneGeneratorSetting preset = new OneGeneratorSetting();
            preset.name = name;
            preset.settings = clonedSettings;
            m_presets.Add(preset);
        }

#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
#endif
    }
}
