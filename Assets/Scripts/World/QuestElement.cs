using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum QuestElementType
{
    Point,
    Cuboid,
    Sphere,
}

public class QuestElement : MonoBehaviour
{
    [SerializeField] string m_name;
    [SerializeField] QuestElementType m_elementType;
    [ShowIf("m_elementType", QuestElementType.Cuboid)]
    [SerializeField] Vector3 m_size;
    [ShowIfGroup("m_elementType", QuestElementType.Sphere)]
    [SerializeField] float m_radius;

    GameObject m_visual = null;

    public QuestElementType GetQuestElementType()
    {
        return m_elementType;
    }

    public string GetName()
    {
        return m_name;
    }

    public void SetName(string name)
    {
        m_name = name;
    }

    public Vector3 GetSize()
    {
        if (m_elementType != QuestElementType.Cuboid)
        {
            Debug.LogWarning("Get size was called on a quest element that is not a cuboid");
            return Vector3.zero;
        }

        return m_size;
    }

    public void SetSize(Vector3 size)
    {
        if (m_elementType != QuestElementType.Cuboid)
        {
            Debug.LogWarning("Set size was called on a quest element that is not a cuboid");
            return;
        }

        m_size = size;

        UpdateDisplay();
    }

    public float GetRadius()
    {
        if(m_elementType != QuestElementType.Sphere)
        {
            Debug.LogWarning("Get radius was called on a quest element that is not a sphere");
            return 0;
        }

        return m_radius;
    }

    public void SetRadius(float radius)
    {
        if (m_elementType != QuestElementType.Sphere)
        {
            Debug.LogWarning("Set radius was called on a quest element that is not a sphere");
            return ;
        }

        m_radius = radius;

        UpdateDisplay();
    }

    private void Start()
    {
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if(!CanDraw() || m_elementType == QuestElementType.Point)
        {
            if (m_visual != null)
                Destroy(m_visual);
            return;
        }

        if(m_visual == null)
        {
            m_visual = new GameObject("Visual");
            m_visual.transform.parent = transform;
            m_visual.transform.localPosition = Vector3.zero;
            m_visual.transform.localRotation = Quaternion.identity;
            m_visual.transform.localScale = Vector3.one;
            m_visual.layer = gameObject.layer;

            m_visual.AddComponent<MeshFilter>();

            var renderer = m_visual.AddComponent<MeshRenderer>();
            renderer.material = Global.instance.editorDatas.questElementMaterial;
        }

        var filter = m_visual.GetComponent<MeshFilter>();
        if(filter != null)
        {
            if (filter.mesh != null)
                Destroy(filter.mesh);

            if (m_elementType == QuestElementType.Cuboid)
                filter.mesh = WireframeMesh.SimpleCube(m_size, Color.white);
            else if (m_elementType == QuestElementType.Sphere)
                filter.mesh = WireframeMesh.Sphere(m_radius, 6, 8, Color.white);
        }
    }

    bool CanDraw()
    {
        return EditorGridBehaviour.instance != null;
    }
}
