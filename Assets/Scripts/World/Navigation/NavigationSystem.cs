using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NavigationSystem : MonoBehaviour
{
    [SerializeField] bool m_debugDraw = false;

    Dictionary<string, NavigationSurface> m_surfaces = new Dictionary<string, NavigationSurface>();

    SubscriberList m_subscriberList = new SubscriberList();

    bool m_generationEnded = false;

    static NavigationSystem m_instance = null;
    public static NavigationSystem instance { get { return m_instance; } }

    private void Awake()
    {
        m_instance = this;

        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(OnGenerationEnd));
        m_subscriberList.Add(new Event<BuildingListAddEvent>.Subscriber(OnBuildingAdd));
        m_subscriberList.Add(new Event<BuildingListRemoveEvent>.Subscriber(OnBuildingRemove));
        m_subscriberList.Add(new Event<OnBuildingDestroyEvent>.Subscriber(OnBuildingDeath));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        if (m_instance == this)
            m_instance = null;

        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        int profileNb = Global.instance.editorDatas.navigationProfiles.Count;

        for(int i = 0; i < profileNb; i++)
        {
            var nav = new NavigationSurface();
            nav.profile = Global.instance.editorDatas.navigationProfiles[i].profile;
            m_surfaces.Add(Global.instance.editorDatas.navigationProfiles[i].name, nav);
        }
    }

    void OnGenerationEnd(GenerationFinishedEvent e)
    {
        m_generationEnded = true;
        NeedRebuild();
    }

    void OnBuildingAdd(BuildingListAddEvent e)
    {
        NeedRebuild();
    }

    void OnBuildingRemove(BuildingListRemoveEvent e)
    {
        NeedRebuild();
    }

    void OnBuildingDeath(OnBuildingDestroyEvent e)
    {
        NeedRebuild();
    }

    void NeedRebuild()
    {
        if (!m_generationEnded)
            return;

        foreach(var surface in m_surfaces)
        {
            surface.Value.Rebuild();
        }
    }

    public NavigationSurface GetSurface(string profile)
    {
        NavigationSurface surface;
        m_surfaces.TryGetValue(profile, out surface);

        return surface;
    }

    private void Update()
    {
        if(m_debugDraw)
        {
            if (m_surfaces.Count == 0)
                return;

            NavigationSurface surface = m_surfaces.First().Value;

            //surface.DebugDrawGrid();

            var cam = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());
            if (cam.camera != null)
            {
                Vector3 pos;
                var ray = cam.camera.ScreenPointToRay(Input.mousePosition);
                if(PlaceBuildingCursor.LoopCursorRatcast(ray, Global.instance.editorDatas.groundLayer, out pos))
                {
                    Vector2Int startPos = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));

                    surface.DebugDrawPathFromPos(startPos, 1, -1, Color.red);
                    surface.DebugDrawPathFromPos(startPos, 1, -0.5f, Color.yellow);
                    surface.DebugDrawPathFromPos(startPos, 1, 0, Color.green);
                    surface.DebugDrawPathFromPos(startPos, 1, 0.5f, Color.cyan);
                    surface.DebugDrawPathFromPos(startPos, 1, 1, Color.blue);
                }
            }
        }
    }
}
