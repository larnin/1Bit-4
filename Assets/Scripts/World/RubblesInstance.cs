using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class RubblesInstance : MonoBehaviour
{
    Vector2Int m_size = Vector2Int.one;

    float m_lifeTimer;

    Transform m_render;

    public void Awake()
    {
        UpdateRenderPosition();
    }

    public void SetSize(Vector2Int size)
    {
        m_size = size;

        CreateRender();
        UpdateRenderPosition();
    }

    protected void Update()
    {
        if (GameInfos.instance.paused)
            return;

        m_lifeTimer += Time.deltaTime;

        UpdateRenderPosition();
    }

    void UpdateRenderPosition()
    {
        float normTime = 0;

        if (m_lifeTimer < Global.instance.buildingDatas.destructionDatas.appearDuration)
            normTime = 1 - (m_lifeTimer / Global.instance.buildingDatas.destructionDatas.appearDuration);
        else if(m_lifeTimer > Global.instance.buildingDatas.destructionDatas.displayDuration)
        {
            normTime = m_lifeTimer - Global.instance.buildingDatas.destructionDatas.displayDuration;
            normTime /= Global.instance.buildingDatas.destructionDatas.hideDuration;
        }

        normTime = Mathf.Clamp01(normTime);

        normTime = DOVirtual.EasedValue(0, 1, normTime, Global.instance.buildingDatas.destructionDatas.hideCurve);
        normTime *= -Global.instance.buildingDatas.destructionDatas.hideDistance;

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

    public bool HaveEnded()
    {
        return m_lifeTimer > Global.instance.buildingDatas.destructionDatas.displayDuration + Global.instance.buildingDatas.destructionDatas.hideDuration;
    }
}
