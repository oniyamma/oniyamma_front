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

    private IList<AppMirrorAction> actionQueue;

    private static IDictionary<string, object> sentenceCommandMap = new Dictionary<string, object>
    {
           { "行ってき", Oniyamma.LogType.LEAVE_HOME },
           { "いってき", Oniyamma.LogType.LEAVE_HOME },
           { "ただい", Oniyamma.LogType.GO_HOME },
           { "天気", AppMirrorAction.AppActionTypes.WeatherQuery },
           { "てんき", AppMirrorAction.AppActionTypes.WeatherQuery },
    };
    private static IDictionary<AppMirrorAction.HandGestures, object> gestureCommandMap = new Dictionary<AppMirrorAction.HandGestures, object>
    {
           { AppMirrorAction.HandGestures.ThumbsUp, Oniyamma.LogType.LEAVE_HOME },
           { AppMirrorAction.HandGestures.ThumbsDown, Oniyamma.LogType.GO_HOME },
           { AppMirrorAction.HandGestures.VSign, AppMirrorAction.AppActionTypes.WeatherQuery },
    };
    private static IDictionary<Oniyamma.LogType, AppMirrorAction.AppActionTypes> serviceUiFeedbackMap = new Dictionary<Oniyamma.LogType, AppMirrorAction.AppActionTypes>
    {
        { Oniyamma.LogType.LEAVE_HOME, AppMirrorAction.AppActionTypes.LeaveHomeFeedback },
        { Oniyamma.LogType.GO_HOME, AppMirrorAction.AppActionTypes.GoHomeFeedback },
    };
    private IDictionary<AppMirrorAction.AppActionTypes, GameObject> commandUIMap;
    private Animator menuAnimator;

    // Use this for initialization
    void Start()
    {
        this.actionQueue = new List<AppMirrorAction>();
        this.commandUIMap = new Dictionary<AppMirrorAction.AppActionTypes, GameObject>()
           {
               { AppMirrorAction.AppActionTypes.LeaveHomeFeedback, this.leaveHomeCommandUI },
               { AppMirrorAction.AppActionTypes.GoHomeFeedback, this.goHomeCommandUI },
               { AppMirrorAction.AppActionTypes.WeatherQueryFeedback, this.weatherCommandUI },
          };

        this.menuAnimator = this.informationPanel.GetComponent<Animator>();
        this.menuAnimator.SetBool("visible", false);
        this.bird.SetActive(true);

        var faceTracker = this.GetComponent<FaceTracker>();
        faceTracker.onFaceTacked = delegate ()
        {
            this.AddAction(new AppMirrorAction(MirrorAction<AppMirrorAction.AppActionTypes>.ActionTypes.SystemFeedback, AppMirrorAction.AppActionTypes.FaceTrackedFeedback));
        };
        faceTracker.onFaceLost = delegate ()
        {
            this.AddAction(new AppMirrorAction(MirrorAction<AppMirrorAction.AppActionTypes>.ActionTypes.SystemFeedback, AppMirrorAction.AppActionTypes.FaceLostFeedback));
        };

        this.StartCoroutine(this.MainProcess());
    }

    // Update is called once per frame
    void Update()
    {
    }

    private IEnumerator MainProcess()
    {
        while (true)
        {
            if (this.actionQueue.Count > 0)
            {
                var action = this.actionQueue[0];
                switch (action.ActionType)
                {
                    case AppMirrorAction.ActionTypes.HumanInput:
                        this.ProcessHumanInput(action);
                        break;
                    case AppMirrorAction.ActionTypes.SystemFeedback:
                        this.ProcessSystemFeedback(action);
                        break;
                }
                this.actionQueue.RemoveAt(0);
            }
            yield return 0;
        }
    }

    private void ProcessHumanInput(AppMirrorAction action)
    {
        if (action.FaceExpressions != null)
        {
            this.RequestServiceCommand(AppMirrorAction.AppActionTypes.EmotionLoging, null, action);
            this.echoExpressionText.text = string.Format("SMILE : {0}", action.FaceExpressions.Smile);
        }
        else
        {
            if (action.Sentence != string.Empty)
            {
                this.echoSentenseText.text = action.Sentence;
                foreach (var key in sentenceCommandMap.Keys)
                {
                    if (action.Sentence.IndexOf(key) > -1)
                    {
                        if (sentenceCommandMap[key] is Oniyamma.LogType)
                        {
                            this.RequestServiceCommand(AppMirrorAction.AppActionTypes.GreetingLogging, (Oniyamma.LogType)sentenceCommandMap[key], action);
                        }
                        if (sentenceCommandMap[key] is AppMirrorAction.AppActionTypes)
                        {
                            this.RequestServiceCommand((AppMirrorAction.AppActionTypes)sentenceCommandMap[key], null, action);
                        }
                    }
                }
            }
            if (action.HandGesture != AppMirrorAction.HandGestures.None)
            {
                this.echoGestureText.text = AppMirrorAction.handGestureNames[action.HandGesture];
                foreach (var key in gestureCommandMap.Keys)
                {
                    if (action.HandGesture == key)
                    {
                        if (gestureCommandMap[key] is Oniyamma.LogType)
                        {
                            this.RequestServiceCommand(AppMirrorAction.AppActionTypes.GreetingLogging, (Oniyamma.LogType)gestureCommandMap[key], action);
                        }
                        if (gestureCommandMap[key] is AppMirrorAction.AppActionTypes)
                        {
                            this.RequestServiceCommand((AppMirrorAction.AppActionTypes)gestureCommandMap[key], null, action);
                        }
                    }
                }
            }
        }
    }

    private void RequestServiceCommand(AppMirrorAction.AppActionTypes actionType, Oniyamma.LogType? logType, AppMirrorAction action)
    {
        switch (actionType)
        {
            case AppMirrorAction.AppActionTypes.GreetingLogging:
                {
                    this.StartCoroutine(this.TakePhoto(
                       delegate (string fileName)
                       {
                           Debug.Log(string.Format("Command:{0}  ScreenShot:{1}", logType, fileName));

                           OniyammaService.Current.AddLog(new LogParameter()
                           {
                               Type = logType,
                               FilePath = fileName,
                               UserId = action.UserName,
                           });
                       })
                       );
                    this.AddAction(new AppMirrorAction(MirrorAction<AppMirrorAction.AppActionTypes>.ActionTypes.SystemFeedback, serviceUiFeedbackMap[(Oniyamma.LogType)logType]));
                }
                break;
            case AppMirrorAction.AppActionTypes.EmotionLoging:
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
            case AppMirrorAction.AppActionTypes.WeatherQuery:
                {
                    var weather = Oniyamma.OniyammaService.Current.GetWeather();
                    Debug.Log(weather.Type);
                }
                this.AddAction(new AppMirrorAction(MirrorAction<AppMirrorAction.AppActionTypes>.ActionTypes.SystemFeedback, AppMirrorAction.AppActionTypes.WeatherQueryFeedback));
                break;
        }
    }

    private void ProcessSystemFeedback(AppMirrorAction action)
    {
        Debug.Log(action.HasAppData);
        if (action.HasAppData)
        {
            var actionType = action.AppData;
            switch (actionType)
            {
                case AppMirrorAction.AppActionTypes.FaceTrackedFeedback:
                    {
                        this.StartCoroutine(this.OnFaceActivted());
                    }
                    break;
                case AppMirrorAction.AppActionTypes.FaceLostFeedback:
                    {
                        this.StartCoroutine(this.OnFaceLost());
                    }
                    break;
                case AppMirrorAction.AppActionTypes.LeaveHomeFeedback:
                case AppMirrorAction.AppActionTypes.GoHomeFeedback:
                case AppMirrorAction.AppActionTypes.WeatherQueryFeedback:
                    {
                        var ui = this.commandUIMap[actionType];
                        ui.GetComponent<Animator>().Play("Fire", 0, 0.0f);
                    }
                    break;
            }
        }
    }
    public void AddAction(AppMirrorAction action)
    {
        this.actionQueue.Add(action);
    }

    private IEnumerator OnFaceActivted()
    {
        this.GetComponent<AudioSource>().PlayOneShot(this.faceTrackedSound);
        this.informationPanel.GetComponent<Animator>().SetBool("visible", true);
        this.bird.SetActive(true);
        yield return new WaitForSeconds(1);
        this.airplane.SetActive(true);
    }

    private IEnumerator OnFaceLost()
    {
        this.informationPanel.GetComponent<Animator>().SetBool("visible", false);
        this.bird.SetActive(false);
        this.airplane.SetActive(false);
        yield break;
    }

    public void OnGestureThumbUp()
    {
        this.AddAction(new AppMirrorAction(AppMirrorAction.HandGestures.ThumbsUp));
    }

    public void OnGestureThumbDown()
    {
        this.AddAction(new AppMirrorAction(AppMirrorAction.HandGestures.ThumbsDown));
    }

    public void OnGestureVSign()
    {
        this.AddAction(new AppMirrorAction(AppMirrorAction.HandGestures.VSign));
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
        var menuVisible = this.menuAnimator.GetBool("visible");
        var speed = this.menuAnimator.speed;
        this.menuAnimator.speed = 100;

        this.UIRoot.SetActive(false);

        yield return new WaitForEndOfFrame();

        Application.CaptureScreenshot(fileName);
        this.UIRoot.SetActive(true);
        this.menuAnimator.SetBool("visible", menuVisible);
        this.menuAnimator.speed = speed;
        callback(fileName);
    }
}
