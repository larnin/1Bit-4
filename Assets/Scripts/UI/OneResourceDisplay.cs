using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OneResourceDisplay : MonoBehaviour
{
    [SerializeField] Sprite m_spriteMore;
    [SerializeField] Sprite m_spriteLess;

    public void SetData(ResourceType type, float count)
    {
        UpdateResourceSprite(type);
        UpdateResourceStorage(count, 0);
        DisableDelta();
    }

    public void SetData(ResourceType type, float count, float maxCount)
    {
        UpdateResourceSprite(type);
        UpdateResourceStorage(count, maxCount);
        DisableDelta();
    }

    public void SetDataWithDelta(ResourceType type, float count, float delta)
    {
        UpdateResourceSprite(type);
        UpdateResourceStorage(count, 0);
        SetDelta(delta);
    }

    public void SetDataWithDelta(ResourceType type, float count, float maxCount, float delta)
    {
        UpdateResourceSprite(type);
        UpdateResourceStorage(count, maxCount);
        SetDelta(delta);
    }

    void UpdateResourceSprite(ResourceType type)
    {
        var tr = transform.Find("ResourceSprite");
        if (tr == null)
            return;
        var img = tr.GetComponent<Image>();
        if (img == null)
            return;

        var resourceData = Global.instance.resourceDatas.GetResource(type);
        if (resourceData != null)
            img.sprite = resourceData.sprite;
    }

    void UpdateResourceStorage(float count, float maxCount)
    {
        var tr = transform.Find("StockTxt");
        if (tr == null)
            return;
        var txt = tr.GetComponent<TMP_Text>();
        if (txt == null)
            return;
        string text = ((int)count).ToString();
        if (maxCount > 0)
            text += "/" + ((int)maxCount).ToString();
        txt.text = text;
    }

    void DisableDelta()
    {
        var img = transform.Find("StockEvolutionSprite");
        if (img != null)
            img.gameObject.SetActive(false);
        var txt = transform.Find("StockEvolutionTxt");
        if (txt != null)
            txt.gameObject.SetActive(false);
    }

    void SetDelta(float value)
    {
        var imgObj = transform.Find("StockEvolutionSprite");
        if (imgObj != null)
        {
            imgObj.gameObject.SetActive(true);
            var img = imgObj.GetComponent<Image>();
            if(img != null)
            {
                if (value > 0)
                    img.sprite = m_spriteMore;
                else if (value < 0)
                    img.sprite = m_spriteLess;
                else imgObj.gameObject.SetActive(false);
            }
        }
        var txtObj = transform.Find("StockEvolutionTxt");
        if (txtObj != null)
        {
            txtObj.gameObject.SetActive(true);
            var txt = txtObj.GetComponent<TMP_Text>();
            if (txt != null)
            {
                if(value < 10)
                    txt.text = value.ToString("#0.00");
                else if(value < 100)
                    txt.text = value.ToString("#0.0");
                else txt.text = ((int)value).ToString();
            }
        }
    }
}

