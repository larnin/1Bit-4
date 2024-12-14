using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class IconDisplayInfos
{
    public GameObject target;
    public Vector3 position;
    public float offset;
    public float duration;
    public Sprite sprite;
    public bool flashing;
    public string text;
    public bool displayIfOutOfScreen;
    public OneIcon instance;
}

public class DisplayIcons : MonoBehaviour
{
    [Serializable]
    class IconInfos
    {
        public string name;
        public Sprite icon;
    }

    [SerializeField] List<IconInfos> m_icons;
    [SerializeField] GameObject m_iconPrefab;

    static DisplayIcons m_instance = null;
    public static DisplayIcons instance { get { return m_instance; } }

    List<IconDisplayInfos> m_displayList = new List<IconDisplayInfos>();

    private void Awake()
    {
        m_instance = this;
    }

    public void Register(GameObject target, float offset, string iconName, string text = "", bool displayOutOfScreen = false, bool flash = false)
    {
        IconDisplayInfos infos = null;
        foreach(var d in m_displayList)
        {
            if(d.target == target)
            {
                infos = d;
                break;
            }
        }

        if(infos == null)
        {
            m_displayList.Add(new IconDisplayInfos());
            infos = m_displayList[m_displayList.Count - 1];
        }

        Sprite sprite = GetSprite(iconName);

        infos.target = target;
        infos.offset = offset;
        infos.sprite = sprite;
        infos.position = Vector3.zero;
        infos.duration = -1;
        infos.text = text;
        infos.flashing = flash;
        infos.displayIfOutOfScreen = displayOutOfScreen;
    }

    public void Register(Vector3 position, float offset, float duration, string iconName, string text = "", bool displayOutOfScreen = false, bool flash = false)
    {
        float maxDistance = 0.5f;

        IconDisplayInfos infos = null;
        float dist = 0;
        foreach (var d in m_displayList)
        {
            if (d.target != null)
                continue;

            float currentDist = (position - d.position).sqrMagnitude;
            if (currentDist > maxDistance * maxDistance)
                continue;

            if (infos != null && dist < currentDist)
                continue;

            dist = currentDist;
            infos = d;
        }

        if (infos == null)
        {
            m_displayList.Add(new IconDisplayInfos());
            infos = m_displayList[m_displayList.Count - 1];
        }

        Sprite sprite = GetSprite(iconName);

        infos.target = null;
        infos.offset = offset;
        infos.sprite = sprite;
        infos.position = position;
        infos.duration = duration;
        infos.text = text;
        infos.flashing = flash;
        infos.displayIfOutOfScreen = displayOutOfScreen;
    }

    public void Unregister(GameObject target)
    {
        for(int i = 0; i < m_displayList.Count; i++)
        {
            if(m_displayList[i].target == target)
            {
                if (m_displayList[i].instance != null)
                    Destroy(m_displayList[i].instance.gameObject);
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

    private void Update()
    {
        List<IconDisplayInfos> toRemove = new List<IconDisplayInfos>();

        foreach(var d in m_displayList)
        {
            if (d.duration > 0)
            {
                d.duration -= Time.deltaTime;
                if (d.duration <= 0)
                    toRemove.Add(d);
            }
            else if (d.target == null)
                toRemove.Add(d);

            if(d.instance == null)
            {
                var obj = Instantiate(m_iconPrefab);
                obj.transform.SetParent(transform, false);

                var icon = obj.GetComponent<OneIcon>();
                if (icon == null)
                    Destroy(obj);
                else d.instance = icon;
            }

            if (d.instance != null)
                d.instance.SetDatas(d);
        }

        foreach(var d in toRemove)
        {
            if (d.instance != null)
                Destroy(d.instance.gameObject);
            m_displayList.Remove(d);
        }
    }
}
