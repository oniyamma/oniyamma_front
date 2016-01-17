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

    public string Sentence { get; set; }
    public PXCMFaceData.ExpressionsData.FaceExpression FaceExpression { get; set; }
    public int FaceExpressionIntensity { get; set; }
    public HandGestures HandGesture { get; set; }

    public MirrorAction() : this(string.Empty)
    {
    }

    public MirrorAction(string sentence) : this(sentence, PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_SMILE, 0, HandGestures.None)
    {
    }

    public MirrorAction(PXCMFaceData.ExpressionsData.FaceExpression faceExpression, int faceExpressionIntensity) : 
        this(string.Empty, PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_SMILE, faceExpressionIntensity, HandGestures.None)
    {
    }

    public MirrorAction(HandGestures handGesture) : 
        this(string.Empty, PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_SMILE, 0, handGesture)
    {
    }

    public MirrorAction(string sentence, PXCMFaceData.ExpressionsData.FaceExpression faceExpression, int faceExpressionIntensity, HandGestures handGesture)
    {
        this.Sentence = sentence;
        this.FaceExpression = faceExpression;
        this.FaceExpressionIntensity = faceExpressionIntensity;
        this.HandGesture = handGesture;
    }
}
