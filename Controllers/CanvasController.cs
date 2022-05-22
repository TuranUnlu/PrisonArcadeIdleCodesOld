using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CanvasController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMoney;
    public Image PlayerCircle;
    public Image PlayerCircleOutline;
    
    [Header("JoyStick")]
    //
    public FloatingJoystick joystick;
    
    [Header("Buttons")]
    public Button NextButton, RestartButton;
    [SerializeField] private Button ButtonTapToStart;

    [Header("Panel Buy")]
    //
    [SerializeField] private GameObject BuyPanel;
    [SerializeField] private Button ButtonBuyPanelClose;
    [SerializeField] private Button ButtonBuyPanelBuyGuard;
    [SerializeField] private Slider sliderGuardAmount;
    [SerializeField] private TextMeshProUGUI textBuyGuardCost;
    [SerializeField] private Button ButtonBuyPanelUpgradeStack;
    [SerializeField] private Slider sliderPlayerStack;
    [SerializeField] private TextMeshProUGUI textUpgradeStackCost;
    
    GameManager GameManager;
    private GameData gameData;
    private DataBase dataBase;
    private MapManager mapManager;
    
    #region OnEnable & OnDisable
    private void OnEnable()
    {
        EventManager.Pending += PendingEvent;
        EventManager.Start += StartEvent;
        EventManager.Fail += FailEvent;
        EventManager.Win += WinEvent;
        EventManager.NextButton += NextButtonClick;
        EventManager.RestartButton += RestartButtonClick;
        EventManager.CoinUpdate += CoinUpdate;
    }

    private void OnDisable()
    {
        EventManager.Pending -= PendingEvent;
        EventManager.Start -= StartEvent;
        EventManager.Fail -= FailEvent;
        EventManager.Win -= WinEvent;
        EventManager.NextButton -= NextButtonClick;
        EventManager.RestartButton -= RestartButtonClick;
        EventManager.CoinUpdate -= CoinUpdate;
    }
    
    void PendingEvent()
    {
        StateChanger(0);
    }
    
    void StartEvent()
    {
        StateChanger(1);
    }
    
    void FailEvent()
    {
        StateChanger(2);
    }
    
    void WinEvent()
    {
        StateChanger(3);
    }
    public void StateChanger(int k)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (i != 5)
            {
                transform.GetChild(i).gameObject.SetActive(false);
            }
        }
        transform.GetChild(k).gameObject.SetActive(true);
    }
    public void RestartButtonClick()
    {
        RestartButton.interactable = false;
        SceneManager.LoadScene(ObjectManager.GameData.Level);
    }
    public void NextButtonClick()
    {
        NextButton.interactable = false;

        ObjectManager.GameData.LevelForCanvas++;

        if (SceneManager.sceneCountInBuildSettings - 1 > ObjectManager.GameData.Level)
        {
            ObjectManager.GameData.Level++;
        }
        else
        {
            ObjectManager.GameData.Level = 1;
            ObjectManager.GameData.Loop = true;
        }

        ObjectManager.GameData.Coin = EventManager.Coin;
        DataManager.SaveData(ObjectManager.GameData);
        SceneManager.LoadScene(ObjectManager.GameData.Level);
    }
    
    void CoinUpdate()
    {
        string moneyText;
        int moneyAmount = EventManager.Coin;
        if (moneyAmount > 1000)
        {
            moneyText = (moneyAmount / 1000).ToString() + "." + ((moneyAmount % 1000) / 100).ToString() + " K";
        }
        else
        {
            moneyText = moneyAmount.ToString();
        }

        //textMoney.text = EventManager.Coin.ToString();
        textMoney.text = moneyText;
    }
    #endregion
    
    private void Awake()
    {
        ObjectManager.CanvasManager = this;
    }
    void Start()
    {
        StartMethods();
    }
    
    #region Start Methods

    private void StartMethods()
    {
        GetGameManager();
        GetGameData();
        GetDataBase();
        GetMapManger();
        EventManager.Coin = gameData.Coin;
    }
    
    private void GetGameManager()
    {
        GameManager = ObjectManager.GameManager;
    }

    private void GetGameData()
    {
        gameData = ObjectManager.GameData;
    }

    private void GetDataBase()
    {
        dataBase = ObjectManager.DataBase;
    }

    private void GetMapManger()
    {
        mapManager = ObjectManager.MapManager;
    }
    #endregion
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ShowBuyPanel();
        }
    }

    public void ButtonActionTapToStart()
    {
        ButtonTapToStart.interactable = false;
        EventManager.Start();
    }

    public void ButtonActionIncreaseMoney()
    {
        gameData.Coin += 5000;
        EventManager.Coin = gameData.Coin;
    }
    
    #region Buy Panel

    public void ShowBuyPanel()
    {
        BuyPanel.SetActive(true);
        ObjectManager.PlayerManager.ChangeBuyPanelStatus(true);
        joystick.gameObject.SetActive(false);
        FillBuyPanel();
    }

    private void FillBuyPanel()
    {
        if (dataBase.guardData.guardCountCurrent < dataBase.guardData.guardCountMax)
        {
            textBuyGuardCost.text = dataBase.guardData.guarBuyCosts[dataBase.guardData.guardCountCurrent].ToString();
            if (dataBase.guardData.guarBuyCosts[dataBase.guardData.guardCountCurrent] > gameData.Coin)
            {
                ButtonBuyPanelBuyGuard.interactable = false;
            }
            else
            {
                ButtonBuyPanelBuyGuard.interactable = true;
            }
        }
        else
        {
            ButtonBuyPanelBuyGuard.interactable = false;
            textBuyGuardCost.enabled = false;
        }
        ChangeSliderGuardCount();

        if (dataBase.player.objectStackCount < dataBase.player.objectStackCountMax)
        {
            textUpgradeStackCost.text = dataBase.player.upgradeStackCosts[dataBase.player.objectStackCount].ToString();
            if (dataBase.player.upgradeStackCosts[dataBase.player.objectStackCount] > gameData.Coin)
            {
                ButtonBuyPanelUpgradeStack.interactable = false;
            }
            else
            {
                ButtonBuyPanelUpgradeStack.interactable = true;
            }
        }
        else
        {
            ButtonBuyPanelUpgradeStack.interactable = false;
            textUpgradeStackCost.enabled = false;
        }
        ChangeSliderPlayerStackCount();
    }

    private void ChangeSliderGuardCount()
    {
        sliderGuardAmount.maxValue = dataBase.guardData.guardCountMax;
        sliderGuardAmount.value = dataBase.guardData.guardCountCurrent;
    }
    public void ButtonActionBuyPanelBuyGuard()
    {
        ButtonBuyPanelBuyGuard.interactable = false;
        gameData.Coin -= dataBase.guardData.guarBuyCosts[dataBase.guardData.guardCountCurrent];
        dataBase.guardData.guardCountCurrent++;
        EventManager.Coin = gameData.Coin;
        DataManager.SaveData(dataBase);
        DataManager.SaveData(gameData);
        FillBuyPanel();
        mapManager.SpawnGuard();
    }

    private void ChangeSliderPlayerStackCount()
    {
        sliderPlayerStack.maxValue = dataBase.player.objectStackCountMax;
        sliderPlayerStack.value = dataBase.player.objectStackCount;
    }

    public void ButtonActionBuyPanelUpgradePlayerStackCount()
    {
        ButtonBuyPanelUpgradeStack.interactable = false;
        gameData.Coin -= dataBase.player.upgradeStackCosts[dataBase.player.objectStackCount];
        dataBase.player.objectStackCount++;
        EventManager.Coin = gameData.Coin;
        DataManager.SaveData(dataBase);
        DataManager.SaveData(gameData);
        FillBuyPanel();
    }
    public void ButtonActionCloseBuyPanel()
    {
        BuyPanel.SetActive(false);
        joystick.gameObject.SetActive(true);
        ObjectManager.PlayerManager.ChangeBuyPanelStatus(false);
    }
    
    #endregion
}
