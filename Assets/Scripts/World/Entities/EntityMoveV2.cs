using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityMoveV2 : MonoBehaviour
{
    enum MoveType
    {
        Idle,
        Moving,
        Jumping,
    }

    [SerializeField] float m_moveSpeed = 1;
    [SerializeField] float m_acceleration = 1;
    [SerializeField] float m_rotationSpeed = 1;

    EntityMoveTargetInterface m_moveInterface;

    MoveType m_state = MoveType.Idle;

    float m_speed = 0;
    float m_angle = 0;

    Vector3 m_jumpStart = Vector3.zero;
    Vector3 m_jumpEnd = Vector3.zero;
    float m_jumpTimer = 0;
    float m_jumpTimeMax = 0;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Start()
    {
        m_moveInterface = GetComponent<EntityMoveTargetInterface>();
    }

    private void Awake()
    {
        m_subscriberList.Add(new Event<LoadEvent>.LocalSubscriber(Load, gameObject));
        m_subscriberList.Add(new Event<SaveEvent>.LocalSubscriber(Save, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();    
    }

    private void LateUpdate()
    {
        if (m_moveInterface == null)
            return;

        if (GameInfos.instance.paused)
            return;

        if (Utility.IsFrozen(gameObject))
            return;

        if (Utility.IsDead(gameObject))
            return;

        if (m_state == MoveType.Jumping)
            UpdateJump();
        else UpdateMove();
    }

    void UpdateMove()
    {
        if(StartJump())
        {
            UpdateJump();
            return;
        }

        bool moving = m_moveInterface.CanMove();

        if(moving)
            m_speed += m_acceleration;
        else m_speed -= 2 * m_acceleration;
        if (m_speed < 0)
            m_speed = 0;
        if (m_speed > m_moveSpeed)
            m_speed = m_moveSpeed;

        if (m_speed > 0.001f)
        {
            Vector3 target = m_moveInterface.GetNextPos();
            
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

            MoveTo(newPos);

            transform.forward = moveDir;
        }
    }

    //todo make the entity jump 
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
        foreach (var p in testPos)
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

    void UpdateJump()
    {

    }

    bool StartJump()
    {
        return false;
    }

    void MoveTo(Vector3 next, bool retry = false)
    {
        Vector3 current = transform.position;
        Vector3Int currentI = new Vector3Int(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y), Mathf.RoundToInt(current.z));
        Vector3Int nextI = new Vector3Int(Mathf.RoundToInt(next.x), Mathf.RoundToInt(next.y), Mathf.RoundToInt(next.z));

        if(currentI == nextI || m_moveInterface.IsNavigable(nextI))
        {
            transform.position = next;
            return;
        }

        Vector2[] points = new Vector2[]{
            new Vector2(currentI.x - 0.5f, currentI.z - 0.5f),
            new Vector2(currentI.x - 0.5f, currentI.z + 0.5f),
            new Vector2(currentI.x + 0.5f, currentI.z + 0.5f),
            new Vector2(currentI.x + 0.5f, currentI.z - 0.5f) };

        Vector2 current2 = new Vector2(current.x, current.z);

        bool intersect = false;
        float intersectDist = 0;
        Vector2 intersectPos = Vector2.zero;
        Vector2 intersectDir = Vector2.zero;

        for (int i = 0; i < 4; i++)
        {
            Vector2 p1 = points[i];
            Vector2 p2 = i == 3 ? points[0] : points[i + 1];

            Vector2 result = Utility.IntersectLines(current2, new Vector2(next.x, next.z), p1, p2);

            Vector2 dir = result - current2;
            float dist = dir.magnitude;
            dir /= dist;

            if (dist > 1)
                continue;

            if (dist > 0.01f)
                dist -= 0.01f;
            else dist = 0;

            if(!intersect || dist < intersectDist)
            {
                intersect = true;
                intersectDist = dist;
                intersectPos = dir * dist + current2;
                intersectDir = (p2 - p1).normalized;
            }
        }

        if(!intersect)
        {
            transform.position = next;
            return;
        }

        transform.position = new Vector3(intersectPos.x, next.y, intersectPos.y);
        if (retry)
            return;

        Vector3 remaining = next - transform.position;
        Vector3 remainingDir = Vector3.Project(remaining, new Vector3(intersectDir.x, 0, intersectDir.y));

        Vector3 newNext = transform.position + remainingDir;

        MoveTo(newNext, true);

    }

    void Load(LoadEvent e)
    {
        var jsonObj = e.obj.GetElement("entityMove");
        if (jsonObj != null && jsonObj.IsJsonObject())
        {
            var obj = jsonObj.JsonObject();

            MoveType state;
            var stateJson = obj.GetElement("state");
            if (stateJson != null && stateJson.IsJsonString())
            {
                if (Enum.TryParse<MoveType>(stateJson.String(), out state))
                    m_state = state;
            }

            var jsonSpeed = obj.GetElement("speed");
            if (jsonSpeed != null && jsonSpeed.IsJsonNumber())
                m_speed = jsonSpeed.Float();

            var jsonAngle = obj.GetElement("angle");
            if (jsonAngle != null && jsonAngle.IsJsonNumber())
                m_angle = jsonAngle.Float();

            var jsonJumpStart = obj.GetElement("jumpStart");
            if (jsonJumpStart != null && jsonJumpStart.IsJsonArray())
                m_jumpStart = Json.ToVector3(jsonJumpStart.JsonArray());

            var jsonJumpEnd = obj.GetElement("jumpEnd");
            if (jsonJumpEnd != null && jsonJumpEnd.IsJsonArray())
                m_jumpEnd = Json.ToVector3(jsonJumpEnd.JsonArray());

            var jsonJumpTimer = obj.GetElement("jumpTimer");
            if (jsonJumpTimer != null && jsonJumpTimer.IsJsonNumber())
                m_jumpTimer = jsonJumpTimer.Float();

            var jsonJumpTimeMax = obj.GetElement("jumpTimeMax");
            if (jsonJumpTimeMax != null && jsonJumpTimeMax.IsJsonNumber())
                m_jumpTimeMax = jsonJumpTimeMax.Float();

        }
    }

    void Save(SaveEvent e)
    {
        var obj = new JsonObject();
        e.obj.AddElement("entityMove", obj);

        obj.AddElement("srtate", m_state.ToString());

        obj.AddElement("speed", m_speed);
        obj.AddElement("angle", m_angle);

        obj.AddElement("jumpStart", Json.FromVector3(m_jumpStart));
        obj.AddElement("jumpEnd", Json.FromVector3(m_jumpEnd));
        obj.AddElement("jumpTimer", m_jumpTimer);
        obj.AddElement("jumpTimeMax", m_jumpTimeMax);
    }
}

