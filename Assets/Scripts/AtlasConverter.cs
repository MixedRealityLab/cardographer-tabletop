using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using System.Threading.Tasks;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using UnityEngine.UI;

[Serializable]
public struct DeckInfo
{
    //Card back should be the last card
    //Currently only generic card backs for a deck are supported
    //Total Cards in atlas
    public int cardCount;
    //List of X Y card counts for each sprite sheet
    public List<Vector2> cardXY;
    //Name of deck
    public string deckName;
    //Atlas URL list
    public List<string> deckAtlasURLs;
    //Card List
    public List<string> cards;
    //Card Texture list
    public List<Texture2D> cardTextures;
    public Vector3 cardSize;
    public string deckBuilder;
}

[Serializable]
public class Deck
{
    public string name;
    //public int atlasCount;
    public string[] atlasURLs;
    public int cardCount;
    public int[] cardX;
    public int[] cardY;
    public string[] cardInfo;
    public int[] cardSize;
    public string deckBuilderID;
}

[Serializable]
public class Board
{
    public string boardName;
    public string boardID;
    public string boardTitle;
    public string[] boardImageURL;
    public string boardDeck;
    public int[] boardSize;
}

[Serializable]
public struct NamedAtlas
{
    public string deckName;
    public List<Texture2D> deckAtlas;
}

class ForceAcceptAll : CertificateHandler {
    protected override bool ValidateCertificate(byte[] certificateData)
    {
        return true;
    }
}

public class AtlasConverter : MonoBehaviour
{
    public List<DeckInfo> decks;

    public List<Deck> jsonDecks;
    public List<Board> jsonBoards;

    public List<NamedAtlas> atlasRef;
    Deck[] downloadedDecks;
    Board[] downloadedBoards;

    public GameObject loaderPrefab;

    public DeckSpawner roomDeckSpawner;

    public ServerManagement serverMan;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Atlas Start");
        decks = new List<DeckInfo>();
        atlasRef = new List<NamedAtlas>();
        jsonDecks = new List<Deck>();
        
        //StartCoroutine(getLocalDeckFile());
        //GetDeckFile();
        GetBoardFile();
    }

    public List<string> getCategiorySelection()
    {
        List<string> categories = new List<string>();
        
        for(int i = 0; i < jsonDecks.Count; i++)
        {
            categories.Add(jsonDecks[i].name);
        }

        return categories;
    }

    public List<string> getBoardSelection(){
        List<string> boards = new List<string>();

        for(int i = 0; i < jsonBoards.Count; i++)
        {
            boards.Add(jsonBoards[i].boardName);
        }

        return boards;
    }

    public DeckInfo getDeck(string name)
    {
        DeckInfo temp = new DeckInfo();
        Debug.Log("Deck name given: " + name);
        if (decks.Exists(x => x.deckName == name))
        {
            if (!string.IsNullOrEmpty(name))
            {
                Debug.Log("Found deck: " + name);
                temp = decks.Find(x => x.deckName == name);
            }
        }
        else
        {
            Debug.Log("No deck found under: "+ name + ", downloading");
            temp = downloadDeck(name);
        }

        return temp;
    }

    public Board getBoard(string name)
    {
        return jsonBoards.Find(x => x.boardName == name);
    }

    IEnumerator APICall(string url, List<Texture2D> atlasLocal, string atlasName)
    {
        Debug.Log("Get Atlas Request for: " + atlasName);
        GameObject cloneLoader = Instantiate(loaderPrefab);
        cloneLoader.transform.SetParent(transform);
        Image temp = cloneLoader.GetComponent<Image>();
        bool loaded = false;
        Davinci
                .get()
                .load(url)
                .setCached(true)
                .into(temp)
                .withEndAction(() =>
                {
                    loaded = true;
                })
                .start();

        Debug.Log("Waiting on atlas");
        //yield return new WaitUntil(() => (Texture2D)temp.material.mainTexture != null);
        yield return new WaitUntil(() => loaded);

        Debug.Log("Atlas Ref exists: " + atlasRef.Exists(x => x.deckName == atlasName));

        NamedAtlas foundAtlas = atlasRef.Find(x => x.deckName == atlasName);

        Debug.Log("Atlas name: " + foundAtlas.deckName);

        foundAtlas.deckAtlas.Add((Texture2D)temp.mainTexture);
        //atlasLocal.Add((Texture2D)temp.mainTexture);
        atlasLocal.Add(CleanAtlas((Texture2D)temp.mainTexture));
        Destroy(cloneLoader);
    }


    public IEnumerator AtlasDownload(string deckName)
    {
        if (!string.IsNullOrEmpty(deckName))
        {
            if (atlasRef.Exists(x => x.deckName == deckName))
            {
                Debug.Log("Atlas already loaded");
                yield break;
            }
            Debug.Log("Loading atlas for deck: " + deckName);
            List<Texture2D> atlasLocal = new List<Texture2D>();
            int atlasCount = 0;

            string[] urlData = jsonDecks.Find( x => x.name == deckName).atlasURLs;
            atlasCount = urlData.Length;

            if (!atlasRef.Exists(x => x.deckName == deckName))
            {
                NamedAtlas newAtlas = new NamedAtlas();
                newAtlas.deckName = deckName;

                List<Texture2D> newAtlasTexList = new List<Texture2D>();
                newAtlas.deckAtlas = newAtlasTexList;
                atlasRef.Add(newAtlas);
            }

            for (int i = 0; i < atlasCount; i++)
            {
                StartCoroutine(APICall(urlData[i], atlasLocal, deckName));
            }
            yield return new WaitUntil(() => atlasLocal.Count == atlasCount && atlasCount != 0);
        }
        else
        {
            yield return null;
        }
        if( decks.Exists( x => x.deckName == deckName))
        {
            Debug.Log("Deck already loaded");
            yield return false;
        }
    }

    //Dead Space Removal
    Texture2D CleanAtlas( Texture2D atlasToClean )
    {
        Texture2D cleanedTexture;
        Color deadSpaceColour;
        Color[] tL, tR, bL, bR;
        //Step 1: Sample corners. Top Left, Top Right, Bottom Left, Bottom Right. Image is flipped (upside-down)

        tL = atlasToClean.GetPixels(0, 0, 10, 10);
        tR = atlasToClean.GetPixels(atlasToClean.width -10 , 0, 10, 10);
        bL = atlasToClean.GetPixels(0, atlasToClean.height -10, 10 , 10);
        bR = atlasToClean.GetPixels(atlasToClean.width - 10, atlasToClean.height - 10, 10 , 10);

        //Step 2: From samples, get pixel colour of dead space

        if(tL[0] != bR[0])
        {
            //If the top left pixel is missmatched to the bottom right, means the last pixel (Top Left) 
            // might be the card back, or is dead space

            if(tL[0] == tR[0])
            {
                //If the top left and top right pixel is the same, it is dead space
                //This is the colour to remove from the atlas
                deadSpaceColour = tL[0];
            }
            else
            {
                //If the pixels are different, there is no issue, can return the texture
                return atlasToClean;
            }
        }
        else
        {
            //Unlikely to have any dead space in atlas, return it
            return atlasToClean;
        }
        //Step 3: Scan original texture bottom to top, until the pixel colour of dead space stops
        Color[] column = atlasToClean.GetPixels(0, 0, 1, atlasToClean.height );
        int deadspaceCutOff = 0;
        for(int c = 0; c < column.Length; c++)
        {
            if(column[c] != deadSpaceColour)
            {
                deadspaceCutOff = c;
                break;
            }
        }
        //Step 4: Cut texture at x,y where the pixel colour stops
        Color[] cutPixels = atlasToClean.GetPixels(0,deadspaceCutOff, atlasToClean.width, atlasToClean.height - deadspaceCutOff);
        cleanedTexture = new Texture2D(atlasToClean.width, atlasToClean.height - deadspaceCutOff);
        cleanedTexture.SetPixels(0,0, atlasToClean.width, atlasToClean.height - deadspaceCutOff, cutPixels);
        cleanedTexture.Apply();
        //Step 5: Return newly cut texture
        return cleanedTexture;
    }
    //Dead Space Removal

    List<Texture2D> AtlasToList(DeckInfo deck)
    {
        List<Texture2D> cardTextures = new List<Texture2D>();
        int count = 0;
        Debug.Log("Atlas slice: " + deck.deckName);
        Debug.Log("Card Count in atlas slice: " + deck.cardCount);
        Debug.Log("Atlas URL count for: " + deck.deckName + " is: " + deck.deckAtlasURLs.Count);
        for (int i = 0; i < deck.deckAtlasURLs.Count; i++)
        {
            Debug.Log("Atlas Ref count: " + atlasRef.Count);
            foreach (var item in atlasRef)
            {
                Debug.Log("Item Name: " + item.deckName);
                Debug.Log("Item atlas count: " + item.deckAtlas.Count);
            }
            //if (atlasRef.Exists(x => x.deckName == deck.deckName)) return null;
            Debug.Log("Card tex slicing begins here");
            Texture2D atlas = atlasRef.Find( x => x.deckName == deck.deckName ).deckAtlas[i];
            
            int batchX, batchY;
            batchX = atlas.width / (int)deck.cardXY[i].x;
            batchY = atlas.height / (int)deck.cardXY[i].y;

            for (int y = (int)deck.cardXY[i].y; y > 0; y--)
            {
                for (int x = 0; x < deck.cardXY[i].x; x++)
                {
                    count++;

                    Texture2D destTex = new Texture2D(batchX, batchY);

                    Color[] pix = atlas.GetPixels(x * batchX, (y - 1) * batchY, batchX, batchY);
                    destTex.SetPixels(pix);
                    destTex.Apply();

                    cardTextures.Add(destTex);
                    Debug.Log("Generating tex no: " + x);
                    if (count == deck.cardCount) break;
                }
                if (count == deck.cardCount) break;
            }
        }
        return cardTextures;
    }

    //Function to retrieve decks from server
    DeckInfo downloadDeck(string deck)
    {
        DeckInfo temp = new DeckInfo();
        Deck foundDeck = jsonDecks.Find(x => x.name == deck);
        List<string> tempCards = new List<string>();
        List<string> tempURLs = new List<string>();
        List<Vector2> tempCardXY = new List<Vector2>();

        temp.deckName = foundDeck.name;
        temp.cardCount = foundDeck.cardCount;
        temp.cardSize = new Vector3(foundDeck.cardSize[0], 0 , foundDeck.cardSize[1]);
        //Populate URLS
        for(int i = 0; i < foundDeck.atlasURLs.Length; i++)
        {
            tempURLs.Add(foundDeck.atlasURLs[i]);
        }
        temp.deckAtlasURLs = tempURLs;

        //Populate Cards
        for (int c = 0; c < foundDeck.cardInfo.Length; c++)
        {
            tempCards.Add(foundDeck.cardInfo[c]);
        }
        temp.cards = tempCards;

        //Populate Card X Y
        for(int k = 0; k < foundDeck.cardX.Length; k++)
        {
            tempCardXY.Add(new Vector2(foundDeck.cardX[k], foundDeck.cardY[k]));
        }
        temp.cardXY = tempCardXY;

        temp.cardTextures = new List<Texture2D>();
        temp.cardTextures = AtlasToList(temp);
        temp.deckBuilder = foundDeck.deckBuilderID;
        decks.Add(temp);
        return temp;
    }

    //New Method
    public List<Card> DeckConstructor(string deck)
    {
        List<Card> deckCards = new List<Card>();
        Deck foundDeck = jsonDecks.Find(x => x.name == deck);
        List<string> tempCards = new List<string>();

        for (int c = 0; c < foundDeck.cardInfo.Length; c++)
        {
            Card tCard = new Card();
            tCard.globalID = -1;
            tCard.cardSize = new Vector3(foundDeck.cardSize[0], 0, foundDeck.cardSize[1]);
            tCard.cardName = foundDeck.cardInfo[c];
            tCard.cardID = c;
            tCard.cardDeck = deck;

            deckCards.Add(tCard);
        }

        return deckCards;
    }

    //New Method

    IEnumerator GetRequest(string url, Action<UnityWebRequest> callback)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
#if UNITY_SERVER
            // Insecure hack to force unity to ignore the certificate
            if(url.StartsWith("https://cardographer.cs.nott.ac.uk/")) {
                request.certificateHandler = new ForceAcceptAll();
            }
#endif
            // Send the request and wait for a response
            yield return request.SendWebRequest();
            callback(request);
        }
    }

    public void AccessDeckInfo(string info)
    {
        StartCoroutine(getLocalDeckFile(info));
    }

    IEnumerator getLocalDeckFile(string info)
    {
        yield return new WaitUntil( () => info != "");
        Debug.Log("Deckinfo is loaded!");
        downloadedDecks = JsonConvert.DeserializeObject<Deck[]>(info);

        //List<string> categories = new List<string>();

        foreach (Deck deck in downloadedDecks)
        {
            if (jsonDecks.Exists(x => x.name == deck.name)) break;
            Debug.Log(deck.name);
            jsonDecks.Add(deck);
        }
        yield return 0;
    }

    public void GetDeckFile()
    {
        StartCoroutine(GetRequest("https://cardographer.cs.nott.ac.uk/DeckInfo.json", (UnityWebRequest req) =>
        {
            if (req.isNetworkError || req.isHttpError)
            {
                Debug.Log($"{req.error}: {req.downloadHandler.text}");
            }
            else
            {
                Debug.Log(req.downloadHandler.text);

                downloadedDecks = JsonConvert.DeserializeObject<Deck[]>(req.downloadHandler.text);
                
                //List<string> categories = new List<string>();

                foreach (Deck deck in downloadedDecks)
                {
                    Debug.Log(deck.name);
                    //categories.Add(deck.name);
                    jsonDecks.Add(deck);
                }
                //if(roomDeckSpawner == null) roomDeckSpawner = GameObject.FindGameObjectWithTag("DeckSpawner").GetComponent<DeckSpawner>();
                //roomDeckSpawner.populateDropdown(categories);
            }
        }));
    }

    public void GetBoardFile()
    {
        StartCoroutine(GetRequest("https://cardographer.cs.nott.ac.uk/Boards.json", (UnityWebRequest req) =>
        {
            if (req.isNetworkError || req.isHttpError)
            {
                Debug.Log($"{req.error}: {req.downloadHandler.text}");
            }
            else
            {
                Debug.Log(req.downloadHandler.text);

                downloadedBoards = JsonConvert.DeserializeObject<Board[]>(req.downloadHandler.text);
                foreach (Board board in downloadedBoards)
                {
                    Debug.Log(board.boardTitle);
                    jsonBoards.Add(board);
                }
            }
        }));
    }
}