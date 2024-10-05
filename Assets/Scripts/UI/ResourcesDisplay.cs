using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] GameObject m_resourcePrefab;
    [SerializeField] float m_resourceDelta;

    List<OneResourceDisplay> m_resources = new List<OneResourceDisplay>();

    private void Update()
    {
        if (ResourceSystem.instance == null)
            return;

        var list = Enum.GetValues(typeof(ResourceType));

        int nb = 0;
        foreach(var value in list)
        {
            var type = (ResourceType)value;
            //if (!ResourceSystem.instance.HaveResource(type))
            //    continue;

            if (m_resources.Count == nb)
                m_resources.Add(CreateOneResource());

            float stored = ResourceSystem.instance.GetResourceStored(type);
            float storageMax = ResourceSystem.instance.GetResourceStorageMax(type);
            float delta = ResourceSystem.instance.GetLastSecondResourceMean(type);

            m_resources[nb].SetDataWithDelta(type, stored, storageMax, delta);

            nb++;
        }

        while(nb < m_resources.Count)
        {
            var r = m_resources[m_resources.Count - 1];
            m_resources.RemoveAt(m_resources.Count - 1);
            Destroy(r.gameObject);
        }

        for(int i = 0; i < m_resources.Count; i++)
        {
            var tr = m_resources[i].GetComponent<RectTransform>();
            if (tr != null)
                tr.anchoredPosition = new Vector2(m_resourceDelta * i, 0);
        }
    }

    OneResourceDisplay CreateOneResource()
    {
        var obj = Instantiate(m_resourcePrefab);
        obj.transform.SetParent(transform, true);
        var display = obj.GetComponent<OneResourceDisplay>();
        return display;
    }
}

