using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

public class LevelSelectionMenu : MonoBehaviour
{
    [SerializeField] GameObject m_OneLevelPrefab;
    [SerializeField] float m_distanceBetweenLevel = 200;
    [SerializeField] float m_transitionDuration = 0.5f;

    GameObject m_leftButton;
    GameObject m_rightButton;

    GameObject m_levelsPivot;
    int m_levelIndex = 0;

    List<LevelSelectionElement> m_elements = new List<LevelSelectionElement>();

    private void Awake()
    {
        var leftTr = transform.Find("ArrowLeft");
        if (leftTr != null)
            m_leftButton = leftTr.gameObject;
        var rightTr = transform.Find("ArrowRight");
        if (rightTr != null)
            m_rightButton = rightTr.gameObject;

        var pivotTr = transform.Find("ElementPivot");
        if (pivotTr != null)
            m_levelsPivot = pivotTr.gameObject;

        PopulateLevels();

        m_leftButton.SetActive(false);
    }

    void PopulateLevels()
    {
        for(int i = 0; i < Global.instance.levelsData.Levels.Count; i++)
        {
            var instance = Instantiate(m_OneLevelPrefab);
            var element = instance.GetComponent<LevelSelectionElement>();
            if(element == null)
            {
                Destroy(instance);
                break;
            }

            element.SetLevelIndex(i);

            instance.transform.SetParent(m_levelsPivot.transform, false);
            instance.transform.localPosition = new Vector3(i * m_distanceBetweenLevel, 0, 0);

            m_elements.Add(element);
        }

        var infiniteInstance = Instantiate(m_OneLevelPrefab);
        var infiniteElement = infiniteInstance.GetComponent<LevelSelectionElement>();
        if(infiniteElement == null)
        {
            Destroy(infiniteInstance);
            return;
        }

        infiniteElement.SetInfiniteMode();

        infiniteInstance.transform.SetParent(m_levelsPivot.transform, false);
        infiniteInstance.transform.localPosition = new Vector3(Global.instance.levelsData.Levels.Count * m_distanceBetweenLevel, 0, 0);

        m_elements.Add(infiniteElement);
    }

    void UpdateArrowsButtons()
    {
        if (m_levelIndex <= 0)
            m_leftButton.SetActive(false);
        else m_leftButton.SetActive(true);

        if (m_levelIndex >= m_elements.Count - 1)
            m_rightButton.SetActive(false);
        else m_rightButton.SetActive(true);
    }

    public void ClickLeft()
    {
        if (m_levelIndex <= 0)
            return;

        m_levelIndex--;

        m_levelsPivot.transform.DOLocalMoveX(-m_levelIndex * m_distanceBetweenLevel, m_transitionDuration);

        UpdateArrowsButtons();
    }

    public void ClickRight()
    {
        if (m_levelIndex >= m_elements.Count - 1)
            return;

        m_levelIndex++;

        m_levelsPivot.transform.DOLocalMoveX(-m_levelIndex * m_distanceBetweenLevel, m_transitionDuration);

        UpdateArrowsButtons();
    }

    public void ClickPlay()
    {
        //todo
    }

    public void ClickCancel()
    {
        Destroy(gameObject);
    }
}
