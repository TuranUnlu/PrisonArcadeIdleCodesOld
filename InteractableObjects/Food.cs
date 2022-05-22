using System.Collections;
using UnityEngine;

public class Food : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer smr;

    public void DecreaseFoodAmount(float value)
    {
        smr.SetBlendShapeWeight(0, value);
    }
}
