using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class ForceSpecificBuilding : NamedQuestObject
{
    [SerializeField] BuildingType m_allowedBuilding = BuildingType.Pylon;
    [SerializeField] GameObject m_visualInstance;
    [SerializeField] string m_iconName;
    [SerializeField] float m_iconOffset = 10;

    Vector3Int m_pos = Vector3Int.zero;
    bool m_completed = false;

    SubscriberList m_subscriberList = new SubscriberList();

    public override bool IsTaskComplete()
    {
        return m_completed;
    }

    private void Awake()
    {
        m_subscriberList.Add(new Event<ValidateNewBuildingPositionEvent>.Subscriber(ValidatePos));
        m_subscriberList.Add(new Event<OnBuildingBuildEvent>.Subscriber(OnBuild));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        PlaceVisualInstance();

        m_completed = false;

        if (GameSystem.instance != null)
            GameSystem.instance.SetForcedBuilding(m_allowedBuilding);

        if(DisplayIconsV2.instance != null && m_visualInstance != null && m_iconName != null && m_iconName.Length > 0)
            DisplayIconsV2.instance.Register(m_visualInstance, m_iconOffset, m_iconName, "", true);
    }

    private void OnDisable()
    {
        if (GameSystem.instance != null)
            GameSystem.instance.DisableForcedBuilding();

        if (DisplayIconsV2.instance != null && m_visualInstance != null)
            DisplayIconsV2.instance.Unregister(m_visualInstance);
    }

    void PlaceVisualInstance()
    {
        var pos = transform.position;
        m_pos = new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z));

        if (m_visualInstance == null)
            return;

        var data = Global.instance.buildingDatas.GetBuilding(m_allowedBuilding);
        if (data == null)
            return;

        Vector3 scale = new Vector3(data.size.x, 1, data.size.z);
        m_visualInstance.transform.localScale = scale;

        pos = new Vector3(m_pos.x, m_pos.y, m_pos.z);
        pos.x += (scale.x - 1) / 2;
        pos.z += (scale.z - 1) / 2;

        m_visualInstance.transform.position = pos;
    }

    void ValidatePos(ValidateNewBuildingPositionEvent e)
    {
        if (e.pos != m_pos)
            e.placeType = BuildingPlaceType.PositionLocked;

        Debug.Log(e.pos - m_pos);
    }

    void OnBuild(OnBuildingBuildEvent e)
    {
        if (e.type == m_allowedBuilding && e.pos == m_pos)
        {
            m_completed = true;
            Destroy(gameObject);
        }
    }
}
