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

    [SerializeField] GameObject simpleTextPrefab;
    [SerializeField] GameObject spacePrefab;
    [SerializeField] GameObject linePrefab;
    [SerializeField] GameObject valuePrefab;

    T CreateImpl<T>(UIElementContainer container) where T : UIElementBase
    {
        GameObject prefab = null;

        if (typeof(T) == typeof(UIElementSimpleText))
            prefab = simpleTextPrefab;
        else if (typeof(T) == typeof(UIElementSpace))
            prefab = spacePrefab;
        else if (typeof(T) == typeof(UIElementLine))
            prefab = linePrefab;
        else if (typeof(T) == typeof(UIElementValue))
            prefab = valuePrefab;

        if (prefab == null)
            return null;

        var instance = GameObject.Instantiate(prefab);

        var elem = instance.GetComponent<T>();
        if (elem == null)
        {
            GameObject.Destroy(instance);
            return null;
        }

        container.AddElement(elem);

        return elem;
    }

    public static T Create<T>(UIElementContainer container) where T : UIElementBase
    {
        return Global.instance.UIElementDatas.CreateImpl<T>(container);
    }
}
