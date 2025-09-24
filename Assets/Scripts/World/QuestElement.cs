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

    bool m_added = false;
    bool m_isCursor = false;

    private void Awake()
    {
        Add();
    }

    private void OnDestroy()
    {
        Remove();
    }

    public void SetAsCursor(bool asCursor)
    {
        m_isCursor = asCursor;

        if (!m_isCursor)
            Add();
        else Remove();
    }

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

        UpdateCollider();
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

        UpdateCollider();
        UpdateDisplay();
    }

    private void Start()
    {
        UpdateCollider();
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if(!CanDraw())
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
                filter.mesh = WireframeMesh.Sphere(m_radius, 4, 6, Color.white);
            else if (m_elementType == QuestElementType.Point)
                filter.mesh = WireframeMesh.Cross(Vector3.one * 2, Color.white);
        }
    }

    void UpdateCollider()
    {
        if(!CanDraw())
        {
            var tempCollider = GetComponent<Collider>();
            if (tempCollider != null)
                Destroy(tempCollider);
        }

        var collider = GetComponent<Collider>();

        if(m_elementType == QuestElementType.Cuboid || m_elementType == QuestElementType.Point)
        {
            var cubeCollider = collider as BoxCollider;
            if (cubeCollider == null && collider != null)
                Destroy(collider);
            if (cubeCollider == null)
                cubeCollider = gameObject.AddComponent<BoxCollider>();

            if (m_elementType == QuestElementType.Cuboid)
                cubeCollider.size = m_size;
            else cubeCollider.size = new Vector3(2, 2, 2);
        }
        else if(m_elementType == QuestElementType.Sphere)
        {
            var sphereCollider = collider as SphereCollider;
            if (sphereCollider == null && collider != null)
                Destroy(collider);
            if (sphereCollider == null)
                sphereCollider = gameObject.AddComponent<SphereCollider>();

            sphereCollider.radius = m_radius;
        }
    }

    bool CanDraw()
    {
        return EditorGridBehaviour.instance != null;
    }

    void Add()
    {
        if (m_isCursor)
            return;

        var manager = QuestElementList.instance;
        if (manager != null)
        {
            m_added = true;
            manager.Register(this);
        }
    }

    void Remove()
    {
        if (!m_added)
            return;

        var manager = QuestElementList.instance;
        if (manager != null)
            manager.UnRegister(this);

        m_added = false;
    }

    public bool IsPosOnTrigger(Vector3 pos)
    {
        if (m_elementType == QuestElementType.Point)
            return false;

        if(m_elementType == QuestElementType.Sphere)
        {
            float sqrDir = (pos - transform.position).sqrMagnitude;
            return sqrDir <= m_radius * m_radius;
        }

        if(m_elementType == QuestElementType.Cuboid)
        {
            var bounds = new Bounds(transform.position, m_size);
            return bounds.Contains(pos);
        }

        return false;
    }

    public JsonObject Save()
    {
        var obj = new JsonObject();

        obj.AddElement("type", m_elementType.ToString());

        obj.AddElement("pos", Json.FromVector3(transform.position));

        if(m_elementType == QuestElementType.Cuboid)
        {
            obj.AddElement("size", Json.FromVector3(m_size));
            float rot = transform.rotation.eulerAngles.y;
            obj.AddElement("rot", rot);
        }
        else if(m_elementType == QuestElementType.Sphere)
        {
            obj.AddElement("radius", m_radius);
        }

        return obj;
    }

    public static QuestElement Create(JsonObject obj)
    {
        QuestElementType type = QuestElementType.Point;
        var jsonType = obj.GetElement("type");
        if (!jsonType.IsJsonString())
            return null;
        Enum.TryParse<QuestElementType>(jsonType.String(), out type);

        var prefab = Global.instance.editorDatas.GetQuestElementPrefab(type);
        if (prefab == null)
            return null;

        var instance = GameObject.Instantiate(prefab);
        if (QuestElementList.instance != null)
            instance.transform.parent = QuestElementList.instance.transform;

        var jsonPos = obj.GetElement("pos");
        if(jsonPos.IsJsonArray())
            instance.transform.position = Json.ToVector3(jsonPos.JsonArray());

        var elem = instance.GetComponent<QuestElement>();
        if(elem == null)
        {
            Destroy(instance);
            return null;
        }

        if(type == QuestElementType.Cuboid)
        {
            var jsonSize = obj.GetElement("size");
            if (jsonSize.IsJsonArray())
                elem.SetSize(Json.ToVector3(jsonSize.JsonArray()));

            var jsonRot = obj.GetElement("rot");
            if (jsonRot.IsJsonNumber())
                instance.transform.rotation = Quaternion.Euler(0, jsonRot.Float(), 0);
        }
        else if(type == QuestElementType.Sphere)
        {
            var jsonRadius = obj.GetElement("radius");
            if (jsonRadius.IsJsonNumber())
                elem.SetRadius(jsonRadius.Float());
        }

        return elem;
    }
}
