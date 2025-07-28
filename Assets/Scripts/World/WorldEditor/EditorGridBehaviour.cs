using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class EditorGridBehaviour : MonoBehaviour
{
    [SerializeField] int m_initialGridSize = 4;
    [SerializeField] int m_initialGridHeight = 2;
    [SerializeField] Material m_gridSizeMaterial;
    [SerializeField] Color m_gridSizeColor;

    static EditorGridBehaviour m_instance;

    public static EditorGridBehaviour instance { get { return m_instance; } }

    GridBehaviour m_gridBehaviour;
    MeshFilter m_gridSizeDisplay;

    private void Awake()
    {
        m_gridBehaviour = GetComponent<GridBehaviour>();

        if(m_instance == null)
            m_instance = this;
    }

    private void Start()
    {
        CreateInitialGrid();
    }

    void CreateInitialGrid()
    {
        Grid grid = new Grid(m_initialGridSize, m_initialGridHeight);
        int size = GridEx.GetRealSize(grid);
        
        for(int i = 0; i < size; i++)
        {
            for(int k = 0; k < size; k++)
            {
                GridEx.SetBlock(grid, new Vector3Int(i, 0, k), new Block(BlockType.water));
            }
        }

        m_gridBehaviour.SetGrid(grid);
        Event<SetGridEvent>.Broadcast(new SetGridEvent(grid));

        UpdateGridSizeDisplay();
    }

    void PopulateNewChunkFunction(Matrix<Block> chunk, Vector3Int pos)
    {
        if (pos.y != 0)
            return;

        for(int i = 0; i < Grid.ChunkSize; i++)
        {
            for(int k = 0; k < Grid.ChunkSize; k++)
            {
                chunk.Set(i, 0, k, new Block(BlockType.water));
            }
        }
    }

    public void SetGridSize(int size, int height)
    {
        if (m_gridBehaviour == null)
            return;

        m_gridBehaviour.ResizeGrid(size, height, PopulateNewChunkFunction);

        Event<SetGridEvent>.Broadcast(new SetGridEvent(m_gridBehaviour.GetGrid()));

        UpdateGridSizeDisplay();
    }

    void UpdateGridSizeDisplay()
    {
        if (m_gridBehaviour == null || m_gridBehaviour.GetGrid() == null)
            return;

        if(m_gridSizeDisplay == null)
        {
            var obj = new GameObject("Grid Size");
            obj.layer = LayerMask.NameToLayer(Global.instance.editorDatas.editorLayer);
            obj.transform.parent = transform;
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            obj.transform.localScale = Vector3.one;

            m_gridSizeDisplay = obj.AddComponent<MeshFilter>();
            var renderer = obj.AddComponent<MeshRenderer>();
            renderer.material = m_gridSizeMaterial;
        }

        var oldMesh = m_gridSizeDisplay.mesh;
        if (oldMesh != null)
            Destroy(oldMesh);

        var grid = m_gridBehaviour.GetGrid();
        int size = GridEx.GetRealSize(grid);
        int height = GridEx.GetRealHeight(grid);

        m_gridSizeDisplay.mesh = WireframeMesh.SimpleCube(new Vector3(size, height, size), Vector3.zero, m_gridSizeColor);
    }
}
