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
    public GameObject weatherCommandUI;
    public GameObject bird;
    public GameObject snowman;
    public GameObject airplane;
    public AudioClip faceTrackedSound;

    public Text echoSentenseText;
    public Text echoGestureText;
    public Text echoExpressionText;

    private IList<MirrorAction> actionQueue;

    private enum CommandType
    {
        Log,
        Weather,
        Emotion,
    }

    private static IDictionary<string, object> sentenceCommands = new Dictionary<string, object>
    {
        { "行ってき", Oniyamma.LogType.LEAVE_HOME },
        { "いってき", Oniyamma.LogType.LEAVE_HOME },
        { "ただい", Oniyamma.LogType.GO_HOME },
        { "天気", CommandType.Weather },
        { "てんき", CommandType.Weather },
   };
    private static IDictionary<MirrorAction.HandGestures, object> gestureCommands = new Dictionary<MirrorAction.HandGestures, object>
    {
        { MirrorAction.HandGestures.ThumbsUp, Oniyamma.LogType.LEAVE_HOME },
        { MirrorAction.HandGestures.ThumbsDown, Oniyamma.LogType.GO_HOME },
        { MirrorAction.HandGestures.VSign, CommandType.Weather },
   };
    private IDictionary<object, GameObject> commandUIs;

    // Use this for initialization
    void Start ()
    {
        this.actionQueue = new List<MirrorAction>();
        this.commandUIs = new Dictionary<object, GameObject>()
        {
            { Oniyamma.LogType.LEAVE_HOME, this.leaveHomeCommandUI },
            { Oniyamma.LogType.GO_HOME, this.goHomeCommandUI },
            { CommandType.Weather, this.weatherCommandUI },
       };

        this.informationPanel.SetActive(false);
        this.bird.SetActive(true);

        var faceTracker = this.GetComponent<FaceTracker>();
        faceTracker.onFaceTacked = delegate ()
        {
            this.StartCoroutine(this.OnFaceActivted());
        };
        faceTracker.onFaceLost = delegate ()
        {
            this.informationPanel.SetActive(false);
            this.bird.SetActive(false);
            this.airplane.SetActive(false);
        };

        this.StartCoroutine(this.MainProcess());
    }

    private IEnumerator OnFaceActivted()
    {
        this.GetComponent<AudioSource>().PlayOneShot(this.faceTrackedSound);
        this.informationPanel.SetActive(true);
        this.bird.SetActive(true);
        yield return new WaitForSeconds(1);
        this.airplane.SetActive(true);
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
                if (action.FaceExpressions != null)
                {
                    this.SendCommand(CommandType.Emotion, null, action);
                    this.echoExpressionText.text = string.Format("SMILE : {0}", action.FaceExpressions.Smile);
                }
                else
                {
                    if (action.Sentence != string.Empty)
                    {
                        this.echoSentenseText.text = action.Sentence;
                        foreach (var key in sentenceCommands.Keys)
                        {
                            if (action.Sentence.IndexOf(key) > -1)
                            {
                                if (sentenceCommands[key] is Oniyamma.LogType)
                                {
                                    this.SendCommand(CommandType.Log, (Oniyamma.LogType)sentenceCommands[key], action);
                                }
                                if (sentenceCommands[key] is CommandType)
                                {
                                    this.SendCommand((CommandType)sentenceCommands[key], null, action);
                                }
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
                                if (gestureCommands[key] is Oniyamma.LogType)
                                {
                                    this.SendCommand(CommandType.Log, (Oniyamma.LogType)gestureCommands[key], action);
                                }
                                if (gestureCommands[key] is CommandType)
                                {
                                    this.SendCommand((CommandType)gestureCommands[key], null, action);
                                }
                            }
                        }
                    }
                }

                this.actionQueue.RemoveAt(0);
            }
            yield return 0;
        }
    }

    private void SendCommand(CommandType commandType, Oniyamma.LogType? logType, MirrorAction action)
    {
        switch (commandType)
        {
            case CommandType.Log:
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
                           });
                       })
                       );
                }
                break;
            case CommandType.Emotion:
                {
                    Oniyamma.OniyammaService.Current.ApplyEmotion(new EmotionParameter()
                    {
                        Kiss = action.FaceExpressions.Kiss,
                        Smile = action.FaceExpressions.Smile,
                        MouthOpen = action.FaceExpressions.MouthOpen,
                        EyesUp = action.FaceExpressions.EyesUp,
                        EyesDown = action.FaceExpressions.EyesDown,
                        EyesClosedLeft = action.FaceExpressions.EyesClosedLeft,
                        EyesClosedRight = action.FaceExpressions.EyesClosedRight,
                    });
                }
                break;
            case CommandType.Weather:
                {
                    var ui = this.commandUIs[CommandType.Weather];
                    ui.GetComponent<Animator>().Play("Fire", 0, 0.0f);
                    var weather = Oniyamma.OniyammaService.Current.GetWeather();
                    Debug.Log(weather.Type);
                }
                break;
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
