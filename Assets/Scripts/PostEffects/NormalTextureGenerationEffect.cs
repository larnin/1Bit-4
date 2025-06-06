﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class NormalTextureGenerationEffect : MonoBehaviour
{
    [SerializeField] Shader m_replacementShader;

    RenderTexture m_renderTexture;
    RenderTexture m_depthTexture;

    SubscriberList m_subscriberList = new SubscriberList();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GetNormalsTextureEvent>.Subscriber(GetTexture));
        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void LateUpdate()
    {
        if(m_renderTexture == null || m_renderTexture.width != Screen.width || m_renderTexture.height != Screen.height)
        {
            if (m_renderTexture != null)
                m_renderTexture.Release();
            m_renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
        }

        if(m_depthTexture == null || m_depthTexture.width != Screen.width || m_depthTexture.height != Screen.height)
        {
            if (m_depthTexture != null)
                m_depthTexture.Release();
            m_depthTexture = new RenderTexture(Screen.width, Screen.height, 24, RenderTextureFormat.Depth);
        }

        Event<SetDecalsEnabledEvent>.Broadcast(new SetDecalsEnabledEvent(false));

        RenderTexture.active = m_renderTexture;
        GL.Clear(false, true, Color.gray);
        RenderTexture.active = m_depthTexture;
        GL.Clear(true, false, Color.black, 1);
        RenderTexture.active = null;

        var cameras = Event<GetAllMainCameraEvent>.Broadcast(new GetAllMainCameraEvent());

        if (m_replacementShader != null)
        {
            foreach (var c in cameras.cameras)
            {
                var rt = c.targetTexture;
                c.SetTargetBuffers(m_renderTexture.colorBuffer, m_depthTexture.depthBuffer);
                c.RenderWithShader(m_replacementShader, "RenderType");
                c.targetTexture = rt;
            }
        }

        Event<SetDecalsEnabledEvent>.Broadcast(new SetDecalsEnabledEvent(true));
    }

    void GetTexture(GetNormalsTextureEvent e)
    {
        e.normals = m_renderTexture;
        e.depth = m_depthTexture;
    }
}