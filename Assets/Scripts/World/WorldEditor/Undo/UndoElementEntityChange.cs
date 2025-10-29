using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public enum UndoEntityChangeState
{
    Place,
    Remove,
    Update,
}

public class UndoElementEntityChange : UndoElementBase
{
    UndoEntityChangeState m_state;
    Guid m_entityID;
    JsonObject m_oldEntityData;
    JsonObject m_newEntityData;
    EntityType m_entityType;

    public void SetPlace(EntityType type, Guid entityID, JsonObject newData)
    {
        m_state = UndoEntityChangeState.Place;
        m_entityID = entityID;
        m_oldEntityData = null;
        m_newEntityData = newData;
        m_entityType = type;
    }

    public void SetRemove(EntityType type, Guid entityID, JsonObject oldData)
    {
        m_state = UndoEntityChangeState.Remove;
        m_entityID = entityID;
        m_oldEntityData = oldData;
        m_newEntityData = null;
        m_entityType = type;
    }

    public void SetChange(EntityType type, Guid entityID, JsonObject oldData, JsonObject newData)
    {
        m_state = UndoEntityChangeState.Update;
        m_entityID = entityID;
        m_oldEntityData = oldData;
        m_newEntityData = newData;
        m_entityType = type;
    }

    public override void Apply()
    {
        if(m_state == UndoEntityChangeState.Remove)
        {
            switch(m_entityType)
            {
                case EntityType.Building:
                    BuildingBase.Create(m_oldEntityData);
                    break;
                case EntityType.GameEntity:
                    GameEntity.Create(m_oldEntityData);
                    break;
                case EntityType.Projectile:
                    ProjectileBase.Create(m_oldEntityData);
                    break;
                case EntityType.Quest:
                    QuestElement.Create(m_oldEntityData);
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (IDList.instance == null)
                return;

            var entity = IDList.instance.GetEntityFromID(m_entityID);
            if (entity == null)
                return;

            if (m_state == UndoEntityChangeState.Place)
                GameEntity.Destroy(entity.gameObject);
            else if(m_state == UndoEntityChangeState.Update)
            {
                switch(m_entityType)
                {
                    case EntityType.Building:
                        var building = entity.GetComponent<BuildingBase>();
                        if (building != null)
                            building.Load(m_oldEntityData);
                        break;
                    case EntityType.GameEntity:
                        var gameEntity = entity.GetComponent<GameEntity>();
                        if (gameEntity != null)
                            gameEntity.Load(m_oldEntityData);
                        break;
                    case EntityType.Projectile:
                        var projectile = entity.GetComponent<ProjectileBase>();
                        if (projectile != null)
                            projectile.Load(m_oldEntityData);
                        break;
                    case EntityType.Quest:
                        var questElm = entity.GetComponent<QuestElement>();
                        if (questElm != null)
                            questElm.Load(m_oldEntityData);
                        break;
                    default:
                        break;
                }
            }
        }
    }

    public override UndoElementBase GetRevertElement()
    {
        var elem = new UndoElementEntityChange();
        if(m_state == UndoEntityChangeState.Place)
        {
            elem.SetRemove(m_entityType, m_entityID, m_newEntityData);
        }
        else if(m_state == UndoEntityChangeState.Remove)
        {
            elem.SetPlace(m_entityType, m_entityID, m_oldEntityData);
        }
        else if(m_state == UndoEntityChangeState.Update)
        {
            elem.SetChange(m_entityType, m_entityID, m_oldEntityData, m_newEntityData);
        }

        return elem;
    }
}
