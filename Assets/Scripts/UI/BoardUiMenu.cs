using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardUiMenu : BaseUiMenu
{
    public override void SpawnAnnotation()
    {
        Target.GetComponent<BoardBase>().AskCreateAnnotation("", Vector3.zero);
        Controller.ToggleBoardMenu(Vector3.zero);
    }

    public override void FlipTarget()
    {
        Target.GetComponent<BoardBase>().UiCardFlip();
        Controller.ToggleBoardMenu(Vector3.zero);
    }

    public void ToggleLock()
    {
        Target.GetComponent<BoardBase>().ToggleBoardLock();
        Controller.ToggleBoardMenu(Vector3.zero);
    }

    public void Delete()
    {
        Target.GetComponent<BoardBase>().AskDelete();
        Controller.ToggleBoardMenu(Vector3.zero);
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
