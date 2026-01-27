using UnityEngine;
using System.Collections;

class AddScreenShakeEvent
{
    public ScreenShakeSourceType sourceType;
    public Vector3 sourcePosition;
    public GameObject sourceObject;
    public ScreenShakeBase screenShake;
    public int resultID;

    public AddScreenShakeEvent(ScreenShakeBase _screenShake)
    {
        sourceType = ScreenShakeSourceType.Infinite;
        screenShake = _screenShake;
    }

    public AddScreenShakeEvent(ScreenShakeBase _screenShake, Vector3 pos)
    {
        sourceType = ScreenShakeSourceType.Position;
        sourcePosition = pos;
        screenShake = _screenShake;
    }

    public AddScreenShakeEvent(ScreenShakeBase _screenShake, GameObject obj)
    {
        sourceType = ScreenShakeSourceType.GameObject;
        sourceObject = obj;
        screenShake = _screenShake;
    }
}

class StopScreenShakeEvent
{
    public int ID;

    public StopScreenShakeEvent(int id)
    {
        ID = id;
    }
}

class IsScreenShakePlayingEvent
{
    public int ID;

    public bool playing;

    public IsScreenShakePlayingEvent(int id)
    {
        ID = id;
        playing = false;
    }
}

class StopAllScreenShakeEvent { }
