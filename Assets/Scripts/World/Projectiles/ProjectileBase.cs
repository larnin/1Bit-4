using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum ProjectileType
{

}

public abstract class ProjectileBase : MonoBehaviour
{
    [SerializeField] protected ProjectileType m_projectileType;
    [SerializeField] protected float m_damages = 1;
    [SerializeField] protected DamageType m_damageType = DamageType.Normal;
    [SerializeField] protected float m_damageEffect = 1;

    protected GameObject m_target;
    protected Team m_casterTeam;
    protected GameObject m_caster;
    protected float m_damagesMultiplier = 1;

    Guid m_targetSave = Guid.Empty;
    Guid m_casterSave = Guid.Empty;

    bool m_added = false;

    void OnEnable()
    {
        Add();
    }

    void OnDisable()
    {
        Remove();
    }

    private void OnDestroy()
    {
        Remove();
    }

    protected virtual void Update()
    {
        if (!m_added)
            Add();
    }

    void Add()
    {
        var manager = ProjectileList.instance;
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

        var manager = ProjectileList.instance;
        if (manager != null)
            manager.UnRegister(this);

        m_added = false;
    }

    protected virtual void Start()
    {
        if (IDList.instance != null)
        {
            if (m_targetSave != Guid.Empty)
            {
                var entity = IDList.instance.GetEntityFromID(m_targetSave);
                if (entity != null)
                    m_target = entity.gameObject;
                m_targetSave = Guid.Empty;
            }

            if (m_casterSave != Guid.Empty)
            {
                var entity = IDList.instance.GetEntityFromID(m_casterSave);
                if (entity != null)
                {
                    m_caster = entity.gameObject;
                    m_casterTeam = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), m_caster).team;
                }
                m_casterSave = Guid.Empty;
            }
        }
    }

    public void SetTarget(GameObject target)
    {
        m_target = target;
    }

    public void SetCaster(GameObject caster)
    {
        m_caster = caster;
        m_casterTeam = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), caster).team;
    }

    public void SetDamagesMultiplier(float multiplier)
    {
        m_damagesMultiplier = multiplier;
    }

    public JsonObject Save()
    {
        var obj = new JsonObject();

        obj.AddElement("type", m_projectileType.ToString());
        obj.AddElement("rot", Json.FromQuaternion(transform.localRotation));
        obj.AddElement("pos", Json.FromVector3(transform.localPosition));

        if(m_caster != null)
        {
            var id = Event<GetEntityIDEvent>.Broadcast(new GetEntityIDEvent(), m_caster).id;
            obj.AddElement("caster", id.ToString("N"));
        }

        if(m_target != null)
        {
            var id = Event<GetEntityIDEvent>.Broadcast(new GetEntityIDEvent(), m_target).id;
            obj.AddElement("target", id.ToString("N"));
        }

        obj.AddElement("mul", m_damagesMultiplier);

        SaveImpl(obj);

        Event<SaveEvent>.Broadcast(new SaveEvent(obj), gameObject);

        return obj;
    }

    public static ProjectileBase Create(JsonObject obj)
    {
        var typeJson = obj.GetElement("type");
        if (typeJson == null || !typeJson.IsJsonString())
            return null;

        ProjectileType type;
        if (!Enum.TryParse<ProjectileType>(typeJson.String(), out type))
            return null;

        var prefab = Global.instance.editorDatas.GetProjectilePrefab(type);
        if (prefab == null)
            return null;

        var instance = Instantiate(prefab);
        instance.transform.parent = ProjectileList.instance.transform;
        var projectile = instance.GetComponent<ProjectileBase>();
        if (projectile == null)
        {
            Destroy(instance);
            return null;
        }

        projectile.Load(obj);

        return projectile;
    }

    public void Load(JsonObject obj)
    {
        var posJson = obj.GetElement("pos");
        if (posJson != null && posJson.IsJsonArray())
            transform.localPosition = Json.ToVector3(posJson.JsonArray());

        var rotJson = obj.GetElement("rot");
        if (rotJson != null && rotJson.IsJsonArray())
        {
            var rot = Json.ToQuaternion(rotJson.JsonArray());
            transform.localRotation = rot;
        }

        m_casterSave = Guid.Empty;
        var casterJson = obj.GetElement("caster");
        if (casterJson != null && casterJson.IsJsonString())
            Guid.TryParse(casterJson.String(), out m_casterSave);

        m_targetSave = Guid.Empty;
        var targetJson = obj.GetElement("target");
        if (targetJson != null && targetJson.IsJsonString())
            Guid.TryParse(targetJson.String(), out m_targetSave);

        var mulJson = obj.GetElement("mul");
        if (mulJson != null && mulJson.IsJsonNumber())
            m_damagesMultiplier = mulJson.Float();

        LoadImpl(obj);
        Event<LoadEvent>.Broadcast(new LoadEvent(obj), gameObject);

    }

    protected virtual void LoadImpl(JsonObject obj) { }

    protected virtual void SaveImpl(JsonObject obj) { }
}
