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
    [SerializeField] Team m_defaultTeam = Team.Neutral;
    [SerializeField] GameEntityType m_entityType;

    bool m_added = false;

    Team m_team = Team.Neutral;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GetTeamEvent>.LocalSubscriber(GetTeam, gameObject));
        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Subscribe();

        m_team = m_defaultTeam;
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

    public void SetTeam(Team team)
    {
        m_team = team;
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

    public GameEntityType GetEntityType()
    {
        return m_entityType;
    }

    public JsonObject Save()
    {
        var obj = new JsonObject();

        obj.AddElement("type", GetEntityType().ToString());
        obj.AddElement("team", GetTeam().ToString());
        obj.AddElement("rot", Json.FromQuaternion(transform.rotation));

        var pos = transform.localPosition;
        obj.AddElement("pos", Json.FromVector3Int(new Vector3Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.z))));

        Event<SaveEvent>.Broadcast(new SaveEvent(obj), gameObject);

        return obj;
    }

    public static GameEntity Create(JsonObject obj)
    {
        var typeJson = obj.GetElement("type");
        if (typeJson == null || !typeJson.IsJsonString())
            return null;

        GameEntityType type;
        if (!Enum.TryParse<GameEntityType>(typeJson.String(), out type))
            return null;

        var prefab = Global.instance.editorDatas.GetEntityPrefab(type);
        if (prefab == null)
            return null;

        var instance = Instantiate(prefab);
        instance.transform.parent = EntityList.instance.transform;
        var posJson = obj.GetElement("pos");
        if (posJson != null && posJson.IsJsonArray())
            instance.transform.localPosition = Json.ToVector3Int(posJson.JsonArray());
        var entity = instance.GetComponent<GameEntity>();
        if (entity == null)
        {
            Destroy(instance);
            return null;
        }

        var teamJson = obj.GetElement("team");
        if (teamJson != null && teamJson.IsJsonString())
        {
            Team team;
            if (Enum.TryParse<Team>(teamJson.String(), out team))
                entity.SetTeam(team);
        }
        var rotJson = obj.GetElement("rot");
        if (rotJson != null && rotJson.IsJsonArray())
        {
            var rot = Json.ToQuaternion(rotJson.JsonArray());
            instance.transform.localRotation = rot;
        }

        Event<LoadEvent>.Broadcast(new LoadEvent(obj), instance);

        return entity;
    }
}
