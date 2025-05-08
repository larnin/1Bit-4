using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityMove : MonoBehaviour
{
    [SerializeField] float m_moveSpeed = 1;
    [SerializeField] float m_acceleration = 1;
    [SerializeField] float m_rotationSpeed = 1;

    EntityPath m_path = new EntityPath();

    float m_speed = 0;
    float m_angle = 0;
    Vector3 m_lastTarget;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<BuildingListAddEvent>.Subscriber(OnAdd));
        m_subscriberList.Add(new Event<BuildingListRemoveEvent>.Subscriber(OnRemove));
        m_subscriberList.Subscribe();

        m_lastTarget = transform.position;
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    public void SetTarget(Vector3 target)
    {
        var team = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), gameObject);
        m_path.SetTarget(transform.position, target, team.team);

        m_lastTarget = target;
    }

    public void Stop()
    {
        var team = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), gameObject);
        m_path.SetTarget(transform.position, transform.position, team.team);
    }

    public bool IsMoving()
    {
        return m_path.GetStatus() != EntityPathStatus.Stopped;
    }

    private void Update()
    {
        if (GameInfos.instance.paused)
            return;

        if (Utility.IsFrozen(gameObject))
            return;

        if (Utility.IsDead(gameObject))
            return;

        Vector3 target = m_path.GetNextPoint(transform.position);

        if (m_path.GetStatus() != EntityPathStatus.Following)
            m_speed -= 2 * m_acceleration;
        else m_speed += m_acceleration;
        if (m_speed < 0)
            m_speed = 0;
        if (m_speed > m_moveSpeed)
            m_speed = m_moveSpeed;

        DebugDraw.Line(transform.position, target, Color.cyan);

        if (m_speed > 0.001f)
        {
            Vector3 dir = target - transform.position;
            float angleDir = Mathf.Atan2(dir.z, dir.x);
            float deltaAngle = angleDir - m_angle;
            while (deltaAngle < -Mathf.PI)
                deltaAngle += Mathf.PI * 2;
            while (deltaAngle > Mathf.PI)
                deltaAngle -= Mathf.PI * 2;

            float angleDist = m_rotationSpeed * Time.deltaTime;
            if (angleDist > Mathf.Abs(deltaAngle))
                angleDist = Mathf.Abs(deltaAngle);
            angleDist *= Mathf.Sign(deltaAngle);

            m_angle += angleDist;

            Vector3 moveDir = new Vector3(Mathf.Cos(m_angle), 0, Mathf.Sin(m_angle));
            Vector3 newPos = transform.position + moveDir * Time.deltaTime * m_speed;
            newPos.y = GetHeight(newPos);

            transform.position = newPos;

            transform.forward = moveDir;
        }
    }

    float GetHeight(Vector3 newPos)
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return newPos.y;

        float radius = 0.4f;

        Vector2[] testPos = new Vector2[]
        {
            Vector2.zero, new Vector2(-radius, -radius), new Vector2(-radius, radius), new Vector2(radius, radius), new Vector2(radius, -radius)
        };

        float top = float.MinValue;
        foreach(var p in testPos)
        {
            Vector2 point = p + new Vector2(newPos.x, newPos.z);

            float height = GridEx.GetHeight(grid.grid, new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y))) + 1;
            if (height > top)
                top = height;
        }

        if (top > newPos.y - 10)
            return top;

        return newPos.y;
    }

    void OnAdd(BuildingListAddEvent e)
    {
        CheckNeedRebuildIfAround(e.building);
    }

    void OnRemove(BuildingListRemoveEvent e)
    {
        CheckNeedRebuildIfAround(e.building);
    }

    void CheckNeedRebuildIfAround(BuildingBase b)
    {
        if (b == null)
            return;

        var bounds = b.GetBounds();
        var min = bounds.min - Vector3Int.one;
        var max = bounds.max;

        var path = m_path.GetPoints();

        bool needRebuild = false;
        foreach(var p in path)
        {
            if(p.x >= min.x && p.y >= min.y && p.z >= min.z && p.x <= max.x && p.y <= max.y && p.z <= max.z)
            {
                needRebuild = true;
                break;
            }
        }

        if(needRebuild)
        {
            var team = Event<GetTeamEvent>.Broadcast(new GetTeamEvent(), gameObject);
            m_path.SetTarget(transform.position, m_lastTarget, team.team);
        }
    }
}
