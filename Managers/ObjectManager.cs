using System;
using System.Collections.Generic;
using UnityEngine;

public static class ObjectManager
{
    #region Managers
    public static GameManager GameManager;
    public static CameraController CameraManager;
    public static CanvasController CanvasManager;
    public static GameData GameData;
    public static DataBase DataBase;
    public static PlayerController PlayerManager;
    public static MapManager MapManager;
    public static PrisonerSpawner prisonerSpawner;
    #endregion

    #region GameElements
    public static GameObject Player;
    public static GameObject FinishLine;
    public static List<Cell> cells = new List<Cell>();
    #endregion
}
