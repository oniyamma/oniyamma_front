using UnityEngine;
using System.Collections;
using RSUnityToolkit;
using System;
using System.IO;
using System.Text;

public class FaceRecognizer : MonoBehaviour {

    private PXCMFaceData.LandmarkPoint[] landmarkPoints = null;

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


    }

    // Update is called once per frame
    void Update ()
    {
        if (SenseToolkitManager.Instance.FaceModuleOutput == null)
        {
            return;
        }

        // 検出した顔の一覧を取得する
        var faces = SenseToolkitManager.Instance.FaceModuleOutput.QueryFaces();
        if (faces.Length == 0)
        {
            return;
        }

        // 最初の顔の表情を取得する
        var face = faces[0];
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
                File.AppendAllText("E:\\Users\\maemoto\\Desktop\\landmark.csv", ",,,\n", Encoding.GetEncoding("UTF-8"));
                foreach (PXCMFaceData.LandmarkType landmark in Enum.GetValues(typeof(PXCMFaceData.LandmarkType)))
                {
                    var index = landmarks.QueryPointIndex(landmark);
                    //Debug.Log(string.Format("{0}:{1}", landmark, index));
                    if (index > -1)
                    {
                        var point = landmarkPoints[index].world;
                        var line = string.Format("{0},{1},{2},{3}\n", landmark, point.x, point.y, point.z);
                        //Debug.Log(line);
                        File.AppendAllText("E:\\Users\\maemoto\\Desktop\\landmark.csv", line, Encoding.GetEncoding("UTF-8"));
                    }
                }
            }
        }
    }
}
