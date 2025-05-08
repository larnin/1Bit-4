using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OneIcon : MonoBehaviour
{
    enum IconElementType
    {
        Arrow,
        Icon,
        Text,
    }

    enum OnScreenType
    {
        OutOfScreen,
        OutOfBorder,
        InBorder
    }

    class IconElementStatus
    {
        public GameObject obj;
        public bool enabled;

        public IconElementStatus(GameObject _obj)
        {
            obj = _obj;
            enabled = obj.activeSelf;
        }
    }
    
    [SerializeField] float m_screenBorder = 50;

    IconDisplayInfos m_datas;

    Transform m_arrow;
    Image m_icon;
    TMP_Text m_text;

    RectTransform m_parent;
    RectTransform m_current;

    List<IconElementStatus> m_status = new List<IconElementStatus>();

    public void SetDatas(IconDisplayInfos infos)
    {
        m_datas = infos;
    }

    private void Awake()
    {
        m_arrow = transform.Find("Arrow");
        var iconTransform = transform.Find("Icon");
        if (iconTransform != null)
            m_icon = iconTransform.GetComponent<Image>();
        var textTransform = transform.Find("Text");
        if (textTransform != null)
            m_text = textTransform.GetComponent<TMP_Text>();

        m_text.raycastTarget = false;

        m_current = GetComponent<RectTransform>();

        m_status.Add(new IconElementStatus(m_arrow.gameObject));
        m_status.Add(new IconElementStatus(m_icon.gameObject));
        m_status.Add(new IconElementStatus(m_text.gameObject));

        SetVisible(false);
    }

    private void LateUpdate()
    {
        if (m_parent == null)
        {
            var parent = transform.parent;
            if (parent != null)
                m_parent = parent.GetComponent<RectTransform>();
        }

        var camera = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
        
        if(camera.UICamera == null || m_parent == null)
        {
            SetVisible(false);
            return;
        }

        var worldPos = GetTargetPosition();
        Vector2 screenPos = camera.UICamera.WorldToScreenPoint(worldPos);

        var onScreen = IsOnScreen(screenPos);

        if(onScreen == OnScreenType.OutOfScreen && !m_datas.displayIfOutOfScreen)
        {
            SetVisible(false);
            return;
        }

        if (m_datas.sprite == null)
            SetVisible(IconElementType.Icon, false);
        else
        {
            SetVisible(IconElementType.Icon, true);
            m_icon.sprite = m_datas.sprite;
            
            Color c = m_icon.color;
            if (m_datas.flashing)
            {
                float t = Time.time;
                t = t % 1;
                t *= 3.0f;
                if (t > 1)
                    t = 1;
                c.a = t;
            }
            else c.a = 1;
            m_icon.color = c;
        }
        if (m_datas.text == null || m_datas.text.Length == 0)
            SetVisible(IconElementType.Text, false);
        else
        {
            SetVisible(IconElementType.Text, true);
            m_text.text = m_datas.text;
        }

        if(onScreen != OnScreenType.InBorder && m_datas.displayIfOutOfScreen)
            screenPos = ProjectOnBorder(screenPos);

        Vector2 transformPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(m_parent, screenPos, camera.UICamera, out transformPoint);

        m_current.anchoredPosition = transformPoint;

        if (onScreen == OnScreenType.InBorder || !m_datas.displayIfOutOfScreen)
            SetVisible(IconElementType.Arrow, false);
        else
        {
            SetVisible(IconElementType.Arrow, true);
            float angle = Vector2.SignedAngle(new Vector2(0, 1), screenPos - new Vector2(Screen.width / 2, Screen.height / 2));
            Quaternion rot = Quaternion.Euler(0, 0, angle);
            m_arrow.localRotation = rot;
        }
    }

    OnScreenType IsOnScreen(Vector2 pos)
    {
        if (pos.x < 0)
            return OnScreenType.OutOfScreen;
        if (pos.y < 0)
            return OnScreenType.OutOfScreen;
        if (pos.x > Screen.width)
            return OnScreenType.OutOfScreen;
        if (pos.y > Screen.height)
            return OnScreenType.OutOfScreen;

        if (pos.x < m_screenBorder)
            return OnScreenType.OutOfBorder;
        if (pos.y < m_screenBorder)
            return OnScreenType.OutOfBorder;
        if (pos.x > Screen.width - m_screenBorder)
            return OnScreenType.OutOfBorder;
        if (pos.y > Screen.height - m_screenBorder)
            return OnScreenType.OutOfBorder;

        return OnScreenType.InBorder;
    }

    Vector2 ProjectOnBorder(Vector2 pos)
    {
        Vector2 min = new Vector2(m_screenBorder, m_screenBorder);
        Vector2 max = new Vector2(Screen.width - m_screenBorder, Screen.height - m_screenBorder);

        Vector2 origin = new Vector2(Screen.width / 2, Screen.height / 2);

        Vector2[] points = new Vector2[]
        {
            Utility.IntersectLines(origin, pos, min, new Vector2(min.x, max.y)),
            Utility.IntersectLines(origin, pos, new Vector2(min.x, max.y), max),
            Utility.IntersectLines(origin, pos, max, new Vector2(max.x, min.y)),
            Utility.IntersectLines(origin, pos, new Vector2(max.x, min.y), min)
        };

        int selectedIndex = -1;

        for(int i = 0; i < points.Length; i++)
        {
            Vector2 p = points[i];
            float d = Vector2.Dot(p - origin, pos - origin);
            if (d < 0)
                continue;

            if (selectedIndex < 0)
            {
                selectedIndex = i;
                continue;
            }

            float bestDist = (points[selectedIndex] - origin).sqrMagnitude;
            float dist = (p - origin).sqrMagnitude;

            if (dist < bestDist)
                selectedIndex = i;
        }

        if (selectedIndex < 0)
            return min;

        return points[selectedIndex];
    }

    Vector3 GetTargetPosition()
    {
        if (m_datas == null)
            return Vector3.zero;

        if (m_datas.target == null)
            return m_datas.position + new Vector3(0, m_datas.offset, 0);

        var pos = m_datas.target.transform.position;

        var targetType = GameSystem.GetEntityType(m_datas.target);
        if(targetType == EntityType.Building)
        {
            var building = m_datas.target.GetComponent<BuildingBase>();
            if (building != null)
                pos = building.GetGroundCenter();
        }

        return pos + new Vector3(0, m_datas.offset, 0);
    }

    void SetVisible(bool visible)
    {
        SetVisible(IconElementType.Arrow, visible);
        SetVisible(IconElementType.Icon, visible);
        SetVisible(IconElementType.Text, visible);
    }

    void SetVisible(IconElementType type, bool visible)
    {
        int index = (int)type;

        if (m_status.Count < index)
            return;

        if (m_status[index].enabled == visible)
            return;

        m_status[index].enabled = visible;
        m_status[index].obj.SetActive(visible);
    }
}
