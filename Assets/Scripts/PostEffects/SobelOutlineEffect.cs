using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SobelOutlineEffect : MonoBehaviour
{
    [SerializeField] float m_thickness = 1.0f;
    [SerializeField] float m_depthMultiplier = 1.0f;
    [SerializeField] float m_depthBias = 1.0f;
    [SerializeField] float m_normalMultiplier = 1.0f;
    [SerializeField] float m_normalBias = 10.0f;
    [SerializeField] Color m_color = Color.black;
    [SerializeField] Material m_material = null;

    void Start()
    {
        if (m_material == null || m_material.shader == null || !m_material.shader.isSupported)
        {
            enabled = false;
            return;
        }
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, m_material);
    }

    private void Update()
    {
        GetNormalsTextureEvent texture = new GetNormalsTextureEvent();
        Event<GetNormalsTextureEvent>.Broadcast(texture);
        if (texture.normals != null)
            m_material.SetTexture("_NormalTex", texture.normals);

        if (texture.depth != null)
            m_material.SetTexture("_DepthTex", texture.depth);

        GetCameraScaleEvent scale = new GetCameraScaleEvent();
        Event<GetCameraScaleEvent>.Broadcast(scale);

        m_material.SetFloat("_OutlineDepthScale", scale.scale);
        m_material.SetFloat("_OutlineThickness", m_thickness);
        m_material.SetFloat("_OutlineDepthMultiplier", m_depthMultiplier);
        m_material.SetFloat("_OutlineDepthBias", m_depthBias);
        m_material.SetFloat("_OutlineNormalMultiplier", m_normalMultiplier);
        m_material.SetFloat("_OutlineNormalBias", m_normalBias);
        m_material.SetColor("_OutlineColor", m_color);
    }
}
