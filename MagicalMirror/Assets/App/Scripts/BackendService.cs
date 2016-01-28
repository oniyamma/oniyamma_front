using UnityEngine;
using System.Collections;
using Oniyamma;

public class BackendService : MonoBehaviour {

    public bool isOnline = true;
    public string serviceURL = "http://10.0.1.148:3000";

    // Use this for initialization
    void Start () {
        Oniyamma.OniyammaService.Current.Init(this.serviceURL);
    }

    // Update is called once per frame
    void Update () {
	
	}

    public void AddLog(LogParameter logParameter)
    {
        if (this.isOnline)
        {
            OniyammaService.Current.AddLog(logParameter);
        }
    }

    public void ApplyEmotion(EmotionParameter emotioinParameter)
    {
        if (this.isOnline)
        {
            Oniyamma.OniyammaService.Current.ApplyEmotion(emotioinParameter);
        }
    }

    public WeatherInfo GetWeather(WeatherType dummyType = WeatherType.SUNNY)
    {
        if (this.isOnline)
        {
            return Oniyamma.OniyammaService.Current.GetWeather(dummyType);
        }
        var weatherInfo = new WeatherInfo();
        weatherInfo.Type = dummyType;
        return weatherInfo;
    }
}
