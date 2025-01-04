using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class BuildingDestroyed : BuildingBase
{
    [SerializeField] float m_displayDuration = 10;
    [SerializeField] float m_appearDuration = 1;
    [SerializeField] float m_hideDuration = 2;
    [SerializeField] float m_hideDistance = 2;
    [SerializeField] Ease m_hideCurve;

    Vector2Int m_size = Vector2Int.one;

    float m_lifeTimer;

    Transform m_render;

    public override void Awake()
    {
        base.Awake();

        UpdateRenderPosition();
    }

    public void SetSize(Vector2Int size)
    {
        m_size = size;

        CreateRender();
        UpdateRenderPosition();
    }

    public override Vector3Int GetSize()
    {
        var size = base.GetSize();
        size.x = m_size.x;
        size.z = m_size.y;

        return size;
    }

    public override BuildingType GetBuildingType()
    {
        return BuildingType.DestroyedBuilding;
    }

    protected override void OnUpdateAlways()
    {
        m_lifeTimer += Time.deltaTime;

        UpdateRenderPosition();

        if (m_lifeTimer > m_displayDuration + m_hideDuration)
            Destroy(gameObject);
    }

    void UpdateRenderPosition()
    {
        float normTime = 0;

        if (m_lifeTimer < m_appearDuration)
            normTime = 1 - (m_lifeTimer / m_appearDuration);
        else if(m_lifeTimer > m_displayDuration)
        {
            normTime = m_lifeTimer - m_displayDuration;
            normTime /= m_hideDuration;
        }

        normTime = Mathf.Clamp01(normTime);

        normTime = DOVirtual.EasedValue(0, 1, normTime, m_hideCurve);
        normTime *= -m_hideDistance;

        if (m_render != null)
            m_render.localPosition = new Vector3(0, normTime, 0);
    }

    void CreateRender()
    {
        if (m_render != null)
        {
            Destroy(m_render.gameObject);
            m_render = null;
        }

        var data = Global.instance.buildingDatas.GetDestructedBuildingDatas(m_size);
        if (data == null || data.prefab == null)
            return;

        var obj = Instantiate(data.prefab);
        m_render = obj.transform;
        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
    }
}
