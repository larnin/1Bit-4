using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class TurretBehaviour : MonoBehaviour
{
    [SerializeField] float m_turretRotSpeed;

    enum TurretState
    {
        NoTarget,
        MovingToTarget,
        Target,
        MovingToDefault,
    }

    Vector3 m_target;
    bool m_haveTarget = false;

    Transform m_turretPivot;
    Quaternion m_turretInitialRotation;
    Quaternion m_turretStartRotation;
    TurretState m_turretState;
    float m_turretTimer;
    float m_turretTimerMax;

    SubscriberList m_subscriberList = new SubscriberList();

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

    private void Start()
    {
        m_turretPivot = transform.Find("Pivot");
        m_turretInitialRotation = m_turretPivot.rotation;
    }

    public void SetTarget(Vector3 target)
    {
        m_target = target;
        m_haveTarget = true;
    }

    public void SetNoTarget()
    {
        m_target = Vector3.zero;
        m_haveTarget = false;
    }

    public bool CanFire()
    {
        if (m_turretState != TurretState.Target)
            return false;

        var forward = (m_target - m_turretPivot.position).normalized;
        Quaternion targetAngle = Quaternion.LookRotation(forward, Vector3.up);

        float delta = Mathf.Abs(Quaternion.Angle(m_turretPivot.rotation, targetAngle));

        return delta < 5;
    }

    private void Update()
    {
        if (GameInfos.instance.paused)
            return;

        if (Utility.IsFrozen(gameObject))
            return;

        if (m_turretPivot == null)
        {
            m_turretState = TurretState.Target;
        }
        else
        {
            switch (m_turretState)
            {
                case TurretState.NoTarget:
                    {
                        if (m_haveTarget)
                        {
                            m_turretStartRotation = m_turretPivot.rotation;

                            var targetPos = m_target;
                            var forward = (targetPos - m_turretPivot.position).normalized;

                            float angle = Mathf.Abs(Quaternion.Angle(m_turretStartRotation, Quaternion.LookRotation(forward)));
                            m_turretTimer = 0;
                            m_turretTimerMax = angle / m_turretRotSpeed / Mathf.Rad2Deg;

                            m_turretState = TurretState.MovingToTarget;
                        }

                        break;
                    }
                case TurretState.MovingToTarget:
                    {
                        if (!m_haveTarget)
                        {
                            m_turretStartRotation = m_turretPivot.rotation;
                            MakeInitialRotationFromCurrent();
                            float angle = Mathf.Abs(Quaternion.Angle(m_turretStartRotation, m_turretInitialRotation));
                            m_turretTimer = 0;
                            m_turretTimerMax = angle / m_turretRotSpeed / Mathf.Rad2Deg;

                            m_turretState = TurretState.MovingToDefault;
                            break;
                        }

                        m_turretTimer += Time.deltaTime;
                        if (m_turretTimer >= m_turretTimerMax)
                        {
                            m_turretTimer = m_turretTimerMax;
                            m_turretState = TurretState.Target;
                        }

                        float normTime = m_turretTimer / m_turretTimerMax;

                        var forward = (m_target - m_turretPivot.position).normalized;
                        Quaternion targetAngle = Quaternion.LookRotation(forward, Vector3.up);

                        m_turretPivot.rotation = Quaternion.Lerp(m_turretStartRotation, targetAngle, normTime);

                        break;
                    }
                case TurretState.Target:
                    {
                        if (!m_haveTarget)
                        {
                            m_turretStartRotation = m_turretPivot.rotation;
                            MakeInitialRotationFromCurrent();
                            float angle = Mathf.Abs(Quaternion.Angle(m_turretStartRotation, m_turretInitialRotation));
                            m_turretTimer = 0;
                            m_turretTimerMax = angle / m_turretRotSpeed / Mathf.Rad2Deg;

                            m_turretState = TurretState.MovingToDefault;
                        }

                        var targetPos = m_target;
                        var forward = (targetPos - m_turretPivot.position).normalized;
                        Quaternion targetAngle = Quaternion.LookRotation(forward, Vector3.up);

                        float delta = Mathf.Abs(Quaternion.Angle(m_turretPivot.rotation, targetAngle));

                        float maxDelta = Time.deltaTime * m_turretRotSpeed * Mathf.Rad2Deg;
                        if (delta < maxDelta)
                            m_turretPivot.rotation = targetAngle;
                        else m_turretPivot.rotation = Quaternion.Lerp(m_turretPivot.rotation, targetAngle, maxDelta / delta);

                        break;
                    }
                case TurretState.MovingToDefault:
                    {
                        if (m_haveTarget)
                        {
                            m_turretState = TurretState.NoTarget;
                            break;
                        }

                        m_turretTimer += Time.deltaTime;
                        if (m_turretTimer >= m_turretTimerMax)
                        {
                            m_turretTimer = m_turretTimerMax;
                            m_turretState = TurretState.NoTarget;
                        }

                        float normTime = m_turretTimer / m_turretTimerMax;

                        m_turretPivot.rotation = Quaternion.Lerp(m_turretStartRotation, m_turretInitialRotation, normTime);

                        break;
                    }
            }
        }
    }

    public static Vector3 GetTargetCenter(GameObject target)
    {
        var targetType = GameSystem.GetEntityType(target);
        if(targetType == EntityType.Building)
        {
            var building = target.GetComponent<BuildingBase>();
            if(building != null)
            {
                var center = building.GetGroundCenter();
                var size = building.GetSize();
                center.y += size.y / 2.0f;

                return center;
            }
        }
        return target.transform.position;
    }

    void MakeInitialRotationFromCurrent()
    {
        var current = m_turretPivot.forward;
        current.y = 0;

        m_turretInitialRotation = Quaternion.LookRotation(current, Vector3.up);
    }

    void Load(LoadEvent e)
    {
        var objJson = e.obj.GetElement("turret");
        if(objJson != null && objJson.IsJsonObject())
        {
            var obj = objJson.JsonObject();

            var jsonTarget = obj.GetElement("target");
            if (jsonTarget != null && jsonTarget.IsJsonArray())
                m_target = Json.ToVector3(jsonTarget.JsonArray());

            var jsonHaveTarget = obj.GetElement("haveTarget");
            if (jsonHaveTarget != null && jsonHaveTarget.IsJsonNumber())
                m_haveTarget = jsonHaveTarget.Int() != 0;

            var startRotationJson = obj.GetElement("startRotation");
            if (startRotationJson != null && startRotationJson.IsJsonArray())
                m_turretStartRotation = Json.ToQuaternion(startRotationJson.JsonArray());

            var stateJson = obj.GetElement("state");
            if(stateJson != null && stateJson.IsJsonString())
            {
                if (!Enum.TryParse<TurretState>(stateJson.String(), out m_turretState))
                    m_turretState = TurretState.NoTarget;
            }

            var timerJson = obj.GetElement("timer");
            if(timerJson != null && timerJson.IsJsonNumber())
                m_turretTimer = timerJson.Float();

            var timerMaxJson = obj.GetElement("timerMax");
            if (timerMaxJson != null && timerMaxJson.IsJsonNumber())
                m_turretTimerMax = timerMaxJson.Float();
        }
    }

    void Save(SaveEvent e)
    {
        var obj = new JsonObject();
        e.obj.AddElement("turret", obj);

        obj.AddElement("target", Json.FromVector3(m_target));
        obj.AddElement("haveTarget", m_haveTarget ? 1 : 0);
        obj.AddElement("startRotation", Json.FromQuaternion(m_turretStartRotation));
        obj.AddElement("state", m_turretState.ToString());
        obj.AddElement("timer", m_turretTimer);
        obj.AddElement("timerMax", m_turretTimerMax);
    }
}
