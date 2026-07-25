using System;
using System.Collections;
using UnityEngine;

public class PlayerDuel : MonoBehaviour
{
    [SerializeField] AnimationCurve stepSpeedCurve;
    [SerializeField] private Transform playerShootRotation;
    [SerializeField] private GameObject gunHand;
    [SerializeField] private GameObject gunLeg;
    
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
        manager.OnLose -= Lose;
        manager.OnLose += Lose;
        gunHand.SetActive(false);
        gunLeg.SetActive(true);
    }

    private void OnDisable()
    {
        manager.OnLose -= Lose;
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
        transform.rotation = playerShootRotation.rotation;
        animator.SetTrigger("Shoot");
        gunHand.SetActive(true);
        gunLeg.SetActive(false);
    }

    void ShootBullet()
    {
        Bullet bullet = BulletManager.Instance.GetObject();
        bullet.transform.position = gunHand.transform.position;
        bullet.transform.forward = Vector3.back;
        bullet.Setup(Vector3.back, 25);
    }
    void Lose()
    {
        animator.SetTrigger("Lose");
    }
}
