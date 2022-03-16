using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckUiMenu : BaseUiMenu
{
    public override void SpawnAnnotation()
    {
        Target.GetComponent<DeckBase>().AskCreateAnnotation("", Vector3.zero);
        Controller.ToggleDeckMenu(Vector3.zero);
    }

    public override void FlipTarget()
    {
        Target.GetComponent<DeckBase>().UiCardFlip();
        Controller.ToggleDeckMenu(Vector3.zero);
    }

    public void Delete()
    {
        Target.GetComponent<DeckBase>().AskDelete();
        Controller.ToggleDeckMenu(Vector3.zero);
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
