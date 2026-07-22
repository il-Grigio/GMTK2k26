using System;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private inputHandler _input;
    [SerializeField] private float moveSpeed;
    [SerializeField] private Camera cam;


    private void Awake()
    {
        _input = GetComponent<inputHandler>();
    }

    void Update()
    {
        var targetVector = new Vector3(_input.InputVector.x, 0, _input.InputVector.y);

        // si muove nella direzione che miriamo
        MoveTowardTarget(targetVector);

       //RotateTowardMovementVect();
    }

    private void MoveTowardTarget(Vector3 targetVector)
    {
        var speed = moveSpeed * Time.deltaTime;
        targetVector = Quaternion.Euler(0, cam.gameObject.transform.eulerAngles.y, 0) * targetVector;
        var targetPosition = transform.position + targetVector * speed;
        transform.position = targetPosition;
    }
}
