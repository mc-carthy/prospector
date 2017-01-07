using UnityEngine;
using System.Collections.Generic;

public class Prospector : MonoBehaviour {

	static public Prospector S;

    public Deck deck;
    public TextAsset deckXML;

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
    }

}
