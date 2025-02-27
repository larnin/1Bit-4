using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MapDisplay : MonoBehaviour
{
    [SerializeField] Image m_renderImage;
    [SerializeField] Material m_mapMaterial;
    [SerializeField] float m_rotationOffset;

    SubscriberList m_subscriberList = new SubscriberList();

    Texture2D m_mapTexture;

    private void Awake()
    {
        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(OnLoadEnd));
        m_subscriberList.Add(new Event<SettingsDisplayMapChangedEvent>.Subscriber(OnSettingsChange));

        m_subscriberList.Subscribe();

        SetVisibility(GameInfos.instance.settings.GetDisplayMap());
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
    }

    private void LateUpdate()
    {
        GetCameraRotationEvent e = new GetCameraRotationEvent();
        e.rotation = transform.localRotation.eulerAngles.z - m_rotationOffset;

        Event<GetCameraRotationEvent>.Broadcast(e);
        transform.localRotation = Quaternion.Euler(0, 0, e.rotation + m_rotationOffset);
    }

    void OnLoadEnd(GenerationFinishedEvent e)
    {
        var grid = new GetGridEvent();
        Event<GetGridEvent>.Broadcast(grid);
        
        if (grid.grid == null)
            return;

        Color landColor = Color.white;
        Color waterColor = Color.black;

        var gridSize = GridEx.GetRealSize(grid.grid);

        m_mapTexture = new Texture2D(gridSize, gridSize);

        for(int x = 0; x < gridSize; x++)
        {
            for(int z = 0; z < gridSize; z++)
            {
                int height = GridEx.GetHeight(grid.grid, new Vector2Int(x, z));
                if (height < 0)
                    m_mapTexture.SetPixel(x, z, waterColor);
                else
                {
                    var b = GridEx.GetBlock(grid.grid, new Vector3Int(x, height, z));
                    if (b == BlockType.water)
                        m_mapTexture.SetPixel(x, z, waterColor);
                    else m_mapTexture.SetPixel(x, z, landColor);
                }
            }
        }

        m_mapTexture.Apply();

        if (m_renderImage != null && m_mapMaterial != null)
        {
            m_mapMaterial.SetTexture("_MainTex", m_mapTexture);

            m_renderImage.material = m_mapMaterial;
        }
    }

    void OnSettingsChange(SettingsDisplayMapChangedEvent e)
    {
        SetVisibility(GameInfos.instance.settings.GetDisplayMap());
    }

    void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }
}
