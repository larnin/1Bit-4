using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DisplayIconsV2 : MonoBehaviour
{
    [Serializable]
    class IconInfos
    {
        public string name;
        public Sprite icon;
    }

    class IconDatas
    {
        public GameObject target;
        public Vector3 position;
        public float offset;
        public Sprite icon;
        public string text;
        public bool flashing;
        public bool displayOutOfScreen;
        public List<OneIconV2> instances = new List<OneIconV2>();
        public float duration;
    }

    [SerializeField] List<IconInfos> m_icons;
    [SerializeField] GameObject m_iconPrefab;
    [SerializeField] float m_screenBorder;

    List<IconDatas> m_displayList = new List<IconDatas>();
    List<OneIconV2> m_poolList = new List<OneIconV2>();

    static DisplayIconsV2 m_instance = null;
    public static DisplayIconsV2 instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;
    }

    public void Register(GameObject target, float offset, string iconName, string text = "", bool displayOutOfScreen = false, bool flash = false)
    {
        IconDatas datas = null;
        foreach (var d in m_displayList)
        {
            if (d.target == target)
            {
                datas = d;
                break;
            }
        }

        if (datas == null)
        {
            m_displayList.Add(new IconDatas());
            datas = m_displayList[m_displayList.Count - 1];
        }

        Sprite sprite = GetSprite(iconName);

        datas.target = target;
        datas.offset = offset;
        datas.icon = sprite;
        datas.position = Vector3.zero;
        datas.duration = -1;
        datas.text = text;
        datas.flashing = flash;
        datas.displayOutOfScreen = displayOutOfScreen;
    }

    public void Register(Vector3 position, float offset, float duration, string iconName, string text = "", bool displayOutOfScreen = false, bool flash = false)
    {
        float maxDistance = 0.5f;

        IconDatas datas = null;
        float dist = 0;
        foreach (var d in m_displayList)
        {
            if (d.target != null)
                continue;

            float currentDist = (position - d.position).sqrMagnitude;
            if (currentDist > maxDistance * maxDistance)
                continue;

            if (datas != null && dist < currentDist)
                continue;

            dist = currentDist;
            datas = d;
        }

        if (datas == null)
        {
            m_displayList.Add(new IconDatas());
            datas = m_displayList[m_displayList.Count - 1];
        }

        Sprite sprite = GetSprite(iconName);

        datas.target = null;
        datas.offset = offset;
        datas.icon = sprite;
        datas.position = position;
        datas.duration = duration;
        datas.text = text;
        datas.flashing = flash;
        datas.displayOutOfScreen = displayOutOfScreen;
    }

    public void Unregister(GameObject target)
    {
        for (int i = 0; i < m_displayList.Count; i++)
        {
            if (m_displayList[i].target == target)
            {
                foreach(var icon in m_displayList[i].instances)
                {
                    AddToPool(icon);
                }

                m_displayList.RemoveAt(i);
                i--;
            }
        }
    }

    Sprite GetSprite(string name)
    {
        foreach (var s in m_icons)
        {
            if (s.name == name)
                return s.icon;
        }

        return null;
    }

    void AddToPool(OneIconV2 icon)
    {
        m_poolList.Add(icon);
        icon.gameObject.SetActive(false);
    }

    OneIconV2 GetIconFromPool()
    {
        if(m_poolList.Count == 0)
        {
            var obj = Instantiate(m_iconPrefab);
            var icon = obj.GetComponent<OneIconV2>();
            if(icon == null)
            {
                Destroy(obj);
                return null;
            }

            obj.transform.SetParent(transform, false);

            return icon;
        }

        var poolItem = m_poolList[m_poolList.Count - 1];
        m_poolList.RemoveAt(m_poolList.Count - 1);

        poolItem.gameObject.SetActive(true);

        return poolItem;
    }

    private void Update()
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;
        var size = GridEx.GetRealSize(grid.grid);

        var dups = Event<GetCameraDuplicationEvent>.Broadcast(new GetCameraDuplicationEvent());

        var camera = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());

        float width = Screen.width;
        float height = Screen.height;

        Color flashColor = Color.white;
        {
            float t = Time.time;
            t = t % 1;
            t *= 3.0f;
            if (t > 1)
                t = 1;
            flashColor.a = t;
        }

        for (int i = 0; i < m_displayList.Count; i++)
        {
            var item = m_displayList[i];

            if(item.duration > 0)
            {
                item.duration -= Time.deltaTime;
                if(item.duration <= 0)
                {
                    foreach (var icon in item.instances)
                    {
                        AddToPool(icon);
                    }

                    m_displayList.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            bool isVisible = false;
            List<Vector2> validPoints = new List<Vector2>();

            Vector3 pos = GetTargetPosition(item);

            foreach (var d in dups.duplications)
            {
                var loopPos = pos - size * new Vector3(d.x, 0, d.y);
                Vector2 screenPos = camera.UICamera.WorldToScreenPoint(loopPos);

                bool isInsideScreen = false;
                if (item.displayOutOfScreen)
                    isInsideScreen = screenPos.x > m_screenBorder && screenPos.y > m_screenBorder && screenPos.x < width - m_screenBorder && screenPos.y < height - m_screenBorder;
                else isInsideScreen = screenPos.x > 0 && screenPos.y > 0 && screenPos.x < width && screenPos.y < height;

                if (isInsideScreen && !isVisible)
                    validPoints.Clear();
                if (isInsideScreen || !isVisible)
                    validPoints.Add(screenPos);

                isVisible |= isInsideScreen;
            }

            if (!isVisible && !item.displayOutOfScreen)
                validPoints.Clear();

            if(!isVisible && validPoints.Count > 0)
            {
                float bestDistance = float.MaxValue;
                Vector2 bestPos = Vector2.zero;

                foreach(var p in validPoints)
                {
                    var tempPos = p;
                    if (tempPos.x > m_screenBorder)
                        tempPos.x -= width - (2 * m_screenBorder);
                    if (tempPos.y > m_screenBorder)
                        tempPos.y -= height - (2 * m_screenBorder);

                    tempPos -= new Vector2(m_screenBorder, m_screenBorder);
                    float dist = tempPos.sqrMagnitude;
                    if(dist < bestDistance)
                    {
                        bestDistance = dist;
                        bestPos = p;
                    }
                }

                validPoints.Clear();
                validPoints.Add(ProjectOnBorder(bestPos));
            }

            while (item.instances.Count() < validPoints.Count)
                item.instances.Add(GetIconFromPool());
            while(item.instances.Count > validPoints.Count)
            {
                AddToPool(item.instances[item.instances.Count - 1]);
                item.instances.RemoveAt(item.instances.Count - 1);
            }

            for(int j = 0; j < item.instances.Count; j++)
                item.instances[j].SetData(validPoints[j], item.icon, !isVisible, item.flashing ? flashColor : Color.white, item.text, camera.UICamera);
        }
    }

    Vector3 GetTargetPosition(IconDatas data)
    {
        if (data == null)
            return Vector3.zero;

        if (data.target == null)
            return data.position + new Vector3(0, data.offset, 0);

        var pos = data.target.transform.position;

        var targetType = GameSystem.GetEntityType(data.target);
        if (targetType == EntityType.Building)
        {
            var building = data.target.GetComponent<BuildingBase>();
            if (building != null)
                pos = building.GetGroundCenter();
        }

        return pos + new Vector3(0, data.offset, 0);
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

        for (int i = 0; i < points.Length; i++)
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
}
