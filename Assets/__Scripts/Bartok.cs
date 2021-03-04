using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Bartok : MonoBehaviour
{
    static public Bartok S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public Vector3 layoutCenter = Vector3.zero;
    public float handFanDegrees = 10f;
    public int numStaringCards = 7;
    public float drawTimeStagger = 0.1f;

    [Header("Set Dynamically")]
    public Deck deck;
    public List<CardBartok> drawPile;
    public List<CardBartok> discardPile;
    public List<Player> players;
    public CardBartok targetCard;

    private BartokLayout layout;
    private Transform layoutAnchor;

    private void Awake()
    {
        S = this;
    }

    private void Start()
    {
        deck = GetComponent<Deck>(); // Получить компонент Deck
        deck.InitDeck(deckXML.text); // Передать ему DeckXML
        Deck.Shuffle(ref deck.cards); // Перетасовать колоду

        layout = GetComponent<BartokLayout>(); //Получить ссылку на компонент Layout 
        layout.ReadLayout(layoutXML.text); // Передать ему LayoutXML
        drawPile = UpgradeCardsList(deck.cards);
        LayoutGame();
    }

    List<CardBartok> UpgradeCardsList(List<Card> lCD)
    {
        List<CardBartok> lCB = new List<CardBartok>();
        foreach(Card tCD in lCD)
        {
            lCB.Add(tCD as CardBartok);
        }
        return (lCB);
    }

    // Позиционирует все карты в drawPile
    public void ArrangeDrawPile()
    {
        CardBartok tCB;
        for (int i = 0; i < drawPile.Count; i++)
        {
            tCB = drawPile[i];
            tCB.transform.parent = layoutAnchor;
            tCB.transform.localPosition = layout.drawPile.pos;
            // Угол поворота начинается с 0
            tCB.faceUp = false;
            tCB.SetSortingLayerName(layout.drawPile.layerName);
            tCB.SetSortOrder(-i * 4); // Упорядочить от первых к последним
            tCB.state = CBState.drawpile;
        }
    }

    void LayoutGame()
    {
        // Создать пустой GameObject - точку приязки для расладки
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;                
            layoutAnchor.transform.position = layoutCenter;      
        }

        // Позиционировать свободные карты
        ArrangeDrawPile();

        // Настроить игроков
        Player pl;
        players = new List<Player>();
        foreach (SlotDef tSD in layout.slotDefs)
        {
            pl = new Player();
            pl.handSlotDef = tSD;
            players.Add(pl);
            pl.playerNum = players.Count;
        }
        players[0].type = PlayerType.human; // 0-й игрок - человек

        CardBartok tCB;
        // Раздать игрокам по семь карт
        for(int i = 0; i < numStaringCards; i++)
        {
            for(int j = 0; j < 4; j++)
            {
                tCB = Draw(); // Снять карту
                // Немного отложить начало перемещения карты.
                tCB.timeStart = Time.time + drawTimeStagger * (i * 4 + j);

                players[(j + 1)% 4].AddCard(tCB);
            }
        }

        Invoke("DrawFirstTarget", drawTimeStagger * (numStaringCards*4+4));
    }

    public void DrawFirstTarget()
    {
        // Перевернуть первую целевую карту лицевой стороной вверх
        CardBartok tCB = MoveToTarget(Draw());
    }

    /// <summary>
    /// Делает указанную карту целевой
    /// </summary>
    public CardBartok MoveToTarget(CardBartok tCB)
    {
        tCB.timeStart = 0;
        tCB.MoveTo(layout.discardPile.pos + Vector3.back);
        tCB.state = CBState.toTarget;
        tCB.faceUp = true;

        tCB.SetSortingLayerName("10");
        tCB.eventualSortLayer = layout.target.layerName;
        if(targetCard != null)
        {
            MoveToDiscard(targetCard);
        }

        targetCard = tCB;

        return (tCB);
    }

    public CardBartok MoveToDiscard(CardBartok tCB)
    {
        tCB.state = CBState.discard;
        discardPile.Add(tCB);
        tCB.SetSortingLayerName(layout.discardPile.layerName);
        tCB.SetSortOrder(discardPile.Count * 4);
        tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;

        return (tCB);
    }

    /// <summary>
    /// Снимает верхнюю карту со стопки свободных карт и возращает её
    /// </summary>
    public CardBartok Draw()
    {
        CardBartok cd = drawPile[0]; // Извлечь 0-ю карту
        drawPile.RemoveAt(0); // Удалить ее из списка drawPile
        return (cd); // и вернуть
    }

    /// <summary>
    /// Временно используется для проверки добавления карты в руки игрока
    /// </summary>
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Alpha1))
        {
            players[0].AddCard(Draw());
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            players[1].AddCard(Draw());
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            players[2].AddCard(Draw());
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            players[3].AddCard(Draw());
        }
    }
}
