using System;
using System.Collections;
using Grigios;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class DuelManager : Grigios.Singleton<DuelManager>
{
    [SerializeField] private GameObject npc;
    [SerializeField] private GameObject player;

    [SerializeField] Transform npcTargetPosition;
    [SerializeField] Transform playerTargetPosition;

    [SerializeField] CinemachineCamera indoorCamera;
    [SerializeField] private GameObject mainMenu;
    
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

    [SerializeField] GameObject playerUi;
    [SerializeField] GameObject tutorialUI;
    private InputHandler _input;
    private int countdown;
    private Coroutine duelCoroutine;

    private bool stepForwardInput;
    private bool canStepForward;
    private bool shootInput;
    private bool _canShoot;

    private bool inputEnabled;

    private bool canShoot
    {
        get => _canShoot;
        set
        {
            _canShoot = value;
            Debug.Log("Can Shoot: " + _canShoot);
        }
    }

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
    }

    private void OnDisable()
    {
        _input.OnShoot -= Shoot;
        _input.OnStepForward -= StepForward;
        StopDuelCoroutine();
        isDuelActive = false;
    }

    public void PrepareDuel()
    {
        mainMenu.gameObject.SetActive(false);
        inputEnabled = true;
        AudioManager.Instance.SetMusicState(4);
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
        playerUi.SetActive(false);
        tutorialUI.SetActive(true);
        TimerSystem.Instance.PauseTimer();
    }
    
    private void StartDuel()
    {
        tutorialUI.SetActive(false);
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
        while (countdown > 1)
        {
            duelCameraSwitcher.SwitchCamera();
            yield return new WaitForSeconds(1.5f);
            yield return new WaitForSeconds(Random.Range(minReactionDelay, maxReactionDelay));

            if (!isDuelActive) yield break;

            countdown--;
            
            OnCountSign?.Invoke(countdown);
            yield return new WaitForSeconds(0.2f);
            Debug.Log("Duel Countdown: " + countdown);
            OnCountDown?.Invoke(countdown);

            stepForwardInput = false;
            yield return WaitForPlayerInput(
                hasInput: () => stepForwardInput,
                enableInput: () => canStepForward = true,
                disableInput: () => canStepForward = false);
            
            if (!isDuelActive) yield break;
        }

        if (!isDuelActive) yield break;
        duelCameraSwitcher.SwitchCamera();
        yield return new WaitForSeconds(1.5f);
        yield return new WaitForSeconds(Random.Range(minReactionDelay, maxReactionDelay));

        if (!isDuelActive) yield break;

        countdown--;
        OnCountSign?.Invoke(countdown);
        yield return new WaitForSeconds(0.2f);
        Debug.Log("Duel Countdown SHOOT: " + countdown);
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
        if (!inputEnabled) return;
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
        if (!inputEnabled) return;
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
        playerUi.SetActive(true);
        if (!isDuelActive) return;
        inputEnabled = false;
        isDuelActive = false;
        StopDuelCoroutine();
        OnWin?.Invoke();
        Debug.Log("Duel Win");
        AudioManager.Instance.SetMusicState(0);
        duelCameraSwitcher.EndDuel();
        PointSystem.Instance.DoubleScore();

        if(TimerSystem.Instance.GetTimer() <= 0)
        {
            PointSystem.Instance.DoubleScore();
            FinishGame();
        }
        else
        {
            TimerSystem.Instance.ResumeTimer();
            SaloonManager.Instance.SetHeat(0);
        }
    }

    private void Lose()
    {
        if (!isDuelActive) return;
        inputEnabled = false;
        isDuelActive = false;
        StopDuelCoroutine();
        OnLose?.Invoke();
        Debug.Log("Duel Lost");
        
        AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.shootSound, player.transform.position);
        duelCameraSwitcher.EndDuel();
        FinishGame();
    }

    private void FinishGame()
    {
        mainMenu.gameObject.SetActive(true);
        indoorCamera.Priority = 5;
        GameOver.Instance.FinishGame();
        duelCameraSwitcher.SwitchToLeaderboardCamera();
        AudioManager.Instance.SetMusicState(0);
    }
}