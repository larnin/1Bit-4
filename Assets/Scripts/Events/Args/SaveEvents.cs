using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SaveLevelEvent
{
    public JsonObject obj;

    public SaveLevelEvent(JsonObject _obj)
    {
        obj = _obj;
    }
}

public class LoadLevelEvent
{
    public JsonObject obj;

    public LoadLevelEvent(JsonObject _obj)
    {
        obj = _obj;
    }
}

public class SaveGameEvent
{
    public JsonObject obj;

    public SaveGameEvent(JsonObject _obj)
    {
        obj = _obj;
    }
}

public class LoadGameEvent
{
    public JsonObject obj;

    public LoadGameEvent(JsonObject _obj)
    {
        obj = _obj;
    }
}
