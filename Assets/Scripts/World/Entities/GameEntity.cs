using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameEntity : MonoBehaviour
{
    [SerializeField] string m_name;
    [SerializeField] string m_description;
    [SerializeField] Team m_team = Team.Neutral;

    bool m_added = false;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GetTeamEvent>.LocalSubscriber(GetTeam, gameObject));
        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Subscribe();
    }

    public virtual void OnEnable()
    {
        Add();
    }

    public virtual void OnDisable()
    {
        Remove();
    }

    private void OnDestroy()
    {
        Remove();
        m_subscriberList.Unsubscribe();
    }

    public virtual void Update()
    {
        if (!m_added)
            Add();
    }

    void Add()
    {
        var manager = EntityList.instance;
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

        var manager = EntityList.instance;
        if (manager != null)
            manager.UnRegister(this);

        m_added = false;
    }

    public Team GetTeam()
    {
        return m_team;
    }

    void GetTeam(GetTeamEvent e)
    {
        e.team = GetTeam();
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        UIElementData.Create<UIElementSimpleText>(e.container).SetText(m_name).SetAlignment(UIElementAlignment.center);
        UIElementData.Create<UIElementSpace>(e.container).SetSpace(5);
        UIElementData.Create<UIElementSimpleText>(e.container).SetText(m_description);
    }
}
