using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WaveMode : GamemodeBase
{
    enum State
    {
        Waiting,
        Spawning,
    }

    WaveModeAsset m_asset;

    State m_state = State.Waiting;
    int m_waveIndex = 0;
    float m_timerMax = 0;
    float m_timer = 0;

    List<WaveModeSpawner> m_portals = new List<WaveModeSpawner>();

    public WaveMode(WaveModeAsset asset, GamemodeSystem owner)
    : base(owner)
    {
        m_asset = asset;

        for(int i = 0; i < m_asset.portals.Count; i++)
            m_portals.Add(new WaveModeSpawner(this, i));
    }

    public override GamemodeAssetBase GetAsset()
    {
        return m_asset;
    }

    public WaveModeAsset GetWaveAsset()
    {
        return m_asset;
    }

    public override GamemodeStatus GetStatus()
    {
        if (m_waveIndex >= GetWaveNb())
            return GamemodeStatus.Completed;
        return GamemodeStatus.Ongoing;
    }

    public override void Begin()
    {
        StartWave(0);
    }

    public override void Process()
    {
        if(m_state == State.Waiting)
        {
            m_timer += Time.deltaTime;

            float remainingTime = m_timerMax - m_timer;
            foreach (var p in m_portals)
                p.StartAppear(remainingTime, m_waveIndex);

            if (remainingTime <= 0)
                StartSpawning();
        }
        else if(m_state == State.Spawning)
        {
            bool allEnded = true;
            float deltaTime = Time.deltaTime;

            foreach (var portal in m_portals)
                allEnded &= portal.ProcessWave(deltaTime);

            if (allEnded)
                StartWave(m_waveIndex + 1);
        }
    }

    public override void End()
    {
        foreach(var portal in m_portals)
            portal.OnEnd();
    }

    void StartWave(int index)
    {
        m_waveIndex = index;
        m_state = State.Waiting;
        m_timerMax = 100;
        m_timer = 0;

        if (index >= GetWaveNb())
            return;

        if (index == 0)
            m_timerMax = m_asset.delayBeforeFirstWave;
        else m_timerMax = m_asset.delayBetweenWave;
    }

    void StartSpawning()
    {
        if(m_waveIndex >= GetWaveNb())
        {
            m_timerMax = 100;
            m_timer = 0;
            return;
        }

        m_state = State.Spawning;
        foreach (var portal in m_portals)
            portal.StartWave(m_waveIndex);
    }

    int GetWaveNb()
    {
        if (m_asset == null)
            return 0;

        int count = 0;

        foreach(var portal in m_asset.portals)
        {
            int portalCount = portal.waveStart + portal.waves.Count;
            if (portalCount > count)
                count = portalCount;
        }

        return count;
    }
}