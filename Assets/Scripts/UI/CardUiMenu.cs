using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardUiMenu : BaseUiMenu
{
    public override void SpawnAnnotation()
    {
        Target.GetComponent<CardBase>().AskCreateAnnotation("", Vector3.zero);
        Controller.ToggleCardMenu(Vector3.zero);
    }

    public override void FlipTarget()
    {
        Target.GetComponent<CardBase>().UiCardFlip();
        Controller.ToggleCardMenu(Vector3.zero);
    }

    public void Delete()
    {
        Target.GetComponent<CardBase>().AskDelete();
        Controller.ToggleCardMenu(Vector3.zero);
    }

    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
