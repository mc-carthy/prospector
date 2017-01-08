using UnityEngine;
using System.Collections.Generic;

// The CardState variable type has one of four values: drawpile, tableau, target, discard
public enum CardState
{
    drawpile,
    tableau,
    target,
    discard
}

public class CardProspector : Card {

    public CardState state = CardState.drawpile;
    // The hiddenBy List stores which other cards will keep this one face down
    public List<CardProspector> hiddenBy = new List<CardProspector> ();
    // LayoutID matches this card to a LayoutXML id if it's a tableau card
    public int layoutID;
    // The SlotDef class stores information pulled in from the LayoutXML <slot>
    public SlotDef SlotDef;

}
