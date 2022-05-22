using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Resource : MonoBehaviour
{
    public enum ResourceType
    {
        food,
        cloth
    }

    public ResourceType resourceType;

    [SerializeField] private List<GameObject> resourceModels;

    public int resourceCount;
    private float counter;

    public Transform colliderField;

    private DataBase dataBase;
    private MapManager mapManager;
    private DataBase.ResourceData targetResourceData;
    
    
    // Start is called before the first frame update
    void Start()
    {
        StartMethods();
    }
    
    #region Start Methods

    private void StartMethods()
    {
        GetDataBase();
        SetTargetResourceData();
        GetMapManager();
        AddYourSelfToMapManager();
    }
    private void GetDataBase()
    {
        dataBase = ObjectManager.DataBase;
    }
    private void SetTargetResourceData()
    {
        if (resourceType == ResourceType.cloth)
        {
            targetResourceData = dataBase.resourceLoundry;
        }
        else if (resourceType == ResourceType.food)
        {
            targetResourceData = dataBase.resourceFood;
        }
    }
    private void GetMapManager()
    {
        mapManager = ObjectManager.MapManager;
    }
    private void AddYourSelfToMapManager()
    {
        mapManager.AddResource(this);
    }
    
    #endregion

    // Update is called once per frame
    void Update()
    {
        if (counter >= targetResourceData.creationTime)
        {
            if (resourceCount < targetResourceData.maxStackCount)
            {
                counter = 0;
                IncreaseResourceCount();
            }
        }
        else
        {
            counter += Time.deltaTime;
        }
    }

    public void DecreaseResourceCount()
    {
        resourceCount--;
        resourceModels[resourceCount].SetActive(false);
    }

    public void IncreaseResourceCount()
    {
        resourceCount++;
        resourceModels[resourceCount -1].SetActive(true);
    }
}
