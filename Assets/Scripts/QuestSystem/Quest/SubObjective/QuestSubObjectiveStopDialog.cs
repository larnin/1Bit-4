using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class QuestSubObjectiveStopDialog : QuestSubObjectiveBase
{
    public override bool IsCompleted()
    {
        return true;
    }

    public override void Start()
    {
        if (MenuSystem.instance == null)
            return;

        MenuSystem.instance.CloseMenu<DialogPopup>();
    }

    public override void Update(float deltaTime) { }

    public override void End() { }
}
