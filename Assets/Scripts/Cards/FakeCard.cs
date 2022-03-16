using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FakeCard : MonoBehaviour
{
    public void loadTextureFront(Texture2D texture)
    {
        Renderer[] temp = GetComponentsInChildren<Renderer>();
        temp[0].material.mainTexture = texture;
        temp[0].material.mainTextureScale = new Vector2(-1f, -1f);
    }

    public void loadTextureBack(Texture2D texture)
    {
        Renderer[] temp = GetComponentsInChildren<Renderer>();
        temp[2].material.mainTexture = texture;
    }
}