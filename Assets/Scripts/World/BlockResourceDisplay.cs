using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class BlockResourceDisplay : MonoBehaviour
{
    [SerializeField] ResourceType m_resourceType;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<BuildSelectionDetailCommonEvent>.LocalSubscriber(BuildCommon, gameObject));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void BuildCommon(BuildSelectionDetailCommonEvent e)
    {
        var data = Global.instance.resourceDatas.GetResource(m_resourceType);
        if (data == null)
            return;

        UIElementData.Create<UIElementSimpleText>(e.container).SetText(data.name).SetAlignment(UIElementAlignment.center);
        UIElementData.Create<UIElementSpace>(e.container).SetSpace(5);

        UIElementData.Create<UIElementSimpleText>(e.container).SetText(data.description);
    }
}

