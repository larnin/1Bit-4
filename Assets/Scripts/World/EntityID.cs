using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EntityID : MonoBehaviour
{
    Guid m_uniqueID;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_uniqueID = Guid.NewGuid();

        m_subscriberList.Add(new Event<SaveEvent>.LocalSubscriber(Save, gameObject));
        m_subscriberList.Add(new Event<LoadEvent>.LocalSubscriber(Load, gameObject));
        m_subscriberList.Add(new Event<GetEntityIDEvent>.LocalSubscriber(GetID, gameObject));
        m_subscriberList.Subscribe();

        if (IDList.instance != null)
            IDList.instance.Register(this);
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();

        if (IDList.instance != null)
            IDList.instance.UnRegister(this);
    }

    void Save(SaveEvent e)
    {
        e.obj.AddElement("guid", m_uniqueID.ToString("N"));
    }

    void Load(LoadEvent e)
    {
        var guidJson = e.obj.GetElement("guid");
        if(guidJson != null && guidJson.IsJsonString())
        {
            Guid newGuid;
            if (Guid.TryParse(guidJson.String(), out newGuid))
                m_uniqueID = newGuid;
        }
    }

    public Guid GetID()
    {
        return m_uniqueID;
    }

    void GetID(GetEntityIDEvent e)
    {
        e.id = m_uniqueID;
    }
}