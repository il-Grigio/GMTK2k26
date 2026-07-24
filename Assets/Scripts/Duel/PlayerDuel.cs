using System;
using System.Collections;
using UnityEngine;

public class PlayerDuel : MonoBehaviour
{
    [SerializeField] AnimationCurve stepSpeedCurve;
    Animator animator;
    DuelManager manager;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        manager = DuelManager.Instance;
        
    }

    private void OnEnable()
    {
        manager.OnStepForward -= StepForward;
        manager.OnStepForward += StepForward;
        manager.OnShoot -= Shoot;
        manager.OnShoot += Shoot;
    }

    private void OnDisable()
    {
        manager.OnShoot -= Shoot;
        manager.OnStepForward -= StepForward;
    }


    void StepForward()
    {
        animator.SetTrigger("Walk");
        StartCoroutine(Walk());
    }

    IEnumerator Walk()
    {
        float t = 1.1f;
        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + transform.forward * 1.3f;
        while (t > 0)
        {
            transform.position = Vector3.Lerp(startPos, endPos, stepSpeedCurve.Evaluate(1 - (t / 1.1f)));
            t -= Time.deltaTime;
            yield return null;
        }
    }
    void Shoot()
    {
        animator.SetTrigger("Shoot");
    }
}
