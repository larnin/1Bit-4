using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public  class MainMenuCamera : MonoBehaviour
{
    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GetCameraEvent>.Subscriber(GetCamera));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    void GetCamera(GetCameraEvent e)
    {
        var camera = GetComponentInChildren<Camera>();
        e.camera = camera;
        e.UICamera = camera;
    }
}
