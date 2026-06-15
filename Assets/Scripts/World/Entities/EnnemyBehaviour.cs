using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EnnemyBehaviour : MonoBehaviour
{
    EntityWeaponBase m_weapon;
    EntityMove m_move;

    GameObject m_target;
    BuildingBase m_buildingTarget;

    SubscriberList m_subscriberList = new SubscriberList();

    float m_difficultyOnSpawn;

    private void Start()
    {
        m_weapon = GetComponent<EntityWeaponBase>();
        m_move = GetComponent<EntityMove>();

        //todo
        //if (DifficultySystem.instance != null)
        //    m_difficultyOnSpawn = DifficultySystem.instance.GetDifficulty();
    }

    private void Awake()
    {
        m_subscriberList.Add(new Event<DeathEvent>.LocalSubscriber(OnDeath, gameObject));
        m_subscriberList.Add(new Event<LifeLossEvent>.LocalSubscriber(OnLifeLoss, gameObject));
        m_subscriberList.Add(new Event<LoadLevelEvent>.LocalSubscriber(Load, gameObject));
        m_subscriberList.Add(new Event<SaveLevelEvent>.LocalSubscriber(Save, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void Update()
    {
        if (GameInfos.instance.paused)
            return;

        if (Utility.IsFrozen(gameObject))
            return;

        if (m_weapon == null || m_move == null)
            return;

        var target = m_weapon.GetTarget();

        if (target != m_target)
        {
            if (target != null)
            {
                Vector3 targetPos;
                m_buildingTarget = target.GetComponent<BuildingBase>();
                if (m_buildingTarget != null)
                    targetPos = m_buildingTarget.GetGroundCenter();
                else targetPos = target.transform.position;

                m_move.SetTarget(targetPos);
            }
            else m_move.Stop();

            m_target = target;
        }

        if (m_target != null)
        {
            float range = m_weapon.GetMoveDistance();
            Vector3 realTargetPos;
            if (m_buildingTarget != null)
                realTargetPos = m_buildingTarget.GetGroundCenter();
            else realTargetPos = target.transform.position;

            float distance = (realTargetPos - transform.position).sqrMagnitude;
            if(distance < range * range)
            {
                if (m_move.IsMoving())
                    m_move.Stop();
            }
            else if(!m_move.IsMoving())
                m_move.SetTarget(realTargetPos);
        }
    }

    void OnDeath(DeathEvent e)
    {
        Event<OnEnnemyKillEvent>.Broadcast(new OnEnnemyKillEvent(this));
    }

    void OnLifeLoss(LifeLossEvent e)
    {
        Event<OnEnnemyDamagedEvent>.Broadcast(new OnEnnemyDamagedEvent(this));
    }

    void Load(LoadLevelEvent e)
    {
        var jsonObj = e.obj.GetElement("ennemy");
        if (jsonObj != null && jsonObj.IsJsonObject())
        {
            var obj = jsonObj.JsonObject();

            var jsonDifficulty = obj.GetElement("difficulty");
            if (jsonDifficulty != null && jsonDifficulty.IsJsonNumber())
                m_difficultyOnSpawn = jsonDifficulty.Float();
        }
    }

    void Save(SaveLevelEvent e)
    {
        var obj = new JsonObject();
        e.obj.AddElement("ennemy", obj);

        obj.AddElement("difficulty", m_difficultyOnSpawn);
    }
}

