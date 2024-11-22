using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NRand;

public class GameInfos
{
    static GameInfos m_instance = null;

    public Settings settings = new Settings();
    public GameParams gameParams = new GameParams();
    public bool paused = false;

    public static GameInfos instance
    {
        get
        {
            if (m_instance == null)
                m_instance = new GameInfos();
            return m_instance;
        }
    }
}

public class Settings
{
    public Settings()
    {
        //todo load settings
    }
}

public class GameParams
{
    public int seed;
    public WorldSize worldSize;

    public GameParams()
    {
        worldSize = WorldSize.Small;
        seed = (int)StaticRandomGenerator<MT19937>.Get().Next();
    }
}

