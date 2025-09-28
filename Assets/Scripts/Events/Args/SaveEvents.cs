using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SaveEvent
{
    public JsonObject obj;

    public SaveEvent(JsonObject _obj)
    {
        obj = _obj;
    }
}

public class LoadEvent
{
    public JsonObject obj;

    public LoadEvent(JsonObject _obj)
    {
        obj = _obj;
    }
}
