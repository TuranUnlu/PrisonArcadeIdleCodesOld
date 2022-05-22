using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public GameData gameData;
    public DataBase dataBase;
    public int Level;
    bool onceForPending, onceForStart, onceForFail, onceForWin;
    public bool isTest;
    
    

    public enum GameStates
    {
        Pending,
        Start,
        Win,
        Fail
    }
    public GameStates gameState;
    
    #region OnEnable & OnDisable
    
    private void OnEnable()
    {
        EventManager.Pending += PendingGame;
        EventManager.Start += StartGame;
        EventManager.Win += WinGame;
        EventManager.Fail += FailGame;
        EventManager.BuyAction += CalculateAreaCountNGameInfo;
    }

    private void OnDisable()
    {
        EventManager.Pending -= PendingGame;
        EventManager.Start -= StartGame;
        EventManager.Win -= WinGame;
        EventManager.Fail -= FailGame;
        EventManager.BuyAction -= CalculateAreaCountNGameInfo;
    }

    void PendingGame()
    {
        if (!onceForPending)
        {
            onceForPending = true;
            gameState = GameStates.Pending;
        }
    }

    void StartGame()
    {
        if (!onceForStart)
        {
            onceForStart = true;
            gameState = GameStates.Start;
            TinySauce.OnGameStarted(levelNumber:gameData.LevelForCanvas.ToString());
        }
    }

    void FailGame()
    {
        if (!onceForFail)
        {
            onceForFail = true;
            gameState = GameStates.Fail;
            //TinySauce.OnGameFinished(false, 0, levelNumber:gameData.LevelForCanvas.ToString());
        }
    }

    void WinGame()
    {
        if (!onceForWin)
        {
            onceForWin = true;
            gameState = GameStates.Win;
            //TinySauce.OnGameFinished(true, 0, levelNumber:gameData.LevelForCanvas.ToString());
        }
    }
    #endregion
    
    private void Awake()
    {
        ObjectManager.GameManager = this;
        DataManager.LoadData(gameData);
        ObjectManager.GameData = gameData;

        if (dataBase != null)
        {
            DataManager.LoadData(dataBase);
            ObjectManager.DataBase = dataBase;
        }
    }

    void Start()
    {
        if (!isTest)
        {
            if (SceneManager.GetActiveScene().buildIndex != gameData.Level)
            {
                SceneManager.LoadScene(gameData.Level);
            }
            else
            {
                EventManager.Pending();
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonUp(0))
        {
            //EventManager.Start();
        }
    }

    private void CalculateAreaCountNGameInfo()
    {
        int cellCount = 0;
        for (int i = 0; i < dataBase.cellData.cellInfos.Count; i++)
        {
            if (dataBase.cellData.cellInfos[i] == 1)
            {
                cellCount++;
            }
        }

        cellCount -= 3;

        int totalCount = cellCount;

        totalCount = dataBase.yardData.isYardActive ? totalCount + 1 : totalCount;
        totalCount = dataBase.area3.isActive ? totalCount + 1 : totalCount;
        totalCount = dataBase.cafeteriaData.isCafeteriaActive ? totalCount + 1 : totalCount;
        
        
        for (int i = 0; i < dataBase.cafeteriaData.infoList.Count; i++)
        {
            if (dataBase.cafeteriaData.infoList[i] != 0)
            {
                totalCount++;
            }
        }

        for (int i = 0; i < dataBase.yardData.infoList.Count; i++)
        {
            if (dataBase.yardData.infoList[i] != 0)
            {
                totalCount++;
            }
        }


        EventManager.countOfBuyedAreas = totalCount;

        int yardInfo = dataBase.yardData.isYardActive ? 1 : 0;
        int cafeteriaInfo = dataBase.cafeteriaData.isCafeteriaActive ? 1 : 0;
        int area3Info = dataBase.area3.isActive ? 1 : 0;

        EventManager.gameInfo = "Total Count " + totalCount.ToString() + " Cell Count " + cellCount.ToString() + " yard status " +
                                yardInfo.ToString() +
                                " cafeteria status " + cafeteriaInfo.ToString() +
                                " area3 status " + area3Info.ToString();
        
        Debug.Log(EventManager.countOfBuyedAreas);
        Debug.Log(EventManager.gameInfo);
        TinySauce.OnGameFinished(true, 0, levelNumber: EventManager.countOfBuyedAreas.ToString());
    }
    
}
