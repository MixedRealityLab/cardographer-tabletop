using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckDropdown : MonoBehaviour
{
    public GameObject deckSpawnerObject;

    void Start()
    {
        deckSpawnerObject = GameObject.FindGameObjectWithTag("DeckSpawner");
    }

    public void setSpawnDeck(int input)
    {
        if(deckSpawnerObject == null) deckSpawnerObject = GameObject.FindGameObjectWithTag("DeckSpawner");
        deckSpawnerObject.GetComponent<DeckSpawner>().setSpawnDeck(input);
    }

    public void spawnDeck()
    {
        Debug.Log("Dropdown Deck spawn");
        if (deckSpawnerObject == null) deckSpawnerObject = GameObject.FindGameObjectWithTag("DeckSpawner");
        deckSpawnerObject.GetComponent<DeckSpawner>().SpawnDeck();
    }
}
