using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FoodDeliveryPoint : MonoBehaviour
{
    [Header("Sit Points")]
    public List<ActionFields> sitPoints = new List<ActionFields>();

    [Header("UI Elements")]
    //
    [SerializeField]
    private Image image;

    public List<PrisonerController> hungryPrisoners = new List<PrisonerController>();

    public bool isUsing;

    public void DecreaseFillAmount(float value)
    {
        image.fillAmount -= value;
    }

    public void ResetFillAmount()
    {
        image.fillAmount = 1;
    }

    public void SetUsingCondition(bool condition)
    {
        isUsing = condition;
    }

}
