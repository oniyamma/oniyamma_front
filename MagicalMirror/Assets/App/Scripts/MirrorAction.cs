using System.Collections.Generic;

public class MirrorAction
{
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

    public string UserName { get; set; }
    public string Sentence { get; set; }
    public HandGestures HandGesture { get; set; }
    public ExpressionInfo FaceExpressions;

    public MirrorAction() : this(string.Empty)
    {
    }

    public MirrorAction(string sentence) : this(sentence, null, HandGestures.None)
    {
    }

    public MirrorAction(ExpressionInfo expressionInfo) : 
        this(string.Empty, expressionInfo, HandGestures.None)
    {
    }

    public MirrorAction(HandGestures handGesture) : 
        this(string.Empty, null, handGesture)
    {
    }

    public MirrorAction(string sentence, ExpressionInfo expressionInfo, HandGestures handGesture)
    {
        this.UserName = "Taro";
        this.Sentence = sentence;
        this.HandGesture = handGesture;
        this.FaceExpressions = expressionInfo;
    }
}
