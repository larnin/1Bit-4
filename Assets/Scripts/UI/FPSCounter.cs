using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;

public class FPSCounter : MonoBehaviour
{
    [SerializeField] bool m_displayDetails = true;

    TMP_Text m_text;

    float lastFPS = 0;

    private void Awake()
    {
        m_text = GetComponent<TMP_Text>();
        if (m_text == null)
            Destroy(gameObject);
    }

    private void Update()
    {
        string data = "";

        if(m_displayDetails)
        {
            int jobs = ThreadPool.GetPendingJobCount();
            data += "Jobs: " + jobs.ToString() + "\n";

            if(BuildingList.instance != null)
            {
                int buildings = BuildingList.instance.GetBuildingNb();
                data += "Buildings: " + buildings.ToString() + "\n";
            }

            if(EntityList.instance != null)
            {
                int entities = EntityList.instance.GetEntityNb();
                data += "Entities: " + entities.ToString() + "\n";
            }
        }

        float fps = 1 / Time.deltaTime;


        if (lastFPS == 0)
            lastFPS = fps;

        const float percent = 0.1f;

        lastFPS = lastFPS * (1 - percent) + fps * percent;

        data += Mathf.RoundToInt(lastFPS).ToString();

        m_text.text = data;
    }
}
