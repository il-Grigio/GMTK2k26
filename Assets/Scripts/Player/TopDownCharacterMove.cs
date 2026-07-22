using System;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private inputHandler _input;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private bool rotateTowardsMouse;
    private Camera cam;

    private void Awake()
    {
        _input = GetComponent<inputHandler>();
    }

    private void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        var targetVector = new Vector3(_input.InputVector.x, 0, _input.InputVector.y);
        var movementVector = MoveTowardTarget(targetVector);

        if (!rotateTowardsMouse)
            RotateTowardMovementVect(movementVector);
        else
            RotateTowardsMouseVector();
    }

    private void RotateTowardsMouseVector()
    {
        Ray ray = cam.ScreenPointToRay(_input.MousePosition);

        if(Physics.Raycast(ray, out RaycastHit hitInfo, maxDistance: 300f))
        {
            var target = hitInfo.point;
            target.y = transform.position.y;
            transform.LookAt(target);
        }
    }

    private void RotateTowardMovementVect(Vector3 movementVector)
    {
        if (movementVector.magnitude == 0)
            return;
        var rotation = Quaternion.LookRotation(movementVector);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, rotateSpeed);
    }

    private Vector3 MoveTowardTarget(Vector3 targetVector)
    {
        var speed = moveSpeed * Time.deltaTime;

        // se voglio che il personaggio vada verso la direzione del mouse
        targetVector = transform.TransformDirection(targetVector);

        // muove il personaggio rispetto alla rotazione della telecamera

        // targetVector = Quaternion.Euler(0, cam.gameObject.transform.eulerAngles.y, 0) * targetVector;

        var targetPosition = transform.position + targetVector * speed;
        transform.position = targetPosition;
        return targetVector;
    }
}
