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

    SubscriberList m_subscriberList = new SubscriberList();

    Texture2D m_mapTexture;

    private void Awake()
    {
        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(OnLoadEnd));

        m_subscriberList.Subscribe();
    }

    private void OnDestroy()
    {
        m_subscriberList.Unsubscribe();
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
}
