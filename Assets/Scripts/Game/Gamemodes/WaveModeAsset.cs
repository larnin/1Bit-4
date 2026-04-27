using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class WaveModeEntity
{
    [Serializable]
    public class WaveStat
    {
        public StatType stat;
        public float value = 0;
    }

    public EntityChoice entityType;
    public int count = 1;

    public List<WaveStat> stats = new List<WaveStat>();
}

[Serializable]
public class WaveModeWave
{
    public float delayBeforeFirstEntity = 0;
    public float spawnDuration = 10;
    public List<WaveModeEntity> entities = new List<WaveModeEntity>();
}


[Serializable]
public class WaveModePortal
{
    public string positionName;
    public BuildingType portalType;
    public int waveStart;
    public bool destroyAfterLastWave = false;

    public List<WaveModeWave> waves = new List<WaveModeWave>();
}

[CreateAssetMenu(fileName = "WaveMode", menuName = "Game/Gamemode/WaveMode", order = 1)]
public class WaveModeAsset : GamemodeAssetBase
{
    public List<WaveModePortal> portals = new List<WaveModePortal>();

    public float delayBeforeFirstWave = 60;
    public float delayBetweenWave = 10;

    public override GamemodeBase MakeGamemode(GamemodeSystem owner)
    {
        WaveMode mode = new WaveMode(this, owner);

        return mode;
    }
}
