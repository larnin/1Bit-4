using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class QuestSubObjectiveDiscoverResource : QuestSubObjectiveBase
{
    [SerializeField] int m_checkSpeed = 1;
    public int checkSpeed { get { return m_checkSpeed; } set { m_checkSpeed = value; } }

    [SerializeField] BlockType m_blockType = BlockType.ground;
    public BlockType blockType { get { return m_blockType; } set { m_blockType = value; } }

    bool m_completed = false;
    float m_lastTime = 0;
    Vector2Int m_lastPos = Vector2Int.zero;

    public override bool IsCompleted()
    {
        return m_completed;
    }

    public override void Start()
    {
        m_completed = false;
        m_lastTime = 0;
        m_lastPos = Vector2Int.zero;
    }

    public override void Update(float deltaTime)
    {
        if (m_completed)
            return;

        m_lastTime += deltaTime;

        int checkCount = Mathf.FloorToInt(m_checkSpeed / m_lastTime);
        m_lastTime -= checkCount / m_checkSpeed;

        if (CustomLightsManager.instance == null)
            return;

        if (checkCount < 0)
            return;

        var grid = Event<GetGridEvent>.Broadcast(new GetGridEvent()).grid;
        if (grid == null)
            return;

        int size = GridEx.GetRealSize(grid);

        if (checkCount > size * size)
            checkCount = size * size;

        if (m_lastPos.x < 0 || m_lastPos.x >= size || m_lastPos.y < 0 || m_lastPos.y >= size)
            m_lastPos = Vector2Int.zero;

        for(int i = 0; i < checkCount; i++)
        {
            if (CheckPosition(grid, m_lastPos))
            {
                m_completed = true;
                return;
            }

            m_lastPos.x++;
            if(m_lastPos.x >= size)
            {
                m_lastPos.x = 0;
                m_lastPos.y++;

                if (m_lastPos.y >= size)
                    m_lastPos.y = 0;
            }
        }
    }

    bool CheckPosition(Grid grid, Vector2Int pos)
    {
        if (!CustomLightsManager.instance.IsPosVisible(new Vector2(pos.x, pos.y)))
            return false;

        int height = GridEx.GetHeight(grid, pos);
        if (height < 0)
            return false;

        var block = GridEx.GetBlock(grid, new Vector3Int(pos.x, height, pos.y));

        return block.type == m_blockType;
    }

    public override void End()
    {

    }

}
