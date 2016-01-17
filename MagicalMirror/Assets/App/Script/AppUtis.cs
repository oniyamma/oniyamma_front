using System;
using System.IO;
using UnityEngine;

public static class AppUtis
{
    private static Controller controller = null;
    public static Controller Controller
    {
        get
        {
            if (controller == null)
            {
                controller = GameObject.FindWithTag("GameController").GetComponent<Controller>();
            }
            return controller;
        }
    }

    public const string AppTitle = "MagicalMirror";

    public static string AppScreenShotPath
    {
        get
        {
            var path = System.Environment.GetFolderPath(Environment.SpecialFolder.MyPictures) + Path.DirectorySeparatorChar + AppUtis.AppTitle;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            return path;
        }
    }

}
