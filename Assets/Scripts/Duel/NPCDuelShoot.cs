using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class NPCDuelShoot : MonoBehaviour
{
    [SerializeField] AnimationCurve stepSpeedCurve;
    [SerializeField] private Transform playerShootRotation;

    bool alive = true;
    Animator animator;
    DuelManager manager;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        manager = DuelManager.Instance;
        
    }
    void OnEnable()
    {
        manager.OnCountDown -= CountDown;
        manager.OnCountDown += CountDown;
        manager.OnWin -= Killed;
        manager.OnWin += Killed;
        alive = true;
    }

    private void OnDisable()
    {
        manager.OnWin -= Killed;
        manager.OnCountDown -= CountDown;
    }

    private void CountDown(int i)
    {
        if (i > 0)
        {
            Step();
        }
        else
        {
            Invoke(nameof(Shoot), Random.Range(.35f, .7f));
        }
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

    private void Shoot()
    {
        if (!alive) return;
        transform.rotation = playerShootRotation.rotation;
        animator.SetTrigger("Shoot");
        Debug.Log("NPC Shoot");
        manager.OnNPCShoot();
    }
    private void Step()
    {
        animator.SetTrigger("Walk");
        StartCoroutine(Walk());
    }
    void Killed()
    {
        alive = false;
        animator.SetTrigger("Lose");
    }
}
