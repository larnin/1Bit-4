using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BuildingTower : BuildingBase, I_UIElementBuilder
{
    [SerializeField] float m_powerGeneration = 10;
    [SerializeField] float m_placementRadius = 5;

    public override BuildingType GetBuildingType()
    {
        return BuildingType.Tower;
    }

    public override float EnergyGeneration()
    {
        return m_powerGeneration;
    }

    public override float PlacementRadius()
    {
        return m_placementRadius;
    }

    public void Build(UIElementContainer container)
    {
        var titleElt = UIElementData.CreateAndGet<UIElementSimpleText>(Global.instance.UIElementDatas.simpleTextPrefab);
        titleElt.SetText("Title");
        container.AddElement(titleElt);

        var descElt = UIElementData.CreateAndGet<UIElementSimpleText>(Global.instance.UIElementDatas.simpleTextPrefab);
        descElt.SetText("Some longer text that use at least 2 lignes, and maybe more");
        container.AddElement(descElt);
    }
}
