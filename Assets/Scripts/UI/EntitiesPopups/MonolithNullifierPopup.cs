using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class MonolithNullifierPopup : EntityPopupBase
{
    [SerializeField] Toggle m_activeCheckbox;

    protected override void OnInit()
    {
        if (m_activeCheckbox == null || m_entity == null)
            return;

        var nullifier = m_entity.GetComponent<BuildingMonolithNullifier>();
        if (nullifier == null)
            return;

        m_activeCheckbox.isOn = nullifier.IsNullifierActive();
    }

    public void OnActiveChange(bool toggled)
    {
        var nullifier = m_entity.GetComponent<BuildingMonolithNullifier>();
        if (nullifier == null)
            return;

        nullifier.SetNullifierActive(toggled);
    }
}
