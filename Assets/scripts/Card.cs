using UnityEngine;
using System.Collections.Generic;

public class Card : MonoBehaviour {

	// Suit of the card (C, D, H or S)
    public string suit;
    // Rank of the card (1 - 14)
    public int rank;
    // Color to tint pips
    public Color color = Color.black;
    // Name of color
    public string colS = "Black";
    // This List holds all of the Decorator GameObjects
    public List<GameObject> decoGOs = new List<GameObject> ();
    // This List holds all of the Pip GameObjects
    public List<GameObject> pipGOs = new List<GameObject> ();
    // The GameObject of the back of the card
    public GameObject back;
    // Parsed from DeckXML.xml
    public CardDefinition def;

    // List of the SpriteRenderer Components of this GameObject and its children
    public SpriteRenderer [] spriteRenderers;

    private void Start ()
    {
        // Ensures that the card starts properly depth sorted
        SetSortOrder (0);
    }

    public bool faceUp {
        get {
            return !back.activeSelf;
        }
        set {
            back.SetActive (!value);
        }
    }

    // If spriteRenderers is not yet defined, this function defines it
    public void PopulateSpriteRenderers ()
    {
        // If spriteRenderers is null or empty
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            // Get the SpriteRenderer Components of this GameObject and its children
            spriteRenderers = GetComponentsInChildren<SpriteRenderer> ();
        }
    }

    // Sets the sortingLayerName on all SpriteRenderer Components
    public void SetSortingLayerName (string tSLN)
    {
        PopulateSpriteRenderers ();

        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            tSR.sortingLayerName = tSLN;
        }
    }

    // Sets the sortingOrder of all SpriteRenderer Components
    public void SetSortOrder (int sOrd)
    {
        PopulateSpriteRenderers ();

        // The white background of the card is on bottom (sOrd)
        // On top of that are all the pips, decorators, face etc. (sOrd + 1)
        // The back is on top so that when visible, it covers the rest (sOrd + 2)

        // Iterate through all the spriteRenderers in tSR
        foreach (SpriteRenderer tSR in spriteRenderers)
        {
            if (tSR.gameObject == this.gameObject)
            {
                // If the gameObject is this.gameObject, it's the background
                tSR.sortingOrder = sOrd;
                // And continue to the next iteration of the loop
                continue;
            }

            // Each of the children of this GameObject are named
            // switch based on the name

            switch (tSR.gameObject.name)
            {
                case ("back"):
                    tSR.sortingOrder = sOrd + 2;
                    break;
                case ("face"):
                default:
                    tSR.sortingOrder = sOrd + 1;
                    break;
            }
        }
    }

    virtual public void OnMouseUpAsButton ()
    {
        print (name);
    }

}

[System.SerializableAttribute]
public class Decorator {
    
    // This class stores information about each decorator or pip from DeckXML

    // For card pips, type = "pip"
    public string type;
    // The location of the Sprite on the card
    public Vector3 loc;
    // Whether to flip the Sprite vertically
    public bool flip;
    // The scale of the sprite
    public float scale = 1f;
}

[System.SerializableAttribute]
public class CardDefinition {
    // This class stores information for each rank of card

    // Sprite to use for each face card
    public string face;
    // The rank (1 - 13) for this card
    public int rank;
    // Pips used
    public List<Decorator> pips = new List<Decorator> ();

    // Because decorators (from the XML) are used the same way on every card in
    // the deck ,pips only stores information about the pips on numbered cards
}
