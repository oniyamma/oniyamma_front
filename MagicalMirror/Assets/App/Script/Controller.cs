using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour {

    public Text echoSentenseText;
    public Text echoGestureText;
    public Text echoExpressionText;
    private IList<MirrorAction> actionQueue;

	// Use this for initialization
	void Start ()
    {
        this.actionQueue = new List<MirrorAction>();
        this.StartCoroutine(this.MainProcess());
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}

    private IEnumerator MainProcess()
    {
        while (true)
        {
            if (this.actionQueue.Count > 0)
            {
                var action = this.actionQueue[0];
                if (action.Sentence != string.Empty)
                {
                    this.echoSentenseText.text = action.Sentence;
                }
                if (action.HandGesture != MirrorAction.HandGestures.None)
                {
                    this.echoGestureText.text = MirrorAction.handGestureNames[action.HandGesture];
                }
                this.echoExpressionText.text = string.Format("SMILE : {0}", action.FaceExpressionIntensity);

                this.actionQueue.RemoveAt(0);
            }
            yield return 0;
        }
    }

    public void AddAction(MirrorAction action)
    {
        this.actionQueue.Add(action);
    }

    public void OnGestureThumbUp()
    {
        this.AddAction(new MirrorAction(MirrorAction.HandGestures.ThumbsUp));
    }

    public void OnGestureThumbDown()
    {
        this.AddAction(new MirrorAction(MirrorAction.HandGestures.ThumbsDown));
    }

    public void OnGestureVSign()
    {
        this.AddAction(new MirrorAction(MirrorAction.HandGestures.VSign));
    }
}
