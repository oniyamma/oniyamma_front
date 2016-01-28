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
    public Animator menuAnimator;
    public GameObject leaveHomeCommandUI;
    public GameObject goHomeCommandUI;
    public GameObject weatherCommandUI;
    public AudioClip faceTrackedSound;

    public GameObject bird;
    public GameObject pig;
    public GameObject dog;
    public GameObject duck;
    public GameObject rabbit;
    public GameObject airplane;
    public GameObject sunny;
    public GameObject cloudy;
    public GameObject snow;
    public GameObject rain;
    public GameObject unityChanBase;
    public GameObject unityChan;
    public GameObject unityChanBase2;

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
    private WeatherType currentWeather = WeatherType.SUNNY;
    public float weatherFeedbackActionTime = 5.0f;

    private GameObject[] leaveHomeEffects;
    private GameObject[] goHomeEffects;
    private GameObject[] idlingRandomEffects;

    private BackendService Service { get; set; }

    // Use this for initialization
    void Start()
    {
        this.Service = this.GetComponent<BackendService>();

        this.actionQueue = new List<AppMirrorAction>();
        this.commandUIMap = new Dictionary<AppMirrorAction.AppActionTypes, GameObject>()
           {
               { AppMirrorAction.AppActionTypes.LeaveHomeFeedback, this.leaveHomeCommandUI },
               { AppMirrorAction.AppActionTypes.GoHomeFeedback, this.goHomeCommandUI },
               { AppMirrorAction.AppActionTypes.WeatherQueryFeedback, this.weatherCommandUI },
          };
        this.leaveHomeEffects = new GameObject[]
        {
            this.airplane,
        };
        this.goHomeEffects = new GameObject[]
        {
        };
        this.idlingRandomEffects = new GameObject[]
        {
            this.bird,
            this.pig,
            this.dog,
            this.duck,
            this.rabbit,
            this.unityChanBase2,
            this.unityChanBase2,
        };

        this.menuAnimator.SetTrigger("hideTrigger");

        foreach (var effect in this.idlingRandomEffects)
        {
            effect.SetActive(false);
        }

        this.bird.SetActive(false);
        this.airplane.SetActive(false);
        this.sunny.SetActive(false);
        this.cloudy.SetActive(false);
        this.rain.SetActive(false);
        this.snow.SetActive(false);

        this.unityChan.SetActive(false);
        this.unityChanBase.SetActive(false);

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
        this.StartCoroutine(this.BackgroundProcess());
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
            this.echoExpressionText.text = string.Format("SMILE:{0} KISS:{1}", action.FaceExpressions.Smile, action.FaceExpressions.Kiss);
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
                           Debug.Log(string.Format("Command:{0}  ScreenShot:{1} {2}", logType, fileName, File.Exists(fileName)));
                           this.Service.AddLog(new LogParameter()
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
                    Debug.Log(string.Format("Command:{0}  SMILE:{1}", "Emotion", action.FaceExpressions.Smile));
                    this.Service.ApplyEmotion(new EmotionParameter()
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
                    var weathers = new WeatherType[4]
                    {
                        WeatherType.SUNNY,
                        WeatherType.CLOUDY,
                        WeatherType.RAINY,
                        WeatherType.SNOW,
                    };
                    var weather = this.Service.GetWeather(weathers[UnityEngine.Random.Range(0, 4)]);
                    Debug.Log(weather.Type);
                    this.currentWeather = weather.Type;
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
                    {
                        var ui = this.commandUIMap[actionType];
                        ui.GetComponent<Animator>().Play("Fire", 0, 0.0f);
                        this.StartCoroutine(this.OnLeaveHomeFeedback());
                    }
                    break;
                case AppMirrorAction.AppActionTypes.GoHomeFeedback:
                    {
                        var ui = this.commandUIMap[actionType];
                        ui.GetComponent<Animator>().Play("Fire", 0, 0.0f);
                        this.StartCoroutine(this.OnGoHomeFeedback());
                    }
                    break;
                case AppMirrorAction.AppActionTypes.WeatherQueryFeedback:
                    {
                        var ui = this.commandUIMap[actionType];
                        ui.GetComponent<Animator>().Play("Fire", 0, 0.0f);
                        this.StartCoroutine(this.OnWeatherFeedback());
                    }
                    break;
            }
        }
    }

    private IEnumerator OnLeaveHomeFeedback()
    {
        foreach (var effect in this.leaveHomeEffects)
        {
            effect.SetActive(false);
            effect.SetActive(true);
        }
        this.unityChan.GetComponent<Animator>().CrossFade("KneelDown", 0);
        yield return new WaitForSeconds(3f);
        
        yield return new WaitForSeconds(7f);
        this.unityChan.SetActive(false);
        this.unityChan.SetActive(true);
        foreach (var effect in this.leaveHomeEffects)
        {
            effect.SetActive(false);
        }
    }

    private IEnumerator OnGoHomeFeedback()
    {
        foreach (var effect in this.goHomeEffects)
        {
            effect.SetActive(false);
            effect.SetActive(true);
        }
        this.unityChan.GetComponent<Animator>().CrossFade("TopOfJump(loop)", 0);
        yield return new WaitForSeconds(1.5f);
        this.unityChan.GetComponent<Animator>().CrossFade("TopOfJump(loop)", 0);
        yield return new WaitForSeconds(1.5f);
        this.unityChan.GetComponent<Animator>().CrossFade("TopOfJump(loop)", 0);

        yield return new WaitForSeconds(7f);

        foreach (var effect in this.goHomeEffects)
        {
            effect.SetActive(false);
        }
    }
    private IEnumerator OnWeatherFeedback()
    {
        GameObject target = null;
        switch(this.currentWeather)
        {
            case WeatherType.SUNNY:
                target = this.sunny;
                break;
            case WeatherType.CLOUDY:
                target = this.cloudy;
                break;
            case WeatherType.RAINY:
                target = this.rain;
                break;
            case WeatherType.SNOW:
                target = this.snow;
                break;
        }
        if (target == null)
        {
            yield break;
        }
        target.SetActive(true);
        yield return new WaitForSeconds(this.weatherFeedbackActionTime);
        target.SetActive(false);
    }

    public void AddAction(AppMirrorAction action)
    {
        this.actionQueue.Add(action);
    }

    private IEnumerator OnFaceActivted()
    {
        this.GetComponent<AudioSource>().PlayOneShot(this.faceTrackedSound);
        this.menuAnimator.SetTrigger("showTrigger");
        this.unityChanBase.SetActive(true);
        this.unityChan.SetActive(true);
        this.unityChan.GetComponent<Animator>().CrossFade("TopOfJump(loop)", 0);
        yield break;
    }

    private IEnumerator OnFaceLost()
    {
        this.menuAnimator.SetTrigger("hideTrigger");
        this.unityChan.SetActive(false);
        this.unityChanBase.SetActive(false);
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
        var speed = this.menuAnimator.speed;
        this.menuAnimator.speed = 100;
        var menuVisible = this.menuAnimator.GetBool("visible");

        this.UIRoot.SetActive(false);

        yield return new WaitForEndOfFrame();

        Application.CaptureScreenshot(fileName);
        this.UIRoot.SetActive(true);
        this.menuAnimator.SetBool("visible", menuVisible);
        this.menuAnimator.SetTrigger("showTrigger");
        this.menuAnimator.speed = speed;

        yield return new WaitForSeconds(0.5f);

        callback(fileName);
    }

    private IEnumerator BackgroundProcess()
    {
        while (true)
        {
            var nextWait = UnityEngine.Random.Range(5f, 15f);

            var target = this.idlingRandomEffects[UnityEngine.Random.Range(0, this.idlingRandomEffects.Length)];
            target.SetActive(false);
            target.SetActive(true);

            yield return new WaitForSeconds(nextWait);
        }
    }
}
