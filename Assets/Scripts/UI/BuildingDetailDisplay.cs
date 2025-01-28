using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class BuildingDetailDisplay : MonoBehaviour
{
    [SerializeField] GameObject m_resourcePrefab;
    [SerializeField] float m_resourceDelta;

    List<OneResourceDisplay> m_resources = new List<OneResourceDisplay>();

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public void SetBuilding(BuildingType type)
    {
        gameObject.SetActive(true);

        foreach (var r in m_resources)
            Destroy(r.gameObject);
        m_resources.Clear();

        var data = Global.instance.buildingDatas.GetBuilding(type);
        if (data == null)
            return;

        var title = transform.Find("Name")?.GetComponent<TMP_Text>();
        if (title != null)
            title.text = data.name;
        var desc = transform.Find("Description")?.GetComponent<TMP_Text>();
        if (desc != null)
            desc.text = data.description;

        var costPivot = transform.Find("CostPivot");
        if(costPivot != null)
        {
            if (data.IsFree())
            {
                var obj = Instantiate(m_resourcePrefab);
                obj.transform.SetParent(costPivot, false);
                var rectTransform = obj.GetComponent<RectTransform>();
                if (rectTransform != null)
                    rectTransform.anchoredPosition = new Vector2(-m_resourceDelta / 2, 0);

                var resourceDisplay = obj.GetComponent<OneResourceDisplay>();
                if (resourceDisplay != null)
                {
                    resourceDisplay.SetFree();
                    m_resources.Add(resourceDisplay);
                }
            }
            else
            {
                for (int i = 0; i < data.cost.cost.Count; i++)
                {
                    var c = data.cost.cost[i];

                    var obj = Instantiate(m_resourcePrefab);
                    obj.transform.SetParent(costPivot, false);
                    var rectTransform = obj.GetComponent<RectTransform>();
                    if (rectTransform != null)
                        rectTransform.anchoredPosition = new Vector2((i - data.cost.cost.Count / 2.0f) * m_resourceDelta, 0);

                    var resourceDisplay = obj.GetComponent<OneResourceDisplay>();
                    if (resourceDisplay != null)
                    {
                        resourceDisplay.SetData(c.type, c.count);
                        m_resources.Add(resourceDisplay);
                    }
                }
            }
        }
    }

    public void SetDisabled()
    {
        foreach (var r in m_resources)
            Destroy(r.gameObject);
        m_resources.Clear();

        gameObject.SetActive(false);
    }
}
