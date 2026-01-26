using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingMonolithNullifier : BuildingBase
{
    [SerializeField] float m_energyConsumption = 1;
    [SerializeField] GameObject m_nullifierMesh;

    BuildingMonolith m_monolith;

    bool m_active = false;
    float m_energyUptake;
    float m_energyEfficiency = 1;

    SubscriberList m_subscriberList = new SubscriberList();

    public override void Awake()
    {
        base.Awake();
        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Subscribe();

        SetNullifierActive(false);
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        m_subscriberList.Unsubscribe();
    }

    public override void Start()
    {
        base.Start();
        m_monolith = FindUnderlyingMonilith();
    }

    public override BuildingType GetBuildingType()
    {
        return BuildingType.MonolithNullifier;
    }

    public override float EnergyUptakeWanted()
    {
        if(m_active)
            return m_energyConsumption;
        return 0;
    }

    public override void EnergyUptake(float value)
    {
        m_energyUptake = value;
        m_energyEfficiency = value / EnergyUptakeWanted();
        if (m_energyEfficiency > 1)
            m_energyEfficiency = 1;

        m_energyEfficiency *= m_energyEfficiency;
    }

    public override BuildingPlaceType CanBePlaced(Vector3Int pos)
    {
        if (BuildingList.instance == null)
            return BuildingPlaceType.Unknow;

        Vector3Int testPos = pos + new Vector3Int(0, -1, 0);

        var b = BuildingList.instance.GetBuildingAt(testPos);
        if (b == null)
            return BuildingPlaceType.NeedMonolith;
        if (b.GetBuildingType() != BuildingType.Monolith)
            return BuildingPlaceType.NeedMonolith;
        if (b.GetPos() != testPos)
            return BuildingPlaceType.NeedMonolith;

        return BuildingPlaceType.Valid;
    }

    public void SetNullifierActive(bool active)
    {
        m_active = active;

        if (m_nullifierMesh != null)
            m_nullifierMesh.SetActive(m_active);

        if (active)
            TriggerGamemode();
    }

    public bool IsNullifierActive()
    {
        return m_active;
    }

    string EnergyUptakeStr()
    {
        return m_energyUptake.ToString("#0.##") + '/' + m_energyConsumption.ToString("#0.##");
    }

    float GetEfficiency()
    {
        return m_energyEfficiency;
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        DisplayGenericInfos(e.container);

        if (m_active)
            UIElementData.Create<UIElementSimpleText>(e.container).SetText("Active");

        UIElementData.Create<UIElementLabelAndText>(e.container).SetLabel("Energy Uptake").SetTextFunc(EnergyUptakeStr);
        UIElementData.Create<UIElementFillValue>(e.container).SetLabel("Efficiency").SetMax(1).SetValueFunc(GetEfficiency).SetValueDisplayType(UIElementFillValueDisplayType.percent).SetNbDigits(0);
    }

    BuildingMonolith FindUnderlyingMonilith()
    {
        Vector3Int testPos = GetPos() + new Vector3Int(0, -1, 0);
        var b = BuildingList.instance.GetBuildingAt(testPos);
        if (b == null)
            return null;

        return b as BuildingMonolith;
    }

    void TriggerGamemode()
    {
        if (m_monolith == null)
            return;

        if (GamemodeSystem.instance == null)
            return;

        int nbMode = GamemodeSystem.instance.GetGamemodeNb();

        for(int i = 0; i < nbMode; i++)
        {
            var mode = GamemodeSystem.instance.GetGamemodeFromIndex(i) as MonolithMode;
            if (mode == null)
                continue;

            mode.TriggerMonolith(m_monolith);
        }
    }
}
