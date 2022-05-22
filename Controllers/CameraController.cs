using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    GameObject Target;
    private GameManager GameManager;
    public Vector3 Dif;

    private bool isTutorialChangeHappen;
    private Transform TutorialTarget;

    private void Awake()
    {
        ObjectManager.CameraManager = this;
    }
    void Start()
    {
        GameManager = ObjectManager.GameManager;
    }

    void Update()
    {

    }
    private void LateUpdate()
    {
        if (GameManager.gameState == GameManager.GameStates.Start)
        {
            if (TutorialTarget != null)
            {
                transform.position = Vector3.MoveTowards(transform.position, TutorialTarget.position + Dif, 25f * Time.deltaTime);
            }
            else if (Target != null)
            {
                Follow();
                Look();
            }
        }
    }

    private void OnEnable()
    {
        EventManager.Target += SetTarget;
        EventManager.TutorialCameraChange += TutorialCameraAction;
    }

    private void OnDisable()
    {
        EventManager.Target -= SetTarget;
        EventManager.TutorialCameraChange -= TutorialCameraAction;
    }

    void SetTarget(GameObject target)
    {
        Target = target;
    }

    void Follow()
    {
        //transform.position = Vector3.MoveTowards(transform.position, Target.transform.position + Dif, 25f * Time.deltaTime);
        transform.position = Target.transform.position + Dif;
    }

    void Look()
    {
        transform.LookAt(Target.transform);
        //transform.eulerAngles = new Vector3(transform.eulerAngles.x, 0f, 0f);
    }

    void TutorialCameraAction()
    {
        if (!isTutorialChangeHappen)
        {
            isTutorialChangeHappen = true;
            TutorialTarget = ObjectManager.MapManager.resourceCloth.transform;
            StartCoroutine(DelayedChange());
        }
    }

    IEnumerator DelayedChange()
    {
        ObjectManager.PlayerManager.ChangeTutorialSceneStatus(true);
        yield return new WaitForSeconds(3f);
        TutorialTarget = ObjectManager.Player.transform;
        yield return new WaitForSeconds(1.5f);
        TutorialTarget = null;
        ObjectManager.PlayerManager.ChangeTutorialSceneStatus(false);
    }
}
