using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum QuestSpawnCanvasObjectAnchor
{
    Center,
    Top,
    Left,
    Right,
    Bottom,
    TopLeft,
    TopRight,
    BottomLeft,
    BottomRight,
}

public class QuestSubObjectiveSpawnCanvasObject : QuestSubObjectiveBase
{
    [SerializeField] GameObject m_prefab;
    public GameObject prefab { get { return m_prefab; } set { m_prefab = value; } }

    [SerializeField] string m_name;
    public string name { get { return m_name; } set { m_name = value; } }

    [SerializeField] Vector3 m_location;
    public Vector3 location { get { return m_location; } set { m_location = value; } }

    [SerializeField] QuestSpawnCanvasObjectAnchor m_anchor = QuestSpawnCanvasObjectAnchor.Center;
    public QuestSpawnCanvasObjectAnchor anchor { get { return m_anchor; } set { m_anchor = value; } }

    [SerializeField] bool m_waitTaskComplete = false;
    public bool waitTaskComplete { get { return m_waitTaskComplete; } set { m_waitTaskComplete = value; } }

    [SerializeField] bool m_destroyOnObjectiveComplete = false;
    public bool destroyOnObjectiveComplete { get { return m_destroyOnObjectiveComplete; } set { m_destroyOnObjectiveComplete = value; } }

    GameObject m_obj = null;
    NamedQuestObject m_instance = null;

    public override bool IsCompleted()
    {
        if (!m_waitTaskComplete)
            return true;

        if (m_instance == null)
            return true;

        return m_instance.IsTaskComplete();
    }

    public override void Start()
    {
        if (m_prefab == null)
            return;

        var canvas = Event<GetCanvasEvent>.Broadcast(new GetCanvasEvent()).canvas;
        if (canvas == null)
            return;

        m_obj = GameObject.Instantiate(m_prefab);
        m_obj.transform.SetParent(canvas.transform, false);

        var rectTransform = m_obj.GetComponent<RectTransform>();
        if(rectTransform != null)
        {
            Vector2 anchor = GetAnchor();
            rectTransform.anchorMin = anchor;
            rectTransform.anchorMax = anchor;

            rectTransform.anchoredPosition = m_location;
            rectTransform.localRotation = Quaternion.identity;
            rectTransform.localScale = Vector3.one;
        }

        m_instance = m_obj.GetComponent<NamedQuestObject>();
        if (m_instance != null)
            m_instance.SetName(m_name);
    }

    public override void Update(float deltaTime) { }
    
    public override void End()
    {
        if (m_destroyOnObjectiveComplete && m_obj != null)
            GameObject.Destroy(m_obj);
    }

    Vector2 GetAnchor()
    {
        switch(m_anchor)
        {
            case QuestSpawnCanvasObjectAnchor.Center:
                return new Vector2(0.5f, 0.5f);
            case QuestSpawnCanvasObjectAnchor.Top:
                return new Vector2(0.5f, 1);
            case QuestSpawnCanvasObjectAnchor.Left:
                return new Vector2(0, 0.5f);
            case QuestSpawnCanvasObjectAnchor.Right:
                return new Vector2(1, 0.5f);
            case QuestSpawnCanvasObjectAnchor.Bottom:
                return new Vector2(0.5f, 0);
            case QuestSpawnCanvasObjectAnchor.TopLeft:
                return new Vector2(0, 1);
            case QuestSpawnCanvasObjectAnchor.TopRight:
                return new Vector2(1, 1);
            case QuestSpawnCanvasObjectAnchor.BottomLeft:
                return new Vector2(0, 0);
            case QuestSpawnCanvasObjectAnchor.BottomRight:
                return new Vector2(1, 0);
            default:
                break;
        }

        return Vector2.zero;
    }
}
