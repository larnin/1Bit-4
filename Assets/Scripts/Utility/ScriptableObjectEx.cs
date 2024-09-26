using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public static class ScriptableObjectEx
{

#if UNITY_EDITOR
    //path starting after Assets/Resources
    //name without extention
    public static T CreateAsset<T>(string path, string name) where T : ScriptableObject
    {
        string[] sub = path.Split('/');

        int index = 0;

        string partialPath = "Assets";
        string addingPath = "Resources";

        do
        {
            if (!AssetDatabase.IsValidFolder(partialPath + "/" + addingPath))
                AssetDatabase.CreateFolder(partialPath, addingPath);

            partialPath += "/" + addingPath;

            if (index == sub.Length)
                break;
            addingPath = sub[index];

            index++;
        } while (index <= sub.Length);

        T asset = ScriptableObject.CreateInstance(typeof(T)) as T;
        if (asset == null)
        {
            Debug.LogError("Can't create a " + typeof(T).Name);
            return null;
        }

        AssetDatabase.CreateAsset(asset, partialPath + "/" + name + ".asset");
        AssetDatabase.SaveAssets();

        return asset;
    }
#endif
}
