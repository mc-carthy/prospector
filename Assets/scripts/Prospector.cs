using UnityEngine;
using System.Collections.Generic;

public class Prospector : MonoBehaviour {

	static public Prospector S;

    public Deck deck;
    public TextAsset deckXML;

    public Layout layout;
    public TextAsset layoutXML;
    public Vector3 layoutCenter;
    public float xOffset = 3f;
    public float yOffset = -2.5f;
    public Transform layoutAnchor;

    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;

    public List<CardProspector> drawPile;

    private void Awake ()
    {
        S = this;
    }

    private void Start ()
    {
        deck = GetComponent<Deck> ();
        deck.InitDeck (deckXML.text);
        // The ref keyword passes a reference into deck.cards, which
        // allows deck.cards to be modified by Deck.Shuffle ()
        Deck.Shuffle (ref deck.cards);

        // Get the layout
        layout = GetComponent<Layout> ();
        // Pass layoutXML to is
        layout.ReadLayout (layoutXML.text);

        drawPile = ConverListCardsToListCardProspectors (deck.cards);
        LayoutGame ();
    }

    // The Draw function will pull a single card from the drawPile and return it
    private CardProspector Draw ()
    {
        // Pull the 0th CardProspector
        CardProspector cd = drawPile [0];
        // Then remove it from the List<> drawPile
        drawPile.RemoveAt (0);
        // Return it
        return cd;
    }

    // LayoutGame positions the initial tableau of cards
    private void LayoutGame ()
    {
        // Create an empty GameObject to serve as an anchor for the tableau
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject ("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        CardProspector cp;
        // Follow the layout and iterate through all of the SlotDefs in the layout.slotDefs as tSD
        foreach (SlotDef tSD in layout.slotDefs)
        {
            // Pull a card from the top (beginning) of the drawPile
            cp = Draw ();
            // Set its faceUp to the value in slotDef
            cp.faceUp = tSD.faceUp;
            // Make its parent layoutAnchor. This replaces the previous parent: deck.deckAnchor,
            // which apprears as _Deck in the Hierarchy when the scene is playing
            cp.transform.parent = layoutAnchor;
            // Set the localPosition of the card based on slotDef
            cp.transform.localPosition = new Vector3 (
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID
            );
            cp.layoutID = tSD.id;
            cp.SlotDef = tSD;
            cp.state = CardState.tableau;

            // Set the sorting layers
            cp.SetSortingLayerName (tSD.layerName);

            tableau.Add (cp);
        }
    }

    private List<CardProspector> ConverListCardsToListCardProspectors (List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector> ();

        CardProspector tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardProspector;
            lCP.Add (tCP);
        }

        return lCP;
    }

}
