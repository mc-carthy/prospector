using UnityEngine;
using System.Collections.Generic;

// The Scoreboard class maanges showing the score to the player
public class Scoreboard : MonoBehaviour {

	public static Scoreboard S;

    public GameObject prefabFloatingScore;

    public bool _________________________;

    [SerializeField]
    private int _score = 0;
    public string _scoreString;

    // The score property also sets the scoreString
    public int score {
        get {
            return _score;
        }
        set {
            _score = value;
            scoreString = Utils.AddCommasToNumber (_score);
        }
    }

    // The scoreString property also sets the GUIText.text
    public string scoreString {
        get {
            return _scoreString;
        }
        set {
            _scoreString = value;
            GetComponent<GUIText> ().text = _scoreString;
        }
    }

    private void Awake ()
    {
        S = this;
    }

    // When called by SendMessage, this adds the fs.score to this.score
    public void FSCallback (FloatingScore fs)
    {
        score += fs.score;
    }

    // This will Instantiate a new FloatingScore GameObject and initialize it.
    // It also returns a pointer to the FloatingScore create so that the
    // calling function can do more with it (like set fontSizes, etc.)
    public FloatingScore CreateFloatingScore (int amt, List<Vector3> pts)
    {
        GameObject go = Instantiate (prefabFloatingScore) as GameObject;
        FloatingScore fs = go.GetComponent<FloatingScore> ();

        fs.score = amt;
        // Set fs to call back to this
        fs.reportFinishTo = this.gameObject;
        fs.Init (pts);

        return fs;
    }

}
