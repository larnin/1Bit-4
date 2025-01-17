using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class GameOverMenu : MonoBehaviour
{
    [SerializeField] string m_menuName;
    [SerializeField] float m_appearOffset;
    [SerializeField] float m_appearDuration;
    [SerializeField] Ease m_appearCurve;

    bool m_selected = false;

    public void OnContinue()
    {
        if (m_selected)
            return;

        m_selected = true;

        SceneSystem.changeScene(m_menuName);
    }

    private void Awake()
    {
        var obj = transform.Find("Pivot");
        if(obj != null)
        {
            var target = obj.localPosition;
            var start = obj.localPosition + new Vector3(0, m_appearOffset, 0);

            obj.localPosition = start;

            obj.DOLocalMove(target, m_appearDuration).SetEase(m_appearCurve);
        }
    }
}
