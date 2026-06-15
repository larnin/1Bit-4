using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SaveGlobal
{
    public int lastPlayedSlot = -1;

    public void Load(JsonDocument doc)
    {
        
        var headerElt = doc.GetRoot();
        if (headerElt.IsJsonObject())
        {
            var headerObj = headerElt.JsonObject();

            var lastSlotJson = headerObj.GetElement("lastSlot");
            if (lastSlotJson != null && lastSlotJson.IsJsonNumber())
                lastPlayedSlot = lastSlotJson.Int();
        }
    }

    public JsonDocument Save()
    {
        var doc = new JsonDocument();
        var root = new JsonObject();
        doc.SetRoot(root);

        root.AddElement("lastSlot", lastPlayedSlot);

        return doc;
    }
}

