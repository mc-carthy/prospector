using UnityEngine;
using System.Collections.Generic;

public class Deck : MonoBehaviour {

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
        ReadDeck (deckXMLText);
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

}
