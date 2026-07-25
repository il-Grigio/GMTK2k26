using System;
using UnityEngine;
using Unity.Cinemachine;
public class DuelCameraSwitcher : MonoBehaviour
{
    public CinemachineCamera[] Cameras;
    public CinemachineCamera WinCamera;
    public CinemachineCamera LoseCamera;
    public CinemachineCamera LoseStepForwardCamera;
    
    int currentCamera = 0;

    private void OnEnable()
    {
        ResetCameras();
        DuelManager.Instance.OnWin -= SwitchToWinCamera;
        DuelManager.Instance.OnWin += SwitchToWinCamera;
        DuelManager.Instance.OnLose -= SwitchToLoseCamera;
        DuelManager.Instance.OnLose += SwitchToLoseCamera;
    }

    private void OnDisable()
    {
        DuelManager.Instance.OnWin -= SwitchToWinCamera;
        DuelManager.Instance.OnLose -= SwitchToLoseCamera;
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

    private void SwitchToWinCamera()
    {
        ResetCameras();
        WinCamera.Priority = 20;
    }
    public void SwitchToStepForwardLoseCamera()
    {
        ResetCameras();
        LoseStepForwardCamera.Priority = 20;
    }
    private void SwitchToLoseCamera()
    {
        ResetCameras();
        LoseCamera.Priority = 20;
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
        WinCamera.Priority = 5;
        LoseCamera.Priority = 5;
        LoseStepForwardCamera.Priority = 5;
    }
}
