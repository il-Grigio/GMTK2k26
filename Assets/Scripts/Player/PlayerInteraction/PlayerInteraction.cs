using UnityEditor.Compilation;
using UnityEngine;

public abstract class PlayerInteraction : MonoBehaviour
{
    protected InputHandler _input;
    protected Camera cam;

    private void Awake()
    {
        _input = InputHandler.Instance;
    }
    private void Start()
    {
        cam = Camera.main;
    }
    protected abstract void OnEnable();
    protected abstract void OnDisable();
}
