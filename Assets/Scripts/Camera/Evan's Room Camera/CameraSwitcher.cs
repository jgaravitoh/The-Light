using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;
public class CameraSwitcher
{
    static List<CinemachineCamera> cameras = new List<CinemachineCamera>();
    public static CinemachineCamera activeCamera = null;

    public static bool IsActiveCamera(CinemachineCamera _camera)
    {
        return _camera == activeCamera;
    }

    public static void SwitchCamera(CinemachineCamera _camera)
    {
        _camera.Priority = 10;
        activeCamera = _camera;

        foreach (CinemachineCamera c in cameras)
        {
            if (c != _camera && c.Priority != 0)
            {
                c.Priority = 0;
            }
        }
    }

    public static void Register(CinemachineCamera _camera)
    {
        cameras.Add(_camera);
        Debug.Log("Camera registered", _camera);
    }
    public static void Unregister(CinemachineCamera _camera)
    {
        cameras.Remove(_camera);
        Debug.Log("Camera unregistered", _camera);
    }
}
