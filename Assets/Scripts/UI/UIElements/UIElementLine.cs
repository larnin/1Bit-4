using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;

public class UIElementLine : UIElementBase
{
    Image m_image;

    private void Awake()
    {
        m_image = GetComponentInChildren<Image>();
    }
}
