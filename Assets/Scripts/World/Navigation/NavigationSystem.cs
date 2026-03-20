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

    private void Awake()
    {
        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(OnGenerationEnd));
        m_subscriberList.Add(new Event<BuildingListAddEvent>.Subscriber(OnBuildingAdd));
        m_subscriberList.Add(new Event<BuildingListRemoveEvent>.Subscriber(OnBuildingRemove));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
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

    void NeedRebuild()
    {
        if (!m_generationEnded)
            return;

        foreach(var surface in m_surfaces)
        {
            surface.Value.Rebuild();
        }
    }

    private void Update()
    {
        if(m_debugDraw)
        {
            if (m_surfaces.Count == 0)
                return;

            NavigationSurface surface = m_surfaces.First().Value;

            surface.DebugDrawGrid();
        }
    }
}
