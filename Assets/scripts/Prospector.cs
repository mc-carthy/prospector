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
    }

}
