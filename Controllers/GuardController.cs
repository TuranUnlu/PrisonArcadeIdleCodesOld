using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.AI;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;

public class GuardController : MonoBehaviour
{
    public enum State
    {
        idle, 
        pickPrisonersToMoveCell,
        pickPrisonersToMoveCafeteria,
        pickPrisonersToMoveYard,
        pickClothsToDelivery,
        pickFoodsToDelivery,
        deliveringCloth,
        deliveringFood,
        movingToYard,
        movingToCafeteria,
        movingToCells
    }

    public State state;

    private PrisonerController.PrisonerStage targetPrisonerStage;
    private State targetState;


    [Header("NavMesh Agent")]
    //
    [SerializeField] private NavMeshAgent myAgent;


    [Header("Animator")]
    // 
    [SerializeField] private Animator anim;
    
    #region Stack Variables

    [Header("Stack Variables")]
    //
    [SerializeField] private GameObject stackObject;
    [SerializeField] List<GameObject> meshesCloth = new List<GameObject>();
    [SerializeField] List<GameObject> meshesFood  = new List<GameObject>();
    private List<Resource> resources = new List<Resource>();
    private Resource targetResourceField;
    [SerializeField]
    private int resourceCountFood;
    [SerializeField]
    private int resourceCountCloth;
    [SerializeField]
    private int stackCount;

    private FoodDeliveryPoint currentFoodDeliveryPoint;

    #endregion

    private List<PrisonerController> prisonerList = new List<PrisonerController>();
    [SerializeField]
    private List<PrisonerController> targetPrisoners = new List<PrisonerController>();
    [SerializeField]
    private int randomActionIndex;
    private bool pickCondition;
    [SerializeField]
    public List<PrisonerController> currentPrisoners = new List<PrisonerController>();
    [SerializeField]
    private List<Cell> cells = new List<Cell>();
    [SerializeField]
    private List<Cell> targetCells = new List<Cell>();
    private PrisonerController tempPrisoner;
    [SerializeField]
    private PrisonerController currentTargetPrisoner;
    private Cell tempCell;

    [Header("DeBug")]
    //
    [SerializeField] private Transform target;


    private DataBase dataBase;
    private GameData gameData;
    private PlayerController playerManager;
    private MapManager mapManager;


    //                                  ************ AWAKE *******************
    private void Awake()
    {
        AwakeMethods();
    }
    
    #region AwakeMethods    **********************************

    private void AwakeMethods()
    {
        
    }
    
    #endregion

    // Start is called before the first frame update START *******************
    void Start()
    {
        StartMethods();
    }
    
    #region Start Methods       **************************************

    private void StartMethods()
    {
        GetGameData();
        GetDataBase();
        GetPlayerController();
        GetMapManager();
        GetMapMangerPrisonerList();
        SetFirstDestination();
        //GetMapManagerCells();
        Invoke("GetMapManagerCells", 2);
        ChangeAgentSpeed();
        StartCoroutine(PickActionRoutine());
    }

    private void GetGameData()
    {
        gameData = ObjectManager.GameData;
    }

    private void GetDataBase()
    {
        dataBase = ObjectManager.DataBase;
    }

    private void GetPlayerController()
    {
        playerManager = ObjectManager.PlayerManager;
    }

    private void GetMapManager()
    {
        mapManager = ObjectManager.MapManager;
    }

    private void GetMapMangerPrisonerList()
    {
        prisonerList = mapManager.prisonerList;
    }

    private void GetMapManagerCells()
    {
        cells = mapManager.cells;
    }

    private void SetFirstDestination()
    {
        SetDestination(transform);
    }
    
    #endregion

    // Update is called once per frame  ************ UPDATE ******************
    void Update()
    {
        if (myAgent.hasPath)
        {
            if (myAgent.remainingDistance < 0.5f)
            {
                AnimIdle();
            }
            else
            {
                AnimWalking();
            }
        }
    }

    #region On TRIGGER ENTER & EXIT    ****************************************
    
    //                                 ******** ON TRIGGER ENTER **************
    private void OnTriggerEnter(Collider other)
    {
        if(dataBase == null)
        {
            GetDataBase();
        }
        if (other.CompareTag(dataBase.tags.prisoner))
        {
            tempPrisoner = other.GetComponent<PrisonerController>();
            if (state == State.pickPrisonersToMoveCell)
            {
                if (targetPrisoners.Contains(tempPrisoner))
                {
                    if (!currentPrisoners.Contains(tempPrisoner))
                    {
                        currentPrisoners.Add(tempPrisoner);
                        tempPrisoner.FollowGuard(this);
                        if (tempPrisoner == currentTargetPrisoner)
                        {
                            currentTargetPrisoner = null;
                        }
                    }
                }
            }
            
        }
        else if (other.CompareTag(dataBase.tags.cell))
        {
            tempCell = other.transform.parent.GetComponent<Cell>();
            if (targetCells.Count > 0 && targetCells[0] == other.GetComponent<Cell>())
            {
                if (state == State.movingToCells)
                {
                    targetCells[0].UnMarkCell();
                    targetCells.RemoveAt(0);
                }
            }
            if (state == State.deliveringCloth)
            {
                TryToDeliveryCloth(other.transform.parent.GetComponent<Cell>());
            }
            else if (state == State.pickPrisonersToMoveYard || state == State.pickPrisonersToMoveCafeteria)
            {
                if (currentTargetPrisoner != null &&
                    currentTargetPrisoner.currentCell == other.transform.parent.GetComponent<Cell>())
                {
                    StartCoroutine(TryToAddPrisonerToQueeFromCell(other.transform.parent.GetComponent<Cell>()));
                }
            }
        }
        else if (other.CompareTag(dataBase.tags.resouceField))
        {
            targetResourceField = other.GetComponent<Resource>();
        }
        else if (other.CompareTag(dataBase.tags.foodDeliveryPoint))
        {
            currentFoodDeliveryPoint = other.GetComponent<FoodDeliveryPoint>();
            if (state == State.deliveringFood)
            {
                StartCoroutine(TryToDeliveryFood(other.GetComponent<FoodDeliveryPoint>()));
            }
        }
    }
    //                                 ******** ON TRIGGER EXIT ***************
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(dataBase.tags.cell))
        {
            if (tempCell == other.transform.parent.GetComponent<Cell>())
            {
                tempCell = null;
            }
        }
        else if (other.CompareTag(dataBase.tags.foodDeliveryPoint))
        {
            if (currentFoodDeliveryPoint == other.GetComponent<FoodDeliveryPoint>())
            {
                currentFoodDeliveryPoint = null;
            }
        }
    }
    #endregion
    
    
    #region AI STATE ACTIONS
    
        #region Picking Actions
    IEnumerator PickActionRoutine()
    {
        state = State.idle;
        SetDestination(mapManager.guardIdlePos);
        yield return new WaitForSeconds(0.5f);
        while (state == State.idle)
        {
            PickAction();
            yield return new WaitForSeconds(0.1f);
        }

        if (state == State.pickPrisonersToMoveCell)
        {
            StartCoroutine(RoutinePickPrisonerToMoveCell());
        }
        else if (state == State.pickClothsToDelivery)
        {
            StartCoroutine(RoutinePickPrisonerToDeliveryCloth());
        }
        else if (state == State.pickPrisonersToMoveYard)
        {
            StartCoroutine(RoutinePickPrisonerToMoveArea());
        }
        else if (state == State.pickPrisonersToMoveCafeteria)
        {
            StartCoroutine(RoutinePickPrisonerToMoveArea());
        }
        else if (state == State.pickFoodsToDelivery)
        {
            StartCoroutine(RoutinePickPrisonerToDeliveryFood());
        }
    }

    private void PickAction()
    {
        targetPrisoners.Clear();
        
        if (randomActionIndex < 4)
        {
            randomActionIndex++;
        }
        else
        {
            randomActionIndex = 0;
        }
        
        
        switch (randomActionIndex)
        {
            case 0:
                targetPrisonerStage = PrisonerController.PrisonerStage.WaitingToMoveCell;
                targetState = State.pickPrisonersToMoveCell;
                //TryToPickPrisonerToMoveCell();
                break;
            case 1:
                targetPrisonerStage = PrisonerController.PrisonerStage.WaitingToMoveYard;
                targetState = State.pickPrisonersToMoveYard;
                //TryToPickPrisonerToMoveYard();
                break;
            case 2:
                targetPrisonerStage = PrisonerController.PrisonerStage.WaitingToMoveCafeteria;
                targetState = State.pickPrisonersToMoveCafeteria;
                //TryToPickPrisonerToMoveCafeteria();
                break;
            case 3:
                targetPrisonerStage = PrisonerController.PrisonerStage.WaitingForCloths;
                targetState = State.pickClothsToDelivery;
                //TryToPickClothToDelivery();
                break;
            case 4:
                targetPrisonerStage = PrisonerController.PrisonerStage.WaitingForFoodAtCafeteria;
                targetState = State.pickFoodsToDelivery;
                //TryToPickFoodToDelivery();
                break;
        }
        
        TryToPickAction();
    }
    
    private void TryToPickAction()
    {
        for (int i = 0; i < prisonerList.Count; i++)
        {
            //Debug For Dynamic List
            if (i >= prisonerList.Count)
            {
                break;
            }
            else
            {
                if (targetState == State.pickClothsToDelivery && targetState == State.pickFoodsToDelivery)
                {
                    pickCondition = targetPrisoners.Count < dataBase.guardData.objectStackCountCurrent;
                }
                else
                {
                    pickCondition = targetPrisoners.Count < dataBase.guardData.capacityPrisonerCurrent;
                }

                if (!pickCondition)
                {
                    break;
                }
                
                if (prisonerList[i].prisonerStage == targetPrisonerStage && !prisonerList[i].isMarked)
                {
                    targetPrisoners.Add(prisonerList[i]);
                    prisonerList[i].MarkPrisoner(this);
                }
            }
        }

        if (targetPrisoners.Count > 0)
        {
            state = targetState;
        }
    }
    #region Move To Cell
    IEnumerator RoutinePickPrisonerToMoveCell()
    {
        yield return new WaitForSeconds(0.1f);
        while (currentPrisoners.Count < targetPrisoners.Count)
        {
            yield return new WaitForSeconds(0.5f);
            CheckTargetPrisonerList();
            if (targetPrisoners.Count == 0)
            {
                break;
            }
            if (currentTargetPrisoner == null && currentPrisoners.Count < targetPrisoners.Count)
            {
                for (int i = 0; i < targetPrisoners.Count; i++)
                {
                    if (!currentPrisoners.Contains(targetPrisoners[i]))
                    {
                        currentTargetPrisoner = targetPrisoners[i];
                        break;
                    }
                }
            }

            if (currentTargetPrisoner != null)
            {
                SetDestination(currentTargetPrisoner.transform);
            }
        }

        if (currentPrisoners.Count > 0)
        {
            StartCoroutine(RoutineDeliveringPrisonerToCell());
        }
        else
        {
            //ACTION FAILED !
            StartCoroutine(PickActionRoutine());
        }
    }

    IEnumerator RoutineDeliveringPrisonerToCell()
    {
        state = State.movingToCells;
        yield return new WaitForSeconds(0.1f);
        GetEmptyCells();
        while (currentPrisoners.Count > 0)
        {
            if (targetCells.Count > 0)
            {
                SetDestination(targetCells[0].colliderActive.transform);
            }
            yield return new WaitForSeconds(0.5f);
            if (targetCells.Count < currentPrisoners.Count)
            {
                CheckTargetCells();
                FillTargetCells();
                Debug.Log("***");
            }

            if (currentPrisoners.Count == 0)
            {
                for (int i = 0; i < targetCells.Count; i++)
                {
                    targetCells[i].UnMarkCell();
                }
            }
        }

        StartCoroutine(PickActionRoutine());

    }

    private void FillTargetCells()
    {
        for (int i = 0; i < cells.Count; i++)
        {
            if ((!cells[i].isMarked || cells[i].markerGuard == this) && cells[i].currentPrisoner == null)
            {
                if (!targetCells.Contains(cells[i]))
                {
                    cells[i].MarkCell(this);
                    targetCells.Add(cells[i]);
                }

                if (targetCells.Count >= currentPrisoners.Count)
                {
                    break;
                }
            }
        }
    }

    private void CheckTargetCells()
    {
        for (int i = 0; i < targetCells.Count; i++)
        {
            if (targetCells[i].markerGuard != this)
            {
                targetCells.RemoveAt(i);
                i--;
            }
        }
    }

    private void GetEmptyCells()
    {
        ClearTargetCells();
        for (int i = 0; i < cells.Count; i++)
        {
            if ((!cells[i].isMarked || cells[i].markerGuard == this) && cells[i].currentPrisoner == null)
            {
                if (!targetCells.Contains(cells[i]))
                {
                    cells[i].MarkCell(this);
                    targetCells.Add(cells[i]);
                }
                if (targetCells.Count >= currentPrisoners.Count)
                {
                    break;
                }
            }
        }
    }
    #endregion
    
    #region Move To Yard & Cafeteria
    
    IEnumerator RoutinePickPrisonerToMoveArea()
    {
        yield return new WaitForSeconds(0.1f);
        while (currentPrisoners.Count < targetPrisoners.Count)
        {
            yield return new WaitForSeconds(0.5f);
            CheckTargetPrisonerList();
            if (currentTargetPrisoner == null && currentPrisoners.Count < targetPrisoners.Count)
            {
                for (int i = 0; i < targetPrisoners.Count; i++)
                {
                    if (!currentPrisoners.Contains(targetPrisoners[i]))
                    {
                        currentTargetPrisoner = targetPrisoners[i];
                        break;
                    }
                }
            }

            if (currentTargetPrisoner != null)
            {
                SetDestination(currentTargetPrisoner.currentCell.colliderActive.transform);
                while (currentTargetPrisoner != null)
                {
                    yield return new WaitForSeconds(0.2f);
                }
            }
            CheckTargetPrisonerList();
            if (currentPrisoners.Count >= targetPrisoners.Count)
            {
                break;
            }
        }

        if (currentPrisoners.Count > 0)
        {
            //StartCoroutine(RoutineDeliveringPrisonerToCell());
            //state = State.movingToYard;
            if (state == State.pickPrisonersToMoveYard)
            {
                state = State.movingToYard;
            }
            else if (state == State.pickPrisonersToMoveCafeteria)
            {
                state = State.movingToCafeteria;
            }
            
            while (currentPrisoners.Count > 0)
            {
                if (state == State.movingToCafeteria)
                {
                    SetDestination(mapManager.cafeteriaColliderArea);
                }
                else if (state == State.movingToYard)
                {
                    SetDestination(mapManager.yardColliderArea);
                }
                yield return new WaitForSeconds(0.5f);

                if (currentPrisoners.Count > 0)
                {
                    /*
                    if (currentPrisoners[0].prisonerStage != PrisonerController.PrisonerStage.MovingToYard && currentPrisoners[0].prisonerStage != PrisonerController.PrisonerStage.MovingToCafeteria)
                    {
                        currentPrisoners[0].UnMarkPrisonner();
                    }
                    */
                }
            }

            StartCoroutine(PickActionRoutine());
        }
        else
        {
            //ACTION FAILED !
            StartCoroutine(PickActionRoutine());
        }
    }
    
    
    
    
    #endregion
    
    private void TryToPickPrisoner(Cell targetCell)
    {
        if (targetCell.currentPrisoner != null)
        {
            PrisonerController cellPrisoner = targetCell.currentPrisoner;
            if (cellPrisoner.prisonerStage == targetPrisonerStage)
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
                yield return new WaitForSeconds(0.01f);
            }
            else
            {
                targetCell.ResetFillAmount();
                condition = false;
                break;
            }
        }

        if (condition && tempCell == targetCell)
        {
            if (targetCell != null && targetCell.currentPrisoner != null)
            {
                currentPrisoners.Add(targetCell.currentPrisoner);
                targetCell.currentPrisoner.FollowGuard(this);
                targetCell.ResetCellPrisoner();
            }
            currentTargetPrisoner = null;
        }
    }
    
    
    #region Pick Prisoner To Cloth & Delivering Cloth
    IEnumerator RoutinePickPrisonerToDeliveryCloth()
    {
        yield return new WaitForSeconds(0.1f);
        SetDestination(mapManager.resourceCloth.transform);
        yield return new WaitForSeconds(0.1f);
        while (myAgent.remainingDistance > 0.5f)
        {
            yield return new WaitForSeconds(0.5f);
            CheckTargetPrisonerList();
            if (targetPrisoners.Count == 0)
            {
                break;
            }
        }

        if (targetPrisoners.Count > 0)
        {
            //StartCoroutine(RoutineDeliveringPrisonerToCell());
            //StartCoroutine()
            while (resourceCountCloth < targetPrisoners.Count)
            {
                CheckTargetPrisonerList();
                if (targetPrisoners.Count == 0)
                {
                    break;
                }

                if (targetResourceField != null)
                {
                    GetResource();
                }
                else
                {
                    Debug.Log("Resource Field Null");
                }
                yield return new WaitForSeconds(0.5f);
            }

            if (targetPrisoners.Count == 0)
            {
                StartCoroutine(PickActionRoutine());
            }
            else
            {
                state = State.deliveringCloth;
                float debugCounter = 0;
                if (targetPrisoners.Count > 0)
                {
                    PrisonerController debugPrisoner = targetPrisoners[0];
                }
                while (targetPrisoners.Count > 0)
                {
                    CheckTargetPrisonerList();
                    if (targetPrisoners.Count > 0)
                    {
                        if (debugCounter < 7)
                        {
                            debugCounter += 0.1f;
                            SetDestination(targetPrisoners[0].currentCell.colliderActive.transform);
                        }
                        else if(debugCounter < 10)
                        {
                            SetDestination(mapManager.guardIdlePos);
                            debugCounter += 0.1f;
                        }
                        else
                        {
                            debugCounter = 0;
                        }
                        yield return new WaitForSeconds(0.1f);
                        /*
                        while (myAgent.remainingDistance > 0.5f)
                        {
                            CheckTargetPrisonerList();
                            if (targetPrisoners.Count > 0)
                            {
                                yield return new WaitForSeconds(0.2f);
                            }
                            else
                            {
                                break;
                            }
                        }
                        */
                    }
                    else
                    {
                        break;
                    }
                    yield return new WaitForSeconds(0.2f);
                }

                StartCoroutine(PickActionRoutine());
            }
        }
        else
        {
            //ACTION FAILED !
            StartCoroutine(PickActionRoutine());
        }
        
        
    }
    IEnumerator RoutinePickPrisonerToDeliveryFood()
    {
        yield return new WaitForSeconds(0.1f);
        SetDestination(mapManager.resourceFood.colliderField);
        yield return new WaitForSeconds(0.1f);
        while (myAgent.remainingDistance > 0.5f)
        {
            yield return new WaitForSeconds(0.5f);
            CheckTargetPrisonerList();
            if (targetPrisoners.Count == 0)
            {
                break;
            }
        }

        if (targetPrisoners.Count > 0)
        {
            //StartCoroutine(RoutineDeliveringPrisonerToCell());
            //StartCoroutine()
            while (resourceCountFood < targetPrisoners.Count)
            {
                CheckTargetPrisonerList();
                if (targetPrisoners.Count == 0)
                {
                    break;
                }

                if (targetResourceField != null)
                {
                    state = State.pickFoodsToDelivery;
                    GetResource();
                }
                else
                {
                    Debug.Log("Resource Field Null");
                }
                yield return new WaitForSeconds(0.5f);
            }

            if (targetPrisoners.Count == 0)
            {
                StartCoroutine(PickActionRoutine());
            }
            else
            {
                state = State.deliveringFood;
                while (targetPrisoners.Count > 0)
                {
                    CheckTargetPrisonerList();
                    if (targetPrisoners.Count > 0)
                    {
                        SetDestination(targetPrisoners[0].targetActionField.targetFoodDeliveryPoint.transform);
                        yield return new WaitForSeconds(0.1f);
                        /*
                        while (myAgent.remainingDistance > 0.5f)
                        {
                            CheckTargetPrisonerList();
                            if (targetPrisoners.Count > 0)
                            {
                                yield return new WaitForSeconds(0.2f);
                            }
                            else
                            {
                                break;
                            }
                        }
                        */
                    }
                    else
                    {
                        break;
                    }
                    yield return new WaitForSeconds(0.2f);
                }

                StartCoroutine(PickActionRoutine());
            }
        }
        else
        {
            //ACTION FAILED !
            StartCoroutine(PickActionRoutine());
        }
        
        
    }
    #region ResourceInteraction

    

    private void GetResource()
    {
        if (stackCount < dataBase.guardData.objectStackCountCurrent  && targetResourceField.resourceCount > 0)
        {
            targetResourceField.DecreaseResourceCount();
            stackCount++;
            if (!stackObject.activeInHierarchy)
            {
                stackObject.SetActive(true);
            }
            resources.Add(targetResourceField);
            if (targetResourceField.resourceType == Resource.ResourceType.cloth)
            {
                resourceCountCloth++;
            }
            else if (targetResourceField.resourceType == Resource.ResourceType.food)
            {
                resourceCountFood++;
            }
            ArrangeResourceModels();
        }
        else if (stackCount >= dataBase.guardData.objectStackCountCurrent && targetResourceField.resourceCount > 0)
        {
            for (int i = 0; i < resources.Count; i++)
            {
                if (resources[i].resourceType != targetResourceField.resourceType)
                {
                    resources[i] = targetResourceField;
                    if (targetResourceField.resourceType == Resource.ResourceType.cloth)
                    {
                        resourceCountCloth++;
                        resourceCountFood--;
                    }
                    else if (targetResourceField.resourceType == Resource.ResourceType.food)
                    {
                        resourceCountFood++;
                        resourceCountCloth--;
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
                    meshesCloth[i].SetActive(true);
                    meshesFood[i].SetActive(false);
                }
                else if (resources[i].resourceType == Resource.ResourceType.food)
                {
                    meshesCloth[i].SetActive(false);
                    meshesFood[i].SetActive(true);
                }
            }
            else
            {
                meshesCloth[i].SetActive(false);
                meshesFood[i].SetActive(false);
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
    private void TryToDeliveryCloth(Cell targetCell)
    {
        if (resourceCountCloth > 0 && targetPrisoners.Count > 0 && targetCell == targetPrisoners[0].currentCell)
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
                yield return new WaitForSeconds(0.01f);
            }
            else
            {
                targetCell.ResetFillAmount();
                condition = false;
                break;
            }
        }

        if (condition)
        {
            GiveCloth(targetCell.currentPrisoner);
            targetCell.ResetFillAmount();
            targetCell.currentPrisoner.UnMarkPrisonner();
        }
    }

    private void GiveFood()
    {
        if (resourceCountFood > 0)
        {
            resourceCountFood--;
            RemoveResource(Resource.ResourceType.food);
        }
    }

    private void GiveCloth(PrisonerController targetPrisoner)
    {
        if (resourceCountCloth > 0)
        {
            resourceCountCloth--;
            RemoveResource(Resource.ResourceType.cloth);
            targetPrisoner.WearNewClothes();
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
            stackObject.SetActive(false);
        }
        ArrangeResourceModels();
    }

    IEnumerator TryToDeliveryFood(FoodDeliveryPoint foodDeliveryPoint)
    {
        yield return new WaitForSeconds(0.1f);
        while (currentFoodDeliveryPoint == foodDeliveryPoint)
        {
            if (currentFoodDeliveryPoint.hungryPrisoners.Count > 0 && resourceCountFood > 0 && currentFoodDeliveryPoint == foodDeliveryPoint)
            {
                bool condition = true;
                for (float i = 0; i < 1; i += 0.01f)
                {
                    yield return new WaitForSeconds(0.01f);
                    CheckTargetPrisonerList();
                    if (currentFoodDeliveryPoint == foodDeliveryPoint && resourceCountFood > 0 && targetPrisoners.Count > 0)
                    {
                        foodDeliveryPoint.DecreaseFillAmount(0.01f);
                    }
                    else
                    {
                        foodDeliveryPoint.ResetFillAmount();
                        condition = false;
                        break;
                    }
                }
                foodDeliveryPoint.ResetFillAmount();
                if (condition && targetPrisoners.Count > 0 )
                {
                    targetPrisoners[0].GetFood();
                    GiveFood();
                }
            }
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    #endregion
    
    #endregion

    private void ClearTargetCells()
    {
        for (int i = 0; i < targetCells.Count; i++)
        {
            if (targetCells[i].markerGuard == this)
            {
                targetCells[i].UnMarkCell();
                i--;
            }
            else
            {
                targetCells.RemoveAt(i);
                i--;
            }
        }
    }

    public void CellUnMarked(Cell unMarkedCell)
    {
        if (targetCells.Contains(unMarkedCell))
        {
            targetCells.Remove(unMarkedCell);
        }
    }

    private void CheckTargetPrisonerList()
    {
        for (int i = 0; i < targetPrisoners.Count; i++)
        {
            if (!currentPrisoners.Contains(targetPrisoners[i]))
            {
                if (targetPrisoners[i].markerGuard != this || targetPrisoners[i].prisonerStage != targetPrisonerStage)
                {
                    targetPrisoners.RemoveAt(i);
                    i--;
                }
            }
        }
    }
    
    #region Movement                                *************** Movement ******************

    public void SetDestination(Transform destination)
    {
        target = destination;
        myAgent.SetDestination(target.position);
    }
    
    
    #endregion
    
    /*
    private void TryToPickPrisonerToMoveCell()
    {
        for (int i = 0; i < prisonerList.Count; i++)
        {
            //Debug For Dynamic List
            if (i >= prisonerList.Count)
            {
                break;
            }
            else
            {
                if (prisonerList[i].prisonerStage == PrisonerController.PrisonerStage.WaitingToMoveCell && targetPrisoners.Count < dataBase.guardData.capacityPrisoner)
                {
                    targetPrisoners.Add(prisonerList[i]);
                    prisonerList[i].MarkPrisoner(this);
                }
            }
        }

        if (targetPrisoners.Count > 0)
        {
            state = State.pickPrisonersToMoveCell;
        }
    }

    private void TryToPickPrisonerToMoveYard()
    {
        for (int i = 0; i < prisonerList.Count; i++)
        {
            //Debug For Dynamic List
            if (i >= prisonerList.Count)
            {
                break;
            }
            else
            {
                if (prisonerList[i].prisonerStage == PrisonerController.PrisonerStage.WaitingToMoveYard && targetPrisoners.Count < dataBase.guardData.capacityPrisoner)
                {
                    targetPrisoners.Add(prisonerList[i]);
                    prisonerList[i].MarkPrisoner(this);
                }
            }
        }
    }

    private void TryToPickPrisonerToMoveCafeteria()
    {
        for (int i = 0; i < prisonerList.Count; i++)
        {
            //Debug For Dynamic List
            if (i >= prisonerList.Count)
            {
                break;
            }
            else
            {
                if (prisonerList[i].prisonerStage == PrisonerController.PrisonerStage.WaitingToMoveCafeteria && targetPrisoners.Count < dataBase.guardData.capacityPrisoner)
                {
                    targetPrisoners.Add(prisonerList[i]);
                    prisonerList[i].MarkPrisoner(this);
                }
            }
        }
    }

    private void TryToPickClothToDelivery()
    {
        for (int i = 0; i < prisonerList.Count; i++)
        {
            //Debug For Dynamic List
            if (i >= prisonerList.Count)
            {
                break;
            }
            else
            {
                if (prisonerList[i].prisonerStage == PrisonerController.PrisonerStage.WaitingForCloths && targetPrisoners.Count < dataBase.guardData.capacityMachine)
                {
                    targetPrisoners.Add(prisonerList[i]);
                    prisonerList[i].MarkPrisoner(this);
                }
            }
        }
    }

    private void TryToPickFoodToDelivery()
    {
        for (int i = 0; i < prisonerList.Count; i++)
        {
            //Debug For Dynamic List
            if (i >= prisonerList.Count)
            {
                break;
            }
            else
            {
                if (prisonerList[i].prisonerStage == PrisonerController.PrisonerStage.WaitingForFoodAtCafeteria && targetPrisoners.Count < dataBase.guardData.capacityMachine)
                {
                    targetPrisoners.Add(prisonerList[i]);
                    prisonerList[i].MarkPrisoner(this);
                }
            }
        }
        
    }
    */
    
        #endregion
    
    #endregion

    public void ChangeAgentSpeed()
    {
        myAgent.speed = dataBase.guardData.speed;
    }

    public void RemovePrisoner(PrisonerController prisoner)
    {
        if (currentPrisoners.Contains(prisoner))
        {
            currentPrisoners.Remove(prisoner);
        }

        if (targetPrisoners.Contains(prisoner))
        {
            targetPrisoners.Remove(prisoner);
        }
        
        

        if (currentTargetPrisoner == prisoner)
        {
            currentTargetPrisoner = null;
        }
        
        
    }
    
    #region Animations

    private void AnimWalking()
    {
        anim.SetBool(dataBase.guardData.anim.idle, false);
        anim.SetBool(dataBase.guardData.anim.walking, true);
    }

    private void AnimIdle()
    {
        anim.SetBool(dataBase.guardData.anim.idle, true);
        anim.SetBool(dataBase.guardData.anim.walking, false);
    }

    private void AnimChangeLayerWeight(float value)
    {
        anim.SetLayerWeight(1, value);
    }
    
    
    #endregion
}
