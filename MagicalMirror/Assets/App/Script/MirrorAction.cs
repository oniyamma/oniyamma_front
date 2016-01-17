public class MirrorAction
{
    public string Sentence { get; set; }
    public PXCMFaceData.ExpressionsData.FaceExpression FaceExpression { get; set; }
    public int FaceExpressionIntensity { get; set; }

    public MirrorAction() : this(string.Empty)
    {
    }

    public MirrorAction(string sentence) : this(sentence, PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_SMILE, 0)
    {
    }

    public MirrorAction(PXCMFaceData.ExpressionsData.FaceExpression faceExpression, int faceExpressionIntensity) : this(string.Empty, PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_SMILE, faceExpressionIntensity)
    {
    }

    public MirrorAction(string sentence, PXCMFaceData.ExpressionsData.FaceExpression faceExpression, int faceExpressionIntensity)
    {
        this.Sentence = sentence;
        this.FaceExpression = faceExpression;
        this.FaceExpressionIntensity = faceExpressionIntensity;
    }
}
