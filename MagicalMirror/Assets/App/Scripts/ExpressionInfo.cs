using UnityEngine;
using System.Collections;

public class ExpressionInfo
{
    public int? Kiss { get; set; }
    public int? Smile { get; set; }
    public int? MouthOpen { get; set; }
    public int? EyesUp { get; set; }
    public int? EyesDown { get; set; }
    public int? EyesClosedLeft { get; set; }
    public int? EyesClosedRight { get; set; }

    public ExpressionInfo()
    {
        this.Kiss = null;
        this.Smile = null;
        this.MouthOpen = null;
        this.EyesUp = null;
        this.EyesDown = null;
        this.EyesClosedLeft = null;
        this.EyesClosedRight = null;
    }
}
