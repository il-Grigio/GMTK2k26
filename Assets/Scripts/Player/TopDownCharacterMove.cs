using UnityEngine;

public class TopDownCharacterMove : MonoBehaviour
{
    private InputHandler _input;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private Animator anim;
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
    }

    void Update()
    {
        var targetVector = new Vector3(_input.InputVector.x, 0, _input.InputVector.y);

        if (_input.CurrentState == InputHandler.State.Game)
        {
            MoveTowardTarget(targetVector);
            RotateTowardsMouseVector();
            UpdateAnimator(targetVector);
        }
        else if (anim != null)
        {
            anim.SetFloat(MoveX, 0f);
            anim.SetFloat(MoveY, 0f);
        }
    }

    private void UpdateAnimator(Vector3 inputVector)
    {
        if (anim == null) return;

        Vector2 dir = new Vector2(inputVector.x, inputVector.z);

        if (dir.sqrMagnitude < 0.001f)
        {
            anim.SetFloat(MoveX, 0f);
            anim.SetFloat(MoveY, 0f);
            return;
        }

        dir.Normalize();

        anim.SetFloat(MoveX, dir.x);
        anim.SetFloat(MoveY, dir.y);
    }


    private void RotateTowardsMouseVector()
    {
        Ray ray = cam.ScreenPointToRay(_input.MousePosition);

        if(Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f))
        {
            var target = hitInfo.point;
            target.y = transform.position.y;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(target - transform.position), rotateSpeed * Time.deltaTime);
            //transform.LookAt(target);
        }
    }


    private void MoveTowardTarget(Vector3 targetVector)
    {
        var speed = moveSpeed * Time.deltaTime;

        var targetPosition = transform.position + targetVector * speed;
        transform.position = targetPosition;
    }
}
