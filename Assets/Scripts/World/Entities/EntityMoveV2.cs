using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Profiling;
using UnityEngine;
using NRand;

public class EntityMoveV2 : MonoBehaviour
{
    static readonly ProfilerMarker ms_profilerMarker = new ProfilerMarker(ProfilerCategory.Scripts, "EntityMoveTest");

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
    float m_avoidanceVelocityMultiplier = 1;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Start()
    {
        m_moveInterface = GetComponent<EntityMoveTargetInterface>();
    }

    private void Awake()
    {
        m_subscriberList.Add(new Event<LoadLevelEvent>.LocalSubscriber(Load, gameObject));
        m_subscriberList.Add(new Event<SaveLevelEvent>.LocalSubscriber(Save, gameObject));
        m_subscriberList.Add(new Event<GetVelocityEvent>.LocalSubscriber(GetVelocity, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();    
    }

    private void Update()
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

        float targetSpeed = m_moveSpeed * m_avoidanceVelocityMultiplier;
        if (!moving)
            targetSpeed = 0;

        float m_lastSpeed = m_speed;
        if (targetSpeed > m_speed)
            m_speed += m_acceleration * Time.deltaTime;
        else
        {
            float deceleration = m_moveSpeed / 0.25f;
            m_speed -= deceleration * Time.deltaTime;
        }
        if (m_lastSpeed <= targetSpeed && m_speed > targetSpeed)
            m_speed = targetSpeed;
        else if (m_lastSpeed >= targetSpeed && m_speed < targetSpeed)
            m_speed = targetSpeed;
        m_speed = Mathf.Clamp(m_speed, 0, m_moveSpeed);

        if (m_speed > 0.01f)
        {
            Vector3 target = m_moveInterface.GetNextPos();

            Vector3 dir = target - transform.position;
            dir = GetDirWithLoop(dir);
            dir = DeviateDirWithAvoidance(dir, out m_avoidanceVelocityMultiplier);
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

            transform.position = ReplacePosOnGridWithLoop(transform.position);
            transform.forward = moveDir;
        }
        else m_avoidanceVelocityMultiplier = 1;
    }

    Vector3 GetDirWithLoop(Vector3 dir)
    {
        //fast exit
        if (MathF.Abs(dir.x) < 5 && Mathf.Abs(dir.z) < 5)
            return dir;

        var grid = GridEx.GetCurrentGrid();
        if (grid == null)
            return dir;

        int size = GridEx.GetRealSize(grid);

        if(grid.LoopX())
        {
            if(Mathf.Abs(dir.x) >=  size / 2)
            {
                if (dir.x > 0)
                    dir.x -= size;
                else dir.x += size;
            }
        }

        if(grid.LoopZ())
        {
            if(Mathf.Abs(dir.z) >= size / 2)
            {
                if (dir.z > 0)
                    dir.z -= size;
                else dir.z += size;
            }
        }

        return dir;
    }

    Vector3 DeviateDirWithAvoidance(Vector3 dir, out float outVelocityMultiplier)
    {
        outVelocityMultiplier = 1;

        if (EntityList.instance == null)
            return dir;

        Vector3 pos = gameObject.transform.position;
        Vector3 target = pos + dir;

        bool ownerCurrent = IsEntityAvoidanceValid(EntityList.instance.GetFilledAvoidance(pos));
        bool ownerTarget = IsEntityAvoidanceValid(EntityList.instance.GetFilledAvoidance(target));

        if (ownerCurrent && ownerTarget)
            return dir;

        Vector3 leftDir = GetOffsetAvoidanceVector(dir, false);
        Vector3 targetLeft = pos + leftDir;
        Vector3Int targetLeftI = new Vector3Int(Mathf.RoundToInt(targetLeft.x), Mathf.RoundToInt(targetLeft.y), Mathf.RoundToInt(targetLeft.z));
        bool leftNavigable = m_moveInterface.IsNavigable(targetLeftI);
        bool ownerLeft = leftNavigable && IsEntityAvoidanceValid(EntityList.instance.GetFilledAvoidance(targetLeft));

        Vector3 rightDir = GetOffsetAvoidanceVector(dir, true);
        Vector3 targetRight = pos + rightDir;
        Vector3Int targetRightI = new Vector3Int(Mathf.RoundToInt(targetRight.x), Mathf.RoundToInt(targetRight.y), Mathf.RoundToInt(targetRight.z));
        bool rightNavigable = m_moveInterface.IsNavigable(targetRightI);
        bool ownerRight = rightNavigable && IsEntityAvoidanceValid(EntityList.instance.GetFilledAvoidance(targetRight));

        if (ownerLeft && !ownerRight)
            return leftDir;
        if (ownerRight && !ownerLeft)
            return rightDir;
        if(!ownerLeft && !ownerRight)
        {
            outVelocityMultiplier = 0;
            return dir;
        }

        RandomHash rand = new RandomHash(GetInstanceID());
        bool left = Rand.BernoulliDistribution(rand);
        if (left)
            return leftDir;
        return rightDir;
    }

    bool IsEntityAvoidanceValid(GameEntity e)
    {
        return e == null || e.gameObject == gameObject;
    }

    Vector3 GetOffsetAvoidanceVector(Vector3 dir, bool right)
    {
        float offsetAngle = (right ? 45.0f : -45.0f) * Mathf.Deg2Rad;

        float angle = Mathf.Atan2(dir.z, dir.x) + offsetAngle;

        return new Vector3(Mathf.Cos(angle), dir.y, Mathf.Sin(angle));
    }

    //infos used in the next function
    const float radius = 0.4f;
    static Vector2[] testPos = new Vector2[]
    {
    Vector2.zero, new Vector2(-radius, -radius), new Vector2(-radius, radius), new Vector2(radius, radius), new Vector2(radius, -radius)
    };

    //todo make the entity jump 
    float GetHeight(Vector3 newPos)
    {
        var grid = GridEx.GetCurrentGrid();
        if (grid == null)
            return newPos.y;

        float top = float.MinValue;
        foreach (var p in testPos)
        {
            Vector2 point = p + new Vector2(newPos.x, newPos.z);

            float height = GridEx.GetHeight(grid, new Vector2Int(Mathf.RoundToInt(point.x), Mathf.RoundToInt(point.y))) + 1;
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

    static Vector2[] offsets = new Vector2[]{
            new Vector2(-0.5f, -0.5f),
            new Vector2(-0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, -0.5f)};

    Vector3 MoveTo(Vector3 next, bool retry = false)
    {
        Grid grid = GridEx.GetCurrentGrid();

        Vector3 current = transform.position;
        Vector3Int currentI = new Vector3Int(Mathf.RoundToInt(current.x), Mathf.RoundToInt(current.y), Mathf.RoundToInt(current.z));
        Vector3Int nextI = new Vector3Int(Mathf.RoundToInt(next.x), Mathf.RoundToInt(next.y), Mathf.RoundToInt(next.z));

        if(currentI == nextI || m_moveInterface.IsNavigable(nextI))
        {
            transform.position = next;
            return current - next;
        }

        if (retry)
            return Vector3.zero;

        Vector3Int dirI = nextI - currentI;
        Vector3 dir = next - current;
        if (dirI.x != 0 && dirI.z != 0)
        {
            Vector3Int nextLeft = currentI + new Vector3Int(dirI.x, dirI.y, 0);
            if (m_moveInterface.IsNavigable(nextLeft))
                dir.z = 0;
            else
            {
                Vector3Int nextRight = currentI + new Vector3Int(0, dirI.y, dirI.z);
                if (m_moveInterface.IsNavigable(nextRight))
                    dir.x = 0;
                else return Vector3.zero;
            }
        }
        else if (dirI.x != 0)
            dir.x = 0;
        else if (dirI.z != 0)
            dir.z = 0;

        return MoveTo(current + dir, true);
    }

    Vector3 ReplacePosOnGridWithLoop(Vector3 pos)
    {
        var grid = GridEx.GetCurrentGrid();
        if (grid == null)
            return pos;

        var loopPos = GridEx.GetRealPosFromLoop(grid, pos);
        if (!grid.LoopX())
            loopPos.x = pos.x;
        if (!grid.LoopZ())
            loopPos.z = pos.z;

        var size = GridEx.GetRealSize(grid);

        if (loopPos.x <= -0.5f)
            loopPos.x = -0.499f;
        if (loopPos.x >= size - 0.5f)
            loopPos.x = size - 0.501f;

        if (loopPos.z <= -0.5f)
            loopPos.z = -0.499f;
        if (loopPos.z >= size - 0.5f)
            loopPos.z = size - 0.501f;

        return loopPos;
    }

    void GetVelocity(GetVelocityEvent e)
    {
        e.velocity = new Vector3(Mathf.Cos(m_angle), 0, Mathf.Sin(m_angle)) * m_speed;
    }
    void Load(LoadLevelEvent e)
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

    void Save(SaveLevelEvent e)
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

