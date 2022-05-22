using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrisonerSpawner : MonoBehaviour
{
    [Header("Transforms")]
    //
    [SerializeField] public List<Transform> targetPoints = new List<Transform>();
    [SerializeField]
    private List<int> infos = new List<int>();
    [SerializeField]
    private List<Cell> cells = new List<Cell>();

    private MapManager mapManager;
    private DataBase dataBase;
    private GameData gameData;

    private GameObject tempPrisoner;

    [SerializeField]
    private int activeCellCount = 0;
    private int pointIndex = 0;

    private int prisonerSpawnCount = 0;


    private void Awake()
    {
        AwakeMethods();
    }
    
    #region AwakeMethods

    private void AwakeMethods()
    {
        AddYourSelfToObjectManger();
    }

    private void AddYourSelfToObjectManger()
    {
        ObjectManager.prisonerSpawner = this;
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
        GetGameData();
        GetDataBase();
        GetMapManager(); 
        ArrangeInfosSize();
        StartCoroutine(SpawnPrisonerRoutine());
    }

    private void GetGameData()
    {
        gameData = ObjectManager.GameData;
    }

    private void GetDataBase()
    {
        dataBase = ObjectManager.DataBase;
    }

    private void GetMapManager()
    {
        mapManager = ObjectManager.MapManager;
    }

    private void ArrangeInfosSize()
    {
        infos.Clear();
        for (int i = 0; i < targetPoints.Count; i++)
        {
            infos.Add(0);
        }
    }
    
    #endregion


    IEnumerator SpawnPrisonerRoutine()
    {
        yield return new WaitForSeconds(5);
        GetCells();
        while (true)
        {
            yield return new WaitForSeconds(5);

            if (dataBase.isTutorialPassed)
            {
                //*
                //***
                //      Can Be Improved With Event or Triggered Methods
                //***
                //*
                activeCellCount = 0;
                for (int i = 0; i < cells.Count; i++)
                {
                    if (cells[i].isCellActive)
                    {
                        activeCellCount++;
                    }
                }

                if (mapManager.prisonerList.Count < activeCellCount)
                {
                    SpawnPrisoner();
                }
            }
        }
    }

    private void GetCells()
    {
        cells = mapManager.cells;
    }

    private void SpawnPrisoner()
    {
        GetEmptyIndex();

        while (pointIndex != -1 && activeCellCount > mapManager.prisonerList.Count)
        {
            infos[pointIndex] = 1;
            //tempPrisoner = Instantiate(gameData.prisonerPrefab, targetPoints[pointIndex].position, Quaternion.identity);
            prisonerSpawnCount++;
            tempPrisoner = Instantiate(gameData.prisonerPrefab, transform.position, Quaternion.identity);
            tempPrisoner.GetComponent<PrisonerController>().SetSpawnIndex(pointIndex);
            tempPrisoner.name = "Prisoner " + prisonerSpawnCount.ToString();
            GetEmptyIndex();
        }
    }

    private void GetEmptyIndex()
    {
        pointIndex = -1;
        for (int i = 0; i < infos.Count; i++)
        {
            if (infos[i] == 0)
            {
                pointIndex = i;
                break;
            }
        }
    }

    public void ClearTheIndex(int value)
    {
        infos[value] = 0;
    }

}
