using System.Collections;
using UnityEngine;

public class ActionFields : MonoBehaviour
{
    public enum FieldType
    {
        yardField,
        cafeteriaField
    }

    public FieldType fieldType;

    public enum TargetYardAnimation
    {
        PushUp,
        SitUp,
        BasketBall,
        Boxing,
        Dumbell,
        Sitting
    }

    public TargetYardAnimation targetYardAnimation;

    public Transform waitingField;

    public int actionValue;

    public FoodDeliveryPoint targetFoodDeliveryPoint;

    public PrisonerController currentPrisoner;

    private MapManager mapManager;
    
    // Start is called before the first frame update
    void Start()
    {
        StartMethods();
    }
    #region Start Methods

    private void StartMethods()
    {
        GetMapManager();
        AddYourSelf();
    }

    private void GetMapManager()
    {
        mapManager = ObjectManager.MapManager;
    }

    private void AddYourSelf()
    {
        mapManager.AddField(fieldType, this);
    }
    
    #endregion

    public void ActionFieldUsuable()
    {
        AddYourSelf();
    }

    public void AddPrisonerToFoodDeliveryPoint(PrisonerController targetPrisoner)
    {
        currentPrisoner = targetPrisoner;
        targetFoodDeliveryPoint.hungryPrisoners.Add(currentPrisoner);
    }

    public void RemovePrisonerFromDeliveryPoint()
    {
        if (targetFoodDeliveryPoint.hungryPrisoners.Contains(currentPrisoner))
        {
            targetFoodDeliveryPoint.hungryPrisoners.Remove(currentPrisoner);
        }
    }
}
