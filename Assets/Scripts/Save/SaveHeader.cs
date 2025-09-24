using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class SaveHeader
{
    public SaveHeader()
    {
        empty = true;
        m_startPlayTime = Time.time;
    }

    float m_startPlayTime;

    public bool empty { get; private set; }

    public float playTime
    {
        get
        {
            return m_startPlayTime - Time.time;
        }
    }

    public string GetPlayTimeAsString()
    {
        int sec = Mathf.RoundToInt(playTime);
        int min = sec / 60; sec %= 60;
        int hours = min / 60; hours %= 60;
        int days = hours / 60; hours %= 24;

        string value = "";
        if (days > 0)
            value += days + "d";
        if (hours > 0)
            value += hours + "h";
        if (min > 0)
            value += min + "m";
        if (sec > 0)
            value += sec + "s";

        return value;
    }

    public void ResetPlayTime()
    {
        m_startPlayTime = Time.time;
    }

    void Start()
    {
        if (!empty)
            return;
        ResetPlayTime();
        empty = false;
    }

    public void Load(JsonDocument doc)
    {
        empty = false;

        var headerElt = doc.GetRoot();
        if(headerElt.IsJsonObject())
        {
            var headerObj = headerElt.JsonObject();

            var playTimeElt = headerObj.GetElement("PlayTime");
            if(playTimeElt != null && playTimeElt.IsJsonNumber())
            {
                float time = playTimeElt.Float();
                m_startPlayTime = Time.time - time;
            }
        }
    }

    public JsonDocument Save()
    {
        var doc = new JsonDocument();
        var root = new JsonObject();
        doc.SetRoot(root);

        root.AddElement("PlayTime", playTime);

        return doc;
    }
}