
using System;
using UnityEngine;
using UnityEngine.Events;

public class MenuButton : MonoBehaviour
{
    [SerializeField] private Material materialBase;
    [SerializeField] private Material materialHover;

    [SerializeField] private LayerMask layerButtons;
    private Camera cam;

    [SerializeField] private UnityEvent onClick;
    [SerializeField] private UnityEvent spinLeft;
    [SerializeField] private UnityEvent spinRight;

    private Collider collider;

    private bool hitRight;
    MeshRenderer meshRenderer;
    InputHandler inputHandler;

    bool _isHover;

    bool isHover
    {
        get => _isHover;
        set
        {
            _isHover = value;
            SwitchMaterial(_isHover);
        }
    }

    private void Awake()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        inputHandler = InputHandler.Instance;
        collider = GetComponent<Collider>();
    }

    private void OnEnable()
    {
        inputHandler.OnMenuAction -= OnClickMenu;
        inputHandler.OnMenuAction += OnClickMenu;
        collider.enabled = true;
    }

    private void OnDisable()
    {
        inputHandler.OnMenuAction -= OnClickMenu;
    }

    private void Start()
    {
        cam = Camera.main;
    }

    public void SwitchMaterial(bool isHover)
    {
        meshRenderer.material = isHover ? materialHover : materialBase;
    }

    private void Update()
    {
        Hover();
    }

    private void Hover()
    {
        GameObject newHover = null;

        Ray ray = cam.ScreenPointToRay(inputHandler.UIPointerPosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, layerButtons))
        {
            isHover = hit.collider.gameObject == gameObject;
            if (isHover)
            {
                Vector3 localPoint = transform.InverseTransformPoint(hit.point);

                hitRight = localPoint.x > 0;
            }
        }
        else
        {
            isHover = false;
        }
    }

    private void OnClickMenu()
    {
        if (isHover)
        {
            onClick?.Invoke();
            if (hitRight)
                spinRight?.Invoke();
            else
                spinLeft?.Invoke();
            isHover = false;
            collider.enabled = false;
            AudioManager.Instance.PlayOneShot(FMODEventsManager.Instance.shootSound, transform.position);
            //Invoke(nameof(EnableCollider), 1f); 
        }
    }

    public void EnableCollider()
    {
        collider.enabled = true;
    }
    public void DisableCollider()
    {
        collider.enabled = false;
    }
}
