using DG.Tweening;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeSceneParams
{
    public string sceneName;
    public bool instant = false;
    public bool skipFadeOut = false;
    public bool skipFadeIn = false;
    public Action finishedCallback;

    public ChangeSceneParams(string _sceneName)
    {
        sceneName = _sceneName;
    }
}

public class SceneSystem
{
    const string defaultSceneName = "MainMenu";
    const float delay = 1.5f;

    static bool m_starting = false;

    public static void changeScene(ChangeSceneParams scene)
    {
        if (m_starting)
            return;
        m_starting = true;

        if (!Application.CanStreamedLevelBeLoaded(scene.sceneName))
        {
            Debug.LogError("Can't load scene " + scene.sceneName + ". Back to main menu");
            scene.sceneName = defaultSceneName;
        }

        Time.timeScale = 1.0f;

        if(!scene.skipFadeIn)
            Event<ShowLoadingScreenEvent>.Broadcast(new ShowLoadingScreenEvent(true));

        if (!scene.instant)
        {
            DOVirtual.DelayedCall(delay, () =>
            {
                var operation = SceneManager.LoadSceneAsync(scene.sceneName);
                execChangeScene(operation, scene);
            });
        }
        else
        {
            var operation = SceneManager.LoadSceneAsync(scene.sceneName);
            execChangeScene(operation, scene);
        }
    }

    static void execChangeScene(AsyncOperation operation, ChangeSceneParams scene)
    {
        if (!operation.isDone)
            DOVirtual.DelayedCall(0.1f, () => execChangeScene(operation, scene));
        else
        {
            if(!scene.skipFadeOut)
                Event<ShowLoadingScreenEvent>.Broadcast(new ShowLoadingScreenEvent(false));
            m_starting = false;
            if (scene.finishedCallback != null)
                scene.finishedCallback();
        }
    }
}