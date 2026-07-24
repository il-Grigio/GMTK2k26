using System;
using System.Collections;
using Grigios;
using UnityEngine;
using Random = UnityEngine.Random;

public class DuelManager : Singleton<DuelManager>
{
    [SerializeField] private GameObject npc;
    [SerializeField] private GameObject player;

    [SerializeField] Transform npcTargetPosition;
    [SerializeField] Transform playerTargetPosition;
    
    [SerializeField] DuelCameraSwitcher duelCameraSwitcher;
    [Header("Timing")]
    [SerializeField] private int countdownStart = 4;
    [SerializeField] private float minReactionDelay = 0.5f;
    [SerializeField] private float maxReactionDelay = 3f;
    [SerializeField] private float inputTimeLimit = 1f;

    public Action<int> OnCountSign;
    public Action<int> OnCountDown;
    public Action OnShoot;
    public Action OnStepForward;
    public Action OnWin;
    public Action OnLose;

    private InputHandler _input;
    private int countdown;
    private Coroutine duelCoroutine;

    private bool stepForwardInput;
    private bool canStepForward;
    private bool shootInput;
    private bool canShoot;

    bool isDuelStarted;
    private bool isDuelActive
    {
        get{ return isDuelStarted;}
        set
        {
            isDuelStarted = value;
            Debug.Log("Duel Active: " + isDuelActive);
        }
    }

    private void Awake()
    {
        _input = InputHandler.Instance;
    }

    private void OnEnable()
    {
        _input.OnShoot -= Shoot;
        _input.OnShoot += Shoot;

        _input.OnStepForward -= StepForward;
        _input.OnStepForward += StepForward;
        Invoke(nameof(PrepareDuel), 0.2f);
    }

    private void OnDisable()
    {
        _input.OnShoot -= Shoot;
        _input.OnStepForward -= StepForward;
        StopDuelCoroutine();
        isDuelActive = false;
    }

    void PrepareDuel()
    {
        countdown = countdownStart;
        Debug.Log("Duel Countdown: " + countdown);
        canStepForward = false;
        canShoot = false;
        stepForwardInput = false;
        shootInput = false;
        player.transform.position = playerTargetPosition.position;
        player.transform.rotation = playerTargetPosition.rotation;
        npc.transform.position = npcTargetPosition.position;
        npc.transform.rotation = npcTargetPosition.rotation;
        duelCameraSwitcher.PrepareDuel();
    }
    
    public void StartDuel()
    {
        isDuelActive = true;
        StopDuelCoroutine();
        duelCoroutine = StartCoroutine(DuelCoroutine());
    }

    private void StopDuelCoroutine()
    {
        if (duelCoroutine != null)
        {
            StopCoroutine(duelCoroutine);
            duelCoroutine = null;
        }
    }

    private IEnumerator DuelCoroutine()
    {
        while (countdown > 0)
        {
            duelCameraSwitcher.SwitchCamera();
            yield return new WaitForSeconds(1.5f);
            yield return new WaitForSeconds(Random.Range(minReactionDelay, maxReactionDelay));

            if (!isDuelActive) yield break;

            countdown--;
        Debug.Log("Duel Countdown: " + countdown);
            
            OnCountSign?.Invoke(countdown);
            yield return new WaitForSeconds(0.2f);
            OnCountDown?.Invoke(countdown);

            stepForwardInput = false;
            yield return WaitForPlayerInput(
                hasInput: () => stepForwardInput,
                enableInput: () => canStepForward = true,
                disableInput: () => canStepForward = false);
            
            if (!isDuelActive) yield break;
        }

        yield return new WaitForSeconds(1.5f);
        yield return new WaitForSeconds(Random.Range(minReactionDelay, maxReactionDelay));

        if (!isDuelActive) yield break;

        countdown--;
        Debug.Log("Duel Countdown SHOOT: " + countdown);
        OnCountSign?.Invoke(countdown);
        yield return new WaitForSeconds(0.2f);
        OnCountDown?.Invoke(countdown);

        shootInput = false;
        yield return WaitForPlayerInput(
            hasInput: () => shootInput,
            enableInput: () => canShoot = true,
            disableInput: () => canShoot = false);
    }

    private IEnumerator WaitForPlayerInput(Func<bool> hasInput, Action enableInput, Action disableInput)
    {
        enableInput();

        float time = inputTimeLimit;
        while (!hasInput())
        {
            time -= Time.deltaTime;
            if (time <= 0f)
            {
                disableInput();
                Lose();
                yield break;
            }
            yield return null;
        }

        disableInput();
    }

    public void OnNPCShoot()
    {
        if (!isDuelActive) return;
        if (!shootInput)
            Lose();
    }

    private void Shoot()
    {
        if (!isDuelActive) return;

        if (canShoot)
        {
            shootInput = true;
            canShoot = false;
            OnShoot?.Invoke();
            Win();
        }
        else
        {
            Lose();
        }
    }

    private void StepForward()
    {
        if (!isDuelActive)
        {
            StartDuel();
            return;
        }

        if (canStepForward)
        {
            stepForwardInput = true;
            canStepForward = false;
            OnStepForward?.Invoke();
        }
        else
        {
            Lose();
        }
    }

    private void Win()
    {
        if (!isDuelActive) return;
        isDuelActive = false;
        StopDuelCoroutine();
        OnWin?.Invoke();
        Debug.Log("Duel Win");
    }

    private void Lose()
    {
        if (!isDuelActive) return;
        isDuelActive = false;
        StopDuelCoroutine();
        OnLose?.Invoke();
        Debug.Log("Duel Lost");
    }
}