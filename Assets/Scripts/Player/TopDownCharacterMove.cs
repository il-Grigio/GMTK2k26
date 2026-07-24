using UnityEngine;

public class TopDownCharacterMove : MonoBehaviour
{
    private InputHandler _input;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private Animator anim;
    private Camera cam;

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
        else
        {
            if (anim != null)
                anim.SetBool("IsMoving", false);
        }
    }

    private void UpdateAnimator(Vector3 targetVector)
    {
        if (anim == null) return;

        bool isMoving = targetVector.sqrMagnitude > 0.001f;
        anim.SetBool("IsMoving", isMoving);
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
