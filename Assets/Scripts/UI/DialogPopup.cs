using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogPopup : MonoBehaviour
{
    enum State
    {
        Writing,
        Waiting,
    }

    [SerializeField] float m_displaySpeed = 10;
    [SerializeField] float m_buttonAnimationSpeed = 2;
    [SerializeField] float m_buttonAnimationAmplitude = 5;
    [SerializeField] Button m_nextButton;
    [SerializeField] TMP_Text m_textWidget;

    bool m_waitInputEnd = false;
    List<string> m_texts = new List<string>();

    int m_currentIndex = 0;
    float m_timer = 0;
    State m_state = State.Waiting;

    Vector3 m_nextButtonInitialPos;

    public void DisplayText(string text, bool waitInput)
    {
        DisplayTexts(new List<string>{ text }, waitInput);
    }

    private void Start()
    {
        m_nextButtonInitialPos = m_nextButton.transform.localPosition;
    }

    public void DisplayTexts(List<string> texts, bool waitInputEnd)
    {
        m_texts = texts;
        m_waitInputEnd = waitInputEnd;

        StartWriting(0);
    }


    private void Update()
    {
        switch(m_state)
        {
            case State.Waiting:
                UpdateWaiting();
                break;

            case State.Writing:
                UpdateWriting();
                break;
        }
    }

    void UpdateWaiting()
    {
        if (m_nextButton.gameObject.activeSelf && Input.GetKeyDown(KeyCode.Return))
            OnNextButtonClick();

        if(m_nextButton.gameObject.activeSelf)
        {
            float offset = Mathf.Sin(Time.time * Mathf.PI * 2 * m_buttonAnimationSpeed) * m_buttonAnimationAmplitude;
            Vector3 pos = m_nextButtonInitialPos + new Vector3(0, offset, 0);
            m_nextButton.transform.localPosition = pos;
        }
    }

    void UpdateWriting()
    {
        m_timer += Time.deltaTime;

        int charIndex = (int)(m_timer * m_displaySpeed);
        if(charIndex >= m_texts[m_currentIndex].Length)
        {
            m_textWidget.text = m_texts[m_currentIndex];
            StartWaiting();
            return;
        }

        string displayText = m_texts[m_currentIndex].Substring(0, charIndex) + "<color=#00000000>" + m_texts[m_currentIndex].Substring(charIndex) + "</color>";
        m_textWidget.text = displayText;
    }

    public void OnNextButtonClick()
    {
        if (m_state == State.Waiting)
            StartWriting(m_currentIndex + 1);
    }
    void StartWriting(int index)
    {
        if(index >= m_texts.Count)
        {
            if (MenuSystem.instance != null)
                MenuSystem.instance.CloseMenu<DialogPopup>();
            return;
        }

        m_timer = 0;
        m_currentIndex = index;
        m_textWidget.SetText("");

        m_state = State.Writing;

        m_nextButton.gameObject.SetActive(false);
    }

    void StartWaiting()
    {
        if (m_currentIndex < m_texts.Count - 1 || m_waitInputEnd)
            m_nextButton.gameObject.SetActive(true);

        m_state = State.Waiting;
    }
}
