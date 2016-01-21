public class AppMirrorAction : MirrorAction<AppMirrorAction.AppActionTypes>
{
    public enum AppActionTypes
    {
        IdlingFeedback,

        GreetingLogging,
        WeatherQuery,
        EmotionLoging,

        FaceTrackedFeedback,
        FaceLostFeedback,
        LeaveHomeFeedback,
        GoHomeFeedback,
        WeatherQueryFeedback,
    }

    public AppMirrorAction(ActionTypes actionType) : base(actionType, string.Empty, null, HandGestures.None, emptyObject)
    {
    }
    public AppMirrorAction(ActionTypes actionType, AppMirrorAction.AppActionTypes appData) : base(actionType, string.Empty, null, HandGestures.None, appData)
    {
    }

    public AppMirrorAction(string sentence) : base(ActionTypes.HumanInput, sentence, null, HandGestures.None, emptyObject)
    {
    }

    public AppMirrorAction(ExpressionInfo expressionInfo) :
        base(ActionTypes.HumanInput, string.Empty, expressionInfo, HandGestures.None, emptyObject)
    {
    }

    public AppMirrorAction(HandGestures handGesture) :
        base(ActionTypes.HumanInput, string.Empty, null, handGesture, emptyObject)
    {
    }
}
