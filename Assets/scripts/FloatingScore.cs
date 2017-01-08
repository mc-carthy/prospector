using UnityEngine;
using System.Collections.Generic;

// An enum to track the possible states of a FloatingScore
public enum FSState {
    idle,
    pre,
    active,
    post
}

// FloatingScore can move itself on screen following a Bézier curve
public class FloatingScore : MonoBehaviour {

	public FSState state = FSState.idle;
    [SerializeField]
    // The private score field
    private int _score = 0;
    public string scoreString;

    // The score property also sets scoreString when set
    public int score {
        get {
            return (_score);
        }
        set {
            _score = value;
            scoreString = Utils.AddCommasToNumber (_score);
            GetComponent<GUIText> ().text = scoreString;
        }
    }

    // Bézier points for movement
    public List<Vector3> bezierPts;
    // Bézier points for font scaling
    public List<float> fontSizes;
    public float timeStart = -1f;
    public float timeDuration = 1f;
    // Use Easing in Utils.cs
    public string easingCurve = Easing.InOut;

    // The GameObject that will recieve the SendMessage when this is done moving
    public GameObject reportFinishTo = null;

    // Set up FloatingScore and movement
    // Note the use of parameter defaults for eTimeS & eTimeD
    public void Init (List<Vector3> ePts, float eTimeS = 0, float eTimeD = 1)
    {
        bezierPts = new List<Vector3> (ePts);

        // If there is only one point, then go there
        if (ePts.Count == 1)
        {
            transform.position = ePts [0];
            return;
        }

        // If eTimeS is the default, just start at the current time
        if (eTimeS == 0)
        {
            eTimeS = Time.time;
            timeStart = eTimeS;
            timeDuration = eTimeD;

            // Set it to the pre state, ready to start moving
            state = FSState.pre;
        }
    }

    public void FSCallback (FloatingScore fs)
    {
        // When this callback is called by SendMessage,
        // add the score from the calling FloatingScore
        score += fs.score;
    }

    private void Update ()
    {
        // If this is not moving, just return
        if (state == FSState.idle)
        {
            return;
        }

        // Get u from the current time and duration
        // u ranges from 0 to 1
        float u = (Time.time - timeStart) / timeDuration;
        // Use Easing class from Utils to curve the u value
        float uC = Easing.Ease (u, easingCurve);

        // If u < 0, we shouldn't move yet
        if (u < 0)
        {
            state = FSState.pre;
            // Move the the initial point
            transform.position = bezierPts [0];
        }
        else
        {
            // If u >= 1, we're done moving
            if (u >= 1)
            {
                // Set uC to 1 so we don't overshoot
                uC = 1;
                state = FSState.post;
                // If there's a callback GameObject
                if (reportFinishTo != null)
                {
                    // Use SendMessage to call the FSCallback method
                    // with this parameter
                    reportFinishTo.SendMessage ("FSCallback", this);
                    // Now that the message has been sent, destroy the GameObject
                    Destroy (gameObject);
                }
                else
                {
                    // If there is nothing to callback then don't
                    // destroy this. Just let it stay still
                    state = FSState.idle;
                }
            }
            else
            {
                // 0 <= u < 1, this is active and moving
                state = FSState.active;
            }
            // Use Bézier curve to move this to the right point
            Vector3 pos = Utils.Bezier (uC, bezierPts);
            transform.position = pos;

            // If fontSizes has values in it, then adjust the fontSize of this GUIText
            if (fontSizes != null && fontSizes.Count > 0)
            {
                int size = Mathf.RoundToInt (Utils.Bezier (uC, fontSizes));
                GetComponent<GUIText>(). fontSize = size;
            }
        }
    }

}
