using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [Header("Nav Mesh Agent")] 
    //
    [SerializeField] private NavMeshAgent myAgent;
    
    #region Movement Variables
    
    [Header("Movement Variables")]
    private FloatingJoystick joystick;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private Vector3 direction;
    [SerializeField] private LayerMask layerMask;
    private Ray myRay;
    private RaycastHit hitInfo;
    
    #endregion

    [Header("Animator")]
    //
    [SerializeField] private Animator anim;
    
    private int prisonerCount;
    public List<PrisonerController> queuePrisoners = new List<PrisonerController>();
    private PrisonerController tempPrisoner;

    private Cell tempCell;
    private PrisonerController cellPrisoner;
    
    #region Resource Variables

    [Header("Stack Settings")]
    //
    [SerializeField] private GameObject StackObject;
    [SerializeField] private List<GameObject> resourceModelsFood = new List<GameObject>();
    [SerializeField] private List<GameObject> resourceModelsCloth = new List<GameObject>();
    [SerializeField] private GameObject jobObject;
    
    private int stackCount;
    [SerializeField]
    private List<Resource> resources = new List<Resource>();
    [SerializeField]
    private Resource targetResourceField;

    [SerializeField]
    private int resourceFoodCount;
    [SerializeField]
    private int resourceClothCount;
    
    #endregion

    private bool isInYardBuyField;
    private bool isInCafeteriaBuyField;
    private BuyField currentBuyField;
    private FoodDeliveryPoint currentFoodDeliveryPoint;
    private bool isBuyPanelActive = false;
    private bool isInHrField = false;

    private bool isTutorialCutSceneActive;

    [Header("UI Settings")]
    //
    [SerializeField] private Image uiCircle;
    [SerializeField] private Image uiCircleOutline;
    
    
    private GameData gameData;
    private DataBase dataBase;
    private GameManager gameManager;
    private MapManager mapManger;

    
    #region OnEnable & OnDisable

    private void OnEnable()
    {
        ObjectManager.Player = gameObject;
        ObjectManager.PlayerManager = this;
    }

    private void OnDisable()
    {
        
    }

    #endregion
    
    private void Awake()
    {
        AwakeMethods();
    }
    
    #region AwakeMethods

    private void AwakeMethods()
    {
        
    }

    #endregion

    // Start is called before the first frame update
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
        GetJoystick();
        GetMapManager();
        SetSpeed();
        SetCamerasTarget();
        SetFirstCoin();
        GetUICircle();
    }

    private void GetGameManager()
    {
        gameManager = ObjectManager.GameManager;
    }

    private void GetGameData()
    {
        gameData = ObjectManager.GameData;
    }

    private void GetDataBase()
    {
        dataBase = ObjectManager.DataBase;
    }

    private void SetSpeed()
    {
        myAgent.speed = dataBase.player.speed;
    }

    private void GetJoystick()
    {
        joystick = ObjectManager.CanvasManager.joystick;
    }

    private void SetCamerasTarget()
    {
        EventManager.Target(gameObject);
    }

    private void SetFirstCoin()
    {
        EventManager.Coin = gameData.Coin;
    }

    private void GetMapManager()
    {
        mapManger = ObjectManager.MapManager;
    }

    private void GetUICircle()
    {
        uiCircle = ObjectManager.CanvasManager.PlayerCircle;
        uiCircleOutline = ObjectManager.CanvasManager.PlayerCircleOutline;
    }
    

    #endregion

    // Update is called once per frame
    void Update()
    {
        if (gameManager.gameState == GameManager.GameStates.Start && !isBuyPanelActive)
        {
            Movement();
            
            //canvas.transform.LookAt(ObjectManager.CameraManager.transform.position + Vector3.up);
            uiCircleOutline.transform.position = Camera.main.WorldToScreenPoint(transform.position + Vector3.up * 2);
        }
    }
    
    #region Movement

    private void Movement()
    {
        if (!isTutorialCutSceneActive)
        {
            GetDirection();
            SetTargetPosition();
            SetDestination();
            SetAcceleration();
        }
    }

    private void GetDirection()
    {
        direction.y = 0;
        direction.x = joystick.Horizontal;
        direction.z = joystick.Vertical;

        if (direction == Vector3.zero)
        {
            //anim.SetBool("Run", false);
            AnimIdle();
        }
        else
        {
            //anim.SetBool("Run", true);
            AnimWalk();
        }

        Vector3 lookPos = transform.position;
        lookPos += direction * 1;
        transform.LookAt(lookPos);
    }

    private void SetTargetPosition()
    {
        CheckDistance();
        //targetPosition = transform.position + direction * dataBase.player.speed ;
    }

    private void SetDestination()
    {
        myAgent.SetDestination(targetPosition);
        //transform.position = targetPosition;
    }

    private void CheckDistance()
    {
        if (Physics.Raycast(transform.position, direction, out hitInfo, dataBase.player.speed, layerMask))
        {
            Debug.DrawRay(transform.position, direction * hitInfo.distance, Color.red);
            targetPosition = transform.position + direction * (hitInfo.distance - 0.25f);
        }
        else
        {
            targetPosition = transform.position + direction * dataBase.player.speed ;
        }
    }
    
    void SetAcceleration()
    {
        if (myAgent.hasPath)
        {
            Vector3 toTarget = myAgent.steeringTarget - transform.position;
            float turnAngle = Vector3.Angle(transform.forward, toTarget);
            myAgent.acceleration = turnAngle * myAgent.speed;
        }
    }

    public void ChangeTutorialSceneStatus(bool status)
    {
        isTutorialCutSceneActive = status;
    }

    public void RemovePrisonerFromQueue(PrisonerController prisoner)
    
    {
        if (queuePrisoners.Contains(prisoner))
        {
            queuePrisoners.Remove(prisoner);
        }
    }

    #endregion

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(dataBase.tags.prisoner))
        {
            tempPrisoner = other.GetComponent<PrisonerController>();
            AddPrisonerToQueue();
           // GiveResourceToPrisoner();
        }
        else if (other.CompareTag(dataBase.tags.resouceField))
        {
            targetResourceField = other.gameObject.GetComponent<Resource>();
            //GetResource();
            StartCoroutine(DelayedGetResource());
        }
        else if (other.CompareTag(dataBase.tags.cell))
        {
            tempCell = other.transform.parent.GetComponent<Cell>();
            TryToPickPrisoner(other.transform.parent.GetComponent<Cell>());
            TryToDeliveryCloth(other.transform.parent.GetComponent<Cell>());
            TryToBuyCell();

        }
        else if (other.CompareTag(dataBase.tags.cellInside))
        {
            tempCell = other.transform.parent.GetComponent<Cell>();
            tempCell.IsPlayerInside(true);
        }
        else if (other.CompareTag(dataBase.tags.yard))
        {
            isInYardBuyField = true;
            StartCoroutine(TryToBuyYard());
        }
        else if (other.CompareTag(dataBase.tags.cafeteria))
        {
            isInCafeteriaBuyField = true;
            StartCoroutine(TryToBuyCafeteria());
        }
        else if (other.CompareTag(dataBase.tags.buyField))
        {
            currentBuyField = other.gameObject.GetComponent<BuyField>();
            StartCoroutine(TryToBuyField(other.gameObject.GetComponent<BuyField>()));
        }
        else if (other.CompareTag(dataBase.tags.foodDeliveryPoint))
        {
            if (!other.GetComponent<FoodDeliveryPoint>().isUsing)
            {
                other.GetComponent<FoodDeliveryPoint>().SetUsingCondition(true);
                currentFoodDeliveryPoint = other.gameObject.GetComponent<FoodDeliveryPoint>();
                StartCoroutine(TryToDeliveryFood(other.gameObject.GetComponent<FoodDeliveryPoint>()));
            }
        }
        else if (other.CompareTag(dataBase.tags.money))
        {
            other.gameObject.SetActive(false);
            IncreaseMoney(1);
        }
        else if (other.CompareTag(dataBase.tags.hr))
        {
            isInHrField = true;
            StartCoroutine(DelayedShowBuyPanel());
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(dataBase.tags.resouceField))
        {
            targetResourceField = null;
        }
        else if (other.CompareTag(dataBase.tags.cell))
        {
            if (tempCell == other.transform.parent.GetComponent<Cell>())
            {
                tempCell = null;
            }
        }
        else if (other.CompareTag(dataBase.tags.cellInside))
        {
            other.transform.parent.GetComponent<Cell>().IsPlayerInside(false);
        }
        else if (other.CompareTag(dataBase.tags.yard))
        {
            isInYardBuyField = false;
        }
        else if (other.CompareTag((dataBase.tags.buyField)))
        {
            currentBuyField = null;
        }
        else if (other.CompareTag(dataBase.tags.cafeteria))
        {
            isInCafeteriaBuyField = false;
        }
        else if (other.CompareTag(dataBase.tags.foodDeliveryPoint))
        {
            if (currentFoodDeliveryPoint == other.GetComponent<FoodDeliveryPoint>())
            {
                other.GetComponent<FoodDeliveryPoint>().SetUsingCondition(false);
                currentFoodDeliveryPoint = null;
            }
        }
        else if (other.CompareTag(dataBase.tags.hr))
        {
            isInHrField = false;
        }
    }

    IEnumerator DelayedShowBuyPanel()
    {
        yield return new WaitForSeconds(0.1f);
        bool condition = true;
        for (float i = 0; i < 1; i += 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
            if (!isInHrField)
            {
                condition = false;
                break;
            }
        }
        if (condition)
        {
            ObjectManager.CanvasManager.ShowBuyPanel();
        }
    }

    void AddPrisonerToQueue()
    {
        if (prisonerCount <= dataBase.player.prisonerStackCount && tempPrisoner.movingType == PrisonerController.MovingType.isNeedToMoveWithEscort)
        {
            queuePrisoners.Add(tempPrisoner);
            tempPrisoner.UnMarkPrisonner();
            tempPrisoner.FollowGuard(this);
        }
    }

    void TryToBuyCell()
    {
        if (tempCell != null && !tempCell.isCellActive)
        {
            if (dataBase.cellData.cellCosts[tempCell.prisonIndex] > 0 && gameData.Coin > 0)
            {
                StartCoroutine(DelayedBuyingCell(tempCell));
            } 
        }
    }

    IEnumerator DelayedBuyingCell(Cell targetCell)
    {

        while (tempCell != null && tempCell == targetCell && !tempCell.isCellActive && dataBase.cellData.cellCosts[tempCell.prisonIndex] > 0 && gameData.Coin > 0)
        {
            if (gameData.Coin < 10)
            {
                dataBase.cellData.cellCosts[tempCell.prisonIndex] -= gameData.Coin;
                DecreaseMoney(gameData.Coin);
                targetCell.ChangeCostText();
                break;
            }
            else
            {
                if (dataBase.cellData.cellCosts[tempCell.prisonIndex] <= 10)
                {
                    DecreaseMoney( dataBase.cellData.cellCosts[tempCell.prisonIndex]);
                    dataBase.cellData.cellCosts[tempCell.prisonIndex] = 0;
                    dataBase.cellData.cellInfos[tempCell.prisonIndex] = 1;
                    targetCell.ChangeCostText();
                    targetCell.ActivateCell(false, true);
                    break;
                }
                else
                {
                    DecreaseMoney(10);
                    dataBase.cellData.cellCosts[tempCell.prisonIndex] -= 10;
                    targetCell.ChangeCostText();
                }
            }

            if (dataBase.cellData.cellCostDefault[tempCell.prisonIndex] > 1000)
            {
                yield return new WaitForSeconds(0.005f);
            }
            else
            {
                yield return new WaitForSeconds(0.025f);
            }

        }
        DataManager.SaveData(gameData);
        DataManager.SaveData(dataBase);

        yield return new WaitForSeconds(0f);
    }

    private IEnumerator TryToBuyYard()
    {
        while (isInYardBuyField)
        {
            if (!dataBase.yardData.isYardActive)
            {
                if (gameData.Coin < 10)
                {
                    dataBase.yardData.yardCost -= gameData.Coin;
                    DecreaseMoney(gameData.Coin);
                    mapManger.RefreshBuyingFieldText();
                    break;
                }
                else
                {
                    if (dataBase.yardData.yardCost <= 10)
                    {
                        DecreaseMoney( dataBase.yardData.yardCost);
                        dataBase.yardData.yardCost = 0;
                        dataBase.yardData.isYardActive = true;
                        mapManger.RefreshBuyingFieldText();
                        mapManger.ActivateYard(true);
                        break;
                    }
                    else
                    {
                        DecreaseMoney(10);
                        dataBase.yardData.yardCost -= 10;
                        mapManger.RefreshBuyingFieldText();
                    }
                }
            }

            if (dataBase.yardData.defaultYardCost > 1000)
            {
                yield return new WaitForSeconds(0.005f);
            }
            else
            {
                yield return new WaitForSeconds(0.025f);
            }
        }
        
        
        DataManager.SaveData(gameData);
        DataManager.SaveData(dataBase);
        yield return new WaitForSeconds(0.1f);
    }

    private IEnumerator TryToBuyCafeteria()
    {
        while (isInCafeteriaBuyField)
        {
            if (!dataBase.cafeteriaData.isCafeteriaActive)
            {
                if (gameData.Coin < 10)
                {
                    dataBase.cafeteriaData.cafeteriaCost -= gameData.Coin;
                    DecreaseMoney(gameData.Coin);
                    mapManger.RefreshBuyingFieldTextCafeteria();
                    break;
                }
                else
                {
                    if (dataBase.cafeteriaData.cafeteriaCost <= 10)
                    {
                        DecreaseMoney( dataBase.cafeteriaData.cafeteriaCost);
                        dataBase.cafeteriaData.cafeteriaCost = 0;
                        dataBase.cafeteriaData.isCafeteriaActive = true;
                        mapManger.RefreshBuyingFieldTextCafeteria();
                        mapManger.ActivateCafeteria(true);
                        break;
                    }
                    else
                    {
                        DecreaseMoney(10);
                        dataBase.cafeteriaData.cafeteriaCost -= 10;
                        mapManger.RefreshBuyingFieldTextCafeteria();
                    }
                }
            }

            if (dataBase.cafeteriaData.defaultCafeteriaCost> 1000)
            {
                yield return new WaitForSeconds(0.005f);
            }
            else
            {
                yield return new WaitForSeconds(0.025f);
            }
        }
        
        
        DataManager.SaveData(gameData);
        DataManager.SaveData(dataBase);
        yield return new WaitForSeconds(0.1f);
    }
    
    private IEnumerator TryToBuyField(BuyField targetBuyField)
    {
        bool isBuyActionComplete = false;
        
        yield return new WaitForSeconds(1);
        
        int defaultCost = 0;

        if (targetBuyField.areaType == BuyField.AreaType.Yard)
        {
            defaultCost = dataBase.yardData.defaultUpgradeCost[targetBuyField.index];
        }
        else if (targetBuyField.areaType == BuyField.AreaType.Cafeteria)
        {
            defaultCost = dataBase.cafeteriaData.defaultUpgradeCost[targetBuyField.index];
        }
        else if (targetBuyField.areaType == BuyField.AreaType.Area3)
        {
            defaultCost = dataBase.area3.defaultCost;
        }
        
        
        while (currentBuyField == targetBuyField)
        {
            if (targetBuyField.areaType == BuyField.AreaType.Yard)
            {
                if (gameData.Coin < 10)
                {
                    dataBase.yardData.upgradeCost[targetBuyField.index] -= gameData.Coin;
                    DecreaseMoney(gameData.Coin);
                    targetBuyField.ChangeCostTextNImage();
                    break;
                }
                else
                {
                    if (dataBase.yardData.upgradeCost[targetBuyField.index] <= 10)
                    {
                        DecreaseMoney( dataBase.yardData.upgradeCost[targetBuyField.index]);
                        dataBase.yardData.upgradeCost[targetBuyField.index] = 0;
                        dataBase.yardData.isYardActive = true;
                        targetBuyField.ChangeCostTextNImage();
                        mapManger.ActivateYardField(targetBuyField.index);
                        targetBuyField.DeActivate();
                        isBuyActionComplete = true;
                        break;
                    }
                    else
                    {
                        DecreaseMoney(10);
                        dataBase.yardData.upgradeCost[targetBuyField.index] -= 10;
                        targetBuyField.ChangeCostTextNImage();
                    }
                }
            }
            else if (targetBuyField.areaType == BuyField.AreaType.Cafeteria)
            {
                if (gameData.Coin < 10)
                {
                    dataBase.cafeteriaData.upgradeCost[targetBuyField.index] -= gameData.Coin;
                    DecreaseMoney(gameData.Coin);
                    targetBuyField.ChangeCostTextNImage();
                    break;
                }
                else
                {
                    if (dataBase.cafeteriaData.upgradeCost[targetBuyField.index] <= 10)
                    {
                        DecreaseMoney( dataBase.yardData.yardCost);
                        dataBase.cafeteriaData.upgradeCost[targetBuyField.index]= 0;
                        dataBase.cafeteriaData.infoList[targetBuyField.index] = 1;
                        targetBuyField.ChangeCostTextNImage();
                        mapManger.ActivateCafeteriaField(targetBuyField.index);
                            targetBuyField.DeActivate();
                            isBuyActionComplete = true;
                        break;
                    }
                    else
                    {
                        DecreaseMoney(10);
                        dataBase.cafeteriaData.upgradeCost[targetBuyField.index] -= 10;
                        targetBuyField.ChangeCostTextNImage();
                    }
                }
            }
            else if (targetBuyField.areaType == BuyField.AreaType.Area3)
            {
                if (gameData.Coin < 10)
                {
                    dataBase.area3.cost -= gameData.Coin;
                    DecreaseMoney(gameData.Coin);
                    targetBuyField.ChangeCostTextNImage();
                    break;
                }
                else
                {
                    if (dataBase.area3.cost <= 10)
                    {
                        DecreaseMoney( dataBase.yardData.yardCost);
                        dataBase.area3.cost= 0;
                        dataBase.area3.isActive = true;
                        targetBuyField.ChangeCostTextNImage();
                        //mapManger.ActivateCafeteriaField(targetBuyField.index);
                        mapManger.ActivateAreaThree(true);
                        targetBuyField.DeActivate();
                        break;
                    }
                    else
                    {
                        DecreaseMoney(10);
                        dataBase.area3.cost -= 10;
                        targetBuyField.ChangeCostTextNImage();
                    }
                }
            }
            if (defaultCost > 1000)
            {
                yield return new WaitForSeconds(0.005f);
            }
            else
            {
                
                yield return new WaitForSeconds(0.025f);
            }
        }
        DataManager.SaveData(gameData);
        DataManager.SaveData(dataBase);
        yield return new WaitForSeconds(0.1f);
    }

    void DecreaseMoney(int value)
    {
        gameData.Coin -= value;
        EventManager.Coin = gameData.Coin;
    }

    void IncreaseMoney(int value)
    {
        gameData.Coin += value * 50;
        EventManager.Coin = gameData.Coin;
    }
    
    #region ResourceInteraction

    private IEnumerator DelayedGetResource()
    {
        yield return new WaitForEndOfFrame();
        while (targetResourceField != null )
        {
            GetResource();
            yield return new WaitForSeconds(0.2f);
        }
    }

    private void GetResource()
    {
        if (stackCount < dataBase.player.objectStackCount && targetResourceField.resourceCount > 0)
        {
            targetResourceField.DecreaseResourceCount();
            stackCount++;
            if (!StackObject.activeInHierarchy)
            {
                StackObject.SetActive(true);
            }
            resources.Add(targetResourceField);
            if (targetResourceField.resourceType == Resource.ResourceType.cloth)
            {
                resourceClothCount++;
            }
            else if (targetResourceField.resourceType == Resource.ResourceType.food)
            {
                resourceFoodCount++;
            }
            ArrangeResourceModels();
        }
        else if (stackCount >= dataBase.player.objectStackCount && targetResourceField.resourceCount > 0)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                if (resources[i].resourceType != targetResourceField.resourceType)
                {
                    resources[i] = targetResourceField;
                    if (targetResourceField.resourceType == Resource.ResourceType.cloth)
                    {
                        resourceClothCount++;
                        resourceFoodCount--;
                    }
                    else if (targetResourceField.resourceType == Resource.ResourceType.food)
                    {
                        resourceFoodCount++;
                        resourceClothCount--;
                    }
                    ArrangeResourceModels();
                    break;
                }
            }
        }
    }

    private void ArrangeResourceModels()
    {
        for (int i = 0; i < dataBase.player.objectStackCount; i++)
        {
            if (i < resources.Count)
            {
                if (resources[i].resourceType == Resource.ResourceType.cloth)
                {
                    resourceModelsCloth[i].SetActive(true);
                    resourceModelsFood[i].SetActive(false);
                }
                else if (resources[i].resourceType == Resource.ResourceType.food)
                {
                    resourceModelsCloth[i].SetActive(false);
                    resourceModelsFood[i].SetActive(true);
                }
            }
            else
            {
                resourceModelsCloth[i].SetActive(false);
                resourceModelsFood[i].SetActive(false);
            }
        }

        if (stackCount > 0)
        {
            AnimChangeLayerWeight(1);
        }
        else
        {
            AnimChangeLayerWeight(0);
        }
    }
    
    
    private void GiveResourceToPrisoner()
    {
        if (tempPrisoner.prisonerStage == PrisonerController.PrisonerStage.WaitingForCloths)
        {
           // GiveCloth();
        }
        else if (tempPrisoner.prisonerStage == PrisonerController.PrisonerStage.WaitingForFoodAtCafeteria)
        {
            GiveFood();
        }
    }
    private void TryToDeliveryCloth(Cell targetCell)
    {
        if (resourceClothCount > 0 && targetCell.currentPrisoner != null && targetCell.currentPrisoner.prisonerStage == PrisonerController.PrisonerStage.WaitingForCloths)
        {
            StartCoroutine(DelayedClothDelivery(targetCell));
        }
    }

    IEnumerator DelayedClothDelivery(Cell targetCell)
    {
        bool condition = true;
        for (float i = 0; i < 1; i+=0.01f)
        {
            if (tempCell == targetCell)
            {
                targetCell.DecreaseFillAmount(0.01f);
                UICircleIncreaseFillAmount(0.01f);
                yield return new WaitForSeconds(0.01f);
            }
            else
            {
                targetCell.ResetFillAmount();
                UICircleDisable();
                condition = false;
                break;
            }
        }

        if (condition)
        {
            GiveCloth(targetCell.currentPrisoner);
            targetCell.ResetFillAmount();
            UICircleDisable();
        }
    }

    private void GiveFood()
    {
        if (resourceFoodCount > 0)
        {
            resourceFoodCount--;
            RemoveResource(Resource.ResourceType.food);
        }
    }

    private void GiveCloth(PrisonerController targetPrisoner)
    {
        if (resourceClothCount > 0)
        {
            resourceClothCount--;
            RemoveResource(Resource.ResourceType.cloth);
            targetPrisoner.WearNewClothes();
            targetPrisoner.UnMarkPrisonner();
        }
    }

    private void RemoveResource(Resource.ResourceType targetResourceType)
    {
        for (int i = resources.Count - 1; i >= 0; i--)
        {
            if (resources[i].resourceType == targetResourceType)
            {
                resources.RemoveAt(i);
                stackCount--;
                break;
            }
        }

        if (stackCount <= 0)
        {
            StackObject.SetActive(false);
        }
        ArrangeResourceModels();
    }

    IEnumerator TryToDeliveryFood(FoodDeliveryPoint foodDeliveryPoint)
    {
        yield return new WaitForSeconds(0.1f);
        while (currentFoodDeliveryPoint == foodDeliveryPoint)
        {
            if (currentFoodDeliveryPoint.hungryPrisoners.Count > 0 && resourceFoodCount > 0 && currentFoodDeliveryPoint == foodDeliveryPoint)
            {
                bool condition = true;
                for (float i = 0; i < 1; i += 0.01f)
                {
                    yield return new WaitForSeconds(0.01f);
                    if (currentFoodDeliveryPoint == foodDeliveryPoint && currentFoodDeliveryPoint.hungryPrisoners.Count > 0)
                    {
                        foodDeliveryPoint.DecreaseFillAmount(0.01f);
                        UICircleIncreaseFillAmount(0.01f);
                    }
                    else
                    {
                        foodDeliveryPoint.ResetFillAmount();
                        UICircleDisable();
                        condition = false;
                        break;
                    }
                }
                foodDeliveryPoint.ResetFillAmount();
                if (condition)
                {
                    foodDeliveryPoint.hungryPrisoners[0].UnMarkPrisonner();
                    foodDeliveryPoint.hungryPrisoners[0].GetFood();
                    GiveFood();
                    UICircleDisable();
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    #endregion

    private void TryToPickPrisoner(Cell targetCell)
    {
        if (targetCell.currentPrisoner != null)
        {
            cellPrisoner = targetCell.currentPrisoner;
            if ((cellPrisoner.prisonerStage == PrisonerController.PrisonerStage.WaitingToMoveCafeteria ||
                cellPrisoner.prisonerStage == PrisonerController.PrisonerStage.WaitingToMoveYard) && prisonerCount <= dataBase.player.prisonerStackCount)
            {
                StartCoroutine(TryToAddPrisonerToQueeFromCell(targetCell));
            }
        }
    }

    IEnumerator TryToAddPrisonerToQueeFromCell(Cell targetCell)
    {
        bool condition = true;
        for (float i = 0; i < 1; i+=0.01f)
        {
            if (tempCell == targetCell)
            {
                targetCell.DecreaseFillAmount(0.01f);
                UICircleIncreaseFillAmount(0.01f);
                yield return new WaitForSeconds(0.01f);
            }
            else
            {
                targetCell.ResetFillAmount();
                UICircleDisable();
                condition = false;
                break;
            }
        }

        if (condition && tempCell == targetCell)
        {
            if (targetCell.currentPrisoner != null)
            {
                queuePrisoners.Add(targetCell.currentPrisoner);
                targetCell.currentPrisoner.UnMarkPrisonner();
                targetCell.currentPrisoner.FollowGuard(this);
                targetCell.ResetCellPrisoner();
                UICircleDisable();
            }
        }
    }

    public void ChangeBuyPanelStatus(bool isActive)
    {
        isBuyPanelActive = isActive;
    }
    
    #region Animations

    private void AnimWalk()
    {
        anim.SetBool("Run", true);
    }

    private void AnimIdle()
    {
        anim.SetBool("Run", false);
    }

    private void AnimChangeLayerWeight(float value)
    {
        anim.SetLayerWeight(1, value);
        if (value == 1)
        {
            jobObject.SetActive(false);
        }
        else
        {
            jobObject.SetActive(true);
        }
    }
    
    #endregion
    
    #region UI

    private void UICircleIncreaseFillAmount(float value)
    {
        uiCircleOutline.gameObject.SetActive(true);
        uiCircle.fillAmount += value;
    }

    private void UICircleDisable()
    {
        uiCircleOutline.gameObject.SetActive(false);
        uiCircle.fillAmount = 0;
    }
    
    #endregion
}
