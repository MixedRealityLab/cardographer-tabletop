using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUiMenu : MonoBehaviour
{
    protected UIController Controller;
    public GameObject Target;

    public virtual void FlipTarget()
    {

    }

    public virtual void SpawnAnnotation()
    {

    }

    public void SetTarget(GameObject target)
    {
        Target = target;
    }

    protected virtual void Start()
    {
        Controller = GameObject.FindGameObjectWithTag("UIController").GetComponent<UIController>();
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        
    }
}
