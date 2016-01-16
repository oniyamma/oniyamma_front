public class MirrorAction
{
    public string Sentence { get; set; }

    public MirrorAction() : this(string.Empty)
    {
    }

    public MirrorAction(string sentence)
    {
        this.Sentence = sentence;
    }
}
