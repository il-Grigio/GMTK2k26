using UnityEngine;

public class TopDownCharacterMove : MonoBehaviour
{
    private InputHandler _input;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    private Camera cam;

    private void Awake()
    {
        _input = GetComponent<InputHandler>();
    }

    private void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        var targetVector = new Vector3(_input.InputVector.x, 0, _input.InputVector.y);
        
        MoveTowardTarget(targetVector);
        RotateTowardsMouseVector();
    }

    private void RotateTowardsMouseVector()
    {
        Ray ray = cam.ScreenPointToRay(_input.MousePosition);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.green);

        if(Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f))
        {
            var target = hitInfo.point;
            Debug.DrawLine(target, target + Vector3.up * 100f, Color.red);
            target.y = transform.position.y;
            Debug.Log(target);
            transform.LookAt(target);
        }
    }


    private void MoveTowardTarget(Vector3 targetVector)
    {
        var speed = moveSpeed * Time.deltaTime;

        var targetPosition = transform.position + targetVector * speed;
        transform.position = targetPosition;
    }
}
