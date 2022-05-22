using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cell : MonoBehaviour
{
    public bool isCellActive;
    public bool isCellHavePrisoner;
    public bool isPlayerInside;
    public bool isPrisonerInside;
    
    [Header("Prison Index")]
    //
    public int prisonIndex;
    
    [Header("Cell Game Objects")]
    //
    [Tooltip("Main Parts Gonna Show When Cell Active")]
    [SerializeField] private GameObject mainBody;
    
    #region Transforms
    [Header("Cell Transfrom Points")]
    public Transform cellSpawnPoint;
    public Transform cellWanderPointFirst;
    public Transform cellWanderPointSecond;
    public Transform cellWaitingPoint;
    #endregion

    [Header("Animator")]
    //
    [SerializeField] private Animator anim;
    
    [Space]
    public PrisonerController currentPrisoner;

    #region UI Elements
    [Header("UI Elements")]
    //
    [SerializeField] private Canvas canvas;
    [SerializeField] private TextMeshProUGUI cashText;
    [SerializeField] private Image image;
    [SerializeField] private Image moneyImage;
    [SerializeField] private GameObject InActiveObjects;
    [SerializeField] private GameObject ActiveObjects;
    #endregion

    #region Colliders
    [Header("Cell Colliders")]
    //
    [SerializeField] private BoxCollider colliderBuy;
    [SerializeField] public  BoxCollider colliderActive;
    [SerializeField] private BoxCollider colliderInside;
    #endregion

    [SerializeField]
    public List<PrisonerController> prisonersInTheCell = new List<PrisonerController>();
    public int prisonerInTheCellCount;

    [Header("Mark Variables")]
    //
    public GuardController markerGuard;
    public bool isMarked;

    private DataBase dataBase;
    private GameData gameData;

    [Header("Arrow")] [SerializeField] private GameObject cellArrow;

    private void OnDisable()
    {
        EventManager.TutorialPrisonelPicked -= ActivateCellArrow;
    }

    private void Awake()
    {
        AwakeMethods();
        EventManager.TutorialPrisonelPicked += ActivateCellArrow;
    }
    
    
    #region Awake Methods

    private void AwakeMethods()
    {
        AddYourSelf();
    }

    private void AddYourSelf()
    {
        if (!ObjectManager.cells.Contains(this))
        {
            ObjectManager.cells.Add(this);
        }
    }
    #endregion

    private void Start()
    {
        StartMethods();
    }
    
    #region StartMethods

    private void StartMethods()
    {
        GetDataBase();
        GeTGameData();
        //StartCoroutine(DelayedChangeText());
    }

    private void GetDataBase()
    {
        dataBase = ObjectManager.DataBase;
    }

    private void GeTGameData()
    {
        gameData = ObjectManager.GameData;
    }
    
    #endregion

    public void CellPicked(PrisonerController prisoner)
    {
        currentPrisoner = prisoner;
    }

    public void ActivateCell(bool _isCellHavePrisoner, bool byPlayer)
    {
        ActivateCell(_isCellHavePrisoner);
        
        EventManager.BuyAction();
    }

    public void ActivateCell(bool _isCellHavePrisoner)
    {
        isCellActive = true;
        mainBody.SetActive(true);
        ActiveObjects.SetActive(true);
        InActiveObjects.SetActive(false);
        isCellHavePrisoner = _isCellHavePrisoner;
        dataBase.cellData.cellInfos[prisonIndex] = 1;
        colliderActive.enabled = true;
        colliderBuy.enabled = false;
        colliderInside.enabled = true;
        if (isCellHavePrisoner)
        {
            AnimCloseTheDoor();
        }
        else
        {
            AnimOpenTheDoor();
            image.sprite = gameData.targetSprites.prisoner;
        }
        
        
        DataManager.SaveData(gameData);
        DataManager.SaveData(dataBase);


    }

    public void DeActivateCell()
    {
        isCellActive = false;
        mainBody.SetActive(false);
        ActiveObjects.SetActive(false);
        InActiveObjects.SetActive(true);
        colliderActive.enabled = false;
        colliderBuy.enabled = true;
        colliderInside.enabled = false;
        StartCoroutine(DelayedChangeText());
    }

    public void AnimOpenTheDoor()
    {
        if (dataBase == null)
        {
            dataBase = ObjectManager.DataBase;
        }
        anim.SetBool(dataBase.cellData.animParam.doorClose,false);
        anim.SetBool(dataBase.cellData.animParam.doorOpen, true);
    }

    public void AnimCloseTheDoor()
    {
        if (dataBase == null)
        {
            dataBase = ObjectManager.DataBase;
        }
        anim.SetBool(dataBase.cellData.animParam.doorOpen, false);
        anim.SetBool(dataBase.cellData.animParam.doorClose, true);
    }

    public void ChangeUIState(PrisonerController.PrisonerStage prisonerState)
    {
        if (prisonerState == PrisonerController.PrisonerStage.WaitingForCloths)
        {
            image.fillAmount = 1;
            image.gameObject.SetActive(true);
            image.sprite = gameData.targetSprites.cloth;
        }
        else if (prisonerState == PrisonerController.PrisonerStage.WaitingToMoveCafeteria)
        {
            image.gameObject.SetActive(true);
            image.fillAmount = 1;
            image.sprite = gameData.targetSprites.food;
            
        }
        else if (prisonerState == PrisonerController.PrisonerStage.WaitingToMoveYard)
        {
            image.fillAmount = 1;
            image.gameObject.SetActive(true);
            image.sprite = gameData.targetSprites.yard;
        }
        else if(prisonerState == PrisonerController.PrisonerStage.WaitingAtCell)
        {
            image.gameObject.SetActive(true);
            image.sprite = gameData.targetSprites.handcuff;
            //image.gameObject.SetActive(false);
        }
    }

    IEnumerator DelayedChangeText()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(2f);
        ChangeCostText();
    }

    public void ChangeCostText()
    {
        string targetText = "";
        int cost = dataBase.cellData.cellCosts[prisonIndex];
        if (cost > 1000)
        {
            targetText = (cost / 1000).ToString() + "." + ((cost % 1000) / 100).ToString() + " K";
        }
        else if (cost < 1000)
        {
            targetText = cost.ToString();
        }

        cashText.text = targetText;

        moneyImage.fillAmount = Mathf.InverseLerp(0, dataBase.cellData.cellCostDefault[prisonIndex],
            dataBase.cellData.cellCosts[prisonIndex]);
    }

    public void DecreaseFillAmount(float decreaseValue)
    {
        image.fillAmount -= decreaseValue;
    }

    public void ResetFillAmount()
    {
        image.fillAmount = 1;
    }

    public void ChangeFillAmount(float value)
    {
        image.gameObject.SetActive(true);
        image.sprite = gameData.targetSprites.handcuff;
        image.fillAmount = value;
    }

    public void IsPlayerInside(bool _isPlayerInside)
    {
        isPlayerInside = _isPlayerInside;
    }

    public void IsPrisonerInside(bool _isPrisionerInside)
    {
        isPrisonerInside = _isPrisionerInside;
    }

    public void AddPrisonerInTheCell(PrisonerController prisoner)
    {
        if (!prisonersInTheCell.Contains(prisoner))
        {
            prisonersInTheCell.Add(prisoner);
        }

        prisonerInTheCellCount = prisonersInTheCell.Count;
    }

    public void RemovePrisonerFromInTheCell(PrisonerController prisoner)
    {
        if (prisonersInTheCell.Contains(prisoner))
        {
            prisonersInTheCell.Remove(prisoner);
        }

        prisonerInTheCellCount = prisonersInTheCell.Count;
    }

    public void ResetCellPrisoner()
    {
        currentPrisoner = null;
        AnimOpenTheDoor();
        //image.gameObject.SetActive(false);
        image.sprite = gameData.targetSprites.prisoner;
        image.fillAmount = 1;
    }

    public void MarkCell(GuardController marker)
    {
        isMarked = true;
        markerGuard = marker;
    }

    public void UnMarkCell()
    {
        isMarked = false;
        if (markerGuard != null)
        {
            markerGuard.CellUnMarked(this);
        }
        markerGuard = null;
    }

    private void ActivateCellArrow()
    {
        if (currentPrisoner == null && cellArrow != null && isCellActive)
        {
            cellArrow.SetActive(true);
        }
    }

    public void ActivateCellArrowFromPrisoner()
    {
        cellArrow.SetActive(true);
    }

    public void DeActivateCellArrow()
    {
        cellArrow.SetActive(false);
    }
}
