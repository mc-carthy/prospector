using UnityEngine;

public class Deck : MonoBehaviour {

	public PT_XMLReader xmlr;

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
        print (s);
    }

}
