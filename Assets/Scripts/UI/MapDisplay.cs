using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MapDisplay : MonoBehaviour
{
    class BuildingSprite
    {
        public BuildingBase building;
        public Image sprite;
    }

    [SerializeField] Image m_renderImage;
    [SerializeField] Material m_mapMaterial;
    [SerializeField] float m_rotationOffset;
    [SerializeField] RectTransform m_cameraRect;
    [SerializeField] Sprite m_buildingSprite;
    [SerializeField] float m_buildingSpriteSize;
    [SerializeField] RectTransform m_buildingSpritesContainer;

    SubscriberList m_subscriberList = new SubscriberList();

    Texture2D m_mapTexture;

    List<BuildingSprite> m_buildings = new List<BuildingSprite>();

    private void Awake()
    {
        m_subscriberList.Add(new Event<GenerationFinishedEvent>.Subscriber(OnLoadEnd));
        m_subscriberList.Add(new Event<SettingsDisplayMapChangedEvent>.Subscriber(OnSettingsChange));
        m_subscriberList.Add(new Event<BuildingListAddEvent>.Subscriber(OnAddBuilding));
        m_subscriberList.Add(new Event<BuildingListRemoveEvent>.Subscriber(OnRemoveBuilding));

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

        Vector2 pos1, pos2, pos3, pos4;
        GetCameraSeeRect(out pos1, out pos2, out pos3, out pos4);
        Rect camRect;
        float camAngle;
        PointsToNormalizedOBB(pos1, pos2, pos3, pos4, out camRect, out camAngle);
        SetCameraRect(camRect, camAngle);
    }

    void OnLoadEnd(GenerationFinishedEvent e)
    {
        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
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
                    if (b.type == BlockType.water)
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

        if (CustomLightsManager.instance != null)
            m_mapMaterial.SetFloat("_LightBaseRange", CustomLightsManager.instance.GetLightBaseRange());
    }

    void OnSettingsChange(SettingsDisplayMapChangedEvent e)
    {
        SetVisibility(GameInfos.instance.settings.GetDisplayMap());
    }

    void SetVisibility(bool visible)
    {
        gameObject.SetActive(visible);
    }

    void GetCameraSeeRect(out Vector2 pos1, out Vector2 pos2, out Vector2 pos3, out Vector2 pos4)
    {
        float h = 0;
        if(BuildingList.instance != null)
        {
            var tower = BuildingList.instance.GetFirstBuilding(BuildingType.Tower);
            if (tower != null)
                h = tower.GetGroundCenter().y;
        }

        var plane = new Plane(Vector3.up, new Vector3(0, h, 0));

        var camera = Event<GetCameraEvent>.Broadcast(new GetCameraEvent());

        pos1 = Vector2.zero;
        pos2 = Vector2.zero;
        pos3 = Vector2.zero;
        pos4 = Vector2.zero;

        if (camera.camera == null)
            return;

        var width = Screen.width;
        var height = Screen.height;

        var ray1 = camera.camera.ScreenPointToRay(new Vector3(0, 0, 0));
        var ray2 = camera.camera.ScreenPointToRay(new Vector3(width, 0, 0));
        var ray3 = camera.camera.ScreenPointToRay(new Vector3(width, height, 0));
        var ray4 = camera.camera.ScreenPointToRay(new Vector3(0, height, 0));

        float enter = 0;
        plane.Raycast(ray1, out enter);
        var hitPoint = ray1.GetPoint(enter);
        pos1 = new Vector2(hitPoint.x, hitPoint.z);

        plane.Raycast(ray2, out enter);
        hitPoint = ray2.GetPoint(enter);
        pos2 = new Vector2(hitPoint.x, hitPoint.z);

        plane.Raycast(ray3, out enter);
        hitPoint = ray3.GetPoint(enter);
        pos3 = new Vector2(hitPoint.x, hitPoint.z);

        plane.Raycast(ray4, out enter);
        hitPoint = ray4.GetPoint(enter);
        pos4 = new Vector2(hitPoint.x, hitPoint.z);
    }

    void PointsToNormalizedOBB(Vector2 pos1, Vector2 pos2, Vector2 pos3, Vector2 pos4, out Rect rect, out float angle)
    {
        var left = (pos1 + pos2) / 2;
        var right = (pos3 + pos4) / 2;
        var top = (pos2 + pos3) / 2;
        var bottom = (pos4 + pos1) / 2;

        float width = (left - right).magnitude;
        float height = (top - bottom).magnitude;

        var center = (pos1 + pos2 + pos3 + pos4) / 4;

        rect = new Rect(center - new Vector2(width, height) / 2, new Vector2(width, height));

        angle = Vector2.SignedAngle(new Vector2(1, 0), (left - right));
    }

    void SetCameraRect(Rect rect, float angle)
    {
        if (m_cameraRect == null)
            return;

        m_cameraRect.localRotation = Quaternion.Euler(0, 0, angle);

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;

        int gridSize = GridEx.GetRealSize(grid.grid);

        var normalizedRect = new Rect(rect.position / gridSize, rect.size / gridSize);

        m_cameraRect.anchorMin = normalizedRect.position;
        m_cameraRect.anchorMax = normalizedRect.position + normalizedRect.size;

        m_cameraRect.offsetMin = Vector2.zero;
        m_cameraRect.offsetMax = Vector2.zero;
    }

    void OnAddBuilding(BuildingListAddEvent e)
    {
        if (m_buildingSprite == null)
            return;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent());
        if (grid.grid == null)
            return;

        var gridSize = GridEx.GetRealSize(grid.grid);

        var instance = new BuildingSprite();
        instance.building = e.building;

        GameObject obj = new GameObject("Building", typeof(RectTransform));
        instance.sprite = obj.AddComponent<Image>();
        instance.sprite.sprite = m_buildingSprite;

        var pos = e.building.GetGroundCenter() / gridSize;
        var size = m_buildingSpriteSize / 1000;

        var tr = instance.sprite.rectTransform;
        tr.SetParent(m_buildingSpritesContainer, false);

        tr.anchorMin = new Vector2(pos.x - size, pos.z - size);
        tr.anchorMax = new Vector2(pos.x + size, pos.z + size);

        tr.offsetMin = Vector2.zero;
        tr.offsetMax = Vector2.zero;

        m_buildings.Add(instance);
    }

    void OnRemoveBuilding(BuildingListRemoveEvent e)
    {
        var instance = m_buildings.Find(x => { return x.building == e.building; });
        if (instance == null)
            return;

        Destroy(instance.sprite.gameObject);

        m_buildings.Remove(instance);
    }
}
