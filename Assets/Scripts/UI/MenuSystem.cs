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

    [SerializeField] List<MenuData> m_menus = new List<MenuData>();

    List<MenuData> m_openMenus = new List<MenuData>();

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

    public T OpenMenu<T>(string name, bool overrideOpen = false, bool returnOpen = false) where T : Component
    {
        GameObject openMenu = null;
        foreach (var w in m_openMenus)
        {
            if (w.name == name)
            {
                openMenu = w.menu;
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

        var camera = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());

        var menu = Instantiate(prefab, transform);

        var canvas = menu.GetComponent<Canvas>();
        if (camera.UICamera != null && canvas != null)
        {
            canvas.worldCamera = camera.UICamera;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.planeDistance = 1;
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
            if (m.name == name)
            {
                m.menu = menu;
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
            m_openMenus.Add(data);
        }

        GameInfos.instance.paused = IsPaused();

        return comp;
    }

    public bool CloseMenu(string menuName)
    {
        int menuIndex = -1;
        for(int i = 0; i < m_openMenus.Count; i++)
        {
            if(m_openMenus[i].name == menuName)
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
            if (m_openMenus[i].menu == null)
                continue;

            var comp = m_openMenus[i].menu.GetComponentInChildren<T>();
            if (comp != null)
                menuIndex = i;
        }

        return RemoveAt(menuIndex);
    }

    public T GetOpenedMenu<T>() where T : Component
    {
        for (int i = 0; i < m_openMenus.Count; i++)
        {
            if (m_openMenus[i].menu == null)
                continue;

            var comp = m_openMenus[i].menu.GetComponentInChildren<T>();
            if (comp != null)
                return comp;
        }

        return null;
    }

    bool RemoveAt(int menuIndex)
    {
        if (menuIndex < 0)
            return false;

        if (m_openMenus[menuIndex].menu != null)
            Destroy(m_openMenus[menuIndex].menu);

        m_openMenus.RemoveAt(menuIndex);

        GameInfos.instance.paused = IsPaused();

        return true;
    }

    public bool IsPaused()
    {
        foreach(var m in m_openMenus)
        {
            if (m.pauseGame)
                return true;
        }
        return false;
    }

    private void Update()
    {
        bool changed = false;
        for(int i = 0; i < m_openMenus.Count; i++)
        {
            if(m_openMenus[i].menu == null)
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

