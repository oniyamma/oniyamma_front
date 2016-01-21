using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpeechRecognizer : MonoBehaviour
{
    public int languageIndex = 1;

    private PXCMAudioSource source;
    private PXCMSpeechRecognition recognizer;
    private PXCMSession session;

    private static IList<PXCMAudioSource.DeviceInfo> devices = new List<PXCMAudioSource.DeviceInfo>();
    private PXCMAudioSource.DeviceInfo device = new PXCMAudioSource.DeviceInfo();

    private static Controller gameController;

    [Range(0.2f, 0.8f)]
    public float setVolume = 0.2f;

    void Start()
    {
        gameController = AppUtis.Controller;

        this.session = PXCMSession.CreateInstance();
        this.AudioSourceCheck();
        this.InitSession(session);
    }

    void AudioSourceCheck()
    {
        /* Create the AudioSource instance */
        this.source = session.CreateAudioSource();

        if (this.source != null)
        {
            this.source.ScanDevices();

            for (int i = 0; ; i++)
            {
                PXCMAudioSource.DeviceInfo dinfo;
                if (source.QueryDeviceInfo(i, out dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR)
                {
                    break;
                }

                devices.Add(dinfo);
                UnityEngine.Debug.Log("Device : " + dinfo.name);
            }
        }
    }

    void OnAlert(PXCMSpeechRecognition.AlertData data)
    {
        Debug.Log(data.label);
    }

    static void OnRecognition(PXCMSpeechRecognition.RecognitionData data)
    {
        UnityEngine.Debug.Log("RECOGNIZED sentence : " + data.scores[0].sentence);
        UnityEngine.Debug.Log("RECOGNIZED tags : " + data.scores[0].tags);

        gameController.AddAction(new AppMirrorAction(data.scores[0].sentence));
    }

    void InitSession(PXCMSession session)
    {
        if (source == null)
        {
            Debug.Log("Source was null!  No audio device?");
            return;
        }

        // Set audio volume to 0.2 
        source.SetVolume(setVolume);

        // Set Audio Source 
        Debug.Log("Using device: " + device.name);
        source.SetDevice(device);

        // Set Module 
        PXCMSession.ImplDesc mdesc = new PXCMSession.ImplDesc();
        mdesc.iuid = 0;

        var status = session.CreateImpl<PXCMSpeechRecognition>(out this.recognizer);

        if (status >= pxcmStatus.PXCM_STATUS_NO_ERROR)
        {
            // Configure 
            PXCMSpeechRecognition.ProfileInfo pinfo;
            // Language
            this.recognizer.QueryProfile(this.languageIndex, out pinfo);
            Debug.Log(pinfo.language);
            this.recognizer.SetProfile(pinfo);

            // Set Command/Control or Dictation 
            this.recognizer.SetDictation();

            // Initialization 
            Debug.Log("Init Started");
            PXCMSpeechRecognition.Handler handler = new PXCMSpeechRecognition.Handler();
            handler.onRecognition = OnRecognition;
            handler.onAlert = OnAlert;

            status = this.recognizer.StartRec(source, handler);

            if (status >= pxcmStatus.PXCM_STATUS_NO_ERROR)
            {
                Debug.Log("Voice Rec Started");
            }
            else
            {
                Debug.Log("Voice Rec Start Failed");
            }
        }
        else
        {
            Debug.Log("Voice Rec Session Failed");
        }
    }

    void OnApplicationQuit()
    {
        this.recognizer.StopRec();
        Debug.Log("Clean up using OnApplicationQuit");
        this.recognizer.Dispose();
    }
}
