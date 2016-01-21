using System.Collections.Generic;

public class MirrorAction<T>
{
    public enum ActionTypes
    {
        None,
        HumanInput,
        SystemFeedback,
    }

    public enum HandGestures 
    {
        None,
        ThumbsUp,
        ThumbsDown,
        VSign,
    }

    public static IDictionary<HandGestures, string> handGestureNames = new Dictionary<HandGestures, string>()
    {
        {HandGestures.ThumbsUp, "Thumbs Up" },
        {HandGestures.ThumbsDown, "Thumbs Down" },
        {HandGestures.VSign, "V_Sign" },
    };

    public ActionTypes ActionType { get; set; }
    public string UserName { get; set; }
    public string Sentence { get; set; }
    public HandGestures HandGesture { get; set; }
    public ExpressionInfo FaceExpressions;
    public T AppData { get; set; }

    public bool HasAppData
    {
        get
        {
            return !this.AppData.Equals(emptyObject);
        }
    }

    protected static T emptyObject;

    public MirrorAction(ActionTypes actionType, string sentence, ExpressionInfo expressionInfo, HandGestures handGesture, T appData)
    {
        this.ActionType = actionType;
            
        this.UserName = "Taro";
        this.Sentence = sentence;
        this.HandGesture = handGesture;
        this.FaceExpressions = expressionInfo;
        this.AppData = appData;
    }
}
