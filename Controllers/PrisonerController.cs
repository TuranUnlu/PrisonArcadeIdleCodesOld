using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;
using DG.Tweening;

public class PrisonerController : MonoBehaviour
{
    public enum PrisonerStage
    {
        WaitingToMoveCell,
        MovingToCell,
        WaitingForCloths,
        MovingInToCell,
        WaitingAtCell,
        WaitingToMoveYard,
        MovingToYard,
        WaitingSeatAtYard,
        MovingToSeatAtYard,
        YardAction,
        WaitingToMoveCafeteria,
        MovingToCafeteria,
        WaitingSeatAtCafeteria,
        MovingToSeatAtCafeteria,
        WaitingForFoodAtCafeteria,
        EatingFood,
        Leave
    }
    
    public PrisonerStage prisonerStage;

    private PrisonerStage targetPrisonerStage;

    public enum MovingType
    {
        canMoveAlone,
        isNeedToMoveWithEscort,
        isFollowingGuard,
        isFollowingPlayer,
    }

    public MovingType movingType;

    private enum PunishmentType
    {
        justWaiting,
        yard,
        cafeteria
    }

    [SerializeField] private List<PunishmentType> punishments = new List<PunishmentType>();

    private enum WaitingInCellType
    {
        wander,
        pushUp,
        sitUp
    }

    [SerializeField] private WaitingInCellType cellWaitingType;

    [SerializeField] public ActionFields targetActionField;

    [SerializeField]
    private float punishmentTimeCounter;

    private float punishmentTimeTarget;
    [SerializeField]
    private Transform currentCellTarget;
    

    [Header("NavMesh Agent")]
    //
    [SerializeField] private NavMeshAgent myAgent;

    [Header("Animator")]
    //
    [SerializeField] private Animator anim;

    #region Clothes 
    
    [Header("Clothes")]
    //
    [SerializeField] private GameObject clothPrisoner;
    //[SerializeField] private GameObject clothCivil;
    [SerializeField] private List<GameObject> clothsCivil;

    #endregion

    [Header("Items")]
    //
    [SerializeField] private GameObject handCuff;
    [SerializeField] private GameObject shackles;
    [SerializeField] private GameObject basketball;
    [SerializeField] private GameObject dumbellLeft;
    [SerializeField] private GameObject dumbellRight;
    [SerializeField] private GameObject particleChange;
    
    
    [Space]
    [Tooltip("is Prisoner Generated at Last Sesion")]
    public bool isRespawnPrisoner;


    public Cell currentCell;
    private bool hasCloth;
    private bool hasFood;

    private GuardController guardAI;
    private PlayerController guardPlayer;
    [SerializeField] private Transform target;

    private Vector3 lookPos;
    private int spawnIndex = -1;
    private bool isFirstAction;
    [SerializeField] private GameObject prisonerArrow;
    public bool isTutorialPrisoner;


    [Header("Mark Variables")]
    public bool isMarked;
    public GuardController markerGuard;
    
    
    
    //public Transform followTarget;

    private GameObject tempFood;
    private Food foodController;
    private DataBase database;
    private GameData gameData;
    private MapManager mapManager;
    private Vector3 firstBasketBallPos;
    private Transform basketBallParent;

    public void SetSpawnIndex(int value)
    {
        spawnIndex = value;
        AddYourSelfToMapManager();
        StartCoroutine(MoveToField());
        lookPos = mapManager.triggerField.position;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
    }

    IEnumerator MoveToField()
    {
        GetGameData();
        GetDataBase();
        AnimWalk();
        yield return new WaitForSeconds(0.5f);
        myAgent.enabled = false;
        while (!myAgent.enabled)
        {
            transform.position =Vector3.Lerp(transform.position, lookPos, Time.deltaTime);
            AnimWalk();
            yield return new WaitForSeconds(Time.deltaTime);
        }

        target = ObjectManager.prisonerSpawner.targetPoints[spawnIndex];
        myAgent.SetDestination(target.position);
    }

    private void Awake()
    {
        AwakeMethods();
    }

    #region AwakeMethods

    void AwakeMethods()
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
        GetDataBase();
        GetGameData();
        GetMapManager();
        GetFirstBasketBallPos();
        GetBasketBallParent();
        WearRandomCloth();
        AnimChangeLayerWeight(100);
        AnimIdle();
        WaitToMoveCell();
        AddYourSelfToMapManager();
        CheckTutorial();
    }

    private void GetDataBase()
    {
        database = ObjectManager.DataBase;
    }

    private void GetMapManager()
    {
        mapManager = ObjectManager.MapManager;
    }

    private void GetFirstBasketBallPos()
    {
        firstBasketBallPos = basketball.transform.localPosition;
    }

    private void GetBasketBallParent()
    {
        basketBallParent = basketball.transform.parent;
    }

    private void WearRandomCloth()
    {
        int randIndex = Random.Range(0, clothsCivil.Count);
        clothPrisoner.SetActive(false);
        foreach (GameObject cloth in clothsCivil)
        {
            cloth.SetActive(false);
        }
        clothsCivil[randIndex].SetActive(true);
    }

    private void GetGameData()
    {
        gameData = ObjectManager.GameData;
    }

    private void AddYourSelfToMapManager()
    {
        if (mapManager == null)
        {
            GetMapManager();
        }
        mapManager.AddPrisoner(this);
    }

    private void CheckTutorial()
    {
        if (database.isTutorialPassed)
        {
            if (prisonerArrow != null)
            {
                prisonerArrow.SetActive(false);
            }
        }
    }
    #endregion

    // Update is called once per frame
    void Update()
    {
        
        if (target != null)
        {
            myAgent.SetDestination(target.position);
        }

        if (prisonerStage == PrisonerStage.WaitingToMoveCell)
        {
            if (myAgent.enabled && (!myAgent.hasPath || myAgent.remainingDistance < 0.5f))
            {
                AnimIdle();
            }
            else
            {
                AnimWalk();
            }
        }
        
        if (movingType == MovingType.isFollowingPlayer)
        {
            int targetIndex = guardPlayer.queuePrisoners.IndexOf(this);
            if (targetIndex == 0)
            {
                target = guardPlayer.transform;
            }
            else
            {
               target = guardPlayer.queuePrisoners[targetIndex - 1].transform;
            }

            myAgent.speed = database.player.speed;
            if (!myAgent.enabled)
            {
                myAgent.enabled = true;
            }
            myAgent.SetDestination(target.position - target.forward * 0.5f);
            if (myAgent.remainingDistance < 0.5f)
            {
                myAgent.speed = 0;
                AnimIdle();
            }
            else
            {
                AnimWalk();
            }
            SetAcceleration();
            lookPos = target.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);
        }
        else if (movingType == MovingType.isFollowingGuard)
        {
            int targetIndex = guardAI.currentPrisoners.IndexOf(this);
            if (targetIndex == 0)
            {
                target = guardAI.transform;
            }
            else
            {
                target = guardAI.currentPrisoners[targetIndex - 1].transform;
            }

            myAgent.speed = database.player.speed;
            if (!myAgent.enabled)
            {
                myAgent.enabled = true;
            }

            myAgent.SetDestination(target.position - target.forward * 0.5f);
            if (myAgent.remainingDistance < 0.5f)
            {
                myAgent.speed = 0;
                AnimIdle();
            }
            else
            {
                AnimWalk();
            }
            SetAcceleration();
            lookPos = target.position;
            lookPos.y = transform.position.y;
            transform.LookAt(lookPos);
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
    
    #region Stage Changes & Actions

    public void WaitToMoveCell()
    {
        prisonerStage = PrisonerStage.WaitingToMoveCell;
        movingType = MovingType.isNeedToMoveWithEscort;
        AnimChangeLayerWeight(100);

    }

    public void FollowGuard(PlayerController player)
    {
        movingType = MovingType.isFollowingPlayer;
        guardPlayer = player;
        FollowGuard();
    }

    public void FollowGuard(GuardController guard)
    {
        movingType = MovingType.isFollowingGuard;
        guardAI = guard;
        FollowGuard();
    }

    private void FollowGuard()
    {
        if (!isFirstAction)
        {
            isFirstAction = true;
            if (spawnIndex != -1)
            {
                ObjectManager.prisonerSpawner.ClearTheIndex(spawnIndex);
            }

            if (!database.isTutorialPassed && prisonerArrow != null)
            {
                prisonerArrow.SetActive(false);
                EventManager.TutorialPrisonelPicked();
            }
        }
        
        AnimStopCellAction();
        AnimChangeLayerWeight(100);
        
        if (prisonerStage == PrisonerStage.WaitingToMoveCell)
        {
            prisonerStage = PrisonerStage.MovingToCell;
        }
        else if (prisonerStage == PrisonerStage.WaitingToMoveCafeteria)
        {
            prisonerStage = PrisonerStage.MovingToCafeteria;
        }
        else if (prisonerStage == PrisonerStage.WaitingToMoveYard)
        {
            prisonerStage = PrisonerStage.MovingToYard;
        }

        currentCell = null;
    }


    private void MoveInToCell()
    {
        RemoveYourSelfFromGuardList();
        
        AnimChangeLayerWeight(0);
        
        movingType = MovingType.canMoveAlone;
        prisonerStage = PrisonerStage.MovingInToCell;
        //currentCell.CellPicked(this);
        target = currentCell.cellSpawnPoint;
        myAgent.SetDestination(currentCell.cellSpawnPoint.position);
        myAgent.speed = database.prisonerData.defaultSpeed;
        myAgent.acceleration = database.prisonerData.defaultSpeed;

        if (!database.isTutorialPassed && prisonerArrow != null)
        {
            currentCell.DeActivateCellArrow();
        }
        
        SetAcceleration();
        StartCoroutine(WaitForMoveInToCell());
    }

    IEnumerator WaitForMoveInToCell()
    {
        yield return new WaitForSeconds(1.5f);
        
        while(currentCell.isPlayerInside || !(currentCell.isPrisonerInside || currentCell.prisonersInTheCell.Contains(this)) || (currentCell.prisonerInTheCellCount != 1) || myAgent.remainingDistance > 0.5f)
        {
            yield return new WaitForSeconds(0.5f);
        }
        currentCell.AnimCloseTheDoor();

        if (!hasCloth)
        {
            WaitForClothes();
        }
        else
        {
            WaitingInTheCell();
        }
    }
    

    private void WaitForClothes()
    {
        //prisonerStage = PrisonerStage.WaitingForCloths;
        targetPrisonerStage = PrisonerStage.WaitingForCloths;
        StartCoroutine(WaitingForTheNeedActions());
    }

    private void WaitForMoveToYard()
    {
        //prisonerStage = PrisonerStage.WaitingToMoveYard;
        targetPrisonerStage = PrisonerStage.WaitingToMoveYard;
        StartCoroutine(WaitingForTheNeedActions());
    }

    private void WaitForMoveToCafetaria()
    {
        //prisonerStage = PrisonerStage.WaitingToMoveCafeteria;
        targetPrisonerStage = PrisonerStage.WaitingToMoveCafeteria;
        StartCoroutine(WaitingForTheNeedActions());
    }

    IEnumerator WaitingForTheNeedActions()
    {
        /*
        if (prisonerStage != PrisonerStage.WaitingForCloths)
        {
            int randValue = Random.Range(0, 2) == 0 ? 0 : 2;
            AnimStartCellAction(randValue);
            yield return new WaitForSeconds(10f);
            AnimStopCellAction();
        }
        */
        if (targetPrisonerStage != PrisonerStage.WaitingForCloths)
        {
            int randValue = Random.Range(0, 2) == 0 ? 0 : 2;
            AnimStartCellAction(randValue);
            for(float i = 0; i<5; i+= 0.01f)
            {
                yield return new WaitForSeconds(0.01f);
                punishmentTimeCounter -= 0.01f;
                currentCell.ChangeFillAmount(Mathf.InverseLerp(0, punishmentTimeTarget, punishmentTimeCounter));
            }
            //yield return new WaitForSeconds(10f);
            AnimStopCellAction();
        }
        
        yield return new WaitForSeconds(0.5f);
        target = currentCell.cellWaitingPoint;
        myAgent.SetDestination(currentCell.cellWaitingPoint.position);
        while (myAgent.remainingDistance > 0.5f)
        {
            AnimWalk();
            yield return new WaitForSeconds(0.2f);
        }
        AnimWaitingForNeedInCell();
        prisonerStage = targetPrisonerStage;
        currentCell.ChangeUIState(prisonerStage);
        if (!database.isTutorialPassed && prisonerArrow != null)
        {
            currentCell.ActivateCellArrowFromPrisoner();
            mapManager.ActivateArrowCloth();
            EventManager.TutorialCameraChange();
        }
    }

    public void WearNewClothes()
    {
        //clothCivil.SetActive(false);
        hasCloth = true;
        foreach (GameObject civilCloth in clothsCivil)
        {
            civilCloth.SetActive(false);
        }
        clothPrisoner.SetActive(true);
        particleChange.transform.position = clothPrisoner.transform.position;
        particleChange.SetActive(true);
        prisonerStage = PrisonerStage.WaitingAtCell;

        if (!database.isTutorialPassed && prisonerArrow != null)
        {
            currentCell.DeActivateCellArrow();
            mapManager.TryDeActivateArrowCloth();
        }
        WaitingInTheCell();
    }

    private void WaitingInTheCell()
    {
        prisonerStage = PrisonerStage.WaitingAtCell;
        target = currentCell.cellSpawnPoint;
        myAgent.SetDestination(currentCell.cellSpawnPoint.position);
        currentCell.ChangeUIState(prisonerStage);

        if (punishments.Count == 0)
        {
            if (Random.Range(0, 2) > 0)
            {
                if (database.yardData.isYardActive && Random.Range(0, 100) < database.prisonerData.addPunishmentChance && mapManager.fieldCountYard > 0)
                {
                    mapManager.fieldCountYard--;
                    punishments.Add(PunishmentType.yard);
                }

                if (database.cafeteriaData.isCafeteriaActive && Random.Range(0, 100) < database.prisonerData.addPunishmentChance && mapManager.fieldCountCafeteria > 0)
                {
                    mapManager.fieldCountCafeteria--;
                    punishments.Add(PunishmentType.cafeteria);
                }
            }
            else
            {
                if (database.cafeteriaData.isCafeteriaActive && Random.Range(0, 100) < database.prisonerData.addPunishmentChance && mapManager.fieldCountCafeteria > 0)
                {
                    mapManager.fieldCountCafeteria--;
                    punishments.Add(PunishmentType.cafeteria);
                }

                if (database.yardData.isYardActive && Random.Range(0, 100) < database.prisonerData.addPunishmentChance && mapManager.fieldCountYard > 0)
                {
                    mapManager.fieldCountYard--;
                    punishments.Add(PunishmentType.yard);
                }
            }
            
            punishments.Add(PunishmentType.justWaiting);
        }

        if (punishmentTimeTarget == 0)
        {
            if (!database.isTutorialPassed)
            {
                punishmentTimeCounter = database.prisonerData.punishmentTimeDefault;
            }
            else
            {
                punishmentTimeCounter = Random.Range(database.prisonerData.punishmentTimeDefault, database.prisonerData.punishmentTimeMax);
            }

            punishmentTimeTarget = punishmentTimeCounter;
        }

        if (punishments[0] == PunishmentType.justWaiting)
        {
            StartCoroutine(WaitingInTheCellRoutine());
        }
        else if (punishments[0] == PunishmentType.cafeteria)
        {
            punishments.RemoveAt(0);
            WaitForMoveToCafetaria();
        }
        else if (punishments[0] == PunishmentType.yard)
        {
            punishments.RemoveAt(0);
            WaitForMoveToYard();
        }
    }

    IEnumerator WaitingInTheCellRoutine()
    {
        int randValue = Random.Range(0, 3);
        if (randValue == 0)
        {
            cellWaitingType = WaitingInCellType.pushUp;
        }
        else if (randValue == 1)
        {
            cellWaitingType = WaitingInCellType.wander;
        }
        else if (randValue == 2)
        {
            cellWaitingType = WaitingInCellType.sitUp;
        }
        currentCell.ChangeUIState(prisonerStage);

        yield return new WaitForSeconds(0.1f);
        /*
        if (!database.isTutorialPassed)
        {
            punishmentTimeCounter = database.prisonerData.punishmentTimeDefault;
        }
        else
        {
            punishmentTimeCounter = Random.Range(database.prisonerData.punishmentTimeDefault, database.prisonerData.punishmentTimeMax);
        }

        punishmentTimeTarget = punishmentTimeCounter;
        */
        float deltaTime;
        if (cellWaitingType == WaitingInCellType.wander)
        {
            AnimStartCellAction(1);
            currentCellTarget = currentCell.cellWanderPointSecond;
            SetInCellLookPos();
            target = null;
            myAgent.SetDestination(currentCellTarget.position);
            myAgent.speed = 1;
            while (punishmentTimeCounter > 0)
            {
                deltaTime = Time.deltaTime;
                punishmentTimeCounter -= deltaTime;
                yield return new WaitForSeconds(deltaTime);
                currentCell.ChangeFillAmount(Mathf.InverseLerp(0, punishmentTimeTarget, punishmentTimeCounter));
                if (myAgent.remainingDistance < 0.5f)
                {
                    if (currentCellTarget == currentCell.cellWanderPointFirst)
                    {
                        currentCellTarget = currentCell.cellWanderPointSecond;
                        target = currentCellTarget;
                        SetInCellLookPos();
                        myAgent.SetDestination(currentCellTarget.position);
                    }
                    else
                    {
                        currentCellTarget = currentCell.cellWanderPointFirst;
                        target = currentCellTarget;
                        SetInCellLookPos();
                        myAgent.SetDestination(currentCellTarget.position);
                    }
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForEndOfFrame();
                }
                
                target = currentCellTarget;
                myAgent.SetDestination(currentCellTarget.position);
                transform.LookAt(lookPos);
            }
        }
        else if (cellWaitingType == WaitingInCellType.pushUp)
        {
            AnimStartCellAction(0);
            currentCellTarget = currentCell.cellSpawnPoint;
            SetInCellLookPos();
            myAgent.SetDestination(currentCellTarget.position);
            while (myAgent.remainingDistance > 0.5f)
            {
                deltaTime = Time.deltaTime;
                punishmentTimeCounter -= deltaTime;
                currentCell.ChangeFillAmount(Mathf.InverseLerp(0, punishmentTimeTarget, punishmentTimeCounter));
                transform.LookAt(lookPos);
                yield return new WaitForSeconds(deltaTime);
            }

            AnimStartCellAction(0);
            while (punishmentTimeCounter > 0)
            {
                deltaTime = Time.deltaTime;
                punishmentTimeCounter -= deltaTime;
                currentCell.ChangeFillAmount(Mathf.InverseLerp(0, punishmentTimeTarget, punishmentTimeCounter));
                yield return new WaitForSeconds(deltaTime);
            }
        }
        else if (cellWaitingType == WaitingInCellType.sitUp)
        {
            AnimStartCellAction(2);
            currentCellTarget = currentCell.cellSpawnPoint;
            SetInCellLookPos();
            myAgent.SetDestination(currentCellTarget.position);
            while (myAgent.remainingDistance > 0.5f)
            {
                deltaTime = Time.deltaTime;
                punishmentTimeCounter -= deltaTime;
                currentCell.ChangeFillAmount(Mathf.InverseLerp(0, punishmentTimeTarget, punishmentTimeCounter));
                transform.LookAt(lookPos);
                yield return new WaitForSeconds(deltaTime);
            }
            AnimStartCellAction(1);
            while (punishmentTimeCounter > 0)
            {
                deltaTime = Time.deltaTime;
                punishmentTimeCounter -= deltaTime;
                currentCell.ChangeFillAmount(Mathf.InverseLerp(0, punishmentTimeTarget, punishmentTimeCounter));
                yield return new WaitForSeconds(deltaTime);
            }
        }

        if (prisonerStage == PrisonerStage.WaitingAtCell && punishmentTimeCounter < 0)
        {
            StartCoroutine(LeavePrison());
        }

    }

    private void SetInCellLookPos()
    {
        lookPos = currentCellTarget.position;
        lookPos.y = transform.position.y;
    }

    IEnumerator LeavePrison()
    {
        prisonerStage = PrisonerStage.Leave;
        UnMarkPrisonner();
        mapManager.RemovePrisoner(this);
        if (mapManager.prisonerList.Count == 0)
        {
            database.isTutorialPassed = true;
            DataManager.SaveData(database);
        }
        yield return new WaitForSeconds(0.1f);
        InstantiateMoney(currentCell.colliderActive.transform, 1);
        AnimStopCellAction();
        currentCell.ResetCellPrisoner();
        AnimWalk();
        myAgent.speed = database.prisonerData.defaultSpeed;
        myAgent.acceleration = database.prisonerData.defaultSpeed;
        yield return new WaitForSeconds(0.5f);
        target = mapManager.mainDoor;
        myAgent.SetDestination(mapManager.mainDoor.transform.position);
        WearRandomCloth();
        
        yield return new WaitForSeconds(0.5f);
        while (myAgent.remainingDistance > 0.5f)
        {
            yield return new WaitForSeconds(0.5f);
        }

        if (currentCell != null)
        {
            currentCell.RemovePrisonerFromInTheCell(this);
        }
        gameObject.SetActive(false);
    }
   

    #endregion

    private void MoveToActionField()
    {
        StartCoroutine(KeepMoveToActionField());
    }

    IEnumerator KeepMoveToActionField()
    {
        target = targetActionField.transform;
        myAgent.SetDestination(targetActionField.transform.position);
        myAgent.speed = database.prisonerData.defaultSpeed;
        yield return new WaitForSeconds(0.2f);
        while (myAgent.remainingDistance > 0.5f)
        {
            yield return new WaitForSeconds(0.1f);
            myAgent.SetDestination(target.position);
        }

        if (prisonerStage == PrisonerStage.MovingToSeatAtCafeteria)
        {
            targetActionField.AddPrisonerToFoodDeliveryPoint(this);
            StartCoroutine(StartCafeteriaAction());
        }
        else if (prisonerStage == PrisonerStage.MovingToSeatAtYard)
        {
            StartCoroutine(StartYardAction());
        }
    }
    
    #region Yard Actions ***********************************************
    
    IEnumerator StartYardAction()
    {
        yield return new WaitForSeconds(0.1f);
        prisonerStage = PrisonerStage.YardAction;
        if (targetActionField.targetYardAnimation != ActionFields.TargetYardAnimation.Sitting)
        {
            PickYardAnimation();
        }
        else
        {
            StartCoroutine(SittingActions());
        }
        particleChange.transform.position = firstBasketBallPos;
        particleChange.SetActive(true);
        for (float i = 0; i < 10; i += 0.1f)
        {
            if (targetActionField.targetYardAnimation == ActionFields.TargetYardAnimation.BasketBall && i % 1 == 0)
            {
                basketball.transform.localPosition = firstBasketBallPos;
                basketball.SetActive(true);
                basketball.transform.DOJump(mapManager.BasketballHoop.position, 1, 1, 1);
            }
            yield return new WaitForSeconds(0.1f);
        }
        CloseItems();
        if (targetActionField.targetYardAnimation != ActionFields.TargetYardAnimation.Sitting)
        {
            AnimStopYardAction();
        }
        else
        {
            AnimStopSittingActions();
        }
        
        InstantiateMoney(targetActionField.waitingField, targetActionField.actionValue);
        
        StartCoroutine(MoveToActionWaitingField());
    }

    private void PickYardAnimation()
    {
        /*
         *  0 Push Up
         *  1 Sit Up
         *  2 Punching
         *  3 BasketBall
         *  4 Dumbell
         */

        switch (targetActionField.targetYardAnimation)
        {
            case ActionFields.TargetYardAnimation.PushUp :
                AnimStartYardAction(0);
                break;
            case ActionFields.TargetYardAnimation.SitUp :
                AnimStartYardAction(1);
                break;
            case ActionFields.TargetYardAnimation.Boxing :
                AnimStartYardAction(2);
                break;
            case ActionFields.TargetYardAnimation.BasketBall :
                lookPos = mapManager.BasketballHoop.position;
                lookPos.y = transform.position.y;
                transform.LookAt(lookPos);
                AnimStartYardAction(3);
                basketball.transform.SetParent(null);
                break;
            case ActionFields.TargetYardAnimation.Dumbell :
                AnimStartYardAction(4);
                dumbellLeft.SetActive(true);
                dumbellRight.SetActive(true);
                break;
            default:
                Debug.LogWarning("Target Yard Animation Case Missing!");
                break;
        }
    }

    private void CloseItems()
    {
        if (basketball.activeInHierarchy)
        {
            particleChange.transform.position = firstBasketBallPos;
            particleChange.SetActive(true);
        }
        basketball.SetActive(false);
        if (dumbellLeft.activeInHierarchy)
        {
            particleChange.transform.position = dumbellLeft.transform.position;
            particleChange.SetActive(true);
        }
        dumbellLeft.SetActive(false);
        if (dumbellRight.activeInHierarchy)
        {
            particleChange.transform.position = dumbellRight.transform.position;
            particleChange.SetActive(true);
        }
        dumbellRight.SetActive(false);
        basketball.transform.SetParent(basketBallParent);
        basketball.transform.localPosition = firstBasketBallPos;
    }
    

    IEnumerator MoveToActionWaitingField()
    {
        target = targetActionField.waitingField;
        while (myAgent.remainingDistance < 0.5f)
        {
            yield return new WaitForSeconds(0.1f);
        }

        prisonerStage = PrisonerStage.WaitingToMoveCell;
        movingType = MovingType.isNeedToMoveWithEscort;
        targetActionField.ActionFieldUsuable();
        targetActionField = null;
        AnimIdle();
    }
    
    #endregion
    
    #region Cafeteria Actions ******************************************

    IEnumerator StartCafeteriaAction()
    {
        yield return new WaitForSeconds(0.1f);
        prisonerStage = PrisonerStage.WaitingForFoodAtCafeteria;
        StartCoroutine(SittingCafeteriaActions());
    }
    #endregion

    private void RemoveYourSelfFromGuardList()
    {
        myAgent.speed = database.prisonerData.defaultSpeed;
        myAgent.acceleration = database.prisonerData.defaultSpeed;
        if (movingType == MovingType.isFollowingPlayer)
        {
            guardPlayer.RemovePrisonerFromQueue(this);
        }
        else if(movingType == MovingType.isFollowingGuard)
        {
            if (guardAI != null)
            {
                guardAI.RemovePrisoner(this);
                UnMarkPrisonner();
            }
        }

        if (guardAI != null)
        {
            guardAI.RemovePrisoner(this);
            UnMarkPrisonner();
        }
    }

    IEnumerator SittingActions()
    {
        yield return new WaitForSeconds(0.1f);

        lookPos = targetActionField.transform.position + 2 * targetActionField.transform.forward;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
        yield return new WaitForSeconds(0.2f);
        
        /*
         *  0 Stand To Sit
         *  1 Sitting Idle
         *  2 Sitting To Stand
         *  3 Eating
         */
        AnimStartSittingActions(0);
        for (float i = 0; i < 1; i += 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        AnimStartSittingActions(1);
        for (float i = 0; i < 8; i += 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        AnimStartSittingActions(2);
        for (float i = 0; i < 1; i += 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
        }
    }
    
    IEnumerator SittingCafeteriaActions()
    {
        yield return new WaitForSeconds(0.1f);

        lookPos = targetActionField.transform.position + 2 * targetActionField.transform.forward;
        lookPos.y = transform.position.y;
        transform.LookAt(lookPos);
        yield return new WaitForSeconds(0.2f);
        
        /*
         *  0 Stand To Sit
         *  1 Sitting Idle
         *  2 Sitting To Stand
         *  3 Eating
         */
        AnimStartSittingActions(0);
        for (float i = 0; i < 1; i += 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        AnimStartSittingActions(1);
        while (prisonerStage == PrisonerStage.WaitingForFoodAtCafeteria)
        {
            yield return new WaitForSeconds(0.1f);
        }
        AnimStartSittingActions(3);
        for (float i = 0; i < 5; i += 0.1f)
        {
            foodController.DecreaseFoodAmount(i/5f);
            yield return new WaitForSeconds(0.1f);
        }
        tempFood.SetActive(false);
        AnimStartSittingActions(2);
        for (float i = 0; i < 1; i += 0.1f)
        {
            yield return new WaitForSeconds(0.1f);
        }
        
        AnimStopSittingActions();
        
        InstantiateMoney(transform, targetActionField.actionValue);

        StartCoroutine(MoveToActionWaitingField());
    }

    public void GetFood()
    {
        targetActionField.RemovePrisonerFromDeliveryPoint();
        prisonerStage = PrisonerStage.EatingFood;
        InstantiateFood();
        RemoveYourSelfFromGuardList();
    }

    private void InstantiateFood()
    {
        tempFood = Instantiate(gameData.foodPrefab, transform.forward + transform.position, Quaternion.identity);
        foodController = tempFood.GetComponent<Food>();
    }

    private void InstantiateMoney(Transform targetPoint, int amount)
    {
        Vector3 targetPos = targetPoint.position;
        for (int i = 0; i < amount * 5 ; i++)
        {
            //targetPos = targetPos + Vector3.right * Random.Range(-1.5f, 1.5f) + Vector3.forward * Random.Range(-1.5f, 1.5f);
            targetPos = targetPoint.position + Vector3.right * (i %2) + Vector3.forward * (i%2)  +Vector3.up * i *0.5f;
            Instantiate(gameData.moneyPrefab, targetPos, Quaternion.identity);
        }
    }

    public void MarkPrisoner(GuardController marker)
    {
        isMarked = true;
        markerGuard = marker;
    }

    public void UnMarkPrisonner()
    {
        isMarked = false;
        if (markerGuard != null)
        {
            markerGuard.RemovePrisoner(this);
        }
        markerGuard = null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(database.tags.cell))
        {
            if (prisonerStage == PrisonerStage.MovingToCell && currentCell == null && other.transform.parent.GetComponent<Cell>().currentPrisoner == null && other.transform.parent.GetComponent<Cell>().isCellActive)
            {
                other.transform.parent.GetComponent<Cell>().CellPicked(this);
                currentCell = other.transform.parent.gameObject.GetComponent<Cell>();
                currentCell.UnMarkCell();
                MoveInToCell();
            }
        }
        else if ( other.CompareTag(database.tags.player) || other.CompareTag(database.tags.guard))
        {
            
        }
        else if (other.CompareTag(database.tags.cellInside))
        {
            if (other.transform.parent.GetComponent<Cell>() == currentCell)
            {
                currentCell.IsPrisonerInside(true);
            }
            other.transform.parent.GetComponent<Cell>().AddPrisonerInTheCell(this);
        }
        else if (other.CompareTag(database.tags.yard))
        {
            if (prisonerStage == PrisonerStage.MovingToYard)
            {
                targetActionField = mapManager.PickActionField(prisonerStage);
                RemoveYourSelfFromGuardList();
                prisonerStage = PrisonerStage.MovingToSeatAtYard;
                movingType = MovingType.canMoveAlone;
                MoveToActionField();
            }
        }
        else if (other.CompareTag(database.tags.cafeteria))
        {
            if (prisonerStage == PrisonerStage.MovingToCafeteria)
            {
                targetActionField = mapManager.PickActionField(prisonerStage);
                RemoveYourSelfFromGuardList();
                prisonerStage = PrisonerStage.MovingToSeatAtCafeteria;
                movingType = MovingType.canMoveAlone;
                MoveToActionField();
            }
        }
        else if (other.CompareTag(database.tags.triggerField))
        {
            myAgent.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(database.tags.cellInside))
        {
            if (other.transform.parent.GetComponent<Cell>() == currentCell)
            {
                if (currentCell.currentPrisoner != null && currentCell.currentPrisoner == this)
                {
                    currentCell.IsPrisonerInside(false);
                }
            }
            other.transform.parent.GetComponent<Cell>().RemovePrisonerFromInTheCell(this);
        }
    }
    
    #region Animations

    private void AnimWalk()
    {
        anim.SetBool(database.prisonerData.animParam.Walk, true);
        anim.SetBool(database.prisonerData.animParam.Idle, false);
    }

    private void AnimIdle()
    {
        anim.SetBool(database.prisonerData.animParam.Walk, false);
        anim.SetBool(database.prisonerData.animParam.Idle,true);
    }

    private void AnimWaitingForNeedInCell()
    {
        anim.SetBool(database.prisonerData.animParam.Cell, true);
        anim.SetFloat(database.prisonerData.animParam.Blend, 3);
    }

    private void AnimChangeLayerWeight(float weight)
    {
        anim.SetLayerWeight(1, weight);
        if (weight == 0)
        {
            if (handCuff.activeInHierarchy)
            {
                particleChange.transform.position = handCuff.transform.position;
                particleChange.SetActive(true);
            }
            handCuff.SetActive(false);
            shackles.SetActive(false);
        }
        else
        {
            if (!handCuff.activeInHierarchy)
            {
                particleChange.transform.position = handCuff.transform.position;
                particleChange.SetActive(true);
            }
            handCuff.SetActive(true);
            shackles.SetActive(true);
        }
    }

    private void AnimStartCellAction(float value)
    {
        /*
         *  0 Push Up
         *  1 Slow Walk
         *  2 Sit Up
         */
        anim.SetBool(database.prisonerData.animParam.Cell, true);
        anim.SetFloat(database.prisonerData.animParam.Blend, value);
    }

    private void AnimStopCellAction()
    {
        anim.SetBool(database.prisonerData.animParam.Cell, false);
    }

    private void AnimStartYardAction(float value)
    {
        /*
         *  0 Push Up
         *  1 Sit Up
         *  2 Punching
         *  3 BasketBall
         *  4 Dumbell
         */
        AnimWalk();
        anim.SetBool(database.prisonerData.animParam.Yard, true);
        anim.SetFloat(database.prisonerData.animParam.Blend, value);
        AnimChangeLayerWeight(0);
    }

    private void AnimStopYardAction()
    {
        anim.SetBool(database.prisonerData.animParam.Yard,false);
        AnimChangeLayerWeight(100);
    }

    private void AnimStartSittingActions(float value)
    {
        /*
         *  0 Stand To Sit
         *  1 Sitting Idle
         *  2 Sitting To Stand
         *  3 Eating
         */
        AnimWalk();
        AnimChangeLayerWeight(0);
        anim.SetBool(database.prisonerData.animParam.Sit, true);
        anim.SetFloat(database.prisonerData.animParam.Blend, value);
    }

    private void AnimStopSittingActions()
    {
        anim.SetBool(database.prisonerData.animParam.Sit, false);
        AnimChangeLayerWeight(1);
    }
    
    
    #endregion
}
