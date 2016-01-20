using RSUnityToolkit;
using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

public class FaceTracker : MonoBehaviour {

    public float expressionDetectInterval = 1.0f;

    public bool outputLandmarkFile = false;
    private string landmarkDataFileName = System.Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + Path.DirectorySeparatorChar + "landmark.csv";
    private PXCMFaceData.LandmarkPoint[] landmarkPoints = null;

    public UnityAction onFaceTacked = null;
    public UnityAction onFaceLost = null;

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
        var faceCount = 0;
        var currentWait = 0f;
        while (true)
        {
            if (SenseToolkitManager.Instance.FaceModuleOutput != null)
            {
                // 検出した顔の一覧を取得する
                var faces = SenseToolkitManager.Instance.FaceModuleOutput.QueryFaces();
                if (faces.Length > 0)
                {
                    if (faceCount == 0)
                    {
                        if (this.onFaceTacked != null)
                        {
                            this.onFaceTacked();
                        }
                        currentWait = 0;
                    }

                    // 最初の顔の表情を取得する
                    var face = faces[0];
                    if (currentWait > this.expressionDetectInterval)
                    {
                        var expression = face.QueryExpressions();
                        if (expression != null)
                        {
                            ExpressionInfo info = new ExpressionInfo();
                            PXCMFaceData.ExpressionsData.FaceExpressionResult result;

                            expression.QueryExpression(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_KISS, out result);
                            info.Kiss = result.intensity;
                            expression.QueryExpression(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_SMILE, out result);
                            info.Smile = result.intensity;
                            expression.QueryExpression(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_MOUTH_OPEN, out result);
                            info.MouthOpen = result.intensity;
                            expression.QueryExpression(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_EYES_UP, out result);
                            info.EyesUp = result.intensity;
                            expression.QueryExpression(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_EYES_DOWN, out result);
                            info.EyesDown = result.intensity;
                            expression.QueryExpression(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_EYES_CLOSED_LEFT, out result);
                            info.EyesClosedLeft = result.intensity;
                            expression.QueryExpression(PXCMFaceData.ExpressionsData.FaceExpression.EXPRESSION_EYES_CLOSED_RIGHT, out result);
                            info.EyesClosedRight = result.intensity;

                            AppUtis.Controller.AddAction(new MirrorAction(info));
                        }
                        currentWait = 0f;
                    }

                    var landmarks = face.QueryLandmarks();
                    if (landmarks != null)
                    {
                        var pointCount = landmarks.QueryNumPoints();
                        if (this.landmarkPoints == null)
                        {
                            this.landmarkPoints = new PXCMFaceData.LandmarkPoint[pointCount];
                        }
                        if (landmarks.QueryPoints(out this.landmarkPoints))
                        {
                            if (this.outputLandmarkFile)
                            {
                                File.AppendAllText(this.landmarkDataFileName, ",,,\n", Encoding.GetEncoding("UTF-8"));
                                foreach (PXCMFaceData.LandmarkType landmark in Enum.GetValues(typeof(PXCMFaceData.LandmarkType)))
                                {
                                    var index = landmarks.QueryPointIndex(landmark);
                                    //Debug.Log(string.Format("{0}:{1}", landmark, index));
                                    if (index > -1)
                                    {
                                        var point = landmarkPoints[index].world;
                                        var line = string.Format("{0},{1},{2},{3}\n", landmark, point.x, point.y, point.z);
                                        //Debug.Log(line);
                                        File.AppendAllText(this.landmarkDataFileName, line, Encoding.GetEncoding("UTF-8"));
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (faceCount != 0)
                    {
                        if (this.onFaceLost != null)
                        {
                            this.onFaceLost();
                        }
                    }
                }
                faceCount = faces.Length;
                currentWait += Time.deltaTime;
            }
            yield return 0;
        }
    }
}
