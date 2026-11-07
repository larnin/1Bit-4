using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class DebugAnimationScene : MonoBehaviour
{
    [SerializeField] List<SimpleTextAnimation> m_textAnimation;

    bool m_loadingScreenDisplay = false;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            if(m_textAnimation != null)
            {
                foreach (var t in m_textAnimation)
                    t.Restart();
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
            ToggleFade();
    }

    void ToggleFade()
    {
        m_loadingScreenDisplay = !m_loadingScreenDisplay;
        Event<ShowLoadingScreenEvent>.Broadcast(new ShowLoadingScreenEvent(m_loadingScreenDisplay));
    }
}
