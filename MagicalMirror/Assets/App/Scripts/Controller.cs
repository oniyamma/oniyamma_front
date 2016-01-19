using Oniyamma;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Controller : MonoBehaviour {
    public GameObject UIRoot;
    public GameObject informationPanel;
    public GameObject leaveHomeCommandUI;
    public GameObject goHomeCommandUI;
    public GameObject bird;
    public GameObject snowman;

    public Text echoSentenseText;
    public Text echoGestureText;
    public Text echoExpressionText;

    private IList<MirrorAction> actionQueue;
    private static IDictionary<string, Oniyamma.LogType> sentenceCommands = new Dictionary<string, Oniyamma.LogType>
    {
        { "行ってき", Oniyamma.LogType.LEAVE_HOME },
        { "いってき", Oniyamma.LogType.LEAVE_HOME },
        { "ただい", Oniyamma.LogType.GO_HOME },
    };
    private static IDictionary<MirrorAction.HandGestures, Oniyamma.LogType> gestureCommands = new Dictionary<MirrorAction.HandGestures, Oniyamma.LogType>
    {
        { MirrorAction.HandGestures.ThumbsUp, Oniyamma.LogType.LEAVE_HOME },
        { MirrorAction.HandGestures.ThumbsDown, Oniyamma.LogType.GO_HOME },
    };
    private IDictionary<Oniyamma.LogType, GameObject> commandUIs;

    // Use this for initialization
    void Start ()
    {
        this.actionQueue = new List<MirrorAction>();
        this.commandUIs = new Dictionary<Oniyamma.LogType, GameObject>()
        {
            { Oniyamma.LogType.LEAVE_HOME, this.leaveHomeCommandUI },
            { Oniyamma.LogType.GO_HOME, this.goHomeCommandUI },
        };

        this.informationPanel.SetActive(false);
        this.bird.SetActive(true);

        var faceTracker = this.GetComponent<FaceTracker>();
        faceTracker.onFaceTacked = delegate ()
        {
            this.informationPanel.SetActive(true);
            this.bird.SetActive(true);
        };
        faceTracker.onFaceLost = delegate ()
        {
            this.informationPanel.SetActive(false);
            this.bird.SetActive(false);
        };

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
                    foreach (var key in sentenceCommands.Keys)
                    {
                        if (action.Sentence.IndexOf(key) > -1)
                        {
                            this.SendLog(sentenceCommands[key], action);
                        }
                    }
                }
                if (action.HandGesture != MirrorAction.HandGestures.None)
                {
                    this.echoGestureText.text = MirrorAction.handGestureNames[action.HandGesture];
                    foreach (var key in gestureCommands.Keys)
                    {
                        if (action.HandGesture == key)
                        {
                            this.SendLog(gestureCommands[key], action);
                        }
                    }
                }
                this.echoExpressionText.text = string.Format("SMILE : {0}", action.FaceExpressionIntensity);

                this.actionQueue.RemoveAt(0);
            }
            yield return 0;
        }
    }

    private void SendLog(Oniyamma.LogType logType, MirrorAction action)
    {
        this.StartCoroutine(this.TakePhoto(
            delegate (string fileName)
            {
                Debug.Log(string.Format("Command:{0}  ScreenShot:{1}", logType, fileName));

                if (this.commandUIs.ContainsKey(logType))
                {
                    var ui = this.commandUIs[logType];
                    ui.GetComponent<Animator>().Play("Fire", 0, 0.0f);
                }

                OniyammaService.Current.AddLog(new LogParameter()
                {
                    Type = logType,
                    FilePath = fileName,
                    UserId = action.UserName,
                    //Kiss = 1,
                    //Smile = action.FaceExpressionIntensity,
                });
            })
            );
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
        //this.AddAction(new MirrorAction(MirrorAction.HandGestures.VSign));
    }

    public void OnTakePhoto()
    {
        this.StartCoroutine(this.TakePhoto(
            delegate (string fileName) 
            {
                Debug.Log("ScreenShot : " + fileName);
            })
            );
    }

    private IEnumerator TakePhoto(UnityAction<string> callback)
    {
        var fileName = string.Format("{0}{1}{2}.png", AppUtis.AppScreenShotPath, Path.DirectorySeparatorChar, DateTime.Now.ToString("yyyyMMdd-HHmmss.fff"));
        //this.UIRoot.SetActive(false);

        yield return new WaitForEndOfFrame();

        Application.CaptureScreenshot(fileName);
        //this.UIRoot.SetActive(true);
        callback(fileName);
    }
}
