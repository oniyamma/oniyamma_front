using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controller : MonoBehaviour {

    public Text echoSentenseText;
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
                else
                {
                    this.echoExpressionText.text = string.Format("SMILE : {0}", action.FaceExpressionIntensity);
                }

                this.actionQueue.RemoveAt(0);
            }
            yield return 0;
        }
    }

    public void AddAction(MirrorAction action)
    {
        this.actionQueue.Add(action);
    }
}
