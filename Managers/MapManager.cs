using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class MapManager : MonoBehaviour
{
    [SerializeField] private AnimationCurve costCurve;

    #region Yard Settings & Variables

    [Header("Yard & Yard Objects")]
    //
    [SerializeField] private GameObject yardMoneyField;
    [SerializeField] private List<GameObject> yardUpgradeFields;
    [SerializeField] private GameObject yard;
    public Transform BasketballHoop;

    [SerializeField] private Image imageBuyField;
    [SerializeField] private TextMeshProUGUI textCostBuyField;
    public Transform yardColliderArea;
    [SerializeField] private Transform yardMoneySpawnPoint;

    #endregion

    #region Cafeteria Settings & Variables

    [Header("Cefateria & Cafeteria Objects")]
    //
    [SerializeField] private GameObject cafeteria;

    [SerializeField] private List<GameObject> cafeteriaUpgradeFiels = new List<GameObject>();
    [SerializeField] private GameObject cafeteriaMoneyField;
    [SerializeField] private Image imageBuyFiledCafeteria;
    [SerializeField] private TextMeshProUGUI textCostCostBuyFieldCafeteria;
    [SerializeField] private Transform cafeteriaMoneySpawnPoint;
    public Transform cafeteriaColliderArea;

    #endregion
    
    [Header("Area 3 Settings")]
    [SerializeField] private GameObject area3;

    [SerializeField] private GameObject area3MoneyField;
    [SerializeField] private Transform area3MoneySpawnPoint;

    private GameData gameData;
    private DataBase dataBase;

    public int fieldCountYard;
    public int fieldCountCafeteria;

    public List<ActionFields> fieldsYard = new List<ActionFields>();
    public List<ActionFields> fieldsCafeteria = new List<ActionFields>();

    public List<Cell> cells = new List<Cell>();

    public Transform mainDoor;

    public TextMeshProUGUI errorMessage;

    public List<PrisonerController> prisonerList = new List<PrisonerController>();

    public Resource resourceCloth;
    public Resource resourceFood;

    public Transform guardSpawnPos;
    public Transform guardIdlePos;


    public Transform triggerField;

    [SerializeField] private GameObject arrowCloth;

    #region OnEnable & OnDisable

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
    }

    #endregion

    private void Awake()
    {
        AwakeMethods();
    }

    #region Awake Methods

    private void AwakeMethods()
    {
        SetObjectManger();
    }

    private void SetObjectManger()
    {
        ObjectManager.MapManager = this;
    }

    #endregion

    private void Start()
    {
        StartMethods();
    }

    #region StartMethods

    private void StartMethods()
    {
        GetGameData();
        GetDataBase();
        ArrangeMap();
    }


    private void GetGameData()
    {
        gameData = ObjectManager.GameData;
    }

    private void GetDataBase()
    {
        dataBase = ObjectManager.DataBase;
    }

    private void ArrangeMap()
    {
        ArrangeCells();
        ArrangeYard();
        ArrangeCafeteria();
        ArrangeAreaThree();
        SpawnGuardAtOpening();
    }

    private void SpawnGuardAtOpening()
    {
        for (int i = 0; i < dataBase.guardData.guardCountCurrent; i++)
        {
            SpawnGuard();
        }
    }

        #region Cell Methods

    private void ArrangeCells()
    {
        try
        {
            cells = ObjectManager.cells;
            cells = cells.OrderBy(c => c.prisonIndex).ToList();
        
        
        

            if (dataBase.cellData.cellInfos.Count < 3)
            {
                dataBase.cellData.cellInfos.Clear();
                for (int i = 0; i < 3; i++)
                {
                    dataBase.cellData.cellInfos.Add(1);
                    cells[i].ActivateCell(false);
                }
            }

            for (int i = 0; i < cells.Count; i++)
            {
                if (dataBase.cellData.cellInfos.Count <= i)
                {
                    dataBase.cellData.cellInfos.Add(0);
                    cells[i].DeActivateCell();
                }
                else
                {
                    if (dataBase.cellData.cellInfos[i] == 0)
                    {
                        cells[i].DeActivateCell();
                    }
                    else if (dataBase.cellData.cellInfos[i] == 1)
                    {
                        cells[i].ActivateCell(false);
                    }
                    else if (dataBase.cellData.cellInfos[i] == 2)
                    {
                        cells[i].ActivateCell(true);
                    }
                }
                // cells[i].ChangeCostText();
            }

            SetCellCosts();

            for (int i = 0; i < cells.Count; i++)
            {
                cells[i].ChangeCostText();
            }
            
            //DataManager.SaveData();
        }
        catch (Exception e)
        {
           //Console.WriteLine(e);
            throw;
        }

        

    }

    void SetCellCosts()
    {
        float tempValue;
        for (int i = 0; i < cells.Count; i++)
        {
            if (dataBase.cellData.cellCosts.Count <= i)
            {
                if (dataBase.cellData.cellInfos[i] != 0)
                {
                    dataBase.cellData.cellCosts.Add(0);
                    dataBase.cellData.cellCostDefault.Add(0);
                }
                else
                {
                    tempValue = (int) (costCurve.Evaluate(i) * 100);
                    tempValue = Mathf.CeilToInt(tempValue / 10) * 50;
                    dataBase.cellData.cellCosts.Add((int) tempValue);
                    dataBase.cellData.cellCostDefault.Add((int) tempValue);
                }
            }
        }
    }

        #endregion

        #region Yard Methods

        private void ArrangeYard()
        {
            if (dataBase.yardData.isYardActive)
            {
                ActivateYard();
            }
            else
            {
                yardMoneyField.SetActive(true);
                yard.SetActive(false);
                RefreshBuyingFieldText();
            }
        }

        public void ActivateYard(bool byPlayer)
        {
            ActivateYard();
            SpawnMoney(yardMoneySpawnPoint, 20);
            
            DataManager.SaveData(gameData);
            DataManager.SaveData(dataBase);
            EventManager.BuyAction();
        }

        public void ActivateYard()
        {
            dataBase.yardData.isYardActive = true;
            yard.SetActive(true);
            yardMoneyField.SetActive(false);
            CheckYardFieldActivasionYard();
        }

        public void RefreshBuyingFieldText()
        {
            string costText = "";
            if (dataBase.yardData.yardCost >= 1000)
            {
                costText = (dataBase.yardData.yardCost / 1000).ToString() + "." +
                         ((dataBase.yardData.yardCost % 1000) / 100).ToString() + " K";
            }
            else
            {
                costText = dataBase.yardData.yardCost.ToString();
            }
            textCostBuyField.text = costText;
            imageBuyField.fillAmount = Mathf.InverseLerp(0, dataBase.yardData.defaultYardCost, dataBase.yardData.defaultYardCost);
        }

        private void CheckYardFieldActivasionYard()
        {
            for (int i = 0; i < yardUpgradeFields.Count; i++)
            {
                if (dataBase.yardData.infoList[i] == 0)
                {
                    yardUpgradeFields[i].SetActive(false);
                }
                else
                {
                    yardUpgradeFields[i].SetActive(true);
                }
            }
        }

        public void ActivateYardField(int index)
        {
            dataBase.yardData.infoList[index] = 1;
            yardUpgradeFields[index].SetActive(true);
            DataManager.SaveData(dataBase);
            DataManager.SaveData(gameData);
            
            EventManager.BuyAction();
        }
    
        #endregion
        
        #region Cafeteria

        private void ArrangeCafeteria()
        {
            if (dataBase.cafeteriaData.isCafeteriaActive)
            {
                ActivateCafeteria();
            }
            else
            {
                cafeteriaMoneyField.SetActive(true);
                cafeteria.SetActive(false);
                RefreshBuyingFieldTextCafeteria();
            }
        }

        public void ActivateCafeteria(bool byPlayer)
        {
            ActivateCafeteria();
            SpawnMoney(cafeteriaMoneySpawnPoint, 40);
            
            DataManager.SaveData(gameData);
            DataManager.SaveData(dataBase);
            EventManager.BuyAction();
        }

        public void ActivateCafeteria()
        {
            dataBase.cafeteriaData.isCafeteriaActive = true;
            cafeteria.SetActive(true);
            cafeteriaMoneyField.SetActive(false);
            CheckFieldActivationCafeteria();
        }
        public void ActivateCafeteriaField(int index)
        {
            dataBase.cafeteriaData.infoList[index] = 1;
            cafeteriaUpgradeFiels[index].SetActive(true);
            DataManager.SaveData(dataBase);
            DataManager.SaveData(gameData);
            EventManager.BuyAction();
        }

        private void CheckFieldActivationCafeteria()
        {
            for (int i = 0; i < cafeteriaUpgradeFiels.Count; i++)
            {
                if (dataBase.cafeteriaData.infoList[i] == 0)
                {
                    cafeteriaUpgradeFiels[i].SetActive(false);
                }
                else
                {
                    cafeteriaUpgradeFiels[i].SetActive(true);
                }
            }
        }

        public void RefreshBuyingFieldTextCafeteria()
        {
            string costText = "";
            int cost = dataBase.cafeteriaData.cafeteriaCost;
            if (cost >= 1000)
            {
                costText = (cost / 1000).ToString() + "." +
                           ((cost % 1000) / 100).ToString() + " K";
            }
            else
            {
                costText = cost.ToString();
            }
            textCostCostBuyFieldCafeteria.text = costText;
            imageBuyFiledCafeteria.fillAmount = Mathf.InverseLerp(0, dataBase.cafeteriaData.defaultCafeteriaCost, cost);
        }
        #endregion

        private void ArrangeAreaThree()
        {
            if (dataBase.area3.isActive)
            {
                ActivateAreaThree();
            }
            else
            {
                DeactivateAreaThree();
            }
        }

        public void ActivateAreaThree(bool byplayer)
        {
            ActivateAreaThree();
            SpawnMoney(area3MoneySpawnPoint, 60);
            EventManager.BuyAction();
        }

        public void ActivateAreaThree()
        {
            area3MoneyField.SetActive(false);
            area3.SetActive(true);
            dataBase.area3.isActive = true; 
            DataManager.SaveData(gameData);
            DataManager.SaveData(dataBase);
        }

        private void DeactivateAreaThree()
        {
            area3.SetActive(false);
            area3MoneyField.SetActive(true);
        }

    #endregion

    public void AddField(ActionFields.FieldType fieldType, ActionFields sender)
    {
        if (fieldType == ActionFields.FieldType.cafeteriaField)
        {
            fieldCountCafeteria++;
            fieldsCafeteria.Add(sender);
        }
        else if(fieldType == ActionFields.FieldType.yardField)
        {
            fieldCountYard++;
            fieldsYard.Add(sender);
        }
    }

    public ActionFields PickActionField(PrisonerController.PrisonerStage prisonerStage)
    {
        ActionFields targetField;
        if (prisonerStage == PrisonerController.PrisonerStage.MovingToCafeteria)
        {
            targetField = fieldsCafeteria[Random.Range(0, fieldsCafeteria.Count)];
            fieldsCafeteria.Remove(targetField);
            return targetField;
        }
        else if (prisonerStage == PrisonerController.PrisonerStage.MovingToYard)
        {
            targetField =  fieldsYard[Random.Range(0, fieldsYard.Count)];
            fieldsYard.Remove(targetField);
            return targetField;
        }
        else
        {
            Debug.LogWarning("Pick Action Field Error!");
            return null;
        }
    }

    public void RemovePrisoner(PrisonerController prisoner)
    {
        if (prisonerList.Contains(prisoner))
        {
            prisonerList.Remove(prisoner);
        }
    }

    public void AddPrisoner(PrisonerController prisoner)
    {
        if (!prisonerList.Contains(prisoner))
        {
            prisonerList.Add(prisoner);
        }
    }

    public void AddResource(Resource resource)
    {
        if (resource.resourceType == Resource.ResourceType.cloth)
        {
            resourceCloth = resource;
        }
        else if (resource.resourceType == Resource.ResourceType.food)
        {
            resourceFood = resource;
        }
    }
    public void SpawnGuard()
    {
        Instantiate(gameData.guardPrefab, guardSpawnPos.position, Quaternion.identity);
    }

    public void ActivateArrowCloth()
    {
        arrowCloth.SetActive(true);
    }

    public void TryDeActivateArrowCloth()
    {
        bool conditon = true;
        for (int i = 0; i < prisonerList.Count; i++)
        {
            if (prisonerList[i].prisonerStage == PrisonerController.PrisonerStage.WaitingForCloths)
            {
                conditon = false;
                break;
            }
        }

        if (conditon)
        {
            arrowCloth.SetActive(false);
        }
    }

    public void SpawnMoney(Transform targetPoint, int amount)
    {
        Vector3 targetPos = targetPoint.position;
        for (int i = 0; i < amount ; i++)
        {
            //targetPos = targetPos + Vector3.right * Random.Range(-1.5f, 1.5f) + Vector3.forward * Random.Range(-1.5f, 1.5f);
            targetPos = targetPoint.position + Vector3.right * (i %2) + Vector3.forward * (i%2)  +Vector3.up * i *0.5f;
            Instantiate(gameData.moneyPrefab, targetPos, Quaternion.identity);
        }
    }
}