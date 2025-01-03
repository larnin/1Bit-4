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
    [SerializeField] float m_hideDuration = 2;
    [SerializeField] float m_hideDistance = 2;
    [SerializeField] Ease m_hideCurve;

    Vector2Int m_size = Vector2Int.one;

    float m_lifeTimer;

    Transform m_render;
    Vector3 m_initialRenderPos;

    public override void Awake()
    {
        base.Awake();

        m_render = transform.Find("Mesh");
        if (m_render != null)
            m_initialRenderPos = m_render.localPosition;
    }

    public void SetSize(Vector2Int size)
    {
        m_size = size;
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

        if (m_lifeTimer > m_displayDuration)
            UpdateRenderPosition();

        if (m_lifeTimer > m_displayDuration + m_hideDuration)
            Destroy(gameObject);
    }

    void UpdateRenderPosition()
    {
        float normTime = m_lifeTimer - m_displayDuration;
        normTime /= m_hideDuration;

        normTime = Mathf.Clamp01(normTime);

        normTime = DOVirtual.EasedValue(0, 1, normTime, m_hideCurve);
        normTime *= -m_hideDistance;

        Vector3 pos = m_initialRenderPos + new Vector3(0, normTime, 0);

        if (m_render != null)
            m_render.localPosition = pos;
    }
}
