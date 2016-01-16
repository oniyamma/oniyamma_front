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

}
