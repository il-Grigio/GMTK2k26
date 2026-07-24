using System;
using UnityEngine;
using Unity.Cinemachine;
public class DuelCameraSwitcher : MonoBehaviour
{
    public CinemachineCamera[] Cameras;
    int currentCamera = 0;
    private void Start()
    {
        ResetCameras();
    }

    public void PrepareDuel()
    {
        ResetCameras();
        currentCamera = 0;
        Cameras[currentCamera].Priority = 20;
    }

    public void SwitchCamera()
    {
        currentCamera = (currentCamera + 1) % Cameras.Length;
        ResetCameras();
        Cameras[currentCamera].Priority = 20;
    }
    public void EndDuel()
    {
        ResetCameras();
    }

    private void ResetCameras()
    {
        foreach (var cam in Cameras)
        {
            cam.Priority = 5;
        }
    }
}
