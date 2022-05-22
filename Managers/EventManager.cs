using System;
using UnityEngine;

public static class EventManager
{
    #region GameManagerEvents
    public static Action Pending;
    public static Action Start;
    public static Action Win;
    public static Action Fail;
    #endregion

    #region CameraEvents
    public static Action<GameObject> Target;
    #endregion

    public static Action TutorialPrisonelPicked;
    public static Action TutorialCameraChange;

    public static Action BuyAction;
    public static int countOfBuyedAreas;
    public static string gameInfo;

    #region CanvasEvents
    public static Action NextButton;
    public static Action RestartButton;
    
    private static int _coin;
    public static int Coin 
    {
        get {
            return _coin;
        }
        set {
            _coin = value;
            CoinUpdate();
        }
    }
    public static Action CoinUpdate;
    #endregion
}