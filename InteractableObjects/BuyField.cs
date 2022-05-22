using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyField : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI textCost;
    [SerializeField] private Image moneyImage;
    
    public enum AreaType
    {
        Yard,
        Cafeteria,
        Area3
    }
    
    [Header("Buying Field Settings")]
    public AreaType areaType;
    public int index;
    

    private int defaultCost;
    private int currentCost;
    private string costText = "";

    private GameData gameData;
    private DataBase dataBase;
    
    // Start is called before the first frame update
    void Start()
    {
        StartMethods();
    }
    
    #region Start Metods

    private void StartMethods()
    {
        GetGameData();
        GetDataBase();
        GetDefaultCostNCheckDeActivate();
        ChangeCostTextNImage();
    }

    private void GetGameData()
    {
        gameData = ObjectManager.GameData;
    }

    private void GetDataBase()
    {
        dataBase = ObjectManager.DataBase;
    }

    private void GetDefaultCostNCheckDeActivate()
    {
        if (areaType == AreaType.Yard)
        {
           defaultCost = dataBase.yardData.defaultUpgradeCost[index];
           if (dataBase.yardData.infoList[index] != 0)
           {
               DeActivate();
           }
        }
        else if (areaType == AreaType.Cafeteria)
        {
            defaultCost = dataBase.cafeteriaData.defaultUpgradeCost[index];
            if (dataBase.cafeteriaData.infoList[index] != 0)
            {
                DeActivate();
            }
        }
        else if (areaType == AreaType.Area3)
        {
            defaultCost = dataBase.area3.defaultCost;
            if (dataBase.area3.isActive)
            {
                DeActivate();
            }
        }
    }
    #endregion

    public void ChangeCostTextNImage()
    {
        if (areaType == AreaType.Yard)
        {
            currentCost = dataBase.yardData.upgradeCost[index];
        }
        else if (areaType == AreaType.Cafeteria)
        {
            currentCost = dataBase.cafeteriaData.upgradeCost[index];
        }
        else if (areaType == AreaType.Area3)
        {
            currentCost = dataBase.area3.cost;
        }

        if (currentCost > 1000)
        {
            costText = (currentCost / 1000).ToString() + "." + ((currentCost % 1000) / 100).ToString() + " K";
        }
        else
        {
            costText = currentCost.ToString();
        }

        textCost.text = costText;

        moneyImage.fillAmount = Mathf.InverseLerp(0, defaultCost, currentCost);
    }

    public void DeActivate()
    {
        gameObject.SetActive(false);
    }

}
