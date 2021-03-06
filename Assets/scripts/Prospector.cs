﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

// An enum to handle all the possible scoring events
public enum ScoreEvent {
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}

public class Prospector : MonoBehaviour {

	static public Prospector S;
    static public int SCORE_FROM_PREV_ROUND = 0;
    static public int HIGH_SCORE = 0;

    // The delay between rounds
    public float reloadDelay = 1f;

    public Vector3 fsPosMid = new Vector3 (0.5f, 0.9f, 0);
    public Vector3 fsPosRun = new Vector3 (0.5f, 0.75f, 0);
    public Vector3 fsPosMid2 = new Vector3 (0.5f, 0.5f, 0);
    public Vector3 fsPosEnd = new Vector3 (1f, 0.65f, 0);

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

    // Fields to track score info
    // Chain of cards in this run
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;
    public FloatingScore fsRun;

    public GUIText GTGameOver;
    public GUIText GTRoundResult;

    private void Awake ()
    {
        S = this;
        // Check for a highscore in PlayerPrefs
        if (PlayerPrefs.HasKey ("ProspectorHighScore"))
        {
            HIGH_SCORE = PlayerPrefs.GetInt ("ProspectorHighScore");
        }
        // Add the score from last round, which will be > 0 if it was a win
        score += SCORE_FROM_PREV_ROUND;
        // Reset the SCORE_FROM_PREV_ROUND
        SCORE_FROM_PREV_ROUND = 0;

        // Set up the GUITexts that show at the end of the round
        // Get the GUIText components
        GameObject go = GameObject.Find ("GameOver");
        if (go != null)
        {
            GTGameOver = go.GetComponent<GUIText> ();
        }

        go = GameObject.Find ("RoundResult");
        if (go != null)
        {
            GTRoundResult = go.GetComponent<GUIText> ();
        }

        ShowResultGTs (false);

        go = GameObject.Find ("HighScore");
        string hScore = "High Score: " + Utils.AddCommasToNumber (HIGH_SCORE);
        go.GetComponent<GUIText> ().text = hScore;
    }

    private void ShowResultGTs (bool show)
    {
        GTGameOver.gameObject.SetActive (show);
        GTRoundResult.gameObject.SetActive (show);
    }

    private void Start ()
    {
        Scoreboard.S.score = score;
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

    // Convert from the layoutID int to the CardProspector with that id
    private CardProspector FindCardByLayoutID (int layoutID)
    {
        // Search through all cards in the tableau List<>
        foreach (CardProspector tCP in tableau)
        {
            // If the card has the same ID, return it
            if (tCP.layoutID == layoutID)
            {
                return tCP;
            }
        }
        return null;
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

        // Set which cards are hiding others
        foreach (CardProspector tCP in tableau)
        {
            foreach (int hid in tCP.SlotDef.hiddenBy)
            {
                cp = FindCardByLayoutID (hid);
                tCP.hiddenBy.Add (cp);
            }
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
                ScoreManager (ScoreEvent.draw);
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
                // Update the tableau card face ups
                SetTableauFaces ();
                ScoreManager (ScoreEvent.mine);
                break;
        }
        // Check to see whether the game is over or not
        CheckForGameOver ();
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

    // This turns cards in the Mine face up or face down
    private void SetTableauFaces ()
    {
        foreach (CardProspector cd in tableau)
        {
            // Assume the card will be face up
            bool fup = true;
            foreach (CardProspector cover in cd.hiddenBy)
            {
                // If either of the covering cards are in the tableau
                if (cover.state == CardState.tableau)
                {
                    // Then this card is face down
                    fup = false;
                }
            }
            cd.faceUp = fup;
        }
    }

    // Test whether the game is over
    private void CheckForGameOver ()
    {
        // If the tableau is empty, the game is over
        if (tableau.Count == 0)
        {
            // Call GameOver() with a win
            GameOver (true);
            return;
        }
        // If there are still cards in the draw pile, the game's not over
        if (drawPile.Count > 0)
        {
            return;
        }
        // Check for remaining valid plays
        foreach (CardProspector cd in tableau)
        {
            // If there is a valid play, the game's not over
            if (AdjacentRank (cd, target))
            {
                return;
            }
        }
        // Since there are no valid plays, the game is over
        // Call GameOver() with a lose
        GameOver (false);
    }

    // Called when the game is over, simple for now, but expandable
    private void GameOver (bool won)
    {
        if (won)
        {
            ScoreManager (ScoreEvent.gameWin);
        }
        else
        {
            ScoreManager (ScoreEvent.gameLoss);
        }
        // Reload the scene in reloadDelay seconds
        // This will give the score a moment to travel
        Invoke ("ReloadLevel", reloadDelay);
    }

    private void ReloadLevel ()
    {
        SceneManager.LoadScene (SceneManager.GetActiveScene ().name, LoadSceneMode.Single);
    }

    // ScoreManager handles all of the scoring
    private void ScoreManager (ScoreEvent sEvt)
    {
        List<Vector3> fsPts;
        switch (sEvt)
        {
            // Same things need to happen whether it's a draw, a win or a loss
            
            // Drawing a card
            case ScoreEvent.draw:
            // Won the round
            case ScoreEvent.gameWin:
            // Lost the round
            case ScoreEvent.gameLoss:
                // Reset the score chain
                chain = 0;
                // Add scoreRun to total score
                score += scoreRun;
                // Reset scoreRun
                scoreRun = 0;
                // Add fsRun to the _Scoreboard score
                if (fsRun != null)
                {
                    // Create points for the Bézier curve
                    fsPts = new List<Vector3> ();
                    fsPts.Add (fsPosRun);
                    fsPts.Add (fsPosMid2);
                    fsPts.Add (fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init (fsPts, 0, 1);
                    // Also adjust the font size
                    fsRun.fontSizes = new List<float> (new float[] { 28, 36, 4 });
                    // Clear fsRun so it's created again
                    fsRun = null;
                }
                break;
            // Remove a mine card
            case ScoreEvent.mine:
                // Increase the score chain
                chain++;
                // Add score for this card to run
                scoreRun += chain;
                // Create a FloatingScore for this score
                FloatingScore fs;
                // Move it from the mousePosition to fsPosRun
                Vector3 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                
                fsPts = new List<Vector3> ();
                fsPts.Add (p0);
                fsPts.Add (fsPosMid);
                fsPts.Add (fsPosRun);

                fs = Scoreboard.S.CreateFloatingScore (chain, fsPts);
                fs.fontSizes = new List<float> (new float[] {4, 50, 28 });

                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }

                break;
        }

        // This second switch statement handles round wins and losses
        switch (sEvt)
        {
            case (ScoreEvent.gameWin):
                GTGameOver.text = "Round Over";
                // If it's a win, add the score to the next round
                // static fields are NOT reset by SceneManagement.LoadLevel ()
                Prospector.SCORE_FROM_PREV_ROUND = score;
                print ("You won this round! Round score: " + score.ToString ());
                GTRoundResult.text = "You won this round!\nRound Score: " + score.ToString ();
                ShowResultGTs (true);
                break;
            case (ScoreEvent.gameLoss):
            GTGameOver.text = "Game Over";
                // If it's a loss, check against the high score
                if (Prospector.HIGH_SCORE <= score)
                {
                    print ("You got the high score! High score: " + score.ToString ());
                    string sRR = "You got the high score!\nHigh Score: " + score.ToString ();
                    GTRoundResult.text = sRR;
                    Prospector.HIGH_SCORE = score;
                    PlayerPrefs.SetInt ("ProspectorHighScore", score);
                }
                else
                {
                    print ("Your final score for this game was: " + score.ToString ());
                    GTRoundResult.text = "Your final score was: " + score.ToString ();
                }
                ShowResultGTs (true);
                break;
            default:
                print ("Score: " + score.ToString() + " scoreRun: " + scoreRun + " chain: " + chain);
                break;
        }
    }
}
