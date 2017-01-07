using UnityEngine;
using System.Collections.Generic;

public class Card : MonoBehaviour {

	

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
