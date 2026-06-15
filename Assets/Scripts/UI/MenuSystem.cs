using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class MenuSystem : MonoBehaviour
{
    [Serializable]
    class MenuData
    {
        public string name;
        public GameObject menu;
        public bool pauseGame;
    }

    class OpenMenuData
    {
        public MenuData menuData;
        public GameObject holder;
    }

    [SerializeField] GameObject m_menuHolder;
    [SerializeField] List<MenuData> m_menus = new List<MenuData>();

    List<OpenMenuData> m_openMenus = new List<OpenMenuData>();

    static MenuSystem m_instance = null;
    public static MenuSystem instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;

        GameInfos.instance.paused = false;
    }

    public T OpenMenu<T>(string name, bool overrideOpen = false, bool returnOpen = false, bool withHolder = true) where T : Component
    {
        GameObject openMenu = null;
        foreach (var w in m_openMenus)
        {
            if (w.menuData.name == name)
            {
                openMenu = w.menuData.menu;
                break;
            }
        }

        if (openMenu != null)
        {
            if (overrideOpen)
                CloseMenu(name);
            else if (returnOpen)
                return openMenu.GetComponentInChildren<T>();
            else return default(T);
        }

        GameObject prefab = null;
        bool paused = false;
        foreach (var m in m_menus)
        {
            if (m.name == name)
            {
                prefab = m.menu;
                paused = m.pauseGame;
                break;
            }
        }
        if (prefab == null)
        {
            Debug.LogError("No menu named " + name);
            return default(T);
        }

        var menu = Instantiate(prefab);
        GameObject holder = null;

        if (withHolder)
        {
            holder = Instantiate(m_menuHolder, transform);

            var camera = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());

            var canvas = holder.GetComponent<Canvas>();
            if (camera.UICamera != null && canvas != null)
            {
                canvas.worldCamera = camera.UICamera;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                canvas.planeDistance = 1;
            }

            if (canvas != null)
            {
                var pivot = canvas.transform.Find("Pivot");
                if (pivot != null)
                {
                    menu.transform.SetParent(pivot.transform, false);
                    var tr = menu.GetComponent<RectTransform>();
                    if (tr != null)
                    {
                        tr.localPosition = Vector3.zero;
                        tr.localRotation = Quaternion.identity;
                        tr.localScale = Vector3.one;
                        tr.offsetMin = Vector2.zero;
                        tr.offsetMax = Vector2.zero;

                    }
                    
                }
            }

        }

        T comp = menu.GetComponentInChildren<T>();
        if (comp == null)
        {
            Debug.LogError("No component " + typeof(T).Name + " in menu " + name);
            Destroy(menu);
            return comp;
        }

        bool found = false;
        foreach (var m in m_openMenus)
        {
            if (m.menuData.name == name)
            {
                m.menuData.menu = menu;
                m.holder = holder;
                found = true;
                break;
            }
        }
        if (!found)
        {
            MenuData data = new MenuData();
            data.name = name;
            data.menu = menu;
            data.pauseGame = paused;
            OpenMenuData openData = new OpenMenuData();
            openData.menuData = data;
            openData.holder = holder;
            m_openMenus.Add(openData);
        }

        GameInfos.instance.paused = IsPaused();

        return comp;
    }

    public bool CloseMenu(string menuName)
    {
        int menuIndex = -1;
        for(int i = 0; i < m_openMenus.Count; i++)
        {
            if(m_openMenus[i].menuData.name == menuName)
            {
                menuIndex = i;
                break;
            }
        }
        return RemoveAt(menuIndex);
    }

    public bool CloseMenu<T>() where T : Component
    {
        int menuIndex = -1;
        for (int i = 0; i < m_openMenus.Count; i++)
        {
            if (m_openMenus[i].menuData.menu == null)
                continue;

            var comp = m_openMenus[i].menuData.menu.GetComponentInChildren<T>();
            if (comp != null)
                menuIndex = i;
        }

        return RemoveAt(menuIndex);
    }

    public T GetOpenedMenu<T>() where T : Component
    {
        for (int i = 0; i < m_openMenus.Count; i++)
        {
            if (m_openMenus[i].menuData.menu == null)
                continue;

            var comp = m_openMenus[i].menuData.menu.GetComponentInChildren<T>();
            if (comp != null)
                return comp;
        }

        return null;
    }

    bool RemoveAt(int menuIndex)
    {
        if (menuIndex < 0)
            return false;

        if (m_openMenus[menuIndex].menuData.menu != null)
            Destroy(m_openMenus[menuIndex].menuData.menu);

        m_openMenus.RemoveAt(menuIndex);

        GameInfos.instance.paused = IsPaused();

        return true;
    }

    public bool IsPaused()
    {
        foreach(var m in m_openMenus)
        {
            if (m.menuData.pauseGame)
                return true;
        }
        return false;
    }

    private void Update()
    {
        bool changed = false;
        for(int i = 0; i < m_openMenus.Count; i++)
        {
            if(m_openMenus[i].menuData.menu == null)
            {
                changed = true;
                m_openMenus.RemoveAt(i);
                i--;
            }
        }

        if(changed)
            GameInfos.instance.paused = IsPaused();
    }
}

