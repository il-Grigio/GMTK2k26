using FMOD.Studio;
using UnityEngine;
using FMOD.Studio;
public class TopDownCharacterMove : MonoBehaviour
{
    private InputHandler _input;
    private EventInstance playerFootsteps;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private Animator anim;
    [SerializeField] private float animDampTime = 0.1f;

    private bool isWalking;
    private Camera cam;

    private static readonly int MoveX = Animator.StringToHash("MoveX");
    private static readonly int MoveY = Animator.StringToHash("MoveY");

    private void Awake()
    {
        _input = InputHandler.Instance;
    }

    private void Start()
    {
        cam = Camera.main;
        playerFootsteps = AudioManager.Instance.CreateInstance(FMODEventsManager.Instance.playerFootstepsSFX);
    }

    void Update()
    {
        var targetVector = new Vector3(_input.InputVector.x, 0, _input.InputVector.y);

        if (_input.CurrentState == InputHandler.State.Game)
        {
            isWalking = targetVector.sqrMagnitude > 0.001f;

            MoveTowardTarget(targetVector);
            RotateTowardsMouseVector();
            UpdateAnimator(targetVector);
        }
        else if (anim != null)
        {
            anim.SetFloat(MoveX, 0f, animDampTime, Time.deltaTime);
            anim.SetFloat(MoveY, 0f, animDampTime, Time.deltaTime);
        }

        UpdateSound();
    }

    private void UpdateAnimator(Vector3 inputVector)
    {
        if (anim == null) return;

        if (inputVector.sqrMagnitude < 0.001f)
        {
            anim.SetFloat(MoveX, 0f, animDampTime, Time.deltaTime);
            anim.SetFloat(MoveY, 0f, animDampTime, Time.deltaTime);
            return;
        }

        Vector3 localMove = transform.InverseTransformDirection(inputVector.normalized);

        Vector2 dir = new Vector2(localMove.x, localMove.z);

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            dir = new Vector2(Mathf.Sign(dir.x), 0f);
        }
        else
        {
            dir = new Vector2(0f, Mathf.Sign(dir.y));
        }

        anim.SetFloat(MoveX, dir.x, animDampTime, Time.deltaTime);
        anim.SetFloat(MoveY, dir.y, animDampTime, Time.deltaTime);
    }

    private void RotateTowardsMouseVector()
    {
        Ray ray = cam.ScreenPointToRay(_input.MousePosition);

        if (Physics.Raycast(ray, out RaycastHit hitInfo, 300f))
        {
            var target = hitInfo.point;
            target.y = transform.position.y;

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(target - transform.position),
                rotateSpeed * Time.deltaTime
            );
        }
    }

    private void MoveTowardTarget(Vector3 targetVector)
    {
        var speed = moveSpeed * Time.deltaTime;
        var targetPosition = transform.position + targetVector * speed;
        transform.position = targetPosition;
    }

    private void UpdateSound()
    {
        if (isWalking)
        {
            PLAYBACK_STATE playbackState;
            playerFootsteps.getPlaybackState(out playbackState);
            if (playbackState.Equals(PLAYBACK_STATE.STOPPED))
            {
                playerFootsteps.start();
            }
            else
            {
                playerFootsteps.stop(STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
}