using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class UIElementData
{
    public float spacing = 1;
    public GameObject simpleTextPrefab;

    public static T CreateAndGet<T>(GameObject prefab) where T : UIElementBase
    {
        var instance = GameObject.Instantiate(prefab);

        var elem = instance.GetComponent<T>();
        if(elem == null)
        {
            GameObject.Destroy(instance);
            return null;
        }

        return elem;
    }
}
