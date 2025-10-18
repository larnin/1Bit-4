using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class ProjectileFrozen : ProjectileBase
{
    [SerializeField] float m_speed = 5;
    [SerializeField] float m_maxLife = 5;
    [SerializeField] LayerMask m_hitLayer;
    [SerializeField] LayerMask m_groundLayer;
    [SerializeField] LayerMask m_explosionLayer;
    [SerializeField] float m_explosionDuration = 1;
    [SerializeField] float m_explosionRadius = 3;
    [SerializeField] Ease m_explosionCurve = Ease.Linear;
    [SerializeField] float m_explosionFadeEndPercent = 0.8f;
    [SerializeField] string m_explosionSound;
    [SerializeField] float m_explosionSoundVolume = 1;

    enum State
    {
        Move,
        Explosion,
        WaitEnd
    }

    float m_time = 0;
    State m_state = State.Move;

    Transform m_projectile;
    Transform m_explosion;
    Renderer m_explosionRenderer;
    Material m_explosionMaterial;
    Color m_explosionInitialColor;
    const string m_colorName = "_RimColor";
    List<GameObject> m_hitEntities = new List<GameObject>();

    List<Guid> m_hitEntitiesSave = new List<Guid>();

    private void Awake()
    {
        m_projectile = transform.Find("Projectile");
        m_explosion = transform.Find("Explosion");
        if(m_explosion != null)
        {
            m_explosionRenderer = m_explosion.GetComponentInChildren<Renderer>();
            if(m_explosionRenderer != null)
            {
                m_explosionMaterial = m_explosionRenderer.material;
                if(m_explosionMaterial != null)
                {
                    m_explosionInitialColor = m_explosionMaterial.GetColor(m_colorName);
                    m_explosionInitialColor.a = 1;
                    m_explosionMaterial.SetColor(m_colorName, m_explosionInitialColor);
                }
            }

            m_explosion.gameObject.SetActive(false);
        }
    }

    protected override void Start()
    {
        base.Start();

        if(m_hitEntitiesSave.Count > 0 && IDList.instance != null)
        {
            m_hitEntities.Clear();

            foreach(var e in m_hitEntitiesSave)
            {
                var entity = IDList.instance.GetEntityFromID(e);
                if (entity != null)
                    m_hitEntities.Add(entity.gameObject);
            }

            m_hitEntitiesSave.Clear();
        }
    }

    protected override void Update()
    {
        switch(m_state)
        {
            case State.Move:
                UpdateMoving();
                break;
            case State.Explosion:
                UpdateExplosion();
                break;
            case State.WaitEnd:
                UpdateEnd();
                break;

        }
    }

    void UpdateMoving()
    {
        if (GameInfos.instance.paused)
            return;

        var dir = transform.forward;

        var nextPos = transform.position + dir * Time.deltaTime * m_speed;
        var ray = new Ray(transform.position, dir);

        RaycastHit hit;
        var haveHit = Physics.Raycast(ray, out hit, Time.deltaTime * m_speed + 0.01f, m_hitLayer);
        if (haveHit)
            StartExplosion(hit);

        transform.position = nextPos;

        m_time += Time.deltaTime;
        if (m_time > m_maxLife)
            StartExplosion(hit);
    }

    void UpdateExplosion()
    {
        if (GameInfos.instance.paused)
            return;

        m_time += Time.deltaTime;
        if(m_time >= m_explosionDuration)
        {
            StartEnd();
            return;
        }

        float radius = DOVirtual.EasedValue(0, m_explosionRadius, m_time / m_explosionDuration, m_explosionCurve);

        var cols = Physics.OverlapSphere(transform.position, radius / 2, m_explosionLayer);
        foreach (var col in cols)
        {
            if (m_hitEntities.Contains(col.gameObject))
                continue;
            Event<HitEvent>.Broadcast(new HitEvent(new Hit(m_damages * m_damagesMultiplier, m_caster, m_damageType, m_damageEffect)), col.gameObject);
            m_hitEntities.Add(col.gameObject);
        }

        UpdateExplosionRender();
    }

    void UpdateEnd()
    {
        m_time += Time.deltaTime;
        if (m_time >= 2)
            Destroy(gameObject);
    }

    void StartExplosion(RaycastHit hit)
    {
        bool startExplosion = true;

        if (hit.collider != null)
        {
            startExplosion = false;
            if ((m_groundLayer.value & (1 << hit.collider.gameObject.layer)) != 0)
                startExplosion = true;
            else
            {
                var team = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), hit.collider.gameObject);

                if (TeamEx.GetOppositeTeam(team.team) == m_casterTeam)
                    startExplosion = true;
            }
        }

        if (startExplosion)
        {
            m_projectile.gameObject.SetActive(false);
            m_explosion.gameObject.SetActive(true);

            m_state = State.Explosion;
            m_time = 0;

            UpdateExplosionRender();

            if(SoundSystem.instance != null)
            {
                SoundSystem.instance.PlaySound(m_explosionSound, transform.position, m_explosionSoundVolume);
            }
        }
    }

    void StartEnd()
    {
        m_time = 0;
        m_state = State.WaitEnd;

        m_projectile.gameObject.SetActive(false);
        m_explosion.gameObject.SetActive(false);
    }

    void UpdateExplosionRender()
    {
        float normTime = m_time / m_explosionDuration;

        float radius = DOVirtual.EasedValue(0, m_explosionRadius, normTime, m_explosionCurve);
        m_explosion.localScale = Vector3.one * radius;

        Color c = m_explosionInitialColor;

        if(normTime > m_explosionFadeEndPercent)
        {
            float percent = (normTime - m_explosionFadeEndPercent) / (1 - m_explosionFadeEndPercent);
            c.a = 1 - percent;
            m_explosionMaterial.SetColor(m_colorName, c);
            m_explosionRenderer.material = m_explosionMaterial;
        }
    }

    protected override void SaveImpl(JsonObject obj)
    {
        obj.AddElement("time", m_time);
        obj.AddElement("state", m_state.ToString());

        if (m_hitEntities.Count > 0)
        {
            var array = new JsonArray();
            obj.AddElement("hit", array);

            foreach(var e in m_hitEntities)
            {
                var id = Event<GetEntityIDEvent>.Broadcast(new GetEntityIDEvent(), e).id;
                if (id != Guid.Empty)
                    array.Add(id.ToString());
            }
        }
    }

    protected override void LoadImpl(JsonObject obj)
    {
        var timeJson = obj.GetElement("time");
        if (timeJson != null && timeJson.IsJsonNumber())
            m_time = timeJson.Float();

        var stateJson = obj.GetElement("state");
        if (stateJson != null && stateJson.IsJsonString())
            Enum.TryParse<State>(stateJson.String(), out m_state);

        m_hitEntitiesSave.Clear();
        var hitJson = obj.GetElement("hit");
        if(hitJson != null && hitJson.IsJsonArray())
        {
            foreach(var e in hitJson.JsonArray())
            {
                if(e.IsJsonString())
                {
                    Guid id = Guid.Empty;
                    Guid.TryParse(e.String(), out id);
                    if (id != Guid.Empty)
                        m_hitEntitiesSave.Add(id);
                }
            }
        }
    }
}
