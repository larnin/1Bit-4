using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using NRand;

public class EnemyBehaviourV2 : MonoBehaviour, EntityMoveTargetInterface
{
    [SerializeField] NavigationProfileChoice m_navigationProfile;

    List<EntityWeaponBase> m_weapons = new List<EntityWeaponBase>();

    SubscriberList m_subscriberList = new SubscriberList();

    float m_deviation = 0;
    int m_seed = 0;

    Vector3Int m_nextPos;
    Vector2Int m_targetPos;
    bool m_needMove = false;

    static int m_lastSeed = 1;

    private void Awake()
    {
        m_subscriberList.Subscribe();

        m_deviation = Rand.UniformFloatDistribution(-1, 1, StaticRandomGenerator<MT19937>.Get());
        m_seed = m_lastSeed;
        m_lastSeed++;
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Start()
    {
        m_weapons = GetComponentsInChildren<EntityWeaponBase>().ToList();
    }

    float GetStopDistance()
    {
        float minDist = -1;
        foreach(var w in m_weapons)
        {
            if (minDist < 0 || w.GetMoveDistance() < minDist)
                minDist = w.GetMoveDistance();
        }

        if (minDist < 0)
            minDist = 0;

        return minDist;
    }

    private void Update()
    {
        if (GameInfos.instance.paused)
            return;

        if (Utility.IsFrozen(gameObject))
            return;

        if (Utility.IsDead(gameObject))
            return;

        if (NavigationSystem.instance == null)
            return;

        var surface = NavigationSystem.instance.GetSurface(m_navigationProfile.GetValue());
        if (surface == null)
        {
            Debug.LogError("Unable to find a surface with name " + m_navigationProfile.GetValue());
            return;
        }

        var pos = transform.position;
        var posI = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));

        var result = surface.QueryNext(posI, m_seed, m_deviation);
        m_nextPos = result.nextPos;
        m_targetPos = result.targetPos;

        UpdateNeedMove();
    }

    void UpdateNeedMove()
    {
        m_needMove = false;

        var target = GetTarget();
        if (target == null)
            return;

        var targetPos = TurretBehaviour.GetTargetCenter(target.gameObject);

        float sqrDist = (transform.position - targetPos).sqrMagnitude;
        float minDist = GetStopDistance();

        m_needMove = sqrDist > minDist * minDist;
    }

    public GameObject GetTarget()
    {
        //todo select other target (enemy entity ...)

        return GetBuildingTarget();
    }

    GameObject GetBuildingTarget()
    {
        if (m_targetPos.x < 0 || m_targetPos.y < 0)
            return null;

        if (BuildingList.instance == null)
            return null;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent()).grid;
        if (grid == null)
            return null;

        int height = GridEx.GetHeight(grid, m_targetPos);
        if (height < 0)
            return null;

        var building = BuildingList.instance.GetBuildingAt(new Vector3Int(m_targetPos.x, height + 1, m_targetPos.y));
        if (building == null)
            return null;

        return building.gameObject;
    }

    Vector3 EntityMoveTargetInterface.GetNextPos()
    {
        return new Vector3(m_nextPos.x, m_nextPos.y, m_nextPos.z);
    }

    public bool CanMove()
    {
        return m_needMove;
    }

    public bool IsNavigable(Vector3Int pos)
    {
        if (NavigationSystem.instance == null)
            return false;

        var surface = NavigationSystem.instance.GetSurface(m_navigationProfile.GetValue());
        if (surface == null)
        {
            Debug.LogError("Unable to find a surface with name " + m_navigationProfile.GetValue());
            return false;
        }

        var posI = new Vector2Int(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.z));

        return surface.IsNavigable(posI);
    }
}