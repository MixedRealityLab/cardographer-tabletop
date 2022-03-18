using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardPool : NetworkBehaviour
{
    struct CardRef
    {
        public int globalId;
        public GameObject card;
    }

    List<CardRef> cardRefList;
    public List<GameObject> cards;
    public int cardCount = 0;

    readonly public SyncList<GameObject> cardSync = new SyncList<GameObject>();
    public readonly SyncDictionary<int, GameObject> cardDictionary = new SyncDictionary<int, GameObject>();

  
    void Start()
    {
        cards = new List<GameObject>();
        cardRefList = new List<CardRef>();
    }

    public int getGlobalIndex()
    {
        int temp = cardCount;
        cardCount++;
        return temp;
    }
}
