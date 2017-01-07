﻿using UnityEngine;
using System.Collections.Generic;

public class Deck : MonoBehaviour {

    // Suits
    public Sprite suitClub;
    public Sprite suitDiamond;
    public Sprite suitHeart;
    public Sprite suitSpade;

    public Sprite [] faceSprites;
    public Sprite [] rankSprites;

    public Sprite cardBack;
    public Sprite cardBackGold;
    public Sprite cardFront;
    public Sprite cardFrontGold;

    // Prefabs
    public GameObject prefabSprite;
    public GameObject prefabCard;


	public PT_XMLReader xmlr;
    public List<string> cardNames;
    public List<Card> cards;
    public List<Decorator> decorators;
    public List<CardDefinition> cardDefs;
    public Transform deckAnchor;
    public Dictionary<string, Sprite> dictSuits;

    // InitDeck is called by Prospector when it is ready
    public void InitDeck (string deckXMLText)
    {
        // This creates an anchor for all the Card GmaeObjects in the Hierarchy
        if (GameObject.Find ("_Deck") == null)
        {
            GameObject anchorGO = new GameObject ("_Deck");
            deckAnchor = anchorGO.transform;
        }

        // Initialise the Dictionary of SuitSprites with necessary Sprites
        dictSuits = new Dictionary<string, Sprite> ()
        {
            { "C", suitClub },
            { "D", suitDiamond },
            { "H", suitHeart },
            { "S", suitSpade },            
        };
        
        ReadDeck (deckXMLText);
        MakeCards ();
    }

    // ReadDeck parses the XML file passed to it into CardDefinitions
    public void ReadDeck (string deckXMLText)
    {
        xmlr = new PT_XMLReader ();
        xmlr.Parse (deckXMLText);

        // This prints a test line to show how xmlr can be used

        string s = "xml [0] decorator [0]";
        s += "type = " + xmlr.xml ["xml"][0]["decorator"][0].att ("type");
        s += " x = " + xmlr.xml ["xml"][0]["decorator"][0].att ("x");
        s += " y = " + xmlr.xml ["xml"][0]["decorator"][0].att ("y");
        s += " scale = " + xmlr.xml ["xml"][0]["decorator"][0].att ("scale");
        // print (s);

        // Read decorators for all cards
        decorators = new List<Decorator> ();

        // Get a PT_XMLHashList of all <decorator>s in the XML file
        PT_XMLHashList xDecos = xmlr.xml ["xml"][0]["decorator"];

        Decorator deco;

        // For each <decorator> in the XML...
        for (int i = 0; i < xDecos.Count; i++)
        {
            // Make a new decorator
            deco = new Decorator ();
            // Copy the attributes of the <decorator> to the Decorator
            deco.type = xDecos [i].att ("type");
            // Set the bool flip based on whether the text of the attribute is
            // "1" or something else. This is an atypical but perfectly fine
            // use of the == comparison operator. It will return a true or
            // false, which will be assigned to deco.flip
            deco.flip = (xDecos [i].att ("flip") == "1");
            // floats need to be parsed from the attribute strings
            deco.scale = float.Parse (xDecos [i].att ("scale"));
            // Vector3 loc initialises to [0, 0, 0], so we just need to modify it
            deco.loc.x = float.Parse (xDecos [i].att ("x"));
            deco.loc.y = float.Parse (xDecos [i].att ("y"));
            deco.loc.z = float.Parse (xDecos [i].att ("z"));
            // Add the temporary deco to the List decorators
            decorators.Add (deco);
        }

        // Read pip locations for each card number
        // Init the list of Cards
        cardDefs = new List<CardDefinition> ();
        // Get a PT_XMLHashList of all the <Card>s in the XML file
        PT_XMLHashList xCardDefs = xmlr.xml ["xml"][0]["card"];

        // For each of the <Card>s
        for (int i = 0; i < xCardDefs.Count; i++)
        {
            // Create a new card definition
            CardDefinition cDef = new CardDefinition ();
            // Parse the attributes and add them to cDef
            cDef.rank = int.Parse (xCardDefs [i].att ("rank"));
            // Get a PT_XMLHashList of all the pips on this card
            PT_XMLHashList xPips = xCardDefs [i]["pip"];

            if (xPips != null)
            {
                // Iterate through all of the pips
                for (int j = 0; j < xPips.Count; j++)
                {
                    deco = new Decorator ();
                    // <pip>s on the <card> are handled via the Decorator class
                    deco.type = "pip";
                    deco.flip = (xPips [j].att ("flip") == "1");
                    deco.loc.x = float.Parse (xPips [j].att ("x"));
                    deco.loc.y = float.Parse (xPips [j].att ("y"));
                    deco.loc.z = float.Parse (xPips [j].att ("z"));

                    if (xPips [j].HasAtt ("scale"))
                    {
                        deco.scale = float.Parse (xPips [j].att ("scale"));
                    }

                    cDef.pips.Add (deco);
                }
            }

            // Face cards have a face attribute
            // cDef.face is the base name of the face card Sprite
            // e.g. FaceCard_11 i the base name for all Jack face Sprites
            // The Jack of Clubs is FaceCard_11C, hearts is FaceCard_11H, etc.

            if (xCardDefs [i].HasAtt ("face"))
            {
                cDef.face = xCardDefs [i].att ("face");
            }
            cardDefs.Add (cDef);
        }
    }

    // Get the proper CardDefinitions based on Rank (1 to 14 is Ace to King)
    public CardDefinition GetCardDefinitionByRank (int rnk)
    {
        // Search through all of the CardDefinitions
        foreach (CardDefinition cd in cardDefs)
        {
            // If the rank is correct, return this definition
            if (cd.rank == rnk)
            {
                return cd;
            }
        }
        return null;
    }

    // Make the Card GameObjects
    public void MakeCards ()
    {
        // cardNames will be the names of the cards to build
        // Each suit goes from 1 to 13 (e.g., C1 to C13 for Clubs)
        cardNames = new List<string> ();
        string [] letters = new string [] { "C", "D", "H", "S" };
        foreach (string s in letters)
        {
            for (int i = 0; i < 13; i++)
            {
                cardNames.Add (s + (i + 1));
            }
        }

        // Make a List to hold all the cards
        cards = new List<Card> ();
        // Several variables that will be reused several times
        Sprite tS = null;
        GameObject tGO = null;
        SpriteRenderer tSR = null;

        // Iterate through all of the card names that were just made
        for (int i = 0; i < cardNames.Count; i++)
        {
            // Create a new Card GameObject
            GameObject cgo = Instantiate (prefabCard) as GameObject;
            // Set the transform.parent of the new card to the anchor
            cgo.transform.parent = deckAnchor;
            // Get the card component
            Card card = cgo.GetComponent<Card> ();

            // This stacks the cards so that they're all in nice rows
            cgo.transform.localPosition = new Vector3 ((i % 13) * 3, i / 13 * 4, 0);

            // Assign basic values to the Card
            card.name = cardNames [i];
            card.suit = card.name[0].ToString ();
            card.rank = int.Parse (card.name.Substring(1));

            if (card.suit == "D" || card.suit == "H")
            {
                card.colS = "Red";
                card.color = Color.red;
            }

            // Pull the CardDefinition for this card
            card.def = GetCardDefinitionByRank (card.rank);

            // Add Decorators
            foreach (Decorator deco in decorators)
            {
                if (deco.type == "suit")
                {
                    // Instantiate a Sprite GameObject
                    tGO = Instantiate (prefabSprite) as GameObject;
                    // Get the SpriteRenderer component
                    tSR = tGO.GetComponent<SpriteRenderer> ();
                    // Set the sprite to the proper suit
                    tSR.sprite = dictSuits [card.suit];
                }
                // If it's not a suit, it's a rank deco
                else
                {
                    tGO = Instantiate (prefabSprite) as GameObject;
                    tSR = tGO.GetComponent<SpriteRenderer> ();
                    // Get the proper Sprite to show this rank
                    tS = rankSprites [card.rank];
                    tSR.sprite = tS;
                    // Set the color of the rank to match the suit
                    tSR.color = card.color;
                }
                // Make the deco Sprites render above the card
                tSR.sortingOrder = 1;
                // Make the decorator Sprite a child of the card
                tGO.transform.parent = cgo.transform;
                // Set the localPosition based on the location from DeckXML
                tGO.transform.localPosition = deco.loc;
                // flip the directions if needed
                if (deco.flip)
                {
                    // An Euler rotation of 180 degrees around the Z-axis will flip it
                    tGO.transform.rotation = Quaternion.Euler (0, 0, 180);
                }
                // Set the scale to keep decos from being too big
                if (deco.scale != 1)
                {
                    tGO.transform.localScale = Vector3.one * deco.scale;
                }
                // Name this GameObject so it's easy to find
                tGO.name = deco.type;
                // Add this deco GameObject to the List card.decoGOs
                card.decoGOs.Add (tGO);
            }

            // Add Pips
            // For each of the pips in the definition
            foreach (Decorator pip in card.def.pips)
            {
                // Instantiate a Sprite GameObject
                tGO = Instantiate (prefabSprite) as GameObject;
                // Set the parent to be the card GameObject
                tGO.transform.parent = cgo.transform;
                // Set the position to that specified in the XML
                tGO.transform.localPosition = pip.loc;
                // Flip it if necessary
                if (pip.flip)
                {
                    tGO.transform.rotation = Quaternion.Euler (0, 0, 180);
                }
                // Scale it if necessary (only for the Ace)
                if (pip.scale != 1)
                {
                    tGO.transform.localScale = Vector3.one * pip.scale;
                }
                // Give this GameObject a name
                tGO.name = "pip";
                // Get the SpriteRenderer component
                tSR = tGO.GetComponent<SpriteRenderer> ();
                // Set the Sprite to the proper suit
                tSR.sprite = dictSuits [card.suit];
                // Set sortingOrder so the pip is rendered above the Card_Front
                tSR.sortingOrder = 1;
                // Add this to the Card's list of pips
                card.pipGOs.Add (tGO);
            }

            // Handle Face Cards
            // If this has a face in card.def
            if (card.def.face != "")
            {
                tGO = Instantiate (prefabSprite) as GameObject;
                tSR = tGO.GetComponent<SpriteRenderer> ();
                // Generate the right name and pass it to GetFace ()
                tS = GetFace (card.def.face + card.suit);
                // Assign this Sprite to tSR
                tSR.sprite = tS;
                // Set the sortingOrder
                tSR.sortingOrder = 1;
                tGO.transform.parent = card.transform;
                tGO.transform.localPosition = Vector3.zero;
                tGO.name = "face";
            }

            // Add Card Back
            // The Card_Back will be able to cover everything else on the Card
            tGO = Instantiate (prefabSprite) as GameObject;
            tSR = tGO.GetComponent<SpriteRenderer> ();
            tSR.sprite = cardBack;
            tGO.transform.parent = card.transform;
            tGO.transform.localPosition = Vector3.zero;
            // This is a higher sortingOrder than anything else
            tSR.sortingOrder = 2;
            tGO.name = "back";
            card.back = tGO;

            // Default to face up
            // Use the proprty faceUp of Card
            card.faceUp = true;

            // Add the card to the deck
            cards.Add (card);
        }
    }

    // Find the proper face card Sprite
    public Sprite GetFace (string faceS)
    {
        foreach (Sprite tS in faceSprites)
        {
            // If this Sprite has the right name, return it
            if (tS.name == faceS)
            {
                return tS;
            }
        }
        return null;
    }

}
