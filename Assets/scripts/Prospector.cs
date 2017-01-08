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

        // Set up the initial target card
        MoveToTarget (Draw ());
        // Set up the drawPile
        UpdateDrawPile ();
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

    // CardClicked is called any time a card in the game is clicked
    public void CardClicked (CardProspector cd)
    {
        // The reaction is determined bt the state of the clicked card
        switch (cd.state)
        {
            case CardState.target:
                // Clicking the target card does nothing
                break;
            case CardState.drawpile:
                // Clicking any card in the drawPile will draw the next card
                // Move the target to the discardPile
                MoveToDiscard (target);
                // Move the next drawn card to the target
                MoveToTarget (Draw());
                // Restacks the drawPile
                UpdateDrawPile ();
                break;
            case CardState.tableau:
                // Clicking a card in the tableau will check if it's a valid play
                bool validMatch = true;
                // If the card is face down, it's not valid
                if (!cd.faceUp)
                {
                    validMatch = false;
                }
                // If it's not an adjacent rank, it's not valid
                if (!AdjacentRank (cd, target))
                {
                    validMatch = false;
                }
                if (!validMatch)
                {
                    return;
                }
                // It's a valid card, remove it from the tableau List<> and make it the target card
                tableau.Remove (cd);
                MoveToTarget (cd);
                break;
        }
    }

    // Moves the current target to the discardPile
    private void MoveToDiscard (CardProspector cd)
    {
        // Set the state of the card to discard
        cd.state = CardState.discard;
        // Add it to the discardPile List<>
        discardPile.Add (cd);
        // Update its transform parent
        cd.transform.parent = layoutAnchor;
        // Position it on the discardPile
        cd.transform.localPosition = new Vector3 (
          layout.multiplier.x * layout.discardPile.x,  
          layout.multiplier.y * layout.discardPile.y,
          -layout.discardPile.layerID + 0.5f
        );
        cd.faceUp = true;
        // Place it on top of the pile for depth sorting
        cd.SetSortingLayerName (layout.discardPile.layerName);
        cd.SetSortOrder (-100 + discardPile.Count);
    }

    // Make cd the new target card
    private void MoveToTarget (CardProspector cd)
    {
        // If there is currently a target card, move it to discardPile
        if (target != null)
        {
            MoveToDiscard (target);
        }
        // cd is the new target
        target = cd;
        cd.state = CardState.target;
        cd.transform.parent = layoutAnchor;
        // Move to the target postion
        cd.transform.localPosition = new Vector3 (
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID
        );
        // Make it face up
        cd.faceUp = true;
        // Set the depth sorting
        cd.SetSortingLayerName (layout.discardPile.layerName);
        cd.SetSortOrder (0);
    }

    // Arranges all the cards of the drawPile to show how many are left
    private void UpdateDrawPile ()
    {
        CardProspector cd;
        // Go through all of the cards in the drawPile
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile [i];
            cd.transform.parent = layoutAnchor;
            // Position it correctly with the layout.drawpile.stagger
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3 (
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f * i
            );
            // Make them all face down
            cd.faceUp = false;
            cd.state = CardState.drawpile;
            // Set depth sorting
            cd.SetSortingLayerName (layout.drawPile.layerName);
            cd.SetSortOrder (-10 * i);
        }
    }

    // Return true if the two cards are adjacent in rank (A & K wrap around)
    public bool AdjacentRank (CardProspector c0, CardProspector c1)
    {
        // If either card is face down, it's not adjacent
        if (!c0.faceUp || !c1.faceUp)
        {
            return false;
        }

        // If the are 1 apart, they are adjacent
        if (Mathf.Abs (c0.rank - c1.rank) == 1)
        {
            return true;
        }

        // If one is A and the other K, they're adjacent
        if (c0.rank == 1 && c1.rank == 13)
        {
            return true;
        }
        if (c0.rank == 13 && c1.rank == 1)
        {
            return true;
        }

        return false;

    }

}
