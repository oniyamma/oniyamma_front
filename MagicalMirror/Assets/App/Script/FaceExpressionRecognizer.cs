using UnityEngine;
using System.Collections;
using RSUnityToolkit;

public class FaceExpressionRecognizer : MonoBehaviour {

	// Use this for initialization
	void Start ()
    {
        var senseToolkitManager = GameObject.FindObjectOfType(typeof(SenseToolkitManager));
        if (senseToolkitManager == null)
        {
            Debug.LogWarning("Sense Manager Object not found and was added automatically");
            senseToolkitManager = (GameObject)Instantiate(Resources.Load("SenseManager"));
            senseToolkitManager.name = "SenseManager";
        }
        SenseToolkitManager.Instance.SetSenseOption(SenseOption.SenseOptionID.Face);

        this.StartCoroutine(this.Process());
    }

    // Update is called once per frame
    void Update ()
    {
	}

    private IEnumerator Process()
    {
        while (true)
        {
            if (SenseToolkitManager.Instance.FaceModuleOutput != null)
            {
                // 検出した顔の一覧を取得する
                var faces = SenseToolkitManager.Instance.FaceModuleOutput.QueryFaces();
                if (faces.Length > 0)
                {
                    // 最初の顔の表情を取得する
                    var face = faces[0];
                    var expression = face.QueryExpressions();
                    if (expression != null)
                    {
                        PXCMFaceData.ExpressionsData.FaceExpressionResult result;
                        expression.QueryExpression(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_SMILE, out result);
                        AppUtis.Controller.AddAction(new MirrorAction(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_SMILE, result.intensity));
                    }
                }
            }
            yield return new WaitForSeconds(1.0f);
        }
    }
}
